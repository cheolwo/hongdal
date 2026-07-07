namespace 홍달.Services.Options;

public sealed class PublicDataOptions
{
    public const string SectionName = "PublicData";

    public string ServiceKey { get; set; } = string.Empty;

    public string DataGoKrServiceKey { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 20;

    public RoadAddressOptions RoadAddress { get; set; } = new();

    public ApartmentComplexOptions ApartmentComplex { get; set; } = new();

    public ApartmentManagementFeeOptions ApartmentManagementFee { get; set; } = new();
}

public sealed class RoadAddressOptions
{
    public string ConfirmKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://business.juso.go.kr";

    public string SearchPath { get; set; } = "/addrlink/addrLinkApi.do";
}

public sealed class ApartmentComplexOptions
{
    public string ServiceKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://apis.data.go.kr";

    public string ListPath { get; set; } = "/1613000/AptListService3/getLegaldongAptList";

    public string BasicInfoPath { get; set; } = "/1613000/AptBasisInfoServiceV4/getAphusBassInfo";
}

public sealed class ApartmentManagementFeeOptions
{
    public string ServiceKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://apis.data.go.kr";

    public string PublicManagementFeePath { get; set; } = "/1613000/AptPublicManageCostService/getHsmpPublicManageCostInfo";

    public string IndividualUsageFeePath { get; set; } = "/1613000/AptIndvdlzManageCostService/getHsmpIndvdlzManageCostInfo";

    public string LongTermRepairReservePath { get; set; } = "/1613000/AptLongTermRepairReserveService/getHsmpLongTermRepairReserveInfo";
}
