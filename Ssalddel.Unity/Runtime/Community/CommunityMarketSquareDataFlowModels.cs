using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Community
{
    public static class CommunitySquareDataFlowVersions
    {
        public const string InterpreterContract = "community-square-interpretation-v1";
        public const string RuleSet = "public-community-object-v1";
        public const string VisualRule = "community-square-visual-v1";
        public const string PresentationContract = "community-square-presentation-v1";
        public const string Perspective = "PublicObserver";
    }

    public sealed class CommunitySquareBoardData
    {
        public string StableId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTimeOffset? AsOfUtc { get; set; }
    }

    public sealed class CommunitySquarePostData
    {
        public string StableId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DetailHref { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTimeOffset AsOfUtc { get; set; }
    }

    public sealed class CommunitySquareActivityData
    {
        public string StableId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTimeOffset AsOfUtc { get; set; }
    }

    public sealed class CommunitySquareLedgerData
    {
        public string StableId { get; set; } = string.Empty;
        public string SourcePostStableId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool DetailAvailable { get; set; }
        public string DetailHref { get; set; } = string.Empty;
    }

    public sealed class CommunitySquareDataSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public CommunitySquareBoardData[] Boards { get; set; } = Array.Empty<CommunitySquareBoardData>();
        public CommunitySquarePostData[] Posts { get; set; } = Array.Empty<CommunitySquarePostData>();
        public CommunitySquareActivityData[] Activities { get; set; } = Array.Empty<CommunitySquareActivityData>();
        public CommunitySquareLedgerData[] Ledgers { get; set; } = Array.Empty<CommunitySquareLedgerData>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class CommunitySquareDataMapper
    {
        public CommunitySquareDataSnapshot Map(CommunityMarketSquareSnapshotApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            RequireStableId(source.StableId);
            Require(source.Revision, "CommunitySquareRevisionMissing");
            if (source.GeneratedAtUtc == default) throw new InvalidOperationException("CommunitySquareGeneratedAtMissing");
            if (source.Boards == null || source.Posts == null || source.ActivitySignals == null || source.Ledgers == null)
                throw new InvalidOperationException("CommunitySquareCollectionsMissing");

            Reject(source.Boards.Select(value => value?.StableId), "CommunitySquareBoard");
            Reject(source.Posts.Select(value => value?.StableId), "CommunitySquarePost");
            Reject(source.ActivitySignals.Select(value => value?.StableId), "CommunitySquareActivity");
            Reject(source.Ledgers.Select(value => value?.StableId), "CommunitySquareLedger");
            var postIds = new HashSet<string>(source.Posts.Select(value => value.StableId), StringComparer.Ordinal);
            foreach (var ledger in source.Ledgers)
            {
                if (!postIds.Contains(ledger.SourcePostStableId))
                    throw new InvalidOperationException("CommunitySquareLedgerPostUnknown:" + ledger.StableId);
                if (!ledger.DetailAvailable && !string.IsNullOrWhiteSpace(ledger.DetailHref))
                    throw new InvalidOperationException("CommunitySquareLedgerDetailScopeInvalid:" + ledger.StableId);
            }

            return new CommunitySquareDataSnapshot
            {
                StableId = source.StableId.Trim(), DataRevision = source.Revision.Trim(), GeneratedAtUtc = source.GeneratedAtUtc,
                Boards = source.Boards.Select(value => new CommunitySquareBoardData
                {
                    StableId = Valid(value.StableId), Title = Required(value.DisplayName, value.StableId),
                    Summary = value.Description?.Trim() ?? string.Empty, Status = value.PostingAccessCode?.Trim() ?? string.Empty,
                    Count = value.PostCount, AsOfUtc = value.LatestPostAtUtc,
                }).ToArray(),
                Posts = source.Posts.Select(value => new CommunitySquarePostData
                {
                    StableId = Valid(value.StableId), Title = Required(value.Title, value.StableId),
                    Summary = value.Excerpt?.Trim() ?? string.Empty, Status = value.Category?.Trim() ?? string.Empty,
                    DetailHref = value.DetailHref?.Trim() ?? string.Empty, Count = value.CommentCount, AsOfUtc = value.PublishedAtUtc,
                }).ToArray(),
                Activities = source.ActivitySignals.Select(value => new CommunitySquareActivityData
                {
                    StableId = Valid(value.StableId), Title = Required(value.Title, value.StableId),
                    Summary = value.Summary?.Trim() ?? string.Empty, Status = value.ActivityKind?.Trim() ?? string.Empty,
                    Count = value.AggregationCount, AsOfUtc = value.OccurredAtUtc,
                }).ToArray(),
                Ledgers = source.Ledgers.Select(value => new CommunitySquareLedgerData
                {
                    StableId = Valid(value.StableId), SourcePostStableId = value.SourcePostStableId.Trim(),
                    Title = Required(value.Title, value.StableId), Summary = value.TemplateName?.Trim() ?? string.Empty,
                    Status = (value.State + "/" + value.CurrentStage).Trim('/'), DetailAvailable = value.DetailAvailable,
                    DetailHref = value.DetailAvailable ? value.DetailHref?.Trim() ?? string.Empty : string.Empty,
                }).ToArray(),
            };
        }

        private static void Reject(IEnumerable<string?> values, string kind)
        {
            if (values.Any(value => string.IsNullOrWhiteSpace(value))) throw new InvalidOperationException(kind + "StableIdMissing");
            var duplicate = values.Where(value => value != null).GroupBy(value => value!, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null) throw new InvalidOperationException("Duplicate" + kind + ":" + duplicate.Key);
        }
        private static string Valid(string value) { RequireStableId(value); return value.Trim(); }
        private static string Required(string value, string id)
        { Require(value, "CommunitySquareTitleMissing:" + id); return value.Trim(); }
        private static void RequireStableId(string value)
        { if (!StableDataId.IsValid(value)) throw new InvalidOperationException("CommunitySquareStableIdInvalid:" + value); }
        private static void Require(string value, string error)
        { if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(error); }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public interface ICommunitySquareDataRepository
    {
        Task<CommunitySquareDataSnapshot> 조회Async(CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public sealed class CommunitySquareApiDataRepository : ICommunitySquareDataRepository
    {
        private readonly ICommunityMarketSquareApiClient apiClient;
        private readonly CommunitySquareDataMapper mapper;
        public CommunitySquareApiDataRepository(ICommunityMarketSquareApiClient apiClient, CommunitySquareDataMapper mapper)
        { this.apiClient = apiClient; this.mapper = mapper; }
        public async Task<CommunitySquareDataSnapshot> 조회Async(CancellationToken cancellationToken = default)
            => mapper.Map(await apiClient.GetPublicSnapshotAsync(cancellationToken).ConfigureAwait(false));
    }

    public sealed class CommunitySquareWorldInterpreter
    {
        public CommunityMarketSquareSnapshot Interpret(CommunitySquareDataSnapshot source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var inputs = new DataRevisionSet(new[]
            {
                new DataRevisionReference(source.StableId, source.DataRevision, source.GeneratedAtUtc),
            });
            var revision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                inputs, CommunitySquareDataFlowVersions.InterpreterContract,
                CommunitySquareDataFlowVersions.RuleSet, "public");
            var items = source.Boards.Select(value => Item(value.StableId, "Board", value.Title, value.Summary, value.Status, string.Empty, value.Count, value.AsOfUtc))
                .Concat(source.Posts.Select(value => Item(value.StableId, "Post", value.Title, value.Summary, value.Status, value.DetailHref, value.Count, value.AsOfUtc)))
                .Concat(source.Activities.Select(value => Item(value.StableId, "Activity", value.Title, value.Summary, value.Status, string.Empty, value.Count, value.AsOfUtc)))
                .Concat(source.Ledgers.Select(value => Item(value.StableId, "Ledger", value.Title, value.Summary, value.Status, value.DetailHref, 0, null)))
                .ToArray();
            return new CommunityMarketSquareSnapshot
            {
                StableId = source.StableId, Revision = source.DataRevision, GeneratedAtUtc = source.GeneratedAtUtc,
                Items = items,
                Lineage = new InterpretationLineage(
                    inputs, CommunitySquareDataFlowVersions.InterpreterContract,
                    CommunitySquareDataFlowVersions.RuleSet, revision),
            };
        }

        private static CommunitySquareWorldItem Item(
            string id, string kind, string title, string summary, string status,
            string href, int count, DateTimeOffset? asOf)
            => new CommunitySquareWorldItem
            {
                StableId = id, Kind = kind, Title = title, Summary = summary,
                Status = status, DetailHref = href, Count = count, AsOfUtc = asOf,
            };
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class CommunitySquareDataFlowQueryUseCase
    {
        private readonly ICommunitySquareDataRepository repository;
        private readonly CommunitySquareWorldInterpreter interpreter;
        public CommunitySquareDataFlowQueryUseCase(ICommunitySquareDataRepository repository, CommunitySquareWorldInterpreter interpreter)
        { this.repository = repository; this.interpreter = interpreter; }
        public async Task<CommunityMarketSquareSnapshot> 실행Async(CancellationToken cancellationToken = default)
            => interpreter.Interpret(await repository.조회Async(cancellationToken).ConfigureAwait(false));
    }
}
