namespace ShipperApp.Models.BackOffice;

public sealed class 공개화물요약응답
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화물종류 { get; set; } = string.Empty;
    public string 운송방식 { get; set; } = string.Empty;
    public string 차량종류 { get; set; } = string.Empty;
    public int? 화물수량 { get; set; }
    public decimal? 화물중량Kg { get; set; }
    public string 의뢰상태 { get; set; } = string.Empty;
    public string 배차상태 { get; set; } = string.Empty;
    public DateTime 생성일시 { get; set; }
}
