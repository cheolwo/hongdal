namespace Hongdal.Contracts.Common.Inbound;

public static class 입고계약유형코드
{
    public const string 보관대행 = "StorageOnly";
    public const string 위탁판매 = "ConsignmentSale";
    public const string 마켓풀필먼트 = "MarketFulfillment";
    public const string 수입통관풀필먼트 = "ImportCustomsFulfillment";

    public static string Normalize(string? value)
        => value?.Trim() switch
        {
            위탁판매 => 위탁판매,
            마켓풀필먼트 => 마켓풀필먼트,
            수입통관풀필먼트 => 수입통관풀필먼트,
            _ => 보관대행
        };

    public static bool CanSellToMarket(string? value)
        => Normalize(value) is 위탁판매 or 마켓풀필먼트 or 수입통관풀필먼트;

    public static bool RequiresCustoms(string? value)
        => Normalize(value) == 수입통관풀필먼트;

    public static string GetDisplayName(string? value)
        => Normalize(value) switch
        {
            위탁판매 => "위탁 판매",
            마켓풀필먼트 => "마켓 풀필먼트",
            수입통관풀필먼트 => "수입 통관 풀필먼트",
            _ => "보관 대행"
        };
}

public sealed class 입고계약스냅샷
{
    public string 계약번호 { get; set; } = string.Empty;

    public string 계약유형 { get; set; } = 입고계약유형코드.보관대행;

    public string 계약상대방명 { get; set; } = string.Empty;

    public string 정산방식 { get; set; } = string.Empty;

    public decimal 판매수수료율 { get; set; }

    public decimal 보관료일단가 { get; set; }

    public bool 통관필요여부 { get; set; }

    public DateTime? 계약시작일 { get; set; }

    public DateTime? 계약종료일 { get; set; }

    public string 계약메모 { get; set; } = string.Empty;

    public bool 마켓판매가능여부 => 입고계약유형코드.CanSellToMarket(계약유형);

    public static 입고계약스냅샷 Default(string? counterpartyName = null)
        => new()
        {
            계약유형 = 입고계약유형코드.보관대행,
            계약상대방명 = counterpartyName?.Trim() ?? string.Empty
        };

    public 입고계약스냅샷 Normalize()
    {
        계약번호 = 계약번호.Trim();
        계약유형 = 입고계약유형코드.Normalize(계약유형);
        계약상대방명 = 계약상대방명.Trim();
        정산방식 = 정산방식.Trim();
        계약메모 = 계약메모.Trim();
        통관필요여부 = 통관필요여부 || 입고계약유형코드.RequiresCustoms(계약유형);
        판매수수료율 = Math.Clamp(판매수수료율, 0m, 100m);
        보관료일단가 = Math.Max(0m, 보관료일단가);
        return this;
    }
}
