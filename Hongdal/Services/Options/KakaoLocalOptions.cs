namespace 홍달.Services.Options;

public sealed class KakaoLocalOptions
{
    public const string SectionName = "KakaoLocal";

    public string BaseUrl { get; set; } = "https://dapi.kakao.com/";

    public string RestApiKey { get; set; } = string.Empty;
}
