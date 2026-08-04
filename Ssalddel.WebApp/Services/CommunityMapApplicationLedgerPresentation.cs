using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Privacy;

namespace Ssalddel.WebApp.Services;

public sealed record CommunityMapApplicationLedgerBadge(
    string KindLabel,
    string WorkLabel,
    string StateLabel,
    string StepLabel,
    string ActionLabel);

public static class CommunityMapApplicationLedgerPresentation
{
    public static CommunityMapApplicationLedgerBadge For(지도신청가원장Response ledger)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        return new(
            ledger.실원장전환됨 ? "내 신청 원장" : "내 가원장",
            WorkLabel(ledger.업무Code),
            StateLabel(ledger),
            StepLabel(ledger.현재단계Key),
            ledger.동의철회보류 ? "동의 철회 검토 상태 선택" : "이 원장 선택");
    }

    private static string WorkLabel(string workCode)
        => workCode switch
        {
            신청개인정보업무Codes.물류대행 => "물류대행",
            신청개인정보업무Codes.운송대행 => "운송대행",
            신청개인정보업무Codes.개별주문 => "개별 주문",
            _ => "지도 신청"
        };

    private static string StateLabel(지도신청가원장Response ledger)
    {
        if (ledger.동의철회보류)
        {
            return "동의 철회 검토";
        }
        if (ledger.운송취소검토상태Code == 지도신청가원장정책.운송취소검토요청됨Code)
        {
            return "운송 취소 검토대기";
        }
        if (ledger.운영신청취소됨)
        {
            return "신청 취소";
        }

        return string.IsNullOrWhiteSpace(ledger.상태) ? "상태 확인 중" : ledger.상태;
    }

    private static string StepLabel(string stepKey)
        => stepKey switch
        {
            지도신청가원장정책.신청접수단계 => "신청서 작성",
            지도신청가원장정책.신청제출단계 => "신청 제출",
            지도신청가원장정책.동의철회확인단계 => "동의 철회 확인",
            지도신청가원장정책.운영신청취소단계 => "운영 신청 취소",
            지도신청가원장정책.운송취소검토단계 => "운송 취소 검토",
            _ => string.IsNullOrWhiteSpace(stepKey) ? "단계 확인 중" : stepKey
        };
}
