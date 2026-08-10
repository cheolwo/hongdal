using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

internal sealed class 공통식품품목IdentityConfiguration
    : IEntityTypeConfiguration<공통식품품목Identity>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<공통식품품목Identity> builder)
    {
        builder.ToTable("agri_common_food_product_identities");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.CanonicalProductStableId).HasMaxLength(120).IsRequired();
        builder.Property(item => item.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Revision).HasMaxLength(80).IsRequired();
        builder.HasIndex(item => item.CanonicalProductStableId).IsUnique();
        builder.HasIndex(item => new { item.IsActive, item.DisplayName });
        builder.HasData(new 공통식품품목Identity
        {
            Id = 1,
            CanonicalProductStableId = "product:potato",
            DisplayName = "감자",
            Revision = "common-food-product-identity.v1",
            IsActive = true,
            CreatedAtUtc = 공통식품품목IdentitySeed.Timestamp,
            UpdatedAtUtc = 공통식품품목IdentitySeed.Timestamp
        });
    }
}

internal sealed class 공통식품품목Code관계Configuration
    : IEntityTypeConfiguration<공통식품품목Code관계>, IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<공통식품품목Code관계> builder)
    {
        builder.ToTable("agri_common_food_product_code_relations");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.RelationStableId).HasMaxLength(180).IsRequired();
        builder.Property(item => item.SourceKey).HasMaxLength(120).IsRequired();
        builder.Property(item => item.CodeScheme).HasMaxLength(80).IsRequired();
        builder.Property(item => item.ExternalCode).HasMaxLength(200);
        builder.Property(item => item.ParentCode).HasMaxLength(100);
        builder.Property(item => item.Label).HasMaxLength(300).IsRequired();
        builder.Property(item => item.RelationStatusCode).HasMaxLength(30).IsRequired();
        builder.Property(item => item.MatchQualityCode).HasMaxLength(80).IsRequired();
        builder.Property(item => item.EvidenceNote).HasMaxLength(2000).IsRequired();
        builder.HasOne(item => item.ProductIdentity)
            .WithMany(item => item.CodeRelations)
            .HasForeignKey(item => item.ProductIdentityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => item.RelationStableId).IsUnique();
        builder.HasIndex(item => new
        {
            item.ProductIdentityId,
            item.SourceKey,
            item.CodeScheme,
            item.ExternalCode
        }).HasDatabaseName("IX_cfpr_product_source_scheme_code");
        builder.HasIndex(item => new
        {
            item.SourceKey,
            item.CodeScheme,
            item.ExternalCode,
            item.IsActive
        });
        builder.HasData(
            공통식품품목IdentitySeed.Relation(1, "relation:product:potato:kamis", "kamis", "KAMIS_ITEM", "152", "100", "감자", "Confirmed", "SourceCodeConfirmed", "KAMIS 식량작물 100의 품목코드 152로 저장·조회되는 관계입니다."),
            공통식품품목IdentitySeed.Relation(2, "relation:product:potato:hs4", "wco-hs", "HS4", "0701", null, "감자", "Candidate", "ExactCommodityCandidate", "국제 HS 4단위 후보이며 종자용 여부·가공도·용도에 따라 국가 세번이 달라질 수 있습니다."),
            공통식품품목IdentitySeed.Relation(3, "relation:product:potato:usda-ams", "usda-ams-market-news", "USDA_AMS_COMMODITY", "Potatoes", null, "Potatoes", "Candidate", "DirectCommodityCandidate", "식용 감자 공통 품목 후보이며 종서용 감자와 품종·등급·시장 단계는 별도로 검토합니다."),
            공통식품품목IdentitySeed.Relation(4, "relation:product:potato:nongsaro", "nongsaro:farm-working-plan-new", "NONGSARO_KIND_OF_COMMODITY", null, null, "농사로 감자 품목구분", "Unlinked", "OfficialCodeRequired", "농사로 공식 품목구분Code를 현재 근거에서 확인하지 못해 이름으로 연결하지 않습니다."));
    }
}

internal sealed class 공통식품품목Code관계검토이력Configuration
    : IEntityTypeConfiguration<공통식품품목Code관계검토이력>,
        IDedicatedDbContextConfiguration
{
    public void Configure(EntityTypeBuilder<공통식품품목Code관계검토이력> builder)
    {
        builder.ToTable("agri_common_food_product_code_relation_reviews");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.RelationStatusCode).HasMaxLength(30).IsRequired();
        builder.Property(item => item.ExternalCode).HasMaxLength(200);
        builder.Property(item => item.ReviewActionCode).HasMaxLength(50).IsRequired();
        builder.Property(item => item.ReviewReason).HasMaxLength(2000).IsRequired();
        builder.Property(item => item.ReviewedBySubjectId).HasMaxLength(120).IsRequired();
        builder.HasOne(item => item.CodeRelation)
            .WithMany(item => item.ReviewHistory)
            .HasForeignKey(item => item.CodeRelationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.CodeRelationId, item.Revision }).IsUnique();
        builder.HasIndex(item => item.ReviewedAtUtc);
        builder.HasData(
            Review(1, 1, "Confirmed", "152", "공식 source code 초기 등록"),
            Review(2, 2, "Candidate", "0701", "국제 HS 후보 초기 등록"),
            Review(3, 3, "Candidate", "Potatoes", "USDA AMS Commodity 후보 초기 등록"),
            Review(4, 4, "Unlinked", null, "공식 농사로 품목구분Code 확인 전 미연결 등록"));
    }

    private static 공통식품품목Code관계검토이력 Review(
        long id,
        long relationId,
        string statusCode,
        string? externalCode,
        string reason)
        => new()
        {
            Id = id,
            CodeRelationId = relationId,
            Revision = 1,
            RelationStatusCode = statusCode,
            ExternalCode = externalCode,
            ReviewActionCode = "Initialized",
            ReviewReason = reason,
            ReviewedBySubjectId = "system-seed",
            ReviewedAtUtc = 공통식품품목IdentitySeed.Timestamp
        };
}

file static class 공통식품품목IdentitySeed
{
    public static readonly DateTime Timestamp =
        new(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc);

    public static 공통식품품목Code관계 Relation(
        long id,
        string stableId,
        string sourceKey,
        string codeScheme,
        string? externalCode,
        string? parentCode,
        string label,
        string statusCode,
        string matchQualityCode,
        string evidenceNote)
        => new()
        {
            Id = id,
            ProductIdentityId = 1,
            RelationStableId = stableId,
            SourceKey = sourceKey,
            CodeScheme = codeScheme,
            ExternalCode = externalCode,
            ParentCode = parentCode,
            Label = label,
            RelationStatusCode = statusCode,
            MatchQualityCode = matchQualityCode,
            EvidenceNote = evidenceNote,
            Revision = 1,
            IsActive = true,
            CreatedAtUtc = Timestamp,
            UpdatedAtUtc = Timestamp
        };
}
