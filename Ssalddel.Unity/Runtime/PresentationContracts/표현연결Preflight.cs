using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Unity.PresentationContracts
{
    public enum 표현연결Readiness { Ready, Conditional, Blocked }
    public enum 표현연결ObservationStatus { Unobserved, Confirmed, Missing, NotApplicable }
    public enum 표현연결항목
    {
        CandidatePath, VisualKey, CandidateFingerprint, Target, Session, StateRevision,
        PresentationRevision, PresentationSlot, StateCode, LogicE5, Component, Renderer,
        Collider, Bounds, Parent, Position, Anchor, InteractionTarget,
        CreationOwnership, DisplayOwnership, SubscriptionOwnership, ReleaseCoverage
    }

    /// <summary>기존 E4 준비를 읽은 값. Key는 컴포넌트 이름/소유 범위 등 동일 항목 내 식별자다.</summary>
    public sealed class 표현연결Requirement
    {
        public 표현연결Requirement(표현연결항목 item, string key, string expectedValue,
            bool required = true, string notApplicableReason = "")
        {
            Item = item; Key = key ?? ""; ExpectedValue = expectedValue ?? "";
            Required = required; NotApplicableReason = notApplicableReason ?? "";
        }
        public 표현연결항목 Item { get; }
        public string Key { get; }
        public string ExpectedValue { get; }
        public bool Required { get; }
        public string NotApplicableReason { get; }
    }

    /// <summary>
    /// WorldVisualCatalog.Resolve, 대상 컴포넌트 검사, 분야별 lease 소유/해제 관측을 공급자가 옮긴 값.
    /// Missing은 관측된 결손/조회 실패이고 Unobserved는 조회하지 않음이다. 여기서 Unity API를 호출하지 않는다.
    /// EvidenceSha256은 공급 근거의 지문이며 이 순수 검사가 파일의 진위를 검증하는 것은 아니다.
    /// </summary>
    public sealed class 표현연결Observation
    {
        public 표현연결Observation(표현연결항목 item, string key, 표현연결ObservationStatus status,
            string value = "", string evidenceRef = "", string evidenceSha256 = "", bool? validity = null)
        {
            Item = item; Key = key ?? ""; Status = status; Value = value ?? "";
            EvidenceRef = evidenceRef ?? ""; EvidenceSha256 = evidenceSha256 ?? "";
            Validity = validity;
        }
        public 표현연결항목 Item { get; }
        public string Key { get; }
        public 표현연결ObservationStatus Status { get; }
        public string Value { get; }
        public string EvidenceRef { get; }
        public string EvidenceSha256 { get; }
        // Component: 요구 타입 유효, Renderer/Collider: 요구 활성 상태, Bounds: 유한/비어있지 않음,
        // Position: 승인 좌표계의 유한 pose. 부모/기준점/상호작용: 정확 대상의 유효 연결.
        // 소유: 해당 범위의 단일 작성자, ReleaseCoverage: 취소/전환 해제 계획이 소유 범위를 빠짐없이 포함.
        // 이는 해제 실행 성공이 아니다. null은 미관측이며 true로 추정하지 않는다.
        public bool? Validity { get; }
    }

    public sealed class 표현연결Plan
    {
        public 표현연결Plan(string preparationRevision, IEnumerable<표현연결Requirement> requirements)
        {
            PreparationRevision = preparationRevision ?? "";
            Requirements = Array.AsReadOnly((requirements ?? throw new ArgumentNullException(nameof(requirements))).ToArray());
            if (Requirements.Any(x => x == null)) throw new ArgumentException("NullRequirement", nameof(requirements));
            // 한 대상/Session/후보/상태 판본 및 표시 계약이 바뀌면 이전 관측은 재사용할 수 없다.
            ContextFingerprint = 표현연결Preflight.Hash(new[] { PreparationRevision }.Concat(
                Requirements.OrderBy(x => x.Item).ThenBy(x => x.Key, StringComparer.Ordinal)
                    .ThenBy(x => x.ExpectedValue, StringComparer.Ordinal).ThenBy(x => x.Required)
                    .ThenBy(x => x.NotApplicableReason, StringComparer.Ordinal)
                    .SelectMany(x => new[] { ((int)x.Item).ToString(CultureInfo.InvariantCulture), x.Key,
                        x.ExpectedValue, x.Required ? "required" : "not-applicable", x.NotApplicableReason })));
        }
        public string PreparationRevision { get; }
        public ReadOnlyCollection<표현연결Requirement> Requirements { get; }
        public string ContextFingerprint { get; }
    }

    public sealed class 표현연결관측Snapshot
    {
        public 표현연결관측Snapshot(string contextFingerprint, IEnumerable<표현연결Observation> observations)
        {
            ContextFingerprint = contextFingerprint ?? "";
            Observations = Array.AsReadOnly((observations ?? throw new ArgumentNullException(nameof(observations))).ToArray());
            if (Observations.Any(x => x == null)) throw new ArgumentException("NullObservation", nameof(observations));
        }
        public string ContextFingerprint { get; }
        public ReadOnlyCollection<표현연결Observation> Observations { get; }
    }

    public sealed class 표현연결Check
    {
        internal 표현연결Check(표현연결항목 item, string key, 표현연결Readiness readiness,
            string code, string expected, string observed, string evidenceRef, string evidenceSha256)
        {
            Item = item; Key = key; Readiness = readiness; Code = code; Expected = expected;
            Observed = observed; EvidenceRef = evidenceRef; EvidenceSha256 = evidenceSha256;
            NextOwner = item >= 표현연결항목.Renderer && item <= 표현연결항목.InteractionTarget
                ? "개발 → 승인된 관측 범위의 월드·공간·배치" : "개발";
            EarliestReopenStage = item == 표현연결항목.LogicE5 ? "Logic E5"
                : item == 표현연결항목.Session || item == 표현연결항목.StateRevision ? "Logic E1" : "Presentation E4";
        }
        public 표현연결항목 Item { get; }
        public string Key { get; }
        public 표현연결Readiness Readiness { get; }
        public string Code { get; }
        public string Expected { get; }
        public string Observed { get; }
        public string EvidenceRef { get; }
        public string EvidenceSha256 { get; }
        public string NextOwner { get; }
        public string EarliestReopenStage { get; }
    }

    public sealed class 표현연결Result
    {
        internal 표현연결Result(표현연결Plan? plan, IEnumerable<표현연결Check> checks)
        {
            Checks = Array.AsReadOnly(checks.OrderBy(x => x.Item).ThenBy(x => x.Key, StringComparer.Ordinal)
                .ThenBy(x => x.Code, StringComparer.Ordinal).ThenBy(x => x.Observed, StringComparer.Ordinal)
                .ThenBy(x => x.EvidenceRef, StringComparer.Ordinal).ThenBy(x => x.EvidenceSha256, StringComparer.Ordinal).ToArray());
            Readiness = Checks.Any(x => x.Readiness == 표현연결Readiness.Blocked) ? 표현연결Readiness.Blocked
                : Checks.Any(x => x.Readiness == 표현연결Readiness.Conditional) ? 표현연결Readiness.Conditional : 표현연결Readiness.Ready;
            ContextFingerprint = plan?.ContextFingerprint ?? "";
            Target = Read(plan, 표현연결항목.Target); Candidate = Read(plan, 표현연결항목.CandidatePath);
            CandidateFingerprint = Read(plan, 표현연결항목.CandidateFingerprint);
            Session = Read(plan, 표현연결항목.Session); StateRevision = Read(plan, 표현연결항목.StateRevision);
            PresentationRevision = Read(plan, 표현연결항목.PresentationRevision);
            ResultFingerprint = 표현연결Preflight.Hash(new[] { ContextFingerprint, Readiness.ToString() }.Concat(
                Checks.SelectMany(x => new[] { x.Item.ToString(), x.Key, x.Readiness.ToString(), x.Code,
                    x.Expected, x.Observed, x.EvidenceRef, x.EvidenceSha256, x.NextOwner, x.EarliestReopenStage })));
        }
        private static string Read(표현연결Plan? plan, 표현연결항목 item)
            => plan?.Requirements.Where(x => x.Item == item).OrderBy(x => x.Key, StringComparer.Ordinal)
                .ThenBy(x => x.ExpectedValue, StringComparer.Ordinal).FirstOrDefault()?.ExpectedValue ?? "";
        public 표현연결Readiness Readiness { get; }
        public ReadOnlyCollection<표현연결Check> Checks { get; }
        public string Target { get; }
        public string Candidate { get; }
        public string CandidateFingerprint { get; }
        public string Session { get; }
        public string StateRevision { get; }
        public string PresentationRevision { get; }
        public string ContextFingerprint { get; }
        public string ResultFingerprint { get; }
        public bool IsE5Completion => false;
        public string EvidenceBoundary => "ProvidedObservationsOnly_NotEditorOrApplicationEvidence";
    }

    /// <summary>
    /// 읽기 전용 연결 사전검사. 실제 조회/생성/표시/구독/해제는 기존 분야별 소비자의 책임이다.
    /// 적용 직전 현재 준비 판본과 관측으로 Review를 다시 호출한다. 이전 Ready를 캐시해 적용하지 않는다.
    /// 위치/외곽/소유 범위 값은 공급자가 승인 기준과 같은 좌표계·판본으로 정규화한 계약 값이며 임의 허용오차는 없다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "후보·상태·대상·배치·소유/해제의 준비 계약과 관측 근거를 읽기 전용으로 대조한다.",
        Boundary = "Ready는 제공된 관측의 연결 준비 판정이며 실제 Unity 조회·조립·E5 완료가 아니다.")]
    public static class 표현연결Preflight
    {
        public static 표현연결Result Review(표현연결Plan? plan, 표현연결관측Snapshot? snapshot)
        {
            var checks = new List<표현연결Check>();
            if (plan == null)
                return new 표현연결Result(null, new[] { Check(표현연결항목.CandidatePath, "", 표현연결Readiness.Conditional, "PreparationMissing") });
            if (string.IsNullOrWhiteSpace(plan.PreparationRevision))
                checks.Add(Check(표현연결항목.CandidateFingerprint, "", 표현연결Readiness.Conditional, "PreparationRevisionMissing"));
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.ContextFingerprint))
                checks.Add(Check(표현연결항목.StateRevision, "", 표현연결Readiness.Conditional, "ObservationContextMissing"));
            else if (snapshot.ContextFingerprint != plan.ContextFingerprint)
                checks.Add(Check(표현연결항목.CandidateFingerprint, "", 표현연결Readiness.Blocked, "ObservationContextChanged_RecheckRequired", plan.ContextFingerprint, snapshot.ContextFingerprint));
            var observations = snapshot?.Observations ?? Array.AsReadOnly(Array.Empty<표현연결Observation>());
            foreach (표현연결항목 item in Enum.GetValues(typeof(표현연결항목)))
            {
                if (!plan.Requirements.Any(x => x.Item == item))
                    checks.Add(Check(item, "", 표현연결Readiness.Conditional, "RequirementNotPrepared"));
                if (item != 표현연결항목.Component && item < 표현연결항목.CreationOwnership
                    && plan.Requirements.Count(x => x.Item == item) > 1)
                    checks.Add(Check(item, "", 표현연결Readiness.Blocked, "SingletonRequirementDuplicate"));
            }
            foreach (var group in plan.Requirements.GroupBy(x => (x.Item, x.Key)))
            {
                var requirement = group.First();
                var item = requirement.Item; var key = requirement.Key;
                if (!Enum.IsDefined(typeof(표현연결항목), item) || string.IsNullOrWhiteSpace(key) || group.Count() != 1)
                { checks.Add(Check(item, key, 표현연결Readiness.Blocked, "RequirementInvalidOrDuplicate")); continue; }
                // 식별/상태/Logic E5는 비적용으로 우회할 수 없다. Component 이후에만 사유 있는 비적용 허용.
                if (!requirement.Required)
                {
                    checks.Add(Check(item, key, item < 표현연결항목.Component || string.IsNullOrWhiteSpace(requirement.NotApplicableReason)
                        ? 표현연결Readiness.Blocked : 표현연결Readiness.Ready,
                        item < 표현연결항목.Component || string.IsNullOrWhiteSpace(requirement.NotApplicableReason)
                        ? "NotApplicableInvalid" : "NotApplicable", requirement.NotApplicableReason));
                    continue;
                }
                if (string.IsNullOrWhiteSpace(requirement.ExpectedValue))
                { checks.Add(Check(item, key, 표현연결Readiness.Conditional, "ExpectedValueNotPrepared")); continue; }
                if (item == 표현연결항목.LogicE5 && requirement.ExpectedValue != "E5")
                { checks.Add(Check(item, key, 표현연결Readiness.Blocked, "LogicE5RequirementCannotBeLowered")); continue; }
                var found = observations.Where(x => x.Item == item && x.Key == key).ToArray();
                if (found.Length > 1)
                { checks.Add(Check(item, key, 표현연결Readiness.Blocked, "ObservationDuplicate")); continue; }
                var value = found.FirstOrDefault();
                if (value == null || value.Status == 표현연결ObservationStatus.Unobserved)
                { checks.Add(Check(item, key, 표현연결Readiness.Conditional, "ObservationNotAvailable", requirement.ExpectedValue)); continue; }
                if (!Enum.IsDefined(typeof(표현연결ObservationStatus), value.Status))
                { checks.Add(Check(item, key, 표현연결Readiness.Blocked, "ObservationStatusInvalid")); continue; }
                var code = value.Status == 표현연결ObservationStatus.Missing ? "ObservedMissing"
                    : value.Status == 표현연결ObservationStatus.NotApplicable ? "RequiredObservationCannotBeNotApplicable"
                    : value.Value != requirement.ExpectedValue ? "ObservedMismatch" : "Confirmed";
                var readiness = code == "Confirmed" ? 표현연결Readiness.Ready : 표현연결Readiness.Blocked;
                // 확인값과 결손 주장을 모두 남기되 근거가 없으면 확인 성공으로 세지 않는다.
                if (string.IsNullOrWhiteSpace(value.EvidenceRef) || !IsSha256(value.EvidenceSha256))
                {
                    checks.Add(Check(item, key, 표현연결Readiness.Conditional, "ObservationEvidenceMissing", requirement.ExpectedValue,
                        value.Value, value.EvidenceRef, value.EvidenceSha256));
                    continue;
                }
                if (readiness == 표현연결Readiness.Ready && (item == 표현연결항목.Component
                    || item == 표현연결항목.Renderer || item == 표현연결항목.Collider
                    || item == 표현연결항목.Bounds || item == 표현연결항목.Position
                    || item == 표현연결항목.Parent || item == 표현연결항목.Anchor || item == 표현연결항목.InteractionTarget
                    || item >= 표현연결항목.CreationOwnership))
                {
                    if (!value.Validity.HasValue) { code = "ValidityNotObserved"; readiness = 표현연결Readiness.Conditional; }
                    else if (!value.Validity.Value) { code = "ObservedInvalid"; readiness = 표현연결Readiness.Blocked; }
                }
                checks.Add(Check(item, key, readiness, code, requirement.ExpectedValue, value.Value, value.EvidenceRef, value.EvidenceSha256));
            }
            // 임의 추가 결손을 무시하지 않는다. 관측/요구 범위의 잘못된 결속도 차단한다.
            foreach (var value in observations.Where(x => !plan.Requirements.Any(r => r.Item == x.Item && r.Key == x.Key)))
                checks.Add(Check(value.Item, value.Key, 표현연결Readiness.Blocked, "ObservationOutsidePreparedScope", "", value.Value, value.EvidenceRef, value.EvidenceSha256));
            return new 표현연결Result(plan, checks);
        }

        internal static 표현연결Check Check(표현연결항목 item, string key, 표현연결Readiness readiness,
            string code, string expected = "", string observed = "", string evidenceRef = "", string evidenceSha256 = "")
            => new 표현연결Check(item, key, readiness, code, expected, observed, evidenceRef, evidenceSha256);

        private static bool IsSha256(string value) => value.Length == 64 && value.All(x =>
            x >= '0' && x <= '9' || x >= 'a' && x <= 'f' || x >= 'A' && x <= 'F');

        internal static string Hash(IEnumerable<string> fields)
        {
            var text = string.Concat(fields.Select(x => x.Length.ToString(CultureInfo.InvariantCulture) + ":" + x));
            using var hash = SHA256.Create();
            return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(text)).Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }
}
