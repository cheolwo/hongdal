using System.Security.Claims;
using System.Security.Cryptography;
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

/// <summary>기존 파일 대장의 제한된 메타데이터를 불변 DB 사본으로 만든다. 제품 선택용 Catalog는 대체하지 않는다.</summary>
public sealed class 개체시각목록UseCase(개체시각대응DbContext db, IAuthorizationService authorization,
    ICurrentUserAccessor currentUser, IOptionsMonitor<개체시각자산Options> options, TimeProvider clock)
{
    public const string Verification = "FileVerified_FitnessUnreviewed";

    public async Task<개체시각목록Result> ImportAsync(ClaimsPrincipal user,
        IReadOnlyList<개체시각자산입력> inputs, CancellationToken ct)
    {
        var access = await AccessAsync(user);
        if (access is not null) return new(access, []);
        if (inputs is null || inputs.Count is < 1 or > 32 || inputs.Any(x => x is null)) return new("InvalidAssetBatch", []);
        if (inputs.Select(Id).Distinct(StringComparer.Ordinal).Count() != inputs.Count) return new("DuplicateAssetInput", []);
        // 모든 입력을 검증한 뒤에만 한 트랜잭션으로 저장. Unity API/임포트/재질 변경은 없다.
        foreach (var input in inputs)
        {
            var diagnostic = VerifyFiles(input, options.CurrentValue);
            if (diagnostic != "Valid") return new(diagnostic, []);
        }
        var existing = 0;
        var pending = new List<개체시각자산판본>();
        foreach (var input in inputs)
        {
            var id = Id(input);
            var row = await db.Assets.AsNoTracking().SingleOrDefaultAsync(x => x.AssetVersionId == id, ct);
            if (row is not null)
            {
                if (Parse(row).Metadata != input) return new("AssetRevisionConflict", []);
                existing++;
            }
            else pending.Add(new()
            {
                AssetVersionId = id, VisualKey = input.VisualKey, CatalogRevision = input.CatalogRevision,
                PrefabGuid = input.PrefabGuid, MetadataHash = 개체시각선택Policy.Hash(input),
                MetadataJson = JsonSerializer.Serialize(input), VerificationState = Verification,
                RegisteredBy = currentUser.UserId!, RegisteredAtUtc = clock.GetUtcNow().UtcDateTime
            });
        }
        db.Assets.AddRange(pending);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { db.ChangeTracker.Clear(); return new("AssetStorageConflictOrFailure", []); }
        var ids = inputs.Select(Id).ToList(); // List.Contains: EF 식에 .NET span overload가 들어가지 않게 한다.
        var saved = await db.Assets.AsNoTracking().Where(x => ids.Contains(x.AssetVersionId)).ToArrayAsync(ct);
        return new("Persisted", saved.Select(Parse).OrderBy(x => x.Metadata.VisualKey, StringComparer.Ordinal).ToArray(), pending.Count, existing);
    }

    public async Task<개체시각목록Result> ListAsync(ClaimsPrincipal user, string? visualKey, int skip, CancellationToken ct)
    {
        var access = await AccessAsync(user);
        if (access is not null) return new(access, []);
        if (skip < 0 || (visualKey is not null && !Token(visualKey))) return new("InvalidAssetQuery", []);
        var query = db.Assets.AsNoTracking();
        if (visualKey is not null) query = query.Where(x => x.VisualKey == visualKey);
        var rows = await query.OrderBy(x => x.VisualKey).ThenBy(x => x.CatalogRevision).Skip(skip).Take(100).ToArrayAsync(ct);
        return new("Found", rows.Select(Parse).ToArray());
    }

    public static string Id(개체시각자산입력 input) => 개체시각선택Policy.Hash(new { input.VisualKey, input.CatalogRevision });

    public static 개체시각자산판본Dto Parse(개체시각자산판본 row)
    {
        var metadata = JsonSerializer.Deserialize<개체시각자산입력>(row.MetadataJson)
            ?? throw new InvalidOperationException("AssetStorageInvalid");
        if (row.AssetVersionId != Id(metadata) || row.VisualKey != metadata.VisualKey ||
            row.CatalogRevision != metadata.CatalogRevision || row.PrefabGuid != metadata.PrefabGuid ||
            row.MetadataHash != 개체시각선택Policy.Hash(metadata) || row.VerificationState != Verification)
            throw new InvalidOperationException("AssetStorageIntegrityFailed");
        return new(row.AssetVersionId, metadata, row.VerificationState, row.RegisteredAtUtc);
    }

    public static bool Matches(개체시각자산판본 row, 개체시각후보Dto candidate)
    {
        var m = Parse(row).Metadata;
        return candidate.AssetVersionId == row.AssetVersionId && candidate.VisualKey == m.VisualKey &&
            candidate.CatalogRevision == m.CatalogRevision && candidate.CatalogFingerprint == m.CatalogFingerprint &&
            candidate.AssetFingerprint == m.AssetFingerprint;
    }

    private async Task<string?> AccessAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(user.FindFirstValue(ClaimTypes.NameIdentifier))) return "Unauthorized";
        if (user.FindFirstValue(ClaimTypes.NameIdentifier) != currentUser.UserId) return "PrincipalMismatch";
        if (!(await authorization.AuthorizeAsync(user, null, 개체시각대응Codes.Policy)).Succeeded) return "Forbidden";
        return options.CurrentValue.ReviewEnabled || options.CurrentValue.Enabled ? null : "FeatureDisabled";
    }

    private static bool Token(string? text) => !string.IsNullOrWhiteSpace(text) && text.Length <= 160 &&
        text.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_' or ':');
    private static bool Hash(string? text) => text is { Length: 64 } && text.All(c => char.IsAsciiHexDigit(c) && !char.IsLower(c));

    private static string VerifyFiles(개체시각자산입력 m, 개체시각자산Options settings)
    {
        if (!Token(m.VisualKey) || !Token(m.CatalogRevision) || !Token(m.Pack) || m.Provider != "Synty" ||
            m.PrefabGuid is not { Length: 32 } || !m.PrefabGuid.All(char.IsAsciiHexDigit) ||
            new[] { m.DisplayName, m.Role }.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 160) ||
            !new[] { m.CatalogFingerprint, m.AssetFingerprint, m.MetaFingerprint, m.EvidenceFingerprint }.All(Hash) ||
            !개체시각자산Catalog.EvidenceReference(m.EvidenceRef)) return "InvalidAssetMetadata";
        if (string.IsNullOrWhiteSpace(settings.UnitySourceRoot) || string.IsNullOrWhiteSpace(settings.EvidenceRoot)) return "AssetSourceRootMissing";
        try
        {
            var prefab = SafeFile(settings.UnitySourceRoot, m.PrefabPath, "Assets/Synty/", ".prefab");
            if (!m.PrefabPath.StartsWith("Assets/Synty/" + m.Pack + "/", StringComparison.Ordinal)) return "AssetPackMismatch";
            var meta = SafeFile(settings.UnitySourceRoot, m.PrefabPath + ".meta", "Assets/Synty/", ".prefab.meta");
            var catalog = SafeFile(settings.UnitySourceRoot, m.CatalogPath, "Assets/Ssalddel/", ".asset");
            var evidence = SafeFile(settings.EvidenceRoot, m.EvidenceRef, "docs/", ".md");
            if (FileHash(prefab) != m.AssetFingerprint || FileHash(meta) != m.MetaFingerprint ||
                FileHash(catalog) != m.CatalogFingerprint || FileHash(evidence) != m.EvidenceFingerprint) return "AssetFileHashMismatch";
            var guids = Regex.Matches(File.ReadAllText(meta), @"(?m)^guid: ([a-fA-F0-9]{32})\r?$");
            if (guids.Count != 1 || guids[0].Groups[1].Value != m.PrefabGuid) return "AssetGuidMismatch";
            // 현재 WorldVisualCatalog의 단순 key/prefab 행만 지원. 지원외 YAML을 추정 파싱하지 않는다.
            var entries = Regex.Matches(File.ReadAllText(catalog),
                @"(?m)^  - visualKey: ([^\r\n]+)\r?\n    prefab: \{fileID: [0-9]+, guid: ([a-fA-F0-9]{32}), type: 3\}");
            var matches = entries.Cast<Match>().Where(x => x.Groups[1].Value == m.VisualKey).ToArray();
            if (matches.Length != 1 || matches[0].Groups[2].Value != m.PrefabGuid) return "AssetCatalogReferenceMismatch";
            return "Valid";
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { return "AssetFileReadOrPathRejected"; }
    }

    private static string SafeFile(string root, string relative, string prefix, string suffix)
    {
        if (string.IsNullOrWhiteSpace(relative) || relative.Length > 512 || !relative.StartsWith(prefix, StringComparison.Ordinal) ||
            !relative.EndsWith(suffix, StringComparison.Ordinal) || relative.Contains('\\') || relative.Contains(':') ||
            relative.Split('/').Any(x => x is "" or "." or "..")) throw new IOException("AssetPathRejected");
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(fullRoot, relative));
        if (!full.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)) throw new IOException("AssetPathRejected");
        var file = new FileInfo(full);
        if (!file.Exists || file.Length is < 1 or > 16 * 1024 * 1024 || (file.Attributes & FileAttributes.ReparsePoint) != 0)
            throw new IOException("AssetFileRejected");
        for (var parent = file.Directory; parent is not null; parent = parent.Parent)
            if ((parent.Attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("AssetReparseRejected");
        return full;
    }
    private static string FileHash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
