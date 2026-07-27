namespace Ssalddel.Contracts.Food;

public static class 음식점조리시간정책
{
    public const int 최소조리분 = 1;
    public const int 최대조리분 = 180;

    public static int 주문추천분(
        IEnumerable<음식주문상품Dto> 상품목록,
        IReadOnlyDictionary<string, int>? 상품별기본조리분,
        int 음식점기본조리분)
    {
        ArgumentNullException.ThrowIfNull(상품목록);

        var 기본조리분 = Clamp(음식점기본조리분);
        var 상품기본값 = 상품별기본조리분 ?? new Dictionary<string, int>();
        var 주문상품 = 상품목록
            .Where(item => !string.IsNullOrWhiteSpace(item.상품명))
            .ToArray();

        if (주문상품.Length == 0)
        {
            return 기본조리분;
        }

        return 주문상품.Max(item =>
            상품기본분(item.상품명, 상품기본값, 기본조리분));
    }

    public static int 상품기본분(
        string? 상품명,
        IReadOnlyDictionary<string, int>? 상품별기본조리분,
        int 음식점기본조리분)
    {
        var 기본조리분 = Clamp(음식점기본조리분);
        if (string.IsNullOrWhiteSpace(상품명) || 상품별기본조리분 is null)
        {
            return 기본조리분;
        }

        var 설정 = 상품별기본조리분.FirstOrDefault(candidate =>
            string.Equals(
                candidate.Key?.Trim(),
                상품명.Trim(),
                StringComparison.OrdinalIgnoreCase));

        return string.IsNullOrWhiteSpace(설정.Key)
            ? 기본조리분
            : Clamp(설정.Value);
    }

    public static int Clamp(int 조리분)
        => Math.Clamp(조리분, 최소조리분, 최대조리분);
}
