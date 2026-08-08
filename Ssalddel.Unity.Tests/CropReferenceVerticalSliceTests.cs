using Ssalddel.Unity.Crops;
using Xunit;

namespace Ssalddel.Unity.Tests;

public sealed class CropReferenceVerticalSliceTests
{
    [Fact]
    public void 공개_작물분류ApiModel을_Unity_snapshot으로_변환한다()
    {
        var mapped = new CropReferenceCategoryMapper().Map(ApiModel());

        Assert.True(mapped.IsMapped);
        var value = Assert.IsType<작물기준정보분류Snapshot>(mapped.Value);
        Assert.Equal("nongsaro:crop-ebook", value.SourceKey);
        Assert.Equal("crop-reference-category:fc", Assert.Single(value.Items).StableId);
    }

    [Fact]
    public void 출처와_기준시각이_없으면_mapping을_거부한다()
    {
        var apiModel = ApiModel();
        apiModel.SourceHref = string.Empty;
        apiModel.RetrievedAt = default;

        var mapped = new CropReferenceCategoryMapper().Map(apiModel);

        Assert.False(mapped.IsMapped);
        Assert.Contains("SourceHrefMissing", mapped.ErrorCodes);
        Assert.Contains("RetrievedAtMissing", mapped.ErrorCodes);
    }

    [Fact]
    public void 중복된_stableId와_분류Code를_거부한다()
    {
        var apiModel = ApiModel();
        apiModel.Items = new[] { apiModel.Items[0], apiModel.Items[0] };

        var mapped = new CropReferenceCategoryMapper().Map(apiModel);

        Assert.Contains("DuplicateStableId:crop-reference-category:fc", mapped.ErrorCodes);
        Assert.Contains("DuplicateCategoryCode:FC", mapped.ErrorCodes);
    }

    [Fact]
    public async Task Repository와_UseCase가_ApiClient_경계를_통해_조회한다()
    {
        var client = new FakeApiClient(ApiModel());
        var repository = new CropReferenceApiRepository(
            client,
            new CropReferenceCategoryMapper());
        var useCase = new 작물기준정보분류조회UseCase(repository);

        var result = await useCase.실행Async();

        Assert.True(result.IsMapped);
        Assert.Equal(1, client.RequestCount);
    }

    private static CropReferenceCategoryListApiModel ApiModel()
        => new()
        {
            SourceTypeCode = CropReferenceCategoryMapper.PublicReferenceSourceType,
            SourceKey = "nongsaro:crop-ebook",
            SourceName = "농촌진흥청 농사로 작목별 농업기술정보",
            SourceHref = "https://www.nongsaro.go.kr/open-api",
            RetrievedAt = DateTimeOffset.Parse("2026-08-08T04:00:00Z"),
            Boundary = "특정 농장의 현재 재배 상태가 아닙니다.",
            Items = new[]
            {
                new CropReferenceCategoryItemApiModel
                {
                    StableId = "crop-reference-category:fc",
                    CategoryCode = "FC",
                    CategoryName = "식량작물",
                },
            },
        };

    private sealed class FakeApiClient(CropReferenceCategoryListApiModel response)
        : ICropReferenceCategoryApiClient
    {
        public int RequestCount { get; private set; }

        public Task<CropReferenceCategoryListApiModel> GetAsync(
            CancellationToken cancellationToken = default)
        {
            RequestCount++;
            return Task.FromResult(response);
        }
    }
}
