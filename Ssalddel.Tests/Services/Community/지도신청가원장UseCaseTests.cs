using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Privacy;
using Ssalddel.Services.Community;
using Ssalddel.Services.Privacy;

namespace Ssalddel.Tests.Services.Community;

public sealed class 지도신청가원장UseCaseTests
{
    public static TheoryData<string, string> 업무별원장종류 => new()
    {
        { 신청개인정보업무Codes.물류대행, CommunityLedgerTemplateKeys.WarehouseInbound },
        { 신청개인정보업무Codes.운송대행, CommunityLedgerTemplateKeys.CargoTransport },
        { 신청개인정보업무Codes.개별주문, CommunityLedgerTemplateKeys.Order }
    };

    [Theory]
    [MemberData(nameof(업무별원장종류))]
    public async Task 지도신청은_유효한동의후_업무별비구속가원장으로저장된다(
        string workCode,
        string expectedTemplateKey)
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var evidenceId = Guid.NewGuid();

        var result = await useCase.생성Async(
            Request(workCode, evidenceId),
            "user-1",
            "지도 신청자");

        Assert.Equal(expectedTemplateKey, result.원장템플릿Key);
        Assert.Equal("news-us-1", result.MapMarkerId);
        Assert.Equal(workCode, result.업무Code);
        Assert.Equal(지도신청가원장정책.신청접수단계, result.현재단계Key);
        Assert.Equal(커뮤니티원장상태.초안, result.상태);
        Assert.False(result.외부실행발생);
        Assert.False(result.기존가원장재사용);
        Assert.Equal(evidenceId, consent.LastEvidenceId);
        Assert.Equal(workCode, consent.LastWorkCode);

        var saved = Assert.IsType<커뮤니티원장저장요청>(store.LastSaveRequest);
        Assert.Equal(expectedTemplateKey, saved.원장템플릿Key);
        Assert.Equal(CommunityPostProvisionalLedgerPolicy.LedgerMaturityCode,
            saved.확장속성[CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey]);
        Assert.Equal(CommunityPostProvisionalLedgerPolicy.NonBindingEffectCode,
            saved.확장속성[CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey]);
        Assert.Equal(bool.FalseString, saved.확장속성["OperationalHandoffAllowed"]);
        Assert.Equal(evidenceId.ToString("D"), saved.외부참조["ApplicationPrivacyConsentEvidenceId"]);
        Assert.Equal("news-us-1", Assert.Single(saved.블록목록).Data["MarkerId"]);
    }

    [Fact]
    public async Task 마커별내원장조회는_본인의정확한마커원장만_반환한다()
    {
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(new RecordingConsentService(), store);
        var own = await useCase.생성Async(
            Request(신청개인정보업무Codes.운송대행, Guid.NewGuid()),
            "user-1",
            "신청자");
        var otherMarkerRequest = Request(신청개인정보업무Codes.개별주문, Guid.NewGuid());
        otherMarkerRequest.MarkerId = "news-us-2";
        await useCase.생성Async(otherMarkerRequest, "user-1", "신청자");
        await useCase.생성Async(
            Request(신청개인정보업무Codes.물류대행, Guid.NewGuid()),
            "user-2",
            "다른 신청자");

        var result = await useCase.내마커원장조회Async("news-us-1", null, "user-1");

        var ledger = Assert.Single(result);
        Assert.Equal(own.원장Id, ledger.원장Id);
        Assert.Equal("news-us-1", ledger.MapMarkerId);
        Assert.Equal(신청개인정보업무Codes.운송대행, ledger.업무Code);
        Assert.True(ledger.기존가원장재사용);
    }

    [Fact]
    public async Task 마커와원장Id가불일치하면_내원장조회가노출하지않는다()
    {
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(new RecordingConsentService(), store);
        var markerOne = await useCase.생성Async(
            Request(신청개인정보업무Codes.운송대행, Guid.NewGuid()),
            "user-1",
            "신청자");
        var markerTwoRequest = Request(신청개인정보업무Codes.개별주문, Guid.NewGuid());
        markerTwoRequest.MarkerId = "news-us-2";
        var markerTwo = await useCase.생성Async(markerTwoRequest, "user-1", "신청자");

        var mismatch = await useCase.내마커원장조회Async("news-us-1", markerTwo.원장Id, "user-1");
        var match = await useCase.내마커원장조회Async("news-us-1", markerOne.원장Id, "user-1");

        Assert.Empty(mismatch);
        Assert.Equal(markerOne.원장Id, Assert.Single(match).원장Id);
    }

    [Fact]
    public async Task 같은동의증적재시도는_같은가원장을재사용한다()
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var request = Request(신청개인정보업무Codes.운송대행, Guid.NewGuid());

        var first = await useCase.생성Async(request, "user-1", "신청자");
        var second = await useCase.생성Async(request, "user-1", "신청자");

        Assert.Equal(first.원장Id, second.원장Id);
        Assert.True(second.기존가원장재사용);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task 유효한동의를확인하지못하면_원장을저장하지않는다()
    {
        var consent = new RecordingConsentService { Reject = true };
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.생성Async(
            Request(신청개인정보업무Codes.개별주문, Guid.NewGuid()),
            "user-1",
            "신청자"));

        Assert.Equal(0, store.SaveCount);
    }

    [Fact]
    public async Task 신청저장성공을반영하면_운영원본을연결하고_실원장으로전환한다()
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var evidenceId = Guid.NewGuid();
        var provisional = await useCase.생성Async(
            Request(신청개인정보업무Codes.운송대행, evidenceId),
            "user-1",
            "신청자");

        var submitted = await useCase.신청제출반영Async(
            provisional.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.운송대행,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "CargoTransportRequest",
                운영원본Id = "cargo-123"
            },
            "user-1");

        Assert.True(submitted.실원장전환됨);
        Assert.True(submitted.외부실행발생);
        Assert.Equal(커뮤니티원장상태.진행중, submitted.상태);
        Assert.Equal(지도신청가원장정책.신청제출단계, submitted.현재단계Key);
        Assert.Equal("CargoTransportRequest", submitted.운영원본종류);
        Assert.Equal("cargo-123", submitted.운영원본Id);
        var saved = Assert.IsType<커뮤니티원장저장요청>(store.LastSaveRequest);
        Assert.Equal(provisional.Revision, saved.기대Revision);
        Assert.Equal(지도신청가원장정책.실원장성숙도Code,
            saved.확장속성[CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey]);
        Assert.Equal(지도신청가원장정책.신청제출효과Code,
            saved.확장속성[CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey]);
        Assert.Equal(bool.TrueString, saved.확장속성["OperationalHandoffAllowed"]);
        Assert.Equal("cargo-123", saved.외부참조[지도신청가원장정책.운영원본IdKey]);
        Assert.True(커뮤니티원장업무투영동기화Service.업무투영허용(await store.원장조회Async(provisional.원장Id) ?? throw new InvalidOperationException()));
    }

    [Fact]
    public async Task 같은운영원본전환재시도는_중복저장하지않는다()
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var evidenceId = Guid.NewGuid();
        var provisional = await useCase.생성Async(
            Request(신청개인정보업무Codes.개별주문, evidenceId),
            "user-1",
            "신청자");
        var request = new 지도신청실원장전환Request
        {
            신청개인정보동의증적Id = evidenceId,
            업무Code = 신청개인정보업무Codes.개별주문,
            신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
            운영원본종류 = "MartOrderRequest",
            운영원본Id = Guid.NewGuid().ToString("D")
        };

        await useCase.신청제출반영Async(provisional.원장Id, request, "user-1");
        var retried = await useCase.신청제출반영Async(provisional.원장Id, request, "user-1");

        Assert.True(retried.기존가원장재사용);
        Assert.Equal(2, store.SaveCount);
    }

    [Fact]
    public async Task 동의철회는_기존운영신청을취소하지않고_원장투영만보류한다()
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var evidenceId = Guid.NewGuid();
        var provisional = await useCase.생성Async(
            Request(신청개인정보업무Codes.물류대행, evidenceId),
            "user-1",
            "신청자");
        await useCase.신청제출반영Async(
            provisional.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.물류대행,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "WarehouseInboundRequest",
                운영원본Id = "42"
            },
            "user-1");
        consent.EvidenceState = 신청개인정보동의상태Codes.철회;
        consent.EvidenceWorkCode = 신청개인정보업무Codes.물류대행;

        var held = await useCase.동의철회반영Async(
            provisional.원장Id,
            new 지도신청동의철회반영Request { 신청개인정보동의증적Id = evidenceId },
            "user-1");

        Assert.True(held.동의철회보류);
        Assert.False(held.운영신청자동취소됨);
        Assert.Equal(커뮤니티원장상태.보류, held.상태);
        Assert.Equal(지도신청가원장정책.동의철회확인단계, held.현재단계Key);
        Assert.Equal("42", held.운영원본Id);
        var ledger = await store.원장조회Async(provisional.원장Id) ?? throw new InvalidOperationException();
        Assert.False(커뮤니티원장업무투영동기화Service.업무투영허용(ledger));
        Assert.Equal(bool.FalseString, ledger.확장속성["OperationalApplicationAutomaticallyCancelled"]);
    }

    [Fact]
    public async Task 다른Client재조회는_동의철회후_같은원장의최신상태를본다()
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var evidenceId = Guid.NewGuid();
        var provisional = await useCase.생성Async(
            Request(신청개인정보업무Codes.개별주문, evidenceId),
            "user-1",
            "신청자");
        await useCase.신청제출반영Async(
            provisional.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.개별주문,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "MartOrderRequest",
                운영원본Id = "order-refresh-1"
            },
            "user-1");
        var before = Assert.Single(await useCase.내마커원장조회Async(
            "news-us-1",
            provisional.원장Id,
            "user-1"));
        consent.EvidenceState = 신청개인정보동의상태Codes.철회;
        consent.EvidenceWorkCode = 신청개인정보업무Codes.개별주문;

        await useCase.동의철회반영Async(
            provisional.원장Id,
            new 지도신청동의철회반영Request { 신청개인정보동의증적Id = evidenceId },
            "user-1");
        var after = Assert.Single(await useCase.내마커원장조회Async(
            "news-us-1",
            provisional.원장Id,
            "user-1"));

        Assert.Equal(커뮤니티원장상태.진행중, before.상태);
        Assert.False(before.동의철회보류);
        Assert.Equal(커뮤니티원장상태.보류, after.상태);
        Assert.True(after.동의철회보류);
        Assert.Equal(지도신청가원장정책.동의철회확인단계, after.현재단계Key);
        Assert.True(after.Revision > before.Revision);
    }

    [Fact]
    public async Task 운영원본조회는_본인의연결된지도신청원장과_동의증적만반환한다()
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var evidenceId = Guid.NewGuid();
        var provisional = await useCase.생성Async(
            Request(신청개인정보업무Codes.개별주문, evidenceId),
            "user-1",
            "신청자");
        await useCase.신청제출반영Async(
            provisional.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.개별주문,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "MartOrderRequest",
                운영원본Id = "request-1"
            },
            "user-1");

        var found = await useCase.운영원본조회Async(
            신청개인정보업무Codes.개별주문,
            "MartOrderRequest",
            "request-1",
            "user-1");
        var hiddenFromOtherUser = await useCase.운영원본조회Async(
            신청개인정보업무Codes.개별주문,
            "MartOrderRequest",
            "request-1",
            "user-2");

        Assert.NotNull(found);
        Assert.Equal(evidenceId, found.신청개인정보동의증적Id);
        Assert.Equal(provisional.원장Id, found.원장Id);
        Assert.Null(hiddenFromOtherUser);
    }

    [Fact]
    public async Task 운영원본조회는_업무와다른원본종류를거부한다()
    {
        var useCase = new 지도신청가원장UseCase(
            new RecordingConsentService(),
            new RecordingLedgerStore());

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.운영원본조회Async(
            신청개인정보업무Codes.물류대행,
            "CargoTransportRequest",
            "1",
            "user-1"));
    }

    [Fact]
    public async Task 사용자가취소한운영신청은_자동취소가아닌명시취소로원장을닫는다()
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var evidenceId = Guid.NewGuid();
        var provisional = await useCase.생성Async(
            Request(신청개인정보업무Codes.개별주문, evidenceId),
            "user-1",
            "신청자");
        await useCase.신청제출반영Async(
            provisional.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.개별주문,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "MartOrderRequest",
                운영원본Id = "request-1"
            },
            "user-1");

        var cancelled = await useCase.운영신청취소반영Async(
            provisional.원장Id,
            new 지도신청운영취소반영Request
            {
                운영원본종류 = "MartOrderRequest",
                운영원본Id = "request-1"
            },
            "user-1");
        var retried = await useCase.운영신청취소반영Async(
            provisional.원장Id,
            new 지도신청운영취소반영Request
            {
                운영원본종류 = "MartOrderRequest",
                운영원본Id = "request-1"
            },
            "user-1");

        Assert.True(cancelled.운영신청취소됨);
        Assert.False(cancelled.운영신청자동취소됨);
        Assert.Equal(커뮤니티원장상태.닫힘, cancelled.상태);
        Assert.Equal(지도신청가원장정책.운영신청취소단계, cancelled.현재단계Key);
        Assert.True(retried.기존가원장재사용);
        Assert.Equal(3, store.SaveCount);
        var ledger = await store.원장조회Async(provisional.원장Id) ?? throw new InvalidOperationException();
        Assert.Equal("UserExplicit", ledger.확장속성["CancellationMode"]);
        Assert.Equal(지도신청가원장정책.신청취소효과Code,
            ledger.확장속성[CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey]);
    }

    [Fact]
    public async Task 원장과다른운영신청취소결과는_반영하지않는다()
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var evidenceId = Guid.NewGuid();
        var provisional = await useCase.생성Async(
            Request(신청개인정보업무Codes.물류대행, evidenceId),
            "user-1",
            "신청자");
        await useCase.신청제출반영Async(
            provisional.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.물류대행,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "WarehouseInboundRequest",
                운영원본Id = "42"
            },
            "user-1");

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.운영신청취소반영Async(
            provisional.원장Id,
            new 지도신청운영취소반영Request
            {
                운영원본종류 = "WarehouseInboundRequest",
                운영원본Id = "99"
            },
            "user-1"));
    }

    [Fact]
    public async Task 운송취소는_물리삭제없이_관리자검토대기원장으로전환한다()
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var evidenceId = Guid.NewGuid();
        var provisional = await useCase.생성Async(
            Request(신청개인정보업무Codes.운송대행, evidenceId),
            "user-1",
            "신청자");
        await useCase.신청제출반영Async(
            provisional.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.운송대행,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "CargoTransportRequest",
                운영원본Id = "cargo-1"
            },
            "user-1");
        var request = new 지도신청운송취소검토요청Request
        {
            운영원본Id = "cargo-1",
            사유 = "상차 일정이 변경되었습니다."
        };

        var pending = await useCase.운송취소검토요청Async(
            provisional.원장Id,
            request,
            "user-1");
        var retried = await useCase.운송취소검토요청Async(
            provisional.원장Id,
            request,
            "user-1");

        Assert.Equal(커뮤니티원장상태.보류, pending.상태);
        Assert.Equal(지도신청가원장정책.운송취소검토단계, pending.현재단계Key);
        Assert.Equal(지도신청가원장정책.운송취소검토요청됨Code, pending.운송취소검토상태Code);
        Assert.Equal(request.사유, pending.운송취소검토사유);
        Assert.False(pending.운영신청취소됨);
        Assert.False(pending.운영신청자동취소됨);
        Assert.True(retried.기존가원장재사용);
        Assert.Equal(3, store.SaveCount);
        var ledger = await store.원장조회Async(provisional.원장Id) ?? throw new InvalidOperationException();
        Assert.Equal(bool.FalseString, ledger.확장속성["OperationalHandoffAllowed"]);
        Assert.False(커뮤니티원장업무투영동기화Service.업무투영허용(ledger));
    }

    [Fact]
    public async Task 운송취소검토는_다른사용자와다른운영원본을거부한다()
    {
        var consent = new RecordingConsentService();
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(consent, store);
        var evidenceId = Guid.NewGuid();
        var provisional = await useCase.생성Async(
            Request(신청개인정보업무Codes.운송대행, evidenceId),
            "user-1",
            "신청자");
        await useCase.신청제출반영Async(
            provisional.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.운송대행,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "CargoTransportRequest",
                운영원본Id = "cargo-1"
            },
            "user-1");
        var request = new 지도신청운송취소검토요청Request
        {
            운영원본Id = "cargo-2",
            사유 = "검토 사유"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.운송취소검토요청Async(
            provisional.원장Id,
            request,
            "user-2"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.운송취소검토요청Async(
            provisional.원장Id,
            request,
            "user-1"));
    }

    [Fact]
    public async Task 관리자검토목록은_운송취소검토대기원장만_반환한다()
    {
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(new RecordingConsentService(), store);
        var evidenceId = Guid.NewGuid();
        var transport = await useCase.생성Async(Request(신청개인정보업무Codes.운송대행, evidenceId), "user-1", "신청자");
        await useCase.신청제출반영Async(
            transport.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.운송대행,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "CargoTransportRequest",
                운영원본Id = "cargo-review-1"
            },
            "user-1");
        await useCase.운송취소검토요청Async(
            transport.원장Id,
            new 지도신청운송취소검토요청Request { 운영원본Id = "cargo-review-1", 사유 = "일정 변경" },
            "user-1");
        await useCase.생성Async(Request(신청개인정보업무Codes.개별주문, Guid.NewGuid()), "user-2", "다른 신청자");

        var reviews = await useCase.관리자운송취소검토목록Async();

        var review = Assert.Single(reviews);
        Assert.Equal(transport.원장Id, review.원장Id);
        Assert.Equal(지도신청가원장정책.운송취소검토요청됨Code, review.운송취소검토상태Code);
    }

    [Fact]
    public async Task 관리자승인은_운송취소검토원장을_닫고_재시도를허용한다()
    {
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(new RecordingConsentService(), store);
        var evidenceId = Guid.NewGuid();
        var transport = await useCase.생성Async(Request(신청개인정보업무Codes.운송대행, evidenceId), "user-1", "신청자");
        await useCase.신청제출반영Async(
            transport.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.운송대행,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "CargoTransportRequest",
                운영원본Id = "cargo-approve-1"
            },
            "user-1");
        await useCase.운송취소검토요청Async(
            transport.원장Id,
            new 지도신청운송취소검토요청Request { 운영원본Id = "cargo-approve-1", 사유 = "신청자 요청" },
            "user-1");
        var decision = new 지도신청운송취소검토처리Request
        {
            승인 = true,
            확인운영원본Id = "cargo-approve-1",
            검토사유 = "신청자 본인 요청 확인"
        };

        await useCase.관리자운송취소검토확인Async(transport.원장Id, decision, "admin-1");
        var approved = await useCase.관리자운송취소검토결과반영Async(transport.원장Id, decision, "admin-1");
        var retried = await useCase.관리자운송취소검토결과반영Async(transport.원장Id, decision, "admin-1");

        Assert.Equal(커뮤니티원장상태.닫힘, approved.상태);
        Assert.Equal(지도신청가원장정책.운영신청취소단계, approved.현재단계Key);
        Assert.Equal(지도신청가원장정책.운송취소검토승인Code, approved.운송취소검토상태Code);
        Assert.True(approved.운영신청취소됨);
        Assert.True(retried.기존가원장재사용);
    }

    [Fact]
    public async Task 관리자거절은_운송신청을_유지하고_다른확인Id는거부한다()
    {
        var store = new RecordingLedgerStore();
        var useCase = new 지도신청가원장UseCase(new RecordingConsentService(), store);
        var evidenceId = Guid.NewGuid();
        var transport = await useCase.생성Async(Request(신청개인정보업무Codes.운송대행, evidenceId), "user-1", "신청자");
        await useCase.신청제출반영Async(
            transport.원장Id,
            new 지도신청실원장전환Request
            {
                신청개인정보동의증적Id = evidenceId,
                업무Code = 신청개인정보업무Codes.운송대행,
                신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
                운영원본종류 = "CargoTransportRequest",
                운영원본Id = "cargo-reject-1"
            },
            "user-1");
        await useCase.운송취소검토요청Async(
            transport.원장Id,
            new 지도신청운송취소검토요청Request { 운영원본Id = "cargo-reject-1", 사유 = "취소 희망" },
            "user-1");
        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.관리자운송취소검토확인Async(
            transport.원장Id,
            new 지도신청운송취소검토처리Request
            {
                승인 = false,
                확인운영원본Id = "cargo-other",
                검토사유 = "확인 불가"
            },
            "admin-1"));

        var rejected = await useCase.관리자운송취소검토결과반영Async(
            transport.원장Id,
            new 지도신청운송취소검토처리Request
            {
                승인 = false,
                확인운영원본Id = "cargo-reject-1",
                검토사유 = "취소 요건 확인 불가"
            },
            "admin-1");

        Assert.Equal(커뮤니티원장상태.진행중, rejected.상태);
        Assert.Equal(지도신청가원장정책.신청제출단계, rejected.현재단계Key);
        Assert.Equal(지도신청가원장정책.운송취소검토거절Code, rejected.운송취소검토상태Code);
        Assert.False(rejected.운영신청취소됨);
    }

    private static 지도신청가원장생성Request Request(string workCode, Guid evidenceId)
        => new()
        {
            신청개인정보동의증적Id = evidenceId,
            업무Code = workCode,
            신청출처Code = 신청개인정보출처Codes.커뮤니티지도,
            MarkerId = "news-us-1",
            MarkerName = "지역 언론사",
            LayerCode = "news-publisher",
            CountryCode = "US"
        };

    private sealed class RecordingConsentService : I신청개인정보동의증적Service
    {
        public bool Reject { get; init; }
        public string EvidenceState { get; set; } = 신청개인정보동의상태Codes.유효;
        public string EvidenceWorkCode { get; set; } = 신청개인정보업무Codes.개별주문;
        public Guid? LastEvidenceId { get; private set; }
        public string? LastWorkCode { get; private set; }

        public Task 유효한동의요구Async(
            Guid? evidenceId,
            string workCode,
            string sourceCode,
            string userId,
            CancellationToken cancellationToken = default)
        {
            LastEvidenceId = evidenceId;
            LastWorkCode = workCode;
            return Reject
                ? Task.FromException(new InvalidOperationException("동의 증적이 유효하지 않습니다."))
                : Task.CompletedTask;
        }

        public Task<신청개인정보동의증적Response> 동의기록Async(
            신청개인정보동의기록Request request,
            string userId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<신청개인정보동의증적Response?> 내증적조회Async(
            Guid evidenceId,
            string userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<신청개인정보동의증적Response?>(new 신청개인정보동의증적Response
            {
                증적Id = evidenceId,
                업무Code = EvidenceWorkCode,
                출처Code = 신청개인정보출처Codes.커뮤니티지도,
                상태Code = EvidenceState
            });

        public Task<신청개인정보동의증적Response> 철회Async(
            Guid evidenceId,
            신청개인정보동의철회Request request,
            string userId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class RecordingLedgerStore : I커뮤니티원장저장소
    {
        private readonly Dictionary<string, 커뮤니티원장Dto> ledgers = [];

        public 커뮤니티원장저장요청? LastSaveRequest { get; private set; }
        public int SaveCount { get; private set; }

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            LastSaveRequest = request;
            SaveCount++;
            var ledger = new 커뮤니티원장Dto
            {
                원장Id = request.원장Id!,
                Revision = ledgers.TryGetValue(request.원장Id!, out var existing) ? existing.Revision + 1 : 1,
                커뮤니티Id = request.커뮤니티Id,
                원장템플릿Key = request.원장템플릿Key,
                제목 = request.제목,
                원함 = request.원함,
                상태 = request.상태 ?? 커뮤니티원장상태.초안,
                현재단계Key = request.현재단계Key,
                대상OsCode = request.대상OsCode,
                대상OsName = request.대상OsName,
                생성자UserId = request.생성자UserId,
                생성자표시명 = request.생성자표시명 ?? "신청자",
                블록목록 = request.블록목록,
                참여자목록 = request.참여자목록,
                외부참조 = request.외부참조,
                확장속성 = request.확장속성
            };
            ledgers[ledger.원장Id] = ledger;
            return Task.FromResult(ledger);
        }

        public Task<커뮤니티원장Dto?> 원장조회Async(
            string 원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(ledgers.GetValueOrDefault(원장Id));

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>(ledgers.Values.ToArray());

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
