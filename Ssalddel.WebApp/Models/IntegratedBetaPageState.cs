namespace Ssalddel.WebApp.Models;

public enum IntegratedBetaStage
{
    Live,
    Beta,
    Experience,
    Preparing
}

public enum WebInteractionBoundary
{
    ReadOnly,
    PlatformPersistence,
    Simulation
}

public sealed record IntegratedBetaPageState(
    IntegratedBetaStage Stage,
    WebInteractionBoundary Boundary,
    bool RequiresAuthentication,
    string Notice,
    bool IsCataloged = true)
{
    public string StageLabel => StageLabelFor(Stage);

    public string StageDescription => StageDescriptionFor(Stage);

    public string BoundaryLabel => Boundary switch
    {
        WebInteractionBoundary.ReadOnly => "조회",
        WebInteractionBoundary.PlatformPersistence => "플랫폼 저장",
        WebInteractionBoundary.Simulation => "Simulation",
        _ => "상태 확인"
    };

    public string AccessLabel => RequiresAuthentication ? "로그인 필요" : "로그인 선택";

    public static string StageLabelFor(IntegratedBetaStage stage)
        => stage switch
    {
        IntegratedBetaStage.Live => "운영",
        IntegratedBetaStage.Beta => "베타",
        IntegratedBetaStage.Experience => "체험",
        IntegratedBetaStage.Preparing => "준비 중",
        _ => "준비 중"
    };

    public static string StageDescriptionFor(IntegratedBetaStage stage)
        => stage switch
    {
        IntegratedBetaStage.Live => "현재 통합 배포에서 사용할 수 있는 핵심 흐름",
        IntegratedBetaStage.Beta => "저장·조회 경로를 검증하며 점진적으로 다듬는 흐름",
        IntegratedBetaStage.Experience => "연결 구조와 화면 동선을 먼저 확인하는 흐름",
        IntegratedBetaStage.Preparing => "운영·법률·외부 연동 준비 전에는 실행하지 않는 흐름",
        _ => "통합 상태를 검토 중인 흐름"
    };
}
