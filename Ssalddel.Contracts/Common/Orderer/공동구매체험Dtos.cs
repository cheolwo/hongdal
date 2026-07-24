namespace Ssalddel.Contracts.Common.Orderer;

public static class 공동구매체험단계코드
{
    public const string 재료선택 = "ChooseIngredient";
    public const string 이웃만남 = "MeetNeighbors";
    public const string 조건맞추기 = "MatchConditions";
    public const string 실제수요준비 = "ReadyForRealDemand";
}

public static class 공동구매체험대화주제코드
{
    public const string 처음참여 = "FirstTime";
    public const string 요리이야기 = "CookingIdeas";
    public const string 보관방법 = "Storage";
    public const string 수령방법 = "Pickup";

    public static IReadOnlyList<string> 지원목록 { get; } =
    [
        처음참여,
        요리이야기,
        보관방법,
        수령방법
    ];

    public static string 정규화(string? value)
        => 지원목록.Contains(value?.Trim(), StringComparer.Ordinal)
            ? value!.Trim()
            : 처음참여;
}

public sealed class 공동구매체험시나리오응답
{
    public string 시나리오Id { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 소개 { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string HS코드후보 { get; set; } = string.Empty;
    public string 온도코드 { get; set; } = string.Empty;
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public decimal 기본희망수량 { get; set; }
    public string 수량단위 { get; set; } = "kg";
    public int 목표참여자수 { get; set; }
    public decimal 목표수량 { get; set; }
    public decimal 연습기준단가 { get; set; }
    public string 통화코드 { get; set; } = "KRW";
    public string 샘플안내 { get; set; } = string.Empty;
}

public sealed class 공동구매체험요청
{
    public string 세션Id { get; set; } = string.Empty;
    public string 시나리오Id { get; set; } = string.Empty;
    public decimal 내희망수량 { get; set; }
    public int 라운드 { get; set; }
    public string 대화주제코드 { get; set; } = 공동구매체험대화주제코드.처음참여;
}

public sealed class 공동구매체험응답
{
    public string 세션Id { get; set; } = string.Empty;
    public bool 시뮬레이션여부 { get; set; } = true;
    public bool 서버저장여부 { get; set; }
    public bool 외부효과발생여부 { get; set; }
    public int 현재라운드 { get; set; }
    public int 최대라운드 { get; set; }
    public string 현재단계코드 { get; set; } = 공동구매체험단계코드.재료선택;
    public bool 완료여부 { get; set; }
    public 공동구매체험시나리오응답 시나리오 { get; set; } = new();
    public IReadOnlyList<공동구매체험참여자응답> 참여자목록 { get; set; } = [];
    public IReadOnlyList<공동구매체험대화응답> 대화목록 { get; set; } = [];
    public 공동구매자동집단진행응답 진행 { get; set; } = new();
    public decimal 연습예상단가 { get; set; }
    public decimal 연습절감률 { get; set; }
    public string 통화코드 { get; set; } = "KRW";
    public string 친해지기질문 { get; set; } = string.Empty;
    public string 응원메시지 { get; set; } = string.Empty;
    public string 다음행동라벨 { get; set; } = string.Empty;
    public string 실제수요전환상품키 { get; set; } = string.Empty;
    public string 실제수요전환안내 { get; set; } = string.Empty;
    public IReadOnlyList<string> 안전경계안내 { get; set; } = [];
}

public sealed class 공동구매체험참여자응답
{
    public string 참여자키 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 이모지 { get; set; } = string.Empty;
    public string 한줄소개 { get; set; } = string.Empty;
    public decimal 희망수량 { get; set; }
    public string 수량단위 { get; set; } = "kg";
    public bool 가상참여자여부 { get; set; }
    public int 합류라운드 { get; set; }
}

public sealed class 공동구매체험대화응답
{
    public string 발화자 { get; set; } = string.Empty;
    public string 본문 { get; set; } = string.Empty;
    public bool 가상대화여부 { get; set; }
}
