using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Infrastructure.Persistence;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class OfficialFoodRecipeSourceConfiguration
    : IEntityTypeConfiguration<OfficialFoodRecipeSource>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<OfficialFoodRecipeSource> builder)
    {
        builder.ToTable("food_official_recipe_sources");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Provider).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.LanguageCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.AccessMethod).HasMaxLength(40).IsRequired();
        builder.Property(x => x.DocumentationUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.TermsUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.LicenseCode).HasMaxLength(120).IsRequired();
        builder.Property(x => x.TextReusePolicy).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ImageReusePolicy).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.AttributionTemplate).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.UpdateCycle).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AutomationState).HasMaxLength(40).IsRequired();

        builder.HasIndex(x => x.SourceKey).IsUnique();
        builder.HasIndex(x => new { x.CountryCode, x.AutomationState });

        builder.HasData(OfficialFoodRecipeSourceSeedData.All);
    }
}

internal static class OfficialFoodRecipeSourceSeedData
{
    private static readonly DateTime RightsVerifiedAtUtc =
        new(2026, 7, 21, 0, 0, 0, DateTimeKind.Utc);

    public static IReadOnlyList<OfficialFoodRecipeSource> All { get; } =
    [
        Create(
            1,
            OfficialFoodRecipeSourceKeys.MfdsCookRecipe,
            "식품의약품안전처 식품안전나라",
            "식약처 조리식품 레시피 COOKRCP01",
            "KR",
            "ko",
            "JSON Open API",
            "https://www.foodsafetykorea.go.kr/api/openApiInfo.do?menu_grp=MENU_GRP31&menu_no=661&show_cnt=10&start_idx=1&svc_no=COOKRCP01",
            "https://www.data.go.kr/data/15060073/openapi.do",
            "공공데이터포털 이용허락범위 제한 없음",
            "원문과 구조화 레시피를 내부 아카이브에 저장할 수 있으나 커뮤니티 게시 전 출처·영양 단위·최신성을 검토합니다.",
            "초기 수집에서는 이미지 URL만 기록하며 이미지 파일은 복제하지 않습니다.",
            "출처: 식품의약품안전처 식품안전나라 COOKRCP01 ({url}, 수집일 {date})",
            "실시간 원천, 운영 수집 주기는 일 1회 이하 권장",
            OfficialFoodRecipeAutomationStates.EnabledWhenConfigured,
            true),
        Create(
            2,
            OfficialFoodRecipeSourceKeys.RdaLocalFood,
            "농촌진흥청 농사로",
            "농촌진흥청 향토 음식",
            "KR",
            "ko",
            "XML Open API",
            "https://www.data.go.kr/data/15101449/openapi.do",
            "https://www.nongsaro.go.kr/portal/ps/psn/psnj/openApiLst.ps?menuId=PS65428&pageIndex=1&pageSize=&sLclasCode=&sText=%ED%96%A5%ED%86%A0+%EC%9D%8C%EC%8B%9D",
            "공공데이터포털 제한 없음 / 농사로 목록 공공누리 유형3 표시",
            "두 공식 페이지의 권리 표기가 달라 내부 검토 아카이브로만 저장하고 외부 게시에는 별도 권리 확인이 필요합니다.",
            "사진 권리는 별도 확인 대상으로 보고 URL만 기록하며 파일은 복제하지 않습니다.",
            "출처: 농촌진흥청 농사로 향토 음식 ({url}, 수집일 {date})",
            "실시간 원천, 월 1회 변경 확인 권장",
            OfficialFoodRecipeAutomationStates.EnabledWhenConfigured,
            true),
        Create(
            3,
            OfficialFoodRecipeSourceKeys.MaffRegionalCuisine,
            "일본 농림수산성(MAFF)",
            "일본 Our Regional Cuisines",
            "JP",
            "en",
            "Official HTML",
            "https://www.maff.go.jp/e/policies/market/k_ryouri/",
            "https://www.maff.go.jp/e/use/term_use.html",
            "Public Data License 1.0 (별도 표시 없는 정부 텍스트)",
            "출처와 편집 여부를 표시한 텍스트 아카이브만 허용하며 제3자 표기가 있는 내용은 수동 검토합니다.",
            "대부분의 사진은 제3자 권리이므로 URL만 기록하고 파일은 복제하지 않습니다.",
            "Created by Ssalddel by extracting text from the Ministry of Agriculture, Forestry and Fisheries website ({url}, accessed {date}); images are not reused.",
            "페이지 변경 시 갱신, 월 1회 확인 권장",
            OfficialFoodRecipeAutomationStates.Enabled,
            true),
        Create(
            4,
            OfficialFoodRecipeSourceKeys.NhsHealthierFamilies,
            "NHS England",
            "NHS Healthier Families recipes",
            "GB",
            "en",
            "Official HTML / JSON-LD",
            "https://www.nhs.uk/healthier-families/recipes/",
            "https://www.nhs.uk/our-policies/terms-and-conditions/",
            "Open Government Licence v3.0 with NHS terms",
            "복사일과 개별 원문 링크를 기록하며 7일이 지난 사본은 다시 수집하기 전 게시 후보에서 제외합니다.",
            "로고·시각물·다수 사진은 표준 허락에서 제외되므로 이미지 URL도 외부 게시에 사용하지 않습니다.",
            "Information from the NHS website ({url}), copied on {date}. Licensed under the Open Government Licence v3.0.",
            "최소 7일마다 갱신, NHS 권장 주기는 24시간",
            OfficialFoodRecipeAutomationStates.Enabled,
            true),
        CreateMetadataOnly(
            5,
            OfficialFoodRecipeSourceKeys.UsdaMyPlate,
            "USDA MyPlate",
            "미국 MyPlate Kitchen 레시피",
            "US",
            "en",
            "https://www.myplate.gov/myplate-kitchen/recipes",
            "https://www.usda.gov/policies-and-links",
            "연방정부·제3자 제공 레시피가 혼재하므로 항목별 권리 확인 전 링크 메타데이터만 저장합니다."),
        CreateMetadataOnly(
            6,
            OfficialFoodRecipeSourceKeys.HealthCanada,
            "Health Canada",
            "캐나다 Food Guide 레시피",
            "CA",
            "en",
            "https://food-guide.canada.ca/en/recipes/",
            "https://www.canada.ca/en/transparency/terms.html",
            "상업적 복제 허가와 제3자 제공 여부를 항목별로 확인하기 전 링크 메타데이터만 저장합니다."),
        CreateMetadataOnly(
            7,
            OfficialFoodRecipeSourceKeys.FranceAgriculture,
            "Ministère de l'Agriculture et de la Souveraineté alimentaire",
            "프랑스 농업부 음식·레시피 자료",
            "FR",
            "fr",
            "https://agriculture.gouv.fr/recettes",
            "https://agriculture.gouv.fr/mentions-legales",
            "재이용 범위와 사진 권리를 항목별로 확인하기 전 링크 메타데이터만 저장합니다.")
    ];

    private static OfficialFoodRecipeSource Create(
        long id,
        string sourceKey,
        string provider,
        string displayName,
        string countryCode,
        string languageCode,
        string accessMethod,
        string documentationUrl,
        string termsUrl,
        string licenseCode,
        string textReusePolicy,
        string imageReusePolicy,
        string attributionTemplate,
        string updateCycle,
        string automationState,
        bool fullTextStorageAllowed)
        => new()
        {
            Id = id,
            SourceKey = sourceKey,
            Provider = provider,
            DisplayName = displayName,
            CountryCode = countryCode,
            LanguageCode = languageCode,
            AccessMethod = accessMethod,
            DocumentationUrl = documentationUrl,
            TermsUrl = termsUrl,
            LicenseCode = licenseCode,
            TextReusePolicy = textReusePolicy,
            ImageReusePolicy = imageReusePolicy,
            AttributionTemplate = attributionTemplate,
            UpdateCycle = updateCycle,
            AutomationState = automationState,
            FullTextStorageAllowed = fullTextStorageAllowed,
            ImageBinaryStorageAllowed = false,
            RequiresEditorialReview = true,
            RightsVerifiedAtUtc = RightsVerifiedAtUtc,
            UpdatedAtUtc = RightsVerifiedAtUtc
        };

    private static OfficialFoodRecipeSource CreateMetadataOnly(
        long id,
        string sourceKey,
        string provider,
        string displayName,
        string countryCode,
        string languageCode,
        string documentationUrl,
        string termsUrl,
        string limitation)
        => Create(
            id,
            sourceKey,
            provider,
            displayName,
            countryCode,
            languageCode,
            "Metadata link only",
            documentationUrl,
            termsUrl,
            "항목별 확인 필요",
            limitation,
            "이미지 저장·재사용 금지",
            "공식 원문 링크: {url} (확인일 {date})",
            "수동 권리 검토 시 갱신",
            OfficialFoodRecipeAutomationStates.MetadataOnly,
            false);
}

internal sealed class OfficialFoodDishConfiguration
    : IEntityTypeConfiguration<OfficialFoodDish>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<OfficialFoodDish> builder)
    {
        builder.ToTable("food_official_dishes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DishKey).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.RegionName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.OriginalName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.EnglishName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.RepresentationState).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ReviewState).HasMaxLength(40).IsRequired();

        builder.HasIndex(x => x.DishKey).IsUnique();
        builder.HasIndex(x => new { x.CountryCode, x.RegionName, x.ReviewState });
        builder.HasIndex(x => x.Name);
    }
}

internal sealed class OfficialFoodRecipeVariantConfiguration
    : IEntityTypeConfiguration<OfficialFoodRecipeVariant>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<OfficialFoodRecipeVariant> builder)
    {
        builder.ToTable("food_official_recipe_variants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecordKey).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ExternalId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Summary).HasColumnType("longtext").IsRequired();
        builder.Property(x => x.RegionName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ServingText).HasMaxLength(300).IsRequired();
        builder.Property(x => x.IngredientsJson).HasColumnType("json").IsRequired();
        builder.Property(x => x.InstructionsJson).HasColumnType("json").IsRequired();
        builder.Property(x => x.NutritionJson).HasColumnType("json").IsRequired();
        builder.Property(x => x.TagsJson).HasColumnType("json").IsRequired();
        builder.Property(x => x.Tips).HasColumnType("longtext").IsRequired();
        builder.Property(x => x.OriginalUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ImageReferenceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.RawPayload).HasColumnType("longtext").IsRequired();
        builder.Property(x => x.ContentChecksum).HasMaxLength(64).IsRequired();
        builder.Property(x => x.LicenseCodeAtCollection).HasMaxLength(120).IsRequired();
        builder.Property(x => x.TextReusePolicyAtCollection).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ImageReusePolicyAtCollection).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.AttributionText).HasMaxLength(1000).IsRequired();

        builder.HasOne(x => x.Source)
            .WithMany(x => x.RecipeVariants)
            .HasForeignKey(x => x.SourceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Dish)
            .WithMany(x => x.RecipeVariants)
            .HasForeignKey(x => x.DishId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FirstCollectionRun)
            .WithMany(x => x.NewRecipeVariants)
            .HasForeignKey(x => x.FirstCollectionRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RecordKey).IsUnique();
        builder.HasIndex(x => new { x.SourceId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.DishId, x.LastCollectedAtUtc });
        builder.HasIndex(x => new { x.ContentExpiresAtUtc, x.IsRemovedAtSource });
    }
}

internal sealed class OfficialFoodRecipeCollectionRunConfiguration
    : IEntityTypeConfiguration<OfficialFoodRecipeCollectionRun>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<OfficialFoodRecipeCollectionRun> builder)
    {
        builder.ToTable("food_official_recipe_collection_runs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RunKey).HasMaxLength(40).IsRequired();
        builder.Property(x => x.SourceKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.QuerySummary).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000).IsRequired();

        builder.HasIndex(x => x.RunKey).IsUnique();
        builder.HasIndex(x => new { x.SourceKey, x.StatusCode, x.StartedAtUtc });
    }
}
