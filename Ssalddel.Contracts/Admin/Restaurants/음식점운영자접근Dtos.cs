namespace Ssalddel.Contracts.Admin.Restaurants;

public sealed class 음식점운영자접근배정요청
{
    public string UserId { get; set; } = string.Empty;

    public long 음식점Id { get; set; }
}

public sealed class 음식점운영자접근응답
{
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public long? 음식점Id { get; set; }

    public bool 음식점역할보유 { get; set; }

    public bool 접근가능 { get; set; }
}
