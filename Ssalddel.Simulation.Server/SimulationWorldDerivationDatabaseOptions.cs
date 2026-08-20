namespace Ssalddel.Simulation.Server;

public sealed class SimulationWorldDerivationDatabaseOptions
{
    public const string SectionName = "SimulationWorldDerivationDatabase";

    public bool Enabled { get; set; }

    public string ConnectionStringName { get; set; } = "SimulationWorldDerived";

    /// <summary>
    /// 파생 DB의 산출물 보관 객체 키를 해석할 로컬 개발용 루트다.
    /// 운영 객체 저장소 주소나 비밀 값은 이 설정에 넣지 않는다.
    /// </summary>
    public string? ArtifactRootPath { get; set; }

    /// <summary>
    /// Unity가 내보낸 의미 기반 156개 경관 문법 Manifest다.
    /// 유료 Prefab 경로·GUID는 포함하지 않는다.
    /// </summary>
    public string LandscapeGrammarManifestPath { get; set; } =
        "../eng/world-seedbeds/manifests/pyeongchang-landscape-grammar.v1.json";

    /// <summary>
    /// 사람이 작성한 Markdown과 분리된 AreaSet 실행 권위 JSON이다.
    /// </summary>
    public string AreaSetDefinitionPath { get; set; } =
        "../eng/world-seedbeds/area-sets/pyeongchang-farm-hub-town.v1/area-set.json";

    /// <summary>
    /// WI와 경관 Graph 공간 역할·능력·용량을 연결하는 승인 대장이다.
    /// </summary>
    public string InteractionGraphBindingCatalogPath { get; set; } =
        "../eng/world-seedbeds/area-sets/pyeongchang-farm-hub-town.v1/spatial-capabilities.v1.json";

    /// <summary>
    /// H1~H3 이론 생산물을 4개 실제 AreaSet과 하나의 AreaSet Network로 결속한
    /// E5 시나리오 공간 대장이다. 공공데이터 E6 또는 운영 상태를 대신하지 않는다.
    /// </summary>
    public string ActualE5SpatialCatalogPath { get; set; } =
        "../eng/world-seedbeds/generated/actual-e5-spatial.v1.json";
}
