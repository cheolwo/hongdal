using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ssalddel.Contracts.Admin.Content;

namespace Ssalddel.Services.Content;

/// <summary>한정된 로컬 파일 근거 읽기. 의미 판단/이미지 생성/권위 상태 변경은 하지 않는다.</summary>
internal static class 시각자산근거검사
{
    internal const string TaxonomyRef = "eng/execution-ledgers/synty-asset-human-taxonomy.json";
    internal static bool Hash(string? s) => s is { Length: 64 } && s.All(c => char.IsAsciiHexDigit(c) && !char.IsLower(c));
    internal static byte[] Read(개체시각자산Options options, 시각선정파일근거 evidence, int max = 16 * 1024 * 1024)
    {
        var root = evidence.Root switch { "Repository" => options.EvidenceRoot, "Unity" => options.UnitySourceRoot, _ => null };
        if (string.IsNullOrWhiteSpace(root) || !Hash(evidence.Sha256) || string.IsNullOrWhiteSpace(evidence.Path) ||
            evidence.Path.Length > 512 || evidence.Path.Contains(':') || evidence.Path.Contains('\\') ||
            evidence.Path.Split('/').Any(x => x is "" or "." or "..") ||
            !(evidence.Root == "Repository" ? new[] { "docs/", "eng/", "artifacts/local/" } : new[] { "Assets/Synty/", "artifacts/local/" })
                .Any(p => evidence.Path.StartsWith(p, StringComparison.Ordinal))) throw new IOException("EvidencePathRejected");
        var path = Path.GetFullPath(Path.Combine(root, evidence.Path));
        for (string? p = path; p is not null; p = Path.GetDirectoryName(p))
            if ((File.Exists(p) || Directory.Exists(p)) && (File.GetAttributes(p) & FileAttributes.ReparsePoint) != 0) throw new IOException("EvidenceReparseRejected");
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length < 1 || stream.Length > max) throw new IOException("EvidenceSizeRejected");
        using var buffer = new MemoryStream(); stream.CopyTo(buffer); var bytes = buffer.ToArray();
        if (Convert.ToHexString(SHA256.HashData(bytes)) != evidence.Sha256) throw new IOException("EvidenceDrift");
        return bytes;
    }

    internal static string? Selection(개체시각자산Options options, 게임객체시각구성Input definition,
        게임객체시각항목Input item, 보유시각자산Dto asset)
    {
        try
        {
            var p = JsonSerializer.Deserialize<시각자동선정근거>(item.SelectionEvidenceJson!);
            if (p is null || p.SchemaVersion != "visual-auto-selection.r1" || p.Origin != "CodexAutomatic" ||
                p.DefinitionId != definition.DefinitionId || p.DefinitionRevision != definition.DefinitionRevision ||
                p.Role != item.Role || p.SlotKey != item.SlotKey || p.Guid != asset.Metadata.Guid ||
                p.ContentVersionId != asset.ContentVersionId || p.AssetHash != asset.Metadata.AssetHash || p.MetaHash != asset.Metadata.MetaHash)
                return "SelectionIdentityMismatch";
            // 첫 묶음은 정적 Prefab뿐. Actor/Clip/정보형/미정에는 자동 대체하지 않는다.
            if (p.ObjectKind != "Physical" || p.AssetKind != "Prefab" || asset.Metadata.AssetKind != p.AssetKind) return "SelectionKindUnsupported";
            if (string.IsNullOrWhiteSpace(p.Purpose) || string.IsNullOrWhiteSpace(p.Rationale) || p.Purpose.Length > 1000 || p.Rationale.Length > 4000 ||
                p.Conditions is null || p.Conditions.Count is < 3 or > 32 || p.Conditions.Any(x => x is null ||
                    string.IsNullOrWhiteSpace(x.Condition) || string.IsNullOrWhiteSpace(x.Reason) || x.Reason.Length > 1000 || x.State is not ("Verified" or "NotApplicable")) ||
                p.Conditions.Select(x => x.Condition).Distinct(StringComparer.Ordinal).Count() != p.Conditions.Count ||
                new[] { "Purpose", "Shape", "Technical" }.Any(k => !p.Conditions.Any(x => x.Condition == k && x.State == "Verified")))
                return "SelectionEvidenceIncomplete";
            if (p.ImageKind != "ExactPrefabPreview" || p.Image is null || p.Review is null ||
                p.Dependencies is null || p.Dependencies.Count is < 1 or > 128) return "SelectionImageOrDependenciesMissing";
            Read(options, new("Unity", asset.Metadata.RelativePath, p.AssetHash));
            Read(options, new("Unity", asset.Metadata.RelativePath + ".meta", p.MetaHash));
            foreach (var dependency in p.Dependencies) Read(options, dependency);
            var image = Read(options, p.Image);
            if (image.Length < 33 || !image.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) ||
                !image.AsSpan(12, 4).SequenceEqual("IHDR"u8)) return "SelectionImageInvalid";
            // 문서/이미지 연결은 명시 검토서의 같은 전체 내용으로 대조한다. 파일 이름으로 추정하지 않는다.
            var declared = JsonSerializer.SerializeToNode(p)!; declared.AsObject().Remove("Review");
            if (!JsonNode.DeepEquals(declared, JsonNode.Parse(Read(options, p.Review)))) return "SelectionReviewMismatch";
            return null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or ArgumentException or NotSupportedException)
        { return "SelectionEvidenceReadOrDrift"; }
    }

    internal static HashSet<string> TaxonomyPaths(byte[] bytes, string revision)
    {
        using var doc = JsonDocument.Parse(bytes); var root = doc.RootElement;
        if (root.GetProperty("revision").GetString() != revision) throw new IOException("TaxonomyRevisionMismatch");
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var scope in root.GetProperty("표현범위").EnumerateArray())
        foreach (var group in scope.GetProperty("기능군").EnumerateArray())
        {
        result.Add(string.Join('/', scope.GetProperty("범위Code").GetString(), group.GetProperty("기능군Code").GetString()));
        foreach (var sub in group.GetProperty("세부기능군").EnumerateArray())
        foreach (var kind in sub.GetProperty("자산종류").EnumerateArray())
            if (!result.Add(string.Join('/', scope.GetProperty("범위Code").GetString(), group.GetProperty("기능군Code").GetString(),
                sub.GetProperty("세부기능군Code").GetString(), kind.GetProperty("자산종류Code").GetString()))) throw new IOException("DuplicateTaxonomyPath");
        }
        return result;
    }
}
