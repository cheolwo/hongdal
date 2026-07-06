using System.Globalization;
using System.Net.Http.Json;
using DriverApp.Models.Driver;
using DriverApp.Models.Driver.Samples;
using DriverApp.Services.Geo;
using Hongdal.Contracts.Driver.Development;

namespace DriverApp.Services.Samples;

public sealed class ServerBackedDriverSampleDataService : IDriverSampleDataService
{
    private readonly HttpClient _httpClient;
    private readonly 기사샘플데이터Service _fallback;
    private bool _loaded;

    private 기사근무샘플상태 _근무상태;
    private 기사현재위치샘플 _기사현재위치;
    private 기사정산샘플요약 _정산요약;
    private IReadOnlyList<DriverRequestItem> _추천의뢰목록;
    private IReadOnlyList<기사예약샘플항목> _예약목록;
    private IReadOnlyList<기사운송샘플항목> _운송목록;
    private IReadOnlyList<기사알림샘플항목> _알림목록;

    public ServerBackedDriverSampleDataService(HttpClient httpClient, 기사샘플데이터Service fallback)
    {
        _httpClient = httpClient;
        _fallback = fallback;
        _근무상태 = fallback.근무상태;
        _기사현재위치 = fallback.기사현재위치;
        _정산요약 = fallback.정산요약;
        _추천의뢰목록 = fallback.추천의뢰목록;
        _예약목록 = fallback.예약목록;
        _운송목록 = fallback.운송목록;
        _알림목록 = fallback.알림목록;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (_loaded)
        {
            return;
        }

        try
        {
            var snapshot = await _httpClient.GetFromJsonAsync<기사개발스냅샷응답>(
                "api/v1/driver/dev-snapshot",
                cancellationToken);

            if (snapshot is null)
            {
                return;
            }

            Apply(snapshot);
            _loaded = true;
        }
        catch
        {
            // 개발 서버가 아직 켜지지 않은 경우 기존 샘플을 유지하고 다음 화면 진입 때 다시 시도한다.
        }
    }

    public 기사근무샘플상태 근무상태 => _근무상태;

    public 기사현재위치샘플 기사현재위치 => _기사현재위치;

    public 기사정산샘플요약 정산요약 => _정산요약;

    public IReadOnlyList<DriverRequestItem> 추천의뢰목록 => _추천의뢰목록;

    public IReadOnlyList<기사예약샘플항목> 예약목록 => _예약목록;

    public IReadOnlyList<기사운송샘플항목> 운송목록 => _운송목록;

    public IReadOnlyList<기사알림샘플항목> 알림목록 => _알림목록;

    public DriverRequestItem? 추천의뢰조회(long 의뢰Id)
    {
        return _추천의뢰목록.FirstOrDefault(x => x.의뢰Id == 의뢰Id.ToString(CultureInfo.InvariantCulture));
    }

    public IReadOnlyList<추천의뢰표시항목> 거리포함추천의뢰목록조회()
    {
        return _추천의뢰목록
            .Select(x => new
            {
                의뢰 = x,
                거리 = 거리계산Service.직선거리Km(
                    기사현재위치.위도,
                    기사현재위치.경도,
                    x.픽업_위도 ?? 기사현재위치.위도,
                    x.픽업_경도 ?? 기사현재위치.경도)
            })
            .OrderBy(x => x.거리)
            .Select((x, index) => new 추천의뢰표시항목(x.의뢰, x.거리, index + 1))
            .ToList();
    }

    public 기사운송샘플항목? 운송조회(long 운송Id)
    {
        return _운송목록.FirstOrDefault(x => x.Id == 운송Id);
    }

    public 기사운송샘플항목? 현재운송조회()
    {
        return _운송목록.FirstOrDefault();
    }

    private void Apply(기사개발스냅샷응답 snapshot)
    {
        _기사현재위치 = new 기사현재위치샘플(
            snapshot.현재위치.위치명,
            snapshot.현재위치.위도,
            snapshot.현재위치.경도,
            snapshot.현재위치.갱신시각);

        _추천의뢰목록 = snapshot.추천의뢰목록.Select(ToRequestItem).ToArray();
        _예약목록 = snapshot.예약목록
            .Select(x => new 기사예약샘플항목(x.Id, x.시작시각, x.시작모드, x.시작위치, x.복귀지, x.상태, x.메모))
            .ToArray();
        _운송목록 = snapshot.운송목록
            .Select(x => new 기사운송샘플항목(
                x.Id,
                x.의뢰Id,
                x.화물종류,
                x.픽업지,
                x.하차지,
                x.픽업위도,
                x.픽업경도,
                x.하차위도,
                x.하차경도,
                x.현재단계,
                x.예정시각,
                x.운송거리Km,
                x.예상수익,
                x.다음행동))
            .ToArray();
        _알림목록 = snapshot.알림목록
            .Select(x => new 기사알림샘플항목(x.Id, x.종류, x.제목, x.내용, x.발생시각, x.읽음))
            .ToArray();

        _근무상태 = new 기사근무샘플상태(
            snapshot.근무상태.기사명,
            snapshot.근무상태.운행상태,
            snapshot.근무상태.시작모드,
            snapshot.근무상태.시작위치,
            snapshot.근무상태.복귀지,
            snapshot.근무상태.시작시각,
            snapshot.근무상태.추천콜수,
            snapshot.근무상태.오늘예약수);

        _정산요약 = new 기사정산샘플요약(
            snapshot.정산요약.년도,
            snapshot.정산요약.월,
            snapshot.정산요약.배차건수,
            snapshot.정산요약.이용료,
            snapshot.정산요약.월상한,
            snapshot.정산요약.결제완료,
            snapshot.정산요약.상세항목
                .Select(x => new 기사정산샘플상세항목(x.항목명, x.설명, x.금액))
                .ToArray());
    }

    private static DriverRequestItem ToRequestItem(기사개발추천의뢰응답 source)
    {
        return new DriverRequestItem
        {
            의뢰Id = source.의뢰Id,
            화물종류 = source.화물종류,
            운송방식 = source.운송방식,
            당일상차필수 = source.당일상차필수,
            당일하차필수 = source.당일하차필수,
            차량톤수 = source.차량톤수,
            차량형태 = source.차량형태,
            인수증필요 = source.인수증필요,
            결제방식 = source.결제방식,
            픽업지 = source.픽업지,
            하차지 = source.하차지,
            픽업_위도 = source.픽업_위도,
            픽업_경도 = source.픽업_경도,
            하차_위도 = source.하차_위도,
            하차_경도 = source.하차_경도,
            직선거리Km = source.직선거리Km,
            픽업거리Km = source.픽업거리Km,
            공차거리Km = source.공차거리Km,
            운송거리Km = source.운송거리Km,
            복귀예상거리Km = source.복귀예상거리Km,
            지금바로복귀거리Km = source.지금바로복귀거리Km,
            복귀우회증가거리Km = source.복귀우회증가거리Km,
            총공차거리Km = source.총공차거리Km,
            주행거리Km = source.주행거리Km,
            예상톨비 = source.예상톨비,
            예상연료비 = source.예상연료비,
            예상총비용 = source.예상총비용,
            예상수익 = source.예상수익,
            추천점수 = source.추천점수,
            추천사유 = source.추천사유,
            복귀지기준추천여부 = source.복귀지기준추천여부,
            복귀지출처 = source.복귀지출처,
            복귀추천사유 = source.복귀추천사유,
            요약설명 = source.요약설명,
            상세설명 = source.상세설명,
            상태 = source.상태,
            배차상태 = source.배차상태
        };
    }
}
