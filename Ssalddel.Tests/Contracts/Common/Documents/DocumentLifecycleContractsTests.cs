using Ssalddel.Contracts.Common.Documents;

namespace Ssalddel.Tests.Contracts.Common.Documents;

public sealed class DocumentLifecycleContractsTests
{
    [Theory]
    [InlineData(문서생명주기상태코드.초안, 문서생명주기상태코드.검토준비)]
    [InlineData(문서생명주기상태코드.검토준비, 문서생명주기상태코드.확인완료)]
    [InlineData(문서생명주기상태코드.발행완료, 문서생명주기상태코드.전달완료)]
    [InlineData(문서생명주기상태코드.수령확인, 문서생명주기상태코드.보관)]
    public void 정상_생명주기_전이를_허용한다(string current, string target)
    {
        Assert.True(문서생명주기Planner.전이가능한가(current, target));
    }

    [Theory]
    [InlineData(문서생명주기상태코드.발행완료, 문서생명주기상태코드.초안)]
    [InlineData(문서생명주기상태코드.대체됨, 문서생명주기상태코드.발행완료)]
    [InlineData(문서생명주기상태코드.폐기, 문서생명주기상태코드.보관)]
    public void 불변_또는_종료_상태의_역행을_막는다(string current, string target)
    {
        Assert.False(문서생명주기Planner.전이가능한가(current, target));
    }

    [Theory]
    [InlineData(원장관행문서종류코드.같이주문집계표, 문서분류코드.업무작업지)]
    [InlineData(원장관행문서종류코드.계약검토자료서, 문서분류코드.당사자합의)]
    [InlineData(원장관행문서종류코드.상업송장, 문서분류코드.거래명세)]
    [InlineData(원장관행문서종류코드.수입통관서류점검표, 문서분류코드.신고준비)]
    public void 원장_관행_문서를_공통_분류로_정렬한다(string sourceDocumentKind, string expected)
    {
        Assert.Equal(expected, 문서분류Resolver.Resolve("원장관행문서초안", sourceDocumentKind));
    }

    [Fact]
    public void 운송_인수증은_수행증빙으로_분류한다()
    {
        Assert.Equal(문서분류코드.수행증빙, 문서분류Resolver.Resolve("인수증"));
    }

    [Fact]
    public void 같은_숫자라도_원장종류가_다르면_서로_다른_StableId다()
    {
        var inventory = 문서StableId.만들기(문서StableId종류코드.입고상품, 41);
        var outbound = 문서StableId.만들기(문서StableId종류코드.출고예정, 41);

        Assert.NotEqual(inventory, outbound);
        Assert.True(문서StableId.분석(outbound, out var kind, out var value));
        Assert.Equal(문서StableId종류코드.출고예정, kind);
        Assert.Equal("41", value);
        Assert.True(
            문서StableId.흐름순서(문서StableId.만들기(문서StableId종류코드.주문참조, "ORDER-1"))
            < 문서StableId.흐름순서(outbound));
    }
}
