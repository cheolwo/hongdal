namespace DriverApp.Services;

public static class ApiEnvironment
{
    public static Uri CreateBaseAddress()
    {
#if ANDROID && DEBUG
        return new Uri("http://10.0.2.2:5104/");
#else
        return new Uri("https://localhost:7117/");
#endif
    }
}
