namespace Ssalddel.Contracts.Food;

/// <summary>
/// 음식점 운영자 계정이 접근할 수 있는 음식점 원장 범위를 나타냅니다.
/// 역할만으로 음식점 ID를 선택하지 않고, 서버가 발급한 사용자 클레임으로 범위를 고정합니다.
/// </summary>
public static class 음식점접근ClaimTypes
{
    public const string 음식점Id = "ssalddel:restaurant_id";
}
