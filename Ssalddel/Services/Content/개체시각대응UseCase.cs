using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Domain.Content;
using Ssalddel.Infrastructure.Persistence.WorldProjection;

namespace Ssalddel.Services.Content;

[SsalddelCodeMetadata(개체시각대응Codes.Feature, SsalddelCodeLayer.Application,
    "시각 대응 검토를 권한·판본·멱등 조건 아래 이력과 함께 원자 저장한다.",
    StepKey = "review", FlowOrder = 20, ExecutionStage = SsalddelCodeExecutionStage.Confirm,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ReadsFrom = SsalddelCodeDataScope.OperationalState | SsalddelCodeDataScope.SharedPublicData,
    WritesTo = SsalddelCodeDataScope.OperationalState,
    Boundary = "전용 대응/이력 테이블만 변경한다. 원천 업무·Unity·게시·수집을 실행하지 않는다.")]
public sealed class 개체시각대응UseCase(
    개체시각대응DbContext db, I개체시각대상Reader source, I개체시각자산Catalog catalog,
    IAuthorizationService authorization, ICurrentUserAccessor currentUser,
    IOptionsMonitor<개체시각자산Options> options, TimeProvider clock)
{
    public async Task<개체시각대응Result> ExecuteAsync(ClaimsPrincipal user, 개체시각대응Request request, CancellationToken ct)
    {
        var access = await AccessAsync(user, review: true);
        if (access is not null) return new(false, access);
        if (request is null || !Token(request.BindingId, 120) || !Token(request.IdempotencyKey, 120) ||
            request.ExpectedRevision < 0 || request.ExpectedRevision == long.MaxValue || !Enum.IsDefined(request.Action) ||
            (request.Action != 개체시각대응Action.SaveDraft && request.Candidate is not null) ||
            string.IsNullOrWhiteSpace(request.Note) || request.Note.Length > 1000 || request.Target is null)
            return new(false, "InvalidRequest");
        var read = await source.ReadAsync(request.Target, ct);
        if (!Valid(read)) return new(false, read.Target is null ? read.Diagnostic : "InvalidSource");
        var target = read.Target!;
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var requestHash = 개체시각선택Policy.Hash(request);
        var key = 개체시각선택Policy.Hash(new { Actor = actor, request.BindingId, request.IdempotencyKey });
        var previous = await db.History.AsNoTracking().SingleOrDefaultAsync(x => x.RequestKeyHash == key, ct);
        if (previous is not null)
        {
            if (previous.RequestHash != requestHash) return new(false, "IdempotencyConflict");
            var replay = Parse(previous.StateJson);
            if (!개체시각선택Policy.SameContext(replay, target)) return new(false, "ContextChanged");
            // 과거 저장 응답 재전달일 뿐 현재 후보 선택/권한 상태의 보증은 아니다.
            return new(true, "ReplayedHistoricalResult", replay, true);
        }
        var row = await db.Bindings.SingleOrDefaultAsync(x => x.BindingId == request.BindingId, ct);
        if (row is not null && row.BindingId != request.BindingId) return new(false, "BindingIdentityMismatch");
        if ((row?.Revision ?? 0) != request.ExpectedRevision) return new(false, "RevisionConflict");
        var current = row is null ? null : ParseRow(row);
        if (current is not null && (current.TypeDefault != request.TypeDefault ||
            !(request.Action == 개체시각대응Action.Exclude ? 개체시각선택Policy.SameSubject(current, target) :
                개체시각선택Policy.SameContext(current, target)))) return new(false, "ContextChanged");
        // 상태가 달라진 과거 대응도 폐기할 수 있지만 과거 문맥을 현재 상태로 위장하지 않는다.
        if (request.Action == 개체시각대응Action.Exclude && current is not null) target = current.Target;
        var context = 개체시각선택Policy.Context(target, request.TypeDefault);
        if (await db.Bindings.AsNoTracking().AnyAsync(x => x.ContextHash == context && x.BindingId != request.BindingId, ct))
            return new(false, "BindingConflict");
        var nextCandidate = request.Action == 개체시각대응Action.SaveDraft ? request.Candidate : current?.Candidate;
        if (nextCandidate?.AssetVersionId is not null)
        {
            var asset = await db.Assets.AsNoTracking().SingleOrDefaultAsync(x => x.AssetVersionId == nextCandidate.AssetVersionId, ct);
            if (asset is null) return new(false, "AssetVersionNotFound");
            if (!개체시각목록UseCase.Matches(asset, nextCandidate)) return new(false, "AssetReferenceMismatch");
        }
        string review;
        switch (request.Action)
        {
            case 개체시각대응Action.SaveDraft:
                if (nextCandidate is not null && !CandidateShape(nextCandidate)) return new(false, "InvalidCandidate");
                review = 개체시각대응Codes.Draft;
                break;
            case 개체시각대응Action.SubmitReview:
                if (current?.ReviewState != 개체시각대응Codes.Draft || nextCandidate is null)
                    return new(false, "ReviewTransitionInvalid");
                if (current.Target.Revision != target.Revision) return new(false, "SourceRevisionChanged");
                review = 개체시각대응Codes.Pending;
                break;
            case 개체시각대응Action.Approve:
                if (current?.ReviewState != 개체시각대응Codes.Pending) return new(false, "ReviewTransitionInvalid");
                if (current.Target.Revision != target.Revision) return new(false, "SourceRevisionChanged");
                var check = catalog.Check(target, nextCandidate, request.TypeDefault);
                if (check != "Valid") return new(false, check);
                review = 개체시각대응Codes.Approved;
                break;
            case 개체시각대응Action.Exclude:
                if (current is null) return new(false, "ReviewTransitionInvalid");
                review = 개체시각대응Codes.Excluded;
                break;
            default: return new(false, "InvalidRequest");
        }
        var next = new 개체시각대응Dto(request.BindingId, checked(request.ExpectedRevision + 1), target,
            request.TypeDefault, nextCandidate, review,
            request.Action == 개체시각대응Action.SaveDraft ? null : actor, clock.GetUtcNow().UtcDateTime);
        var json = JsonSerializer.Serialize(next);
        if (row is null)
        {
            row = new() { BindingId = request.BindingId, ContextHash = context, Kind = target.Kind, AccessScope = target.AccessScope };
            db.Bindings.Add(row);
        }
        row.Revision = next.Revision;
        row.ReviewState = review;
        row.StateJson = json;
        row.AssetVersionId = nextCandidate?.AssetVersionId;
        row.SourceKey = target.SourceKey;
        row.SourceStableId = target.StableId;
        row.SourceRevision = target.Revision;
        row.StateCode = target.StateCode;
        row.Purpose = target.Purpose;
        row.Representation = target.Representation;
        row.TypeDefault = next.TypeDefault;
        db.History.Add(new 개체시각대응이력
        {
            RequestKeyHash = key, RequestHash = requestHash, BindingId = request.BindingId,
            Revision = next.Revision, ReviewerId = actor, Action = request.Action.ToString(), Note = request.Note,
            AtUtc = next.UpdatedAtUtc, StateJson = json
        });
        try
        {
            // 관계형 SaveChanges 한 트랜잭션: 현재 상태·감사·멱등 결과가 함께 성공/실패한다.
            await db.SaveChangesAsync(ct);
            return new(true, "Persisted", next);
        }
        catch (DbUpdateConcurrencyException) { db.ChangeTracker.Clear(); return new(false, "RevisionConflict"); }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            // DB 실패를 성공/샘플/미연결로 숨기지 않는다. 커밋 응답 소실이면 같은 키로 재조회 가능하다.
            return new(false, "StorageWriteFailedOrConflict");
        }
    }

    public async Task<개체시각선택Result> ResolveAsync(ClaimsPrincipal user, 개체시각대상Query query, CancellationToken ct)
    {
        var access = await AccessAsync(user);
        if (access is not null) return new(access);
        var read = await source.ReadAsync(query, ct);
        if (!Valid(read)) return new(read.Target is null ? read.Diagnostic : "InvalidSource");
        var bindings = await LoadAsync(read.Target!, ct);
        return 개체시각선택Policy.Select(read.Target!, bindings, catalog);
    }

    public async Task<개체시각대응목록Result> ListAsync(ClaimsPrincipal user, 개체시각대상Query query, CancellationToken ct)
    {
        var access = await AccessAsync(user, review: true);
        if (access is not null) return new(access, []);
        var read = await source.ReadAsync(query, ct);
        if (!Valid(read)) return new(read.Target is null ? read.Diagnostic : "InvalidSource", []);
        return new("Found", await LoadAsync(read.Target!, ct));
    }

    public async Task<개체시각이력Result> HistoryAsync(ClaimsPrincipal user, string bindingId,
        개체시각대상Query query, CancellationToken ct)
    {
        var access = await AccessAsync(user, review: true);
        if (access is not null) return new(access, []);
        var read = await source.ReadAsync(query, ct);
        if (!Valid(read)) return new(read.Target is null ? read.Diagnostic : "InvalidSource", []);
        var row = await db.Bindings.AsNoTracking().SingleOrDefaultAsync(x => x.BindingId == bindingId, ct);
        if (row is null || row.BindingId != bindingId || !개체시각선택Policy.SameSubject(ParseRow(row), read.Target!)) return new("NotFound", []);
        return new("Found", await db.History.AsNoTracking().Where(x => x.BindingId == bindingId)
            .OrderByDescending(x => x.Revision).Take(100)
            .Select(x => new 개체시각이력Dto(x.Revision, x.Action, x.ReviewerId, x.Note, x.AtUtc)).ToArrayAsync(ct));
    }

    private async Task<개체시각대응Dto[]> LoadAsync(개체시각대상Dto t, CancellationToken ct)
    {
        var record = 개체시각선택Policy.Context(t, false);
        var type = 개체시각선택Policy.Context(t, true);
        var rows = await db.Bindings.AsNoTracking().Where(x => x.ContextHash == record || x.ContextHash == type).ToArrayAsync(ct);
        var states = rows.Select(ParseRow).ToArray();
        foreach (var candidate in states.Select(x => x.Candidate).Where(x => x?.AssetVersionId is not null))
        {
            var asset = await db.Assets.AsNoTracking().SingleOrDefaultAsync(x => x.AssetVersionId == candidate!.AssetVersionId, ct);
            if (asset is null || !개체시각목록UseCase.Matches(asset, candidate!))
                throw new InvalidOperationException("AssetReferenceIntegrityFailed");
        }
        return states;
    }
    private async Task<string?> AccessAsync(ClaimsPrincipal user, bool review = false)
    {
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(user.FindFirstValue(ClaimTypes.NameIdentifier)))
            return "Unauthorized";
        if (user.FindFirstValue(ClaimTypes.NameIdentifier) != currentUser.UserId) return "PrincipalMismatch";
        if (!(await authorization.AuthorizeAsync(user, null, 개체시각대응Codes.Policy)).Succeeded) return "Forbidden";
        return options.CurrentValue.Enabled || (review && options.CurrentValue.ReviewEnabled) ? null : "FeatureDisabled";
    }
    private static bool Valid(개체시각대상ReadResult read) => read.Diagnostic == "Found" && read.Target is { } t &&
        new[] { t.Kind, t.StableId, t.SourceKey, t.AccessScope, t.Revision, t.StateCode, t.Purpose, t.Representation }
            .All(x => !string.IsNullOrWhiteSpace(x) && x.Length <= 160);
    private static bool Token(string? value, int max) => !string.IsNullOrWhiteSpace(value) && value.Length <= max &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or ':' or '.');
    private static bool CandidateShape(개체시각후보Dto c) => new[] { c.VisualKey, c.CatalogRevision, c.CatalogFingerprint,
        c.AssetFingerprint, c.Fitness, c.EvidenceRef, c.EvidenceFingerprint }.All(x => !string.IsNullOrWhiteSpace(x) && x.Length <= 512) &&
        Token(c.VisualKey, 160) && 개체시각자산Catalog.EvidenceReference(c.EvidenceRef) &&
        (c.AssetVersionId is null || (c.AssetVersionId.Length == 64 && c.AssetVersionId.All(char.IsAsciiHexDigit)));
    private static 개체시각대응Dto Parse(string json) => JsonSerializer.Deserialize<개체시각대응Dto>(json)
        ?? throw new InvalidOperationException("VisualBindingStorageInvalid");
    private static 개체시각대응Dto ParseRow(개체시각대응 row)
    {
        var state = Parse(row.StateJson);
        if (row.BindingId != state.BindingId || row.Revision != state.Revision || row.Kind != state.Target.Kind ||
            row.AccessScope != state.Target.AccessScope || row.ReviewState != state.ReviewState ||
            row.ContextHash != 개체시각선택Policy.Context(state.Target, state.TypeDefault))
            throw new InvalidOperationException("VisualBindingStorageIntegrityFailed");
        if (row.AssetVersionId != state.Candidate?.AssetVersionId ||
            (row.SourceKey is not null && (row.SourceKey != state.Target.SourceKey || row.SourceStableId != state.Target.StableId ||
            row.SourceRevision != state.Target.Revision || row.StateCode != state.Target.StateCode ||
            row.Purpose != state.Target.Purpose || row.Representation != state.Target.Representation || row.TypeDefault != state.TypeDefault)) ||
            (row.SourceKey is null && (row.AssetVersionId is not null || row.SourceStableId is not null || row.SourceRevision is not null || row.StateCode is not null ||
                row.Purpose is not null || row.Representation is not null || row.TypeDefault is not null)))
            throw new InvalidOperationException("VisualBindingRelationIntegrityFailed");
        return state;
    }
}
