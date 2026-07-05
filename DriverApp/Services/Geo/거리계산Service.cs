namespace DriverApp.Services.Geo;

public static class 거리계산Service
{
    private const double 지구반지름Km = 6371.0d;

    public static decimal 직선거리Km(decimal 시작위도, decimal 시작경도, decimal 도착위도, decimal 도착경도)
    {
        var 시작위도라디안 = 라디안((double)시작위도);
        var 도착위도라디안 = 라디안((double)도착위도);
        var 위도차 = 라디안((double)(도착위도 - 시작위도));
        var 경도차 = 라디안((double)(도착경도 - 시작경도));

        var a = Math.Sin(위도차 / 2) * Math.Sin(위도차 / 2)
            + Math.Cos(시작위도라디안) * Math.Cos(도착위도라디안)
            * Math.Sin(경도차 / 2) * Math.Sin(경도차 / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return Math.Round((decimal)(지구반지름Km * c), 1, MidpointRounding.AwayFromZero);
    }

    private static double 라디안(double degree)
    {
        return degree * Math.PI / 180.0d;
    }
}
