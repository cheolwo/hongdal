using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Client.Infrastructure.Transport;
using Hongdal.Contracts.Driver.Action;
using Hongdal.Contracts.Driver.Recommendation;
using Hongdal.Contracts.Driver.Work;
using Microsoft.AspNetCore.SignalR.Client;

namespace Hongdal.WebApp.Services;

public sealed class 기사추천수신Service : I기사추천수신Service
{
    private readonly HttpClient _httpClient;
    private readonly WebAuthSessionService _authSession;
    private readonly ITransportRequestLedgerObserver _ledgerObserver;
    private HubConnection? _connection;

    public 기사추천수신Service(
        HttpClient httpClient,
        WebAuthSessionService authSession,
        ITransportRequestLedgerObserver ledgerObserver)
    {
        _httpClient = httpClient;
        _authSession = authSession;
        _ledgerObserver = ledgerObserver;
    }

    public event Func<IReadOnlyList<기사추천수신항목>, Task>? 추천수신;
    public event Func<string, Task>? 상태변경;

    public string 연결상태 => _connection?.State.ToString() ?? "Disconnected";
    public 기사추천수신항목? 선택추천 { get; private set; }
    public string 선택추천출처 { get; private set; } = string.Empty;
    public DateTimeOffset? 선택추천마감시각 { get; private set; }
    public int? 선택추천응답초 { get; private set; }

    public void 선택추천설정(기사추천수신항목 item, string source, DateTimeOffset? deadlineUtc = null, int? responseSeconds = null)
    {
        선택추천 = item;
        선택추천출처 = source;
        선택추천마감시각 = deadlineUtc;
        선택추천응답초 = responseSeconds;
    }

    public 기사추천수신항목? 선택추천조회(string? requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return 선택추천;
        }

        if (선택추천 is not null &&
            string.Equals(선택추천.의뢰Id, requestId, StringComparison.OrdinalIgnoreCase))
        {
            return 선택추천;
        }

        var sample = 모의추천목록()
            .FirstOrDefault(x => string.Equals(x.의뢰Id, requestId, StringComparison.OrdinalIgnoreCase));

        if (sample is not null)
        {
            선택추천설정(sample, "모의 추천");
        }

        return sample;
    }

    public void 선택추천해제(string? requestId = null)
    {
        if (선택추천 is null)
        {
            선택추천출처 = string.Empty;
            선택추천마감시각 = null;
            선택추천응답초 = null;
            return;
        }

        if (!string.IsNullOrWhiteSpace(requestId) &&
            !string.Equals(선택추천.의뢰Id, requestId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        선택추천 = null;
        선택추천출처 = string.Empty;
        선택추천마감시각 = null;
        선택추천응답초 = null;
    }

    public async Task 연결Async(CancellationToken cancellationToken = default)
    {
        await _authSession.RestoreAsync(cancellationToken);
        if (!_authSession.IsLoggedIn || string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            throw new InvalidOperationException("SignalR 추천 수신은 기사 권한 토큰이 필요합니다. 먼저 웹 로그인에서 기사 계정으로 로그인해 주세요.");
        }

        if (_connection is not null)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await Publish상태Async("SignalR 추천 허브에 이미 연결되어 있습니다.");
                return;
            }

            await _connection.DisposeAsync();
            _connection = null;
        }

        var baseAddress = _httpClient.BaseAddress
                          ?? throw new InvalidOperationException("Hongdal API BaseAddress가 설정되어 있지 않습니다.");
        var hubUri = new Uri(baseAddress, "hubs/dispatch-recommendations");

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUri, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(_authSession.AccessToken);
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<List<기사추천수신항목>>(
            "ReceiveDispatchRecommendations",
            async items => await Publish추천수신Async(items));

        _connection.Reconnecting += async error =>
        {
            await Publish상태Async(error is null
                ? "SignalR 추천 허브 재연결을 시도합니다."
                : $"SignalR 추천 허브 연결이 끊겼습니다. 재연결을 시도합니다. {error.Message}");
        };

        _connection.Reconnected += async _ =>
        {
            await Publish상태Async("SignalR 추천 허브에 다시 연결되었습니다.");
        };

        _connection.Closed += async error =>
        {
            await Publish상태Async(error is null
                ? "SignalR 추천 허브 연결이 종료되었습니다."
                : $"SignalR 추천 허브 연결이 종료되었습니다. {error.Message}");
        };

        await _connection.StartAsync(cancellationToken);
        await Publish상태Async("SignalR 추천 허브에 연결되었습니다. 운행 상태나 위치 갱신 이벤트가 발생하면 추천이 수신됩니다.");
    }

    public async Task 운행중상태전송Async(CancellationToken cancellationToken = default)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("먼저 SignalR 추천 허브에 연결해 주세요.");
        }

        await _connection.InvokeAsync(
            "UpdateDriverStatus",
            new 기사상태갱신요청 { 운행상태 = "운행중" },
            cancellationToken);
        await Publish상태Async("기사 운행중 상태를 허브로 전송했습니다. 서버가 가능한 추천을 다시 산정합니다.");
    }

    public async Task 위치전송Async(
        decimal 위도,
        decimal 경도,
        decimal? 상차접근허용반경Km = null,
        CancellationToken cancellationToken = default)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("먼저 SignalR 추천 허브에 연결해 주세요.");
        }

        await _connection.InvokeAsync(
            "SubmitLocationUpdate",
            new 기사위치갱신요청
            {
                위도 = 위도,
                경도 = 경도,
                정확도_m = 30,
                상차접근허용반경Km = 상차접근허용반경Km,
                운행상태 = "운행중",
                기록시각 = DateTime.UtcNow
            },
            cancellationToken);
        await Publish상태Async("기사 위치를 허브로 전송했습니다. 서버가 위치 기준 추천을 다시 산정합니다.");
    }

    public async Task<IReadOnlyList<기사추천수신항목>> 추천조회Async(
        기사추천조회범위 범위,
        CancellationToken cancellationToken = default)
    {
        var path = 범위 switch
        {
            기사추천조회범위.운행중 => "api/v1/driver/recommendations/driving",
            기사추천조회범위.비운행중 => "api/v1/driver/recommendations/idle",
            기사추천조회범위.전국콜 => "api/v1/driver/recommendations/national",
            _ => "api/v1/driver/recommendations"
        };

        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, path, cancellationToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "기사 추천 조회", cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<기사추천수신항목>>(cancellationToken) ?? [];
    }

    public async Task<기사운송의뢰상세응답> 상세조회Async(
        string requestId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            throw new InvalidOperationException("상세 조회할 운송 의뢰 ID가 필요합니다.");
        }

        using var request = await CreateAuthorizedRequestAsync(
            HttpMethod.Get,
            $"api/v1/driver/requests/{Uri.EscapeDataString(requestId.Trim())}",
            cancellationToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "운송 의뢰 상세 조회", cancellationToken);

        return await response.Content.ReadFromJsonAsync<기사운송의뢰상세응답>(cancellationToken)
               ?? throw new InvalidOperationException("운송 의뢰 상세 조회 응답을 읽을 수 없습니다.");
    }

    public async Task<기사추천처리결과> 수락Async(
        string requestId,
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(
            HttpMethod.Post,
            $"api/v1/driver/dispatch-actions/{Uri.EscapeDataString(requestId.Trim())}/accept",
            cancellationToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "배차 수락", cancellationToken);
        _ledgerObserver.Observe(
            new TransportRequestLedgerSnapshot(
                requestId.Trim(),
                "수락완료",
                null,
                "배차확정",
                null,
                DateTimeOffset.UtcNow,
                "Hongdal.WebApp.DriverAccepted"),
            "Hongdal.WebApp.DriverAccepted");
        _ledgerObserver.RequestRefresh(requestId, "Hongdal.WebApp.DriverAccepted");
        return new 기사추천처리결과(requestId, "Accepted", await ReadMessageAsync(response, "수락되었습니다.", cancellationToken));
    }

    public async Task<기사추천처리결과> 거절Async(
        string requestId,
        string? 사유,
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(
            HttpMethod.Post,
            $"api/v1/driver/dispatch-actions/{Uri.EscapeDataString(requestId.Trim())}/reject",
            cancellationToken);
        request.Content = JsonContent.Create(new 기사배차거절요청 { 사유 = 사유 });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "배차 거절", cancellationToken);
        _ledgerObserver.RequestRefresh(requestId, "Hongdal.WebApp.DriverRejected");
        return new 기사추천처리결과(requestId, "Rejected", await ReadMessageAsync(response, "거절되었습니다.", cancellationToken));
    }

    public IReadOnlyList<기사추천수신항목> 모의추천목록()
    {
        var now = DateTime.UtcNow;
        return
        [
            new 기사추천수신항목
            {
                의뢰Id = "HD-WEB-001",
                화물종류 = "냉장식품",
                운송의뢰유형표시 = "일반 화물",
                픽업지 = "서울 양천구 목동",
                하차지 = "경기 수원시 영통구",
                픽업_위도 = 37.526m,
                픽업_경도 = 126.875m,
                하차_위도 = 37.259m,
                하차_경도 = 127.047m,
                픽업거리Km = 7.8m,
                운송거리Km = 42.5m,
                추가예상시간분 = 18,
                기존배송지연분 = 0,
                예상수익 = 88000,
                예상총비용 = 32400,
                예상추가순이익 = 55600,
                추천점수 = 91,
                추천사유 = "상차지 접근 거리와 현재 경로 이점이 좋아 웹 검증용 추천으로 표시합니다.",
                배지 = ["경로 이점", "냉장 주의"],
                경고 = ["상차 전 온도 조건을 확인하세요."],
                차량적합여부 = true,
                추천시작시각 = now,
                추천만료시각 = now.AddSeconds(60),
                상태 = "배차추천",
                배차상태 = "추천중"
            }
        ];
    }

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(
        HttpMethod method,
        string path,
        CancellationToken cancellationToken)
    {
        await _authSession.RestoreAsync(cancellationToken);
        if (!_authSession.IsLoggedIn || string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            throw new InvalidOperationException("이 작업은 서버 인증이 필요합니다. 먼저 웹 로그인에서 기사 계정으로 로그인해 주세요.");
        }

        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authSession.AccessToken);
        return request;
    }

    private async Task Publish추천수신Async(IReadOnlyList<기사추천수신항목> items)
    {
        foreach (var item in items)
        {
            Observe(item);
        }

        var handler = 추천수신;
        if (handler is null)
        {
            return;
        }

        foreach (Func<IReadOnlyList<기사추천수신항목>, Task> callback in handler.GetInvocationList())
        {
            await callback(items);
        }
    }

    private void Observe(기사추천수신항목 item)
    {
        if (string.IsNullOrWhiteSpace(item.의뢰Id))
        {
            return;
        }

        _ledgerObserver.Observe(
            new TransportRequestLedgerSnapshot(
                item.의뢰Id,
                item.상태,
                null,
                item.배차상태,
                null,
                DateTimeOffset.UtcNow,
                "Hongdal.WebApp.DriverRecommendation"),
            "Hongdal.WebApp.DriverRecommendation");
    }

    private async Task Publish상태Async(string message)
    {
        var handler = 상태변경;
        if (handler is null)
        {
            return;
        }

        foreach (Func<string, Task> callback in handler.GetInvocationList())
        {
            await callback(message);
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string actionName, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
            ? $"{actionName} 실패: HTTP {(int)response.StatusCode}"
            : $"{actionName} 실패: HTTP {(int)response.StatusCode}: {body}");
    }

    private static async Task<string> ReadMessageAsync(
        HttpResponseMessage response,
        string fallback,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(body) ? fallback : body;
    }

    public async ValueTask DisposeAsync()
        => await 연결해제Async();

    public async ValueTask 연결해제Async()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
