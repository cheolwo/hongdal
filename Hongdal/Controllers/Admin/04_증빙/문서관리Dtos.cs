using Microsoft.AspNetCore.Http;

namespace Hongdal.Controllers.Admin.Evidence04;

public sealed class 문서업로드요청
{
    public IFormFile File { get; set; } = null!;
    public string 의뢰Id { get; set; } = string.Empty;
    public long? 배송운송Id { get; set; }
    public string 문서코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public bool? 암호화여부 { get; set; }
    public bool? 다운로드허용여부 { get; set; }
}
