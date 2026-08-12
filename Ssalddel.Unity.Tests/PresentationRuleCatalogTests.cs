using Ssalddel.Unity.PresentationContracts;

namespace Ssalddel.Unity.Tests;

public sealed class PresentationRuleCatalogTests
{
    private readonly 표현규칙Validator validator = new();

    [Fact]
    public void 통합대장은_그래픽_카메라_애니메이션_조명_오디오_UI여섯영역을포함한다()
    {
        var catalog = 통합표현규칙CatalogFixture.Create();

        validator.Validate(catalog);

        Assert.Equal(표현규칙영역Codes.All.OrderBy(value => value),
            catalog.Rules.Select(value => value.DomainCode).Distinct().OrderBy(value => value));
    }

    [Fact]
    public void 기존아홉개시각규칙개정번호를변경하지않고분류한다()
    {
        var catalog = 통합표현규칙CatalogFixture.Create();
        var revisions = catalog.Rules
            .Where(value => value.ImplementationStateCode
                == 표현규칙구현상태Codes.ExistingRuleMapped)
            .Select(value => value.LegacyVisualRuleRevision)
            .ToArray();

        Assert.Equal(9, revisions.Length);
        Assert.Contains("community-square-visual-v1", revisions);
        Assert.Contains("public-marker-visual-v1", revisions);
        Assert.Contains("public-data-surface-visual-v1", revisions);
        Assert.Contains("warehouse-primitive-visual-v1", revisions);
        Assert.Contains("urban-market-manager-primitive-visual.v2", revisions);
        Assert.Contains("role-emphasis-visual-v1", revisions);
        Assert.Contains("concept-card-visual-v1", revisions);
        Assert.Contains("npc-movement-visual-v1", revisions);
        Assert.Contains("transport-corridor-visual-v1", revisions);
    }

    [Fact]
    public void 카메라_조명_오디오는계약준비상태이며구현완료로표시하지않는다()
    {
        var catalog = 통합표현규칙CatalogFixture.Create();

        foreach (var domain in new[]
        {
            표현규칙영역Codes.Camera,
            표현규칙영역Codes.Lighting,
            표현규칙영역Codes.Audio,
        })
        {
            var rule = Assert.Single(catalog.Rules, value => value.DomainCode == domain);
            Assert.Equal(표현규칙구현상태Codes.ContractPrepared, rule.ImplementationStateCode);
            Assert.Null(rule.LegacyVisualRuleRevision);
        }
    }

    [Fact]
    public void 모든표현규칙은기준원장을변경하거나업무완료를확정하지않는다()
    {
        var catalog = 통합표현규칙CatalogFixture.Create();

        Assert.All(catalog.Rules, value =>
        {
            Assert.False(value.MutatesCanonicalState);
            Assert.False(value.ConfirmsBusinessCompletion);
        });
    }

    [Fact]
    public void 업무규칙영역은표현대장에등록할수없다()
    {
        var rule = ValidRule();
        rule.DomainCode = "Transport";

        var error = Assert.Throws<InvalidOperationException>(() => validator.Validate(rule));

        Assert.Equal("PresentationRuleDomainInvalid", error.Message);
    }

    [Fact]
    public void 기준원장변경을표시한표현규칙은차단한다()
    {
        var rule = ValidRule();
        rule.MutatesCanonicalState = true;

        var error = Assert.Throws<InvalidOperationException>(() => validator.Validate(rule));

        Assert.Equal("PresentationRuleCanonicalMutationForbidden", error.Message);
    }

    [Fact]
    public void 업무완료확정을표시한표현규칙은차단한다()
    {
        var rule = ValidRule();
        rule.ConfirmsBusinessCompletion = true;

        var error = Assert.Throws<InvalidOperationException>(() => validator.Validate(rule));

        Assert.Equal("PresentationRuleBusinessCompletionForbidden", error.Message);
    }

    [Fact]
    public void 카메라영역은Material출력채널을가질수없다()
    {
        var rule = ValidRule();
        rule.DomainCode = 표현규칙영역Codes.Camera;
        rule.OutputChannelCodes = new[] { 표현출력채널Codes.Material };

        var error = Assert.Throws<InvalidOperationException>(() => validator.Validate(rule));

        Assert.Equal("PresentationRuleOutputChannelNotAllowed", error.Message);
    }

    [Fact]
    public void 기존시각규칙개정번호로대장항목을찾을수있다()
    {
        var catalog = new 표현규칙Catalog(통합표현규칙CatalogFixture.Create());

        var rule = catalog.ResolveLegacyVisualRule("warehouse-primitive-visual-v1");

        Assert.Equal(표현규칙영역Codes.Graphics, rule.DomainCode);
        Assert.Equal("presentation-rule:graphics.warehouse.v1", rule.RuleStableId);
    }

    private static 표현규칙Descriptor ValidRule()
        => new()
        {
            RuleStableId = "presentation-rule:graphics.test.v1",
            Revision = 1,
            DomainCode = 표현규칙영역Codes.Graphics,
            ImplementationStateCode = 표현규칙구현상태Codes.ExistingRuleMapped,
            LegacyVisualRuleRevision = "test-visual-v1",
            PresentationContractVersion = "test-presentation-v1",
            InputPresentationStateCodes = new[] { "AuthorizedPresentationModel" },
            OutputChannelCodes = new[] { 표현출력채널Codes.Material },
            AppliesToVisualKeys = new[] { "visual-key:test" },
            SourceStableIds = new[] { "source:test" },
            Limitations = new[] { "테스트 표현 규칙이다." },
        };
}
