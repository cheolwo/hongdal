using Bogus;
using DriverApp.Models.Driver;
using DriverApp.Models.Driver.Samples;
using DriverApp.Services.Geo;
using System.Globalization;

namespace DriverApp.Services.Samples;

public sealed class 기사샘플데이터Service : IDriverSampleDataService
{
    private readonly IReadOnlyList<DriverRequestItem> _추천의뢰목록;
    private readonly IReadOnlyList<기사예약샘플항목> _예약목록;
    private readonly IReadOnlyList<기사운송샘플항목> _운송목록;
    private readonly IReadOnlyList<기사알림샘플항목> _알림목록;

    public 기사샘플데이터Service()
    {
        Randomizer.Seed = new Random(20260629);
        _추천의뢰목록 = 추천의뢰생성();
        _예약목록 = 예약생성();
        _운송목록 = 운송생성();
        _알림목록 = 알림생성();
        기사현재위치 = new 기사현재위치샘플(
            "서울 강서구 화곡동",
            37.5412m,
            126.8409m,
            DateTime.Now.AddMinutes(-3));
        근무상태 = new 기사근무샘플상태(
            "홍길동 기사님",
            "운행중",
            "일반 운행",
            "서울 강서구 화곡동",
            "서울 양천구 목동",
            DateTime.Today.AddHours(8).AddMinutes(30),
            _추천의뢰목록.Count,
            _예약목록.Count);
        정산요약 = new 기사정산샘플요약(
            DateTime.Today.Year,
            DateTime.Today.Month,
            12,
            5000m,
            5000m,
            true,
            [
                new("이용료", "배차 확정 건수 기준", 5000m),
                new("월 상한 조정", "월 이용료 상한 적용", 0m),
                new("기타 조정", "운영 정책에 따른 정산 반영", 1200m)
            ]);
    }

    public 기사근무샘플상태 근무상태 { get; }

    public 기사현재위치샘플 기사현재위치 { get; }

    public 기사정산샘플요약 정산요약 { get; }

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

    private static IReadOnlyList<DriverRequestItem> 추천의뢰생성()
    {
        var 화물종류 = new[] { "가구", "냉장식품", "생활용품", "전자제품", "소형 이사", "행사용품" };
        var 지역 = new[] { "서울 강서구", "서울 마포구", "경기 부천시", "경기 고양시", "인천 연수구", "경기 수원시", "서울 송파구" };
        var 상태 = new[] { "추천", "적합추천", "긴급추천" };
        var 운송방식 = new[] { "혼적", "독차" };
        var 차량톤수 = new[] { "1톤", "2.5톤", "3.5톤", "5톤" };
        var 차량형태 = new[] { "카고", "윙", "탑", "냉동탑" };
        var 결제방식 = new[] { "하차 후 계좌", "카드 결제", "현금 결제", "인수증 정산" };
        var 상차메모 = new[] { "09시 이후 상차", "오전 상차", "즉시 상차 가능", "점심 이후 상차" };
        var 화물메모 = new[] { "공파렛", "박스 화물", "행거박스", "생활집기", "냉장 보관" };

        return new Faker<DriverRequestItem>("ko")
            .RuleFor(x => x.의뢰Id, f => (f.IndexFaker + 1).ToString(CultureInfo.InvariantCulture))
            .RuleFor(x => x.화물종류, f => f.PickRandom(화물종류))
            .RuleFor(x => x.운송방식, f => f.PickRandom(운송방식))
            .RuleFor(x => x.당일상차필수, f => f.Random.Bool(0.45f))
            .RuleFor(x => x.당일하차필수, f => f.Random.Bool(0.35f))
            .RuleFor(x => x.차량톤수, f => f.PickRandom(차량톤수))
            .RuleFor(x => x.차량형태, f => f.PickRandom(차량형태))
            .RuleFor(x => x.인수증필요, f => f.Random.Bool(0.55f))
            .RuleFor(x => x.결제방식, f => f.PickRandom(결제방식))
            .RuleFor(x => x.픽업지, f => f.PickRandom(지역))
            .RuleFor(x => x.하차지, f => f.PickRandom(지역))
            .RuleFor(x => x.픽업_위도, f => f.Random.Decimal(37.2m, 37.7m))
            .RuleFor(x => x.픽업_경도, f => f.Random.Decimal(126.7m, 127.2m))
            .RuleFor(x => x.하차_위도, f => f.Random.Decimal(37.2m, 37.7m))
            .RuleFor(x => x.하차_경도, f => f.Random.Decimal(126.7m, 127.2m))
            .RuleFor(x => x.직선거리Km, f => f.Random.Decimal(8m, 55m))
            .RuleFor(x => x.픽업거리Km, f => f.Random.Decimal(1m, 9m))
            .RuleFor(x => x.공차거리Km, f => f.Random.Decimal(1m, 9m))
            .RuleFor(x => x.운송거리Km, f => f.Random.Decimal(10m, 70m))
            .RuleFor(x => x.복귀예상거리Km, f => f.Random.Decimal(2m, 18m))
            .RuleFor(x => x.지금바로복귀거리Km, f => f.Random.Decimal(3m, 20m))
            .RuleFor(x => x.복귀우회증가거리Km, (_, x) => (x.복귀예상거리Km ?? 0m) - (x.지금바로복귀거리Km ?? 0m))
            .RuleFor(x => x.총공차거리Km, (_, x) => (x.픽업거리Km ?? 0m) + (x.복귀예상거리Km ?? 0m))
            .RuleFor(x => x.주행거리Km, (_, x) => (x.픽업거리Km ?? 0m) + (x.운송거리Km ?? 0m))
            .RuleFor(x => x.예상톨비, f => f.Random.Decimal(0m, 7000m))
            .RuleFor(x => x.예상연료비, f => f.Random.Decimal(9000m, 32000m))
            .RuleFor(x => x.예상총비용, (_, x) => (x.예상톨비 ?? 0m) + (x.예상연료비 ?? 0m))
            .RuleFor(x => x.예상수익, f => f.Random.Decimal(55000m, 180000m))
            .RuleFor(x => x.추천점수, f => f.Random.Decimal(72m, 98m))
            .RuleFor(x => x.추천사유, f => f.PickRandom("현재 위치와 가깝습니다.", "운송거리 대비 수익성이 좋습니다.", "차량 적합도가 높습니다.", "복귀 동선이 자연스럽습니다."))
            .RuleFor(x => x.복귀지기준추천여부, true)
            .RuleFor(x => x.복귀지출처, f => f.PickRandom("오늘복귀지", "기본복귀지"))
            .RuleFor(x => x.복귀추천사유, (_, x) => (x.복귀우회증가거리Km ?? 0m) <= 0m
                ? "오늘 복귀지 기준으로 보면 이 의뢰를 수행할수록 복귀 동선에 가까워집니다."
                : $"오늘 복귀지 기준으로 바로 복귀하는 경우보다 약 {x.복귀우회증가거리Km:0.0}km 우회합니다.")
            .RuleFor(x => x.요약설명, (_, x) => $"{x.화물종류} 운송, {x.픽업지}에서 {x.하차지}까지")
            .RuleFor(x => x.상세설명, (_, x) => $"{x.픽업지} 상차 후 {x.하차지} 하차 예정입니다. 추천 사유와 비용을 보고 수락/거절을 판단합니다.")
            .RuleFor(x => x.상태, f => f.PickRandom(상태))
            .RuleFor(x => x.배차상태, "배차대기")
            .Generate(6);
    }

    private static IReadOnlyList<기사예약샘플항목> 예약생성()
    {
        var 시작모드 = new[] { "일반 운행", "예약 운행", "복귀 운행" };
        var 위치 = new[] { "서울 강서구", "서울 양천구", "경기 김포시", "인천 계양구" };

        return new Faker<기사예약샘플항목>("ko")
            .CustomInstantiator(f => new 기사예약샘플항목(
                f.IndexFaker + 1,
                DateTime.Today.AddHours(9 + f.IndexFaker * 2),
                f.PickRandom(시작모드),
                f.PickRandom(위치),
                f.PickRandom(위치),
                f.PickRandom("대기", "확정", "곧 시작"),
                f.PickRandom("출발 전 위치를 확인합니다.", "예약 시간 20분 전에 알림을 보냅니다.", "복귀지 기준으로 추천을 조정합니다.")))
            .Generate(3);
    }

    private static IReadOnlyList<기사운송샘플항목> 운송생성()
    {
        var 단계 = new[] { "상차지 이동중", "상차 대기", "하차지 이동중" };

        return new Faker<기사운송샘플항목>("ko")
            .CustomInstantiator(f => new 기사운송샘플항목(
                f.IndexFaker + 1,
                $"DRV-2026-{f.IndexFaker + 1:000}",
                f.PickRandom("가구", "생활용품", "전자제품"),
                f.PickRandom("서울 강서구", "서울 마포구", "인천 연수구"),
                f.PickRandom("경기 수원시", "서울 송파구", "경기 고양시"),
                f.PickRandom(단계),
                DateTime.Now.AddMinutes(20 + f.IndexFaker * 35),
                f.Random.Decimal(12m, 65m),
                f.Random.Decimal(65000m, 160000m),
                f.PickRandom("상차지 도착", "상차 완료", "하차지 도착")))
            .Generate(3);
    }

    private static IReadOnlyList<기사알림샘플항목> 알림생성()
    {
        var 종류 = new[] { "추천", "운송", "정산", "예약" };

        return new Faker<기사알림샘플항목>("ko")
            .CustomInstantiator(f => new 기사알림샘플항목(
                f.IndexFaker + 1,
                f.PickRandom(종류),
                f.PickRandom("새 추천콜이 도착했습니다.", "상차 시간이 가까워졌습니다.", "이번 달 정산이 완료되었습니다.", "예약 운행을 확인해 주세요."),
                f.PickRandom("지금 추천 상세를 확인하고 수락 여부를 결정할 수 있습니다.", "운송 진행 상태를 업데이트해 주세요.", "이용료 상한 정책이 적용되었습니다.", "예약 시간 전에 시작 위치를 확인해 주세요."),
                DateTime.Now.AddMinutes(-f.Random.Int(5, 240)),
                f.Random.Bool()))
            .Generate(8);
    }
}
