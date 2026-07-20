using Ssalddel.Contracts.Common.Inbound;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed record 입고작업보드상태(
    string 현재단계,
    string 다음행동,
    string 안내,
    bool 상태전이후보);

/// <summary>서버 입고 상태를 작업 보드의 읽기 전용 다음 단계 안내로 변환합니다.</summary>
public static class 입고작업보드정책
{
    public static 입고작업보드상태 해석(string? status)
        => status?.Trim() switch
        {
            입고상태코드.예정 => new(
                "입고 예정",
                "도착·상품 확인",
                "서버 상태상 입고 확인을 준비할 수 있습니다. 이 조회 페이지에서는 상태를 변경하지 않습니다.",
                true),
            입고상태코드.운송중 => new(
                "운송 중",
                "도착 확인·검수 준비",
                "도착 사실을 확인한 뒤 검수 단계로 이어질 수 있습니다. 실제 검수는 Simulation 경계에서 별도로 실행합니다.",
                true),
            입고상태코드.완료 => new(
                "입고 완료",
                "재고 확인",
                "입고 상태 전이는 끝났습니다. 생성된 재고는 별도 재고 화면에서 다시 조회합니다.",
                false),
            입고상태코드.취소 => new(
                "입고 취소",
                "추가 작업 없음",
                "취소된 입고 요청이므로 검수·적재 작업을 시작할 수 없습니다.",
                false),
            _ => new(
                "상태 확인 필요",
                "관리자 확인",
                "알 수 없는 서버 상태에서는 후속 작업을 허용하지 않습니다.",
                false)
        };
}
