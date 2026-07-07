namespace DriverApp.Services;

public static class ApiEnvironment
{
    public static Uri CreateBaseAddress()
    {
        return new Uri("https://localhost:7117/");
    }
}
