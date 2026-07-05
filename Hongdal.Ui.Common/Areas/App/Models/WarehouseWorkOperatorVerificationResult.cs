namespace Hongdal.Ui.Common.Areas.App.Models;

public sealed record WarehouseWorkOperatorVerificationResult(
    bool IsAllowed,
    string OperatorName,
    string RoleName,
    string Message);
