namespace Ssalddel.Services.LogisticsProcessing.Warehouse;

public interface IWarehouseServiceAreaPolicy
{
    bool IsInServiceArea(string warehouseAddress, string destinationAddress);
}

public sealed class WarehouseServiceAreaPolicy : IWarehouseServiceAreaPolicy
{
    public bool IsInServiceArea(string warehouseAddress, string destinationAddress)
    {
        var warehouseTokens = NormalizeAddressTokens(warehouseAddress);
        var destinationTokens = NormalizeAddressTokens(destinationAddress);
        if (warehouseTokens.Count == 0 || destinationTokens.Count == 0)
        {
            return false;
        }

        if (string.Equals(warehouseTokens[0], destinationTokens[0], StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return warehouseTokens.Take(2).Any(token => destinationTokens.Take(2).Contains(token, StringComparer.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> NormalizeAddressTokens(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return [];
        }

        return address
            .Replace(",", " ", StringComparison.Ordinal)
            .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.Length >= 2)
            .Take(4)
            .ToArray();
    }
}
