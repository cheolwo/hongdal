using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ssalddel.Unity.TeamRoles
{
    public static class TeamRoleCardApiRoutes
    {
        public static string Base(string sessionStableId)
            => "/api/simulation/v1/sessions/"
               + Uri.EscapeDataString(sessionStableId)
               + "/team-role-cards";

        public static string Get(string sessionStableId, string actorStableId)
            => Base(sessionStableId) + "?actorStableId="
               + Uri.EscapeDataString(actorStableId);
        public static string Equip(string sessionStableId)
            => Base(sessionStableId) + "/equip";
        public static string StartActivity(string sessionStableId)
            => Base(sessionStableId) + "/activities/start";
        public static string EndActivity(string sessionStableId)
            => Base(sessionStableId) + "/activities/end";
    }

    public sealed class TeamRoleCardEquipApiRequest
    {
        public Guid ClientRequestId { get; set; }
        public long ExpectedRevision { get; set; }
        public long ExpectedTeamPolicyRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string SlotCode { get; set; } = string.Empty;
    }

    public sealed class TeamActivityStartApiRequest
    {
        public Guid ClientRequestId { get; set; }
        public long ExpectedRevision { get; set; }
        public long ExpectedTeamPolicyRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string ActivityRoleCode { get; set; } = string.Empty;
        public string ActivityStableId { get; set; } = string.Empty;
        public string LocationStableId { get; set; } = string.Empty;
    }

    public sealed class TeamActivityEndApiRequest
    {
        public Guid ClientRequestId { get; set; }
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string ActivityStableId { get; set; } = string.Empty;
    }

    public sealed class TeamRoleCardApiModel
    {
        public string CardCopyStableId { get; set; } = string.Empty;
        public string CardDefinitionStableId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string[] ActivityRoleCodes { get; set; } = Array.Empty<string>();
        public string EquippedActorStableId { get; set; } = string.Empty;
        public string SlotCode { get; set; } = string.Empty;
        public string LockedActivityStableId { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public bool IsPhysicalItem { get; set; }
        public bool RequiresPhysicalProximityForEquip { get; set; }
    }

    public sealed class TeamActivityAssignmentApiModel
    {
        public string ActivityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string ActivityRoleCode { get; set; } = string.Empty;
        public string LocationStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
    }

    public sealed class TeamMemberRoleApiModel
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string CurrentRoleCode { get; set; } = string.Empty;
        public string ActivityStableId { get; set; } = string.Empty;
        public string[] EquippedCardCopyStableIds { get; set; }
            = Array.Empty<string>();
        public bool IsPermanentProfession { get; set; }
    }

    public sealed class TeamRoleCardStateApiModel
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TeamStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public long TeamPolicyRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] MemberActorStableIds { get; set; } = Array.Empty<string>();
        public TeamRoleCardApiModel[] Cards { get; set; }
            = Array.Empty<TeamRoleCardApiModel>();
        public TeamActivityAssignmentApiModel[] ActiveActivities { get; set; }
            = Array.Empty<TeamActivityAssignmentApiModel>();
        public TeamMemberRoleApiModel[] MemberRoles { get; set; }
            = Array.Empty<TeamMemberRoleApiModel>();
        public bool SupportsRemoteEquip { get; set; }
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class TeamRoleCardPresentationState
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TeamStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public long TeamPolicyRevision { get; set; }
        public TeamRoleCardApiModel[] Cards { get; set; }
            = Array.Empty<TeamRoleCardApiModel>();
        public TeamActivityAssignmentApiModel[] ActiveActivities { get; set; }
            = Array.Empty<TeamActivityAssignmentApiModel>();
        public TeamMemberRoleApiModel[] MemberRoles { get; set; }
            = Array.Empty<TeamMemberRoleApiModel>();
        public bool CanRequestRemoteEquip { get; set; }
        public bool CalculatesRoleLocally { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class TeamRoleCardPresentationMapper
    {
        public TeamRoleCardPresentationState Map(TeamRoleCardStateApiModel source)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.SessionStableId)
                || string.IsNullOrWhiteSpace(source.TeamStableId)
                || source.Revision < 0 || source.TeamPolicyRevision < 0
                || source.MemberActorStableIds == null || source.Cards == null
                || source.ActiveActivities == null || source.MemberRoles == null
                || !source.SupportsRemoteEquip || !source.SimulationOnly
                || source.IsOperationalState
                || source.Cards.Any(value => value == null
                    || value.IsPhysicalItem
                    || value.RequiresPhysicalProximityForEquip
                    || string.IsNullOrWhiteSpace(value.CardCopyStableId)
                    || value.ActivityRoleCodes == null)
                || source.MemberRoles.Any(value => value == null
                    || value.IsPermanentProfession
                    || !source.MemberActorStableIds.Contains(
                        value.ActorStableId, StringComparer.Ordinal)))
                throw new InvalidOperationException(
                    "TeamRoleCardPresentationBoundaryInvalid");

            return new TeamRoleCardPresentationState
            {
                SessionStableId = source.SessionStableId,
                TeamStableId = source.TeamStableId,
                Revision = source.Revision,
                TeamPolicyRevision = source.TeamPolicyRevision,
                Cards = source.Cards.ToArray(),
                ActiveActivities = source.ActiveActivities.ToArray(),
                MemberRoles = source.MemberRoles.ToArray(),
                CanRequestRemoteEquip = true,
                CalculatesRoleLocally = false,
                PresentationOnly = true,
            };
        }
    }

    public interface ITeamRoleCardAuthorityClient
    {
        Task<TeamRoleCardStateApiModel> LoadAsync(string sessionStableId,
            string actorStableId, CancellationToken cancellationToken);
        Task<TeamRoleCardStateApiModel> EquipAsync(string sessionStableId,
            TeamRoleCardEquipApiRequest request, CancellationToken cancellationToken);
        Task<TeamRoleCardStateApiModel> StartActivityAsync(string sessionStableId,
            TeamActivityStartApiRequest request, CancellationToken cancellationToken);
        Task<TeamRoleCardStateApiModel> EndActivityAsync(string sessionStableId,
            TeamActivityEndApiRequest request, CancellationToken cancellationToken);
    }

    public sealed class TeamRoleCardClientCoordinator
    {
        private readonly ITeamRoleCardAuthorityClient authority;
        private readonly TeamRoleCardPresentationMapper mapper;

        public TeamRoleCardClientCoordinator(ITeamRoleCardAuthorityClient client,
            TeamRoleCardPresentationMapper presentationMapper)
        {
            authority = client ?? throw new ArgumentNullException(nameof(client));
            mapper = presentationMapper
                ?? throw new ArgumentNullException(nameof(presentationMapper));
        }

        public TeamRoleCardPresentationState? Current { get; private set; }

        public async Task<TeamRoleCardPresentationState> LoadAsync(
            string sessionStableId, string actorStableId,
            CancellationToken cancellationToken = default)
            => Accept(await authority.LoadAsync(sessionStableId, actorStableId,
                cancellationToken));

        public async Task<TeamRoleCardPresentationState> EquipAsync(
            string sessionStableId, TeamRoleCardEquipApiRequest request,
            CancellationToken cancellationToken = default)
            => Accept(await authority.EquipAsync(sessionStableId, request,
                cancellationToken));

        public async Task<TeamRoleCardPresentationState> StartActivityAsync(
            string sessionStableId, TeamActivityStartApiRequest request,
            CancellationToken cancellationToken = default)
            => Accept(await authority.StartActivityAsync(sessionStableId, request,
                cancellationToken));

        public async Task<TeamRoleCardPresentationState> EndActivityAsync(
            string sessionStableId, TeamActivityEndApiRequest request,
            CancellationToken cancellationToken = default)
            => Accept(await authority.EndActivityAsync(sessionStableId, request,
                cancellationToken));

        private TeamRoleCardPresentationState Accept(
            TeamRoleCardStateApiModel response)
        {
            var next = mapper.Map(response);
            if (Current != null && next.SessionStableId == Current.SessionStableId
                && next.Revision < Current.Revision)
                throw new InvalidOperationException("TeamRoleCardRevisionStale");
            Current = next;
            return next;
        }
    }
}
