using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ssalddel.Unity.WorldEvents
{
    public static class SimulationWorldEventApiRoutes
    {
        public static string Changes(string sessionStableId, long afterWorldRevision)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new ArgumentException("SimulationSessionStableIdMissing",
                    nameof(sessionStableId));
            if (afterWorldRevision < -1)
                throw new ArgumentOutOfRangeException(nameof(afterWorldRevision));

            return "/api/simulation/v1/sessions/"
                + Uri.EscapeDataString(sessionStableId.Trim())
                + "/world-events?afterWorldRevision=" + afterWorldRevision;
        }
    }

    public sealed class SimulationWorldEventChoiceApiModel
    {
        public string ChoiceStableId { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string CardStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string OrientationCode { get; set; } = string.Empty;
        public string KoreanTitle { get; set; } = string.Empty;
        public string KoreanSummary { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldEventApiModel
    {
        public string EventStableId { get; set; } = string.Empty;
        public long EventRevision { get; set; }
        public long LastChangedWorldRevision { get; set; }
        public string EventTypeCode { get; set; } = string.Empty;
        public string TriggerCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int OccurredWorldTick { get; set; }
        public int VisibleFromWorldTick { get; set; }
        public int? ExpiresAfterWorldTick { get; set; }
        public string AudienceScopeCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
        public string ResponseKindCode { get; set; } = string.Empty;
        public string SourceOpportunityStableId { get; set; } = string.Empty;
        public string ChoiceSetStableId { get; set; } = string.Empty;
        public SimulationWorldEventChoiceApiModel[] Choices { get; set; }
            = Array.Empty<SimulationWorldEventChoiceApiModel>();
        public string SelectedChoiceStableId { get; set; } = string.Empty;
        public string ActiveBuildingStableId { get; set; } = string.Empty;
        public string[] AnchorBuildingStableIds { get; set; } = Array.Empty<string>();
        public string[] TileKeys { get; set; } = Array.Empty<string>();
        public string[] RegionStableIds { get; set; } = Array.Empty<string>();
        public string[] ParticipantPlayerStableIds { get; set; } = Array.Empty<string>();
        public int RespondedParticipantCount { get; set; }
        public int RequiredParticipantCount { get; set; }
        public bool CanRespond { get; set; }
        public bool RequiresUnanimousResponse { get; set; }
        public bool RequiresExpectedRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class SimulationWorldEventProjectionApiModel
    {
        public string SessionStableId { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
        public long AfterWorldRevision { get; set; } = -1;
        public long NextAfterWorldRevision { get; set; }
        public bool HasMore { get; set; }
        public SimulationWorldEventApiModel[] Events { get; set; }
            = Array.Empty<SimulationWorldEventApiModel>();
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    /// <summary>
    /// Unity가 카드·정보판·공간 효과를 선택할 때 사용하는 의미 자료다.
    /// Prefab이나 Material은 PresentationKey를 별도 구성 대장에서 해석한다.
    /// </summary>
    public sealed class 세계사건표현Snapshot
    {
        public string EventStableId { get; set; } = string.Empty;
        public long EventRevision { get; set; }
        public long LastChangedWorldRevision { get; set; }
        public string EventTypeCode { get; set; } = string.Empty;
        public string TriggerCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int OccurredWorldTick { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
        public string ResponseKindCode { get; set; } = string.Empty;
        public string SourceOpportunityStableId { get; set; } = string.Empty;
        public string SelectedChoiceStableId { get; set; } = string.Empty;
        public string ActiveBuildingStableId { get; set; } = string.Empty;
        public string[] AnchorBuildingStableIds { get; set; } = Array.Empty<string>();
        public string[] TileKeys { get; set; } = Array.Empty<string>();
        public string[] RegionStableIds { get; set; } = Array.Empty<string>();
        public string[] ParticipantPlayerStableIds { get; set; } = Array.Empty<string>();
        public int RespondedParticipantCount { get; set; }
        public int RequiredParticipantCount { get; set; }
        public bool CanRespond { get; set; }
        public bool RequiresExpectedRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public 세계사건선택지Snapshot[] Choices { get; set; }
            = Array.Empty<세계사건선택지Snapshot>();
    }

    public sealed class 세계사건선택지Snapshot
    {
        public string ChoiceStableId { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string CardStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string OrientationCode { get; set; } = string.Empty;
        public string KoreanTitle { get; set; } = string.Empty;
        public string KoreanSummary { get; set; } = string.Empty;
    }

    public sealed class 세계사건변경Projection
    {
        public string SessionStableId { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
        public long AfterWorldRevision { get; set; }
        public long NextAfterWorldRevision { get; set; }
        public bool HasMore { get; set; }
        public 세계사건표현Snapshot[] Events { get; set; }
            = Array.Empty<세계사건표현Snapshot>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class SimulationWorldEventProjectionMapper
    {
        public 세계사건변경Projection Map(
            SimulationWorldEventProjectionApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Require(source.SessionStableId, "SimulationSessionStableIdMissing");
            if (source.WorldTick < 0 || source.WorldRevision < 0
                || source.AfterWorldRevision < -1
                || source.NextAfterWorldRevision < source.AfterWorldRevision
                || source.NextAfterWorldRevision > source.WorldRevision)
                throw new InvalidOperationException("WorldEventProjectionRevisionInvalid");
            if (!source.SimulationOnly || source.IsOperationalState
                || !source.PresentationOnly)
                throw new InvalidOperationException("WorldEventProjectionBoundaryInvalid");

            var events = source.Events ?? Array.Empty<SimulationWorldEventApiModel>();
            EnsureUnique(events.Select(value => value.EventStableId),
                "WorldEventStableIdDuplicated");
            return new 세계사건변경Projection
            {
                SessionStableId = source.SessionStableId.Trim(),
                WorldTick = source.WorldTick,
                WorldRevision = source.WorldRevision,
                AfterWorldRevision = source.AfterWorldRevision,
                NextAfterWorldRevision = source.NextAfterWorldRevision,
                HasMore = source.HasMore,
                Events = events.Select(value => MapEvent(value, source)).ToArray(),
            };
        }

        private static 세계사건표현Snapshot MapEvent(
            SimulationWorldEventApiModel value,
            SimulationWorldEventProjectionApiModel projection)
        {
            if (value == null)
                throw new InvalidOperationException("WorldEventMissing");
            Require(value.EventStableId, "WorldEventStableIdMissing");
            Require(value.EventTypeCode, "WorldEventTypeMissing");
            Require(value.StateCode, "WorldEventStateMissing");
            Require(value.PresentationKey, "WorldEventPresentationKeyMissing");
            Require(value.RuleRevision, "WorldEventRuleRevisionMissing");
            if (value.EventRevision <= 0 || value.LastChangedWorldRevision < 0
                || value.LastChangedWorldRevision <= projection.AfterWorldRevision
                || value.LastChangedWorldRevision > projection.WorldRevision
                || value.OccurredWorldTick < 0
                || value.VisibleFromWorldTick > projection.WorldTick)
                throw new InvalidOperationException("WorldEventRevisionInvalid");
            if (!value.SimulationOnly || value.IsOperationalState
                || !value.PresentationOnly)
                throw new InvalidOperationException("WorldEventBoundaryInvalid");
            if (value.RespondedParticipantCount < 0
                || value.RequiredParticipantCount < value.RespondedParticipantCount)
                throw new InvalidOperationException("WorldEventResponseCountInvalid");

            var choices = value.Choices ?? Array.Empty<SimulationWorldEventChoiceApiModel>();
            EnsureUnique(choices.Select(choice => choice.ChoiceStableId),
                "WorldEventChoiceStableIdDuplicated");
            var mappedChoices = choices.OrderBy(choice => choice.DisplayOrder)
                .Select(MapChoice).ToArray();
            if (!string.IsNullOrWhiteSpace(value.SelectedChoiceStableId)
                && !mappedChoices.Any(choice => string.Equals(choice.ChoiceStableId,
                    value.SelectedChoiceStableId, StringComparison.Ordinal)))
                throw new InvalidOperationException("WorldEventSelectedChoiceMissing");

            return new 세계사건표현Snapshot
            {
                EventStableId = value.EventStableId.Trim(),
                EventRevision = value.EventRevision,
                LastChangedWorldRevision = value.LastChangedWorldRevision,
                EventTypeCode = value.EventTypeCode.Trim(),
                TriggerCode = value.TriggerCode.Trim(),
                StateCode = value.StateCode.Trim(),
                OccurredWorldTick = value.OccurredWorldTick,
                PresentationKey = value.PresentationKey.Trim(),
                ResponseKindCode = value.ResponseKindCode.Trim(),
                SourceOpportunityStableId = value.SourceOpportunityStableId.Trim(),
                SelectedChoiceStableId = value.SelectedChoiceStableId.Trim(),
                ActiveBuildingStableId = value.ActiveBuildingStableId.Trim(),
                AnchorBuildingStableIds = Clone(value.AnchorBuildingStableIds),
                TileKeys = Clone(value.TileKeys),
                RegionStableIds = Clone(value.RegionStableIds),
                ParticipantPlayerStableIds = Clone(value.ParticipantPlayerStableIds),
                RespondedParticipantCount = value.RespondedParticipantCount,
                RequiredParticipantCount = value.RequiredParticipantCount,
                CanRespond = value.CanRespond,
                RequiresExpectedRevision = value.RequiresExpectedRevision,
                RuleRevision = value.RuleRevision.Trim(),
                Choices = mappedChoices,
            };
        }

        private static 세계사건선택지Snapshot MapChoice(
            SimulationWorldEventChoiceApiModel value)
        {
            if (value == null)
                throw new InvalidOperationException("WorldEventChoiceMissing");
            Require(value.ChoiceStableId, "WorldEventChoiceStableIdMissing");
            Require(value.KoreanTitle, "WorldEventChoiceTitleMissing");
            return new 세계사건선택지Snapshot
            {
                ChoiceStableId = value.ChoiceStableId.Trim(),
                DisplayOrder = value.DisplayOrder,
                CardStableId = value.CardStableId.Trim(),
                CardRevision = value.CardRevision.Trim(),
                OrientationCode = value.OrientationCode.Trim(),
                KoreanTitle = value.KoreanTitle.Trim(),
                KoreanSummary = value.KoreanSummary.Trim(),
            };
        }

        private static string[] Clone(string[] values)
            => (values ?? Array.Empty<string>()).ToArray();

        private static void EnsureUnique(IEnumerable<string> values, string errorCode)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                Require(value, errorCode.Replace("Duplicated", "Missing"));
                if (!seen.Add(value.Trim()))
                    throw new InvalidOperationException(errorCode);
            }
        }

        private static void Require(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(errorCode);
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public interface ISimulationWorldEventApiClient
    {
        Task<SimulationWorldEventProjectionApiModel> GetChangesAsync(
            string sessionStableId,
            long afterWorldRevision,
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public interface I세계사건ProjectionRepository
    {
        Task<세계사건변경Projection> 변경조회Async(
            string sessionStableId,
            long afterWorldRevision,
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public sealed class SimulationWorldEventApiRepository
        : I세계사건ProjectionRepository
    {
        private readonly ISimulationWorldEventApiClient apiClient;
        private readonly SimulationWorldEventProjectionMapper mapper;

        public SimulationWorldEventApiRepository(
            ISimulationWorldEventApiClient apiClient,
            SimulationWorldEventProjectionMapper mapper)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<세계사건변경Projection> 변경조회Async(
            string sessionStableId,
            long afterWorldRevision,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new ArgumentException("SimulationSessionStableIdMissing",
                    nameof(sessionStableId));
            if (afterWorldRevision < -1)
                throw new ArgumentOutOfRangeException(nameof(afterWorldRevision));
            var source = await apiClient.GetChangesAsync(sessionStableId.Trim(),
                afterWorldRevision, cancellationToken).ConfigureAwait(false);
            var projection = mapper.Map(source);
            if (!string.Equals(projection.SessionStableId, sessionStableId.Trim(),
                StringComparison.Ordinal))
                throw new InvalidOperationException("WorldEventSessionMismatch");
            return projection;
        }
    }
}
