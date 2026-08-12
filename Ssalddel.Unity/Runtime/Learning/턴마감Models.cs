using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ssalddel.Unity.Learning
{
    public sealed class 턴카드ApiModel
    {
        public string CardStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string CardKindCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string EffectTimingCode { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public string TargetStatCode { get; set; } = string.Empty;
        public int StatDelta { get; set; }
        public string SourceStableId { get; set; } = string.Empty;
        public string RegionKey { get; set; } = string.Empty;
        public DateTimeOffset? AvailableFromGameDate { get; set; }
        public DateTimeOffset? AvailableThroughGameDate { get; set; }
        public string CalendarRevision { get; set; } = string.Empty;
        public string EffectRuleRevision { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public DateTimeOffset? EvidenceCheckedAtUtc { get; set; }
    }

    public sealed class 턴마감ContextApiModel
    {
        public string SessionStableId { get; set; } = string.Empty;
        public int TurnNumber { get; set; }
        public DateTimeOffset GameDate { get; set; }
        public long Revision { get; set; }
        public int PendingTaskCount { get; set; }
        public bool CanCloseTurn { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public 턴카드ApiModel[] AvailableCards { get; set; } = Array.Empty<턴카드ApiModel>();
    }

    public sealed class 턴마감PreviewRequestApiModel
    {
        public long ExpectedRevision { get; set; }
        public string[] SelectedCardStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 턴마감PreviewApiModel
    {
        public string PreviewStableId { get; set; } = string.Empty;
        public long BaseRevision { get; set; }
        public int ClosingTurnNumber { get; set; }
        public DateTimeOffset ClosingGameDate { get; set; }
        public int NextTurnNumber { get; set; }
        public DateTimeOffset NextGameDate { get; set; }
        public int PendingTaskCount { get; set; }
        public 턴카드ApiModel[] SelectedCards { get; set; } = Array.Empty<턴카드ApiModel>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class 턴마감ConfirmRequestApiModel
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public 턴마감PreviewRequestApiModel Preview { get; set; } = new 턴마감PreviewRequestApiModel();
    }

    public sealed class 활성턴카드EffectApiModel
    {
        public string CardStableId { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public int ActiveTurnNumber { get; set; }
        public string SourceTurnClosingStableId { get; set; } = string.Empty;
    }

    public sealed class 턴마감SessionApiModel
    {
        public string SessionStableId { get; set; } = string.Empty;
        public int CurrentTick { get; set; }
        public long Revision { get; set; }
        public 활성턴카드EffectApiModel[] ActiveTurnCardEffects { get; set; }
            = Array.Empty<활성턴카드EffectApiModel>();
    }

    public interface I턴마감AuthorityClient
    {
        Task<턴마감ContextApiModel> GetContextAsync(
            string sessionStableId,
            CancellationToken cancellationToken);
        Task<턴마감PreviewApiModel> PreviewAsync(
            string sessionStableId,
            턴마감PreviewRequestApiModel request,
            CancellationToken cancellationToken);
        Task<턴마감SessionApiModel> ConfirmAsync(
            string sessionStableId,
            턴마감ConfirmRequestApiModel request,
            CancellationToken cancellationToken);
    }

    public sealed class 턴마감Coordinator
    {
        private readonly I턴마감AuthorityClient client;
        private string sessionStableId = string.Empty;
        private string selectedCardStableId = string.Empty;

        public 턴마감Coordinator(I턴마감AuthorityClient client)
            => this.client = client ?? throw new ArgumentNullException(nameof(client));

        public 턴마감ContextApiModel? CurrentContext { get; private set; }
        public 턴마감PreviewApiModel? CurrentPreview { get; private set; }
        public 턴마감SessionApiModel? CurrentSession { get; private set; }
        public string SelectedCardStableId => selectedCardStableId;

        public async Task<턴마감ContextApiModel> LoadAsync(
            string requestedSessionStableId,
            CancellationToken cancellationToken = default)
        {
            RequiredId(requestedSessionStableId, "TurnClosingSessionStableIdInvalid");
            var context = await client.GetContextAsync(
                requestedSessionStableId.Trim(), cancellationToken).ConfigureAwait(false);
            ValidateContext(context, requestedSessionStableId.Trim());
            sessionStableId = requestedSessionStableId.Trim();
            selectedCardStableId = string.Empty;
            CurrentPreview = null;
            CurrentContext = context;
            return context;
        }

        public void SelectCard(string? cardStableId)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(cardStableId))
            {
                selectedCardStableId = string.Empty;
                CurrentPreview = null;
                return;
            }
            RequiredId(cardStableId, "TurnClosingCardStableIdInvalid");
            if (!CurrentContext!.AvailableCards.Any(value =>
                    value.CardStableId == cardStableId.Trim()))
                throw new InvalidOperationException("TurnClosingCardUnavailable");
            selectedCardStableId = cardStableId.Trim();
            CurrentPreview = null;
        }

        public async Task<턴마감PreviewApiModel> PreviewAsync(
            CancellationToken cancellationToken = default)
        {
            EnsureLoaded();
            var request = CreatePreviewRequest();
            var preview = await client.PreviewAsync(
                sessionStableId, request, cancellationToken).ConfigureAwait(false);
            ValidatePreview(preview, request);
            CurrentPreview = preview;
            return preview;
        }

        public async Task<턴마감SessionApiModel> ConfirmAsync(
            string commandId,
            CancellationToken cancellationToken = default)
        {
            EnsureLoaded();
            RequiredId(commandId, "TurnClosingCommandIdInvalid");
            if (CurrentPreview == null)
                throw new InvalidOperationException("TurnClosingPreviewRequired");
            var previewRequest = CreatePreviewRequest();
            if (CurrentPreview.BaseRevision != CurrentContext!.Revision)
                throw new InvalidOperationException("TurnClosingPreviewStale");
            var result = await client.ConfirmAsync(
                sessionStableId,
                new 턴마감ConfirmRequestApiModel
                {
                    CommandId = commandId.Trim(),
                    ExpectedRevision = CurrentContext.Revision,
                    Preview = previewRequest,
                },
                cancellationToken).ConfigureAwait(false);
            ValidateResult(result, CurrentPreview, selectedCardStableId);
            CurrentSession = result;
            CurrentPreview = null;
            return result;
        }

        private 턴마감PreviewRequestApiModel CreatePreviewRequest()
            => new 턴마감PreviewRequestApiModel
            {
                ExpectedRevision = CurrentContext!.Revision,
                SelectedCardStableIds = string.IsNullOrEmpty(selectedCardStableId)
                    ? Array.Empty<string>()
                    : new[] { selectedCardStableId },
            };

        private void EnsureLoaded()
        {
            if (CurrentContext == null || string.IsNullOrEmpty(sessionStableId))
                throw new InvalidOperationException("TurnClosingContextRequired");
            if (!CurrentContext.CanCloseTurn || CurrentContext.BlockReasonCodes.Length > 0)
                throw new InvalidOperationException("TurnClosingBlocked");
        }

        private static void ValidateContext(턴마감ContextApiModel value, string expectedSession)
        {
            if (value == null || value.SessionStableId != expectedSession
                || value.TurnNumber <= 0 || value.Revision < 0
                || value.PendingTaskCount < 0 || value.AvailableCards == null
                || value.BlockReasonCodes == null)
                throw new InvalidOperationException("TurnClosingContextInvalid");
            foreach (var card in value.AvailableCards)
                ValidateCard(card);
            if (value.AvailableCards.Select(card => card.CardStableId).Distinct().Count()
                != value.AvailableCards.Length)
                throw new InvalidOperationException("TurnClosingCardDuplicate");
        }

        private static void ValidatePreview(
            턴마감PreviewApiModel value,
            턴마감PreviewRequestApiModel request)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.PreviewStableId)
                || value.BaseRevision != request.ExpectedRevision
                || value.ClosingTurnNumber <= 0
                || value.NextTurnNumber != value.ClosingTurnNumber + 1
                || value.SelectedCards == null || value.BlockReasonCodes == null
                || value.BlockReasonCodes.Length > 0
                || value.SelectedCards.Length != request.SelectedCardStableIds.Length)
                throw new InvalidOperationException("TurnClosingPreviewInvalid");
            for (var index = 0; index < value.SelectedCards.Length; index++)
            {
                ValidateCard(value.SelectedCards[index]);
                if (value.SelectedCards[index].CardStableId != request.SelectedCardStableIds[index])
                    throw new InvalidOperationException("TurnClosingPreviewCardMismatch");
            }
        }

        private static void ValidateResult(
            턴마감SessionApiModel value,
            턴마감PreviewApiModel preview,
            string selectedCard)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.SessionStableId)
                || value.CurrentTick != preview.ClosingTurnNumber
                || value.Revision != preview.BaseRevision + 1
                || value.ActiveTurnCardEffects == null)
                throw new InvalidOperationException("TurnClosingResultInvalid");
            if (string.IsNullOrEmpty(selectedCard))
            {
                if (value.ActiveTurnCardEffects.Length != 0)
                    throw new InvalidOperationException("TurnClosingUnexpectedCardEffect");
                return;
            }
            if (value.ActiveTurnCardEffects.Length != 1
                || value.ActiveTurnCardEffects[0].CardStableId != selectedCard
                || value.ActiveTurnCardEffects[0].ActiveTurnNumber != preview.NextTurnNumber
                || value.ActiveTurnCardEffects[0].SourceTurnClosingStableId != preview.PreviewStableId)
                throw new InvalidOperationException("TurnClosingCardEffectMismatch");
        }

        private static void ValidateCard(턴카드ApiModel card)
        {
            if (card == null) throw new InvalidOperationException("TurnClosingCardInvalid");
            RequiredId(card.CardStableId, "TurnClosingCardInvalid");
            if (string.IsNullOrWhiteSpace(card.CardRevision)
                || string.IsNullOrWhiteSpace(card.CardKindCode)
                || string.IsNullOrWhiteSpace(card.Title)
                || card.EffectTimingCode != "NextTurn"
                || string.IsNullOrWhiteSpace(card.EffectCode)
                || string.IsNullOrWhiteSpace(card.TargetStatCode)
                || card.StatDelta <= 0)
                throw new InvalidOperationException("TurnClosingCardInvalid");
            RequiredId(card.SourceStableId, "TurnClosingCardInvalid");
            if (card.CardKindCode == "Culture"
                && (string.IsNullOrWhiteSpace(card.RegionKey)
                    || !card.AvailableFromGameDate.HasValue
                    || !card.AvailableThroughGameDate.HasValue
                    || card.AvailableFromGameDate > card.AvailableThroughGameDate
                    || string.IsNullOrWhiteSpace(card.CalendarRevision)
                    || string.IsNullOrWhiteSpace(card.EffectRuleRevision)
                    || !Uri.TryCreate(card.SourceUrl, UriKind.Absolute, out var sourceUrl)
                    || sourceUrl.Scheme != Uri.UriSchemeHttps
                    || !card.EvidenceCheckedAtUtc.HasValue))
                throw new InvalidOperationException("TurnClosingCultureCardProvenanceInvalid");
        }

        private static void RequiredId(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new InvalidOperationException(error);
        }
    }
}
