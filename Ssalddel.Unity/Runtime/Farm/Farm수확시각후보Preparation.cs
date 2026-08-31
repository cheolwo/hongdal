using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.PresentationContracts;

namespace Ssalddel.Unity.Farm
{
    /// <summary>호출자가 명시적으로 제공하는 동결 후보 정보. 실제 파일 조회 결과를 뜻하지 않는다.</summary>
    public sealed class Farm수확시각후보Reference
    {
        public Farm수확시각후보Reference(string stateCode, string assetPath, string guid,
            string fileSha256, string metaSha256, string familyId, string candidateRevision)
        {
            StateCode = stateCode ?? ""; AssetPath = assetPath ?? ""; Guid = guid ?? "";
            FileSha256 = fileSha256 ?? ""; MetaSha256 = metaSha256 ?? "";
            FamilyId = familyId ?? ""; CandidateRevision = candidateRevision ?? "";
        }
        public string StateCode { get; }
        public string AssetPath { get; }
        public string Guid { get; }
        public string FileSha256 { get; }
        public string MetaSha256 { get; }
        public string FamilyId { get; }
        public string CandidateRevision { get; }
    }

    /// <summary>한 상태와 한 후보를 결속한 불변 E4 자료. 파일·Renderer·Scene 연결의 관측은 아니다.</summary>
    public sealed class Farm수확시각후보State
    {
        internal Farm수확시각후보State(Farm수확상태PresentationState source, Farm수확시각후보Reference candidate)
        {
            Source = source; Candidate = candidate;
            PresentationStateCode = source.StateCode == "Harvested" ? "CropHarvested" : "CropGrowing";
            CandidateFingerprint = 표현연결Preflight.Hash(new[] { candidate.StateCode, candidate.AssetPath,
                candidate.Guid, candidate.FileSha256, candidate.MetaSha256, candidate.FamilyId, candidate.CandidateRevision });
            BindingFingerprint = 표현연결Preflight.Hash(new[] { source.SessionStableId,
                source.CultivationUnitStableId, source.SourceWorldRevision.ToString(CultureInfo.InvariantCulture),
                source.PresentationRevision, source.StateCode, source.PresentationSlot,
                PresentationStateCode, CandidateFingerprint });
        }
        public Farm수확상태PresentationState Source { get; }
        public Farm수확시각후보Reference Candidate { get; }
        public string PresentationStateCode { get; }
        public string CandidateFingerprint { get; }
        public string BindingFingerprint { get; }
        public string SourcePresentationRevision => Source.PresentationRevision;
        public long SourceWorldRevision => Source.SourceWorldRevision;
        public string CandidateRevision => Candidate.CandidateRevision;
        // 명세의 기존 visualKeys를 사용한다. Slot과 같은 문자열이지 Catalog.Resolve 성공 증거가 아니다.
        public string VisualKey => Source.PresentationSlot;
        public string SceneBindingStatus => "E5Unlinked";
        public bool AssetLookupVerified => false;
        public bool CatalogLookupVerified => false;
        public bool CanConfirmAuthority => false;
    }

    /// <summary>
    /// Accepted Farm 연구 r1/명세 assetSurvey의 세 후보만 검증한다. 다른 family를 검색하거나 자동 대체하지 않는다.
    /// file/meta SHA는 별도 원본 지문이며 imported dependency hash가 아니다. 실제 파일 IO는 관측 공급자 책임이다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "한 재배의 불변 상태를 승인된 정적 후보와 표현 상태 이름에 대응한다.",
        WorkOrderIds = new[] { "E7-WO-FARM-CROP-CYCLE" },
        WorldInteractionIds = new[] { "WI-FARM-04" },
        Boundary = "E4 후보 준비만 하며 자산조회·Scene·권위사본 공급·E5 성공을 대신하지 않는다.")]
    public static class Farm수확시각후보Preparation
    {
        public const string Revision = "farm-harvest-visual-candidate-preparation.r1";
        private static readonly Farm수확시각후보Reference[] Frozen =
        {
            new Farm수확시각후보Reference("Growing",
                "Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_S.prefab",
                "53e5ab917382c9749a58810d6e170537",
                "2D5093E764F6F66C08EC2C862ECDF250745B66694CE5278622083E3C9FD12912",
                "EE7CDCB2404C218F97697C60B6E1299A307DDE102B9F52C1E54C1EBFE917BD70",
                "synty-family:farm:plants:potato-s", Revision),
            new Farm수확시각후보Reference("HarvestReady",
                "Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_L.prefab",
                "e48b8d820d122d64484926ce5e8f6e8c",
                "FC01F89A96545D8FBA023FCAE7BE54F4EAE5330306A46519D52D6F3C945FF627",
                "D960A73F1FB4EB2A5A55A3F8045C65B7FF0A889771AABECA8CA1085CDBB98703",
                "synty-family:farm:plants:potato-l", Revision),
            new Farm수확시각후보Reference("Harvested",
                "Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Box_Potato_01.prefab",
                "2131bc3845099584ebe0cb30614e96f4",
                "A128993CF0644A5988A537A0196DC69DCD36619538FC499E01ED7F5A3377C583",
                "374ED7092770D129BFC362BAA94068F24CE76EADC5EE3B6AEE29A0498306011B",
                "synty-family:farm:plants:box-potato", Revision)
        };

        public static bool TryPrepare(Farm수확상태PresentationState? state,
            IEnumerable<Farm수확시각후보Reference>? candidates,
            out Farm수확시각후보State? prepared, out string diagnostic)
        {
            prepared = null;
            if (state == null) { diagnostic = "FarmSnapshotMissing_E5Unlinked"; return false; }
            if (state.ProductStableId != "product:potato") { diagnostic = "FarmVisualProductUnsupported"; return false; }
            if (!Frozen.Any(x => x.StateCode == state.StateCode)) { diagnostic = "FarmVisualStateUnsupported"; return false; }
            if (candidates == null) { diagnostic = "FarmVisualCandidatesMissing"; return false; }
            var supplied = candidates.ToArray();
            if (supplied.Any(x => x == null)) { diagnostic = "FarmVisualCandidateNull"; return false; }
            if (supplied.GroupBy(x => x.StateCode, StringComparer.Ordinal).Any(x => x.Count() != 1)
                || supplied.GroupBy(x => x.AssetPath, StringComparer.Ordinal).Any(x => x.Count() != 1))
            { diagnostic = "FarmVisualCandidateDuplicate"; return false; }
            foreach (var value in supplied)
            {
                var expected = Frozen.SingleOrDefault(x => x.StateCode == value.StateCode);
                if (expected == null) { diagnostic = "FarmVisualCandidateStateUnsupported"; return false; }
                if (value.AssetPath != expected.AssetPath || value.Guid != expected.Guid
                    || value.FileSha256 != expected.FileSha256 || value.MetaSha256 != expected.MetaSha256
                    || value.FamilyId != expected.FamilyId || value.CandidateRevision != expected.CandidateRevision)
                { diagnostic = "FarmVisualCandidateDrift"; return false; }
            }
            var selected = supplied.SingleOrDefault(x => x.StateCode == state.StateCode);
            if (selected == null) { diagnostic = "FarmVisualCandidateMissingForState"; return false; }
            prepared = new Farm수확시각후보State(state, selected);
            diagnostic = "Prepared_NotAssetLookup";
            return true;
        }
    }
}
