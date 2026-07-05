using Hongdal.Ui.Common.Areas.App.Models;

namespace WarehouseManagerApp.Services;

public interface IWarehouseWorkEntryGateService
{
    Task<WarehouseWorkOperatorVerificationResult> VerifyAsync(string processCode, string phoneLastEightDigits, CancellationToken cancellationToken = default);
}
