using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Tests.Contracts.Common.Orderer;

public sealed class 냉장냉동먹거리공동구매계획기Tests
{
    [Fact]
    public void Plan_FrozenImportedFood_MarksColdChainReviewWhenDemandIsEnough()
    {
        var draft = new 냉장냉동먹거리공동구매초안(
            상품명: "냉동 삼겹살",
            주문자집단배송권키: "road-address-level-2:gyeonggi-suwon",
            TargetQuantityKg: 12000m,
            CurrentIntentQuantityKg: 9000m,
            TargetUnitPrice: 8500m,
            HS코드: "0203.29");

        var plan = 냉장냉동먹거리공동구매계획기.계획(draft);

        Assert.True(plan.IsDemandThresholdMet);
        Assert.True(plan.IsFclCandidate);
        Assert.True(plan.HS먹거리후보여부);
        Assert.Equal(공동구매활성화우선순위코드.냉장냉동먹거리중심, plan.활성화우선순위);
        Assert.Equal(공동구매캠페인상태코드.콜드체인검토, plan.제안상태);
        Assert.Contains("HS 식품 분류 확인", plan.RequiredReviewSteps);
        Assert.Contains("수입식품 신고/검역 검토", plan.RequiredReviewSteps);
        Assert.Contains("냉장/냉동 창고와 운송 가능 여부 확인", plan.RequiredReviewSteps);
    }

    [Fact]
    public void Plan_StaysDemandCheckingWhenDemandIsLow()
    {
        var draft = new 냉장냉동먹거리공동구매초안(
            상품명: "냉동 삼겹살",
            주문자집단배송권키: "road-address-level-2:seoul-gangnam",
            TargetQuantityKg: 12000m,
            CurrentIntentQuantityKg: 3000m,
            TargetUnitPrice: 8500m);

        var plan = 냉장냉동먹거리공동구매계획기.계획(draft);

        Assert.False(plan.IsDemandThresholdMet);
        Assert.Equal(공동구매캠페인상태코드.수요확인, plan.제안상태);
    }

    [Fact]
    public void Plan_DomesticAmbientBulk_CanMoveToImportDecisionWithoutColdChainReview()
    {
        var draft = new 냉장냉동먹거리공동구매초안(
            상품명: "상온 보관 식재료",
            주문자집단배송권키: "road-address-level-2:busan-haeundae",
            TargetQuantityKg: 2000m,
            CurrentIntentQuantityKg: 2000m,
            TargetUnitPrice: 3000m,
            품목분류코드: 공동구매품목분류코드.일반커머스,
            온도코드: 공동구매온도코드.상온,
            LogisticsMode: 공동구매물류방식코드.국내벌크,
            RequiresImportFoodReview: false,
            RequiresMfdsManufacturerReview: false,
            RequiresColdStorage: false);

        var plan = 냉장냉동먹거리공동구매계획기.계획(draft);

        Assert.False(plan.IsFclCandidate);
        Assert.False(plan.HS먹거리후보여부);
        Assert.Equal(공동구매활성화우선순위코드.일반, plan.활성화우선순위);
        Assert.Empty(plan.RequiredReviewSteps);
        Assert.Equal(공동구매캠페인상태코드.수입결정, plan.제안상태);
    }

    [Theory]
    [InlineData("0203.29")]
    [InlineData("1602.49")]
    [InlineData("2106.90")]
    public void Plan_HsFoodChapters_AreFoodFocusedCandidates(string hsCode)
    {
        var draft = new 냉장냉동먹거리공동구매초안(
            상품명: "먹거리 공동구매 후보",
            주문자집단배송권키: "road-address-level-2:incheon-yeonsu",
            TargetQuantityKg: 5000m,
            CurrentIntentQuantityKg: 1000m,
            TargetUnitPrice: 6000m,
            HS코드: hsCode,
            온도코드: 공동구매온도코드.냉장);

        var plan = 냉장냉동먹거리공동구매계획기.계획(draft);

        Assert.True(plan.HS먹거리후보여부);
        Assert.Equal(공동구매활성화우선순위코드.냉장냉동먹거리중심, plan.활성화우선순위);
        Assert.Contains("HS 식품 분류 확인", plan.RequiredReviewSteps);
    }

    [Fact]
    public void Plan_NonFoodHS코드_DoesNotRaiseFoodFocusedPriority()
    {
        var draft = new 냉장냉동먹거리공동구매초안(
            상품명: "일반 커머스 상품",
            주문자집단배송권키: "road-address-level-2:seoul-songpa",
            TargetQuantityKg: 5000m,
            CurrentIntentQuantityKg: 5000m,
            TargetUnitPrice: 6000m,
            HS코드: "8543.70",
            품목분류코드: 공동구매품목분류코드.일반커머스,
            온도코드: 공동구매온도코드.상온,
            LogisticsMode: 공동구매물류방식코드.LCL,
            RequiresImportFoodReview: false,
            RequiresMfdsManufacturerReview: false,
            RequiresColdStorage: false);

        var plan = 냉장냉동먹거리공동구매계획기.계획(draft);

        Assert.False(plan.HS먹거리후보여부);
        Assert.Equal(공동구매활성화우선순위코드.일반, plan.활성화우선순위);
    }

    [Fact]
    public void OpeningApplication_Food상품카드_WithAgreement_CanSubmitForApproval()
    {
        var draft = new 주문자집단개설신청초안(
            ApplicantOrdererId: "orderer-1",
            상품카드: CreatePork상품카드(),
            주문자집단배송권키: "road-address-level-2:gyeonggi-suwon",
            주문자집단배송권명: "경기도 수원시 영통구",
            희망수량Kg: 20m,
            DesiredUnitPrice: 8500m,
            NonBindingAgreementAccepted: true,
            Request메모: "동네 사람들과 같이 주문하고 싶습니다.");

        var plan = 주문자집단개설신청계획기.계획(draft);

        Assert.True(plan.CanSubmit);
        Assert.True(plan.IsFoodFocusedCandidate);
        Assert.Equal(주문자집단개설신청상태코드.승인대기, plan.제안상태);
        Assert.Contains("주문자 집단 범위 승인", plan.RequiredAdminReviewSteps);
        Assert.Contains("HS 식품 코드 확인", plan.RequiredAdminReviewSteps);
        Assert.Contains("냉장/냉동 보관 및 운송 검토", plan.RequiredAdminReviewSteps);
    }

    [Fact]
    public void OpeningApplication_RequiresNonBindingAgreementBeforeSubmit()
    {
        var draft = new 주문자집단개설신청초안(
            ApplicantOrdererId: "orderer-1",
            상품카드: CreatePork상품카드(),
            주문자집단배송권키: "road-address-level-2:gyeonggi-suwon",
            주문자집단배송권명: "경기도 수원시 영통구",
            희망수량Kg: 20m,
            DesiredUnitPrice: 8500m,
            NonBindingAgreementAccepted: false);

        var plan = 주문자집단개설신청계획기.계획(draft);

        Assert.False(plan.CanSubmit);
        Assert.Equal(주문자집단개설신청상태코드.초안, plan.제안상태);
    }

    private static HS먹거리공동구매상품카드 CreatePork상품카드()
    {
        return new HS먹거리공동구매상품카드(
            상품카드Id: "hs-food-0203-pork-frozen",
            상품명: "냉동 삼겹살",
            HS코드: "0203.29",
            HS표시명: "돼지고기 냉동 기타",
            온도코드: 공동구매온도코드.냉동,
            예상물류방식: 공동구매물류방식코드.FCL,
            SuggestedTargetQuantityKg: 12000m,
            ExpectedUnitPrice: 8500m);
    }
}
