using Ssalddel.Ui.Common.Areas.App.Models;

namespace SsalddelApp.Services;

public interface IWarehouseWorkEntryGateService
{
    Task<WarehouseWorkOperatorVerificationResult> VerifyAsync(string processCode, string phoneLastEightDigits, CancellationToken cancellationToken = default);
}
