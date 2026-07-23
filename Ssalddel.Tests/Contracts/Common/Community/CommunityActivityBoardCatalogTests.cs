using Ssalddel.Application.Behaviors;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class CommunityActivityBoardCatalogTests
{
    [Fact]
    public void Catalog_CoversEveryRoadmapVersionWithReadOnlyPublicBoards()
    {
        Assert.All(
            SsalddelProductRoadmapCatalog.All,
            stage => Assert.Contains(
                CommunityActivityBoardCatalog.All,
                definition => definition.ProductVersion == stage.Version));

        Assert.All(
            CommunityActivityBoardCatalog.All,
            definition =>
            {
                Assert.True(definition.Board.IsPublic);
                Assert.False(definition.Board.IsUserCreatable);
                Assert.Equal(
                    CommunityBoardPostingAccessCodes.OperatorOnly,
                    definition.Board.PostingAccessCode);
                Assert.Same(
                    definition.Board,
                    CommunityBoardCatalog.Find(definition.Board.Key));
                Assert.Contains(
                    definition.Board.DisplayName,
                    CommunityBoardCatalog.CategoryNamesFor(definition.Board.Key));
            });
    }

    [Fact]
    public void SurfaceMappingBoundary_DefersMappingsUntilSequentialPaginationIsComplete()
    {
        Assert.Contains("2.0", CommunityActivityBoardCatalog.SurfaceMappingBoundary);
        Assert.Contains("2.5", CommunityActivityBoardCatalog.SurfaceMappingBoundary);
        Assert.Contains("3.0", CommunityActivityBoardCatalog.SurfaceMappingBoundary);
        Assert.Contains("3.5", CommunityActivityBoardCatalog.SurfaceMappingBoundary);
        Assert.Contains("단일책임", CommunityActivityBoardCatalog.SurfaceMappingBoundary);
    }

    [Fact]
    public void Catalog_HasUniqueKeysAndReferencesRealApplicationTypes()
    {
        var definitions = CommunityActivityBoardCatalog.All;
        var applicationTypeNames = typeof(CommunityActivityCommandPostBehavior<,>)
            .Assembly
            .GetTypes()
            .Select(type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            definitions.Count,
            definitions.Select(definition => definition.Board.Key)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.Equal(
            definitions.Count,
            definitions.Select(definition => $"{definition.SourceKind}:{definition.SourceName}")
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.All(
            definitions,
            definition => Assert.Contains(definition.SourceName, applicationTypeNames));
    }

    [Theory]
    [InlineData("운송문제신고됨Event")]
    [InlineData("결제승인완료Event")]
    [InlineData("VisaSupportRequestedEvent")]
    [InlineData("파일업로드완료됨Event")]
    public void Catalog_ExcludesSensitiveOrHighRiskOccurrences(string sourceName)
        => Assert.DoesNotContain(
            CommunityActivityBoardCatalog.All,
            definition => definition.SourceName == sourceName);
}
