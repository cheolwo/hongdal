using Hongdal.Services.Community;

namespace Hongdal.Tests.Services.Community;

public sealed class 커뮤니티게시글음성본문분할기Tests
{
    private readonly 커뮤니티게시글음성본문분할기 _sut = new();

    [Fact]
    public void 분할_짧은글은_제목과본문을_한구간으로_정규화한다()
    {
        var result = _sut.분할("  오늘의 글 ", "첫째 줄\n둘째 줄", 1900);

        Assert.Equal("오늘의 글. 첫째 줄 둘째 줄", Assert.Single(result));
    }

    [Fact]
    public void 분할_긴글은_Typecast최대길이이하의_여러구간으로_보존한다()
    {
        var body = string.Join(' ', Enumerable.Repeat("홍달에서 함께 일하고 이야기합니다.", 120));

        var result = _sut.분할("긴 글", body, 500);

        Assert.True(result.Count > 1);
        Assert.All(result, segment => Assert.InRange(segment.Length, 1, 500));
        var expected = RemoveWhitespace($"긴 글. {body}");
        var actual = RemoveWhitespace(string.Concat(result));
        Assert.Equal(expected, actual);
    }

    private static string RemoveWhitespace(string value)
        => string.Concat(value.Where(x => !char.IsWhiteSpace(x)));
}
