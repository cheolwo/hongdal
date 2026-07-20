using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.Customs;
using Ssalddel.Domain.HsCodes;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Application.Customs;

public sealed class 화주HS코드검토조회UseCaseTests
{
    [Fact]
    public async Task 목록은_활성원장과공개근거만집계하고_출처기준을반환한다()
    {
        await using var db = CreateContext();
        var entry = await SeedAsync(db);
        var useCase = new 화주HS코드검토조회UseCase(db);

        var result = await useCase.목록Async("의자", null, 1, 30, default);

        Assert.True(result.IsSuccess);
        var response = result.Value;
        var item = Assert.Single(response.Items);
        Assert.Equal(entry.Id, item.ReviewId);
        Assert.Equal("9401.69", item.Code);
        Assert.Equal("소호(6단위)", item.LevelLabel);
        Assert.Equal(1, item.OfficialCaseCount);
        Assert.Equal(1, item.CustomsAgencyExperienceCount);
        Assert.Equal(1, item.ImportAgencyExperienceCount);
        Assert.True(item.BrokerReviewRecommended);
        Assert.Equal("KSH", item.Source.StandardCode);
        Assert.Equal("KR", item.Source.CountryCode);
        Assert.Equal(10, item.Source.CodeDigits);
        Assert.Equal("2026", item.Source.Revision);
        Assert.Equal("https://example.test/ksh", item.Source.SourceUrl);
    }

    [Fact]
    public async Task 상세는_비공개_비동의_유료경험과제공자정보를노출하지않는다()
    {
        await using var db = CreateContext();
        var entry = await SeedAsync(db);
        var useCase = new 화주HS코드검토조회UseCase(db);

        var result = await useCase.상세Async(entry.Id, default);

        Assert.True(result.IsSuccess);
        var detail = result.Value;
        Assert.Single(detail.RiskTags);
        var officialCase = Assert.Single(detail.OfficialCases);
        Assert.Equal("공개 의자", officialCase.ProductName);
        Assert.Empty(officialCase.SourceUrl);
        Assert.Equal(2, detail.AgencyExperiences.Count);
        Assert.DoesNotContain(detail.AgencyExperiences, item => item.Summary.Contains("비공개", StringComparison.Ordinal));
        Assert.DoesNotContain(detail.AgencyExperiences, item => item.Summary.Contains("유료", StringComparison.Ordinal));
        Assert.Equal(new[] { "상업송장", "원산지 정보", "패킹리스트" }, detail.RequiredDocuments);
        Assert.Contains("최종 품목분류", detail.DecisionBoundary, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 상세는_활성상태의정확한Id가아니면_404를반환한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var useCase = new 화주HS코드검토조회UseCase(db);

        var result = await useCase.상세Async(999_999, default);

        Assert.True(result.IsFailed);
        Assert.Equal(StatusCodes.Status404NotFound, result.Errors[0].Metadata["StatusCode"]);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"shipper-hs-review-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static async Task<HsCodeEntry> SeedAsync(SsalddelContext db)
    {
        var catalog = new HsCodeCatalogVersion
        {
            StandardCode = "KSH",
            CountryCode = "KR",
            CodeDigits = 10,
            Revision = "2026",
            SourceName = "관세청 품목분류 원장",
            SourceUrl = "https://example.test/ksh",
            EffectiveFrom = new DateTime(2026, 1, 1),
            ImportedAtUtc = new DateTime(2026, 7, 20, 1, 0, 0, DateTimeKind.Utc),
            IsActive = true
        };
        var entry = new HsCodeEntry
        {
            CatalogVersion = catalog,
            Code = "9401.69",
            NormalizedCode = "940169",
            Level = HsCodeLevel.Subheading,
            KoreanName = "원목 의자",
            EnglishName = "Wooden chair",
            Description = "목재 프레임 의자",
            SearchKeywords = "의자 가구",
            BusinessCategory = HsCodeBusinessCategory.GeneralCargo,
            IsActive = true,
            RiskTags =
            [
                new HsCodeEntryRiskTag
                {
                    TagType = HsCodeRiskTagType.BrokerReviewRecommended,
                    Label = "관세사 검토 권장",
                    Reason = "재질과 용도를 확인해야 합니다.",
                    Source = HsCodeRiskTagSource.BrokerReview,
                    IsActive = true
                },
                new HsCodeEntryRiskTag
                {
                    TagType = HsCodeRiskTagType.Furniture,
                    Label = "비활성 태그",
                    Reason = "표시되면 안 됩니다.",
                    IsActive = false
                }
            ]
        };
        db.HsCodeEntries.Add(entry);
        db.HsCodeEntries.Add(new HsCodeEntry
        {
            CatalogVersion = catalog,
            Code = "9403.20",
            NormalizedCode = "940320",
            Level = HsCodeLevel.Subheading,
            KoreanName = "비활성 가구",
            EnglishName = "Inactive furniture",
            Description = "조회 제외",
            SearchKeywords = "의자",
            BusinessCategory = HsCodeBusinessCategory.GeneralCargo,
            IsActive = false
        });
        await db.SaveChangesAsync();

        db.HsCodeClassificationCases.AddRange(
            new HsCodeClassificationCase
            {
                HsCodeEntryId = entry.Id,
                HsCode = entry.Code,
                CountryCode = "KR",
                SourceType = "사전심사",
                SourceReferenceNo = "CASE-PUBLIC",
                SourceUrl = "javascript:alert(1)",
                IssuingAuthority = "관세평가분류원",
                DecidedAt = new DateTime(2026, 6, 1),
                ProductName = "공개 의자",
                GoodsDescription = "목재 의자",
                DecisionReason = "공개 판단 근거",
                IsPublicOfficialCase = true
            },
            new HsCodeClassificationCase
            {
                HsCodeEntryId = entry.Id,
                HsCode = entry.Code,
                ProductName = "비공개 사례",
                IsPublicOfficialCase = false
            });

        db.HsCodePlatformAgencyExperiences.AddRange(
            Experience(entry.Code, "CustomsAgency", "공개 통관 경험", true, false, "[\"상업송장\",\"패킹리스트\"]"),
            Experience(entry.NormalizedCode, "ImportAgency", "공개 수입 경험", true, false, "[\"원산지 정보\",\"상업송장\"]"),
            Experience(entry.Code, "CustomsAgency", "비공개 경험", false, false, "[\"노출 금지\"]"),
            Experience(entry.Code, "ImportAgency", "유료 경험", true, true, "[\"유료 서류\"]"));
        await db.SaveChangesAsync();
        return entry;
    }

    private static HsCodePlatformAgencyExperience Experience(
        string hsCode,
        string agencyType,
        string summary,
        bool consented,
        bool paid,
        string documentsJson)
        => new()
        {
            HsCode = hsCode,
            AgencyType = agencyType,
            CountryRoute = "CN → KR",
            CaseStatus = "완료",
            RiskLevel = "보통",
            Summary = summary,
            RequiredDocumentsJson = documentsJson,
            ContributorUserId = "private-user-id",
            ContributorConsented = consented,
            IsPaidDetail = paid,
            PaidAccessPrice = paid ? 5_900m : 0m,
            ContributorRewardRate = paid ? 0.7m : 0m,
            DisclosurePolicy = "익명 공개 동의 범위",
            CompletedAtUtc = new DateTime(2026, 7, 1)
        };

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
