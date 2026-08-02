namespace Ssalddel.Domain.Content;

public enum 앱문맥이미지품질상태
{
    미검토 = 0,
    사용가능 = 1,
    보정필요 = 2,
    제외 = 3
}

public sealed class 앱문맥이미지자산
{
    public long Id { get; set; }
    public string 장면Key { get; set; } = string.Empty;
    public string 앱PackId { get; set; } = string.Empty;
    public int 장면번호 { get; set; }
    public int PromptVersion { get; set; }
    public string 제목 { get; set; } = string.Empty;
    public string 대체Text { get; set; } = string.Empty;
    public string 이미지Url { get; set; } = string.Empty;
    public string StorageContainer { get; set; } = string.Empty;
    public string StorageObjectName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
    public string 화면비율 { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public string RouteRefsJson { get; set; } = "[]";
    public 앱문맥이미지품질상태 품질상태 { get; set; } = 앱문맥이미지품질상태.미검토;
    public bool 활성화여부 { get; set; } = true;
    public DateTimeOffset 생성시각 { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset 수정시각 { get; set; } = DateTimeOffset.UtcNow;
}
