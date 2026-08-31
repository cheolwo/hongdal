using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Content;

public sealed class 게임현실상품자료검토ServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 30, 12, 0, 0, TimeSpan.Zero);
    private static ClaimsPrincipal Operator(string id = "fixture-operator")
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id)], "Fixture"));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task 기존서버관리자역할정책을실제권한서비스로검사한다(bool admin)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorizationCore(options => options.AddPolicy(
            게임현실상품자료검토Service.AuthorizationPolicy, p => p.RequireRole(살뜰.Data.역할명.서버관리자)));
        using var provider = services.BuildServiceProvider();
        var principal = Operator();
        if (admin) ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(
            ClaimTypes.Role, 살뜰.Data.역할명.서버관리자));
        var service = new 게임현실상품자료검토Service(provider.GetRequiredService<IAuthorizationService>(), new FixedClock());
        var result = await service.준비Async(null, Request(null, 게임현실상품자료Action.CreateDraft, Draft()), principal);
        Assert.Equal(admin, result.Prepared);
        NoEffects(result);
    }

    [Theory]
    [InlineData("game:thermos")]
    [InlineData("game:bottle")]
    public async Task 현실자료없이_게임상품초안을먼저만든다(string gameId)
    {
        var input = Draft(gameId) with { 현실후보 = null };
        var result = await Service().준비Async(null, Request(null, 게임현실상품자료Action.CreateDraft, input), Operator());
        Assert.True(result.Prepared);
        Assert.Equal(gameId, result.State!.초안.게임상품.StableId);
        Assert.Null(result.State.초안.현실후보);
        Assert.Equal(게임현실상품자료Codes.Draft, result.State.검토상태);
        NoEffects(result);
    }

    [Theory]
    [InlineData(게임현실상품자료Codes.Similar)]
    [InlineData(게임현실상품자료Codes.Identical)]
    public async Task 가상자료_대응승인과제공검토승인은별도이며_외부효과없다(string mapping)
    {
        var service = Service();
        var state = await Pending(service, Draft() with { 대응종류 = mapping });
        Assert.False(state.대응승인됨);
        var mapped = await Apply(service, state, 게임현실상품자료Action.ApproveMapping);
        Assert.True(mapped.대응승인됨);
        Assert.Equal(CommunityInformationReviewStates.PendingReview, mapped.검토상태);
        var result = await service.준비Async(mapped, Request(mapped, 게임현실상품자료Action.Approve), Operator());
        Assert.True(result.Prepared);
        Assert.Equal(CommunityInformationReviewStates.Approved, result.State!.검토상태);
        Assert.Equal(mapping, result.State.초안.대응종류);
        Assert.Equal(게임현실상품자료Codes.Fixture, result.State.초안.현실후보!.자료종류);
        Assert.Equal(4, result.State.History.Count);
        Assert.All(result.State.History, h => { Assert.Equal("fixture-operator", h.검토자Id); Assert.Equal(Now, h.검토시각); });
        NoEffects(result);
    }

    [Theory]
    [InlineData("anonymous")]
    [InlineData("no-id")]
    [InlineData("denied")]
    public async Task 인증주체와기존운영자정책없이_상태나중복응답을반환하지않는다(string mode)
    {
        var auth = new FakeAuthorization();
        var service = new 게임현실상품자료검토Service(auth, new FixedClock());
        var request = Request(null, 게임현실상품자료Action.CreateDraft, Draft());
        var created = (await service.준비Async(null, request, Operator())).State!;
        var principal = mode switch
        {
            "anonymous" => new ClaimsPrincipal(new ClaimsIdentity()),
            "no-id" => new ClaimsPrincipal(new ClaimsIdentity([], "Fixture")),
            _ => Operator()
        };
        auth.Allowed = false;
        var result = await service.준비Async(created, request, principal);
        Assert.False(result.Prepared);
        Assert.Equal("Unauthorized", result.Diagnostic);
        Assert.Null(result.State);
        Assert.Equal("서버관리자전용", auth.LastPolicy);
    }

    [Fact]
    public async Task 같은요청재시도는개정과감사이력을늘리지않고_최신상태를반환한다()
    {
        var service = Service();
        var request = Request(null, 게임현실상품자료Action.CreateDraft, Draft());
        var first = (await service.준비Async(null, request, Operator())).State!;
        var pending = await Apply(service, first, 게임현실상품자료Action.SubmitReview);
        var replay = await service.준비Async(pending, request, Operator());
        Assert.True(replay.Duplicate);
        Assert.Equal(JsonSerializer.Serialize(pending), JsonSerializer.Serialize(replay.State));
        Assert.Equal(2, replay.State!.Revision);
    }

    [Theory]
    [InlineData("note")]
    [InlineData("operator")]
    [InlineData("payload")]
    [InlineData("action")]
    public async Task 같은키의다른내용이나다른검토자는거부한다(string change)
    {
        var service = Service();
        var request = Request(null, 게임현실상품자료Action.CreateDraft, Draft());
        var state = (await service.준비Async(null, request, Operator())).State!;
        var altered = change switch
        {
            "note" => request with { 검토메모 = "다른 근거" },
            "payload" => request with { 초안 = Draft("game:another") },
            "action" => request with { Action = 게임현실상품자료Action.Exclude },
            _ => request
        };
        var result = await service.준비Async(state, altered, Operator(change == "operator" ? "other" : "fixture-operator"));
        Assert.Equal("IdempotencyConflict", result.Diagnostic);
        Assert.Null(result.State);
        Assert.Single(state.History);
    }

    [Fact]
    public async Task 오래된판본과잘못된대상및직접승인을거부한다()
    {
        var service = Service();
        var state = await Apply(service, null, 게임현실상품자료Action.CreateDraft, Draft());
        var request = Request(state, 게임현실상품자료Action.SubmitReview);
        Assert.Equal("RevisionConflict", (await service.준비Async(state, request with { ExpectedRevision = 0 }, Operator())).Diagnostic);
        Assert.Equal("IdentityMismatch", (await service.준비Async(state, request with { StableId = "other" }, Operator())).Diagnostic);
        Assert.Equal("InvalidTransition", (await service.준비Async(state, Request(state, 게임현실상품자료Action.Approve), Operator())).Diagnostic);
        Assert.Equal(1, state.Revision);
    }

    [Fact]
    public async Task 개정은대응과승인을무효화하고_재검토없이승인불가()
    {
        var service = Service();
        var pending = await Pending(service, Draft());
        var mapped = await Apply(service, pending, 게임현실상품자료Action.ApproveMapping);
        var approved = await Apply(service, mapped, 게임현실상품자료Action.Approve);
        var revised = await Apply(service, approved, 게임현실상품자료Action.ReviseDraft,
            Draft() with { 요약 = "내용 개정", 현실후보 = Draft().현실후보! with { StableId = "candidate:new" } });
        Assert.False(revised.대응승인됨);
        Assert.Equal(게임현실상품자료Codes.Draft, revised.검토상태);
        var resubmitted = await Apply(service, revised, 게임현실상품자료Action.SubmitReview);
        Assert.Equal("MappingUnconfirmed", (await service.준비Async(resubmitted, Request(resubmitted, 게임현실상품자료Action.Approve), Operator())).Diagnostic);
        Assert.Equal(CommunityInformationReviewStates.Approved, approved.검토상태);
    }

    [Fact]
    public async Task 초안개정으로게임상품식별자를바꿀수없다()
    {
        var service = Service();
        var state = await Apply(service, null, 게임현실상품자료Action.CreateDraft, Draft());
        Assert.Equal("GameProductMismatch", (await service.준비Async(state,
            Request(state, 게임현실상품자료Action.ReviseDraft, Draft("other")), Operator())).Diagnostic);
    }

    [Theory]
    [InlineData("source", "SourceIncomplete")]
    [InlineData("url", "SourceIncomplete")]
    [InlineData("timestamp", "SourceIncomplete")]
    [InlineData("future", "SourceIncomplete")]
    [InlineData("platform", "SourceIncomplete")]
    [InlineData("seller", "SourceIncomplete")]
    [InlineData("source-id", "SourceIncomplete")]
    [InlineData("rights", "UsageUnreviewed")]
    [InlineData("rights-evidence", "UsageUnreviewed")]
    [InlineData("price", "PriceOrTermsIncomplete")]
    [InlineData("shipping", "PriceOrTermsIncomplete")]
    [InlineData("negative", "PriceOrTermsIncomplete")]
    [InlineData("currency", "PriceOrTermsIncomplete")]
    [InlineData("quantity", "PriceOrTermsIncomplete")]
    [InlineData("unit", "PriceOrTermsIncomplete")]
    [InlineData("moq", "PriceOrTermsIncomplete")]
    [InlineData("spec", "PriceOrTermsIncomplete")]
    [InlineData("delivery", "PriceOrTermsIncomplete")]
    [InlineData("uncollected", "ObservationMissing")]
    [InlineData("comparison", "NotComparable")]
    [InlineData("comparison-evidence", "NotComparable")]
    [InlineData("missing-cost", "NotComparable")]
    public async Task 누락과미확인조건은0으로채우거나승인하지않는다(string field, string expected)
    {
        var draft = Draft();
        var c = draft.현실후보!;
        c = field switch
        {
            "source" => c with { 출처 = null }, "url" => c with { 상품Url = "javascript:bad" },
            "timestamp" => c with { 관측시각 = null }, "future" => c with { 관측시각 = Now.AddDays(1) },
            "platform" => c with { 플랫폼 = null }, "seller" => c with { 판매자 = null },
            "source-id" => c with { 원천상품Id = null },
            "rights" => c with { 이용조건검토상태 = "Pending" }, "rights-evidence" => c with { 이용조건근거 = null },
            "price" => c with { 가격 = c.가격! with { 현재가격 = null } },
            "shipping" => c with { 가격 = c.가격! with { 배송비 = null } },
            "negative" => c with { 가격 = c.가격! with { 현재가격 = -1 } },
            "currency" => c with { 가격 = c.가격! with { 통화코드 = null } },
            "quantity" => c with { 수량 = null }, "unit" => c with { 단위 = null },
            "moq" => c with { 최소주문수량 = null }, "spec" => c with { 규격 = null },
            "delivery" => c with { 배송조건 = null },
            "uncollected" => c with { 자료종류 = 게임현실상품자료Codes.Uncollected }, _ => c
        };
        draft = draft with { 현실후보 = c };
        if (field == "comparison") draft = draft with { 비교상태 = "NotComparable" };
        if (field == "comparison-evidence") draft = draft with { 비교근거 = null };
        if (field == "missing-cost") draft = draft with { 부족조건 = ["관세 미확인"] };
        var service = Service();
        var state = await Apply(service, await Pending(service, draft), 게임현실상품자료Action.ApproveMapping);
        var before = JsonSerializer.Serialize(state);
        var result = await service.준비Async(state, Request(state, 게임현실상품자료Action.Approve), Operator());
        Assert.False(result.Prepared);
        Assert.Equal(expected, result.Diagnostic);
        Assert.Equal(before, JsonSerializer.Serialize(state));
        NoEffects(result);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("no-evidence")]
    public async Task 이름유사나근거없는동일성은대응승인하지않는다(string condition)
    {
        var draft = Draft();
        draft = condition == "unknown" ? draft with { 대응종류 = 게임현실상품자료Codes.Unconfirmed }
            : draft with { 대응근거 = null };
        var service = Service();
        var state = await Pending(service, draft);
        Assert.Equal("MappingUnconfirmed", (await service.준비Async(state, Request(state, 게임현실상품자료Action.ApproveMapping), Operator())).Diagnostic);
    }

    [Fact]
    public async Task 불완전후보도제외할수있고_승인후제외도새개정이다()
    {
        var service = Service();
        var draft = await Apply(service, null, 게임현실상품자료Action.CreateDraft, Draft() with { 현실후보 = null });
        var excluded = await Apply(service, draft, 게임현실상품자료Action.Exclude);
        Assert.Equal(CommunityInformationReviewStates.Excluded, excluded.검토상태);
        var mapped = await Apply(service, await Pending(service, Draft()), 게임현실상품자료Action.ApproveMapping);
        var approved = await Apply(service, mapped, 게임현실상품자료Action.Approve);
        excluded = await Apply(service, approved, 게임현실상품자료Action.Exclude);
        Assert.Equal(approved.Revision + 1, excluded.Revision);
        Assert.Equal(CommunityInformationReviewStates.Excluded, excluded.검토상태);
    }

    [Fact]
    public async Task 승인요청에새초안을끼워넣을수없고_부족한표시는제출불가()
    {
        var service = Service();
        var state = await Pending(service, Draft());
        var request = Request(state, 게임현실상품자료Action.Approve, Draft());
        Assert.Equal("UnexpectedDraft", (await service.준비Async(state, request, Operator())).Diagnostic);
        var incomplete = await Apply(service, null, 게임현실상품자료Action.CreateDraft, Draft() with { 한계 = null });
        Assert.Equal("ReviewDraftIncomplete", (await service.준비Async(incomplete, Request(incomplete, 게임현실상품자료Action.SubmitReview), Operator())).Diagnostic);
    }

    [Fact]
    public async Task 입력목록변조가반환상태에전파되지않고_취소는상태를만들지않는다()
    {
        var service = Service();
        var missing = new List<string> { "배송지 미확인" };
        var state = await Apply(service, null, 게임현실상품자료Action.CreateDraft, Draft() with { 부족조건 = missing });
        missing.Clear();
        Assert.Single(state.초안.부족조건);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)state.초안.부족조건).Clear());
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => service.준비Async(state,
            Request(state, 게임현실상품자료Action.Exclude), Operator(), cts.Token));
        Assert.Single(state.History);
    }

    [Fact]
    public void 생성자에는권한과시계외에_외부실행저장포트가없다()
    {
        var args = Assert.Single(typeof(게임현실상품자료검토Service).GetConstructors()).GetParameters();
        Assert.Equal([typeof(IAuthorizationService), typeof(TimeProvider)], args.Select(a => a.ParameterType));
    }

    private static 게임현실상품자료검토Service Service() => new(new FakeAuthorization(), new FixedClock());
    private static 게임현실상품자료Request Request(게임현실상품자료State? state,
        게임현실상품자료Action action, 게임현실상품자료초안Dto? draft = null)
        => new("curation:fixture", $"request:{action}:{state?.Revision ?? 0}", state?.Revision ?? 0,
            action, "가상자료 검토 근거; 실제 관측 아님", draft);
    private static async Task<게임현실상품자료State> Apply(게임현실상품자료검토Service service,
        게임현실상품자료State? state, 게임현실상품자료Action action, 게임현실상품자료초안Dto? draft = null)
    {
        var result = await service.준비Async(state, Request(state, action, draft), Operator());
        Assert.True(result.Prepared, result.Diagnostic);
        NoEffects(result);
        return result.State!;
    }
    private static async Task<게임현실상품자료State> Pending(게임현실상품자료검토Service service, 게임현실상품자료초안Dto draft)
        => await Apply(service, await Apply(service, null, 게임현실상품자료Action.CreateDraft, draft), 게임현실상품자료Action.SubmitReview);
    private static void NoEffects(게임현실상품자료Result result)
    {
        Assert.False(result.수집실행); Assert.False(result.실제게시); Assert.False(result.통지발송);
        Assert.False(result.게임상태변경); Assert.False(result.영속확정);
    }
    private static 게임현실상품자료초안Dto Draft(string gameId = "game:thermos")
        => new(new(gameId, "가상 용기"),
            new("candidate:fixture-bottle", 게임현실상품자료Codes.Fixture, "FixturePlatform", "가상 판매자",
                "fixture:001", "https://example.invalid/fixture-bottle", Now.AddHours(-1), "가상 500mL 용기",
                new(10m, null, 0m, "USD"), 1m, "개", 1m, "가상 배송지/비용 조건", "fixture-source:v1",
                "fixture-only:not-a-platform-license", CommunityInformationReviewStates.Approved),
            게임현실상품자료Codes.Similar, "가상 용도/규격 비교; 동일 상품 아님", 게임현실상품자료Codes.Comparable,
            "가상 조건 안의 비교만; 현실 수익성 판단 아님", [], "가상 참고 제목", "가상 요약", "Fixture 출처", "실제 가격 아님");
    private sealed class FixedClock : TimeProvider { public override DateTimeOffset GetUtcNow() => Now; }
    private sealed class FakeAuthorization : IAuthorizationService
    {
        public bool Allowed { get; set; } = true;
        public string? LastPolicy { get; private set; }
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
        {
            LastPolicy = policyName;
            return Task.FromResult(Allowed ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        }
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource,
            IEnumerable<IAuthorizationRequirement> requirements) => throw new NotSupportedException();
    }
}
