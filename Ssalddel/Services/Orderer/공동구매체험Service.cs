using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Orderer;

public interface I공동구매체험Service
{
    IReadOnlyList<공동구매체험시나리오응답> 시나리오목록();

    공동구매체험응답 시뮬레이션(공동구매체험요청 request);
}

[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseDemand)]
[SsalddelUseCase(
    "체험 공동구매",
    Summary = "가상 이웃과 저장 없는 공동구매 집단화 과정을 게임처럼 연습합니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandProcessManager,
    SsalddelCodeLayer.Application,
    "고정된 연습 시나리오와 명시적으로 표시된 가상 참여자를 기존 집단화 진행 계산에 넣어 결과를 반환합니다.",
    ContractType = typeof(I공동구매체험Service),
    FlowOrder = 10,
    Effects = SsalddelCodeEffect.None,
    Boundary = "연습 세션은 서버나 원장에 저장하지 않고 실제 사용자, 결제, 주문, 계약, 신고, 연락처 전달 또는 운송을 만들지 않습니다.")]
public sealed class 공동구매체험Service(
    I공동구매주문자집단화Engine 집단화Engine,
    TimeProvider timeProvider) : I공동구매체험Service
{
    private const int 최대라운드 = 3;

    private static readonly IReadOnlyList<공동구매체험시나리오응답> 시나리오들 =
    [
        new()
        {
            시나리오Id = "frozen-pork-neighbors",
            제목 = "동네 냉동 삼겹살 모임",
            소개 = "보관 공간과 수령 시간을 이야기하며 같은 배송권의 수요가 모이는 과정을 연습합니다.",
            상품키 = "hs-food-0203-pork-frozen",
            상품명 = "냉동 삼겹살",
            HS코드후보 = "0203.29",
            온도코드 = "냉동",
            배송권키 = "practice-neighborhood-a",
            배송권명 = "연습 동네 A",
            기본희망수량 = 5,
            수량단위 = "kg",
            목표참여자수 = 6,
            목표수량 = 20,
            연습기준단가 = 8500,
            샘플안내 = "HS 코드와 단가는 연습용 후보이며 실제 품목분류·견적이 아닙니다."
        },
        new()
        {
            시나리오Id = "prepared-food-sauce",
            제목 = "세계요리 소스 함께 고르기",
            소개 = "서로 해보고 싶은 요리를 이야기하고 상온 소스 수요가 모이는 과정을 연습합니다.",
            상품키 = "hs-food-2106-prepared-food",
            상품명 = "간편식 소스",
            HS코드후보 = "2106.90",
            온도코드 = "상온",
            배송권키 = "practice-neighborhood-b",
            배송권명 = "연습 동네 B",
            기본희망수량 = 3,
            수량단위 = "kg",
            목표참여자수 = 6,
            목표수량 = 18,
            연습기준단가 = 3900,
            샘플안내 = "소스의 성분·용도에 따라 실제 분류가 달라질 수 있는 연습용 자료입니다."
        },
        new()
        {
            시나리오Id = "prepared-meat-picnic",
            제목 = "가공육 피크닉 꾸러미",
            소개 = "보관 방법과 나눌 수량을 맞추며 냉장 재료 공동구매를 연습합니다.",
            상품키 = "hs-food-1602-prepared-meat",
            상품명 = "가공육 세트",
            HS코드후보 = "1602.49",
            온도코드 = "냉장",
            배송권키 = "practice-neighborhood-c",
            배송권명 = "연습 동네 C",
            기본희망수량 = 4,
            수량단위 = "kg",
            목표참여자수 = 6,
            목표수량 = 20,
            연습기준단가 = 7200,
            샘플안내 = "구성·원재료·가공 방식이 확인되기 전에는 실제 HS 코드를 확정할 수 없습니다."
        }
    ];

    private static readonly IReadOnlyList<연습이웃> 연습이웃들 =
    [
        new("practice-mint", "연습 이웃 · 민트", "🌿", "처음이라 보관 방법부터 같이 배우고 싶어요.", 3, 1),
        new("practice-tofu", "연습 이웃 · 두부", "🍲", "이 재료로 만들 수 있는 요리를 서로 추천해 보고 싶어요.", 4, 1),
        new("practice-cloud", "연습 이웃 · 구름", "☁️", "수령 시간을 미리 맞추면 마음이 놓일 것 같아요.", 5, 2),
        new("practice-plum", "연습 이웃 · 매실", "🍑", "필요한 만큼만 나누어 음식 낭비를 줄이고 싶어요.", 6, 2),
        new("practice-star", "연습 이웃 · 별", "⭐", "목표가 채워져도 실제 구매 전에는 조건을 다시 확인하고 싶어요.", 7, 3)
    ];

    public IReadOnlyList<공동구매체험시나리오응답> 시나리오목록()
        => 시나리오들.Select(복제).ToArray();

    public 공동구매체험응답 시뮬레이션(공동구매체험요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var 시나리오 = 시나리오들.FirstOrDefault(item =>
            string.Equals(item.시나리오Id, request.시나리오Id?.Trim(), StringComparison.Ordinal))
            ?? throw new InvalidOperationException("선택한 공동구매 연습 시나리오를 찾을 수 없습니다.");
        if (request.내희망수량 <= 0 || request.내희망수량 > 100_000)
        {
            throw new InvalidOperationException("연습 희망 수량은 0보다 크고 100,000 이하여야 합니다.");
        }

        var 라운드 = Math.Clamp(request.라운드, 0, 최대라운드);
        var 세션Id = 세션Id정규화(request.세션Id);
        var 선택된이웃 = 연습이웃들.Where(item => item.합류라운드 <= 라운드).ToArray();
        var 참여자목록 = new List<공동구매체험참여자응답>
        {
            new()
            {
                참여자키 = "practice-self",
                표시명 = "나",
                이모지 = "🙋",
                한줄소개 = "실제 주문 없이 공동구매 흐름을 연습하고 있습니다.",
                희망수량 = request.내희망수량,
                수량단위 = 시나리오.수량단위,
                가상참여자여부 = false,
                합류라운드 = 0
            }
        };
        참여자목록.AddRange(선택된이웃.Select(item => new 공동구매체험참여자응답
        {
            참여자키 = item.참여자키,
            표시명 = item.표시명,
            이모지 = item.이모지,
            한줄소개 = item.한줄소개,
            희망수량 = item.희망수량,
            수량단위 = 시나리오.수량단위,
            가상참여자여부 = true,
            합류라운드 = item.합류라운드
        }));

        var 수요목록 = 참여자목록.Select(item => new 공동구매자동수요응답
        {
            수요출처키 = $"{세션Id}:{item.참여자키}",
            주문자키 = item.참여자키,
            주문자표시명 = item.표시명,
            상품키 = 시나리오.상품키,
            상품명 = 시나리오.상품명,
            배송권키 = 시나리오.배송권키,
            배송권명 = 시나리오.배송권명,
            거래유형 = 공동구매거래유형코드.B2C,
            가격표시기준 = 공동구매가격표시기준코드.부가세포함,
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제,
            희망수량 = item.희망수량,
            수량단위 = item.수량단위,
            목표참여자수 = 시나리오.목표참여자수,
            목표수량 = 시나리오.목표수량
        }).ToArray();
        var 기준시각Utc = timeProvider.GetUtcNow().UtcDateTime;
        var 진행 = 집단화Engine.진행계산(
            수요목록,
            시나리오.목표참여자수,
            시나리오.목표수량,
            기준시각Utc: 기준시각Utc,
            거래유형: 공동구매거래유형코드.B2C);
        var 완료여부 = 라운드 >= 최대라운드 && 진행.모집조건충족여부;
        var 절감률 = 연습절감률(진행, 시나리오);

        return new 공동구매체험응답
        {
            세션Id = 세션Id,
            시뮬레이션여부 = true,
            서버저장여부 = false,
            외부효과발생여부 = false,
            현재라운드 = 라운드,
            최대라운드 = 최대라운드,
            현재단계코드 = 단계코드(라운드, 완료여부),
            완료여부 = 완료여부,
            시나리오 = 복제(시나리오),
            참여자목록 = 참여자목록,
            대화목록 = 대화목록(request, 참여자목록),
            진행 = 진행,
            연습예상단가 = Math.Round(시나리오.연습기준단가 * (1 - 절감률), 0),
            연습절감률 = 절감률,
            통화코드 = 시나리오.통화코드,
            친해지기질문 = 친해지기질문(request.대화주제코드),
            응원메시지 = 응원메시지(라운드, 완료여부, 진행),
            다음행동라벨 = 완료여부 ? "실제 비구속 수요를 새로 확인하기" : "가상 이웃 한 팀 더 만나기",
            실제수요전환상품키 = 시나리오.상품키,
            실제수요전환안내 = "연습 결과는 복사하거나 자동 제출하지 않습니다. 실제 수요 화면에서 내 수량·배송권·비구속 동의를 다시 확인해야 합니다.",
            안전경계안내 =
            [
                "연습 이웃은 실제 사용자가 아니라 명시적으로 표시된 가상 참여자입니다.",
                "연습 세션과 대화는 서버 원장이나 사용자 계정에 저장하지 않습니다.",
                "실제 주문·결제·계약·통관 신고·연락처 전달·운송은 발생하지 않습니다.",
                "HS 코드 후보와 예상 단가는 학습용 예시이며 전문가 검토나 실제 견적을 대체하지 않습니다."
            ]
        };
    }

    private static 공동구매체험시나리오응답 복제(공동구매체험시나리오응답 source)
        => new()
        {
            시나리오Id = source.시나리오Id,
            제목 = source.제목,
            소개 = source.소개,
            상품키 = source.상품키,
            상품명 = source.상품명,
            HS코드후보 = source.HS코드후보,
            온도코드 = source.온도코드,
            배송권키 = source.배송권키,
            배송권명 = source.배송권명,
            기본희망수량 = source.기본희망수량,
            수량단위 = source.수량단위,
            목표참여자수 = source.목표참여자수,
            목표수량 = source.목표수량,
            연습기준단가 = source.연습기준단가,
            통화코드 = source.통화코드,
            샘플안내 = source.샘플안내
        };

    private static string 세션Id정규화(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return $"practice-{Guid.NewGuid():N}";
        }

        var normalized = value.Trim();
        return normalized.Length <= 80 ? normalized : normalized[..80];
    }

    private static decimal 연습절감률(
        공동구매자동집단진행응답 진행,
        공동구매체험시나리오응답 시나리오)
    {
        var 참여자진행률 = Math.Min(1m, (decimal)진행.참여자수 / 시나리오.목표참여자수);
        var 수량진행률 = Math.Min(1m, 진행.총희망수량 / 시나리오.목표수량);
        var 달성률 = Math.Min(참여자진행률, 수량진행률);
        return Math.Round(달성률 * 0.15m, 4);
    }

    private static string 단계코드(int 라운드, bool 완료여부)
        => 완료여부
            ? 공동구매체험단계코드.실제수요준비
            : 라운드 switch
            {
                0 => 공동구매체험단계코드.재료선택,
                1 => 공동구매체험단계코드.이웃만남,
                _ => 공동구매체험단계코드.조건맞추기
            };

    private static IReadOnlyList<공동구매체험대화응답> 대화목록(
        공동구매체험요청 request,
        IReadOnlyList<공동구매체험참여자응답> 참여자목록)
    {
        var topic = 공동구매체험대화주제코드.정규화(request.대화주제코드);
        var messages = new List<공동구매체험대화응답>
        {
            new()
            {
                발화자 = "나",
                본문 = topic switch
                {
                    공동구매체험대화주제코드.요리이야기 => "이 재료로 어떤 요리를 해보고 싶은지 궁금해요.",
                    공동구매체험대화주제코드.보관방법 => "처음이라 안전하게 보관하는 방법부터 같이 정하고 싶어요.",
                    공동구매체험대화주제코드.수령방법 => "서로 무리 없는 수령 시간과 장소를 먼저 맞춰봐요.",
                    _ => "저도 처음이라 천천히 연습하면서 같이 배우고 싶어요."
                },
                가상대화여부 = false
            }
        };
        messages.AddRange(참여자목록
            .Where(item => item.가상참여자여부)
            .Select(item => new 공동구매체험대화응답
            {
                발화자 = item.표시명,
                본문 = item.한줄소개,
                가상대화여부 = true
            }));
        return messages;
    }

    private static string 친해지기질문(string? topic)
        => 공동구매체험대화주제코드.정규화(topic) switch
        {
            공동구매체험대화주제코드.요리이야기 => "이 재료로 가장 먼저 만들어 보고 싶은 음식은 무엇인가요?",
            공동구매체험대화주제코드.보관방법 => "함께 사기 전에 꼭 확인하고 싶은 보관 조건은 무엇인가요?",
            공동구매체험대화주제코드.수령방법 => "모두가 부담 없이 받을 수 있는 시간대는 언제인가요?",
            _ => "공동구매가 처음인 사람에게 어떤 설명이 있으면 마음이 놓일까요?"
        };

    private static string 응원메시지(
        int 라운드,
        bool 완료여부,
        공동구매자동집단진행응답 진행)
    {
        if (완료여부)
        {
            return "연습 목표를 달성했습니다. 실제 수요는 새 화면에서 다시 확인한 뒤에만 등록됩니다.";
        }

        return 라운드 == 0
            ? "먼저 내 필요를 말해 보았습니다. 다음에는 가상 이웃의 생각을 들어봅니다."
            : $"가상 이웃 {진행.참여자수 - 1}명과 조건을 맞추는 중입니다. 아직 실제 구매는 일어나지 않습니다.";
    }

    private sealed record 연습이웃(
        string 참여자키,
        string 표시명,
        string 이모지,
        string 한줄소개,
        decimal 희망수량,
        int 합류라운드);
}
