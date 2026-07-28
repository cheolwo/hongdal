namespace DriverApp.Services;

public sealed class ApiClient : IApiClient
{
    private readonly IDriverApiClient _driverApiClient;

    public ApiClient(IDriverApiClient driverApiClient)
    {
        _driverApiClient = driverApiClient;
    }

    public Task<TResponse?> GetJsonAsync<TResponse>(string path)
        => _driverApiClient.GetAsync<TResponse>(path, "공통 조회");

    public Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string path, TRequest payload)
        => _driverApiClient.PostAsync<TRequest, TResponse>(path, payload, "공통 등록");

    public Task PostJsonAsync<TRequest>(string path, TRequest payload)
        => _driverApiClient.PostAsync(path, payload, "공통 등록");
}
