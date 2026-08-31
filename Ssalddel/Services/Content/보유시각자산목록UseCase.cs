using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Domain.Content;
using Ssalddel.Infrastructure.Persistence.WorldProjection;

namespace Ssalddel.Services.Content;

/// <summary>키 없는 보유파일 조사 사본도 기존 관리 경계에 보관한다. 후보 선정/원본 쓰기/Unity API는 없다.</summary>
public sealed class 보유시각자산목록UseCase(개체시각대응DbContext db, IAuthorizationService authorization,
    ICurrentUserAccessor currentUser, IOptionsMonitor<개체시각자산Options> options, TimeProvider clock)
{
    public async Task<보유시각자산반입Result> ImportAsync(ClaimsPrincipal user, 보유시각자산반입Request request, CancellationToken ct)
    {
        var access = await Access(user); if (access is not null) return new(access);
        if (request is null || !개체시각자산Catalog.EvidenceReference(request.EvidenceRef) || !Hash(request.EvidenceHash) ||
            request.Items is null || request.Items.Count is < 1 or > 128 || request.Items.Any(x => !Valid(x))) return new("InvalidInventoryInput");
        if (request.Items.Select(x => x.Guid).Distinct(StringComparer.Ordinal).Count() != request.Items.Count ||
            request.Items.Select(x => x.RelativePath).Distinct(StringComparer.Ordinal).Count() != request.Items.Count) return new("DuplicateInventoryInput");
        var settings = options.CurrentValue;
        if (string.IsNullOrWhiteSpace(settings.UnitySourceRoot) || string.IsNullOrWhiteSpace(settings.EvidenceRoot)) return new("AssetSourceRootMissing");
        var items = request.Items.Select(x => x with { ExistingCandidateIds = x.ExistingCandidateIds.Order(StringComparer.Ordinal).ToArray() }).ToArray();
        var observations = new List<(string Path, long Bytes, long Ticks)>();
        try
        {
            var evidence = FilePath(settings.EvidenceRoot, request.EvidenceRef, "docs/");
            if (!evidence.EndsWith(".md", StringComparison.Ordinal) || Observe(evidence, observations, 4 * 1024 * 1024) != request.EvidenceHash) return new("InventoryEvidenceChanged");
            foreach (var item in items)
            {
                ct.ThrowIfCancellationRequested();
                var path = FilePath(settings.UnitySourceRoot, item.RelativePath, "Assets/Synty/" + item.SourceGroup + "/");
                var meta = FilePath(settings.UnitySourceRoot, item.RelativePath + ".meta", "Assets/Synty/" + item.SourceGroup + "/");
                if (Observe(path, observations, 1024L * 1024 * 1024) != item.AssetHash || Observe(meta, observations, 4 * 1024 * 1024) != item.MetaHash)
                    return new("InventoryFileDrift");
                var text = File.ReadAllText(meta);
                if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(meta))) != item.MetaHash) return new("InventoryFileDrift");
                var guids = Regex.Matches(text, @"(?m)^guid: ([a-f0-9]{32})\r?$");
                if (guids.Count != 1 || guids[0].Groups[1].Value != item.Guid) return new("InventoryGuidMismatch");
                if (item.OriginVersion is not null)
                {
                    var versions = Regex.Matches(text, @"(?m)^  packageVersion: ([^\r\n]+)\r?$");
                    if (versions.Count != 1 || versions[0].Groups[1].Value != item.OriginVersion) return new("InventoryOriginVersionMismatch");
                }
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { return new("InventoryFileReadOrPathRejected"); }
        var candidateIds = items.SelectMany(x => x.ExistingCandidateIds).Distinct().ToList();
        var candidates = await db.Assets.AsNoTracking().Where(x => candidateIds.Contains(x.AssetVersionId)).ToArrayAsync(ct);
        foreach (var item in items)
            foreach (var id in item.ExistingCandidateIds)
            {
                var candidate = candidates.SingleOrDefault(x => x.AssetVersionId == id);
                if (candidate is null) return new("InventoryCandidateMissing");
                var m = 개체시각목록UseCase.Parse(candidate).Metadata;
                if (m.PrefabGuid != item.Guid || m.PrefabPath != item.RelativePath || m.AssetFingerprint != item.AssetHash || m.MetaFingerprint != item.MetaHash)
                    return new("InventoryCandidateMismatch");
            }
        var ids = items.Select(Id).ToList(); var guidsInBatch = items.Select(x => x.Guid).ToList();
        var old = await db.InventorySnapshots.AsNoTracking().Where(x => ids.Contains(x.SnapshotId)).ToArrayAsync(ct);
        var knownGuids = await db.InventorySnapshots.AsNoTracking().Where(x => guidsInBatch.Contains(x.Guid)).Select(x => x.Guid).Distinct().ToArrayAsync(ct);
        var pending = new List<보유시각자산사본>(); var links = new List<보유시각자산후보연결>();
        var existing = 0; var firstSeen = 0;
        foreach (var item in items)
        {
            var row = old.SingleOrDefault(x => x.SnapshotId == Id(item));
            if (row is not null)
            {
                if (개체시각선택Policy.Hash(Parse(row).Metadata) != 개체시각선택Policy.Hash(item) || row.EvidenceRef != request.EvidenceRef || row.EvidenceHash != request.EvidenceHash) return new("InventoryRevisionConflict");
                var storedLinks = await db.InventoryLinks.AsNoTracking().Where(x => x.SnapshotId == row.SnapshotId).Select(x => x.AssetVersionId).ToArrayAsync(ct);
                if (!storedLinks.Order().SequenceEqual(item.ExistingCandidateIds.Order())) return new("InventoryLinkIntegrityFailed");
                existing++; continue;
            }
            if (!knownGuids.Contains(item.Guid)) firstSeen++;
            pending.Add(new() { SnapshotId = Id(item), Guid = item.Guid, SurveyRevision = item.SurveyRevision,
                ContentVersionId = ContentId(item), SourceGroup = item.SourceGroup, PackCode = item.PackCode, AssetKind = item.AssetKind,
                Name = item.Name, RelativePath = item.RelativePath, MetadataJson = JsonSerializer.Serialize(item), MetadataHash = 개체시각선택Policy.Hash(item),
                EvidenceRef = request.EvidenceRef, EvidenceHash = request.EvidenceHash,
                RegisteredBy = currentUser.UserId!, RegisteredAtUtc = clock.GetUtcNow().UtcDateTime });
            links.AddRange(item.ExistingCandidateIds.Select(id => new 보유시각자산후보연결 { SnapshotId = Id(item), AssetVersionId = id }));
        }
        // 관측 중 파일을 잠가 읽었으며 저장 직전 길이/mtime를 재검사한다. 이후 변경은 새 조사판본 책임이다.
        foreach (var observed in observations)
        {
            var file = new FileInfo(observed.Path);
            if (!file.Exists || file.Length != observed.Bytes || file.LastWriteTimeUtc.Ticks != observed.Ticks) return new("InventoryFileDrift");
        }
        db.InventorySnapshots.AddRange(pending); db.InventoryLinks.AddRange(links);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { db.ChangeTracker.Clear(); return new("InventoryStorageConflictOrFailure"); }
        return new("Persisted", pending.Count, existing, firstSeen, pending.Count - firstSeen);
    }

    public async Task<보유시각자산목록Result> ListAsync(ClaimsPrincipal user, string? group, string? kind,
        string? name, string? visualKey, string? revision, int skip, CancellationToken ct,
        string? taxonomyPath = null, string? classificationState = null, string? trait = null, string? taxonomyHash = null)
    {
        var access = await Access(user); if (access is not null) return new(access, 0, []);
        if (skip < 0 || new[] { group, kind, visualKey, revision }.Any(x => x is not null && !Token(x)) || name is { Length: > 160 })
            return new("InvalidInventoryQuery", 0, []);
        if (taxonomyPath is not null && !ValidTaxonomyPath(taxonomyPath) || taxonomyHash is not null && !Hash(taxonomyHash) ||
            classificationState is not null && classificationState is not ("CatalogMapped" or "Inferred" or "FileAndImageReviewed" or "Unclassified") ||
            trait is not null && !Token(trait)) return new("InvalidInventoryQuery", 0, []);
        var query = db.InventorySnapshots.AsNoTracking();
        if (group is not null) query = query.Where(x => x.SourceGroup == group || x.PackCode == group);
        if (kind is not null) query = query.Where(x => x.AssetKind == kind);
        if (name is not null) query = query.Where(x => x.Name.Contains(name));
        if (revision is not null) query = query.Where(x => x.SurveyRevision == revision);
        if (visualKey is not null) query = query.Where(x => db.InventoryLinks.Any(l => l.SnapshotId == x.SnapshotId &&
            db.Assets.Any(a => a.AssetVersionId == l.AssetVersionId && a.VisualKey == visualKey)));
        var annotations = db.InventoryClassifications.AsNoTracking();
        if (taxonomyHash is not null) annotations = annotations.Where(x => x.TaxonomyHash == taxonomyHash);
        if (classificationState == "Unclassified") query = query.Where(x => !annotations.Any(a => a.SnapshotId == x.SnapshotId));
        else if (taxonomyPath is not null || classificationState is not null || trait is not null || taxonomyHash is not null)
            query = query.Where(x => annotations.Any(a => a.SnapshotId == x.SnapshotId &&
                (taxonomyPath == null || a.TaxonomyPath == taxonomyPath || a.TaxonomyPath.StartsWith(taxonomyPath + "/")) &&
                (classificationState == null || a.State == classificationState) &&
                (trait == null || ("|" + a.Traits + "|").Contains("|" + trait + "|"))));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.SourceGroup).ThenBy(x => x.Guid).ThenBy(x => x.SurveyRevision).Skip(skip).Take(100).ToArrayAsync(ct);
        var ids = rows.Select(x => x.SnapshotId).ToList();
        var notes = await annotations.Where(x => ids.Contains(x.SnapshotId)).OrderBy(x => x.AnnotationId).ToArrayAsync(ct);
        return new("Found", total, rows.Select(Parse).ToArray(), notes.Select(ParseClassification).ToArray());
    }

    public async Task<보유시각분류Result> ImportClassificationsAsync(ClaimsPrincipal user, 보유시각분류반입Request request, CancellationToken ct)
    {
        var access = await Access(user); if (access is not null) return new(access);
        if (request?.Items is null || request.Items.Count is < 1 or > 128 || request.Items.Any(x => x is null ||
            !Hash(x.SnapshotId) || !Hash(x.ContentVersionId) || !Hash(x.TaxonomyHash) || !Hash(x.EvidenceHash) ||
            !Token(x.TaxonomyRevision) || !ValidTaxonomyPath(x.TaxonomyPath) || x.State is not ("CatalogMapped" or "Inferred" or "FileAndImageReviewed") ||
            x.FamilyId is null || x.FamilyId.Length > 256 || x.Traits is null || x.Traits.Length > 1000 ||
            x.Traits.Split('|', StringSplitOptions.RemoveEmptyEntries).Any(t => !Token(t)) || string.IsNullOrWhiteSpace(x.Rationale) || x.Rationale.Length > 4000))
            return new("InvalidClassificationInput");
        var inputs = request.Items.ToArray();
        if (inputs.Select(ClassificationId).Distinct().Count() != inputs.Length) return new("DuplicateClassificationInput");
        var pending = new List<보유시각분류주석>(); var existing = 0;
        try
        {
            foreach (var input in inputs)
            {
                var row = await db.InventorySnapshots.AsNoTracking().SingleOrDefaultAsync(x => x.SnapshotId == input.SnapshotId, ct);
                if (row is null) return new("InventorySnapshotNotFound");
                var asset = Parse(row);
                if (asset.ContentVersionId != input.ContentVersionId) return new("ClassificationContentMismatch");
                var taxonomy = 시각자산근거검사.Read(options.CurrentValue, new("Repository", 시각자산근거검사.TaxonomyRef, input.TaxonomyHash));
                if (!시각자산근거검사.TaxonomyPaths(taxonomy, input.TaxonomyRevision).Contains(input.TaxonomyPath)) return new("TaxonomyPathUnknown");
                시각자산근거검사.Read(options.CurrentValue, new("Repository", input.EvidenceRef, input.EvidenceHash));
                if (input.State == "CatalogMapped")
                {
                    // 대장에 직접 존재하는 기능군까지만 확정 대응. 이름에서 말단을 자동 확정하지 않는다.
                    if (input.TaxonomyPath.Split('/').Length != 2 || input.Traits.Length != 0 ||
                        input.EvidenceRef != asset.EvidenceRef || input.EvidenceHash != asset.EvidenceHash) return new("ClassificationSourceMismatch");
                    using var evidence = JsonDocument.Parse(asset.Metadata.EvidenceJson);
                    if (!evidence.RootElement.GetProperty("existingModuleEntries").EnumerateArray().Any(e =>
                        e.GetProperty("assetFamilyId").GetString() == input.FamilyId &&
                        e.GetProperty("moduleCodes").EnumerateArray().Any(m => m.GetString() == input.TaxonomyPath.Split('/')[1])))
                        return new("ClassificationSourceMismatch");
                }
                else
                {
                    // 추가 조사 주석은 exact 사본과 전체 입력(자기 참조 제외)을 담은 검토 파일로 결속한다.
                    var declared = JsonSerializer.SerializeToNode(input)!;
                    declared.AsObject().Remove("EvidenceRef"); declared.AsObject().Remove("EvidenceHash");
                    if (!System.Text.Json.Nodes.JsonNode.DeepEquals(declared, System.Text.Json.Nodes.JsonNode.Parse(
                        시각자산근거검사.Read(options.CurrentValue, new("Repository", input.EvidenceRef, input.EvidenceHash)))))
                        return new("ClassificationReviewMismatch");
                }
                var id = ClassificationId(input);
                var old = await db.InventoryClassifications.AsNoTracking().SingleOrDefaultAsync(x => x.AnnotationId == id, ct);
                if (old is not null)
                {
                    if (개체시각선택Policy.Hash(ParseClassification(old)) != 개체시각선택Policy.Hash(input)) return new("ClassificationRevisionConflict");
                    existing++; continue;
                }
                pending.Add(new() { AnnotationId = id, SnapshotId = input.SnapshotId, ContentVersionId = input.ContentVersionId,
                    TaxonomyHash = input.TaxonomyHash, TaxonomyPath = input.TaxonomyPath, State = input.State, Traits = input.Traits,
                    InputJson = JsonSerializer.Serialize(input), InputHash = 개체시각선택Policy.Hash(input),
                    RegisteredBy = currentUser.UserId!, RegisteredAtUtc = clock.GetUtcNow().UtcDateTime });
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or KeyNotFoundException or ArgumentException or InvalidOperationException)
        { return new("ClassificationEvidenceReadOrDrift"); }
        db.InventoryClassifications.AddRange(pending);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { db.ChangeTracker.Clear(); return new("ClassificationStorageConflictOrFailure"); }
        return new("Persisted", pending.Count, existing);
    }

    private static bool ValidTaxonomyPath(string? s) => s is not null && s.Length <= 320 &&
        s.Split('/').Length is 2 or 4 && s.Split('/').All(Token);
    public static string ClassificationId(보유시각분류Input input) => 개체시각선택Policy.Hash(new { input.SnapshotId, input.TaxonomyHash, input.TaxonomyPath });
    private static 보유시각분류Input ParseClassification(보유시각분류주석 row)
    {
        var input = JsonSerializer.Deserialize<보유시각분류Input>(row.InputJson);
        if (input is null || row.AnnotationId != ClassificationId(input) || row.InputHash != 개체시각선택Policy.Hash(input) ||
            row.SnapshotId != input.SnapshotId || row.ContentVersionId != input.ContentVersionId || row.TaxonomyHash != input.TaxonomyHash ||
            row.TaxonomyPath != input.TaxonomyPath || row.State != input.State || row.Traits != input.Traits) throw new InvalidOperationException("ClassificationIntegrityFailed");
        return input;
    }

    public static string Id(보유시각자산Input m) => 개체시각선택Policy.Hash(new { Provider = "Synty", m.Guid, m.SurveyRevision });
    public static string ContentId(보유시각자산Input m) => 개체시각선택Policy.Hash(new { m.Guid, m.AssetHash, m.MetaHash });
    public static 보유시각자산Dto Parse(보유시각자산사본 row)
    {
        var m = JsonSerializer.Deserialize<보유시각자산Input>(row.MetadataJson) ?? throw new InvalidOperationException("InventoryStorageInvalid");
        if (row.SnapshotId != Id(m) || row.ContentVersionId != ContentId(m) || row.MetadataHash != 개체시각선택Policy.Hash(m) ||
            row.Guid != m.Guid || row.SurveyRevision != m.SurveyRevision || row.SourceGroup != m.SourceGroup || row.PackCode != m.PackCode ||
            row.AssetKind != m.AssetKind || row.Name != m.Name || row.RelativePath != m.RelativePath) throw new InvalidOperationException("InventoryStorageIntegrityFailed");
        return new(row.SnapshotId, row.ContentVersionId, m, "Unreviewed", "NotSelected_NotInstantiated", "StoredSurveySnapshot_NotLiveFileCheck", row.RegisteredAtUtc, row.EvidenceRef, row.EvidenceHash);
    }
    private static bool Valid(보유시각자산Input? m)
    {
        if (m is null || !Token(m.SurveyRevision) || !Token(m.SourceGroup) || m.PackCode is not null && !Token(m.PackCode) ||
            !Hash(m.InputFileHash) || !Hash(m.AssetHash) || !Hash(m.MetaHash) || m.Guid is not { Length: 32 } || !Regex.IsMatch(m.Guid, "^[a-f0-9]{32}$") ||
            string.IsNullOrWhiteSpace(m.Name) || m.Name.Length > 256 || string.IsNullOrWhiteSpace(m.RelativePath) ||
            m.RelativePath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) || m.AssetKind != Kind(Path.GetExtension(m.RelativePath).ToLowerInvariant()) ||
            m.OriginVersion is { Length: > 80 } || m.ExistingCandidateIds is null || m.ExistingCandidateIds.Count > 32 ||
            m.ExistingCandidateIds.Any(x => !Hash(x)) || m.ExistingCandidateIds.Distinct().Count() != m.ExistingCandidateIds.Count ||
            string.IsNullOrWhiteSpace(m.EvidenceJson) || Encoding.UTF8.GetByteCount(m.EvidenceJson) > 65536) return false;
        try
        {
            using var evidence = JsonDocument.Parse(m.EvidenceJson); var e = evidence.RootElement;
            return e.ValueKind == JsonValueKind.Object && e.GetProperty("guid").GetString() == m.Guid &&
                e.GetProperty("relativePath").GetString() == m.RelativePath && e.GetProperty("assetHash").GetString() == m.AssetHash &&
                e.GetProperty("metaHash").GetString() == m.MetaHash && e.GetProperty("assetKind").GetString() == m.AssetKind &&
                e.GetProperty("sourceGroup").GetString() == m.SourceGroup && e.GetProperty("surveyRevision").GetString() == m.SurveyRevision;
        }
        catch (Exception e) when (e is JsonException or KeyNotFoundException or InvalidOperationException) { return false; }
    }
    public static string Kind(string extension) => extension switch
    {
        ".prefab" => "Prefab", ".fbx" or ".obj" => "ModelFile", ".mat" => "Material",
        ".png" or ".jpg" or ".jpeg" or ".tga" or ".tif" or ".tiff" or ".psd" or ".exr" => "TextureFile",
        ".anim" => "AnimationClipFile", ".controller" => "AnimatorController", ".overridecontroller" => "AnimatorOverrideController",
        ".unity" => "DemoScene", ".asset" => "UnitySerializedAsset", ".playable" => "TimelinePlayable", ".mask" => "AvatarMask",
        ".shader" or ".shadergraph" or ".shadersubgraph" or ".cginc" => "Shader", ".cs" => "Script",
        ".md" or ".pdf" or ".txt" => "Documentation", ".terrainlayer" => "TerrainLayer", ".lighting" => "LightingData",
        ".preset" => "ImportPreset", ".inputactions" => "InputActions", _ => "OtherFile"
    };
    private static string FilePath(string root, string relative, string prefix)
    {
        if (relative.Length > 512 || !relative.StartsWith(prefix, StringComparison.Ordinal) || relative.Contains('\\') || relative.Contains(':') ||
            relative.Split('/').Any(x => x is "" or "." or "..")) throw new IOException("InventoryPathRejected");
        var full = Path.GetFullPath(Path.Combine(root, relative)); var f = new FileInfo(full);
        if (!f.Exists || (f.Attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("InventoryFileRejected");
        for (var p = f.Directory; p is not null; p = p.Parent) if ((p.Attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("InventoryReparseRejected");
        return full;
    }
    private static string Observe(string path, List<(string, long, long)> observations, long max)
    {
        var f = new FileInfo(path); if (f.Length > max) throw new IOException("InventoryFileTooLarge");
        var length = f.Length; var ticks = f.LastWriteTimeUtc.Ticks;
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = Convert.ToHexString(SHA256.HashData(stream)); f.Refresh();
        if (f.Length != length || f.LastWriteTimeUtc.Ticks != ticks) throw new IOException("InventoryReadChanged");
        observations.Add((path, length, ticks)); return hash;
    }
    private static bool Token(string? s) => !string.IsNullOrWhiteSpace(s) && s.Length <= 160 && s.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or ':' or '_' or '-');
    private static bool Hash(string? s) => 시각자산근거검사.Hash(s);
    private async Task<string?> Access(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(user.FindFirstValue(ClaimTypes.NameIdentifier))) return "Unauthorized";
        if (user.FindFirstValue(ClaimTypes.NameIdentifier) != currentUser.UserId) return "PrincipalMismatch";
        if (!(await authorization.AuthorizeAsync(user, null, 개체시각대응Codes.Policy)).Succeeded) return "Forbidden";
        return options.CurrentValue.ReviewEnabled || options.CurrentValue.Enabled ? null : "FeatureDisabled";
    }
}
