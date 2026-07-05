namespace 홍달.Services.Options
{
    public sealed class 수입식품제품조회Options
    {
        public const string SectionName = "수입식품제품조회";

        public string BaseUrl { get; set; } = "http://apis.data.go.kr/1471000/IprtFoodPrdtDBService02";
        public string Path { get; set; } = "/getIprtFoodPrdtDBInfo2";
        public string ServiceKey { get; set; } = string.Empty;
        public string DefaultType { get; set; } = "xml";
    }
}
