using Microsoft.AspNetCore.Http;

namespace Ssalddel.Controllers.Platform;

public sealed class 파일업로드요청
{
    public IFormFile File { get; set; } = null!;
    public string CommandName { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
}
