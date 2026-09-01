using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2, "정책과 seed에 따른 자원 재성장·신규 생성·토지 제외를 권위 Tick에서 판정한다.",
        Boundary = "독립 자동 전이 Core. 채집·토지 변경·실제 공간·Session·Save를 실행하지 않는다.",
        WorldInteractionIds = new[] { Simulation세계자원재생Codes.WorldInteractionId })]
    public sealed class Simulation세계자원재생Aggregate
    {
        private readonly object 동기화 = new object();
        private readonly string 세계, 세션, 권위주체, 정책Hash;
        private readonly long Seed;
        private readonly Dictionary<string, Simulation자원재생Profile> 정책들 = new Dictionary<string, Simulation자원재생Profile>(StringComparer.Ordinal);
        private readonly Dictionary<string, Simulation자원재생Cell> 셀들 = new Dictionary<string, Simulation자원재생Cell>(StringComparer.Ordinal);
        private Dictionary<string, Simulation자원재생Node> 노드들 = new Dictionary<string, Simulation자원재생Node>(StringComparer.Ordinal);
        private readonly Dictionary<string, (string 입력, Simulation세계자원재생Snapshot 결과)> 전이들 = new Dictionary<string, (string, Simulation세계자원재생Snapshot)>(StringComparer.Ordinal);
        private Simulation행위발현Ledger 행위;
        private long 개정, Tick;

        public Simulation세계자원재생Aggregate(Simulation세계자원재생InitialState 초기)
        {
            if (초기 == null || 초기.Profiles == null || 초기.Cells == null || 초기.Nodes == null || 초기.WorldRevision < 0 || 초기.WorldTick < 0 || 초기.WorldTick > int.MaxValue)
                throw new SimulationContractException("ResourceRegenerationInitialInvalid");
            세계 = 필수(초기.WorldStableId); 세션 = 필수(초기.SessionStableId); 권위주체 = 필수(초기.권위주체StableId);
            Seed = 초기.WorldSeed; 개정 = 초기.WorldRevision; Tick = 초기.WorldTick;
            foreach (var p in 초기.Profiles)
            {
                if (p == null) throw new SimulationContractException("ResourceRegenerationProfileInvalid");
                var id = 필수(p.ProfileStableId); 필수(p.Revision); 필수(p.자원StableId);
                if (정책들.ContainsKey(id) || p.성숙단계 < 1 || p.단계간Tick < 1 || p.단계간Tick > int.MaxValue || p.생성확률Micro < 0 || p.생성확률Micro > 1000000 || p.Tick당생성상한 < 0 ||
                    (p.종류Code != Simulation세계자원재생Codes.식물 && p.종류Code != Simulation세계자원재생Codes.환경묶음))
                    throw new SimulationContractException("ResourceRegenerationProfileInvalid");
                정책들.Add(id, new Simulation자원재생Profile { ProfileStableId = id, Revision = p.Revision, 자원StableId = p.자원StableId,
                    종류Code = p.종류Code, 성숙단계 = p.성숙단계, 단계간Tick = p.단계간Tick, 생성확률Micro = p.생성확률Micro, Tick당생성상한 = p.Tick당생성상한 });
            }
            if (정책들.Count == 0) throw new SimulationContractException("ResourceRegenerationProfileRequired");
            foreach (var c in 초기.Cells)
            {
                if (c == null) throw new SimulationContractException("ResourceRegenerationCellInvalid");
                var id = 필수(c.CellStableId);
                if (셀들.ContainsKey(id) || c.ProfileStableId == null || !정책들.ContainsKey(c.ProfileStableId) ||
                    !new[] { Simulation세계자원재생Codes.자연, Simulation세계자원재생Codes.평탄화, Simulation세계자원재생Codes.건설, Simulation세계자원재생Codes.도로 }.Contains(c.토지용도Code))
                    throw new SimulationContractException("ResourceRegenerationCellInvalid");
                셀들.Add(id, new Simulation자원재생Cell { CellStableId = id, ProfileStableId = c.ProfileStableId, 토지용도Code = c.토지용도Code, 중심Xmm = c.중심Xmm, 중심Zmm = c.중심Zmm });
            }
            var 점유셀 = new HashSet<string>(StringComparer.Ordinal);
            foreach (var n in 초기.Nodes)
            {
                if (n == null) throw new SimulationContractException("ResourceRegenerationNodeInvalid");
                var id = 필수(n.NodeStableId);
                if (노드들.ContainsKey(id) || n.CellStableId == null || !셀들.TryGetValue(n.CellStableId, out var c) || !점유셀.Add(n.CellStableId))
                    throw new SimulationContractException("ResourceRegenerationNodeInvalid");
                var p = 정책들[c.ProfileStableId];
                if (n.성장단계 < 0 || n.성장단계 > p.성숙단계 || n.다음성장Tick < 0 || n.다음성장Tick > int.MaxValue || (n.성장단계 == p.성숙단계 && n.다음성장Tick != 0) ||
                    (p.종류Code == Simulation세계자원재생Codes.환경묶음 && n.성장단계 != p.성숙단계))
                    throw new SimulationContractException("ResourceRegenerationNodeInvalid");
                노드들.Add(id, 복사(n));
            }
            정책Hash = 해시(정규형(세계, 세션, 권위주체, Seed,
                정규형(정책들.Values.OrderBy(p => p.ProfileStableId, StringComparer.Ordinal).Select(p => (object)정규형(p.ProfileStableId, p.Revision, p.자원StableId, p.종류Code, p.성숙단계, p.단계간Tick, p.생성확률Micro, p.Tick당생성상한)).ToArray()),
                정규형(셀들.Values.OrderBy(c => c.CellStableId, StringComparer.Ordinal).Select(c => (object)정규형(c.CellStableId, c.ProfileStableId, c.토지용도Code, c.중심Xmm, c.중심Zmm)).ToArray())));
            행위 = new Simulation행위발현Ledger(세계);
        }

        public Simulation세계자원재생Snapshot Snapshot() { lock (동기화) return 사본(); }
        public Simulation자원재생Preview PreviewTick(Simulation자원재생TickRequest 요청)
        {
            if (요청 == null) throw new SimulationContractException("ResourceRegenerationRequestRequired");
            lock (동기화) return 준비(요청).판독;
        }
        public Simulation자원재생TickResult ApplyTick(Simulation자원재생TickRequest 요청)
        {
            if (요청 == null) throw new SimulationContractException("ResourceRegenerationRequestRequired");
            var id = 필수(요청.TransitionId);
            lock (동기화)
            {
                var 입력 = 정규형(요청.권위주체StableId, 요청.WorldTick, 요청.ExpectedRevision);
                if (전이들.TryGetValue(id, out var 이전))
                {
                    if (입력 != 이전.입력) throw new SimulationConflictException("ResourceRegenerationTransitionConflict");
                    return new Simulation자원재생TickResult
                    {
                        Reused = true,
                        State = 복사(이전.결과),
                        분야성장적용 = 세계파생성장미적용(),
                    };
                }
                var 다음 = 준비(요청);
                if (!다음.판독.CanApply) throw new SimulationConflictException(다음.판독.BlockReasonCodes[0]);
                var 다음행위 = Simulation행위발현Ledger.Restore(행위.Snapshot());
                다음행위.Append(new Simulation행위발현Record
                {
                    WorldStableId = 세계, SessionStableId = 세션, PlayableLoopStableId = "playable-loop:nature-night-day2.v1",
                    WorldInteractionId = Simulation세계자원재생Codes.WorldInteractionId, CommandId = id,
                    ActorStableId = 권위주체, InitiatorStableId = 권위주체, ActorKindCode = "WorldSystem", TriggerSourceCode = "WorldDerived",
                    TargetStableIds = 다음.판독.변경노드StableIds.Length == 0 ? new[] { 세계 } : 다음.판독.변경노드StableIds,
                    OutcomeStableId = "outcome:resource-regeneration:" + id,
                    PrimaryOutcomeCode = 다음.판독.변경노드StableIds.Length == 0 ? "ResourceAvailabilityUnchanged" : "ResourceAvailabilityRestored",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    변화의미Codes = 다음.판독.변경노드StableIds.Length == 0
                        ? new[] { "WorldTickAdvanced" }
                        : new[] { Simulation세계자원재생Codes.ResourceAvailabilityChanged },
                    AppliedWorldTick = checked((int)요청.WorldTick), BeforeWorldRevision = 개정, AfterWorldRevision = 개정 + 1,
                    RuleRevision = Simulation세계자원재생Codes.RuleRevision, SourceReferenceIds = new[] { "policy-sha256:" + 정책Hash },
                });
                노드들 = 다음.노드; Tick = 요청.WorldTick; 개정++; 행위 = 다음행위;
                var 결과 = 사본(); 전이들.Add(id, (입력, 결과));
                return new Simulation자원재생TickResult
                {
                    State = 복사(결과),
                    분야성장적용 = 세계파생성장미적용(),
                };
            }
        }

        private static Simulation분야성장적용Snapshot 세계파생성장미적용()
            => new Simulation분야성장적용Snapshot
            {
                상태Code = Simulation분야성장적용상태Codes.NotApplicable,
                사유Code = Simulation세계자원재생Codes.PlayerProgressionNotApplicableReason,
                PlayerStableId = string.Empty,
                BeforeProfileRevision = 0,
                AfterProfileRevision = 0,
            };

        private (Simulation자원재생Preview 판독, Dictionary<string, Simulation자원재생Node> 노드) 준비(Simulation자원재생TickRequest 요청)
        {
            var 차단 = new List<string>();
            if (요청.권위주체StableId != 권위주체) 차단.Add("ResourceRegenerationAuthorityRequired");
            if (요청.ExpectedRevision != 개정) 차단.Add("ResourceRegenerationExpectedRevisionMismatch");
            if (Tick == int.MaxValue || 요청.WorldTick != Tick + 1) 차단.Add("ResourceRegenerationNextTickRequired");
            if (개정 == long.MaxValue) 차단.Add("ResourceRegenerationRevisionExhausted");
            if (string.IsNullOrWhiteSpace(요청.TransitionId) || 요청.TransitionId != 요청.TransitionId.Trim()) 차단.Add("ResourceRegenerationTransitionIdInvalid");
            var 다음 = 노드들.ToDictionary(x => x.Key, x => 복사(x.Value), StringComparer.Ordinal);
            var 변경 = new List<string>();
            if (차단.Count == 0)
            {
                try
                {
                    foreach (var n in 다음.Values.OrderBy(n => n.NodeStableId, StringComparer.Ordinal))
                    {
                        var c = 셀들[n.CellStableId]; var p = 정책들[c.ProfileStableId];
                        if (c.토지용도Code != Simulation세계자원재생Codes.자연 || n.성장단계 == p.성숙단계 || 요청.WorldTick < n.다음성장Tick) continue;
                        n.성장단계++;
                        n.다음성장Tick = n.성장단계 == p.성숙단계 ? 0 : 다음성장시각(요청.WorldTick, p.단계간Tick);
                        변경.Add(n.NodeStableId);
                    }
                    var 점유 = new HashSet<string>(다음.Values.Select(n => n.CellStableId), StringComparer.Ordinal);
                    foreach (var p in 정책들.Values.OrderBy(p => p.ProfileStableId, StringComparer.Ordinal))
                    {
                        var 후보 = 셀들.Values.Where(c => c.ProfileStableId == p.ProfileStableId && c.토지용도Code == Simulation세계자원재생Codes.자연 && !점유.Contains(c.CellStableId))
                            .Select(c => (셀: c, 키: 해시(정규형(Seed, 세계, c.CellStableId, p.ProfileStableId, p.Revision, 요청.WorldTick))))
                            .Where(c => Convert.ToUInt32(c.키.Substring(0, 8), 16) % 1000000 < p.생성확률Micro)
                            .OrderBy(c => c.키, StringComparer.Ordinal).ThenBy(c => c.셀.CellStableId, StringComparer.Ordinal).Take(p.Tick당생성상한);
                        foreach (var c in 후보)
                        {
                            var id = "resource-node:" + c.키;
                            if (다음.ContainsKey(id)) { 차단.Add("ResourceRegenerationNodeCollision"); break; }
                            var 즉시가용 = p.종류Code == Simulation세계자원재생Codes.환경묶음;
                            다음.Add(id, new Simulation자원재생Node { NodeStableId = id, CellStableId = c.셀.CellStableId, 성장단계 = 즉시가용 ? p.성숙단계 : 0,
                                다음성장Tick = 즉시가용 ? 0 : 다음성장시각(요청.WorldTick, p.단계간Tick) });
                            점유.Add(c.셀.CellStableId); 변경.Add(id);
                        }
                    }
                }
                catch (OverflowException) { 차단.Add("ResourceRegenerationTickOverflow"); }
            }
            return (new Simulation자원재생Preview { WorldRevision = 개정, 변경노드StableIds = 차단.Count == 0 ? 변경.OrderBy(x => x, StringComparer.Ordinal).ToArray() : Array.Empty<string>(), BlockReasonCodes = 차단.ToArray() }, 다음);
        }

        private Simulation세계자원재생Snapshot 사본()
        {
            var 노드 = 노드들.Values.OrderBy(n => n.NodeStableId, StringComparer.Ordinal).Select(n =>
            {
                var c = 셀들[n.CellStableId]; var p = 정책들[c.ProfileStableId];
                return new Simulation자원재생조회Node { NodeStableId = n.NodeStableId, CellStableId = n.CellStableId, ProfileStableId = p.ProfileStableId,
                    자원StableId = p.자원StableId, 종류Code = p.종류Code, Xmm = c.중심Xmm, Zmm = c.중심Zmm,
                    성장단계 = n.성장단계, 다음성장Tick = n.다음성장Tick, 채집가능 = n.성장단계 == p.성숙단계 && c.토지용도Code == Simulation세계자원재생Codes.자연 };
            }).ToArray();
            var 원장 = 행위.Snapshot();
            return new Simulation세계자원재생Snapshot { WorldRevision = 개정, WorldTick = Tick, 정책HashSha256 = 정책Hash, Nodes = 노드, ActionLedger = 원장,
                StateHashSha256 = 해시(정규형(Simulation세계자원재생Codes.RuleRevision, 정책Hash, 개정, Tick,
                    정규형(노드.Select(n => (object)정규형(n.NodeStableId, n.CellStableId, n.성장단계, n.다음성장Tick)).ToArray()), 원장.StateHashSha256)) };
        }
        private static Simulation자원재생Node 복사(Simulation자원재생Node n) => new Simulation자원재생Node
        { NodeStableId = n.NodeStableId, CellStableId = n.CellStableId, 성장단계 = n.성장단계, 다음성장Tick = n.다음성장Tick };
        private static Simulation세계자원재생Snapshot 복사(Simulation세계자원재생Snapshot s) => new Simulation세계자원재생Snapshot
        {
            WorldRevision = s.WorldRevision, WorldTick = s.WorldTick, 정책HashSha256 = s.정책HashSha256, StateHashSha256 = s.StateHashSha256,
            ActionLedger = Simulation행위발현Ledger.Restore(s.ActionLedger).Snapshot(),
            Nodes = s.Nodes.Select(n => new Simulation자원재생조회Node { NodeStableId = n.NodeStableId, CellStableId = n.CellStableId, ProfileStableId = n.ProfileStableId,
                자원StableId = n.자원StableId, 종류Code = n.종류Code, Xmm = n.Xmm, Zmm = n.Zmm, 성장단계 = n.성장단계, 다음성장Tick = n.다음성장Tick, 채집가능 = n.채집가능 }).ToArray(),
        };
        private static long 다음성장시각(long 현재Tick, long 간격)
        {
            var 다음 = checked(현재Tick + 간격);
            // 공통 행위 기록 AppliedWorldTick의 표현 범위 밖에 실행 불가능한 예약을 만들지 않는다.
            if (다음 > int.MaxValue) throw new OverflowException();
            return 다음;
        }
        private static string 필수(string 값) => string.IsNullOrWhiteSpace(값) || 값 != 값.Trim()
            ? throw new SimulationContractException("ResourceRegenerationIdentifierInvalid") : 값;
        private static string 정규형(params object[] 값들) => string.Concat(값들.Select(값 => { var s = Convert.ToString(값, CultureInfo.InvariantCulture) ?? string.Empty; return s.Length.ToString(CultureInfo.InvariantCulture) + ":" + s; }));
        private static string 해시(string 값) { using var h = SHA256.Create(); return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(값))).Replace("-", string.Empty).ToLowerInvariant(); }
    }
}
