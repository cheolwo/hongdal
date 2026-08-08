using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class CropReferenceCategoryQueryTests
{
    [Fact]
    public async Task 농사로_분류를_출처가_있는_typed_projection으로_변환한다()
    {
        var retrievedAt = DateTimeOffset.Parse("2026-08-08T04:00:00Z");
        var source = Response(
            retrievedAt,
            Item("FC", "식량작물"),
            Item("VC", "채소"));
        var sut = new 작물기준정보분류조회UseCase(new FakeModule(source));

        var result = await sut.조회Async();

        Assert.Equal(CropReferenceSourceTypeCodes.PublicReference, result.SourceTypeCode);
        Assert.Equal("nongsaro:crop-ebook", result.SourceKey);
        Assert.Equal(retrievedAt, result.RetrievedAt);
        Assert.Equal(Nongsaro공공데이터Catalog.DocumentationUrl, result.SourceHref);
        Assert.Equal("crop-reference-category:fc", result.Items[0].StableId);
        Assert.Equal("식량작물", result.Items[0].CategoryName);
        Assert.Contains("현재 재배 상태", result.Boundary, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 중복된_분류Code는_잘못된_source_snapshot으로_거부한다()
    {
        var source = Response(
            DateTimeOffset.UtcNow,
            Item("FC", "식량작물"),
            Item("fc", "중복 식량작물"));
        var sut = new 작물기준정보분류조회UseCase(new FakeModule(source));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.조회Async());

        Assert.Contains("중복", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 분류Code나_이름이_없으면_거부한다()
    {
        var source = Response(
            DateTimeOffset.UtcNow,
            new Nongsaro공공데이터Item(new Dictionary<string, string>
            {
                ["mainCategoryCode"] = "FC"
            }));
        var sut = new 작물기준정보분류조회UseCase(new FakeModule(source));

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.조회Async());
    }

    private static Nongsaro공공데이터Response Response(
        DateTimeOffset retrievedAt,
        params Nongsaro공공데이터Item[] items)
        => new(
            Nongsaro공공데이터Catalog.작목기술Service,
            Nongsaro공공데이터Catalog.작목기술주분류Operation,
            "00",
            "정상",
            retrievedAt,
            Nongsaro공공데이터Catalog.DocumentationUrl,
            items);

    private static Nongsaro공공데이터Item Item(string code, string name)
        => new(new Dictionary<string, string>
        {
            ["mainCategoryCode"] = code,
            ["mainCategoryNm"] = name
        });

    private sealed class FakeModule(Nongsaro공공데이터Response response)
        : I농사로작목기술Module
    {
        public Task<Nongsaro공공데이터Response> 주분류조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult(response);
    }
}
