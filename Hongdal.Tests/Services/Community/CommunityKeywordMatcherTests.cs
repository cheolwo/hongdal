using Hongdal.Domain.Community;
using Hongdal.Services.Community;

namespace Hongdal.Tests.Services.Community;

public sealed class CommunityKeywordMatcherTests
{
    private readonly CommunityKeywordMatcher _matcher = new();

    [Fact]
    public void NormalizeAndValidate_NormalizesCompatibilityCaseAndWhitespace()
    {
        var keyword = _matcher.NormalizeAndValidate("  ＣＯＬＤ\t  Chain  ");

        Assert.Equal("cold chain", keyword);
    }

    [Fact]
    public void IsMatch_MatchesKoreanKeywordInsidePostText()
    {
        var post = CreatePost(title: "이번 주 햇사과 공동구매를 엽니다");
        var keyword = _matcher.NormalizeAndValidate("사과");

        Assert.True(_matcher.IsMatch(keyword, post));
    }

    [Fact]
    public void IsMatch_MatchesCategoryAndWorkflowTags()
    {
        var post = CreatePost(
            title: "참여자를 찾습니다",
            category: "공동 구매",
            workflowTag: "냉장 운송");

        Assert.True(_matcher.IsMatch(_matcher.NormalizeAndValidate("공동 구매"), post));
        Assert.True(_matcher.IsMatch(_matcher.NormalizeAndValidate("냉장"), post));
    }

    [Fact]
    public void IsMatch_UsesWordBoundariesForAsciiKeyword()
    {
        var post = CreatePost(title: "Daily maintenance notice");

        Assert.False(_matcher.IsMatch(_matcher.NormalizeAndValidate("ai"), post));
        Assert.True(_matcher.IsMatch(
            _matcher.NormalizeAndValidate("ai"),
            CreatePost(title: "AI 공동구매 도우미")));
    }

    [Theory]
    [InlineData("---")]
    [InlineData("   ")]
    public void NormalizeAndValidate_RejectsKeywordWithoutLettersOrNumbers(string value)
    {
        Assert.ThrowsAny<ArgumentException>(() => _matcher.NormalizeAndValidate(value));
    }

    private static PlatformCommunityPost CreatePost(
        string title,
        string body = "",
        string category = "자유",
        string workflowTag = "국내 화물 운송")
        => new()
        {
            Title = title,
            Body = body,
            Category = category,
            WorkflowTag = workflowTag,
            RoleTag = "플랫폼 구성원"
        };
}
