using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 운송의뢰초안페이지ViewModelTests
{
    [Fact]
    public async Task 명시한출고예정Id만조회하고_초안에연결한다()
    {
        var service = new FakeService { Detail = ReadyPlan(17) };
        var page = Create(service);

        Assert.True(await page.초기화Async(17));
        Assert.Equal([17L], service.DetailIds);
        Assert.Equal(17, page.초안.원장!.OutboundPlanId);
        Assert.True(page.초안.작성가능);
    }

    [Fact]
    public async Task 검토조건을통과하지못한원장은_입력검토를차단한다()
    {
        var service = new FakeService { Detail = ReadyPlan(3, canStart: false) };
        var page = Create(service);
        await page.초기화Async(3);

        Assert.False(page.초안.입력값검토());
        Assert.Contains("검토 조건", page.초안.검증오류);
        Assert.Null(page.초안.검토결과);
    }

    [Fact]
    public async Task 유효한입력은_명시적저장전까지로컬검토결과만만든다()
    {
        var service = new FakeService { Detail = ReadyPlan(9) };
        var api = new RecordingJsonApiClient();
        var page = Create(service, api);
        await page.초기화Async(9);
        page.초안.하차지주소 = "서울특별시 송파구 올림픽로 300";
        page.초안.하차지상세주소 = "동문 상차장";
        page.초안.희망상차일 = new DateTime(2026, 7, 21);
        page.초안.희망상차시각 = new TimeSpan(9, 0, 0);
        page.초안.희망도착일 = new DateTime(2026, 7, 21);
        page.초안.희망도착시각 = new TimeSpan(11, 30, 0);
        page.초안.차량유형 = "1톤 냉장탑차";
        page.초안.상품수량확인 = true;

        Assert.True(page.초안.입력값검토());
        Assert.Equal("OUT-000009-REVIEW", page.초안.검토결과!.ReviewReference);
        Assert.Equal("서울특별시 송파구 올림픽로 300", page.초안.검토결과.DestinationAddress);
        Assert.Empty(api.Requests);
        Assert.Equal([9L], service.DetailIds);
    }

    [Fact]
    public async Task 검토한초안을저장하면_기존운송인계Api를호출하고_같은출고예정을재조회한다()
    {
        var service = new FakeService
        {
            DetailFactory = call => call == 1
                ? ReadyPlan(9)
                : ReadyPlan(9, canStart: false, transportRequestId: "warehouse-outbound-9")
        };
        var api = new RecordingJsonApiClient
        {
            Response = new()
            {
                의뢰Id = "warehouse-outbound-9",
                의뢰상태 = "생성됨",
                배차상태 = "미시작",
                운송상태 = "배차대기"
            }
        };
        var page = Create(service, api);
        await page.초기화Async(9);
        page.초안.하차지주소 = "서울특별시 송파구 올림픽로 300";
        page.초안.하차지상세주소 = "동문 상차장";
        page.초안.희망상차일 = new DateTime(2026, 7, 21);
        page.초안.희망상차시각 = new TimeSpan(9, 0, 0);
        page.초안.희망도착일 = new DateTime(2026, 7, 21);
        page.초안.희망도착시각 = new TimeSpan(11, 30, 0);
        page.초안.차량유형 = "1톤 냉장탑차";
        page.초안.취급메모 = "냉장 유지";
        page.초안.상품수량확인 = true;
        Assert.True(page.초안.입력값검토());

        Assert.True(await page.서버저장Async());

        var request = Assert.Single(api.Requests);
        Assert.Equal("api/v1/warehouse-operations/inventory/reconsignment", api.LastPath);
        Assert.Equal(9, request.출고예정Id);
        Assert.Equal(71, request.입고상품Id);
        Assert.Equal(9, request.요청수량);
        Assert.Equal("서울특별시 송파구 올림픽로 300", request.하차지주소);
        Assert.Equal("냉장 유지", request.취급메모);
        Assert.Equal([9L, 9L], service.DetailIds);
        Assert.Equal("warehouse-outbound-9", page.원장.원장!.TransportRequestId);
    }

    [Fact]
    public void 도착일시가상차일시보다이르면_검토를거부한다()
    {
        var draft = new 운송의뢰초안작성ViewModel();
        draft.원장설정(ReadyPlan(1));
        draft.하차지주소 = "서울특별시 송파구 올림픽로 300";
        draft.희망상차일 = new DateTime(2026, 7, 21);
        draft.희망상차시각 = new TimeSpan(12, 0, 0);
        draft.희망도착일 = new DateTime(2026, 7, 21);
        draft.희망도착시각 = new TimeSpan(11, 0, 0);
        draft.차량유형 = "1톤 냉장탑차";
        draft.상품수량확인 = true;

        Assert.False(draft.입력값검토());
        Assert.Contains("뒤여야", draft.검증오류);
    }

    [Fact]
    public async Task 기사와차량확인후인계완료는_같은출고예정과의뢰Id를재조회한다()
    {
        var completedAt = new DateTime(2026, 7, 21, 2, 30, 0, DateTimeKind.Utc);
        var service = new FakeService
        {
            DetailFactory = call => call == 1
                ? ReadyPlan(
                    9,
                    canStart: false,
                    transportRequestId: "warehouse-outbound-9",
                    canCompleteHandoff: true)
                : ReadyPlan(
                    9,
                    canStart: false,
                    transportRequestId: "warehouse-outbound-9",
                    handoffCompletedAtUtc: completedAt)
        };
        var page = Create(service);
        await page.초기화Async(9);
        page.원장.기사신원확인 = true;
        page.원장.등록차량확인 = true;
        page.원장.상품인계확인 = true;
        page.원장.인계메모 = "봉인 상태 확인";

        Assert.True(await page.서버인계완료Async());

        var command = Assert.Single(service.HandoffRequests);
        Assert.Equal(9, command.OutboundPlanId);
        Assert.True(command.Request.DriverIdentityConfirmed);
        Assert.True(command.Request.VehicleConfirmed);
        Assert.True(command.Request.CargoReleasedConfirmed);
        Assert.Equal("봉인 상태 확인", command.Request.Memo);
        Assert.Equal([9L, 9L], service.DetailIds);
        Assert.Equal(completedAt, page.원장.원장!.HandoffCompletedAtUtc);
        Assert.Equal("warehouse-outbound-9", page.원장.원장.TransportRequestId);
    }

    private static 운송의뢰초안PageViewModel Create(
        FakeService service,
        RecordingJsonApiClient? api = null)
        => new(
            new(service),
            new(),
            new(new 입출고작업Service(api ?? new RecordingJsonApiClient())));

    private static 출고예정검토상세응답 ReadyPlan(
        long id,
        bool canStart = true,
        string? transportRequestId = null,
        bool canCompleteHandoff = false,
        DateTime? handoffCompletedAtUtc = null)
        => new()
        {
            OutboundPlanId = id,
            InboundItemId = 71,
            ProductName = "냉장 감자",
            Quantity = 9,
            CanStartTransportRequestDraft = canStart,
            ReviewStatus = canStart ? "초안 입력 가능" : "원장 보완 필요",
            TransportRequestId = transportRequestId,
            CanCompleteHandoff = canCompleteHandoff,
            HandoffCompletedAtUtc = handoffCompletedAtUtc,
            AssignedDriverId = "driver-7",
            AssignedDriverVehicle = "1톤 냉장탑차"
        };

    private sealed class FakeService : I출고예정검토페이지Service
    {
        public 출고예정검토상세응답? Detail { get; set; }
        public Func<int, 출고예정검토상세응답?>? DetailFactory { get; set; }
        public List<long> DetailIds { get; } = [];
        public List<(long OutboundPlanId, 출고운송인계완료요청 Request)> HandoffRequests { get; } = [];
        public Task<출고예정검토목록페이지응답> 목록조회Async(출고예정검토목록조회요청 request, CancellationToken cancellationToken = default)
            => Task.FromResult(new 출고예정검토목록페이지응답());
        public Task<출고예정검토상세응답?> 상세조회Async(long outboundPlanId, CancellationToken cancellationToken = default)
        {
            DetailIds.Add(outboundPlanId);
            return Task.FromResult(DetailFactory?.Invoke(DetailIds.Count) ?? Detail);
        }
        public Task<출고운송인계완료응답> 인계완료Async(long outboundPlanId,출고운송인계완료요청 request,CancellationToken cancellationToken=default)
        {
            HandoffRequests.Add((outboundPlanId,request));
            return Task.FromResult(new 출고운송인계완료응답
            {
                OutboundPlanId=outboundPlanId,
                TransportRequestId=$"warehouse-outbound-{outboundPlanId}",
                OutboundStatus="출고완료",
                AssignedDriverId="driver-7",
                AssignedDriverVehicle="1톤 냉장탑차",
                HandoffCompletedAtUtc=DateTime.UtcNow
            });
        }
    }

    private sealed class RecordingJsonApiClient : ISsalddelJsonApiClient
    {
        public Ssalddel.Contracts.Shipper.Request.화주운송의뢰응답 Response { get; set; } = new();
        public List<재고운송의뢰생성요청> Requests { get; } = [];
        public string LastPath { get; private set; } = string.Empty;

        public Task<TResponse?> GetAsync<TResponse>(
            string path,
            string operationName,
            bool allowNotFound = true,
            CancellationToken cancellationToken = default)
            => Task.FromResult<TResponse?>(default);

        public Task<TResponse?> SendAsync<TResponse>(
            HttpMethod method,
            string path,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
            => Task.FromResult<TResponse?>(default);

        public Task<TResponse?> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
        {
            LastPath = path;
            if (request is 재고운송의뢰생성요청 transportRequest
                && Response is TResponse typed)
            {
                Requests.Add(transportRequest);
                return Task.FromResult<TResponse?>(typed);
            }
            return Task.FromResult<TResponse?>(default);
        }

        public Task SendAsync(
            HttpMethod method,
            string path,
            string operationName,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SendAsync<TRequest>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
