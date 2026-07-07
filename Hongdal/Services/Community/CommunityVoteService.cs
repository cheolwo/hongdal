using System.Security.Cryptography;
using System.Text;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.ContractManagement;

namespace Hongdal.Services.Community;

public interface ICommunityVoteService
{
    Task<CommunityVoteResponse> CreateAsync(CommunityVoteCreateRequest request, CancellationToken cancellationToken);

    Task<CommunityVoteListResponse> ListAsync(string? appKey, string? communityScope, CancellationToken cancellationToken);

    Task<CommunityVoteResponse?> GetAsync(Guid voteId, CancellationToken cancellationToken);

    Task<CommunityVoteResponse?> CastVoteAsync(Guid voteId, CommunityVoteCastRequest request, CancellationToken cancellationToken);

    Task<CommunityVoteResponse?> CloseAsync(Guid voteId, CommunityVoteCloseRequest request, CancellationToken cancellationToken);

    Task<CommunityVoteResolutionDocumentResponse?> CreateResolutionDraftAsync(
        Guid voteId,
        CommunityVoteResolutionDraftRequest request,
        CancellationToken cancellationToken);

    Task<CommunityVoteResolutionDocumentResponse?> SignResolutionAsync(
        Guid voteId,
        CommunityVoteResolutionSignRequest request,
        CancellationToken cancellationToken);

    Task<CommunityVoteResolutionDocumentResponse?> MarkResolutionReadyToSignAsync(
        Guid voteId,
        CommunityVoteResolutionReadyToSignRequest request,
        CancellationToken cancellationToken);
}

public sealed class InMemoryCommunityVoteService : ICommunityVoteService
{
    private readonly object _gate = new();
    private readonly List<CommunityVoteRecord> _votes = [];

    public Task<CommunityVoteResponse> CreateAsync(CommunityVoteCreateRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("투표 제목이 필요합니다.");
        }

        var options = request.Options
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select((text, index) => new CommunityVoteOptionRecord($"option-{index + 1}", text.Trim()))
            .ToArray();
        if (options.Length < 2)
        {
            throw new InvalidOperationException("투표 선택지는 2개 이상이어야 합니다.");
        }

        var vote = new CommunityVoteRecord
        {
            Id = Guid.NewGuid(),
            AppKey = Normalize(request.AppKey, "platform"),
            CommunityScope = Normalize(request.CommunityScope, "platform"),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            AllowMultipleSelection = request.AllowMultipleSelection,
            ResolutionDocumentEnabled = request.ResolutionDocumentEnabled,
            SignatureRequired = request.SignatureRequired,
            CreatedByDisplayName = Normalize(request.CreatedByDisplayName, "익명 참여자"),
            CreatedAtUtc = DateTime.UtcNow,
            ClosesAtUtc = request.ClosesAtUtc,
            Options = options
        };

        lock (_gate)
        {
            _votes.Add(vote);
            return Task.FromResult(ToResponse(vote));
        }
    }

    public Task<CommunityVoteListResponse> ListAsync(string? appKey, string? communityScope, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var items = _votes.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(appKey))
            {
                items = items.Where(x => string.Equals(x.AppKey, appKey.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(communityScope))
            {
                items = items.Where(x => string.Equals(x.CommunityScope, communityScope.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(new CommunityVoteListResponse
            {
                Items = items.OrderByDescending(x => x.CreatedAtUtc).Select(ToResponse).ToArray()
            });
        }
    }

    public Task<CommunityVoteResponse?> GetAsync(Guid voteId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var vote = Find(voteId);
            return Task.FromResult(vote is null ? null : ToResponse(vote));
        }
    }

    public Task<CommunityVoteResponse?> CastVoteAsync(Guid voteId, CommunityVoteCastRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var vote = Find(voteId);
            if (vote is null)
            {
                return Task.FromResult<CommunityVoteResponse?>(null);
            }

            EnsureOpen(vote);
            if (request.OptionIds.Count == 0)
            {
                throw new InvalidOperationException("선택한 투표 항목이 없습니다.");
            }

            if (!vote.AllowMultipleSelection && request.OptionIds.Count > 1)
            {
                throw new InvalidOperationException("이 투표는 하나의 항목만 선택할 수 있습니다.");
            }

            var validOptionIds = vote.Options.Select(x => x.OptionId).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (request.OptionIds.Any(x => !validOptionIds.Contains(x)))
            {
                throw new InvalidOperationException("존재하지 않는 투표 항목이 포함되어 있습니다.");
            }

            var voterHash = Hash(Normalize(request.VoterKey, request.VoterDisplayName));
            vote.Votes.RemoveAll(x => string.Equals(x.VoterHash, voterHash, StringComparison.Ordinal));
            vote.Votes.Add(new CommunityVoteCastRecord(
                voterHash,
                Normalize(request.VoterDisplayName, "익명 참여자"),
                request.OptionIds.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                DateTime.UtcNow));

            return Task.FromResult<CommunityVoteResponse?>(ToResponse(vote));
        }
    }

    public Task<CommunityVoteResponse?> CloseAsync(Guid voteId, CommunityVoteCloseRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var vote = Find(voteId);
            if (vote is null)
            {
                return Task.FromResult<CommunityVoteResponse?>(null);
            }

            vote.Status = CommunityVoteStatusCodes.Closed;
            vote.ClosedAtUtc = DateTime.UtcNow;
            vote.ClosedByDisplayName = Normalize(request.ClosedByDisplayName, "운영자");
            return Task.FromResult<CommunityVoteResponse?>(ToResponse(vote));
        }
    }

    public Task<CommunityVoteResolutionDocumentResponse?> CreateResolutionDraftAsync(
        Guid voteId,
        CommunityVoteResolutionDraftRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var vote = Find(voteId);
            if (vote is null)
            {
                return Task.FromResult<CommunityVoteResolutionDocumentResponse?>(null);
            }

            if (!vote.ResolutionDocumentEnabled)
            {
                throw new InvalidOperationException("이 투표는 결의문 생성을 사용하지 않습니다.");
            }

            if (vote.Status == CommunityVoteStatusCodes.Open)
            {
                throw new InvalidOperationException("투표를 마감한 뒤 결의문을 만들 수 있습니다.");
            }

            if (request.RequiredSigners.Count == 0 && vote.SignatureRequired)
            {
                throw new InvalidOperationException("서명 필수 결의문은 서명 요청 대상이 필요합니다.");
            }

            var documentText = BuildDocumentText(vote, request);
            var documentHash = Hash(documentText);
            var documentNumber = $"COMM-VOTE-{DateTime.UtcNow:yyyyMMdd}-{vote.Id:N}"[..42];
            var signatureBundle = request.RequiredSigners.Count == 0
                ? null
                : ContractElectronicSignaturePlanner.CreateBundle(
                    documentNumber,
                    documentHash,
                    request.RequiredSigners.Select(x => new ContractSignatureRequest(
                        x.PartyId,
                        x.RoleCode,
                        x.SignerDisplayName,
                        IsRequiredSigner: true,
                        DateTimeOffset.UtcNow)),
                    DateTimeOffset.UtcNow);

            vote.Status = CommunityVoteStatusCodes.ResolutionDrafted;
            vote.ResolutionDocument = new CommunityVoteResolutionDocumentRecord
            {
                Id = Guid.NewGuid(),
                VoteId = vote.Id,
                DocumentNumber = documentNumber,
                DocumentTitle = Normalize(request.DocumentTitle, $"{vote.Title} 결의문"),
                ResolutionText = Normalize(request.ResolutionText, "투표 결과에 따른 커뮤니티 결의 초안입니다."),
                DocumentHash = documentHash,
                Status = request.LegalReviewRequested
                    ? CommunityVoteResolutionStatusCodes.LegalReviewRequired
                    : vote.SignatureRequired
                        ? CommunityVoteResolutionStatusCodes.ReadyToSign
                        : CommunityVoteResolutionStatusCodes.Draft,
                LegalEffectNotice = LegalEffectNotice,
                CreatedAtUtc = DateTime.UtcNow,
                SignatureBundle = signatureBundle
            };

            return Task.FromResult<CommunityVoteResolutionDocumentResponse?>(ToResolutionResponse(vote.ResolutionDocument));
        }
    }

    public Task<CommunityVoteResolutionDocumentResponse?> SignResolutionAsync(
        Guid voteId,
        CommunityVoteResolutionSignRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var vote = Find(voteId);
            var document = vote?.ResolutionDocument;
            if (document?.SignatureBundle is null)
            {
                return Task.FromResult<CommunityVoteResolutionDocumentResponse?>(null);
            }

            if (document.Status == CommunityVoteResolutionStatusCodes.LegalReviewRequired)
            {
                throw new InvalidOperationException("법무/운영 검토가 필요한 결의문은 서명 가능 상태로 전환한 뒤 서명해야 합니다.");
            }

            var evidence = new ContractSignatureEvidence(
                request.PartyId,
                Normalize(request.SignerDisplayName, "익명 참여자"),
                Normalize(request.SignatureMethodCode, ContractSignatureMethodCode.PlatformClickSign),
                document.DocumentHash,
                Hash(request.ConsentText),
                Hash(request.SignatureEvidencePayload),
                DateTimeOffset.UtcNow,
                request.ClientIpHash);
            document.SignatureBundle = ContractElectronicSignaturePlanner.AddEvidence(document.SignatureBundle, evidence);

            var plan = ContractElectronicSignaturePlanner.Plan(document.SignatureBundle, DateTimeOffset.UtcNow);
            document.Status = plan.IsFullySigned
                ? CommunityVoteResolutionStatusCodes.Signed
                : CommunityVoteResolutionStatusCodes.PartiallySigned;

            return Task.FromResult<CommunityVoteResolutionDocumentResponse?>(ToResolutionResponse(document));
        }
    }

    public Task<CommunityVoteResolutionDocumentResponse?> MarkResolutionReadyToSignAsync(
        Guid voteId,
        CommunityVoteResolutionReadyToSignRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var document = Find(voteId)?.ResolutionDocument;
            if (document is null)
            {
                return Task.FromResult<CommunityVoteResolutionDocumentResponse?>(null);
            }

            if (document.SignatureBundle is null)
            {
                document.Status = CommunityVoteResolutionStatusCodes.Draft;
            }
            else
            {
                document.Status = CommunityVoteResolutionStatusCodes.ReadyToSign;
            }

            document.LegalEffectNotice = $"{LegalEffectNotice} 검토자: {Normalize(request.ReviewedByDisplayName, "운영자")}.";
            return Task.FromResult<CommunityVoteResolutionDocumentResponse?>(ToResolutionResponse(document));
        }
    }

    private CommunityVoteRecord? Find(Guid voteId)
        => _votes.FirstOrDefault(x => x.Id == voteId);

    private static void EnsureOpen(CommunityVoteRecord vote)
    {
        if (vote.Status != CommunityVoteStatusCodes.Open || vote.ClosesAtUtc is not null && vote.ClosesAtUtc <= DateTime.UtcNow)
        {
            vote.Status = CommunityVoteStatusCodes.Closed;
            vote.ClosedAtUtc ??= DateTime.UtcNow;
            throw new InvalidOperationException("마감된 투표입니다.");
        }
    }

    private static CommunityVoteResponse ToResponse(CommunityVoteRecord vote)
    {
        var counts = vote.Votes
            .SelectMany(x => x.OptionIds)
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase);
        var maxCount = counts.Count == 0 ? 0 : counts.Values.Max();

        return new CommunityVoteResponse
        {
            Id = vote.Id,
            AppKey = vote.AppKey,
            CommunityScope = vote.CommunityScope,
            Title = vote.Title,
            Description = vote.Description,
            Status = vote.Status,
            AllowMultipleSelection = vote.AllowMultipleSelection,
            ResolutionDocumentEnabled = vote.ResolutionDocumentEnabled,
            SignatureRequired = vote.SignatureRequired,
            CreatedByDisplayName = vote.CreatedByDisplayName,
            CreatedAtUtc = vote.CreatedAtUtc,
            ClosesAtUtc = vote.ClosesAtUtc,
            ClosedAtUtc = vote.ClosedAtUtc,
            TotalVoteCount = vote.Votes.Count,
            Options = vote.Options.Select(x =>
            {
                counts.TryGetValue(x.OptionId, out var count);
                return new CommunityVoteOptionResponse
                {
                    OptionId = x.OptionId,
                    Text = x.Text,
                    VoteCount = count,
                    IsWinningOption = count > 0 && count == maxCount
                };
            }).ToArray(),
            ResolutionDocument = vote.ResolutionDocument is null ? null : ToResolutionResponse(vote.ResolutionDocument)
        };
    }

    private static CommunityVoteResolutionDocumentResponse ToResolutionResponse(CommunityVoteResolutionDocumentRecord document)
    {
        return new CommunityVoteResolutionDocumentResponse
        {
            Id = document.Id,
            VoteId = document.VoteId,
            DocumentNumber = document.DocumentNumber,
            DocumentTitle = document.DocumentTitle,
            ResolutionText = document.ResolutionText,
            DocumentHash = document.DocumentHash,
            Status = document.Status,
            LegalEffectNotice = document.LegalEffectNotice,
            CreatedAtUtc = document.CreatedAtUtc,
            SignaturePlan = document.SignatureBundle is null
                ? null
                : ContractElectronicSignaturePlanner.Plan(document.SignatureBundle, DateTimeOffset.UtcNow)
        };
    }

    private static string BuildDocumentText(CommunityVoteRecord vote, CommunityVoteResolutionDraftRequest request)
    {
        var resultLines = ToResponse(vote).Options
            .OrderByDescending(x => x.VoteCount)
            .Select(x => $"- {x.Text}: {x.VoteCount}표");
        return string.Join('\n',
            Normalize(request.DocumentTitle, $"{vote.Title} 결의문"),
            vote.Title,
            vote.Description,
            Normalize(request.ResolutionText, "투표 결과에 따른 커뮤니티 결의 초안입니다."),
            "투표 결과:",
            string.Join('\n', resultLines));
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private const string LegalEffectNotice =
        "이 문서는 커뮤니티 투표 결과와 전자서명 증적을 정리한 플랫폼 결의문입니다. 실제 법적 효력과 제출 가능 여부는 문서 종류, 당사자 권한, 고지/동의, 상대 기관 기준, 관련 법령 검토가 필요합니다.";

    private sealed class CommunityVoteRecord
    {
        public Guid Id { get; set; }
        public string AppKey { get; set; } = string.Empty;
        public string CommunityScope { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = CommunityVoteStatusCodes.Open;
        public bool AllowMultipleSelection { get; set; }
        public bool ResolutionDocumentEnabled { get; set; }
        public bool SignatureRequired { get; set; }
        public string CreatedByDisplayName { get; set; } = string.Empty;
        public string ClosedByDisplayName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ClosesAtUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public IReadOnlyList<CommunityVoteOptionRecord> Options { get; set; } = [];
        public List<CommunityVoteCastRecord> Votes { get; } = [];
        public CommunityVoteResolutionDocumentRecord? ResolutionDocument { get; set; }
    }

    private sealed record CommunityVoteOptionRecord(string OptionId, string Text);

    private sealed record CommunityVoteCastRecord(
        string VoterHash,
        string VoterDisplayName,
        IReadOnlyList<string> OptionIds,
        DateTime VotedAtUtc);

    private sealed class CommunityVoteResolutionDocumentRecord
    {
        public Guid Id { get; set; }
        public Guid VoteId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentTitle { get; set; } = string.Empty;
        public string ResolutionText { get; set; } = string.Empty;
        public string DocumentHash { get; set; } = string.Empty;
        public string Status { get; set; } = CommunityVoteResolutionStatusCodes.LegalReviewRequired;
        public string LegalEffectNotice { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public ContractElectronicSignatureBundle? SignatureBundle { get; set; }
    }
}
