namespace 살뜰.Services.Notifications;

public sealed record KakaoAlimTalkMessage(
    string RecipientPhoneNumber,
    string TemplateCode,
    string Title,
    string Body,
    IReadOnlyDictionary<string, string> Variables);

public interface IKakaoAlimTalkService
{
    Task<bool> SendAsync(KakaoAlimTalkMessage message, CancellationToken cancellationToken = default);
}
