namespace ShipperApp.Services.Commerce.Naver;

public interface INaverCommerceSignatureGenerator
{
    string Generate(string clientId, string clientSecret, long timestamp);
}
