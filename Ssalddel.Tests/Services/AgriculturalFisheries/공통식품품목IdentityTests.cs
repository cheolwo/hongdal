using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class 공통식품품목IdentityTests
{
    [Fact]
    public void 감자는_공통StableId아래에_출처별코드와관계상태를분리한다()
    {
        var item = 공통식품품목IdentityCatalog.GetRequired("product:potato");

        Assert.Equal("감자", item.DisplayName);
        Assert.Equal(4, item.CodeRelations.Count);
        Assert.Contains(item.CodeRelations, relation =>
            relation.CodeScheme == 공통식품품목CodeSchemes.KamisItem
            && relation.Code == "152"
            && relation.ParentCode == "100"
            && relation.RelationStatusCode == 공통식품품목관계StatusCodes.Confirmed);
        Assert.Contains(item.CodeRelations, relation =>
            relation.CodeScheme == 공통식품품목CodeSchemes.Hs4
            && relation.Code == "0701"
            && relation.RelationStatusCode == 공통식품품목관계StatusCodes.Candidate);
        Assert.Contains(item.CodeRelations, relation =>
            relation.CodeScheme == 공통식품품목CodeSchemes.UsdaAmsCommodity
            && relation.Code == "Potatoes"
            && relation.RelationStatusCode == 공통식품품목관계StatusCodes.Candidate);
    }

    [Fact]
    public void 농사로공식품목Code가없으면_이름으로추정하지않고_Unlinked를유지한다()
    {
        var item = 공통식품품목IdentityCatalog.GetRequired("product:potato");
        var nongsaro = Assert.Single(item.CodeRelations, relation =>
            relation.CodeScheme == 공통식품품목CodeSchemes.NongsaroKindOfCommodity);

        Assert.Null(nongsaro.Code);
        Assert.Equal(공통식품품목관계StatusCodes.Unlinked, nongsaro.RelationStatusCode);
        Assert.Contains("공식", nongsaro.EvidenceNote, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 조회UseCase는_DB에서_StableId와검토이력을조회한다()
    {
        await using var db = CreateDb();
        Seed(db);
        await db.SaveChangesAsync();
        var sut = new 공통식품품목Identity조회UseCase(db);

        var potato = await sut.단건조회Async(" product:potato ");

        Assert.Equal("product:potato", potato?.CanonicalProductStableId);
        Assert.Null(await sut.단건조회Async("product:unknown"));
        Assert.Single((await sut.목록조회Async()).Items);
        Assert.All(potato!.CodeRelations, relation =>
        {
            Assert.Equal(1, relation.Revision);
            Assert.Single(relation.ReviewHistory);
        });
    }

    [Fact]
    public async Task DB가비어있으면_정적Catalog로대체하지않는다()
    {
        await using var db = CreateDb();
        var sut = new 공통식품품목Identity조회UseCase(db);

        var result = await sut.목록조회Async();

        Assert.Empty(result.Items);
        Assert.Null(await sut.단건조회Async("product:potato"));
    }

    private static AgriculturalFisheriesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private static void Seed(AgriculturalFisheriesDbContext db)
    {
        var source = 공통식품품목IdentityCatalog.GetRequired("product:potato");
        var product = new 공통식품품목Identity
        {
            CanonicalProductStableId = source.CanonicalProductStableId,
            DisplayName = source.DisplayName,
            Revision = source.Revision
        };
        var relationIndex = 0;
        foreach (var sourceRelation in source.CodeRelations)
        {
            relationIndex++;
            var relation = new 공통식품품목Code관계
            {
                RelationStableId = $"relation:potato:{relationIndex}",
                SourceKey = sourceRelation.SourceKey,
                CodeScheme = sourceRelation.CodeScheme,
                ExternalCode = sourceRelation.Code,
                ParentCode = sourceRelation.ParentCode,
                Label = sourceRelation.Label,
                RelationStatusCode = sourceRelation.RelationStatusCode,
                MatchQualityCode = sourceRelation.MatchQualityCode,
                EvidenceNote = sourceRelation.EvidenceNote,
                Revision = sourceRelation.Revision
            };
            foreach (var history in sourceRelation.ReviewHistory)
            {
                relation.ReviewHistory.Add(new 공통식품품목Code관계검토이력
                {
                    Revision = history.Revision,
                    RelationStatusCode = history.RelationStatusCode,
                    ExternalCode = history.ExternalCode,
                    ReviewActionCode = history.ReviewActionCode,
                    ReviewReason = history.ReviewReason,
                    ReviewedBySubjectId = "system-seed",
                    ReviewedAtUtc = history.ReviewedAtUtc
                });
            }

            product.CodeRelations.Add(relation);
        }

        db.CommonFoodProductIdentities.Add(product);
    }
}
