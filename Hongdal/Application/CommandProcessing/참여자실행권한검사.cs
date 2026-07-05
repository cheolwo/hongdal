using 홍달.도메인.사용자;
using 홍달.Data;

namespace Hongdal.Application.CommandProcessing;

public interface I참여자실행권한검사
{
    bool Try검증(string? 현재사용자Id, string? 현재보안역할, string 참여자Id, 홍달역할유형 실행역할, out string 오류메시지);
}

public sealed class 참여자실행권한검사 : I참여자실행권한검사
{
    public bool Try검증(
        string? 현재사용자Id,
        string? 현재보안역할,
        string 참여자Id,
        홍달역할유형 실행역할,
        out string 오류메시지)
    {
        if (string.IsNullOrWhiteSpace(현재사용자Id))
        {
            오류메시지 = "인증된 사용자 정보를 찾을 수 없습니다.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(참여자Id))
        {
            오류메시지 = "참여자Id는 필수입니다.";
            return false;
        }

        if (!string.Equals(현재사용자Id, 참여자Id, StringComparison.Ordinal))
        {
            오류메시지 = "요청 참여자와 인증 사용자가 일치하지 않습니다.";
            return false;
        }

        if (역할검증불필요(실행역할))
        {
            오류메시지 = string.Empty;
            return true;
        }

        var 허용역할 = Get허용보안역할(실행역할);
        if (허용역할.Length == 0)
        {
            오류메시지 = string.Empty;
            return true;
        }

        if (허용역할.Any(role => string.Equals(role, 현재보안역할, StringComparison.OrdinalIgnoreCase)))
        {
            오류메시지 = string.Empty;
            return true;
        }

        오류메시지 = $"실행역할({실행역할})에 필요한 보안 역할이 없습니다.";
        return false;
    }

    private static bool 역할검증불필요(홍달역할유형 실행역할)
    {
        return 실행역할 is 홍달역할유형.주문자
            or 홍달역할유형.교육참여자
            or 홍달역할유형.모임참여자;
    }

    private static string[] Get허용보안역할(홍달역할유형 실행역할)
    {
        return 실행역할 switch
        {
            홍달역할유형.기사 => [역할명.기사, 역할명.용달기사, 역할명.배달기사],
            홍달역할유형.판매자 => [역할명.판매자, 역할명.화주],
            홍달역할유형.창고관리자 => [역할명.창고관리자, 역할명.서버관리자],
            홍달역할유형.관세사 => [역할명.관세사, 역할명.서버관리자],
            홍달역할유형.운영자 => [역할명.서버관리자],
            _ => []
        };
    }
}
