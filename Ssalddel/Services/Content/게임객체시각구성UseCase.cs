using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Domain.Content;
using Ssalddel.Infrastructure.Persistence.WorldProjection;

namespace Ssalddel.Services.Content;

/// <summary>편집 정의·불변 초안 구성만 저장한다. 기존 단일 대응 선택이나 실제 World를 변경하지 않는다.</summary>
public sealed class 게임객체시각구성UseCase(개체시각대응DbContext db, IAuthorizationService authorization,
    ICurrentUserAccessor currentUser, IOptionsMonitor<개체시각자산Options> options, TimeProvider clock)
{
    public async Task<게임객체시각구성Result> SaveAsync(ClaimsPrincipal user, 게임객체시각구성Request request, CancellationToken ct)
    {
        var access = await Access(user);
        if (access is not null) return new(access);
        if (!Valid(request)) return new("InvalidComposition");
        // 입력 컬렉션을 변경하지 않고 고정한 사본을 검증·해시·저장에 공통 사용한다.
        request = request with { Definition = request.Definition with
            { Items = request.Definition.Items.OrderBy(x => x.ItemId, StringComparer.Ordinal).ToArray() } };
        var input = request.Definition;
        var key = 개체시각선택Policy.Hash(new { input.DefinitionId, request.RequestId });
        var hash = 개체시각선택Policy.Hash(request);
        var journal = await db.CompositionHistory.AsNoTracking().SingleOrDefaultAsync(x => x.RequestKeyHash == key, ct);
        if (journal is not null)
        {
            var previous = await Read(journal.CompositionId, ct);
            return journal.RequestHash == hash && previous.ReviewerId == currentUser.UserId
                ? new("Persisted", previous, true) : new("IdempotencyConflict");
        }
        // 모든 자산 참조를 확인한 뒤에 변경한다. 미선정(null)과 손상된 참조는 구분한다.
        foreach (var id in input.Items.Select(x => x.AssetVersionId).OfType<string>().Distinct(StringComparer.Ordinal))
        {
            var asset = await db.Assets.AsNoTracking().SingleOrDefaultAsync(x => x.AssetVersionId == id, ct);
            if (asset is null) return new("AssetVersionNotFound");
            _ = 개체시각목록UseCase.Parse(asset);
        }
        var head = await db.Definitions.SingleOrDefaultAsync(x => x.DefinitionId == input.DefinitionId, ct);
        if (head is not null && head.DefinitionId != input.DefinitionId) return new("DefinitionIdentityMismatch");
        if ((head?.Revision ?? 0) != request.ExpectedRevision) return new("RevisionConflict");
        var automatic = input.Items.Any(x => x.SelectionEvidenceJson is not null);
        게임객체시각구성Dto? before = null;
        if (automatic)
        {
            if (head is null) return new("AutomaticExistingDefinitionRequired");
            before = await Read(Id(head.DefinitionId, head.Revision), ct);
            if (input.DefinitionRevision != before.Definition.DefinitionRevision || input.DisplayName != before.Definition.DisplayName ||
                input.EvidenceRef != before.Definition.EvidenceRef || input.EvidenceFingerprint != before.Definition.EvidenceFingerprint)
                return new("AutomaticDefinitionChanged");
            foreach (var old in before.Definition.Items)
            {
                var next = input.Items.SingleOrDefault(x => x.ItemId == old.ItemId);
                if (next is null || next.Role != old.Role || next.SlotKey != old.SlotKey || next.AnchorIntent != old.AnchorIntent ||
                    ((old.AssetVersionId is not null || old.InventorySnapshotId is not null) && next != old)) return new("ExistingSelectionProtected");
            }
        }
        var newSelections = 0;
        foreach (var item in input.Items.Where(x => x.InventorySnapshotId is not null))
        {
            var row = await db.InventorySnapshots.AsNoTracking().SingleOrDefaultAsync(x => x.SnapshotId == item.InventorySnapshotId, ct);
            if (row is null) return new("InventorySnapshotNotFound");
            var asset = 보유시각자산목록UseCase.Parse(row);
            // 기존 선택의 재보관은 그대로 보존; 새 선택에만 현재 근거 관문을 적용한다.
            if (before?.Definition.Items.Any(x => x == item) == true) continue;
            if (++newSelections > 10) return new("AutomaticBatchLimit");
            if (item.SelectionEvidenceJson is null) return new("AutomaticEvidenceRequired");
            var issue = 시각자산근거검사.Selection(options.CurrentValue, input, item, asset);
            if (issue is not null) return new(issue);
        }
        if (automatic && input.Items.Any(x => x.AssetVersionId is not null && before!.Definition.Items.All(old => old != x)))
            return new("AutomaticLegacySelectionNotSupported");
        var revision = request.ExpectedRevision + 1;
        if (head is null) { head = new() { DefinitionId = input.DefinitionId }; db.Definitions.Add(head); }
        head.Revision = revision;
        var compositionId = Id(input.DefinitionId, revision);
        db.Compositions.Add(new() { CompositionId = compositionId, DefinitionId = input.DefinitionId, Revision = revision,
            SnapshotJson = JsonSerializer.Serialize(input), SnapshotHash = 개체시각선택Policy.Hash(input),
            ReviewerId = currentUser.UserId!, AtUtc = clock.GetUtcNow().UtcDateTime });
        db.CompositionItems.AddRange(input.Items.Select(x => new 게임객체시각구성항목
        { CompositionId = compositionId, ItemId = x.ItemId, Role = x.Role, SlotKey = x.SlotKey,
            AssetVersionId = x.AssetVersionId, AnchorIntent = x.AnchorIntent,
            InventorySnapshotId = x.InventorySnapshotId, SelectionEvidenceJson = x.SelectionEvidenceJson }));
        db.CompositionHistory.Add(new() { RequestKeyHash = key, RequestHash = hash, CompositionId = compositionId });
        // EF 한 SaveChanges 트랜잭션에 포인터/새 판본/항목/이력을 넣는다. 과거 판본 수정·삭제는 없다.
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { db.ChangeTracker.Clear(); return new("RevisionConflict"); }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            // 공급자에 따라 새 판본의 UNIQUE 실패가 포인터 동시성 실패보다 먼저 발생한다.
            var observed = await db.Definitions.AsNoTracking().SingleOrDefaultAsync(x => x.DefinitionId == input.DefinitionId, ct);
            return new((observed?.Revision ?? 0) != request.ExpectedRevision ? "RevisionConflict" : "CompositionStorageConflictOrFailure");
        }
        return new("Persisted", await Read(compositionId, ct));
    }

    public async Task<게임객체시각구성Result> GetAsync(ClaimsPrincipal user, string definitionId, long? revision, CancellationToken ct)
    {
        var access = await Access(user);
        if (access is not null) return new(access);
        if (!Token(definitionId, 120) || revision is <= 0) return new("InvalidCompositionQuery");
        var head = await db.Definitions.AsNoTracking().SingleOrDefaultAsync(x => x.DefinitionId == definitionId, ct);
        if (head is null) return new("NotFound");
        var id = Id(definitionId, revision ?? head.Revision);
        if (!await db.Compositions.AnyAsync(x => x.CompositionId == id, ct))
        {
            if (revision is null) throw new InvalidOperationException("CompositionHeadIntegrityFailed");
            return new("NotFound");
        }
        return new("Found", await Read(id, ct));
    }

    public async Task<게임객체시각구성목록Result> ListAsync(ClaimsPrincipal user, int skip, CancellationToken ct)
    {
        var access = await Access(user);
        if (access is not null) return new(access, []);
        if (skip < 0) return new("InvalidCompositionQuery", []);
        var heads = await db.Definitions.AsNoTracking().OrderBy(x => x.DefinitionId).Skip(skip).Take(100).ToArrayAsync(ct);
        var result = new List<게임객체시각구성Dto>();
        foreach (var h in heads) result.Add(await Read(Id(h.DefinitionId, h.Revision), ct));
        return new("Found", result);
    }

    private async Task<게임객체시각구성Dto> Read(string id, CancellationToken ct)
    {
        var c = await db.Compositions.AsNoTracking().SingleAsync(x => x.CompositionId == id, ct);
        var input = JsonSerializer.Deserialize<게임객체시각구성Input>(c.SnapshotJson);
        if (input is null || !Valid(new("storage", 0, input)) || c.DefinitionId != input.DefinitionId ||
            c.Revision < 1 || c.CompositionId != Id(c.DefinitionId, c.Revision) || c.SnapshotHash != 개체시각선택Policy.Hash(input))
            throw new InvalidOperationException("CompositionStorageIntegrityFailed");
        var rows = await db.CompositionItems.AsNoTracking().Where(x => x.CompositionId == id).ToArrayAsync(ct);
        var values = rows.OrderBy(x => x.ItemId, StringComparer.Ordinal).Select(x =>
            new 게임객체시각항목Input(x.ItemId, x.Role, x.SlotKey, x.AssetVersionId, x.AnchorIntent, x.InventorySnapshotId, x.SelectionEvidenceJson)).ToArray();
        if (!values.SequenceEqual(input.Items)) throw new InvalidOperationException("CompositionItemsIntegrityFailed");
        var items = new List<게임객체시각항목Dto>();
        foreach (var item in values)
        {
            var asset = item.AssetVersionId is null ? null : 개체시각목록UseCase.Parse(
                await db.Assets.AsNoTracking().SingleAsync(x => x.AssetVersionId == item.AssetVersionId, ct));
            items.Add(new(item, asset, item.InventorySnapshotId is not null ? "AutomaticDraft_NotApplied" : asset is null ? "Unselected" : "Candidate_FitnessUnreviewed",
                item.SelectionEvidenceJson is not null ? "DeclaredExactPrefabEvidence_RecheckOnChange" : "NotObserved"));
        }
        return new(id, c.Revision, input, "EditableDefinition_NotWorldInstance", "Draft", "NotApplied",
            c.ReviewerId, c.AtUtc, items);
    }

    public static string Id(string definitionId, long revision) => 개체시각선택Policy.Hash(new { definitionId, revision });
    private async Task<string?> Access(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(user.FindFirstValue(ClaimTypes.NameIdentifier))) return "Unauthorized";
        if (user.FindFirstValue(ClaimTypes.NameIdentifier) != currentUser.UserId) return "PrincipalMismatch";
        if (!(await authorization.AuthorizeAsync(user, null, 개체시각대응Codes.Policy)).Succeeded) return "Forbidden";
        return options.CurrentValue.Enabled || options.CurrentValue.ReviewEnabled ? null : "FeatureDisabled";
    }
    private static bool Token(string? text, int max) => !string.IsNullOrEmpty(text) && text.Length <= max &&
        text.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_' or ':');
    private static bool Valid(게임객체시각구성Request? r)
    {
        if (r?.Definition is not { } d || !Token(r.RequestId, 120) || r.ExpectedRevision < 0 || r.ExpectedRevision == long.MaxValue ||
            !Token(d.DefinitionId, 120) || !Token(d.DefinitionRevision, 160) || string.IsNullOrWhiteSpace(d.DisplayName) || d.DisplayName.Length > 160 ||
            !개체시각자산Catalog.EvidenceReference(d.EvidenceRef) || d.EvidenceFingerprint is not { Length: 64 } ||
            !d.EvidenceFingerprint.All(c => char.IsAsciiHexDigit(c) && !char.IsLower(c)) || d.Items is null || d.Items.Count > 64) return false;
        if (d.Items.Any(x => x is null || !Token(x.ItemId, 80) || !Token(x.Role, 80) || !Token(x.SlotKey, 80) ||
            (x.AssetVersionId is not null && (x.AssetVersionId.Length != 64 || !x.AssetVersionId.All(c => char.IsAsciiHexDigit(c) && !char.IsLower(c)))) ||
            (x.AnchorIntent is not null && (string.IsNullOrWhiteSpace(x.AnchorIntent) || x.AnchorIntent.Length > 160)) ||
            (x.InventorySnapshotId is not null && (!시각자산근거검사.Hash(x.InventorySnapshotId) || x.AssetVersionId is not null)) ||
            (x.SelectionEvidenceJson is not null && (x.InventorySnapshotId is null || x.SelectionEvidenceJson.Length > 65536)))) return false;
        return d.Items.Select(x => x.ItemId).Distinct(StringComparer.Ordinal).Count() == d.Items.Count &&
            d.Items.Select(x => (x.Role, x.SlotKey)).Distinct().Count() == d.Items.Count;
    }
}
