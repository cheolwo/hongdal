using Ssalddel.Unity.Learning;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class EveningHakdangSimulationTests
{
    private readonly 저녁학당SimulationValidator validator = new();

    [Fact]
    public void EVENING1_바보콘텐츠는학당근거와모를뿐질문을보존한다()
    {
        var snapshot = 저녁학당SimulationFixture.CreateFoolEvening();
        validator.Validate(snapshot);

        var content = snapshot.AvailableContents.Single(value =>
            value.StableId == 저녁학당SimulationFixture.FoolContentStableId);
        Assert.Equal("0. 바보 · 모를 뿐", content.Title);
        Assert.Contains("무분별한 마음", content.TeachingSummary);
        Assert.Equal("지금 나는 무엇을 모르는가?", content.ReflectionPrompt);
        Assert.Equal("qo1tNkwSBVs", content.SourceVideoId);
        Assert.Equal(5339, content.SourceStartSeconds);
    }

    [Fact]
    public void EVENING1_PreviewConfirm은원본과정착경제를바꾸지않는다()
    {
        var source = 저녁학당SimulationFixture.CreateFoolEvening();
        var engine = Engine();

        var preview = engine.Preview(source, 저녁학당SimulationFixture.FoolContentStableId);
        var command = engine.Confirm(source, preview, "내일의 판로 결과를 아직 모른다.");

        Assert.Equal(0, source.InnerState.알아차림);
        Assert.Empty(source.InnerState.ActiveRuleCodes);
        Assert.Empty(source.StudyLedger);
        Assert.Equal(내면StatCodes.Awareness, preview.TargetStatCode);
        Assert.Equal(1, preview.StatAfter);
        Assert.True(preview.RevealsUnknownsInNextDayPreviews);
        Assert.True(preview.RequiresExplicitConfirmation);
        Assert.Equal(source.DataRevision, command.ExpectedDataRevision);
        Assert.DoesNotContain(source.GetType().GetProperties(), property =>
            property.Name.Contains("Treasury", StringComparison.Ordinal)
            || property.Name.Contains("Storage", StringComparison.Ordinal)
            || property.Name.Contains("Labor", StringComparison.Ordinal));
    }

    [Fact]
    public void EVENING1_Tick은다음날알아차림과BeginnerMind를활성화한다()
    {
        var source = 저녁학당SimulationFixture.CreateFoolEvening();
        var engine = Engine();
        var preview = engine.Preview(source, 저녁학당SimulationFixture.FoolContentStableId);
        var command = engine.Confirm(source, preview, "감자 검사 결과와 손실 원인을 아직 모른다.");

        var next = engine.Tick(source, command);

        Assert.Equal(source.DataRevision + 1, next.DataRevision);
        Assert.Equal(source.SimulationDate.AddDays(1), next.SimulationDate);
        Assert.Equal(하루단계Codes.Day, next.DayPhaseCode);
        Assert.Equal(1, next.InnerState.알아차림);
        Assert.Contains(내면규칙Codes.BeginnerMind, next.InnerState.ActiveRuleCodes);
        var record = Assert.Single(next.StudyLedger);
        Assert.Equal(내면StatCodes.Awareness, record.StatCode);
        Assert.Equal(1, record.StatDelta);
        Assert.Equal(command.ReflectionText, record.ReflectionText);
    }

    [Fact]
    public void EVENING1_낮시간과빈성찰은확정을거부한다()
    {
        var source = 저녁학당SimulationFixture.CreateFoolEvening();
        var engine = Engine();
        var preview = engine.Preview(source, 저녁학당SimulationFixture.FoolContentStableId);

        Assert.Equal("EveningStudyReflectionRequired",
            Assert.Throws<InvalidOperationException>(() => engine.Confirm(source, preview, " ")).Message);
        source.DayPhaseCode = 하루단계Codes.Day;
        Assert.Equal("EveningStudyNotAvailable",
            Assert.Throws<InvalidOperationException>(() => engine.Preview(
                source, 저녁학당SimulationFixture.FoolContentStableId)).Message);
    }

    [Fact]
    public void EVENING1_StalePreview와Command를거부한다()
    {
        var source = 저녁학당SimulationFixture.CreateFoolEvening();
        var engine = Engine();
        var preview = engine.Preview(source, 저녁학당SimulationFixture.FoolContentStableId);
        source.DataRevision++;

        Assert.Equal("EveningStudyPreviewStaleOrInvalid",
            Assert.Throws<InvalidOperationException>(() => engine.Confirm(source, preview, "모른다.")).Message);

        source = 저녁학당SimulationFixture.CreateFoolEvening();
        preview = engine.Preview(source, 저녁학당SimulationFixture.FoolContentStableId);
        var command = engine.Confirm(source, preview, "모른다.");
        command.ExpectedDataRevision++;
        Assert.Equal("EveningStudyCommandStaleOrInvalid",
            Assert.Throws<InvalidOperationException>(() => engine.Tick(source, command)).Message);
    }

    [Fact]
    public void EVENING1_같은날깊은학습은한번만허용한다()
    {
        var source = 저녁학당SimulationFixture.CreateFoolEvening();
        source.StudyLedger = new[]
        {
            new 저녁학당학습기록Data
            {
                StableId = "evening-study-record:fool.20260407",
                Revision = 1,
                StudiedOn = source.SimulationDate,
                ContentStableId = 저녁학당SimulationFixture.FoolContentStableId,
                ReflectionText = "이미 공부했다.",
                GrantedRuleCode = 내면규칙Codes.BeginnerMind,
                StatCode = 내면StatCodes.Awareness,
                StatDelta = 1,
                SourceStableIds = new[] { 저녁학당SimulationFixture.FoolContentStableId },
            },
        };

        Assert.Equal("EveningStudyAlreadyCompletedToday",
            Assert.Throws<InvalidOperationException>(() => Engine().Preview(
                source, 저녁학당SimulationFixture.FoolContentStableId)).Message);
    }

    private 저녁학당SimulationEngine Engine() => new(validator);
}
