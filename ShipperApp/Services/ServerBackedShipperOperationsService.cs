using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Contracts.Shipper.Request;
using ShipperApp.Models.Shipper;
using ShipperApp.Services.Samples;

namespace ShipperApp.Services;

public sealed class ServerBackedShipperOperationsService : IShipperOperationsService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSession _authSession;
    private readonly SampleShipperOperationsService _fallback;

    public ServerBackedShipperOperationsService(
        HttpClient httpClient,
        IAuthSession authSession,
        SampleShipperOperationsService fallback)
    {
        _httpClient = httpClient;
        _authSession = authSession;
        _fallback = fallback;
    }

    public async Task<IReadOnlyList<ShipperRequestItem>> GetRequestsAsync(CancellationToken cancellationToken = default)
    {
        var userId = ResolveUserId();
        if (string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            return await _fallback.GetRequestsAsync(cancellationToken);
        }

        try
        {
            var path = $"api/v1/shipper/requests?shipperId={Uri.EscapeDataString(userId)}";
            var response = await GetAuthorizedJsonAsync<IReadOnlyList<화주운송의뢰응답>>(path, cancellationToken);
            return response?.Select(ToRequestItem).ToArray()
                   ?? await _fallback.GetRequestsAsync(cancellationToken);
        }
        catch
        {
            return await _fallback.GetRequestsAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<공개화물요약응답>> GetPublicCargoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IReadOnlyList<공개화물요약응답>>(
                       "api/v1/shipper/requests/public",
                       cancellationToken)
                   ?? await _fallback.GetPublicCargoAsync(cancellationToken);
        }
        catch
        {
            return await _fallback.GetPublicCargoAsync(cancellationToken);
        }
    }

    public Task<IReadOnlyList<창고요약응답>> GetWarehousesAsync(CancellationToken cancellationToken = default)
    {
        return _fallback.GetWarehousesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<입고요청항목응답>> GetInboundsAsync(CancellationToken cancellationToken = default)
    {
        return _fallback.GetInboundsAsync(cancellationToken);
    }

    public Task<IReadOnlyList<재고항목응답>> GetInventoryAsync(CancellationToken cancellationToken = default)
    {
        return _fallback.GetInventoryAsync(cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetVehicleTypesAsync(CancellationToken cancellationToken = default)
    {
        return _fallback.GetVehicleTypesAsync(cancellationToken);
    }

    public Task<decimal> EstimateFareAsync(string vehicleType, decimal distanceKm, CancellationToken cancellationToken = default)
    {
        return _fallback.EstimateFareAsync(vehicleType, distanceKm, cancellationToken);
    }

    public async Task AddRequestAsync(ShipperRequestItem request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            await _fallback.AddRequestAsync(request, cancellationToken);
            return;
        }

        try
        {
            using var httpRequest = CreateAuthorizedRequest(HttpMethod.Post, "api/v1/shipper/requests");
            httpRequest.Content = JsonContent.Create(ToCreateRequest(request));

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await _fallback.AddRequestAsync(request, cancellationToken);
            }
        }
        catch
        {
            await _fallback.AddRequestAsync(request, cancellationToken);
        }
    }

    private async Task<T?> GetAuthorizedJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, path);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authSession.AccessToken);
        return request;
    }

    private string ResolveUserId()
    {
        return string.IsNullOrWhiteSpace(_authSession.UserId) ? "shipper-demo" : _authSession.UserId!;
    }

    private 화주운송의뢰생성요청 ToCreateRequest(ShipperRequestItem source)
    {
        var amount = source.결제예정금액 ?? decimal.ToInt32(source.기준운임 ?? source.기사지급예정운임 ?? 0m);
        var pickupAddress = string.IsNullOrWhiteSpace(source.픽업지) ? "상차지 미정" : source.픽업지!;
        var dropoffAddress = string.IsNullOrWhiteSpace(source.하차지) ? "하차지 미정" : source.하차지!;
        var now = DateTime.UtcNow;

        return new 화주운송의뢰생성요청
        {
            화주Id = ResolveUserId(),
            운송방식 = string.IsNullOrWhiteSpace(source.운송방식) ? "일반" : source.운송방식,
            차량종류 = string.IsNullOrWhiteSpace(source.차량종류) ? "1톤 카고" : source.차량종류,
            결제수단 = string.IsNullOrWhiteSpace(source.결제수단) ? 결제수단.카드.ToString() : source.결제수단,
            결제예정금액 = amount,
            정산조건 = new 화주운송정산조건DTO
            {
                정산시점 = 정산시점.선결제,
                결제수단 = 결제수단.카드,
                증빙방식 = 증빙방식.인수증,
                수납주체 = 수납주체.플랫폼,
                정산메모 = "ShipperApp 서버 연동 생성"
            },
            화물 = new CargoDTO
            {
                화물종류 = string.IsNullOrWhiteSpace(source.화물종류) ? "일반화물" : source.화물종류,
                수량 = 1
            },
            픽업 = CreateLocation(pickupAddress, now.AddHours(1), now.AddHours(3)),
            하차 = CreateLocation(dropoffAddress, now.AddHours(4), now.AddHours(8)),
            요금옵션 = new PricingDTO
            {
                서비스레벨 = "standard",
                요청사항 = "ShipperApp 화면 검증용 서버 생성",
                예상거리Km = source.예상거리Km,
                기본운임 = source.기준운임,
                기사지급예정운임 = source.기사지급예정운임,
                알선정책 = new 화주운송알선정책DTO
                {
                    알선단계 = source.알선단계,
                    재알선금지 = source.재알선금지
                }
            },
            클라이언트요청Id = string.IsNullOrWhiteSpace(source.의뢰Id) ? $"shipper-app-{Guid.NewGuid():N}" : source.의뢰Id,
            결제상태 = string.IsNullOrWhiteSpace(source.결제상태) ? "결제대기" : source.결제상태
        };
    }

    private static LocationContactDTO CreateLocation(string address, DateTime start, DateTime end)
    {
        return new LocationContactDTO
        {
            주소 = new AddressDTO
            {
                도로명주소 = address
            },
            연락처 = new ContactDTO
            {
                이름 = "홍달 앱 담당자",
                전화번호 = "010-0000-0000"
            },
            시간창 = new TimeWindowDTO
            {
                시작일시 = start,
                종료일시 = end
            }
        };
    }

    private static ShipperRequestItem ToRequestItem(화주운송의뢰응답 source)
    {
        return new ShipperRequestItem
        {
            의뢰Id = source.의뢰Id,
            화물종류 = source.요약?.화물종류 ?? string.Empty,
            의뢰상태 = source.의뢰상태,
            결제상태 = source.결제상태,
            배차상태 = source.배차상태,
            운송방식 = source.운송방식,
            차량종류 = source.차량종류,
            결제수단 = source.결제수단,
            결제예정금액 = source.결제예정금액,
            기준운임 = source.최종운임,
            기사지급예정운임 = source.최종운임,
            생성일시 = source.생성일시,
            픽업지 = source.픽업지,
            하차지 = source.하차지
        };
    }
}
