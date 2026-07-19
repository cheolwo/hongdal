namespace Ssalddel.Contracts.Common.Speech;

public sealed record Typecast음성모델Dto(
    string 버전,
    IReadOnlyList<string> 지원감정);

public sealed record Typecast음성캐릭터Dto(
    string VoiceId,
    string 이름,
    string 성별,
    string 연령대,
    string 음성유형,
    IReadOnlyList<string> 용도,
    IReadOnlyList<Typecast음성모델Dto> 지원모델,
    bool 활성화여부,
    DateTime 마지막동기화일시Utc);

public sealed record Typecast음성카탈로그동기화결과Dto(
    bool 실행됨,
    int 수신수,
    int 추가수,
    int 수정수,
    int 비활성화수,
    DateTime? 동기화일시Utc,
    string 메시지);
