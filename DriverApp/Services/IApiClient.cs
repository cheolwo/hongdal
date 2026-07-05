namespace DriverApp.Services;

public interface IApiClient
{
    Task<TResponse?> GetJsonAsync<TResponse>(string path);
    Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string path, TRequest payload);
    Task PostJsonAsync<TRequest>(string path, TRequest payload);
}
