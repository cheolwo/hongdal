namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public sealed record 공통홈베스트글요약(
    long? 게시글Id,
    string? 추천글제목,
    string 제목,
    string 분류,
    string 작성자,
    int 추천수,
    int 댓글수,
    bool 실시간인기,
    DateTime 최근활동일시);
