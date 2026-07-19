namespace Ssalddel.Ui.Common.Areas.App.Models;

public sealed class 주소검색결과Dto
{
    public string Zonecode { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string RoadAddress { get; set; } = string.Empty;
    public string JibunAddress { get; set; } = string.Empty;

    public string UserSelectedType { get; set; } = string.Empty;
    public string AddressType { get; set; } = string.Empty;

    public string Sido { get; set; } = string.Empty;
    public string Sigungu { get; set; } = string.Empty;
    public string SigunguCode { get; set; } = string.Empty;

    public string Bcode { get; set; } = string.Empty;
    public string Bname { get; set; } = string.Empty;
    public string Hname { get; set; } = string.Empty;

    public string Roadname { get; set; } = string.Empty;
    public string RoadnameCode { get; set; } = string.Empty;
    public string BuildingCode { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public string Apartment { get; set; } = string.Empty;

    public string Query { get; set; } = string.Empty;
}
