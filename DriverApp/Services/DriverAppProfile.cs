namespace DriverApp.Services;

public sealed class DriverAppProfile
{
    public string AppKey { get; } = Hongdal.Contracts.Common.Drivers.기사앱식별자.CargoYongdalDriverApp;
    public string DisplayName { get; } = "홍달 화물/용달기사";
    public string DriverRole { get; } = "화물/용달기사";
    public string DriverDomain { get; } = Hongdal.Contracts.Common.Drivers.기사도메인구분.화물용달;
    public string PrimaryWorkType { get; } = Hongdal.Contracts.Common.Drivers.기사업무유형코드.용달운송;

    public IReadOnlyList<DriverWorkProfile> WorkProfiles { get; } =
    [
        new(
            Hongdal.Contracts.Common.Drivers.기사앱식별자.CargoYongdalDriverApp,
            Hongdal.Contracts.Common.Drivers.기사도메인구분.화물용달,
            Hongdal.Contracts.Common.Drivers.기사업무유형코드.화물운송,
            "화물 기사",
            "팔레트, 박스, 수입/통관 물류처럼 화주 운송 중심의 중량 화물을 처리합니다.",
            "차량 제원, FCL/LCL, 상하차 조건, 운임 정산을 우선 확인합니다."),
        new(
            Hongdal.Contracts.Common.Drivers.기사앱식별자.CargoYongdalDriverApp,
            Hongdal.Contracts.Common.Drivers.기사도메인구분.화물용달,
            Hongdal.Contracts.Common.Drivers.기사업무유형코드.용달운송,
            "용달 기사",
            "당일/근거리 생활 화물, 소형 이사, 일적/입차 운송을 처리합니다.",
            "현재 위치, 픽업 거리, 복귀 동선, 현장 결제 조건을 우선 확인합니다."),
        new(
            Hongdal.Contracts.Common.Drivers.기사앱식별자.FoodDeliveryDriverApp,
            Hongdal.Contracts.Common.Drivers.기사도메인구분.음식배달,
            Hongdal.Contracts.Common.Drivers.기사업무유형코드.음식배달,
            "음식 배달 기사",
            "음식점과 홍달마트 픽업처럼 짧은 시간창의 즉시 배달을 처리합니다.",
            "조리/픽업 시간, 고객 도착 시간, 묶음 배달 가능 여부를 우선 확인합니다.")
    ];
}

public sealed record DriverWorkProfile(
    string AppKey,
    string DriverDomain,
    string WorkType,
    string DisplayName,
    string Description,
    string Focus);
