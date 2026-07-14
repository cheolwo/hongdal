namespace Hongdal.Contracts.Common.Content;

public sealed record YouTube감시채널등록요청Dto(
    string ChannelId,
    string? 표시이름 = null);

public sealed record YouTube영상공개설정요청Dto(bool 공개여부);

public sealed record YouTube감시채널Dto(
    string ChannelId,
    string 채널명,
    string? 썸네일Url,
    bool 활성화여부,
    bool 초기동기화완료여부,
    DateTime? 마지막동기화일시Utc,
    string? 마지막영상Id,
    DateTime? 마지막영상게시일시Utc);

public sealed record YouTube채널영상Dto(
    string VideoId,
    string ChannelId,
    string 채널명,
    string 제목,
    string 설명,
    DateTime 게시일시Utc,
    string? 썸네일Url,
    string 시청Url,
    bool 신규업로드여부,
    string 공유상태,
    DateTime 최초감지일시Utc);

public sealed record YouTube채널동기화결과Dto(
    bool 실행됨,
    int 처리채널수,
    int 수신영상수,
    int 추가영상수,
    int 신규업로드수,
    DateTime? 완료일시Utc,
    string 메시지);

public sealed record YouTube재생목록Dto(
    string PlaylistId,
    string ChannelId,
    string 제목,
    string 설명,
    DateTime 게시일시Utc,
    int 영상수,
    string? 썸네일Url,
    string 재생목록Url);

public sealed record YouTube재생목록영상Dto(
    string VideoId,
    string ChannelId,
    string 제목,
    string 설명,
    DateTime 게시일시Utc,
    string? 썸네일Url,
    string 시청Url);
