namespace Hongdal.Ui.Common.Areas.App.Models;

public static class 운송정보입력정책
{
    public const string 일반화물 = "일반 화물(박스/팔레트)";
    public const string 컨테이너Fcl = "컨테이너(FCL)";
    public const string 혼재컨테이너Lcl = "혼재 컨테이너(LCL)";
    public const string 벌크비포장 = "벌크/비포장";
    public const string 기타현장확인 = "기타/현장 확인";

    public static readonly IReadOnlyList<string> 화물적재형태목록 =
    [
        일반화물,
        컨테이너Fcl,
        혼재컨테이너Lcl,
        벌크비포장,
        기타현장확인
    ];

    public static IReadOnlyList<운송정보입력요구사항> Get요구사항(string? 화물적재형태)
    {
        return Normalize(화물적재형태) switch
        {
            컨테이너Fcl => [
                new("상차지", "컨테이너 반출지, 항만, CY 또는 보세구역을 입력합니다."),
                new("하차지", "컨테이너 반입지, 3PL 창고, 공장 또는 현장 주소를 입력합니다."),
                new("요청사항", "컨테이너 번호, 규격, 반출 가능 시간, 반입 예약, 하역 장비 조건을 남깁니다.")
            ],
            혼재컨테이너Lcl => [
                new("상차지", "CFS, 보세창고, 배송대행지 또는 국내 인수 장소를 입력합니다."),
                new("하차지", "3PL 창고, 공동주문 수령지 또는 최종 배송지를 입력합니다."),
                new("요청사항", "박스/팔레트 수, 입고 예약, 분류 필요 여부, 창고 담당자 확인사항을 남깁니다.")
            ],
            벌크비포장 => [
                new("상차지", "현장 진입 가능 여부와 상차 장비가 있는 장소를 입력합니다."),
                new("하차지", "하역 장비, 지게차, 크레인 또는 작업 인력 조건이 맞는 장소를 입력합니다."),
                new("요청사항", "상하차 장비, 고박, 덮개, 수작업 필요 여부를 남깁니다.")
            ],
            기타현장확인 => [
                new("상차지", "현장 담당자가 확인할 수 있는 상차 주소와 연락처를 입력합니다."),
                new("하차지", "현장 담당자가 확인할 수 있는 하차 주소와 연락처를 입력합니다."),
                new("요청사항", "운송 전에 확인해야 할 특수 조건을 남깁니다.")
            ],
            _ => [
                new("상차지", "화물을 실을 주소와 담당자 연락처를 입력합니다."),
                new("하차지", "화물을 내릴 주소와 담당자 연락처를 입력합니다."),
                new("요청사항", "상하차 도움, 엘리베이터, 지게차, 시간 제한이 있으면 남깁니다.")
            ]
        };
    }

    public static string Get안내문구(string? 화물적재형태)
    {
        return Normalize(화물적재형태) switch
        {
            컨테이너Fcl => "FCL 컨테이너는 반출지, 반입지, 컨테이너 규격과 반출 가능 시간이 운송 가능 여부를 좌우합니다.",
            혼재컨테이너Lcl => "LCL 화물은 CFS/보세창고, 박스 또는 팔레트 수, 입고 예약 조건을 함께 확인해야 합니다.",
            벌크비포장 => "벌크 또는 비포장 화물은 상하차 장비와 고박 조건이 먼저 확인되어야 합니다.",
            기타현장확인 => "기타 화물은 현장 담당자가 확인할 수 있는 조건을 요청사항에 남겨야 합니다.",
            _ => "일반 박스/팔레트 화물은 상차지, 하차지, 연락처와 현장 요청사항이 핵심입니다."
        };
    }

    public static string Get권장운송방식(string? 화물적재형태)
    {
        return Normalize(화물적재형태) switch
        {
            컨테이너Fcl => "단독",
            벌크비포장 => "단독",
            혼재컨테이너Lcl => "혼적",
            _ => "혼적"
        };
    }

    public static string Get요청사항힌트(string? 화물적재형태)
    {
        return Normalize(화물적재형태) switch
        {
            컨테이너Fcl => "컨테이너 번호, 20ft/40ft, 반출 가능 시간, 반입 예약, 하역 장비 조건",
            혼재컨테이너Lcl => "CFS/보세창고, 박스/팔레트 수, 입고 예약, 분류 필요 여부",
            벌크비포장 => "지게차/크레인, 고박, 덮개, 상하차 인력, 현장 진입 제한",
            기타현장확인 => "현장 확인이 필요한 특수 조건",
            _ => "상하차 도움, 엘리베이터, 지게차, 시간 제한 등"
        };
    }

    public static IReadOnlyList<string> Get경고목록(
        string? 화물적재형태,
        string? 상차도로명주소,
        string? 하차도로명주소,
        string? 요청사항)
    {
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(상차도로명주소))
        {
            warnings.Add("상차지 주소를 입력해야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(하차도로명주소))
        {
            warnings.Add("하차지 주소를 입력해야 합니다.");
        }

        var normalized = Normalize(화물적재형태);
        if ((normalized is 컨테이너Fcl or 혼재컨테이너Lcl or 벌크비포장 or 기타현장확인)
            && string.IsNullOrWhiteSpace(요청사항))
        {
            warnings.Add($"{normalized} 운송은 요청사항에 '{Get요청사항힌트(normalized)}'를 남겨야 합니다.");
        }

        return warnings;
    }

    private static string Normalize(string? 화물적재형태)
    {
        if (string.IsNullOrWhiteSpace(화물적재형태))
        {
            return 일반화물;
        }

        var normalized = 화물적재형태.Trim();
        return 화물적재형태목록.Contains(normalized, StringComparer.Ordinal)
            ? normalized
            : 기타현장확인;
    }
}

public sealed record 운송정보입력요구사항(
    string 제목,
    string 설명);
