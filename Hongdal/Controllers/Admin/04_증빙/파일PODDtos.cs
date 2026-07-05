using Microsoft.AspNetCore.Http;

namespace Hongdal.Controllers.Admin.Evidence04;

public sealed class 파일POD업로드요청
{
    public IFormFile File { get; set; } = null!;
    public string FileType { get; set; } = string.Empty;
    public string? RequestId { get; set; }
}

public sealed class 파일POD상태변경요청
{
    public string UploadStatus { get; set; } = string.Empty;
}
