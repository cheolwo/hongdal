namespace 홍달.Services.Dispatch.Coordination;

public interface I배차AI판단근거Source
{
    IReadOnlyList<배차AI정책근거Seed> 정책근거목록 { get; }

    IReadOnlyList<배차AI판단사례Seed> 사례목록 { get; }
}

public sealed class 정적배차AI판단근거Source : I배차AI판단근거Source
{
    public IReadOnlyList<배차AI정책근거Seed> 정책근거목록 { get; } =
    [
        new(
            "DCT-POLICY-HARD-CONSTRAINT",
            "필수 조건 우선",
            "냉장·냉동, 위험물, 차량 제원, 인수증, 단독 운송 요청 같은 필수 조건은 거리·수익보다 먼저 차단한다.",
            "docs/Architecture/HIOPSAI.md#운영-가드레일",
            ["필수", "조건", "차량", "냉장", "냉동", "증빙", "단독", "차단"]),
        new(
            "DCT-POLICY-PLATFORM-BUNDLE",
            "플랫폼 수익 묶음 우선",
            "플랫폼은 먼저 단건·2건·3건 이상 의뢰 집합을 만들고, 건당 플랫폼 순이익이 목표값 근처로 회귀하는 묶음을 우선한다.",
            "docs/Architecture/DomesticCargoTransportOS.md#대기-큐와-상태",
            ["플랫폼", "수익", "묶음", "순이익", "목표수익", "배달권", "멀티배차"]),
        new(
            "DCT-POLICY-DRIVER-PAYOUT",
            "기사 목표 지급액 회귀",
            "기사 배정 단계에서는 예상 기사 건당 지급액이 OS 또는 관리자가 정한 목표 단가로 수렴하도록 낮은 후보를 강하게 감점한다.",
            "docs/Architecture/DomesticCargoTransportOS.md#대기-큐와-상태",
            ["기사", "지급액", "단가", "수익", "회귀", "배정", "보정"]),
        new(
            "DCT-POLICY-SCOPE-BUNDLE",
            "같은·인접 배달권 묶음 제한",
            "묶음 후보는 같은 배달권 또는 인접 배달권 안에서 우선 만들고, 외부권 묶음은 운영 가능성이 낮은 후보로 본다.",
            "docs/Architecture/DomesticCargoTransportOS.md#대기-큐와-상태",
            ["배달권", "인접", "같은배달권", "권역", "묶음", "멀티배차"]),
        new(
            "FOOD-POLICY-PICKUP-DELIVERY-DEADLINE",
            "음식 픽업·전달 마감 우선",
            "음식 배달 OS는 조리 완료 또는 포장 완료 예상시각, 픽업 가능시각, 고객 전달 마감을 먼저 보고 배차를 시작한다.",
            "docs/Architecture/EngineOverview.md#os별-스케줄링-정책-카탈로그",
            ["음식", "조리완료", "포장완료", "픽업", "고객전달", "배달완료시간", "EDF"]),
        new(
            "FOOD-POLICY-MULTI-DELIVERY-SCOPE",
            "음식 멀티배차 권역 제한",
            "음식 멀티배차는 최대 2건을 1차 기준으로 두고, 같은 배달권을 우선하며 인접 배달권은 시간·거리 조건이 맞을 때만 허용한다.",
            "docs/Versions/v3.0/food-delivery-pricing-settlement-notes.md#멀티배차-판단",
            ["음식", "멀티배차", "묶음", "같은배달권", "인접배달권", "6km", "2건"]),
        new(
            "FOOD-POLICY-DELIVERY-TIME-LIMIT",
            "조리 완료 후 배달 완료 제한",
            "주문자에게 안내한 최대 배달 시간은 멀티배차 조합 필터로 반영하고, 피크타임에는 정책으로 허용 초과분을 둘 수 있다.",
            "docs/Versions/v3.0/food-delivery-pricing-settlement-notes.md#멀티배차-판단",
            ["음식", "조리완료", "배달완료", "42분", "피크타임", "허용초과", "고객전달"])
    ];

    public IReadOnlyList<배차AI판단사례Seed> 사례목록 { get; } =
    [
        new(
            "DCT-001",
            "냉장 화물과 가까운 일반 차량",
            "국내 화물 운송 OS",
            ["냉장", "차량적합성", "Aging", "인수증", "가까운기사"],
            "상차지에 가까운 일반 차량과 더 먼 냉장 가능 차량이 함께 후보로 들어온 상황이다.",
            "냉장 가능 여부와 인수증 처리 가능성을 먼저 보고, 기존 운송 지연 위험과 기사대기 Aging을 그 다음에 반영한다.",
            "보류",
            "판정 보류",
            "docs/ProjectOverview/hiops-ai-judgment-cases.md#DCT-001-냉장-화물과-가까운-일반-차량"),
        new(
            "DCT-002",
            "단독 운송 요청과 묶음 배송 수익",
            "국내 화물 운송 OS",
            ["단독", "묶음", "수익", "파손주의", "운영자확인"],
            "파손주의 고가 장비가 단독 운송으로 등록되었지만 묶으면 수익성이 좋아지는 상황이다.",
            "화주가 단독 운송을 요청했고 분쟁 위험이 크므로 묶음 수익보다 단독 조건을 우선한다.",
            "보류",
            "판정 보류",
            "docs/ProjectOverview/hiops-ai-judgment-cases.md#DCT-002-단독-운송-요청과-묶음-배송-수익"),
        new(
            "DCT-003",
            "상차 마감 임박과 오래 기다린 기사",
            "국내 화물 운송 OS",
            ["상차마감", "시간창", "Aging", "거리", "GeoNearest"],
            "상차 마감까지 35분 남은 의뢰에서 가까운 기사와 오래 기다린 기사가 충돌한다.",
            "오래 대기한 기사에게 보정점을 주되, 상차 마감 실패 위험이 커지면 가까운 기사를 우선한다.",
            "보류",
            "판정 보류",
            "docs/ProjectOverview/hiops-ai-judgment-cases.md#DCT-003-상차-마감-임박과-오래-기다린-기사"),
        new(
            "DCT-006",
            "후보 없음과 공개배차 전환",
            "국내 화물 운송 OS",
            ["후보없음", "공개배차", "냉동", "권역확장", "차량조건"],
            "냉동 화물 의뢰가 들어왔지만 반경 안에 냉동 가능 차량이 없는 상황이다.",
            "일반 차량에게 추천하지 않고 공개배차 또는 운영자 보류로 전환하며 더 넓은 권역의 냉동 차량을 검색한다.",
            "보류",
            "판정 보류",
            "docs/ProjectOverview/hiops-ai-judgment-cases.md#DCT-006-후보-없음과-공개배차-전환"),
        new(
            "DCT-008",
            "운임은 낮지만 기사 대기가 긴 경우",
            "국내 화물 운송 OS",
            ["낮은운임", "기사대기", "Aging", "최소운임", "지급액"],
            "짧은 거리라 운임이 낮고, 가까운 기사와 오래 대기한 기사가 함께 후보로 들어온 상황이다.",
            "시간 여유가 있고 필수 조건 충돌이 없으면 오래 대기한 기사에게 기회를 주되, 이동거리 대비 순이익이 낮으면 최소 운임 보정을 표시한다.",
            "보류",
            "판정 보류",
            "docs/ProjectOverview/hiops-ai-judgment-cases.md#DCT-008-운임은-낮지만-기사-대기가-긴-경우"),
        new(
            "FOOD-001",
            "같은 배달권 음식 2건 묶음",
            "음식 배달 OS",
            ["음식", "멀티배차", "같은배달권", "조리완료", "배달완료시간"],
            "두 음식 주문의 픽업지가 가깝고 고객 주소도 같은 배달권 안에 있으며 조리 완료 예상시각 차이가 작다.",
            "묶음 내부 예상 운행거리가 6km 이하이고 각 주문의 안내 최대 배달 시간을 넘지 않으면 음식 멀티배차 후보로 승인한다.",
            "묶음 승인",
            "시간·권역 조건 충족",
            "docs/Versions/v3.0/food-delivery-pricing-settlement-notes.md#멀티배차-판단"),
        new(
            "FOOD-002",
            "인접 배달권 음식 묶음과 고객 전달 지연",
            "음식 배달 OS",
            ["음식", "멀티배차", "인접배달권", "고객전달", "피크타임", "42분"],
            "인접 배달권의 두 주문을 묶으면 기사 이동 효율은 좋아지지만 한 주문의 고객 전달 시간이 안내 범위를 넘을 수 있다.",
            "피크타임 허용 초과분을 반영해도 주문별 최대 배달 시간을 넘으면 묶음을 제외하고 단건 또는 다른 조합을 우선한다.",
            "조건부 승인",
            "고객 안내 시간 우선",
            "docs/Versions/v3.0/food-delivery-pricing-settlement-notes.md#멀티배차-판단")
    ];
}

public sealed record 배차AI정책근거Seed(
    string 근거Id,
    string 제목,
    string 요약,
    string 출처,
    IReadOnlyList<string> 키워드);

public sealed record 배차AI판단사례Seed(
    string 사례Id,
    string 제목,
    string 관련OS,
    IReadOnlyList<string> 키워드,
    string 상황요약,
    string 판단요약,
    string 사용자판정,
    string 중용판정,
    string 출처);
