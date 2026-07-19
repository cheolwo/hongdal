namespace 살뜰.Services.Options;

public sealed class 교육기관제출Options
{
    public const string SectionName = "EducationSubmissions";

    public bool 자동전송활성화 { get; set; }
    public int 조회주기초 { get; set; } = 30;
    public int 최대시도횟수 { get; set; } = 5;
    public 교육기관SmtpOptions Smtp { get; set; } = new();
    public Dictionary<string, 교육기관Api제출처Options> Api제출처 { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class 교육기관SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = "살뜰 현장 체험 활동";
}

public sealed class 교육기관Api제출처Options
{
    public string Url { get; set; } = string.Empty;
    public string? ApiKeyHeaderName { get; set; }
    public string? ApiKey { get; set; }
}
