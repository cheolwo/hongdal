namespace Ssalddel.Contracts.Common.Orderer;

/// <summary>
/// 주문자가 실제로 수령할 개인 몫을 어떤 음식에 활용할 수 있는지 확인하는 읽기 전용 요청입니다.
/// 같이 주문 전체 모집 수량과 개인 수령 수량을 혼동하지 않도록 별도 필드로 받습니다.
/// </summary>
public sealed class 같이주문레시피활용조회요청
{
    public string 상품키 { get; set; } = string.Empty;

    public string 상품명 { get; set; } = string.Empty;

    public string? 식재료키 { get; set; }

    public decimal 개인수령검토수량 { get; set; }

    public string 수량단위 { get; set; } = string.Empty;

    public int 최대레시피수 { get; set; } = 3;
}

public sealed class 같이주문레시피활용응답
{
    public string 상품키 { get; set; } = string.Empty;

    public string 상품명 { get; set; } = string.Empty;

    public decimal 개인수령검토수량 { get; set; }

    public string 수량단위 { get; set; } = string.Empty;

    public string 일치식재료키 { get; set; } = string.Empty;

    public string 일치식재료명 { get; set; } = string.Empty;

    public string 조회상태코드 { get; set; } = string.Empty;

    public string 안내 { get; set; } = string.Empty;

    public bool 같이주문자동전환금지 { get; set; } = true;

    public bool 같이주문별도동의필수 { get; set; } = true;

    public bool 정확한소진횟수계산가능 { get; set; }

    public string 수량판단제한 { get; set; } = string.Empty;

    public IReadOnlyList<같이주문레시피활용항목응답> 활용음식 { get; set; } = [];

    public IReadOnlyList<string> 판단도움말 { get; set; } = [];
}

public sealed class 같이주문레시피활용항목응답
{
    public string DishKey { get; set; } = string.Empty;

    public string DishName { get; set; } = string.Empty;

    public string RecipeTitle { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string RegionName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string IngredientQuantityText { get; set; } = string.Empty;

    public string IngredientUnitText { get; set; } = string.Empty;

    public string PreparationNote { get; set; } = string.Empty;

    public string SourceProvider { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public DateTime SourceUpdatedAtUtc { get; set; }

    public bool IsFreshForPublication { get; set; }
}

public static class 같이주문레시피활용조회상태코드
{
    public const string 일치자료있음 = "Matched";

    public const string 일치식재료없음 = "IngredientNotMatched";

    public const string 활용레시피없음 = "RecipeNotFound";
}
