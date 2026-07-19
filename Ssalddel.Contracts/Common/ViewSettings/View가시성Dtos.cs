namespace Ssalddel.Contracts.Common.ViewSettings;

public sealed class View가시성목록응답
{
    public IReadOnlyList<View가시성항목응답> Items { get; set; } = [];
}

public sealed class View가시성항목응답
{
    public string AppKey { get; set; } = string.Empty;
    public string ViewKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool PolicyEnabled { get; set; }
    public bool UserVisible { get; set; }
    public bool EffectiveVisible { get; set; }
    public int SortOrder { get; set; }
}

public sealed class 사용자View가시성수정요청
{
    public string AppKey { get; set; } = string.Empty;
    public string ViewKey { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
}

public sealed class 관리자View정책목록응답
{
    public IReadOnlyList<관리자View정책항목응답> Items { get; set; } = [];
}

public sealed class 관리자View정책항목응답
{
    public long Id { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public string ViewKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool PolicyEnabled { get; set; }
    public int SortOrder { get; set; }
}

public sealed class 관리자View정책수정요청
{
    public bool PolicyEnabled { get; set; }
}
