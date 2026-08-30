using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "기준 배치와 추가 장식의 소유·구성 키·기대 식별자를 결속한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1공간계약,
        Boundary = "해결기가 실제 반환한 등록 키를 전달해야 하며 요청 자체는 자산·지지면 검증이 아니다.")]
    public sealed class Simulation경관배치소비검증Request
    {
        public Simulation세계자산배치Plan SourcePlan { get; set; } = new Simulation세계자산배치Plan();
        public string ExpectedSourcePlanHashSha256 { get; set; } = string.Empty;
        public Simulation세계자산배치Plan BaselinePlan { get; set; } = new Simulation세계자산배치Plan();
        public string ExpectedBaselinePlanHashSha256 { get; set; } = string.Empty;
        public Simulation지도구성Plan MapPlan { get; set; } = new Simulation지도구성Plan();
        public string OwnerCellStableId { get; set; } = string.Empty;
        public string H2StableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public Simulation경관추가배치Binding[] ExpectedDecorations { get; set; } = Array.Empty<Simulation경관추가배치Binding>();
        // 키를 이름 규칙으로 추정하지 않는다. 호출자는 실제 해결기의 조회 결과만 전달한다.
        public string[] ResolvedCompositionKeys { get; set; } = Array.Empty<string>();
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "검토할 추가 장식 한 개의 식별자와 정확한 구성 키를 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1공간계약,
        Boundary = "기대 식별자 정의는 실제 객체 생성 증거가 아니다.")]
    public sealed class Simulation경관추가배치Binding
    {
        public Simulation경관추가배치Binding(string placementStableId, string compositionKey)
        {
            PlacementStableId = placementStableId;
            CompositionKey = compositionKey;
        }
        public string PlacementStableId { get; }
        public string CompositionKey { get; }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "사전 결속 검사와 이후 실제 조립에서 확인할 기대 목록을 분리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1공간계약,
        Boundary = "BindingVerified는 후보 승인·Core 준비·지지면·통로·Renderer·Collider·실제 발현을 뜻하지 않는다.")]
    public sealed class Simulation경관배치소비검증Result
    {
        internal Simulation경관배치소비검증Result(Simulation경관배치소비검증Request request)
        {
            SourcePlanHashSha256 = request.SourcePlan.AssetPlacementPlanHashSha256;
            BaselinePlanHashSha256 = request.BaselinePlan.AssetPlacementPlanHashSha256;
            OwnerCellStableId = request.OwnerCellStableId;
            H2StableId = request.H2StableId;
            AreaSetStableId = request.AreaSetStableId;
            ExpectedPlacements = Array.AsReadOnly(request.ExpectedDecorations
                .OrderBy(value => value.PlacementStableId, StringComparer.Ordinal)
                .Select(value => new Simulation경관추가배치Binding(value.PlacementStableId, value.CompositionKey)).ToArray());
        }
        public string StatusCode => "BindingVerified";
        public string SourcePlanHashSha256 { get; }
        public string BaselinePlanHashSha256 { get; }
        public string OwnerCellStableId { get; }
        public string H2StableId { get; }
        public string AreaSetStableId { get; }
        public IReadOnlyList<Simulation경관추가배치Binding> ExpectedPlacements { get; }
    }

    /// <summary>
    /// 기존 A와 추가 장식만 포함한 공통 계획의 소비 전 결속 검사다.
    /// 실제 B 후보 변환·지형/통로 검사·Unity의 선택형 객체 누락 검사는 이후 소비자의 책임이다.
    /// </summary>
    [SsalddelCodeMetadata(SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Application, "기준 배치를 보존하며 경관 장식의 공통 소비 입력을 검사한다.",
        StepKey = "application.landscape-placement-binding-guard",
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 30,
        Boundary = "입력·권위 상태·WorldRevision·배치 계획을 변경하지 않는 순수 사전검사다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "정규형 hash·소유 계보·기대 장식·등록 키·원본 불변을 검사한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2공간실행,
        Boundary = "성공은 결속 검사뿐이며 후보 승인이나 실제 배치·Play Mode·Game View 증거가 아니다.")]
    public sealed class Simulation경관배치소비검증Service
    {
        public Simulation경관배치소비검증Result Verify(Simulation경관배치소비검증Request request)
        {
            if (request == null) throw Error("InputMissing");
            ValidatePlan(request.SourcePlan, request.ExpectedSourcePlanHashSha256, "Source");
            ValidatePlan(request.BaselinePlan, request.ExpectedBaselinePlanHashSha256, "Baseline");
            ValidateOwnership(request);
            if (request.ExpectedDecorations == null || request.ExpectedDecorations.Length == 0
                || request.ExpectedDecorations.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.PlacementStableId) || string.IsNullOrWhiteSpace(value.CompositionKey)))
                throw Error("ExpectedDecorationInvalid");
            if (request.ExpectedDecorations.Select(value => value.PlacementStableId).Distinct(StringComparer.Ordinal).Count()
                != request.ExpectedDecorations.Length) throw Error("ExpectedDecorationDuplicate");
            if (request.ResolvedCompositionKeys == null || request.ResolvedCompositionKeys.Length == 0
                || request.ResolvedCompositionKeys.Any(string.IsNullOrWhiteSpace)) throw Error("ResolvedCompositionKeysMissing");

            var expected = request.ExpectedDecorations.ToDictionary(value => value.PlacementStableId, StringComparer.Ordinal);
            var baselineIds = new HashSet<string>(request.BaselinePlan.Placements.Select(value => value.PlacementStableId), StringComparer.Ordinal);
            if (expected.Keys.Any(baselineIds.Contains)) throw Error("DecorationAlreadyInBaseline");
            var placements = request.SourcePlan.Placements.ToDictionary(value => value.PlacementStableId, StringComparer.Ordinal);
            if (expected.Keys.Any(id => !placements.ContainsKey(id))) throw Error("ExpectedDecorationMissing");
            if (placements.Keys.Any(id => !baselineIds.Contains(id) && !expected.ContainsKey(id))) throw Error("UnexpectedDecoration");

            // 기존 정규형으로 A 배치·계획 메타데이터·실내 목록을 함께 비교한다.
            var preserved = new Simulation세계자산배치Plan
            {
                SchemaVersion = request.SourcePlan.SchemaVersion,
                RuleRevision = request.SourcePlan.RuleRevision,
                CellStableId = request.SourcePlan.CellStableId,
                SourceWorldRevision = request.SourcePlan.SourceWorldRevision,
                MapPlanHashSha256 = request.SourcePlan.MapPlanHashSha256,
                ChangeProjectionHashSha256 = request.SourcePlan.ChangeProjectionHashSha256,
                SpawnDecisionPlanHashSha256 = request.SourcePlan.SpawnDecisionPlanHashSha256,
                Placements = request.SourcePlan.Placements.Where(value => !expected.ContainsKey(value.PlacementStableId)).ToArray(),
                InteriorPlanHandles = request.SourcePlan.InteriorPlanHandles,
                InteriorPlanBodies = request.SourcePlan.InteriorPlanBodies
            };
            if (Simulation세계자산CanonicalHash.ComputeAssetPlacementPlanHash(preserved)
                != request.BaselinePlan.AssetPlacementPlanHashSha256) throw Error("BaselineChanged");

            var resolved = new HashSet<string>(request.ResolvedCompositionKeys, StringComparer.Ordinal);
            foreach (var binding in request.ExpectedDecorations.OrderBy(value => value.PlacementStableId, StringComparer.Ordinal))
            {
                var placement = placements[binding.PlacementStableId];
                if (placement.OwnerCellStableId != request.OwnerCellStableId) throw Error("DecorationOwnerCellMismatch");
                if (placement.PlacementKindCode != Simulation세계자산배치Codes.Environment
                    || placement.AuthorityKindCode != Simulation세계자산배치Codes.AmbientPresentation
                    || !placement.PresentationOnly) throw Error("DecorationPresentationBoundaryInvalid");
                // 현재 Unity 조립기는 이 네 분류에서 행위 표식을 만들므로 장식에 재사용하지 않는다.
                if (placement.CategoryCode == "NatureResourceNode" || placement.CategoryCode == "NatureCabin"
                    || placement.CategoryCode == "NatureWorkbench" || placement.CategoryCode == "NatureDroppedTimber")
                    throw Error("DecorationInteractionCategoryForbidden");
                if (placement.SourceChangeStableIds.Length != 0 || !string.IsNullOrEmpty(placement.SourceSpawnDecisionStableId))
                    throw Error("DecorationAuthorityLineageForbidden");
                if (!string.IsNullOrEmpty(placement.ParentPlacementStableId)) throw Error("DecorationParentForbidden");
                if (placement.CompositionKey != binding.CompositionKey) throw Error("DecorationCompositionMismatch");
                if (!resolved.Contains(binding.CompositionKey)) throw Error("DecorationCompositionUnresolved");
                if (!new[] { placement.LocalXMeters, placement.LocalYMeters, placement.LocalZMeters,
                    placement.RotationDegrees, placement.UniformScale }.All(Finite)) throw Error("DecorationTransformNotFinite");
                if (placement.UniformScale != 1d) throw Error("DecorationNativeScaleRequired");
                // 환경 장식에 가짜 H1을 만들지 않는다. H1이 있으면 실제 지도 결속을 요구한다.
                if (!string.IsNullOrEmpty(placement.H1StableId)
                    && !request.MapPlan.HBindings.Any(value => value.HLevelCode == "H1"
                        && value.SpatialStableId == placement.H1StableId)) throw Error("DecorationH1Unknown");
            }
            return new Simulation경관배치소비검증Result(request);
        }

        private static void ValidatePlan(Simulation세계자산배치Plan plan, string expectedHash, string name)
        {
            if (plan == null || plan.Placements == null || plan.InteriorPlanHandles == null || plan.InteriorPlanBodies == null
                || plan.Placements.Any(value => value == null || string.IsNullOrWhiteSpace(value.PlacementStableId)
                    || value.SourceChangeStableIds == null)
                || plan.InteriorPlanHandles.Any(value => value == null) || plan.InteriorPlanBodies.Any(value => value == null))
                throw Error(name + "PlanInvalid");
            // 상위 정규형은 실내 본문의 hash만 소비하므로 본문의 봉인도 따로 확인한다.
            foreach (var body in plan.InteriorPlanBodies)
            {
                if (body.Placements == null || body.Placements.Any(value => value == null || value.PresentationFlags == null)
                    || !Sha256(body.BodyHashSha256)
                    || Simulation세계자산CanonicalHash.ComputeInteriorBodyHash(body) != body.BodyHashSha256)
                    throw Error(name + "InteriorBodyHashMismatch");
            }
            if (plan.Placements.Select(value => value.PlacementStableId).Distinct(StringComparer.Ordinal).Count() != plan.Placements.Length)
                throw Error(name + "PlacementDuplicate");
            if (!Sha256(expectedHash) || plan.AssetPlacementPlanHashSha256 != expectedHash
                || Simulation세계자산CanonicalHash.ComputeAssetPlacementPlanHash(plan) != expectedHash) throw Error(name + "PlanHashMismatch");
        }

        private static void ValidateOwnership(Simulation경관배치소비검증Request request)
        {
            var map = request.MapPlan;
            if (map == null || map.HBindings == null || map.Connectors == null || map.Anchors == null
                || map.RequiredCapabilityCodes == null || map.HBindings.Any(value => value == null || value.WorldInteractionIds == null)
                || map.Connectors.Any(value => value == null)
                || map.Anchors.Any(value => value == null || value.AllowedAssetCategoryCodes == null)) throw Error("MapPlanInvalid");
            if (!Sha256(map.MapPlanHashSha256)
                || Simulation세계자산CanonicalHash.ComputeMapPlanHash(map) != map.MapPlanHashSha256) throw Error("MapPlanHashMismatch");
            if (string.IsNullOrWhiteSpace(request.OwnerCellStableId) || request.OwnerCellStableId != map.CellStableId
                || request.SourcePlan.CellStableId != map.CellStableId || request.BaselinePlan.CellStableId != map.CellStableId)
                throw Error("OwnerCellMismatch");
            if (map.SourceWorldRevision < 0 || request.SourcePlan.SourceWorldRevision != map.SourceWorldRevision
                || request.BaselinePlan.SourceWorldRevision != map.SourceWorldRevision
                || request.SourcePlan.MapPlanHashSha256 != map.MapPlanHashSha256
                || request.BaselinePlan.MapPlanHashSha256 != map.MapPlanHashSha256) throw Error("MapLineageMismatch");
            if (string.IsNullOrWhiteSpace(request.H2StableId) || string.IsNullOrWhiteSpace(request.AreaSetStableId)
                || map.HBindings.Count(value => value.HLevelCode == "H2" && value.SpatialStableId == request.H2StableId) != 1
                || map.HBindings.Count(value => value.HLevelCode == "H4" && value.SpatialStableId == request.AreaSetStableId) != 1)
                throw Error("HOwnershipMissingOrAmbiguous");
        }

        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static bool Sha256(string value) => !string.IsNullOrWhiteSpace(value) && value.Length == 64 && value.All(Uri.IsHexDigit);
        private static ArgumentException Error(string code) => new ArgumentException("SimulationLandscapeBinding:" + code);
    }
}
