using System.Security.Cryptography;
using System.Text.Json;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Services.Content;

[SsalddelCodeMetadata(개체시각대응Codes.Feature, SsalddelCodeLayer.Application,
    "같은 문맥의 승인된 개별 대응, 종류 기본, 미연결 순으로 시각 후보를 선택한다.",
    StepKey = "select", FlowOrder = 30, Effects = SsalddelCodeEffect.None,
    ExecutionStage = SsalddelCodeExecutionStage.Preview,
    ReadsFrom = SsalddelCodeDataScope.OperationalState,
    Boundary = "인증·원천 실패는 호출자에서 차단한다. 후보 선택은 실제 Prefab/World 배치를 뜻하지 않는다.")]
public static class 개체시각선택Policy
{
    public static string Hash<T>(T value) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(value)));
    public static string Context(개체시각대상Dto t, bool typeDefault) => Hash(new
    {
        t.Kind, t.SourceKey, t.AccessScope, t.StateCode, t.Purpose, t.Representation,
        StableId = typeDefault ? "*" : t.StableId, TypeDefault = typeDefault
    });
    public static bool SameContext(개체시각대응Dto binding, 개체시각대상Dto target) =>
        Context(binding.Target, binding.TypeDefault) == Context(target, binding.TypeDefault);
    public static bool SameSubject(개체시각대응Dto b, 개체시각대상Dto t) =>
        b.Target.Kind == t.Kind && b.Target.SourceKey == t.SourceKey && b.Target.AccessScope == t.AccessScope &&
        b.Target.Purpose == t.Purpose && b.Target.Representation == t.Representation &&
        (b.TypeDefault || b.Target.StableId == t.StableId);

    public static 개체시각선택Result Select(개체시각대상Dto target,
        IEnumerable<개체시각대응Dto> bindings, I개체시각자산Catalog catalog)
    {
        var relevant = bindings.Where(x => SameContext(x, target)).ToArray();
        var limitations = new List<string>();
        foreach (var typeDefault in new[] { false, true })
        {
            var candidates = relevant.Where(x => x.TypeDefault == typeDefault).ToArray();
            if (candidates.Length > 1) return new("BindingConflict", target);
            var b = candidates.SingleOrDefault();
            if (b is null) { limitations.Add(typeDefault ? "TypeDefaultMissing" : "RecordSpecificMissing"); continue; }
            var problem = b.ReviewState != 개체시각대응Codes.Approved ? "NotApproved" :
                !typeDefault && b.Target.Revision != target.Revision ? "SourceRevisionChanged" : catalog.Check(target, b.Candidate, typeDefault);
            if (problem == "CatalogConflict") return new(problem, target);
            if (problem != "Valid") { limitations.Add((typeDefault ? "TypeDefault:" : "RecordSpecific:") + problem); continue; }
            return new("Selected", target, b.Candidate!.VisualKey, b.BindingId, typeDefault, limitations.ToArray());
        }
        return new("Unmapped", target, Limitations: limitations.ToArray());
    }
}
