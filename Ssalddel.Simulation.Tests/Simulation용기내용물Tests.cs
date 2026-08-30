using System.Globalization;
using System.Numerics;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "HB01 내용물 전송 순수 계산의 보존·거부·불변 사본을 검증한다.",
    Boundary = "집중 규칙 시험만이며 WI 실행·Command·행위·성장·Session·Save·화면 증거가 아니다.",
    WorldInteractionIds = new[] { "WI-ACTOR-CONSUME", "WI-CRAFT-BREW" })]
public sealed class Simulation용기내용물Tests
{
    private static Simulation용기내용물Snapshot 차(string id = "pot", long 양 = 1000,
        long 최대 = 1000, long 온도 = 80000, string 처방 = "recipe:first", string 판본 = "effect:r1",
        Simulation내용물출처Snapshot[]? 출처 = null, bool 보관 = true)
        => new(id, 보관, 최대, 양, "Tea", 처방, 판본, 온도,
            출처 ?? new[] { new Simulation내용물출처Snapshot("batch:a", 양) });
    private static Simulation용기내용물Snapshot 빈용기(string id = "cup", long 최대 = 200, bool 보관 = true)
        => new(id, 보관, 최대, 0, "", "", "", 0, Array.Empty<Simulation내용물출처Snapshot>());
    private static Simulation내용물전송Result 전송(Simulation용기내용물Snapshot a, Simulation용기내용물Snapshot b, long 양 = 200)
        => Simulation용기내용물Calculator.전송(a, b, 양);
    private static void 거부(string 코드, Action 실행)
        => Assert.Equal(코드, Assert.Throws<SimulationContractException>(실행).Message);
    private static Dictionary<string, BigInteger> 계보합(params Simulation용기내용물Snapshot[] 상태)
        => 상태.SelectMany(x => x.제조출처).GroupBy(x => x.제조출처StableId)
            .ToDictionary(x => x.Key, x => x.Aggregate(BigInteger.Zero, (s, v) => s + v.양Ml));

    [Fact]
    public void 첫시험값은_최대량과음용규칙을합치지않는다()
    {
        Assert.Equal(1000, Simulation용기내용물Codes.냄비시험최대량Ml);
        Assert.Equal(500, Simulation용기내용물Codes.병시험최대량Ml);
        Assert.Equal(200, Simulation용기내용물Codes.컵시험최대량Ml);
        var 결과 = 전송(차(), 빈용기());
        Assert.Equal(800, 결과.원천.현재량Ml); Assert.Equal(200, 결과.대상.현재량Ml);
        Assert.Equal("recipe:first", 결과.대상.처방StableId); Assert.Equal("effect:r1", 결과.대상.효과Revision);
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(long.MinValue)]
    public void 양수아닌요청은거부한다(long 양)
        => 거부("ContentsTransferQuantityInvalid", () => 전송(차(), 빈용기(), 양));

    [Theory]
    [InlineData(1001)] [InlineData(long.MaxValue)]
    public void 원천보다많은요청은거부한다(long 양)
        => 거부("ContentsSourceInsufficient", () => 전송(차(), 빈용기(), 양));

    [Fact] public void 동일용기로전송할수없다()
        => 거부("ContentsSameContainer", () => 전송(차(), 빈용기("pot")));
    [Fact] public void 가득찬대상은거부한다()
        => 거부("ContentsTargetFull", () => 전송(차(), 차("cup", 200, 200)));
    [Fact] public void 물보관불가대상은거부한다()
        => 거부("ContentsStorageNotSupported", () => 전송(차(), 빈용기(보관: false)));
    [Fact] public void 물보관불가원천은거부한다()
        => 거부("ContentsStorageNotSupported", () => 전송(차(보관: false), 빈용기()));
    [Fact] public void 빈원천은거부한다()
        => 거부("ContentsSourceInsufficient", () => 전송(빈용기("pot"), 빈용기()));

    [Theory]
    [InlineData("other", "effect:r1")] [InlineData("recipe:first", "effect:r2")]
    public void 처방또는효과판본이다르면거부한다(string 처방, string 판본)
        => 거부("ContentsProfileMismatch", () => 전송(차(), 차("cup", 100, 200, 처방: 처방, 판본: 판본)));

    [Fact]
    public void 넘치는요청은빈용량만이동하고나머지원천을보존한다()
    {
        var 결과 = 전송(차(), 차("cup", 190, 200, 20000), 500);
        Assert.Equal(10, 결과.이동량Ml); Assert.Equal(990, 결과.원천.현재량Ml);
        Assert.Equal(200, 결과.대상.현재량Ml); Assert.Equal(23000, 결과.대상.온도MilliCelsius);
    }

    [Fact]
    public void 전량이동후에도빈용기와능력은남는다()
    {
        var 결과 = 전송(차(양: 100), 빈용기(), 100);
        Assert.Equal("pot", 결과.원천.용기StableId); Assert.True(결과.원천.물보관가능);
        Assert.Equal(1000, 결과.원천.최대량Ml); Assert.Equal(0, 결과.원천.현재량Ml);
        Assert.Equal("", 결과.원천.종류Code); Assert.Equal("", 결과.원천.처방StableId);
        Assert.Equal("", 결과.원천.효과Revision); Assert.Equal(0, 결과.원천.온도MilliCelsius);
        Assert.Empty(결과.원천.제조출처);
        Simulation용기내용물Calculator.검증(결과.원천);
    }

    [Theory]
    [InlineData(1, 2, 1)] [InlineData(-1, -2, -1)] [InlineData(80000, 20000, 50000)]
    public void 온도는가중평균후정수미만을영쪽으로버린다(long 원천온도, long 대상온도, long 예상)
    {
        var 결과 = 전송(차(양: 100, 온도: 원천온도), 차("cup", 100, 200, 대상온도), 100);
        Assert.Equal(예상, 결과.대상.온도MilliCelsius);
    }

    [Fact]
    public void 물은물로만이동하고차효과를생성하지않는다()
    {
        var 물 = new Simulation용기내용물Snapshot("water", true, 1000, 1000, "Water", "", "", 20000,
            new[] { new Simulation내용물출처Snapshot("source:water", 1000) });
        var 결과 = 전송(물, 빈용기());
        Assert.Equal("Water", 결과.대상.종류Code); Assert.Equal("", 결과.대상.효과Revision);
        거부("ContentsProfileMismatch", () => 전송(물, 차("cup", 100, 200)));
    }

    [Fact]
    public void 제조출처는비례분할하고같은출처는합산한다()
    {
        var 원천 = 차(양: 1000, 출처: new[] { new Simulation내용물출처Snapshot("b", 250), new Simulation내용물출처Snapshot("a", 750) });
        var 대상 = 차("cup", 100, 500, 출처: new[] { new Simulation내용물출처Snapshot("a", 100) });
        var 결과 = 전송(원천, 대상, 200);
        Assert.Equal(new long[] { 600, 200 }, 결과.원천.제조출처.Select(x => x.양Ml));
        Assert.Equal(new long[] { 250, 50 }, 결과.대상.제조출처.Select(x => x.양Ml));
        Assert.Equal(new[] { "a", "b" }, 결과.대상.제조출처.Select(x => x.제조출처StableId));
        Assert.Equal(계보합(원천, 대상), 계보합(결과.원천, 결과.대상));
    }

    [Fact]
    public void 출처배열순서와문화권은나머지배분을바꾸지않는다()
    {
        var 출처 = new[] { new Simulation내용물출처Snapshot("b", 1), new Simulation내용물출처Snapshot("a", 1) };
        var 문화권 = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var 첫 = 전송(차(양: 2, 출처: 출처), 빈용기(), 1);
            Array.Reverse(출처); CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var 둘 = 전송(차(양: 2, 출처: 출처), 빈용기(), 1);
            Assert.Equal("a", Assert.Single(첫.대상.제조출처).제조출처StableId);
            Assert.Equal("a", Assert.Single(둘.대상.제조출처).제조출처StableId);
            Assert.Equal("b", Assert.Single(첫.원천.제조출처).제조출처StableId);
        }
        finally { CultureInfo.CurrentCulture = 문화권; }
    }

    [Fact]
    public void 입력과출력배열은격리되고계산은입력을바꾸지않는다()
    {
        var 배열 = new[] { new Simulation내용물출처Snapshot("a", 1000) };
        var 원천 = 차(출처: 배열); var 대상 = 빈용기();
        배열[0] = new Simulation내용물출처Snapshot("tampered", 1);
        var 결과 = 전송(원천, 대상);
        결과.대상.제조출처[0] = new Simulation내용물출처Snapshot("tampered", 1);
        원천.제조출처[0] = new Simulation내용물출처Snapshot("tampered", 1);
        Assert.Equal("a", 결과.대상.제조출처[0].제조출처StableId);
        Assert.Equal("a", 원천.제조출처[0].제조출처StableId);
        Assert.Equal(1000, 원천.현재량Ml); Assert.Equal(0, 대상.현재량Ml);
        Assert.NotSame(원천, 결과.원천); Assert.NotSame(대상, 결과.대상);
    }

    [Fact]
    public void 최대정수물량과온도의곱셈은넘치지않는다()
    {
        var 원천 = 차(양: long.MaxValue, 최대: long.MaxValue, 온도: long.MaxValue);
        var 결과 = 전송(원천, 빈용기(최대: long.MaxValue), long.MaxValue);
        Assert.Equal(long.MaxValue, 결과.대상.현재량Ml); Assert.Equal(long.MaxValue, 결과.대상.온도MilliCelsius);
        var 최소온도 = 전송(차(양: 1, 온도: long.MinValue), 차("cup", 1, 2, long.MaxValue), 1);
        Assert.Equal(0, 최소온도.대상.온도MilliCelsius);
    }

    [Fact]
    public void 두용기합이long범위를넘어도각용기와출처는보존한다()
    {
        var 원천 = 차(양: long.MaxValue, 최대: long.MaxValue);
        var 대상 = 차("bottle", long.MaxValue - 1, long.MaxValue);
        var 결과 = 전송(원천, 대상, 1);
        Assert.Equal(long.MaxValue, 결과.대상.현재량Ml);
        Assert.Equal(계보합(원천, 대상), 계보합(결과.원천, 결과.대상));
    }

    [Theory]
    [InlineData("null", "ContentsStateRequired")]
    [InlineData("id", "ContentsContainerIdInvalid")]
    [InlineData("id-space", "ContentsContainerIdInvalid")]
    [InlineData("capacity-zero", "ContentsCapacityInvalid")]
    [InlineData("negative", "ContentsCapacityInvalid")]
    [InlineData("overcapacity", "ContentsCapacityInvalid")]
    [InlineData("recipe", "ContentsTeaProfileInvalid")]
    [InlineData("effect", "ContentsTeaProfileInvalid")]
    [InlineData("no-origin", "ContentsProvenanceQuantityMismatch")]
    [InlineData("origin-zero", "ContentsProvenanceInvalid")]
    [InlineData("origin-negative", "ContentsProvenanceInvalid")]
    [InlineData("origin-null", "ContentsProvenanceInvalid")]
    [InlineData("origin-id", "ContentsProvenanceInvalid")]
    [InlineData("origin-duplicate", "ContentsProvenanceInvalid")]
    [InlineData("origin-sum-overflow", "ContentsProvenanceQuantityMismatch")]
    public void 잘못된공개입력은계산전거부한다(string 경우, string 오류)
    {
        var 상태 = 경우 switch
        {
            "null" => null!, "id" => 차(id: null!), "id-space" => 차(id: " pot"),
            "capacity-zero" => 차(최대: 0), "negative" => 차(양: -1), "overcapacity" => 차(최대: 999),
            "recipe" => 차(처방: ""), "effect" => 차(판본: null!),
            "no-origin" => 차(출처: Array.Empty<Simulation내용물출처Snapshot>()),
            "origin-zero" => 차(출처: new[] { new Simulation내용물출처Snapshot("a", 0) }),
            "origin-negative" => 차(출처: new[] { new Simulation내용물출처Snapshot("a", -1) }),
            "origin-null" => 차(출처: new Simulation내용물출처Snapshot[] { null! }),
            "origin-id" => 차(출처: new[] { new Simulation내용물출처Snapshot(" ", 1000) }),
            "origin-duplicate" => 차(출처: new[] { new Simulation내용물출처Snapshot("a", 500), new Simulation내용물출처Snapshot("a", 500) }),
            _ => 차(출처: new[] { new Simulation내용물출처Snapshot("a", long.MaxValue), new Simulation내용물출처Snapshot("b", long.MaxValue) })
        };
        거부(오류, () => 전송(상태, 빈용기()));
    }

    [Theory]
    [InlineData("Tea", "r", "e", 0)] [InlineData("", "", "", 1)] [InlineData(null, "", "", 0)]
    public void 빈용기에남은종류와온도는거부한다(string? 종류, string 처방, string 효과, long 온도)
        => 거부("ContentsEmptyStateInvalid", () => 전송(차(),
            new("cup", true, 200, 0, 종류!, 처방, 효과, 온도, Array.Empty<Simulation내용물출처Snapshot>())));

    [Fact]
    public void 미승인종류와물의효과판본은거부한다()
    {
        var 출처 = new[] { new Simulation내용물출처Snapshot("a", 100) };
        거부("ContentsKindInvalid", () => 전송(new("pot", true, 100, 100, "Potion", "", "", 0, 출처), 빈용기(), 1));
        거부("ContentsWaterProfileInvalid", () => 전송(new("pot", true, 100, 100, "Water", "r", "e", 0, 출처), 빈용기(), 1));
        Assert.Throws<ArgumentNullException>(() => new Simulation용기내용물Snapshot("pot", true, 100, 0, "", "", "", 0, null!));
    }

    [Fact]
    public void 결정적인다양한부분전송에서총량과출처별총량을보존한다()
    {
        for (var i = 1; i <= 200; i++)
        {
            var 원천 = 차(양: 201, 출처: new[] { new Simulation내용물출처Snapshot("a", 100), new Simulation내용물출처Snapshot("b", 101) });
            var 대상 = 빈용기(최대: 500); var 결과 = 전송(원천, 대상, i);
            Assert.Equal(201, 결과.원천.현재량Ml + 결과.대상.현재량Ml);
            Assert.Equal(계보합(원천, 대상), 계보합(결과.원천, 결과.대상));
            Simulation용기내용물Calculator.검증(결과.원천); Simulation용기내용물Calculator.검증(결과.대상);
        }
    }
}
