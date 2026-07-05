using DriverApp.Services.Samples;
using Hongdal.Contracts.Driver.Home;

namespace DriverApp.Services;

public sealed class ApiClient : IApiClient
{
    private readonly IDriverSampleDataService _sampleDataService;

    public ApiClient(IDriverSampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string path, TRequest payload)
    {
        await Task.CompletedTask;
        throw new InvalidOperationException($"오프라인 샘플 모드에서는 POST 경로를 지원하지 않습니다: {path}");
    }

    public async Task PostJsonAsync<TRequest>(string path, TRequest payload)
    {
        await Task.CompletedTask;
        throw new InvalidOperationException($"오프라인 샘플 모드에서는 POST 경로를 지원하지 않습니다: {path}");
    }

    public async Task<TResponse?> GetJsonAsync<TResponse>(string path)
    {
        await Task.CompletedTask;

        if (typeof(TResponse) == typeof(기사홈요약응답) && string.Equals(path, "api/v1/driver/home", StringComparison.OrdinalIgnoreCase))
        {
            var currentTransport = _sampleDataService.현재운송조회();
            var reservations = _sampleDataService.예약목록.OrderBy(x => x.시작시각).ToList();
            var recommendations = _sampleDataService.거리포함추천의뢰목록조회();
            var settlement = _sampleDataService.정산요약;

            var response = new 기사홈요약응답
            {
                DriverId = "driver-sample-001",
                기사명 = _sampleDataService.근무상태.기사명,
                운행상태 = _sampleDataService.근무상태.운행상태,
                홈상태문구 = currentTransport is not null ? "진행 중인 운송을 이어서 처리하세요." : "오늘 작업을 확인하고 운행을 시작하세요.",
                주요행동코드 = currentTransport is not null ? "VIEW_CURRENT_TRANSPORT" : "VIEW_RECOMMENDATIONS",
                주요행동문구 = currentTransport is not null ? "진행중 운송 보기" : "추천 의뢰 보기",
                운행중 = string.Equals(_sampleDataService.근무상태.운행상태, "운행중", StringComparison.OrdinalIgnoreCase),
                현재근무Id = 1,
                운행시작시각 = _sampleDataService.근무상태.시작시각,
                진행중운송있음 = currentTransport is not null,
                현재운송Id = currentTransport?.Id,
                현재운송단계 = currentTransport?.현재단계,
                추천콜수 = recommendations.Count,
                적합추천콜수 = recommendations.Count(x => string.Equals(x.의뢰.상태, "적합추천", StringComparison.OrdinalIgnoreCase)),
                오늘예약수 = reservations.Count,
                다음예약시각 = reservations.FirstOrDefault()?.시작시각,
                진행중운송수 = currentTransport is null ? 0 : 1,
                이번달배차건수 = settlement.배차건수,
                이번달이용료 = settlement.이용료,
                이번달이용료상한 = settlement.월상한,
                남은이용료 = settlement.월상한 - settlement.이용료,
                정산결제완료 = settlement.결제완료,
                푸시토큰등록됨 = true,
                알림정상 = true,
                전국콜사용가능 = true,
                오늘할일 = BuildTodoItems(currentTransport, reservations, recommendations)
            };

            return (TResponse?)(object)response;
        }

        throw new InvalidOperationException($"오프라인 샘플 모드에서 지원하지 않는 GET 경로입니다: {path}");
    }

    private static IReadOnlyList<기사홈할일항목> BuildTodoItems(
        DriverApp.Models.Driver.Samples.기사운송샘플항목? currentTransport,
        IReadOnlyList<DriverApp.Models.Driver.Samples.기사예약샘플항목> reservations,
        IReadOnlyList<DriverApp.Models.Driver.Samples.추천의뢰표시항목> recommendations)
    {
        var items = new List<기사홈할일항목>();

        if (currentTransport is not null)
        {
            items.Add(new 기사홈할일항목
            {
                종류 = "진행중 운송",
                제목 = $"{currentTransport.현재단계} 처리",
                설명 = $"{currentTransport.픽업지} → {currentTransport.하차지} 운송을 계속 진행합니다.",
                이동경로 = "/driver/transports/current",
                우선순위 = 1
            });
        }

        var nextReservation = reservations.FirstOrDefault();
        if (nextReservation is not null)
        {
            items.Add(new 기사홈할일항목
            {
                종류 = "예약",
                제목 = $"다음 예약 {nextReservation.시작시각:HH:mm}",
                설명 = $"{nextReservation.시작위치}에서 {nextReservation.복귀지 ?? "도착지 미정"}로 이동 준비를 확인합니다.",
                이동경로 = "/driver/reservations",
                우선순위 = 2
            });
        }

        var bestRecommendation = recommendations.FirstOrDefault();
        if (bestRecommendation is not null)
        {
            items.Add(new 기사홈할일항목
            {
                종류 = "추천 의뢰",
                제목 = bestRecommendation.의뢰.화물종류,
                설명 = bestRecommendation.의뢰.요약설명,
                이동경로 = DriverRoutes.Recommendations,
                우선순위 = 3
            });
        }

        return items.OrderBy(x => x.우선순위).ToArray();
    }
}
