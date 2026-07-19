namespace Ssalddel.Contracts.Common.Exploration;

public static class 운행탐색상태값
{
    public const string 초안 = "초안";
    public const string 탐색중 = "탐색중";
    public const string 응답수집중 = "응답수집중";
    public const string 응답부족 = "응답부족";
    public const string 실행검토 = "실행검토";
    public const string 확정연결대기 = "확정연결대기";
    public const string 종료 = "종료";
    public const string 취소 = "취소";
}

public static class 탐색캠페인상태값
{
    public const string 초안 = 운행탐색상태값.초안;
    public const string 탐색중 = 운행탐색상태값.탐색중;
    public const string 응답수집중 = 운행탐색상태값.응답수집중;
    public const string 응답부족 = 운행탐색상태값.응답부족;
    public const string 실행검토 = 운행탐색상태값.실행검토;
    public const string 확정연결대기 = 운행탐색상태값.확정연결대기;
    public const string 종료 = 운행탐색상태값.종료;
    public const string 취소 = 운행탐색상태값.취소;
}

public static class 운행문의대상상태값
{
    public const string 선정됨 = "선정됨";
    public const string 발송됨 = "발송됨";
    public const string 열람함 = "열람함";
    public const string 있음응답 = "있음응답";
    public const string 없음응답 = "없음응답";
    public const string 미정응답 = "미정응답";
    public const string 나중응답 = "나중응답";
    public const string 만료 = "만료";
}

public static class 탐색캠페인대상상태값
{
    public const string 선정됨 = 운행문의대상상태값.선정됨;
    public const string 발송됨 = 운행문의대상상태값.발송됨;
    public const string 열람함 = 운행문의대상상태값.열람함;
    public const string 있음응답 = 운행문의대상상태값.있음응답;
    public const string 없음응답 = 운행문의대상상태값.없음응답;
    public const string 미정응답 = 운행문의대상상태값.미정응답;
    public const string 나중응답 = 운행문의대상상태값.나중응답;
    public const string 만료 = 운행문의대상상태값.만료;
}

public enum 운행문의응답유형
{
    있음,
    없음,
    미정,
    나중에연락
}
