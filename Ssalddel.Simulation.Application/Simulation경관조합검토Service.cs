using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Box = Ssalddel.Simulation.Application.Simulation배치적합성검사.Box;

namespace Ssalddel.Simulation.Application
{
    /// <summary>World/Session이 없는 검토 전용 입력. 배열 순서는 의미를 갖지 않는다.</summary>
    public sealed class Simulation경관조합검토Input
    {
        public string Revision { get; set; } = "pattern-composition-review.d442.r1";
        public string Seed { get; set; } = string.Empty;
        public string GrammarHash { get; set; } = string.Empty;
        public string GrammarSourceHash { get; set; } = string.Empty;
        public string RulesHash { get; set; } = string.Empty;
        public Simulation경관조합검토Rules Rules { get; set; } = new();
        public double? ReviewSizeMeters { get; set; }
        public string SurfaceRevision { get; set; } = string.Empty;
        public string SurfaceHash { get; set; } = string.Empty;
        public string SurfaceEvidenceKind { get; set; } = string.Empty;
        public Simulation경관검토Placement[] Placements { get; set; } = Array.Empty<Simulation경관검토Placement>();
        public Simulation경관검토Connection[] Connections { get; set; } = Array.Empty<Simulation경관검토Connection>();
        // D443 r2에서만 사용. null/엣지 없음은 연결 선택/단절의 승인이 아니다.
        public Simulation경관검토Relation[]? Relations { get; set; }
        // 검토 원점 좌표의 보호 공간. 빈 배열은 명시적인 보호 공간 없음, null은 미확보.
        public SimulationFarmH2ReservedAreaSnapshot[]? ProtectedAreas { get; set; }
        public bool NeighborhoodComplete { get; set; }
        // 선언된 통행 그래프가 검토 범위의 조사 완료 집합인지 별도로 밝힌다.
        public bool TraversalSurveyComplete { get; set; }
    }

    public sealed class Simulation경관조합검토Rules
    {
        public string Revision { get; set; } = string.Empty;
        public string EvidenceRef { get; set; } = string.Empty;
        public bool TrialOnly { get; set; } = true;
        public SimulationFarmH2MeasurementPolicy? Geometry { get; set; }
        public double? NeighborDistanceMeters { get; set; }
        public double? MaximumConnectorHeightDifferenceMeters { get; set; }
        public bool AccessRequired { get; set; } = true;
        public string AccessNotApplicableReason { get; set; } = string.Empty;
        public Simulation경관경계Rule[] Edges { get; set; } = Array.Empty<Simulation경관경계Rule>();
        public Simulation경관수관Permit[] CanopyPermits { get; set; } = Array.Empty<Simulation경관수관Permit>();
    }
    public sealed class Simulation경관경계Rule
    {
        public string FromProfile { get; set; } = string.Empty;
        public string ToProfile { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty; // Compatible / Forbidden / TransitionRequired
        public double? MinimumGapMeters { get; set; }
        public string EvidenceRef { get; set; } = string.Empty;
    }
    public sealed class Simulation경관수관Permit
    {
        public string FromObjectId { get; set; } = string.Empty;
        public string ToObjectId { get; set; } = string.Empty;
        public string EvidenceRef { get; set; } = string.Empty;
    }
    public sealed class Simulation경관검토Placement
    {
        public string Id { get; set; } = string.Empty;
        // 포함 부모의 외곽만 표시. 자식 점유와 중복되는 합계 측정은 받지 않는다.
        public bool IsContainer { get; set; }
        public string CompositionKey { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Yaw { get; set; }
        public bool Mirrored { get; set; }
        public double? ConnectorLocalHeight { get; set; }
        public string AccessEvidenceRef { get; set; } = string.Empty;
        public bool GeometryComplete { get; set; }
        public Simulation경관검토Object[] Objects { get; set; } = Array.Empty<Simulation경관검토Object>();
        // 패턴 로컬 공간. 미측정 문앞 여유를 문법 footprint로 대체하지 않는다.
        public SimulationFarmH2ReservedAreaSnapshot[]? WorkAreas { get; set; }
    }
    public sealed class Simulation경관검토Object
    {
        public string Id { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Occupancy / Canopy
        public SimulationFarmH2AssetMeasurement Measurement { get; set; } = new();
        public string ExpectedMeasurementHash { get; set; } = string.Empty;
        public string ExpectedAssetFingerprint { get; set; } = string.Empty;
    }
    public sealed class Simulation경관검토Connection
    {
        public string Id { get; set; } = string.Empty;
        public string RelationId { get; set; } = string.Empty;
        public bool Bidirectional { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string FromLocalDirection { get; set; } = string.Empty;
        public string ToLocalDirection { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string RouteSignature { get; set; } = string.Empty;
    }
    public sealed class Simulation경관검토Relation
    {
        public SimulationWorldLandscapeSkeletonEdge Edge { get; set; } = new();
        public string Revision { get; set; } = string.Empty;
        public string EvidenceRef { get; set; } = string.Empty;
        public string Requirement { get; set; } = "Unknown"; // Required / Optional / Separated / Unknown
        public string Observation { get; set; } = "Unknown"; // Confirmed / Blocked / Disconnected / Unknown
        public string Reason { get; set; } = string.Empty;
        public bool RequireReturn { get; set; }
        public bool TraversalForbidden { get; set; }
    }
    public sealed class Simulation경관검토Finding
    {
        public string Rule { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public bool Required { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string EvidenceRef { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string Requirement { get; set; } = string.Empty;
        public string Observation { get; set; } = string.Empty;
        public double? Measured { get; set; }
        public double? Limit { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double? X { get; set; }
        public double? Z { get; set; }
    }
    public sealed class Simulation경관조합검토Result
    {
        public string Revision { get; set; } = "pattern-composition-review.d442.r1";
        public string InputHash { get; set; } = string.Empty;
        public string RulesHash { get; set; } = string.Empty;
        public string ResultHash { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string SurfaceEvidenceKind { get; set; } = string.Empty;
        public Simulation경관검토Finding[] Findings { get; set; } = Array.Empty<Simulation경관검토Finding>();
        public string SurfaceSamplesHash { get; set; } = string.Empty;
        public bool WorldApplied => false;
        public bool ActualTraversalVerified => false;
    }

    /// <summary>기존 문법·Farm 지지/간격/통로·AreaSet 연결 검사의 비저장 조합.</summary>
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E4,
        "격리 경관 조합 후보의 선언·형상·접점과 누락을 판정한다.",
        Boundary = "합성/측정 입력의 순수 검사이며 실제 World·시각·E 성립을 판정하지 않는다.")]
    public sealed class Simulation경관조합검토Service
    {
        public const string RelationReviewRevision = "pattern-composition-review.d443.r2";
        private static readonly string[] Directions = { "north", "east", "south", "west" };
        private static bool Finite(double d) => Simulation배치적합성검사.Finite(d);
        private static bool HashValue(string s) => s != null && s.Length == 64 && s.All(Uri.IsHexDigit);
        private static string Pair(string a, string b) => string.CompareOrdinal(a, b) < 0 ? a + "|" + b : b + "|" + a;

        // 문법 key는 고정하고 후보 목록만 유한하게 검토한다. 실패 후보를 보정/재추첨하지 않는다.
        public Simulation경관조합검토Result[] ReviewCandidates(SimulationWorldLandscapeGrammarCatalog catalog,
            IReadOnlyList<Simulation경관조합검토Input> candidates, ISimulationFarmH2SurfaceReader? surface)
        {
            if (candidates == null || candidates.Count == 0 || candidates.Count > 64)
                throw new ArgumentException("ReviewCandidateBudgetInvalid");
            return candidates.Select(c => Review(catalog, c, surface)).OrderBy(r => r.InputHash, StringComparer.Ordinal).ToArray();
        }

        public Simulation경관조합검토Result Review(SimulationWorldLandscapeGrammarCatalog catalog,
            Simulation경관조합검토Input input, ISimulationFarmH2SurfaceReader? surface)
        {
            if (catalog == null || input == null) throw new ArgumentNullException();
            bool relationReview = input.Revision == RelationReviewRevision;
            var result = new Simulation경관조합검토Result { SurfaceEvidenceKind = input.SurfaceEvidenceKind,
                Revision = relationReview ? RelationReviewRevision : "pattern-composition-review.d442.r1" };
            var findings = new List<Simulation경관검토Finding>();
            void Add(string rule, string target, string state, string detail, bool required = true,
                double? measured = null, double? limit = null, string unit = "", double? x = null, double? z = null) =>
                findings.Add(new Simulation경관검토Finding { Rule = rule, Target = target, State = state, Detail = detail,
                    Required = required, Measured = measured, Limit = limit, Unit = unit, X = x, Z = z,
                    EvidenceRef = input.Rules?.EvidenceRef ?? string.Empty, RuleRevision = input.Rules?.Revision ?? string.Empty });
            void Check(string rule, string target, Action action, double? x = null, double? z = null)
            {
                try { action(); Add(rule, target, "Passed", "ProvidedInputChecked", x: x, z: z); }
                catch (ArgumentException e) { Add(rule, target, "Failed", e.Message, x: x, z: z); }
            }
            Simulation경관조합검토Result Finish()
            {
                result.Findings = findings.OrderBy(f => f.Target, StringComparer.Ordinal).ThenBy(f => f.Rule, StringComparer.Ordinal)
                    .ThenBy(f => f.Detail, StringComparer.Ordinal).ToArray();
                result.State = findings.Any(f => f.Required && f.State == "Failed") ? "Rejected"
                    : findings.Any(f => f.Required && f.State == "NotInspected") ? "Incomplete" : "ReadyForIsolatedReview";
                result.ResultHash = Hash(new { result.Revision, result.InputHash, result.RulesHash, result.State, result.SurfaceEvidenceKind, result.SurfaceSamplesHash, result.Findings });
                return result;
            }
            try { result.InputHash = Hash(new { catalog, input }); result.RulesHash = Hash(input.Rules); }
            catch (ArgumentException) { Add("Input", "review", "Failed", "NonFiniteOrUnserializableInput"); return Finish(); }
            if (input.Revision != result.Revision || !HashValue(input.GrammarHash) || input.GrammarHash != catalog.CatalogHashSha256
                || !HashValue(input.GrammarSourceHash) || input.GrammarSourceHash != catalog.SourceDocumentHashSha256
                || input.Rules == null || input.RulesHash != result.RulesHash || !input.Rules.TrialOnly
                || string.IsNullOrWhiteSpace(input.Seed) || string.IsNullOrWhiteSpace(input.Rules.Revision)
                || string.IsNullOrWhiteSpace(input.Rules.EvidenceRef))
            { Add("Revision", "review", "Failed", "ReviewGrammarOrRulesRevisionMismatch"); return Finish(); }
            if (input.Placements == null || input.Connections == null || input.Placements.Length == 0 || input.Placements.Length > 64
                || input.Connections.Length > 128 || input.Placements.Any(p => p == null || string.IsNullOrWhiteSpace(p.Id)
                    || p.Objects == null || p.Objects.Length > 256 || p.Objects.Any(o => o == null || o.Measurement == null))
                || input.Connections.Any(c => c == null || string.IsNullOrWhiteSpace(c.Id))
                || input.Placements.Select(p => p.Id).Distinct(StringComparer.Ordinal).Count() != input.Placements.Length
                || input.Connections.Select(c => c.Id).Distinct(StringComparer.Ordinal).Count() != input.Connections.Length
                || input.Placements.SelectMany(p => p.Objects).Any(o => string.IsNullOrWhiteSpace(o.Id))
                || input.Placements.SelectMany(p => p.Objects).Select(o => o.Id).Distinct(StringComparer.Ordinal).Count()
                    != input.Placements.Sum(p => p.Objects.Length)
                || input.Placements.Any(p => p.Id.Contains("|") || p.Objects.Any(o => o.Id.Contains("|"))
                    || (p.WorkAreas != null && p.WorkAreas.Any(a => a == null)))
                || (input.ProtectedAreas != null && input.ProtectedAreas.Any(a => a == null)))
            { Add("Input", "review", "Failed", "MissingDuplicateOrOverBudgetInput"); return Finish(); }
            try { catalog.ValidateCanonicalCatalog(); }
            catch (InvalidOperationException e) { Add("Grammar", "review", "Failed", e.Message); return Finish(); }
            var rules = input.Rules;
            var relations = input.Relations ?? Array.Empty<Simulation경관검토Relation>();
            var containsCode = SimulationWorldLandscapeCompositionCodes.Contains;
            var connectsCode = SimulationWorldLandscapeCompositionCodes.Connects;
            var transitionCode = SimulationWorldLandscapeCompositionCodes.TransitionsTo;
            var nodeIds = new HashSet<string>(input.Placements.Select(p => p.Id), StringComparer.Ordinal);
            if (relationReview)
            {
                if (input.Relations == null) Add("Relations", "review", "NotInspected", "RelationPolicyNotProvided");
                if (relations.Length > 128 || relations.Any(r => r == null || r.Edge == null
                    || string.IsNullOrWhiteSpace(r.Edge.EdgeStableId) || !nodeIds.Contains(r.Edge.FromNodeStableId)
                    || !nodeIds.Contains(r.Edge.ToNodeStableId) || r.Edge.FromNodeStableId == r.Edge.ToNodeStableId
                    || r.Edge.NeighborTileKey != null || string.IsNullOrWhiteSpace(r.Revision) || string.IsNullOrWhiteSpace(r.EvidenceRef)
                    || !new[] { containsCode, connectsCode, transitionCode, SimulationWorldLandscapeCompositionCodes.Adjacent }.Contains(r.Edge.RelationCode)
                    || !new[] { "Required", "Optional", "Separated", "Unknown" }.Contains(r.Requirement)
                    || !new[] { "Confirmed", "Blocked", "Disconnected", "Unknown" }.Contains(r.Observation)
                    || ((r.Requirement == "Separated" || r.TraversalForbidden) && string.IsNullOrWhiteSpace(r.Reason))
                    || (r.RequireReturn && (r.Edge.RelationCode != connectsCode || r.Requirement != "Required"))
                    || (r.TraversalForbidden && r.Edge.RelationCode != connectsCode))
                    || relations.Select(r => r.Edge.EdgeStableId).Distinct(StringComparer.Ordinal).Count() != relations.Length
                    || relations.Select(r => Hash(new { r.Edge.RelationCode, r.Edge.FromNodeStableId, r.Edge.ToNodeStableId })).Distinct().Count() != relations.Length)
                { Add("Relations", "review", "Failed", "InvalidDuplicateSelfOrMissingRelation"); return Finish(); }
                var containment = relations.Where(r => r.Edge.RelationCode == containsCode).ToArray();
                if (containment.GroupBy(r => r.Edge.ToNodeStableId).Any(g => g.Count() > 1)
                    || containment.Any(r => Reachable(r.Edge.ToNodeStableId, r.Edge.FromNodeStableId,
                        containment.Select(e => (e.Edge.FromNodeStableId, e.Edge.ToNodeStableId)))))
                { Add("Containment", "review", "Failed", "ContainmentCycleOrMultipleParents"); return Finish(); }
                if (input.Placements.Any(p => p.IsContainer && p.Objects.Length != 0)
                    || containment.Any(r => !input.Placements.Single(p => p.Id == r.Edge.FromNodeStableId).IsContainer))
                { Add("Containment", "review", "Failed", "ParentEnvelopeMustNotDuplicateChildOccupancy"); return Finish(); }
                if (input.Connections.Any(c => !relations.Any(r => r.Edge.EdgeStableId == c.RelationId
                        && r.Edge.RelationCode == connectsCode && r.Edge.FromNodeStableId == c.From && r.Edge.ToNodeStableId == c.To))
                    || input.Connections.Any(c => relations.Any(r => r.Edge.EdgeStableId == c.Id)))
                { Add("Connection", "review", "Failed", "TraversalRelationBindingMissing"); return Finish(); }
            }
            if (rules.Edges == null || rules.CanopyPermits == null || rules.Edges.Length > 128 || rules.CanopyPermits.Length > 128
                || rules.Edges.Any(e => e == null || string.IsNullOrWhiteSpace(e.EvidenceRef)
                    || !(e.State == "Compatible" || e.State == "Forbidden" || e.State == "TransitionRequired"))
                || rules.CanopyPermits.Any(p => p == null || string.IsNullOrWhiteSpace(p.EvidenceRef))
                || rules.Edges.Select(e => Pair(e.FromProfile, e.ToProfile)).Distinct().Count() != rules.Edges.Length)
            { Add("Rules", "review", "Failed", "AmbiguousOrMissingRuleEvidence"); return Finish(); }
            bool policyValid = ValidPolicy(rules.Geometry);
            if (!policyValid) Add("GeometryPolicy", "review", rules.Geometry == null ? "NotInspected" : "Failed", "MissingOrInvalidMeasurementPolicy");
            bool surfaceValid = surface != null && HashValue(input.SurfaceHash) && surface.HashSha256 == input.SurfaceHash
                && !string.IsNullOrWhiteSpace(input.SurfaceRevision) && surface.Revision == input.SurfaceRevision
                && (input.SurfaceEvidenceKind == "SyntheticFixture" || input.SurfaceEvidenceKind == "MeasuredWrapper");
            if (!surfaceValid) Add("Surface", "review", surface == null ? "NotInspected" : "Failed", "MissingOrStaleSurface");
            bool boundaryValid = input.ReviewSizeMeters.HasValue && Finite(input.ReviewSizeMeters.Value) && input.ReviewSizeMeters > 0;
            if (!boundaryValid) Add("Boundary", "review", input.ReviewSizeMeters == null ? "NotInspected" : "Failed", "MissingOrInvalidReviewBoundary");
            bool neighborValid = rules.NeighborDistanceMeters.HasValue && Finite(rules.NeighborDistanceMeters.Value) && rules.NeighborDistanceMeters >= 0;
            if (!neighborValid) Add("Neighborhood", "review", rules.NeighborDistanceMeters == null ? "NotInspected" : "Failed", "MissingOrInvalidNeighborDistance");
            if (!input.NeighborhoodComplete) Add("Neighborhood", "review", "NotInspected", "SurroundingContextNotConfirmed");
            if (input.ProtectedAreas == null) Add("ProtectedAreas", "review", "NotInspected", "ProtectedContextNotProvided");
            var request = new SimulationFarmH2PlacementRequest { Policy = rules.Geometry ?? new(),
                CellSizeMeters = input.ReviewSizeMeters ?? 0, SurfaceHashSha256 = input.SurfaceHash, SurfaceRevision = input.SurfaceRevision };
            var observation = surfaceValid ? new Simulation배치적합성검사.표면관찰Session(request, surface!) : null;
            var placements = new Dictionary<string, (Simulation경관검토Placement P, SimulationWorldLandscapeGrammarEntry G, Box B)>(StringComparer.Ordinal);
            var objects = new List<(Simulation경관검토Object O, string Owner, Box B, double Bottom)>();
            var work = new List<(string Owner, Box B)>();
            foreach (var p in input.Placements.OrderBy(p => p.Id, StringComparer.Ordinal))
            {
                var g = catalog.Entries.SingleOrDefault(g => g.CompositionKey == p.CompositionKey);
                if (g == null || !g.PresentationOnly) { Add("Grammar", p.Id, "Failed", "CompositionMissing"); continue; }
                if (g.RotationCodes == null || g.Connectors == null || g.Connectors.Any(v => v == null)
                    || (g.EdgeProfiles != null && (g.EdgeProfiles.Any(e => e == null || !Directions.Contains(e.DirectionCode) || string.IsNullOrWhiteSpace(e.ProfileCode))
                        || g.EdgeProfiles.Select(e => e.DirectionCode).Distinct().Count() != g.EdgeProfiles.Count)))
                { Add("Grammar", p.Id, "Failed", "AmbiguousOrInvalidDirectionDeclaration"); continue; }
                if (!new[] { p.X, p.Y, p.Z, p.Yaw, g.FootprintX, g.FootprintY }.All(Finite)
                    || g.FootprintX <= 0 || g.FootprintY <= 0 || !new[] { 0d, 90d, 180d, 270d }.Contains(p.Yaw)
                    || !g.RotationCodes.Contains(p.Yaw.ToString("R", CultureInfo.InvariantCulture)) || (p.Mirrored && !g.MirrorAllowed))
                { Add("Transform", p.Id, "Failed", "UnsupportedOrForbiddenNativeTransform", x: p.X, z: p.Z); continue; }
                var reserved = Transform(new Box(-g.FootprintX / 2, -g.FootprintY / 2, g.FootprintX / 2, g.FootprintY / 2), p);
                placements.Add(p.Id, (p, g, reserved));
                Add("Transform", p.Id, "Passed", "NativeQuarterTurnAndAllowedMirror", x: p.X, z: p.Z);
                if (boundaryValid) Check("Boundary", p.Id, () => Simulation배치적합성검사.CellContains(reserved, request), p.X, p.Z);
                if (!p.GeometryComplete || (!p.IsContainer && p.Objects.Length == 0)) Add("Geometry", p.Id, "NotInspected", "CompleteMeasuredCompositionMissing", x: p.X, z: p.Z);
                if (relationReview && p.IsContainer && !relations.Any(r => r.Edge.RelationCode == containsCode && r.Edge.FromNodeStableId == p.Id))
                    Add("Containment", p.Id, "NotInspected", "ContainerChildrenNotProvided");
                if (p.WorkAreas == null) Add("WorkArea", p.Id, "NotInspected", "DoorAndWorkClearanceMissing");
                else foreach (var a in p.WorkAreas)
                    Check("WorkAreaShape", p.Id + ":" + a.SourceStableId, () => work.Add((p.Id, Transform(new Box(a.MinX, a.MinZ, a.MaxX, a.MaxZ), p))));
                bool slopeValid = g.MinimumSlopeDegrees.HasValue && g.MaximumSlopeDegrees.HasValue
                    && Finite(g.MinimumSlopeDegrees.Value) && Finite(g.MaximumSlopeDegrees.Value)
                    && g.MinimumSlopeDegrees >= 0 && g.MaximumSlopeDegrees <= 90 && g.MaximumSlopeDegrees >= g.MinimumSlopeDegrees;
                if (!slopeValid) Add("GrammarSlope", p.Id, "NotInspected", "SlopeDeclarationMissingOrInvalid");
                if (observation != null && slopeValid) Check("GrammarSlope", p.Id, () =>
                {
                    foreach (var point in reserved.Samples())
                    { var s = observation.Read(point.X, point.Z); if (s.SlopeDegrees < g.MinimumSlopeDegrees || s.SlopeDegrees > g.MaximumSlopeDegrees) throw new ArgumentException("GrammarSlopeOutOfRange"); }
                }, p.X, p.Z);
                foreach (var o in p.Objects.OrderBy(o => o.Id, StringComparer.Ordinal))
                {
                    var m = o.Measurement;
                    if (!HashValue(o.ExpectedMeasurementHash) || !HashValue(o.ExpectedAssetFingerprint)
                        || m.MeasurementHashSha256 != o.ExpectedMeasurementHash || m.AssetFingerprintSha256 != o.ExpectedAssetFingerprint
                        || string.IsNullOrWhiteSpace(m.Revision) || string.IsNullOrWhiteSpace(m.EvidenceRef)
                        || !(m.EvidenceKindCode == "SyntheticFixture" || m.EvidenceKindCode == "MeasuredWrapper")
                        || m.UniformScale != 1 || !m.ActiveRenderer || !(o.Role == "Occupancy" || o.Role == "Canopy")
                        || !new[] { m.CenterX, m.CenterY, m.CenterZ, m.SizeX, m.SizeY, m.SizeZ }.All(Finite)
                        || m.SizeX <= 0 || m.SizeY <= 0 || m.SizeZ <= 0)
                    { Add("Measurement", o.Id, "Failed", "MissingStaleOrInvalidMeasurement", x: p.X, z: p.Z); continue; }
                    var b = Transform(new Box(m.CenterX - m.SizeX / 2, m.CenterZ - m.SizeZ / 2, m.CenterX + m.SizeX / 2, m.CenterZ + m.SizeZ / 2), p);
                    var bottom = p.Y + m.CenterY - m.SizeY / 2;
                    objects.Add((o, p.Id, b, bottom));
                    if (o.Role == "Occupancy")
                    {
                        Check("MeasuredEnvelope", o.Id, () => Simulation배치적합성검사.ValidateReserved(reserved, b, 0, o.Id), p.X, p.Z);
                        if (policyValid && observation != null) Check("Support", o.Id,
                            () => Simulation배치적합성검사.ValidateSupport(b, bottom, o.Id, request, observation.Read), p.X, p.Z);
                    }
                }
            }
            var occupied = objects.Where(o => o.O.Role == "Occupancy").ToArray();
            for (var a = 0; a < objects.Count; a++) for (var b = a + 1; b < objects.Count; b++)
            {
                var left = objects[a]; var right = objects[b]; var pair = Pair(left.O.Id, right.O.Id);
                if (left.O.Role == "Occupancy" && right.O.Role == "Occupancy" && policyValid)
                    Check("Spacing", pair, () => Simulation배치적합성검사.ValidateSpacing(new[] { left.B, right.B }, rules.Geometry!.MinimumSpacingMeters), left.B.MinX, left.B.MinZ);
                else if (left.O.Role == "Canopy" && right.O.Role == "Canopy" && left.B.Touches(right.B))
                    Add("CanopyContact", pair, rules.CanopyPermits.Any(p => Pair(p.FromObjectId, p.ToObjectId) == pair) ? "Passed" : "NotInspected",
                        "ExplicitPairPermitRequired_NotGroundOrRouteExemption", x: left.B.MinX, z: left.B.MinZ);
                else if (left.O.Role != right.O.Role && left.B.Touches(right.B)
                    && left.Bottom < right.Bottom + right.O.Measurement.SizeY && right.Bottom < left.Bottom + left.O.Measurement.SizeY)
                    Add("CanopyOccupancyContact", pair, "NotInspected", "MixedRoleIntersectionNeedsSeparateEvidence");
            }
            foreach (var area in work)
                Check("WorkClearance", area.Owner, () => Simulation배치적합성검사.ValidatePreserved(area.B, occupied.Select(o => o.B).ToArray()), area.B.MinX, area.B.MinZ);
            if (input.ProtectedAreas != null) foreach (var a in input.ProtectedAreas)
                Check("ProtectedArea", a.SourceStableId, () => Simulation배치적합성검사.ValidatePreserved(new Box(a.MinX, a.MinZ, a.MaxX, a.MaxZ), objects.Select(o => o.B).ToArray()), a.MinX, a.MinZ);
            var ordered = placements.Values.OrderBy(p => p.P.Id, StringComparer.Ordinal).ToArray();
            for (var a = 0; a < ordered.Length; a++) for (var b = a + 1; b < ordered.Length; b++)
            {
                var l = ordered[a]; var r = ordered[b]; var pair = Pair(l.P.Id, r.P.Id); var distance = l.B.Distance(r.B);
                // 부모/자식 외곽은 같은 층의 독립 이웃으로 중복 계산하지 않는다.
                if (relationReview && (l.P.IsContainer || r.P.IsContainer)) continue;
                bool linked = input.Connections.Any(c => Pair(c.From, c.To) == pair)
                    || (relationReview && relations.Any(e => e.Edge.RelationCode != containsCode && Pair(e.Edge.FromNodeStableId, e.Edge.ToNodeStableId) == pair));
                if (!linked && (!neighborValid || distance > rules.NeighborDistanceMeters)) continue;
                Add("NeighborDistance", pair, "Measurement", linked ? "ExplicitConnectionAndSpatialDistance" : "FootprintSpatialDistance", false, distance, rules.NeighborDistanceMeters, "m", l.P.X, l.P.Z);
                if (!l.P.GeometryComplete || !r.P.GeometryComplete)
                    Add("NeighborEvidence", pair, "NotInspected", "ReservedEnvelopeOnly_NotMeasuredNeighbor");
                if (relationReview && !relations.Any(e => e.Edge.RelationCode == connectsCode && Pair(e.Edge.FromNodeStableId, e.Edge.ToNodeStableId) == pair))
                    Add("ConnectionIntent", pair, "NotInspected", "NoTraversalPolicy_NotAutomaticallyOptional");
                if (l.G.AllowedNeighborTopologyCodes == null || r.G.AllowedNeighborTopologyCodes == null
                    || l.G.ForbiddenNeighborTopologyCodes == null || r.G.ForbiddenNeighborTopologyCodes == null)
                    Add("NeighborTopology", pair, "NotInspected", "NeighborDeclarationMissing");
                else Add("NeighborTopology", pair,
                    l.G.AllowedNeighborTopologyCodes.Contains(r.G.TopologyCode) && r.G.AllowedNeighborTopologyCodes.Contains(l.G.TopologyCode)
                    && !l.G.ForbiddenNeighborTopologyCodes.Contains(r.G.TopologyCode) && !r.G.ForbiddenNeighborTopologyCodes.Contains(l.G.TopologyCode) ? "Passed" : "Failed", "BidirectionalDeclaredTopology");
                var dx = r.P.X - l.P.X; var dz = r.P.Z - l.P.Z;
                if (dx == 0 && dz == 0) Add("Edge", pair, "Failed", "CoincidentPatternCenters");
                else if (Math.Abs(dx) > 0 && Math.Abs(dz) > 0) Add("Edge", pair, "NotInspected", "DiagonalBoundaryContactNotSupported");
                else
                {
                    var d = dx > 0 ? "east" : dx < 0 ? "west" : dz > 0 ? "north" : "south";
                    var le = l.G.EdgeProfiles?.SingleOrDefault(e => Direction(e.DirectionCode, l.P) == d);
                    var re = r.G.EdgeProfiles?.SingleOrDefault(e => Direction(e.DirectionCode, r.P) == Opposite(d));
                    var er = le == null || re == null ? null : rules.Edges.SingleOrDefault(e => Pair(e.FromProfile, e.ToProfile) == Pair(le.ProfileCode, re.ProfileCode));
                    if (le == null || re == null) Add("Edge", pair, "NotInspected", "FacingEdgeDeclarationMissing");
                    else if (er == null && le.ProfileCode != re.ProfileCode) Add("Edge", pair, "NotInspected", "TransitionEvidenceMissing:" + le.ProfileCode + "/" + re.ProfileCode);
                    else if (er?.State == "Forbidden") Add("Edge", pair, "Failed", "ExplicitlyForbiddenBoundary");
                    else if (er?.State == "TransitionRequired" && (!er.MinimumGapMeters.HasValue || !Finite(er.MinimumGapMeters.Value) || er.MinimumGapMeters < 0)) Add("Edge", pair, "NotInspected", "TransitionWidthMissing");
                    else Add("Edge", pair, er?.State == "TransitionRequired" && distance < er.MinimumGapMeters ? "Failed" : "Passed",
                        er == null ? "SameDeclaredProfile" : er.State + ":" + er.EvidenceRef, measured: distance, limit: er?.MinimumGapMeters, unit: "m");
                }
                Add("VariantRepeat", pair, "Measurement", l.G.FamilyCode == "farm" && r.G.FamilyCode == "farm"
                    ? "FarmAlignmentNotARejectionRule" : "ForestRepetitionRequiresVisualComparison", false,
                    l.G.CompositionKey == r.G.CompositionKey ? 1 : 0, unit: "pair");
            }
            foreach (var p in ordered)
            {
                if (!relationReview && rules.AccessRequired && (p.G.Connectors.Count == 0 || string.IsNullOrWhiteSpace(p.P.AccessEvidenceRef)))
                    Add("Access", p.P.Id, "NotInspected", "MeasuredEntranceOrConnectorMissing");
                else if (!relationReview && !rules.AccessRequired)
                    Add("Access", p.P.Id, string.IsNullOrWhiteSpace(rules.AccessNotApplicableReason) ? "NotInspected" : "NotApplicable", rules.AccessNotApplicableReason);
            }
            var traversable = new List<(string From, string To)>();
            var uncertain = new List<(string From, string To)>();
            foreach (var c in input.Connections.OrderBy(c => c.Id, StringComparer.Ordinal))
            {
                var findingStart = findings.Count;
                var policy = relationReview ? relations.Single(r => r.Edge.EdgeStableId == c.RelationId) : null;
                if (!placements.TryGetValue(c.From, out var l) || !placements.TryGetValue(c.To, out var r) || c.From == c.To)
                { Add("Connection", c.Id, "Failed", "ConnectionTargetMissingOrSame"); continue; }
                if (policy != null && (policy.Requirement == "Separated" || policy.TraversalForbidden
                    || policy.Observation == "Blocked" || policy.Observation == "Disconnected"))
                { Add("Connection", c.Id, "Failed", "DeclaredPathConflictsWithSeparationOrObservation"); continue; }
                void Potential()
                { uncertain.Add((c.From, c.To)); if (c.Bidirectional) uncertain.Add((c.To, c.From)); }
                var lcs = l.G.Connectors.Where(v => v.DirectionCode == c.FromLocalDirection && v.ConnectorTypeCode == c.Type).ToArray();
                var rcs = r.G.Connectors.Where(v => v.DirectionCode == c.ToLocalDirection && v.ConnectorTypeCode == c.Type).ToArray();
                if (lcs.Length > 1 || rcs.Length > 1) { Add("Connection", c.Id, "Failed", "AmbiguousConnector"); continue; }
                if (lcs.Length == 0 || rcs.Length == 0) { Add("Connection", c.Id, "NotInspected", "ConnectorMissing"); Potential(); continue; }
                var lc = lcs[0]; var rc = rcs[0];
                if (!new[] { lc.LocalX, lc.LocalZ, lc.Width, rc.LocalX, rc.LocalZ, rc.Width }.All(Finite)
                    || lc.Width <= 0 || rc.Width <= 0) { Add("Connection", c.Id, "Failed", "ConnectorNumberInvalid"); continue; }
                if (string.IsNullOrWhiteSpace(l.P.AccessEvidenceRef) || string.IsNullOrWhiteSpace(r.P.AccessEvidenceRef)
                    || !l.P.GeometryComplete || !r.P.GeometryComplete)
                    Add("ConnectionEvidence", c.Id, "NotInspected", "EntranceOrCompleteEnvelopeEvidenceMissing");
                var from = Connector(lc, l.P); var to = Connector(rc, r.P);
                var relation = new SimulationWorldLandscapeGraphRelationResponse
                { ConnectorPair = new SimulationWorldLandscapeConnectorPairResponse { ConnectorTypeCode = c.Type, RouteSignature = c.RouteSignature } };
                var error = SimulationWorldLandscapeGraphRelationValidator.ValidatePair(relation, from, to);
                Add("Connection", c.Id, error == null ? "Passed" : "Failed", error ?? "ExistingAreaSetTypeDirectionDistanceWidthCheck");
                if (!l.P.ConnectorLocalHeight.HasValue || !r.P.ConnectorLocalHeight.HasValue || !rules.MaximumConnectorHeightDifferenceMeters.HasValue)
                    Add("ConnectorHeight", c.Id, "NotInspected", "ConnectorHeightOrToleranceMissing");
                else
                {
                    var delta = Math.Abs(l.P.Y + l.P.ConnectorLocalHeight.Value - r.P.Y - r.P.ConnectorLocalHeight.Value);
                    Add("ConnectorHeight", c.Id, Finite(delta) && rules.MaximumConnectorHeightDifferenceMeters >= 0 && delta <= rules.MaximumConnectorHeightDifferenceMeters ? "Passed" : "Failed",
                        "ExplicitMeasuredHeight", measured: delta, limit: rules.MaximumConnectorHeightDifferenceMeters, unit: "m");
                }
                if (!policyValid || !boundaryValid || observation == null || input.ProtectedAreas == null || !input.NeighborhoodComplete)
                    Add("Route", c.Id, "NotInspected", "RouteContextOrSurfaceMissing");
                else if ((from.WorldEastingMeters != to.WorldEastingMeters && from.WorldNorthingMeters != to.WorldNorthingMeters)
                    || (from.WorldEastingMeters == to.WorldEastingMeters && from.WorldNorthingMeters == to.WorldNorthingMeters))
                    Add("Route", c.Id, "NotInspected", "DiagonalCurvedOrZeroLengthRouteNotSupported");
                else
                    Check("Route", c.Id, () => Simulation배치적합성검사.ValidateRouteSegment(
                        new SimulationFarmH2AnchorSnapshot { OwnerPlacementStableId = c.From, LocalXMeters = from.WorldEastingMeters, LocalZMeters = from.WorldNorthingMeters },
                        new SimulationFarmH2AnchorSnapshot { OwnerPlacementStableId = c.To, LocalXMeters = to.WorldEastingMeters, LocalZMeters = to.WorldNorthingMeters },
                        new SimulationFarmH2RouteSnapshot { WidthMeters = Math.Min(from.WidthMeters, to.WidthMeters) },
                        // Canopy도 보이는 길을 가릴 수 있다. Collider 유무로 우회하지 않는다.
                        // 접점 소유자 안의 실측 물체도 통로를 막을 수 있다. 점유 면제 없이 검사한다.
                        objects.Select(o => ("object:" + o.O.Id, o.B)), input.ProtectedAreas, Array.Empty<Box>(), request, observation.Read));
                var local = findings.Skip(findingStart).ToArray();
                if (local.All(f => f.State != "Failed" && f.State != "NotInspected"))
                { traversable.Add((c.From, c.To)); if (c.Bidirectional) traversable.Add((c.To, c.From)); }
                else if (!local.Any(f => f.State == "Failed")) Potential();
            }
            if (!relationReview && rules.AccessRequired && input.Connections.Length == 0 && input.Placements.Length > 1)
                Add("RouteNetwork", "review", "NotInspected", "ConnectionAndRoutePlanMissing");
            if (relationReview)
            {
                foreach (var r in relations.OrderBy(r => r.Edge.EdgeStableId, StringComparer.Ordinal))
                {
                    var e = r.Edge;
                    if (!placements.TryGetValue(e.FromNodeStableId, out var from) || !placements.TryGetValue(e.ToNodeStableId, out var to))
                    { Add("Relation", e.EdgeStableId, "Failed", "ValidPlacementMissing"); continue; }
                    int start = findings.Count;
                    if (e.RelationCode == containsCode)
                    {
                        Check("Containment", e.EdgeStableId, () => Simulation배치적합성검사.ValidateReserved(from.B, to.B, 0, e.ToNodeStableId));
                        Add("RelationMeaning", e.EdgeStableId, "NotApplicable", "ContainmentIsNotTraversal", false);
                    }
                    else if (e.RelationCode != connectsCode)
                    {
                        var distance = from.B.Distance(to.B);
                        Add("RelationMeaning", e.EdgeStableId, "NotApplicable", "NeighborOrBoundaryIsNotTraversal", false);
                        Add("RelationGeometry", e.EdgeStableId,
                            !from.P.GeometryComplete || !to.P.GeometryComplete || !neighborValid ? "NotInspected"
                                : distance <= rules.NeighborDistanceMeters ? "Passed" : "Failed",
                            "ExplicitRelationWithinDeclaredNeighborhood", measured: distance, limit: rules.NeighborDistanceMeters, unit: "m");
                    }
                    else
                    {
                        bool path = Reachable(e.FromNodeStableId, e.ToNodeStableId, traversable);
                        bool back = Reachable(e.ToNodeStableId, e.FromNodeStableId, traversable);
                        bool declared = input.Connections.Any(c => c.RelationId == e.EdgeStableId);
                        bool failedPath = input.Connections.Where(c => c.RelationId == e.EdgeStableId)
                            .Any(c => findings.Any(f => f.Target == c.Id && f.State == "Failed"));
                        if (r.TraversalForbidden && (path || back || declared))
                            Add("TraversalPolicy", e.EdgeStableId, "Failed", "ForbiddenTraversalPresent");
                        else if (r.Requirement == "Unknown") Add("TraversalPolicy", e.EdgeStableId, "NotInspected", "ConnectionIntentUndecided");
                        else if (r.Requirement == "Separated") Add("TraversalPolicy", e.EdgeStableId,
                            declared || path || back ? "Failed" : "Passed", "ExplicitSeparation:" + r.Reason);
                        else if (r.Requirement == "Optional" && !declared)
                            Add("TraversalPolicy", e.EdgeStableId, "NotApplicable", "OptionalPathAbsent_NotRequired");
                        else if (r.Requirement == "Optional")
                            Add("TraversalPolicy", e.EdgeStableId, failedPath ? "Failed" : path ? "Passed" : "NotInspected", "DeclaredOptionalPathChecked");
                        else
                        {
                            bool knownBlocked = r.Observation == "Blocked" || r.Observation == "Disconnected";
                            Add("Reachability", e.EdgeStableId, knownBlocked ? "Failed" : path ? "Passed"
                                : failedPath ? "Failed" : input.TraversalSurveyComplete && !Reachable(e.FromNodeStableId, e.ToNodeStableId, traversable.Concat(uncertain)) ? "Failed" : "NotInspected",
                                knownBlocked ? "ObservedRequiredPathBlockedOrDisconnected" : path ? "ValidatedDirectedTraversalOnly" : "RequiredPathEvidenceMissing");
                            if (r.RequireReturn) Add("ReturnReachability", e.EdgeStableId, knownBlocked ? "Failed" : back ? "Passed"
                                : input.TraversalSurveyComplete && !Reachable(e.ToNodeStableId, e.FromNodeStableId, traversable.Concat(uncertain)) ? "Failed" : "NotInspected",
                                "OnlyRequestedReturn_NotGlobalConnectivity");
                        }
                    }
                    foreach (var f in findings.Skip(start))
                    { f.Requirement = r.Requirement; f.Observation = r.Observation; f.EvidenceRef = r.EvidenceRef; f.RuleRevision = r.Revision; }
                }
            }
            if (boundaryValid) Add("OccupancyUpperBound", "review", "Measurement", "SumOfProvidedGroundAABBs_NotUnion_NotDensityPass", false,
                occupied.Sum(o => (o.B.MaxX - o.B.MinX) * (o.B.MaxZ - o.B.MinZ)) / (input.ReviewSizeMeters!.Value * input.ReviewSizeMeters.Value), unit: "ratio");
            Add("VisualQuality", "review", "NotInspected", "MaterialsScaleReadabilityAndClusterSilhouetteNeedIsolatedImages", false);
            if (observation != null)
            { Check("SurfaceRevisionAfter", "review", observation.ValidateRevision); result.SurfaceSamplesHash = Hash(observation.Observations); }
            return Finish();
        }

        private static bool ValidPolicy(SimulationFarmH2MeasurementPolicy? p) => p != null && p.TrialOnly
            && !string.IsNullOrWhiteSpace(p.Revision) && !string.IsNullOrWhiteSpace(p.EvidenceRef)
            && new[] { p.MaximumSlopeDegrees, p.MaximumHeightSpreadMeters, p.GroundClearanceMeters, p.BottomToleranceMeters,
                p.MinimumSpacingMeters, p.MinimumRouteWidthMeters, p.RouteSampleStepMeters, p.MaximumRouteSlopeDegrees, p.MaximumRouteStepMeters }.All(v => Finite(v) && v >= 0)
            && p.MaximumSlopeDegrees <= 90 && p.MaximumRouteSlopeDegrees <= 90 && p.MinimumRouteWidthMeters > 0 && p.RouteSampleStepMeters > 0;
        private static bool Reachable(string from, string to, IEnumerable<(string From, string To)> edges)
        {
            var links = edges.ToArray();
            var seen = new HashSet<string>(StringComparer.Ordinal) { from };
            var queue = new Queue<string>(); queue.Enqueue(from);
            while (queue.Count != 0)
            {
                var current = queue.Dequeue();
                foreach (var edge in links.Where(e => e.From == current))
                { if (edge.To == to) return true; if (seen.Add(edge.To)) queue.Enqueue(edge.To); }
            }
            return false;
        }
        private static (double X, double Z) Point(double x, double z, Simulation경관검토Placement p)
        { var r = Simulation배치적합성검사.Rotate(p.Mirrored ? -x : x, z, p.Yaw); return (r.X + p.X, r.Z + p.Z); }
        private static Box Transform(Box b, Simulation경관검토Placement p)
        { var points = b.Samples().Select(v => Point(v.X, v.Z, p)).ToArray(); return new Box(points.Min(v => v.X), points.Min(v => v.Z), points.Max(v => v.X), points.Max(v => v.Z)); }
        private static string Opposite(string d) => Directions[(Array.IndexOf(Directions, d) + 2) % 4];
        private static string Direction(string d, Simulation경관검토Placement p)
        { var i = Array.IndexOf(Directions, d); if (i < 0) return "unsupported"; if (p.Mirrored) i = (4 - i) % 4; return Directions[(i + (int)(p.Yaw / 90)) % 4]; }
        private static SimulationWorldLandscapeExternalConnectorResponse Connector(SimulationWorldLandscapeGrammarConnector c, Simulation경관검토Placement p)
        {
            var point = Point(c.LocalX, c.LocalZ, p);
            return new SimulationWorldLandscapeExternalConnectorResponse { ConnectorTypeCode = c.ConnectorTypeCode, RouteSignature = c.RouteSignature,
                DirectionCode = Direction(c.DirectionCode, p), WidthMeters = c.Width, WorldEastingMeters = point.X, WorldNorthingMeters = point.Z };
        }
        public static string Hash(object? value)
        {
            using var json = JsonDocument.Parse(JsonSerializer.Serialize(value));
            string Canonical(JsonElement e) => e.ValueKind == JsonValueKind.Object
                ? "{" + string.Join(",", e.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => JsonSerializer.Serialize(p.Name) + ":" + Canonical(p.Value))) + "}"
                : e.ValueKind == JsonValueKind.Array ? "[" + string.Join(",", e.EnumerateArray().Select(Canonical).OrderBy(s => s, StringComparer.Ordinal)) + "]" : e.GetRawText();
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(Canonical(json.RootElement)))).Replace("-", string.Empty);
        }
    }
}
