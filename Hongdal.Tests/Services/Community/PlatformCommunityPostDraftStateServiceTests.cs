using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Tests.Services.Community;

public sealed class PlatformCommunityPostDraftStateServiceTests
{
    [Fact]
    public void Consume_ReturnsPreparedDraftOnlyOnce()
    {
        var service = new PlatformCommunityPostDraftStateService();
        var draft = new PlatformCommunityPostDraft(
            "자유",
            "커뮤니티 신뢰",
            "제목",
            "본문",
            "https://www.youtube.com/watch?v=video-1",
            "홍익학당 · 재생목록");

        service.Prepare(draft);

        Assert.Same(draft, service.Consume());
        Assert.Null(service.Consume());
    }

    [Fact]
    public void Create_BuildsCommunityDraftWithQuoteSourceAndTimestampLink()
    {
        var draft = PrajnaLectureCommunityShareDraftFactory.Create(
            "홍익학당",
            "6남매 모음",
            "마음을 다스리는 요결",
            "https://www.youtube.com/watch?v=video-1",
            "마음에 남은 글귀",
            "일상에서 다시 살펴보고 싶습니다.",
            "12:34");

        Assert.Equal("자유", draft.Category);
        Assert.Equal("커뮤니티 신뢰", draft.WorkflowTag);
        Assert.StartsWith("[반야 나눔]", draft.Title, StringComparison.Ordinal);
        Assert.Contains("“마음에 남은 글귀”", draft.Body, StringComparison.Ordinal);
        Assert.Contains("일상에서 다시 살펴보고 싶습니다.", draft.Body, StringComparison.Ordinal);
        Assert.Contains("함께 본 강의: 마음을 다스리는 요결", draft.Body, StringComparison.Ordinal);
        Assert.Contains("영상 위치: 12:34", draft.Body, StringComparison.Ordinal);
        Assert.Equal("https://www.youtube.com/watch?v=video-1&t=754s", draft.SharedLinkUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_RejectsEmptyQuote(string quote)
    {
        Assert.Throws<ArgumentException>(() => PrajnaLectureCommunityShareDraftFactory.Create(
            "홍익학당",
            "재생목록",
            "강의",
            "https://www.youtube.com/watch?v=video-1",
            quote,
            null,
            null));
    }

    [Theory]
    [InlineData("12:75")]
    [InlineData("시간")]
    [InlineData("1:60:00")]
    public void Create_RejectsInvalidTimestamp(string timestamp)
    {
        Assert.Throws<ArgumentException>(() => PrajnaLectureCommunityShareDraftFactory.Create(
            "홍익학당",
            "재생목록",
            "강의",
            "https://www.youtube.com/watch?v=video-1",
            "글귀",
            null,
            timestamp));
    }
}
