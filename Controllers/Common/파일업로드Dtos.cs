using Microsoft.AspNetCore.Http;

namespace Hongdal.Controllers.Common;

public sealed class 파일업로드요청
{
    public IFormFile File { get; set; } = null!;
    public string? Folder { get; set; }
}
