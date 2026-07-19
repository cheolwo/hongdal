namespace Ssalddel.Contracts.Common.Community;

/// <summary>
/// Defines the five business meanings used by the Ssalddel Bagua navigation.
/// The key <c>gen</c> follows the standard romanization of 간괘(艮卦).
/// </summary>
public sealed record BaguaBusinessAreaDefinition(
    string TrigramKey,
    string TrigramName,
    string TrigramSymbol,
    string PositionName,
    string BusinessCode,
    string BusinessName);

/// <summary>
/// Describes the page meaning produced when one business trigram is moved to another.
/// This is a screen-planning catalog; actual routes are connected by the client later.
/// </summary>
public sealed record BaguaTransitionDefinition(
    string TransitionKey,
    string SourceTrigramKey,
    string TargetTrigramKey,
    string SourceBusinessCode,
    string TargetBusinessCode,
    string PageTitle,
    string Purpose,
    string WorkflowKind,
    bool RequiresSourceSelection,
    bool OpensAgreementFlow);

/// <summary>
/// Defines how one actor role normally reads and acts on Bagua transitions.
/// These actions describe the default screen viewpoint, not an authorization grant.
/// </summary>
public sealed record BaguaActorRoleDefinition(
    string RoleCode,
    string RoleName,
    string PrimaryBusinessCode,
    string OwnerAction,
    string InitiatorAction,
    string ReceiverAction,
    string ObserverAction,
    string GovernanceAction);

/// <summary>
/// A role-specific interpretation layered over one canonical transition.
/// Actual access must still be decided from ledger participation and permissions.
/// </summary>
public sealed record BaguaRoleTransitionPerspectiveDefinition(
    string PerspectiveKey,
    string RoleCode,
    string RoleName,
    string TransitionKey,
    string SourceTrigramKey,
    string TargetTrigramKey,
    string ViewTitle,
    string Interpretation,
    string PrimaryAction,
    string PerspectiveMode);

public static class BaguaTrigramKeys
{
    public const string Zhen = "zhen";
    public const string Li = "li";
    public const string Dui = "dui";
    public const string Kan = "kan";
    public const string Gen = "gen";
}

public static class BaguaBusinessCodes
{
    public const string Order = "order";
    public const string Sales = "sales";
    public const string Warehouse = "warehouse";
    public const string Transport = "transport";
    public const string Agreement = "agreement";
}

public static class BaguaTransitionWorkflowKinds
{
    public const string Home = "home";
    public const string Conversion = "conversion";
    public const string Handoff = "handoff";
    public const string Governance = "governance";
    public const string Execution = "execution";
    public const string Result = "result";
    public const string Return = "return";
}

public static class BaguaActorRoleCodes
{
    public const string Orderer = "orderer";
    public const string Seller = "seller";
    public const string WarehouseManager = "warehouse-manager";
    public const string TransportOperator = "transport-operator";
    public const string CooperativeCoordinator = "cooperative-coordinator";
}

public static class BaguaRolePerspectiveModes
{
    public const string Owner = "owner";
    public const string Initiator = "initiator";
    public const string Receiver = "receiver";
    public const string Governor = "governor";
    public const string Observer = "observer";
}

/// <summary>
/// The first implementation slice of Bagua navigation: five business areas and the
/// complete directed 5 x 5 transition list. Ordering is stable by source and then target:
/// 주문, 판매, 창고, 운송, 합의.
/// </summary>
public static class BaguaTransitionCatalog
{
    private static readonly IReadOnlyList<BaguaBusinessAreaDefinition> AreaDefinitions =
    [
        new(BaguaTrigramKeys.Zhen, "진괘", "☳", "동", BaguaBusinessCodes.Order, "주문"),
        new(BaguaTrigramKeys.Li, "리괘", "☲", "남", BaguaBusinessCodes.Sales, "판매"),
        new(BaguaTrigramKeys.Dui, "태괘", "☱", "서", BaguaBusinessCodes.Warehouse, "창고"),
        new(BaguaTrigramKeys.Kan, "감괘", "☵", "북", BaguaBusinessCodes.Transport, "운송"),
        new(BaguaTrigramKeys.Gen, "간괘", "☶", "중앙", BaguaBusinessCodes.Agreement, "합의")
    ];

    private static readonly IReadOnlyList<BaguaActorRoleDefinition> RoleDefinitions =
    [
        new(
            BaguaActorRoleCodes.Orderer,
            "주문자",
            BaguaBusinessCodes.Order,
            "내 주문과 수량, 결제, 수령 조건을 관리한다.",
            "필요 수량, 예산, 수령 조건을 정해 다음 담당자에게 요청한다.",
            "넘어온 결과를 확인하고 수락, 변경 또는 재요청을 결정한다.",
            "해당 전환이 내 주문의 가격, 일정, 수령 방식에 미치는 영향을 확인한다.",
            "주문 안건에 의견을 내고 투표와 전자서명에 참여한다."),
        new(
            BaguaActorRoleCodes.Seller,
            "판매자",
            BaguaBusinessCodes.Sales,
            "상품, 가격, 공급 가능 수량과 판매 상태를 관리한다.",
            "판매 조건과 공급 가능 정보를 확정해 다음 업무로 넘긴다.",
            "넘어온 수요나 확정안을 검토하고 판매·공급 가능 여부를 응답한다.",
            "재고, 운송, 합의 변화가 판매 약속과 정산에 미치는 영향을 확인한다.",
            "가격, 최소 수량, 공급 조건 안건에 의견을 내고 확정 결과를 이행한다."),
        new(
            BaguaActorRoleCodes.WarehouseManager,
            "창고 관리자",
            BaguaBusinessCodes.Warehouse,
            "재고, 입고, 예약, 피킹, 포장과 출고 작업을 관리한다.",
            "재고와 출고 상태를 확인해 보충, 판매 또는 운송 업무로 인계한다.",
            "넘어온 주문이나 확정안을 작업 지시로 받아 재고와 일정을 배정한다.",
            "예정된 주문, 판매, 운송 변화가 입출고와 보관 공간에 미치는 영향을 확인한다.",
            "재고 배분, 보관·픽업 장소와 작업 우선순위 안건에 의견을 낸다."),
        new(
            BaguaActorRoleCodes.TransportOperator,
            "운송 담당자",
            BaguaBusinessCodes.Transport,
            "운송 의뢰, 배차, 상차, 이동, 하차와 인수 상태를 관리한다.",
            "운송 결과와 예외 상황을 주문, 판매 또는 창고 업무에 전달한다.",
            "넘어온 운송 요청을 검토하고 차량, 기사, 경로와 일정을 계획한다.",
            "주문, 판매, 창고 변화가 배차, 운임과 도착 약속에 미치는 영향을 확인한다.",
            "운임, 노선, 일정과 수행자 선정 안건에 의견을 내고 실행 가능성을 확인한다."),
        new(
            BaguaActorRoleCodes.CooperativeCoordinator,
            "협동조합 운영자",
            BaguaBusinessCodes.Agreement,
            "안건, 투표, 이의 제기, 전자서명과 최종 결의 기록을 관리한다.",
            "확정된 결의가 후속 업무로 정확히 전달되고 실행되는지 관리한다.",
            "업무 영역에서 제기된 안건을 접수해 참여자와 의결 절차를 구성한다.",
            "업무 전환 사이의 책임, 규칙, 분쟁 가능성과 감사 기록을 확인한다.",
            "안건을 구성하고 투표, 이의 제기, 전자서명을 거쳐 결의를 확정·집행 연결한다.")
    ];

    private static readonly IReadOnlyList<BaguaTransitionDefinition> TransitionDefinitions =
    [
        // 진괘 · 주문에서 출발
        Transition(BaguaTrigramKeys.Zhen, BaguaTrigramKeys.Zhen,
            "주문 홈",
            "진행 중인 주문과 참여 주문을 한곳에서 조회한다.",
            BaguaTransitionWorkflowKinds.Home),
        Transition(BaguaTrigramKeys.Zhen, BaguaTrigramKeys.Li,
            "주문 접수 · 판매 확정",
            "구매 의사를 판매자가 접수하고 가격, 수량, 공급 조건을 확정한다.",
            BaguaTransitionWorkflowKinds.Conversion),
        Transition(BaguaTrigramKeys.Zhen, BaguaTrigramKeys.Dui,
            "주문 재고 예약 · 피킹 요청",
            "확정된 주문 수량을 창고 재고에 예약하고 피킹 작업을 요청한다.",
            BaguaTransitionWorkflowKinds.Handoff),
        Transition(BaguaTrigramKeys.Zhen, BaguaTrigramKeys.Kan,
            "주문 기반 운송 의뢰",
            "주문의 수령지와 약속 시간을 바탕으로 배송 또는 화물 운송을 요청한다.",
            BaguaTransitionWorkflowKinds.Handoff),
        Transition(BaguaTrigramKeys.Zhen, BaguaTrigramKeys.Gen,
            "공동구매 · 공동주문 합의",
            "여러 참여자의 수요를 모아 품목, 목표 수량, 가격, 픽업 장소를 의결한다.",
            BaguaTransitionWorkflowKinds.Governance,
            opensAgreementFlow: true),

        // 리괘 · 판매에서 출발
        Transition(BaguaTrigramKeys.Li, BaguaTrigramKeys.Zhen,
            "판매 상품 주문 생성",
            "판매 제안에서 구매 수량과 수령 조건을 선택해 실제 주문을 만든다.",
            BaguaTransitionWorkflowKinds.Conversion),
        Transition(BaguaTrigramKeys.Li, BaguaTrigramKeys.Li,
            "판매 홈",
            "판매 중인 상품, 견적, 거래 확정 상태를 조회한다.",
            BaguaTransitionWorkflowKinds.Home),
        Transition(BaguaTrigramKeys.Li, BaguaTrigramKeys.Dui,
            "판매 확정 · 출고 지시",
            "판매가 확정된 상품의 재고를 지정하고 포장과 출고를 지시한다.",
            BaguaTransitionWorkflowKinds.Handoff),
        Transition(BaguaTrigramKeys.Li, BaguaTrigramKeys.Kan,
            "직배송 · 배송 조건 요청",
            "판매 조건에 배송 방식, 운임 부담, 도착 약속을 연결한다.",
            BaguaTransitionWorkflowKinds.Handoff),
        Transition(BaguaTrigramKeys.Li, BaguaTrigramKeys.Gen,
            "가격 · 수량 · 판매조건 의결",
            "공동 판매 또는 공동구매의 가격, 최소 수량, 판매 조건을 투표로 확정한다.",
            BaguaTransitionWorkflowKinds.Governance,
            opensAgreementFlow: true),

        // 태괘 · 창고에서 출발
        Transition(BaguaTrigramKeys.Dui, BaguaTrigramKeys.Zhen,
            "재고 부족 · 보충 발주",
            "안전 재고 이하 품목이나 예약 부족 수량을 새로운 주문으로 요청한다.",
            BaguaTransitionWorkflowKinds.Conversion),
        Transition(BaguaTrigramKeys.Dui, BaguaTrigramKeys.Li,
            "재고 상품 판매 등록",
            "판매 가능한 재고를 수량과 출고 조건이 포함된 판매 제안으로 전환한다.",
            BaguaTransitionWorkflowKinds.Conversion),
        Transition(BaguaTrigramKeys.Dui, BaguaTrigramKeys.Dui,
            "창고 홈",
            "재고, 입고, 피킹, 포장, 출고 작업을 조회한다.",
            BaguaTransitionWorkflowKinds.Home),
        Transition(BaguaTrigramKeys.Dui, BaguaTrigramKeys.Kan,
            "출고 완료 · 운송 인계",
            "포장 완료 화물을 기사 또는 운송사에 인계하고 증빙을 남긴다.",
            BaguaTransitionWorkflowKinds.Handoff),
        Transition(BaguaTrigramKeys.Dui, BaguaTrigramKeys.Gen,
            "재고 배분 · 픽업 장소 의결",
            "한정 재고의 참여자별 배분량과 공동 픽업·보관 장소를 확정한다.",
            BaguaTransitionWorkflowKinds.Governance,
            opensAgreementFlow: true),

        // 감괘 · 운송에서 출발
        Transition(BaguaTrigramKeys.Kan, BaguaTrigramKeys.Zhen,
            "배송 완료 · 재배송 처리",
            "배송 결과를 주문에 반영하고 실패 건은 재배송 또는 취소 요청으로 돌린다.",
            BaguaTransitionWorkflowKinds.Result),
        Transition(BaguaTrigramKeys.Kan, BaguaTrigramKeys.Li,
            "운송비 판매조건 반영",
            "확정 운임과 배송 가능 범위를 상품의 최종 판매 조건에 반영한다.",
            BaguaTransitionWorkflowKinds.Result),
        Transition(BaguaTrigramKeys.Kan, BaguaTrigramKeys.Dui,
            "반품 회수 · 창고 재입고",
            "회수한 물품을 검수 가능한 창고로 인계하고 재입고 상태를 추적한다.",
            BaguaTransitionWorkflowKinds.Return),
        Transition(BaguaTrigramKeys.Kan, BaguaTrigramKeys.Kan,
            "운송 홈",
            "배차, 상차, 이동, 하차, 인수 상태를 조회한다.",
            BaguaTransitionWorkflowKinds.Home),
        Transition(BaguaTrigramKeys.Kan, BaguaTrigramKeys.Gen,
            "운임 · 노선 · 기사 선정 의결",
            "복수 운송안의 운임, 경로, 일정, 수행자를 비교하고 투표로 정한다.",
            BaguaTransitionWorkflowKinds.Governance,
            opensAgreementFlow: true),

        // 간괘 · 합의에서 출발
        Transition(BaguaTrigramKeys.Gen, BaguaTrigramKeys.Zhen,
            "확정안 주문 생성",
            "전자서명이 완료된 합의안의 참여자별 수량을 실행 주문으로 만든다.",
            BaguaTransitionWorkflowKinds.Execution),
        Transition(BaguaTrigramKeys.Gen, BaguaTrigramKeys.Li,
            "확정안 판매 게시",
            "승인된 가격과 공급 조건을 판매 공고 또는 견적으로 게시한다.",
            BaguaTransitionWorkflowKinds.Execution),
        Transition(BaguaTrigramKeys.Gen, BaguaTrigramKeys.Dui,
            "확정안 창고 작업 생성",
            "승인된 배분안과 픽업 장소를 입고, 보관, 피킹 작업으로 만든다.",
            BaguaTransitionWorkflowKinds.Execution),
        Transition(BaguaTrigramKeys.Gen, BaguaTrigramKeys.Kan,
            "확정안 운송 · 배차 요청",
            "승인된 운송 조건을 운송 의뢰와 배차 후보 요청으로 전환한다.",
            BaguaTransitionWorkflowKinds.Execution),
        Transition(BaguaTrigramKeys.Gen, BaguaTrigramKeys.Gen,
            "합의 · 결의 보관함",
            "투표, 이의 제기, 전자서명, 최종 결의 기록을 조회한다.",
            BaguaTransitionWorkflowKinds.Home)
    ];

    private static readonly IReadOnlyList<BaguaRoleTransitionPerspectiveDefinition> RolePerspectiveDefinitions =
        RoleDefinitions
            .SelectMany(role => TransitionDefinitions.Select(transition => Perspective(role, transition)))
            .ToArray();

    public static IReadOnlyList<BaguaBusinessAreaDefinition> Areas => AreaDefinitions;

    public static IReadOnlyList<BaguaTransitionDefinition> All => TransitionDefinitions;

    public static IReadOnlyList<BaguaActorRoleDefinition> Roles => RoleDefinitions;

    public static IReadOnlyList<BaguaRoleTransitionPerspectiveDefinition> RolePerspectives
        => RolePerspectiveDefinitions;

    public static BaguaBusinessAreaDefinition FindArea(string trigramKey)
        => AreaDefinitions.FirstOrDefault(area =>
               string.Equals(area.TrigramKey, trigramKey, StringComparison.OrdinalIgnoreCase))
           ?? throw new KeyNotFoundException($"등록되지 않은 업무 괘입니다: {trigramKey}");

    public static BaguaTransitionDefinition Find(string sourceTrigramKey, string targetTrigramKey)
        => TransitionDefinitions.FirstOrDefault(transition =>
               string.Equals(transition.SourceTrigramKey, sourceTrigramKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(transition.TargetTrigramKey, targetTrigramKey, StringComparison.OrdinalIgnoreCase))
           ?? throw new KeyNotFoundException(
               $"등록되지 않은 괘 전환입니다: {sourceTrigramKey} -> {targetTrigramKey}");

    public static BaguaTransitionDefinition Find(string transitionKey)
        => TransitionDefinitions.FirstOrDefault(transition =>
               string.Equals(transition.TransitionKey, transitionKey, StringComparison.OrdinalIgnoreCase))
           ?? throw new KeyNotFoundException($"등록되지 않은 괘 전환입니다: {transitionKey}");

    public static BaguaActorRoleDefinition FindRole(string roleCode)
        => RoleDefinitions.FirstOrDefault(role =>
               string.Equals(role.RoleCode, roleCode, StringComparison.OrdinalIgnoreCase))
           ?? throw new KeyNotFoundException($"등록되지 않은 괘 역할입니다: {roleCode}");

    public static IReadOnlyList<BaguaRoleTransitionPerspectiveDefinition> GetRoleMatrix(string roleCode)
    {
        _ = FindRole(roleCode);

        return RolePerspectiveDefinitions
            .Where(perspective =>
                string.Equals(perspective.RoleCode, roleCode, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public static BaguaRoleTransitionPerspectiveDefinition FindPerspective(
        string roleCode,
        string sourceTrigramKey,
        string targetTrigramKey)
    {
        var transition = Find(sourceTrigramKey, targetTrigramKey);

        return RolePerspectiveDefinitions.FirstOrDefault(perspective =>
                   string.Equals(perspective.RoleCode, roleCode, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(
                       perspective.TransitionKey,
                       transition.TransitionKey,
                       StringComparison.OrdinalIgnoreCase))
               ?? throw new KeyNotFoundException(
                   $"등록되지 않은 역할별 괘 전환입니다: {roleCode}, {sourceTrigramKey} -> {targetTrigramKey}");
    }

    public static BaguaRoleTransitionPerspectiveDefinition FindPerspective(
        string roleCode,
        string transitionKey)
    {
        var transition = Find(transitionKey);
        return FindPerspective(roleCode, transition.SourceTrigramKey, transition.TargetTrigramKey);
    }

    private static BaguaTransitionDefinition Transition(
        string sourceTrigramKey,
        string targetTrigramKey,
        string pageTitle,
        string purpose,
        string workflowKind,
        bool opensAgreementFlow = false)
    {
        var source = FindArea(sourceTrigramKey);
        var target = FindArea(targetTrigramKey);
        var isHome = string.Equals(sourceTrigramKey, targetTrigramKey, StringComparison.OrdinalIgnoreCase);

        return new BaguaTransitionDefinition(
            $"{source.BusinessCode}-to-{target.BusinessCode}",
            source.TrigramKey,
            target.TrigramKey,
            source.BusinessCode,
            target.BusinessCode,
            pageTitle,
            purpose,
            workflowKind,
            RequiresSourceSelection: !isHome,
            OpensAgreementFlow: opensAgreementFlow);
    }

    private static BaguaRoleTransitionPerspectiveDefinition Perspective(
        BaguaActorRoleDefinition role,
        BaguaTransitionDefinition transition)
    {
        var source = AreaDefinitions.Single(area => area.TrigramKey == transition.SourceTrigramKey);
        var target = AreaDefinitions.Single(area => area.TrigramKey == transition.TargetTrigramKey);
        var isOwnHome = transition.WorkflowKind == BaguaTransitionWorkflowKinds.Home
                        && role.PrimaryBusinessCode == transition.SourceBusinessCode;
        var isAgreementBoundary = transition.SourceBusinessCode == BaguaBusinessCodes.Agreement
                                  || transition.TargetBusinessCode == BaguaBusinessCodes.Agreement;

        var perspectiveMode = isOwnHome
            ? BaguaRolePerspectiveModes.Owner
            : role.RoleCode == BaguaActorRoleCodes.CooperativeCoordinator && isAgreementBoundary
                ? BaguaRolePerspectiveModes.Governor
                : role.PrimaryBusinessCode == transition.SourceBusinessCode
                    ? BaguaRolePerspectiveModes.Initiator
                    : role.PrimaryBusinessCode == transition.TargetBusinessCode
                        ? BaguaRolePerspectiveModes.Receiver
                        : BaguaRolePerspectiveModes.Observer;

        var primaryAction = perspectiveMode switch
        {
            BaguaRolePerspectiveModes.Owner => role.OwnerAction,
            BaguaRolePerspectiveModes.Initiator => role.InitiatorAction,
            BaguaRolePerspectiveModes.Receiver => role.ReceiverAction,
            BaguaRolePerspectiveModes.Governor => role.GovernanceAction,
            _ => role.ObserverAction
        };

        var interpretation = perspectiveMode switch
        {
            BaguaRolePerspectiveModes.Owner =>
                $"{role.RoleName}의 기본 업무 영역이다. {transition.Purpose}",
            BaguaRolePerspectiveModes.Initiator =>
                $"{role.RoleName}이 {source.BusinessName} 맥락과 조건을 정리해 {target.BusinessName} 측으로 넘기는 화면이다.",
            BaguaRolePerspectiveModes.Receiver =>
                $"{role.RoleName}이 {source.BusinessName} 측에서 넘어온 요청이나 결과를 받아 {target.BusinessName} 업무로 실행하는 화면이다.",
            BaguaRolePerspectiveModes.Governor =>
                $"{role.RoleName}이 {source.BusinessName}과 {target.BusinessName} 사이의 안건, 투표, 이의, 서명과 확정 기록을 관리하는 화면이다.",
            _ =>
                $"{role.RoleName}은 기본 처리 주체가 아니며 {source.BusinessName}에서 {target.BusinessName}으로 넘어가는 진행 상태와 자신의 업무 영향을 확인한다."
        };

        return new BaguaRoleTransitionPerspectiveDefinition(
            $"{role.RoleCode}:{transition.TransitionKey}",
            role.RoleCode,
            role.RoleName,
            transition.TransitionKey,
            transition.SourceTrigramKey,
            transition.TargetTrigramKey,
            $"{transition.PageTitle} · {role.RoleName} 관점",
            interpretation,
            primaryAction,
            perspectiveMode);
    }
}
