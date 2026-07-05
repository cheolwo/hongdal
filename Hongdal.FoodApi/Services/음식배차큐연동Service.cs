using System.Net.Http.Json;
using Hongdal.Contracts.Admin.Inbound;
using Hongdal.Contracts.Food;
using Hongdal.FoodApi.Options;
using Microsoft.Extensions.Options;

namespace Hongdal.FoodApi.Services;

public sealed class 음식배차큐연동Service : I음식배차큐연동Service
{
    private readonly HttpClient _httpClient;
    private readonly 배차연동Options _options;
    private readonly ILogger<음식배차큐연동Service> _logger;

    public 음식배차큐연동Service(
        HttpClient httpClient,
        IOptions<배차연동Options> options,
        ILogger<음식배차큐연동Service> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task 배차대기생성요청Async(음식주문응답 order, decimal? 픽업위도, decimal? 픽업경도, string 픽업주소, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var path = string.IsNullOrWhiteSpace(_options.DispatchWaitPath) ? "/api/v1/dispatch/wait" : _options.DispatchWaitPath;
        var payload = new 배차대기요청
        {
            의뢰Id = order.주문번호,
            화주Id = order.주문자UserId,
            배차업무유형 = 10,
            원본의뢰유형 = "FoodDelivery",
            원본의뢰Id = order.주문번호,
            픽업_도로명주소 = 픽업주소,
            픽업_상세주소 = string.Empty,
            픽업_위도 = 픽업위도,
            픽업_경도 = 픽업경도,
            하차_도로명주소 = order.수령인정보.주소,
            하차_상세주소 = order.수령인정보.상세주소 ?? string.Empty,
            상태 = "대기"
        };

        using var response = await _httpClient.PostAsJsonAsync(path, payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("음식 배차 큐 연동 실패. 주문번호={OrderNo}, 상태코드={StatusCode}, 응답={Response}", order.주문번호, response.StatusCode, body);
            return;
        }

        _logger.LogInformation("음식 배차 큐 연동 성공. 주문번호={OrderNo}", order.주문번호);
    }
}
