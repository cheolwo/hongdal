using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Payments;
using 홍달.도메인.결제;

namespace Hongdal.Tests.Services.Community;

public sealed class 노드스티커CatalogTests
{
    [Fact]
    public void 표준은_업로드와_Metadata_규칙을_정의한다()
    {
        var 표준 = 노드스티커Catalog.표준;

        Assert.Equal(512, 표준.원본캔버스크기Px);
        Assert.Contains(48, 표준.표시크기Px옵션);
        Assert.Contains("image/png", 표준.허용MimeTypes);
        Assert.Contains("image/webp", 표준.허용MimeTypes);
        Assert.Contains("원장", 표준.필수MetadataKeys);
        Assert.Contains("노드", 표준.필수MetadataKeys);
        Assert.Contains("라이선스", 표준.필수MetadataKeys);
        Assert.Contains("저작권", 표준.권리정책);
    }

    [Fact]
    public void 기본팩목록은_승인된_표준준수이미지를_제공한다()
    {
        Assert.NotEmpty(노드스티커Catalog.기본팩목록);

        foreach (var 팩 in 노드스티커Catalog.기본팩목록)
        {
            Assert.False(string.IsNullOrWhiteSpace(팩.팩Key));
            Assert.False(string.IsNullOrWhiteSpace(팩.창작자표시명));
            Assert.Equal(노드스티커검수상태.승인, 팩.검수상태);
            Assert.NotEmpty(팩.이미지목록);
            Assert.NotEmpty(팩.스타일Tags);
            Assert.Contains("중간 다리", 팩.거래정책.플랫폼역할);
            Assert.All(팩.이미지목록, 이미지 =>
            {
                Assert.Equal(팩.팩Key, 이미지.팩Key);
                Assert.Equal(노드스티커검수상태.승인, 이미지.검수상태);
                Assert.True(노드스티커Catalog.표준준수여부(이미지));
            });
        }
    }

    [Theory]
    [InlineData(CommunityLedgerTemplateKeys.CargoTransport, "confirm", "상차", "기사", "basic-confirm")]
    [InlineData(CommunityLedgerTemplateKeys.WarehouseOutbound, "work", "피킹/검수", "작업자", "basic-work")]
    [InlineData(CommunityLedgerTemplateKeys.HongdalMart, "warehouse", "도심 재고", "창고", "basic-warehouse")]
    [InlineData(CommunityLedgerTemplateKeys.FoodDelivery, "delivery", "픽업/배달", "운송자", "basic-delivery")]
    public void 노드기본이미지찾기는_노드에_맞는_스티커를_반환한다(
        string 원장템플릿Key,
        string 노드종류,
        string 노드제목,
        string 역할라벨,
        string 기대이미지Key)
    {
        var 이미지 = 노드스티커Catalog.노드기본이미지찾기(new()
        {
            원장템플릿Key = 원장템플릿Key,
            노드종류 = 노드종류,
            노드제목 = 노드제목,
            역할라벨 = 역할라벨,
            상태라벨 = "처리 중"
        });

        Assert.NotNull(이미지);
        Assert.Equal(기대이미지Key, 이미지.이미지Key);
        Assert.StartsWith("data:image/svg+xml;base64,", 이미지.이미지Url);
    }

    [Fact]
    public void 상점정책은_승인되고_판매중인_표준팩만_노출한다()
    {
        var 상품 = 상점상품생성(
            노드스티커검수상태.승인,
            노드스티커판매상태.판매중,
            노드스티커Catalog.기본팩목록[0].이미지목록);

        Assert.True(노드스티커상점정책.상점노출가능한가(상품));

        상품.검수상태 = 노드스티커검수상태.검수대기;

        Assert.False(노드스티커상점정책.상점노출가능한가(상품));
    }

    [Fact]
    public void 유료스티커는_보유권이_있어야_노드에_적용된다()
    {
        var 이미지 = 노드스티커Catalog.이미지찾기("basic-work");
        var 상품 = 상점상품생성(
            노드스티커검수상태.승인,
            노드스티커판매상태.판매중,
            노드스티커Catalog.기본팩목록[0].이미지목록);
        상품.거래정책 = new()
        {
            가격모드 = 노드스티커가격모드.유료,
            가격금액 = 1200,
            통화Code = "KRW"
        };

        var 구매전판정 = 노드스티커상점정책.노드적용판정(이미지, 상품, null);

        Assert.False(구매전판정.적용가능);
        Assert.Equal(노드스티커노드적용판정Codes.구매필요, 구매전판정.판정Code);

        var 보유권 = new 노드스티커보유권Response
        {
            사용자UserId = "user-1",
            팩Key = 이미지!.팩Key,
            이미지Keys = [이미지.이미지Key],
            보유권출처 = 노드스티커보유권출처.구매
        };

        var 구매후판정 = 노드스티커상점정책.노드적용판정(이미지, 상품, 보유권);

        Assert.True(구매후판정.적용가능);
        Assert.Equal(노드스티커노드적용판정Codes.적용가능, 구매후판정.판정Code);
    }

    [Fact]
    public void 상점에는_FakePG_구매흐름을_검증할_유료팩이_있다()
    {
        var 유료팩 = Assert.Single(
            노드스티커Catalog.기본팩목록,
            팩 => string.Equals(팩.거래정책.가격모드, 노드스티커가격모드.유료, StringComparison.OrdinalIgnoreCase));
        var 상품 = new 노드스티커상점상품Response
        {
            상품Key = $"store-{유료팩.팩Key}",
            팩Key = 유료팩.팩Key,
            제목 = 유료팩.제목,
            창작자표시명 = 유료팩.창작자표시명,
            요약 = 유료팩.요약,
            검수상태 = 유료팩.검수상태,
            판매상태 = 노드스티커판매상태.판매중,
            거래정책 = 유료팩.거래정책,
            이미지목록 = 유료팩.이미지목록
        };

        Assert.True(유료팩.거래정책.가격금액 > 0);
        Assert.True(노드스티커상점정책.상점노출가능한가(상품));
        Assert.True(노드스티커상점정책.구매필요한가(상품));
    }

    [Fact]
    public void 노드스티커팩_결제대상유형은_계약과_도메인이_일치한다()
    {
        Assert.Equal(
            계약결제대상유형.노드스티커팩,
            결제공통정의.결제대상유형.노드스티커팩);
    }

    private static 노드스티커상점상품Response 상점상품생성(
        string 검수상태,
        string 판매상태,
        IReadOnlyList<노드스티커이미지Response> 이미지목록)
        => new()
        {
            상품Key = "store-hongdal-basic-work-node-stickers",
            팩Key = "hongdal-basic-work-node-stickers",
            제목 = "홍달 기본 업무 노드 스티커",
            창작자표시명 = "Hongdal",
            요약 = "원장 다이어그램에 바로 붙일 수 있는 기본 업무 노드 이미지 팩입니다.",
            검수상태 = 검수상태,
            판매상태 = 판매상태,
            이미지목록 = 이미지목록
        };
}
