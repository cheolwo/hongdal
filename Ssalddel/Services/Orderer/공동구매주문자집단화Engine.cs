using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Orderer;

public interface I공동구매주문자집단화Engine
{
    string 자동집단Id생성(공동구매자동수요등록Command command);

    공동구매자동집단진행응답 진행계산(
        IReadOnlyCollection<공동구매자동수요응답> 수요목록,
        int? 목표참여자수,
        decimal? 목표수량,
        string? 현재상태 = null,
        DateTime? 모집종료시각Utc = null,
        DateTime? 기준시각Utc = null,
        string? 거래유형 = null);

    공동구매자동집단배치미리보기응답 배치미리보기(
        공동구매자동수요등록Command command,
        공동구매자동집단응답? 기존집단);
}

public sealed class 공동구매주문자집단화Engine : I공동구매주문자집단화Engine
{
    private const int 기본참여자기준 = 5;
    private const int 예약결제참여자기준 = 2;
    private const decimal 기본수량기준 = 30;

    public string 자동집단Id생성(공동구매자동수요등록Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.상품키) || string.IsNullOrWhiteSpace(command.배송권키))
        {
            throw new InvalidOperationException("상품키와 배송권키를 입력해야 합니다.");
        }

        return 공동구매자동집단화계획기.자동집단키생성(
            정규화(command.상품키, "unknown-product", 120),
            정규화(command.배송권키, "unknown-scope", 160),
            정규화(command.온도코드, "상온", 40),
            정규화(command.물류방식, 공동구매자동수요물류방식코드.후속검토, 40),
            공동구매거래유형코드.정규화(command.거래유형),
            공동구매가격표시기준코드.정규화(command.가격표시기준, command.거래유형),
            정규화(command.수량단위, "kg", 20));
    }

    public 공동구매자동집단진행응답 진행계산(
        IReadOnlyCollection<공동구매자동수요응답> 수요목록,
        int? 목표참여자수,
        decimal? 목표수량,
        string? 현재상태 = null,
        DateTime? 모집종료시각Utc = null,
        DateTime? 기준시각Utc = null,
        string? 거래유형 = null)
    {
        ArgumentNullException.ThrowIfNull(수요목록);

        var 계산시각Utc = Utc시각(기준시각Utc ?? DateTime.UtcNow);
        var 유효모집종료시각Utc = 모집종료시각Utc is { } 종료시각 && 종료시각 != default
            ? Utc시각(종료시각)
            : (DateTime?)null;
        var 모집종료여부 = 유효모집종료시각Utc.HasValue
            && 계산시각Utc >= 유효모집종료시각Utc.Value;
        var 유효목표참여자수 = 양수값(목표참여자수);
        var 유효목표수량 = 양수값(목표수량);
        var 참여자묶음 = 수요목록
            .GroupBy(참여자식별키, StringComparer.Ordinal)
            .ToArray();
        var 예약결제건수 = 수요목록.Count(주문확정수요인가);
        var 참여자수 = 참여자묶음.Length;
        var 예약결제참여자수 = 참여자묶음.Count(group => group.Any(주문확정수요인가));
        var 총희망수량 = 수요목록.Sum(item => Math.Max(0, item.희망수량));
        var 제안상태 = 공동구매자동집단화계획기.상태제안(
            참여자수,
            예약결제참여자수,
            총희망수량,
            유효목표참여자수,
            유효목표수량,
            공동구매거래유형코드.정규화(거래유형));
        var 모집조건충족여부 = 현재상태 == 공동구매자동집단상태코드.확정
            || 제안상태 == 공동구매자동집단상태코드.확정대기;
        var 상태 = 현재상태 == 공동구매자동집단상태코드.확정
            ? 공동구매자동집단상태코드.확정
            : 모집종료여부 && !모집조건충족여부
                ? 공동구매자동집단상태코드.모집종료목표미달
                : 제안상태;
        var 확정검토가능 = 상태 is 공동구매자동집단상태코드.확정대기
            or 공동구매자동집단상태코드.확정;

        return new 공동구매자동집단진행응답
        {
            현재상태 = 상태,
            수요건수 = 수요목록.Count,
            예약결제건수 = 예약결제건수,
            참여자수 = 참여자수,
            예약결제참여자수 = 예약결제참여자수,
            총희망수량 = 총희망수량,
            수량단위 = 수요목록.LastOrDefault(item => !string.IsNullOrWhiteSpace(item.수량단위))?.수량단위
                ?? string.Empty,
            목표참여자수 = 유효목표참여자수,
            목표수량 = 유효목표수량,
            추가필요참여자수 = 유효목표참여자수.HasValue
                ? Math.Max(0, 유효목표참여자수.Value - 참여자수)
                : null,
            추가필요수량 = 유효목표수량.HasValue
                ? Math.Max(0, 유효목표수량.Value - 총희망수량)
                : null,
            모집종료시각Utc = 유효모집종료시각Utc ?? default,
            모집종료여부 = 모집종료여부,
            모집조건충족여부 = 모집조건충족여부,
            확정검토가능 = 확정검토가능,
            다음단계코드 = 다음단계코드(상태),
            안내 = 진행안내(
                상태,
                참여자수,
                예약결제참여자수,
                총희망수량,
                유효목표참여자수,
                유효목표수량,
                유효모집종료시각Utc,
                공동구매거래유형코드.정규화(거래유형))
        };
    }

    public 공동구매자동집단배치미리보기응답 배치미리보기(
        공동구매자동수요등록Command command,
        공동구매자동집단응답? 기존집단)
    {
        미리보기검증(command);

        var 자동집단Id = 자동집단Id생성(command);
        if (기존집단 is not null
            && !string.Equals(기존집단.자동집단Id, 자동집단Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("조회한 자동집단이 현재 배치 기준과 일치하지 않습니다.");
        }

        var 수요출처키 = 수요출처키정규화(command);
        var 주문자키 = 정규화(command.주문자키, "anonymous-orderer", 120);
        var 기존수요목록 = 기존집단?.수요목록.ToArray() ?? [];
        var 동일출처수요목록 = 기존수요목록
            .Where(item => string.Equals(item.수요출처키, 수요출처키, StringComparison.Ordinal))
            .ToArray();
        if (동일출처수요목록.Any(item =>
                !string.IsNullOrWhiteSpace(item.주문자키)
                && !string.Equals(item.주문자키, 주문자키, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("동일한 수요출처키를 다른 주문자가 사용할 수 없습니다.");
        }

        var 예상수요목록 = 기존수요목록
            .Where(item => !string.Equals(item.수요출처키, 수요출처키, StringComparison.Ordinal))
            .Append(예상수요(command, 자동집단Id, 수요출처키, 주문자키))
            .ToArray();
        var 예상목표참여자수 = 최소양수(예상수요목록.Select(item => item.목표참여자수))
            ?? (command.목표참여자수 is null ? 기존집단?.목표참여자수 : null);
        var 예상목표수량 = 최소양수(예상수요목록.Select(item => item.목표수량))
            ?? (command.목표수량 is null ? 기존집단?.목표수량 : null);
        var 기존수요갱신여부 = 동일출처수요목록.Length > 0;
        var 기준시각Utc = DateTime.UtcNow;
        var 모집종료시각Utc = 모집종료시각(기존집단, 기준시각Utc);
        var 모집종료여부 = 기준시각Utc >= 모집종료시각Utc;

        return new 공동구매자동집단배치미리보기응답
        {
            정책버전 = 공동구매주문자집단화정책코드.현재버전,
            자동집단Id = 자동집단Id,
            배치유형 = 기존집단 is null
                ? 공동구매자동집단배치유형코드.신규집단
                : 공동구매자동집단배치유형코드.기존집단,
            기존수요갱신여부 = 기존수요갱신여부,
            적용기준목록 = 배치기준(command),
            현재진행 = 진행계산(
                기존수요목록,
                기존집단?.목표참여자수,
                기존집단?.목표수량,
                기존집단?.현재상태,
                모집종료시각Utc,
                기준시각Utc,
                기존집단?.거래유형 ?? command.거래유형),
            예상진행 = 진행계산(
                예상수요목록,
                예상목표참여자수,
                예상목표수량,
                기존집단?.현재상태,
                모집종료시각Utc,
                기준시각Utc,
                command.거래유형),
            안내 = 배치안내(기존집단 is not null, 기존수요갱신여부, 모집종료여부),
            비구속안내 = "미리보기와 관심 수요 등록은 주문, 결제 또는 계약을 자동 확정하지 않습니다."
        };
    }

    private static 공동구매자동수요응답 예상수요(
        공동구매자동수요등록Command command,
        string 자동집단Id,
        string 수요출처키,
        string 주문자키)
        => new()
        {
            수요출처키 = 수요출처키,
            자동집단Id = 자동집단Id,
            상품키 = 정규화(command.상품키, "unknown-product", 120),
            상품명 = 정규화(command.상품명, command.상품키, 160),
            거래유형 = 공동구매거래유형코드.정규화(command.거래유형),
            가격표시기준 = 공동구매가격표시기준코드.정규화(command.가격표시기준, command.거래유형),
            구매조직참조키 = 정규화(command.구매조직참조키, string.Empty, 160),
            구매조직표시명 = 정규화(command.구매조직표시명, string.Empty, 160),
            사업자검증상태 = 공동구매거래유형코드.정규화(command.거래유형) == 공동구매거래유형코드.B2B
                ? 주문자집단사업자검증상태코드.필요
                : 주문자집단사업자검증상태코드.불필요,
            세금계산서필요 = command.세금계산서필요,
            주문자키 = 주문자키,
            주문자표시명 = 정규화(command.주문자표시명, "주문자", 80),
            배송권키 = 정규화(command.배송권키, "unknown-scope", 160),
            배송권명 = 정규화(command.배송권명, command.배송권키, 160),
            수요유형 = 수요유형정규화(command.수요유형),
            결제상태 = 결제상태정규화(command.결제상태),
            희망수량 = Math.Max(0, command.희망수량),
            수량단위 = 정규화(command.수량단위, "kg", 20),
            예약결제금액 = command.예약결제금액,
            목표참여자수 = 양수값(command.목표참여자수),
            목표수량 = 양수값(command.목표수량)
        };

    private static IReadOnlyList<공동구매자동집단배치기준응답> 배치기준(
        공동구매자동수요등록Command command)
        =>
        [
            new()
            {
                기준코드 = 공동구매자동집단배치기준코드.상품키,
                기준값 = 정규화(command.상품키, "unknown-product", 120)
            },
            new()
            {
                기준코드 = 공동구매자동집단배치기준코드.배송권,
                기준값 = 정규화(command.배송권키, "unknown-scope", 160)
            },
            new()
            {
                기준코드 = 공동구매자동집단배치기준코드.보관온도,
                기준값 = 정규화(command.온도코드, "상온", 40)
            },
            new()
            {
                기준코드 = 공동구매자동집단배치기준코드.물류방식,
                기준값 = 정규화(command.물류방식, 공동구매자동수요물류방식코드.후속검토, 40)
            },
            new()
            {
                기준코드 = 공동구매자동집단배치기준코드.거래유형,
                기준값 = 공동구매거래유형코드.정규화(command.거래유형)
            },
            new()
            {
                기준코드 = 공동구매자동집단배치기준코드.가격표시기준,
                기준값 = 공동구매가격표시기준코드.정규화(command.가격표시기준, command.거래유형)
            }
        ];

    private static void 미리보기검증(공동구매자동수요등록Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.상품키)
            || string.IsNullOrWhiteSpace(command.상품명)
            || string.IsNullOrWhiteSpace(command.배송권키))
        {
            throw new InvalidOperationException("상품키, 상품명, 배송권키를 입력해야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(command.주문자키))
        {
            throw new InvalidOperationException("집단화 미리보기에는 주문자 식별키가 필요합니다.");
        }

        if (command.희망수량 <= 0)
        {
            throw new InvalidOperationException("희망수량은 0보다 커야 합니다.");
        }

        거래문맥검증(command);
    }

    private static string 참여자식별키(공동구매자동수요응답 수요)
    {
        if (!string.IsNullOrWhiteSpace(수요.주문자키))
        {
            return $"orderer:{수요.주문자키.Trim()}";
        }

        if (!string.IsNullOrWhiteSpace(수요.수요출처키))
        {
            return $"source:{수요.수요출처키.Trim()}";
        }

        return "anonymous-orderer";
    }

    private static bool 주문확정수요인가(공동구매자동수요응답 수요)
        => 수요.수요유형 == 공동구매자동수요유형코드.예약결제
           || 수요.결제상태 is 공동구매자동결제상태코드.예약됨
               or 공동구매자동결제상태코드.결제확정;

    private static string 다음단계코드(string 상태)
        => 상태 switch
        {
            공동구매자동집단상태코드.확정 => 공동구매자동집단다음단계코드.확정완료,
            공동구매자동집단상태코드.확정대기 => 공동구매자동집단다음단계코드.확정검토,
            공동구매자동집단상태코드.모집종료목표미달 => 공동구매자동집단다음단계코드.모집종료,
            _ => 공동구매자동집단다음단계코드.수요추가모집
        };

    private static string 진행안내(
        string 상태,
        int 참여자수,
        int 예약결제참여자수,
        decimal 총희망수량,
        int? 목표참여자수,
        decimal? 목표수량,
        DateTime? 모집종료시각Utc,
        string 거래유형)
    {
        if (상태 == 공동구매자동집단상태코드.확정)
        {
            return "공동구매 집단이 별도 확인 절차를 거쳐 확정되었습니다.";
        }

        if (상태 == 공동구매자동집단상태코드.확정대기)
        {
            return "모집 조건을 충족했습니다. 자동 확정하지 않으며 참여 의사와 공급 조건을 별도로 확인해야 합니다.";
        }

        if (상태 == 공동구매자동집단상태코드.모집종료목표미달)
        {
            var 종료안내 = 모집종료시각Utc.HasValue
                ? $"{모집종료시각Utc.Value:yyyy-MM-dd HH:mm} UTC에"
                : "정책 기한에";
            return $"모집이 {종료안내} 종료되었고 조건을 충족하지 못했습니다. 새 모집 회차를 검토해 주세요.";
        }

        if (거래유형 == 공동구매거래유형코드.B2C && 참여자수 < 2)
        {
            return $"공동구매 후보가 되려면 서로 다른 주문자가 최소 2명 필요합니다. 현재 {참여자수}명입니다.";
        }

        if (거래유형 == 공동구매거래유형코드.B2B && 참여자수 == 1)
        {
            return $"사업 목적 수요 1곳이 모였습니다. 목표 수량·가격·사업자 확인과 공급 조건을 충족해야 계약 검토로 넘어갑니다. 현재 수량 {총희망수량:N0}입니다.";
        }

        if (목표참여자수.HasValue || 목표수량.HasValue)
        {
            var 참여자안내 = 목표참여자수.HasValue
                ? $"참여자 {참여자수}/{목표참여자수.Value}명"
                : $"참여자 {참여자수}명";
            var 수량안내 = 목표수량.HasValue
                ? $"수량 {총희망수량:N0}/{목표수량.Value:N0}"
                : $"수량 {총희망수량:N0}";
            return $"명시된 모집 목표를 더 충족해야 합니다. {참여자안내}, {수량안내}입니다.";
        }

        return $"예약 결제 참여자 {예약결제참여자수}/{예약결제참여자기준}명, 전체 참여자 {참여자수}/{기본참여자기준}명 또는 총수량 {총희망수량:N0}/{기본수량기준:N0} 중 하나를 더 충족해야 합니다.";
    }

    private static string 배치안내(bool 기존집단존재, bool 기존수요갱신, bool 모집종료여부)
    {
        if (모집종료여부)
        {
            return "같은 배치 기준의 기존 모집은 종료되었습니다. 이 미리보기는 저장되지 않으며 새 모집 회차가 필요합니다.";
        }

        if (기존수요갱신)
        {
            return "같은 수요출처의 기존 구매 의향을 새 값으로 교체할 예정입니다.";
        }

        return 기존집단존재
            ? "상품, 배송권, 보관 온도, 물류 방식과 B2B/B2C 거래 문맥이 같은 기존 후보 집단에 합산할 예정입니다."
            : "같은 배치 기준의 기존 후보가 없어 새 공동구매 후보 집단을 만들 예정입니다.";
    }

    internal static void 거래문맥검증(공동구매자동수요등록Command command)
    {
        if (!string.IsNullOrWhiteSpace(command.거래유형)
            && !공동구매거래유형코드.지원여부(command.거래유형))
        {
            throw new InvalidOperationException("공동구매 거래 유형은 B2C 또는 B2B여야 합니다.");
        }

        if (!string.IsNullOrWhiteSpace(command.가격표시기준)
            && !공동구매가격표시기준코드.지원여부(command.가격표시기준))
        {
            throw new InvalidOperationException("가격 표시 기준은 부가세 포함 또는 부가세 별도여야 합니다.");
        }

        var 거래유형 = 공동구매거래유형코드.정규화(command.거래유형);
        if (거래유형 == 공동구매거래유형코드.B2B
            && string.IsNullOrWhiteSpace(command.구매조직참조키)
            && string.IsNullOrWhiteSpace(command.구매조직표시명))
        {
            throw new InvalidOperationException("B2B 공동구매 수요에는 구매 조직 참조 또는 조직 표시명이 필요합니다.");
        }

        if (거래유형 == 공동구매거래유형코드.B2C
            && (command.세금계산서필요
                || !string.IsNullOrWhiteSpace(command.구매조직참조키)
                || !string.IsNullOrWhiteSpace(command.구매조직표시명)
                || string.Equals(
                    command.가격표시기준,
                    공동구매가격표시기준코드.부가세별도,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("B2C 수요에는 구매 조직, 세금계산서 또는 부가세 별도 조건을 지정할 수 없습니다.");
        }
    }

    private static DateTime 모집종료시각(공동구매자동집단응답? 기존집단, DateTime 기준시각Utc)
    {
        if (기존집단?.모집종료시각Utc is { } 종료시각 && 종료시각 != default)
        {
            return Utc시각(종료시각);
        }

        var 시작시각Utc = 기존집단?.생성시각Utc is { } 생성시각 && 생성시각 != default
            ? Utc시각(생성시각)
            : Utc시각(기준시각Utc);
        return 공동구매자동집단모집정책.기본모집종료시각Utc(시작시각Utc);
    }

    private static string 수요출처키정규화(공동구매자동수요등록Command command)
        => 정규화(
            command.수요출처키,
            $"orderer:{정규화(command.주문자키, "anonymous-orderer", 120)}",
            200);

    private static string 수요유형정규화(string? 값)
        => 값 == 공동구매자동수요유형코드.예약결제
            ? 공동구매자동수요유형코드.예약결제
            : 공동구매자동수요유형코드.관심표시;

    private static string 결제상태정규화(string? 값)
        => 값 switch
        {
            공동구매자동결제상태코드.예약됨 => 공동구매자동결제상태코드.예약됨,
            공동구매자동결제상태코드.결제확정 => 공동구매자동결제상태코드.결제확정,
            _ => 공동구매자동결제상태코드.미결제
        };

    private static string 정규화(string? 값, string 기본값, int 최대길이)
    {
        var 정규화값 = string.IsNullOrWhiteSpace(값) ? 기본값 : 값.Trim();
        return 정규화값.Length <= 최대길이 ? 정규화값 : 정규화값[..최대길이];
    }

    private static int? 최소양수(IEnumerable<int?> 값목록)
        => 값목록.Where(값 => 값 is > 0).Min();

    private static decimal? 최소양수(IEnumerable<decimal?> 값목록)
        => 값목록.Where(값 => 값 is > 0).Min();

    private static int? 양수값(int? 값) => 값 is > 0 ? 값 : null;

    private static decimal? 양수값(decimal? 값) => 값 is > 0 ? 값 : null;

    private static DateTime Utc시각(DateTime 값)
        => 값.Kind switch
        {
            DateTimeKind.Utc => 값,
            DateTimeKind.Local => 값.ToUniversalTime(),
            _ => DateTime.SpecifyKind(값, DateTimeKind.Utc)
        };
}
