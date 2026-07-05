using System.Text;

namespace ShipperApp.Services.Commerce.Naver;

public sealed class BCryptNaverCommerceSignatureGenerator : INaverCommerceSignatureGenerator
{
    public string Generate(string clientId, string clientSecret, long timestamp)
    {
        var password = $"{clientId}_{timestamp}";
        var hashed = BCrypt.Net.BCrypt.HashPassword(password, clientSecret);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(hashed));
    }
}
