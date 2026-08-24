using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Perspectives
{
    public static class RolePresentationVersions
    {
        public const string InterpreterContract = "authorized-role-projection-v1";
        public const string RuleSet = "server-authorized-role-v1";
        public const string VisualRule = "role-emphasis-visual-v1";
        public const string PresentationContract = "role-presentation-v1";
    }

    /// <summary>서버가 허용한 role projection만 조회하며 Unity View를 알지 않습니다.</summary>
    public sealed class AuthorizedRoleProjectionQuery
    {
        private readonly 역할관점조회UseCase query;

        public AuthorizedRoleProjectionQuery(역할관점조회UseCase query)
            => this.query = query ?? throw new ArgumentNullException(nameof(query));

        public Task<역할관점Snapshot> ExecuteAsync(
            역할관점조회Request request,
            CancellationToken cancellationToken = default)
            => query.실행Async(request, cancellationToken);
    }

    public sealed class RoleObjectPresentationModel
    {
        public string TargetStableId { get; set; } = string.Empty;
        public string EmphasisCode { get; set; } = string.Empty;
        public string LabelText { get; set; } = string.Empty;
        public string DetailPanelCode { get; set; } = string.Empty;
    }

    public sealed class RoleInteractionPresentationModel
    {
        public string InteractionCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public bool RequiresExplicitConfirmation { get; set; }
        public bool RequiresCanonicalStateRefresh { get; set; }
    }

    public sealed class RolePresentationModel
    {
        public string AuthorizedSnapshotStableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string InterpretationRevision { get; set; } = string.Empty;
        public string PresentationRevision { get; set; } = string.Empty;
        public string AuthorizedRoleCode { get; set; } = string.Empty;
        public string WorldZoneCode { get; set; } = string.Empty;
        public string ViewerScopeCode { get; set; } = string.Empty;
        public RoleObjectPresentationModel[] Objects { get; set; } =
            Array.Empty<RoleObjectPresentationModel>();
        public RoleInteractionPresentationModel[] AllowedInteractions { get; set; } =
            Array.Empty<RoleInteractionPresentationModel>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class RolePresentationPresenter
    {
        public RolePresentationModel Present(역할관점Snapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            var inputs = new DataRevisionSet(new[]
            {
                new DataRevisionReference(
                    snapshot.StableId,
                    snapshot.Revision.ToString(CultureInfo.InvariantCulture),
                    snapshot.GeneratedAt),
            });
            var interpretationRevision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                inputs,
                RolePresentationVersions.InterpreterContract,
                RolePresentationVersions.RuleSet,
                snapshot.AuthorizedRoleCode + "|" + snapshot.WorldZoneCode + "|" + snapshot.AuthorizationDecisionId);
            var presentationRevision = WorldDataFlowRevisionCalculator.CalculatePresentation(
                interpretationRevision,
                snapshot.AuthorizedRoleCode,
                RolePresentationVersions.VisualRule,
                RolePresentationVersions.PresentationContract);

            return new RolePresentationModel
            {
                AuthorizedSnapshotStableId = snapshot.StableId,
                DataRevision = snapshot.Revision,
                InterpretationRevision = interpretationRevision,
                PresentationRevision = presentationRevision,
                AuthorizedRoleCode = snapshot.AuthorizedRoleCode,
                WorldZoneCode = snapshot.WorldZoneCode,
                ViewerScopeCode = snapshot.ViewerScopeCode,
                Objects = snapshot.ObjectEmphases.Select(value => new RoleObjectPresentationModel
                {
                    TargetStableId = value.TargetStableId,
                    EmphasisCode = value.EmphasisCode,
                    LabelText = value.Label,
                    DetailPanelCode = value.DetailPanelCode,
                }).ToArray(),
                AllowedInteractions = snapshot.AllowedInteractions.Select(value => new RoleInteractionPresentationModel
                {
                    InteractionCode = value.InteractionCode,
                    TargetStableId = value.TargetStableId,
                    EffectCode = value.EffectCode,
                    RequiresExplicitConfirmation = value.RequiresExplicitConfirmation,
                    RequiresCanonicalStateRefresh = value.RequiresCanonicalStateRefresh,
                }).ToArray(),
            };
        }
    }

    public interface IRolePresentationTarget
    {
        string StableId { get; }
        void ClearRolePresentation();
        void ApplyRolePresentation(RoleObjectPresentationModel model);
    }

    public interface IRolePresentationInteractionSink
    {
        void ReplaceAllowedInteractions(IReadOnlyList<RoleInteractionPresentationModel> interactions);
    }

    public sealed class RolePresentationApplicationResult
    {
        public int AppliedTargetCount { get; set; }
        public string[] UnresolvedTargetStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class RolePresentationApplicator
    {
        public RolePresentationApplicationResult Apply(
            RolePresentationModel model,
            IReadOnlyList<IRolePresentationTarget> targets,
            IRolePresentationInteractionSink interactionSink)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (interactionSink == null) throw new ArgumentNullException(nameof(interactionSink));

            var targetMap = new Dictionary<string, IRolePresentationTarget>(StringComparer.Ordinal);
            foreach (var target in targets)
            {
                if (target == null || !StableDataId.IsValid(target.StableId))
                    throw new InvalidOperationException("RolePresentationTargetInvalid");
                if (!targetMap.TryAdd(target.StableId, target))
                    throw new InvalidOperationException("DuplicateRolePresentationTarget:" + target.StableId);
                target.ClearRolePresentation();
            }

            var applied = 0;
            var unresolved = new List<string>();
            foreach (var value in model.Objects)
            {
                if (targetMap.TryGetValue(value.TargetStableId, out var target))
                {
                    target.ApplyRolePresentation(value);
                    applied++;
                }
                else
                {
                    unresolved.Add(value.TargetStableId);
                }
            }

            interactionSink.ReplaceAllowedInteractions(model.AllowedInteractions);
            return new RolePresentationApplicationResult
            {
                AppliedTargetCount = applied,
                UnresolvedTargetStableIds = unresolved.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            };
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class RolePresentationPerspectiveCoordinator
    {
        private readonly RolePresentationPresenter presenter;
        private readonly RolePresentationApplicator applicator;

        public RolePresentationPerspectiveCoordinator(
            RolePresentationPresenter presenter,
            RolePresentationApplicator applicator)
        {
            this.presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            this.applicator = applicator ?? throw new ArgumentNullException(nameof(applicator));
        }

        public RolePresentationModel Apply(
            역할관점Snapshot authorizedSnapshot,
            IReadOnlyList<IRolePresentationTarget> targets,
            IRolePresentationInteractionSink interactionSink)
        {
            var model = presenter.Present(authorizedSnapshot);
            applicator.Apply(model, targets, interactionSink);
            return model;
        }
    }
}
