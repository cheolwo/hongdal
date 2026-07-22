using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.WebApp;

public sealed class 커뮤니티화물글초안가져오기ServiceTests
{
    private readonly 커뮤니티화물글초안가져오기Service _service = new();

    [Theory]
    [InlineData(CommunityBoardKeys.Cargo)]
    [InlineData("화물")]
    [InlineData("화물 운송")]
    public void 화물_게시판의_키와_이름과_기존_별칭을_가져올_수_있다(string category)
    {
        var post = CreatePost(category);

        Assert.True(_service.가져올수있음(post));
    }

    [Fact]
    public void 비어_있는_초안에는_제목과_본문과_출처를_반영한다()
    {
        var target = new 운송의뢰작성ViewModel();
        var post = CreatePost(CommunityBoardKeys.Cargo);

        var result = _service.가져오기(target, post);

        Assert.True(result.변경됨);
        Assert.True(result.화물종류채움);
        Assert.Equal(post.Title, target.화물종류);
        Assert.Contains(post.Title, target.화물설명);
        Assert.Contains(post.Body, target.화물설명);
        Assert.Contains("커뮤니티 화물 글 #42", target.화물설명);
        Assert.Contains("커뮤니티 화물 글 #42", target.절차메모);
    }

    [Fact]
    public void 기존_구조화_입력은_보존하고_게시글_본문만_설명에_덧붙인다()
    {
        var target = new 운송의뢰작성ViewModel
        {
            화물종류 = "기존 화물 종류",
            화물설명 = "기존 화물 설명",
            화물수량 = 7,
            화물중량Kg = 125.5m,
            상차도로명주소 = "기존 상차지",
            하차도로명주소 = "기존 하차지",
            상차연락처전화번호 = "010-1111-2222",
            하차연락처전화번호 = "010-3333-4444",
            차량종류 = "1톤 카고",
            결제예정금액 = 99000,
            요청사항 = "기존 요청 사항",
            절차메모 = "기존 절차 메모"
        };
        var post = CreatePost("화물");

        var result = _service.가져오기(target, post);

        Assert.True(result.변경됨);
        Assert.False(result.화물종류채움);
        Assert.Equal("기존 화물 종류", target.화물종류);
        Assert.StartsWith("기존 화물 설명", target.화물설명, StringComparison.Ordinal);
        Assert.Contains(post.Title, target.화물설명);
        Assert.Contains(post.Body, target.화물설명);
        Assert.Equal(7, target.화물수량);
        Assert.Equal(125.5m, target.화물중량Kg);
        Assert.Equal("기존 상차지", target.상차도로명주소);
        Assert.Equal("기존 하차지", target.하차도로명주소);
        Assert.Equal("010-1111-2222", target.상차연락처전화번호);
        Assert.Equal("010-3333-4444", target.하차연락처전화번호);
        Assert.Equal("1톤 카고", target.차량종류);
        Assert.Equal(99000, target.결제예정금액);
        Assert.Equal("기존 요청 사항", target.요청사항);
        Assert.StartsWith("기존 절차 메모", target.절차메모, StringComparison.Ordinal);
    }

    [Fact]
    public void 같은_글을_다시_가져와도_본문과_출처를_중복하지_않는다()
    {
        var target = new 운송의뢰작성ViewModel();
        var post = CreatePost(CommunityBoardKeys.Cargo);

        _service.가져오기(target, post);
        var second = _service.가져오기(target, post);

        Assert.True(_service.이미반영됨(target, post.Id));
        Assert.False(second.변경됨);
        Assert.Equal(1, CountOccurrences(target.화물설명, "커뮤니티 화물 글 #42"));
        Assert.Equal(1, CountOccurrences(target.절차메모, "커뮤니티 화물 글 #42"));
    }

    [Fact]
    public void 다른_게시판_글은_가져오지_않는다()
    {
        var target = new 운송의뢰작성ViewModel { 화물종류 = "기존 값" };
        var post = CreatePost(CommunityBoardKeys.FreeLife);

        var exception = Assert.Throws<InvalidOperationException>(() => _service.가져오기(target, post));

        Assert.Contains("화물 게시판", exception.Message);
        Assert.Equal("기존 값", target.화물종류);
        Assert.Null(target.화물설명);
        Assert.Null(target.절차메모);
    }

    private static PlatformCommunityPostResponse CreatePost(string category)
        => new()
        {
            Id = 42,
            Category = category,
            Title = "냉장 식자재 운송 문의",
            Body = "박스 화물의 운송 조건을 함께 확인하고 싶습니다."
        };

    private static int CountOccurrences(string? value, string search)
        => string.IsNullOrEmpty(value)
            ? 0
            : value.Split(search, StringSplitOptions.None).Length - 1;
}
