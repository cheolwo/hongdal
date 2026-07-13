using Hongdal.Services.Community;

namespace Hongdal.Tests.Services.Community;

public sealed class 커뮤니티게시글음성길이정책Tests
{
    [Theory]
    [InlineData(99, false)]
    [InlineData(100, true)]
    [InlineData(499, true)]
    [InlineData(500, false)]
    public void 판정_100자이상_500자미만만_음성화대상이다(int 글자수, bool expected)
    {
        var result = 커뮤니티게시글음성길이정책.판정(글자수, 100, 500);

        Assert.Equal(expected, result.음성화대상);
        Assert.Equal(100, result.최소글자수);
        Assert.Equal(500, result.최대글자수미만);
    }
}
