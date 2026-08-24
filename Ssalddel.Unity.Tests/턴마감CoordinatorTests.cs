using Ssalddel.Unity.Learning;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class 턴마감CoordinatorTests
{
    private const string SessionId = "simulation-session:turn-ui-1";
    private const string FoolCard = "learning:hongik.fool.beginner-mind";
    private const string SeoulCultureCard = "culture:kr-seoul.living-culture-question.2026";

    [Fact]
    public async Task TURN_CARD_UI1A_Context선택PreviewConfirm은서버revision과다음턴효과를보존한다()
    {
        var client = new StubClient();
        var coordinator = new 턴마감Coordinator(client);

        var context = await coordinator.LoadAsync(SessionId);
        coordinator.SelectCard(FoolCard);
        var preview = await coordinator.PreviewAsync();
        var result = await coordinator.ConfirmAsync("command:turn.ui-1");

        Assert.Equal(1, context.TurnNumber);
        Assert.Equal(FoolCard, coordinator.SelectedCardStableId);
        Assert.Equal(2, preview.NextTurnNumber);
        Assert.Equal(1, result.CurrentTick);
        Assert.Equal(11, result.Revision);
        Assert.Equal(FoolCard, Assert.Single(result.ActiveTurnCardEffects).CardStableId);
        Assert.Null(coordinator.CurrentPreview);
        Assert.Same(result, coordinator.CurrentSession);
    }

    [Fact]
    public async Task TURN_CARD_UI1A_카드없이도Preview와Confirm을허용한다()
    {
        var coordinator = new 턴마감Coordinator(new StubClient());
        await coordinator.LoadAsync(SessionId);
        coordinator.SelectCard(null);

        var preview = await coordinator.PreviewAsync();
        var result = await coordinator.ConfirmAsync("command:turn.ui-no-card");

        Assert.Empty(preview.SelectedCards);
        Assert.Empty(result.ActiveTurnCardEffects);
    }

    [Fact]
    public async Task TURN_CARD_UI1A_미게시카드와Preview없는Confirm을거부한다()
    {
        var coordinator = new 턴마감Coordinator(new StubClient());
        await coordinator.LoadAsync(SessionId);

        Assert.Equal("TurnClosingCardUnavailable",
            Assert.Throws<InvalidOperationException>(() =>
                coordinator.SelectCard("learning:invented.card")).Message);
        Assert.Equal("TurnClosingPreviewRequired",
            (await Assert.ThrowsAsync<InvalidOperationException>(() =>
                coordinator.ConfirmAsync("command:turn.no-preview"))).Message);
    }

    [Fact]
    public async Task TURN_CARD_UI1A_불일치결과는마지막성공Session을교체하지않는다()
    {
        var client = new StubClient();
        var coordinator = new 턴마감Coordinator(client);
        await coordinator.LoadAsync(SessionId);
        await coordinator.PreviewAsync();
        client.ReturnInvalidRevision = true;

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            coordinator.ConfirmAsync("command:turn.bad-result"));

        Assert.Equal("TurnClosingResultInvalid", error.Message);
        Assert.Null(coordinator.CurrentSession);
        Assert.NotNull(coordinator.CurrentPreview);
    }

    [Fact]
    public async Task CULTURE_CARD0_Unity는문화카드의지역기간출처와revision을검증한다()
    {
        var coordinator = new 턴마감Coordinator(new StubClient());

        var context = await coordinator.LoadAsync(SessionId);
        var culture = Assert.Single(context.AvailableCards,
            card => card.CardStableId == SeoulCultureCard);

        Assert.Equal("kr-seoul", culture.RegionKey);
        Assert.Equal("simulation-culture-calendar:kr-seoul:2026.r1", culture.CalendarRevision);
        Assert.Equal("culture-local-context-awareness:r1", culture.EffectRuleRevision);
        Assert.StartsWith("https://www.mcst.go.kr/", culture.SourceUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CULTURE_CARD0_Unity는근거가불완전한문화카드를거부한다()
    {
        var client = new StubClient { ReturnInvalidCulture = true };
        var coordinator = new 턴마감Coordinator(client);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            coordinator.LoadAsync(SessionId));

        Assert.Equal("TurnClosingCultureCardProvenanceInvalid", error.Message);
    }

    private sealed class StubClient : I턴마감AuthorityClient
    {
        public bool ReturnInvalidRevision { get; set; }
        public bool ReturnInvalidCulture { get; set; }

        public Task<턴마감ContextApiModel> GetContextAsync(
            string sessionStableId,
            CancellationToken cancellationToken)
            => Task.FromResult(new 턴마감ContextApiModel
            {
                SessionStableId = sessionStableId,
                TurnNumber = 1,
                GameDate = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
                Revision = 10,
                PendingTaskCount = 2,
                CanCloseTurn = true,
                AvailableCards = new[] { Card(), CultureCard(ReturnInvalidCulture) },
            });

        public Task<턴마감PreviewApiModel> PreviewAsync(
            string sessionStableId,
            턴마감PreviewRequestApiModel request,
            CancellationToken cancellationToken)
            => Task.FromResult(new 턴마감PreviewApiModel
            {
                PreviewStableId = "turn-closing:" + sessionStableId + ":1",
                BaseRevision = request.ExpectedRevision,
                ClosingTurnNumber = 1,
                ClosingGameDate = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
                NextTurnNumber = 2,
                NextGameDate = new DateTimeOffset(2026, 4, 2, 0, 0, 0, TimeSpan.Zero),
                PendingTaskCount = 2,
                SelectedCards = request.SelectedCardStableIds.Length == 0
                    ? Array.Empty<턴카드ApiModel>()
                    : new[] { Card() },
            });

        public Task<턴마감SessionApiModel> ConfirmAsync(
            string sessionStableId,
            턴마감ConfirmRequestApiModel request,
            CancellationToken cancellationToken)
        {
            var hasCard = request.Preview.SelectedCardStableIds.Length == 1;
            return Task.FromResult(new 턴마감SessionApiModel
            {
                SessionStableId = sessionStableId,
                CurrentTick = 1,
                Revision = ReturnInvalidRevision ? 12 : 11,
                ActiveTurnCardEffects = hasCard
                    ? new[]
                    {
                        new 활성턴카드EffectApiModel
                        {
                            CardStableId = FoolCard,
                            EffectCode = "BeginnerMind",
                            ActiveTurnNumber = 2,
                            SourceTurnClosingStableId = "turn-closing:" + sessionStableId + ":1",
                        },
                    }
                    : Array.Empty<활성턴카드EffectApiModel>(),
            });
        }

        private static 턴카드ApiModel Card()
            => new 턴카드ApiModel
            {
                CardStableId = FoolCard,
                CardRevision = "evening-hakdang.fixture-r1",
                CardKindCode = "Philosophy",
                Title = "0. 바보 · 모를 뿐",
                Summary = "초심",
                EffectTimingCode = "NextTurn",
                EffectCode = "BeginnerMind",
                TargetStatCode = "Awareness",
                StatDelta = 1,
                SourceStableId = "source:fixture.evening-hakdang.fool.beginner-mind",
            };

        private static 턴카드ApiModel CultureCard(bool invalid)
            => new 턴카드ApiModel
            {
                CardStableId = SeoulCultureCard,
                CardRevision = "culture-card.fixture-r1",
                CardKindCode = "Culture",
                Title = "서울 생활문화 질문",
                Summary = "현재 경험과 공식 원천을 함께 확인한다.",
                EffectTimingCode = "NextTurn",
                EffectCode = "LocalContextAwareness",
                TargetStatCode = "CommunityInsight",
                StatDelta = 1,
                SourceStableId = "source:kr-regional-culture-promotion-agency",
                RegionKey = "kr-seoul",
                AvailableFromGameDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                AvailableThroughGameDate = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero),
                CalendarRevision = "simulation-culture-calendar:kr-seoul:2026.r1",
                EffectRuleRevision = "culture-local-context-awareness:r1",
                SourceUrl = invalid
                    ? string.Empty
                    : "https://www.mcst.go.kr/site/s_data/corpNaru/corpView.jsp?pSeq=615",
                EvidenceCheckedAtUtc = new DateTimeOffset(2026, 7, 26, 0, 0, 0, TimeSpan.Zero),
            };
    }
}
