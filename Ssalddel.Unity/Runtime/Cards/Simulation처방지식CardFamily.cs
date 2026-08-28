using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ssalddel.Unity.Cards
{
    public static class 처방지식ProjectionCodes
    {
        public const string KnowledgeWorldInteractionId = "WI-ACTOR-03";
        public const string BasicHerbalTeaRecipeStableId =
            "recipe:nature:basic-herbal-tea.v1";
    }

    /// <summary>
    /// Simulation플레이어지식LedgerSnapshot의 Unity transport projection이다.
    /// 서버 계약 어셈블리 참조 없이 동일 wire 속성만 소비한다.
    /// </summary>
    public sealed class 플레이어지식LedgerApiModel
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public string[] KnownRecipeStableIds { get; set; } = Array.Empty<string>();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simulation지식습득PreviewSnapshot의 Unity transport projection이다.
    /// </summary>
    public sealed class 지식습득PreviewApiModel
    {
        public string WorldInteractionId { get; set; }
            = 처방지식ProjectionCodes.KnowledgeWorldInteractionId;
        public long ObservedWorldRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string RecipeStableId { get; set; } = string.Empty;
        public string KnowledgeSourceStableId { get; set; } = string.Empty;
        public bool AlreadyKnown { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public static class 처방지식CardStateCodes
    {
        public const string Readable = "Readable";
        public const string Known = "Known";
        public const string Blocked = "Blocked";
    }

    public sealed class 처방지식CardProjection
    {
        public string RecipeStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string[] KnowledgeSourceStableIds { get; set; }
            = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public CardWorkspaceItem WorkspaceItem { get; set; }
            = new CardWorkspaceItem();
    }

    public sealed class 처방지식CardFamilyProjection
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public 처방지식CardProjection[] Cards { get; set; }
            = Array.Empty<처방지식CardProjection>();
        public CardWorkspaceFamilySnapshot WorkspaceFamily { get; set; }
            = new CardWorkspaceFamilySnapshot();
        public bool PresentationOnly { get; set; }
    }

    /// <summary>
    /// 같은 WorldRevision의 플레이어 지식 원장과 Preview를 처방 지식 카드로
    /// 투영한다. 카드 상태는 표현용이며 Confirm이나 권위 Revision을 변경하지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
        "Simulation 지식 상태 사본을 결정적인 처방 카드 가족으로 검증한다.",
        Boundary = "읽기 전용 투영은 지식 습득 Confirm과 권위 상태 변경을 수행하지 않는다.")]
    public sealed class 처방지식CardFamilyProjector
    {
        public 처방지식CardFamilyProjection Project(
            플레이어지식LedgerApiModel ledger,
            IEnumerable<지식습득PreviewApiModel> previewSnapshots)
        {
            if (ledger == null)
                throw new ArgumentNullException(nameof(ledger));
            if (previewSnapshots == null)
                throw new ArgumentNullException(nameof(previewSnapshots));

            var previews = previewSnapshots.ToArray();
            Validate(ledger, previews);

            var knownRecipeIds = new HashSet<string>(
                ledger.KnownRecipeStableIds, StringComparer.Ordinal);
            var recipeIds = knownRecipeIds
                .Concat(previews.Select(value => value.RecipeStableId))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

            var cards = recipeIds.Select(recipeId => CreateCard(recipeId,
                    knownRecipeIds.Contains(recipeId), previews
                        .Where(value => string.Equals(value.RecipeStableId,
                            recipeId, StringComparison.Ordinal))
                        .OrderBy(value => value.KnowledgeSourceStableId,
                            StringComparer.Ordinal)
                        .ToArray()))
                .ToArray();

            return new 처방지식CardFamilyProjection
            {
                WorldStableId = ledger.WorldStableId,
                PlayerStableId = ledger.PlayerStableId,
                WorldRevision = ledger.WorldRevision,
                Cards = cards,
                WorkspaceFamily = new CardWorkspaceFamilySnapshot
                {
                    FamilyCode = CardFamilyCodes.RecipeKnowledge,
                    Items = cards.Select(value => value.WorkspaceItem).ToArray(),
                    Relations = Array.Empty<CardWorkspaceRelation>(),
                    SourceRevision = ledger.WorldRevision,
                },
                PresentationOnly = true,
            };
        }

        private static 처방지식CardProjection CreateCard(string recipeId,
            bool known, 지식습득PreviewApiModel[] previews)
        {
            var readable = !known && previews.Any(value => value.CanConfirm);
            var stateCode = known
                ? 처방지식CardStateCodes.Known
                : readable
                    ? 처방지식CardStateCodes.Readable
                    : 처방지식CardStateCodes.Blocked;
            var sources = previews.Select(value => value.KnowledgeSourceStableId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var blockReasons = known || readable
                ? Array.Empty<string>()
                : previews.SelectMany(value => value.BlockReasonCodes)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
            var cardStableId = "card:recipe-knowledge:" + recipeId;

            return new 처방지식CardProjection
            {
                RecipeStableId = recipeId,
                StateCode = stateCode,
                KnowledgeSourceStableIds = sources,
                BlockReasonCodes = blockReasons,
                WorkspaceItem = new CardWorkspaceItem
                {
                    CardStableId = cardStableId,
                    CardCopyStableId = cardStableId + ":" + stateCode,
                    Title = ResolveTitle(recipeId),
                    Summary = ResolveSummary(stateCode, blockReasons),
                    FamilyCode = CardFamilyCodes.RecipeKnowledge,
                    HierarchyTierCode = CardHierarchyTierCodes.Knowledge,
                    AuthorityCode = CardAuthorityCodes.ProjectionReadOnly,
                    ActionRouteCode = known || readable
                        ? CardActionRouteCodes.OpenInformation
                        : CardActionRouteCodes.None,
                    SlotCode = "RecipeKnowledge",
                    IsAvailable = known || readable,
                    IsLocked = !known && !readable,
                },
            };
        }

        private static string ResolveTitle(string recipeStableId)
            => string.Equals(recipeStableId,
                처방지식ProjectionCodes.BasicHerbalTeaRecipeStableId,
                StringComparison.Ordinal)
                ? "기초 약초차"
                : recipeStableId;

        private static string ResolveSummary(string stateCode,
            string[] blockReasonCodes)
        {
            if (stateCode == 처방지식CardStateCodes.Known)
                return "습득한 처방 지식";
            if (stateCode == 처방지식CardStateCodes.Readable)
                return "읽고 습득할 수 있는 처방 지식";
            return blockReasonCodes.Length == 0
                ? "현재 읽을 수 없는 처방 지식"
                : "현재 읽을 수 없음: " + string.Join(", ", blockReasonCodes);
        }

        private static void Validate(플레이어지식LedgerApiModel ledger,
            지식습득PreviewApiModel[] previews)
        {
            if (string.IsNullOrWhiteSpace(ledger.WorldStableId)
                || string.IsNullOrWhiteSpace(ledger.SessionStableId)
                || string.IsNullOrWhiteSpace(ledger.PlayerStableId)
                || string.IsNullOrWhiteSpace(ledger.StateHashSha256)
                || ledger.WorldRevision < 0
                || ledger.KnownRecipeStableIds == null
                || ledger.KnownRecipeStableIds.Any(string.IsNullOrWhiteSpace)
                || ledger.KnownRecipeStableIds.Distinct(StringComparer.Ordinal).Count()
                    != ledger.KnownRecipeStableIds.Length)
                throw new InvalidOperationException(
                    "RecipeKnowledgeLedgerSnapshotInvalid");

            if (previews.Any(value => value == null
                    || value.ObservedWorldRevision != ledger.WorldRevision
                    || !string.Equals(value.PlayerStableId, ledger.PlayerStableId,
                        StringComparison.Ordinal)
                    || !string.Equals(value.WorldInteractionId,
                        처방지식ProjectionCodes.KnowledgeWorldInteractionId,
                        StringComparison.Ordinal)
                    || string.IsNullOrWhiteSpace(value.RecipeStableId)
                    || string.IsNullOrWhiteSpace(value.KnowledgeSourceStableId)
                    || value.BlockReasonCodes == null
                    || value.AlreadyKnown != ledger.KnownRecipeStableIds.Contains(
                        value.RecipeStableId, StringComparer.Ordinal)
                    || (value.CanConfirm && (value.AlreadyKnown
                        || value.BlockReasonCodes.Length > 0))
                    || (!value.CanConfirm && !value.AlreadyKnown
                        && value.BlockReasonCodes.Length == 0)))
                throw new InvalidOperationException(
                    "RecipeKnowledgePreviewSnapshotMismatch");
        }
    }

    public sealed class 처방지식CardFamilySource : ICardFamilySource
    {
        private readonly 처방지식CardFamilyProjection projection;

        public 처방지식CardFamilySource(
            처방지식CardFamilyProjection value)
        {
            projection = value ?? throw new ArgumentNullException(nameof(value));
            if (!value.PresentationOnly
                || value.WorkspaceFamily == null
                || value.WorkspaceFamily.FamilyCode != FamilyCode
                || value.WorkspaceFamily.SourceRevision != value.WorldRevision)
                throw new InvalidOperationException(
                    "RecipeKnowledgeCardFamilyProjectionInvalid");
        }

        public string FamilyCode => CardFamilyCodes.RecipeKnowledge;

        public Task<CardWorkspaceFamilySnapshot> LoadAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(projection.WorkspaceFamily);
        }
    }
}
