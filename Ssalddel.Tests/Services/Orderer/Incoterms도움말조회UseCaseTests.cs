using Ssalddel.Contracts.Common.Localization;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class Incoterms도움말조회UseCaseTests
{
    private readonly Incoterms도움말조회UseCase _useCase = new();

    [Fact]
    public void Cif는_비용과위험의이전구간을서로다르게표현한다()
    {
        var response = _useCase.조회("cif", DisplayLanguageCodes.Korean);
        var cif = response.항목목록.Single(item => item.코드 == 같이수입준비Incoterms코드.Cif);
        var mainCarriage = cif.그림구간목록.Single(
            section => section.구간코드 == Incoterms도움말구간코드.주운송);

        Assert.Equal(같이수입준비Incoterms코드.Cif, response.선택코드);
        Assert.Equal(Incoterms도움말역할코드.판매자, mainCarriage.비용부담역할코드);
        Assert.Equal(Incoterms도움말역할코드.구매자, mainCarriage.위험부담역할코드);
        Assert.True(cif.판매자보험부보여부);
        Assert.Contains("도착항", cif.비용이전설명, StringComparison.Ordinal);
        Assert.Contains("출발항", cif.위험이전설명, StringComparison.Ordinal);
    }

    [Fact]
    public void Fob와Cif는해상전용이고_Ddp는모든운송방식이다()
    {
        var response = _useCase.조회("FOB", DisplayLanguageCodes.English);

        Assert.Contains(
            "Sea",
            response.항목목록.Single(item => item.코드 == 같이수입준비Incoterms코드.Fob).적용운송범위,
            StringComparison.Ordinal);
        Assert.Contains(
            "Sea",
            response.항목목록.Single(item => item.코드 == 같이수입준비Incoterms코드.Cif).적용운송범위,
            StringComparison.Ordinal);
        Assert.Contains(
            "Any",
            response.항목목록.Single(item => item.코드 == 같이수입준비Incoterms코드.Ddp).적용운송범위,
            StringComparison.Ordinal);
    }

    [Fact]
    public void 지원하지않는코드는_명시적으로거부한다()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => _useCase.조회("EXW", DisplayLanguageCodes.Korean));

        Assert.Contains("FOB, CIF, DDP", exception.Message, StringComparison.Ordinal);
    }
}
