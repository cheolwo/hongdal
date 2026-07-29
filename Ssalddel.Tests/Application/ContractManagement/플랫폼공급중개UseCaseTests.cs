using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.ContractManagement;
using Ssalddel.Contracts.Common.ContractManagement;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Application.ContractManagement;

public sealed class 플랫폼공급중개UseCaseTests
{
    [Fact]
    public async Task 활성공급계약은_음식점과살들마트가각자이용등록하고개별발주하게한다()
    {
        await using var context = CreateContext();
        var agreement = await CreateActiveAgreementAsync(context);

        var restaurant = CreateOrganizationUseCase(
            context,
            "restaurant-user",
            공급이용조직유형코드.음식점,
            "101");
        var mart = CreateOrganizationUseCase(
            context,
            "mart-user",
            공급이용조직유형코드.살들마트,
            "mart-seoul-01");

        var restaurantParticipation = await restaurant.공급계약이용등록Async(
            agreement.공급계약Id,
            CreateParticipationRequest(공급이용조직유형코드.음식점, agreement.계약문서버전),
            CancellationToken.None);
        var martParticipation = await mart.공급계약이용등록Async(
            agreement.공급계약Id,
            CreateParticipationRequest(공급이용조직유형코드.살들마트, agreement.계약문서버전),
            CancellationToken.None);

        var restaurantOrder = await restaurant.발주등록Async(
            CreateOrderRequest(
                restaurantParticipation.Value.공급계약이용등록Id,
                agreement.품목목록.Single().공급계약품목Id,
                agreement.계약문서버전),
            CancellationToken.None);
        var martOrder = await mart.발주등록Async(
            CreateOrderRequest(
                martParticipation.Value.공급계약이용등록Id,
                agreement.품목목록.Single().공급계약품목Id,
                agreement.계약문서버전),
            CancellationToken.None);

        Assert.True(restaurantOrder.IsSuccess);
        Assert.True(martOrder.IsSuccess);
        Assert.Equal("101", restaurantOrder.Value.구매조직참조Key);
        Assert.Equal("mart-seoul-01", martOrder.Value.구매조직참조Key);
        Assert.Equal(공급중개역할코드.개별발주중개, restaurantOrder.Value.플랫폼역할코드);
        Assert.False(restaurantOrder.Value.플랫폼판매자여부);
        Assert.False(restaurantOrder.Value.플랫폼재판매자여부);
        Assert.False(restaurantOrder.Value.결제실행됨);
        Assert.False(restaurantOrder.Value.재고예약됨);
        Assert.False(restaurantOrder.Value.입고생성됨);
        Assert.Equal(2, await context.공급계약이용등록.CountAsync());
        Assert.Equal(2, await context.조직개별공급발주.CountAsync());
        Assert.Empty(context.입고요청);
        Assert.Empty(context.마트주문);
        Assert.Empty(context.음식주문);
    }

    [Fact]
    public async Task 개별발주는_조직접근범위와수량및계약버전을검증하고멱등저장한다()
    {
        await using var context = CreateContext();
        var agreement = await CreateActiveAgreementAsync(context);
        var owner = CreateOrganizationUseCase(
            context,
            "restaurant-user",
            공급이용조직유형코드.음식점,
            "101");
        var participation = await owner.공급계약이용등록Async(
            agreement.공급계약Id,
            CreateParticipationRequest(공급이용조직유형코드.음식점, agreement.계약문서버전),
            CancellationToken.None);
        var request = CreateOrderRequest(
            participation.Value.공급계약이용등록Id,
            agreement.품목목록.Single().공급계약품목Id,
            agreement.계약문서버전);

        var first = await owner.발주등록Async(request, CancellationToken.None);
        var repeated = await owner.발주등록Async(request, CancellationToken.None);
        var changedQuantity = CreateOrderRequest(
            participation.Value.공급계약이용등록Id,
            agreement.품목목록.Single().공급계약품목Id,
            agreement.계약문서버전);
        changedQuantity.클라이언트요청Id = request.클라이언트요청Id;
        changedQuantity.발주수량 = 11;
        var conflict = await owner.발주등록Async(changedQuantity, CancellationToken.None);

        var otherOrganization = CreateOrganizationUseCase(
            context,
            "other-user",
            공급이용조직유형코드.음식점,
            "202");
        var forbidden = await otherOrganization.발주등록Async(
            CreateOrderRequest(
                participation.Value.공급계약이용등록Id,
                agreement.품목목록.Single().공급계약품목Id,
                agreement.계약문서버전),
            CancellationToken.None);

        Assert.Equal(first.Value.개별공급발주Id, repeated.Value.개별공급발주Id);
        Assert.Equal(409, conflict.Errors.Single().Metadata["StatusCode"]);
        Assert.Equal(403, forbidden.Errors.Single().Metadata["StatusCode"]);
        Assert.Single(context.조직개별공급발주);
    }

    [Fact]
    public async Task 공급자응답은_증거와수량을기록하지만_재고와입고를생성하지않는다()
    {
        await using var context = CreateContext();
        var agreement = await CreateActiveAgreementAsync(context);
        var organization = CreateOrganizationUseCase(
            context,
            "restaurant-user",
            공급이용조직유형코드.음식점,
            "101");
        var participation = await organization.공급계약이용등록Async(
            agreement.공급계약Id,
            CreateParticipationRequest(공급이용조직유형코드.음식점, agreement.계약문서버전),
            CancellationToken.None);
        var order = await organization.발주등록Async(
            CreateOrderRequest(
                participation.Value.공급계약이용등록Id,
                agreement.품목목록.Single().공급계약품목Id,
                agreement.계약문서버전),
            CancellationToken.None);

        var admin = new 플랫폼공급계약관리UseCase(
            context,
            new TestCurrentUserAccessor("platform-admin"));
        var response = await admin.공급자응답기록Async(
            order.Value.개별공급발주Id,
            new 개별공급발주공급자응답기록요청
            {
                공급자응답상태코드 = 개별공급발주상태코드.공급자부분수락,
                수락수량 = 6,
                공급자응답근거참조 = "supplier-portal:response-001",
                공급자응답확인 = true
            },
            CancellationToken.None);
        var withdrawalAfterResponse = await organization.발주철회Async(
            order.Value.개별공급발주Id,
            new 개별공급발주철회요청
            {
                조직유형코드 = 공급이용조직유형코드.음식점
            },
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(개별공급발주상태코드.공급자부분수락, response.Value.상태코드);
        Assert.Equal(6, response.Value.공급자수락수량);
        Assert.Equal("supplier-portal:response-001", response.Value.공급자응답근거참조);
        Assert.Equal(409, withdrawalAfterResponse.Errors.Single().Metadata["StatusCode"]);
        Assert.Empty(context.입고요청);
        Assert.Empty(context.재고이력);
        Assert.Empty(context.마트주문);
        Assert.Empty(context.음식주문);
    }

    [Fact]
    public async Task 계약품목은_허용된조직에만노출되고_활성계약문서동의를요구한다()
    {
        await using var context = CreateContext();
        var agreement = await CreateActiveAgreementAsync(
            context,
            [공급이용조직유형코드.음식점]);
        var restaurant = CreateOrganizationUseCase(
            context,
            "restaurant-user",
            공급이용조직유형코드.음식점,
            "101");
        var mart = CreateOrganizationUseCase(
            context,
            "mart-user",
            공급이용조직유형코드.살들마트,
            "mart-seoul-01");

        var restaurantAgreements = await restaurant.이용가능계약조회Async(
            공급이용조직유형코드.음식점,
            CancellationToken.None);
        var martAgreements = await mart.이용가능계약조회Async(
            공급이용조직유형코드.살들마트,
            CancellationToken.None);
        var staleVersion = await restaurant.공급계약이용등록Async(
            agreement.공급계약Id,
            CreateParticipationRequest(공급이용조직유형코드.음식점, "old-version"),
            CancellationToken.None);

        Assert.Single(restaurantAgreements.Value);
        Assert.Empty(martAgreements.Value);
        Assert.Equal(409, staleVersion.Errors.Single().Metadata["StatusCode"]);
    }

    private static async Task<플랫폼공급계약응답> CreateActiveAgreementAsync(
        SsalddelContext context,
        IReadOnlyList<string>? allowedOrganizationTypes = null)
    {
        var admin = new 플랫폼공급계약관리UseCase(
            context,
            new TestCurrentUserAccessor("platform-admin"));
        var draft = await admin.등록Async(
            new 플랫폼공급계약등록요청
            {
                클라이언트요청Id = Guid.NewGuid(),
                계약번호 = $"SUPPLY-{Guid.NewGuid():N}",
                공급자Key = "supplier-farm-01",
                공급자명 = "푸른산지영농조합",
                계약문서버전 = "terms-2026-01",
                유효시작Utc = DateTime.UtcNow.AddDays(-1),
                유효종료Utc = DateTime.UtcNow.AddMonths(6),
                통화코드 = "KRW",
                정산조건 = "각 구매조직과 공급자가 개별 발주별로 정산",
                반품조건 = "검수 불합격 물량은 공급자와 구매조직이 직접 처리",
                플랫폼중개전용확인 = true,
                품목목록 =
                [
                    new 플랫폼공급계약품목등록요청
                    {
                        계약품목Key = "onion-20kg",
                        SKU = "ONION-20KG",
                        품목명 = "양파",
                        공급단위 = "20kg 망",
                        계약단가 = 28_000m,
                        최소발주수량 = 2,
                        최대발주수량 = 20,
                        원산지표시 = "대한민국",
                        보관조건 = "상온",
                        허용조직유형목록 = allowedOrganizationTypes
                                         ?? [
                                             공급이용조직유형코드.음식점,
                                             공급이용조직유형코드.살들마트
                                         ]
                    }
                ]
            },
            CancellationToken.None);
        var activated = await admin.활성화Async(
            draft.Value.공급계약Id,
            new 플랫폼공급계약활성화요청
            {
                계약문서버전 = draft.Value.계약문서버전,
                계약체결근거참조 = "contracts:supplier-farm-01:2026-01",
                공급자체결확인 = true,
                플랫폼중개전용확인 = true
            },
            CancellationToken.None);
        return activated.Value;
    }

    private static 공급계약이용등록요청 CreateParticipationRequest(
        string organizationTypeCode,
        string termsVersion)
        => new()
        {
            클라이언트요청Id = Guid.NewGuid(),
            조직유형코드 = organizationTypeCode,
            계약문서버전 = termsVersion,
            공급계약이용동의 = true,
            개별발주별도확인동의 = true,
            안내버전 = 공급중개안내.현재버전
        };

    private static 개별공급발주등록요청 CreateOrderRequest(
        Guid participationId,
        Guid itemId,
        string termsVersion)
        => new()
        {
            클라이언트요청Id = Guid.NewGuid(),
            공급계약이용등록Id = participationId,
            공급계약품목Id = itemId,
            발주수량 = 10,
            희망납품일Utc = DateTime.UtcNow.AddDays(3),
            납품지참조Key = "delivery-site:primary",
            계약문서버전 = termsVersion,
            개별발주확인 = true,
            공급자판매자확인 = true,
            플랫폼중개자확인 = true,
            안내버전 = 공급중개안내.현재버전
        };

    private static 조직개별공급발주UseCase CreateOrganizationUseCase(
        SsalddelContext context,
        string userId,
        string organizationTypeCode,
        string organizationKey)
        => new(
            context,
            new TestCurrentUserAccessor(userId),
            new TestOrganizationAccess(organizationTypeCode, organizationKey));

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed record TestCurrentUserAccessor(string? UserId) : ICurrentUserAccessor
    {
        public string? Role => null;
    }

    private sealed class TestOrganizationAccess(
        string organizationTypeCode,
        string organizationKey) : I공급조직접근Accessor
    {
        public string? 조직참조Key조회(string requestedOrganizationTypeCode)
            => string.Equals(
                requestedOrganizationTypeCode,
                organizationTypeCode,
                StringComparison.Ordinal)
                ? organizationKey
                : null;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
