namespace Hongdal.FoodApi.Application.DeliveryTickets;

public sealed record AddressRegionKey(
    string Region1,
    string Region2,
    string Region3)
{
    public static readonly AddressRegionKey Empty = new(string.Empty, string.Empty, string.Empty);

    public string Region2Key => string.IsNullOrWhiteSpace(Region1) || string.IsNullOrWhiteSpace(Region2)
        ? string.Empty
        : $"{Region1}|{Region2}";

    public string Region3Key => string.IsNullOrWhiteSpace(Region2Key) || string.IsNullOrWhiteSpace(Region3)
        ? string.Empty
        : $"{Region2Key}|{Region3}";

    public static AddressRegionKey FromAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return Empty;
        }

        var parts = address
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(3)
            .ToArray();

        return new AddressRegionKey(
            parts.ElementAtOrDefault(0) ?? string.Empty,
            parts.ElementAtOrDefault(1) ?? string.Empty,
            parts.ElementAtOrDefault(2) ?? string.Empty);
    }

    public static AddressRegionKey FromKakao(string? region1, string? region2, string? region3)
    {
        return new AddressRegionKey(
            region1?.Trim() ?? string.Empty,
            region2?.Trim() ?? string.Empty,
            region3?.Trim() ?? string.Empty);
    }
}
