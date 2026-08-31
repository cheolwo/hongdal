using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Domain.Content;
using Ssalddel.Infrastructure.Persistence.WorldProjection;

namespace Ssalddel.Services.Content;

/// <summary>고정된 기존 WI 원문의 검토 입력을 가져온다. 자동 명사 추출/게임 실행/임의 파일 API가 아니다.</summary>
public sealed class 게임객체WI참여UseCase(개체시각대응DbContext db, 게임객체시각구성UseCase definitions,
    IAuthorizationService authorization, ICurrentUserAccessor currentUser,
    IOptionsMonitor<개체시각자산Options> options, TimeProvider clock)
{
    public const string SourceRef = "eng/execution-ledgers/world-interactions.json";
    public async Task<게임객체WI추출Result> ImportAsync(ClaimsPrincipal user, 게임객체WI추출Request r, CancellationToken ct)
    {
        var access = await Access(user); if (access is not null) return new(access);
        if (r is null || !Token(r.RequestId) || r.SourceRef != SourceRef || r.Definitions is null || r.Relations is null ||
            r.Definitions.Count > 32 || r.Relations.Count is < 1 or > 64 || r.Definitions.Any(x => x is null || x.Items is null || x.Items.Count != 0) ||
            r.Relations.Any(x => x is null || !Token(x.WorldInteractionId) || !Token(x.ContextKey) ||
                (x.DefinitionId is not null && !Token(x.DefinitionId)) ||
                !(new[] { "Actor", "Target", "Tool", "Input", "Result", "Place", "Condition" }).Contains(x.Role) ||
                !(new[] { "Actor", "Physical", "Information", "Unresolved", "NotObject" }).Contains(x.ObjectKind) ||
                !(new[] { "DirectMention", "ExistingDefinitionReuse", "InterpretationCandidate", "NonObject", "Unresolved" }).Contains(x.ExtractionState) ||
                string.IsNullOrWhiteSpace(x.ContextNote) || x.ContextNote.Length > 1500 || string.IsNullOrWhiteSpace(x.ExactQuote) || x.ExactQuote.Length > 3000 ||
                (x.ObjectKind == "NotObject" ? x.DefinitionId is not null || x.ExtractionState != "NonObject" : x.DefinitionId is null))) return new("InvalidExtraction");
        if (r.Definitions.Select(x => x.DefinitionId).Distinct(StringComparer.Ordinal).Count() != r.Definitions.Count ||
            r.Relations.Select(Id).Distinct(StringComparer.Ordinal).Count() != r.Relations.Count) return new("DuplicateExtractionInput");
        r = r with { Definitions = r.Definitions.OrderBy(x => x.DefinitionId, StringComparer.Ordinal).ToArray(),
            Relations = r.Relations.OrderBy(Id, StringComparer.Ordinal).ToArray() };
        // 재처리도 먼저 원문 신선도를 확인한다. 오래된 저장 사본은 조회하되 새 반입 성공으로 세지 않는다.
        var source = ReadSource(); if (source is null) return new("SourceUnavailable");
        using var document = source.Value.Document;
        var root = document.RootElement;
        if (source.Value.Hash != r.SourceHash || root.GetProperty("revision").GetString() != r.SourceRevision) return new("SourceDrift");
        var entries = root.GetProperty("items").EnumerateArray().ToArray();
        foreach (var relation in r.Relations)
        {
            var found = entries.Where(x => x.GetProperty("id").GetString() == relation.WorldInteractionId).ToArray();
            if (found.Length != 1) return new("UnknownWorldInteraction");
            if (found[0].GetProperty("ruleRevision").GetString() != relation.RuleRevision) return new("RuleRevisionMismatch");
            if (!(new[] { "worldAction", "actorRequirements", "resourceRequirements", "spatialRequirements", "taskRule", "controlPolicyCode", "startStateCodes", "completionStateCodes" }).Contains(relation.SourceField) ||
                !found[0].TryGetProperty(relation.SourceField, out var field) ||
                !(field.ValueKind == JsonValueKind.String ? field.GetString() == relation.ExactQuote :
                    field.ValueKind == JsonValueKind.Array && field.EnumerateArray().Any(x => x.ValueKind == JsonValueKind.String && x.GetString() == relation.ExactQuote))) return new("SourceQuoteMismatch");
        }
        if (r.Definitions.Any(d => !r.Relations.Any(x => x.DefinitionId == d.DefinitionId))) return new("UnreferencedDefinition");
        var batchId = 개체시각선택Policy.Hash(new { r.RequestId }); var requestHash = 개체시각선택Policy.Hash(r);
        var old = await db.WiBatches.AsNoTracking().SingleOrDefaultAsync(x => x.BatchId == batchId, ct);
        if (old is not null) return old.RequestHash == requestHash && old.ReviewerId == currentUser.UserId
            ? new("Persisted", Duplicate: true) : new("IdempotencyConflict");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var insertedDefinitions = 0; var insertedRelations = 0; var committed = false;
        try
        {
            foreach (var definition in r.Definitions)
            {
                var existing = await definitions.GetAsync(user, definition.DefinitionId, null, ct);
                if (existing.Diagnostic == "Found")
                {
                    if (개체시각선택Policy.Hash(existing.Composition!.Definition) != 개체시각선택Policy.Hash(definition)) return new("DefinitionConflict");
                }
                else if (existing.Diagnostic == "NotFound")
                {
                    var saved = await definitions.SaveAsync(user, new("wi:" + batchId, 0, definition), ct);
                    if (saved.Diagnostic != "Persisted") return new(saved.Diagnostic);
                    insertedDefinitions++;
                }
                else return new(existing.Diagnostic);
            }
            db.WiBatches.Add(new() { BatchId = batchId, RequestHash = requestHash, SourceHash = r.SourceHash,
                SourceRevision = r.SourceRevision, ReviewerId = currentUser.UserId!, AtUtc = clock.GetUtcNow().UtcDateTime });
            foreach (var relation in r.Relations)
            {
                string? version = null;
                if (relation.DefinitionId is not null)
                {
                    var d = await definitions.GetAsync(user, relation.DefinitionId, null, ct);
                    if (d.Diagnostic != "Found") return new("DefinitionNotFound");
                    version = d.Composition!.CompositionId;
                }
                var id = Id(relation); var hash = 개체시각선택Policy.Hash(relation);
                var row = await db.WiUses.AsNoTracking().SingleOrDefaultAsync(x => x.SourceHash == r.SourceHash && x.UseId == id, ct);
                if (row is not null)
                {
                    if (row.InputHash != hash || row.DefinitionCompositionId != version) return new("RelationConflict");
                    continue;
                }
                db.WiUses.Add(new() { SourceHash = r.SourceHash, UseId = id, BatchId = batchId,
                    WorldInteractionId = relation.WorldInteractionId, DefinitionId = relation.DefinitionId, DefinitionCompositionId = version,
                    Role = relation.Role, InputJson = JsonSerializer.Serialize(relation), InputHash = hash });
                insertedRelations++;
            }
            await db.SaveChangesAsync(ct);
            // 파일은 트랜잭션 자원이 아니다. 커밋 직전 다시 확인하되 이후 외부 변경은 조회 freshness로 드러낸다.
            var finalSource = ReadSource();
            if (finalSource is null) return new("SourceUnavailable");
            using (finalSource.Value.Document) { if (finalSource.Value.Hash != r.SourceHash) return new("SourceDrift"); }
            await transaction.CommitAsync(ct); committed = true;
            return new("Persisted", insertedDefinitions, insertedRelations);
        }
        catch (DbUpdateException) { return new("ExtractionStorageConflictOrFailure"); }
        finally { if (!committed) { await transaction.RollbackAsync(CancellationToken.None); db.ChangeTracker.Clear(); } }
    }

    public async Task<게임객체WI조회Result> ListAsync(ClaimsPrincipal user, string? wi, string? definitionId, int skip, CancellationToken ct)
    {
        var access = await Access(user); if (access is not null) return new(access, []);
        if (skip < 0 || wi is not null && !Token(wi) || definitionId is not null && !Token(definitionId)) return new("InvalidExtractionQuery", []);
        var source = ReadSource(); var hash = source?.Hash; source?.Document.Dispose();
        var query = db.WiUses.AsNoTracking(); if (wi is not null) query = query.Where(x => x.WorldInteractionId == wi);
        if (definitionId is not null) query = query.Where(x => x.DefinitionId == definitionId);
        var rows = await query.OrderBy(x => x.WorldInteractionId).ThenBy(x => x.UseId).ThenBy(x => x.SourceHash).Skip(skip).Take(100).ToArrayAsync(ct);
        var result = new List<게임객체WI참여Dto>();
        foreach (var row in rows)
        {
            var input = JsonSerializer.Deserialize<게임객체WI참여Input>(row.InputJson) ?? throw new InvalidOperationException("WiUseStorageInvalid");
            if (row.UseId != Id(input) || row.InputHash != 개체시각선택Policy.Hash(input) || row.DefinitionId != input.DefinitionId ||
                row.WorldInteractionId != input.WorldInteractionId || row.Role != input.Role) throw new InvalidOperationException("WiUseStorageIntegrityFailed");
            var batch = await db.WiBatches.AsNoTracking().SingleAsync(x => x.BatchId == row.BatchId, ct);
            if (batch.SourceHash != row.SourceHash) throw new InvalidOperationException("WiUseSourceIntegrityFailed");
            if (row.DefinitionId is not null && !await db.Compositions.AnyAsync(x => x.CompositionId == row.DefinitionCompositionId && x.DefinitionId == row.DefinitionId, ct))
                throw new InvalidOperationException("WiUseDefinitionIntegrityFailed");
            result.Add(new(row.UseId, batch.SourceRevision, row.SourceHash, hash is null ? "SourceUnavailable" : hash == row.SourceHash ? "CurrentFileSnapshot" : "ReviewRequired_SourceDrift",
                row.DefinitionCompositionId, input, "CatalogStatement_NotDesignApproval", "NotInstantiated"));
        }
        return new("Found", result);
    }

    public async Task<게임객체WI목록Result> InventoryAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var access = await Access(user); if (access is not null) return new(access, null, null, []);
        var source = ReadSource(); if (source is null) return new("SourceUnavailable", null, null, []);
        using var document = source.Value.Document; var hash = source.Value.Hash;
        var reviewed = await db.WiUses.AsNoTracking().Where(x => x.SourceHash == hash).Select(x => x.WorldInteractionId).Distinct().ToArrayAsync(ct);
        return new("Found", document.RootElement.GetProperty("revision").GetString(), hash,
            document.RootElement.GetProperty("items").EnumerateArray().Select(x => new 게임객체WI목록항목(
                x.GetProperty("id").GetString()!, x.GetProperty("title").GetString()!, x.GetProperty("groupCode").GetString()!,
                reviewed.Contains(x.GetProperty("id").GetString()) ? "PartialEvidenceRegistered_NotComplete" : "Unreviewed")).ToArray());
    }

    public static string Id(게임객체WI참여Input r) => 개체시각선택Policy.Hash(new { r.WorldInteractionId, r.DefinitionId, r.Role, r.ContextKey });
    private (JsonDocument Document, string Hash)? ReadSource()
    {
        try
        {
            var root = options.CurrentValue.EvidenceRoot; if (string.IsNullOrWhiteSpace(root)) return null;
            var file = new FileInfo(Path.Combine(Path.GetFullPath(root), SourceRef));
            if (!file.Exists || file.Length is < 1 or > 4 * 1024 * 1024 || (file.Attributes & FileAttributes.ReparsePoint) != 0) return null;
            for (var parent = file.Directory; parent is not null; parent = parent.Parent) if ((parent.Attributes & FileAttributes.ReparsePoint) != 0) return null;
            var bytes = File.ReadAllBytes(file.FullName); var document = JsonDocument.Parse(bytes);
            if (!document.RootElement.TryGetProperty("catalogKey", out var key) || key.GetString() != "simulation-world-interactions" ||
                !document.RootElement.TryGetProperty("revision", out _) || !document.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
            { document.Dispose(); return null; }
            var ids = items.EnumerateArray().Select(x => x.GetProperty("id").GetString()).ToArray();
            if (ids.Any(x => !Token(x)) || ids.Distinct(StringComparer.Ordinal).Count() != ids.Length) { document.Dispose(); return null; }
            return (document, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or ArgumentException or KeyNotFoundException or InvalidOperationException) { return null; }
    }
    private static bool Token(string? s) => !string.IsNullOrWhiteSpace(s) && s.Length <= 120 && s.All(c => char.IsAsciiLetterOrDigit(c) || c is ':' or '.' or '-' or '_');
    private async Task<string?> Access(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(user.FindFirstValue(ClaimTypes.NameIdentifier))) return "Unauthorized";
        if (user.FindFirstValue(ClaimTypes.NameIdentifier) != currentUser.UserId) return "PrincipalMismatch";
        if (!(await authorization.AuthorizeAsync(user, null, 개체시각대응Codes.Policy)).Succeeded) return "Forbidden";
        return options.CurrentValue.Enabled || options.CurrentValue.ReviewEnabled ? null : "FeatureDisabled";
    }
}
