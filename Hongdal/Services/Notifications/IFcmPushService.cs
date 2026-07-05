namespace 홍달.Services.Notifications
{
    public interface IFcmPushService
    {
        Task<bool> SendToTokenAsync(
            string token,
            string title,
            string body,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default);
    }
}
