namespace Hongdal.Controllers.Common;

public sealed class 파일업로드응답
{
    public string BucketName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
