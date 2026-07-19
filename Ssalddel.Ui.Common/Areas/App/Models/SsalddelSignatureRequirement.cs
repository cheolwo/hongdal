namespace Ssalddel.Ui.Common.Areas.App.Models;

public static class SsalddelSignatureContextCode
{
    public const string WarehouseInboundContract = "warehouse.inbound-contract";
    public const string ShipperTradeForm = "shipper.trade-form";
    public const string DriverPickupHandover = "driver.pickup-handover";
    public const string DriverDropoffHandover = "driver.dropoff-handover";
    public const string HrEmploymentContract = "hr.employment-contract";
    public const string GenericContract = "contract.generic";
}

public sealed record SsalddelSignatureRequirement(
    bool IsRequired,
    string ContextCode,
    string ReferenceId,
    string Title,
    string Description,
    string SignerRole,
    string Reason,
    bool IsContractSignature = true)
{
    public static SsalddelSignatureRequirement None(
        string contextCode,
        string referenceId = "")
        => new(
            false,
            contextCode,
            referenceId,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            false);

    public static SsalddelSignatureRequirement Required(
        string contextCode,
        string referenceId,
        string title,
        string description,
        string signerRole,
        string reason,
        bool isContractSignature = true)
        => new(
            true,
            contextCode,
            referenceId,
            title,
            description,
            signerRole,
            reason,
            isContractSignature);
}

public sealed record SsalddelSignatureGateResult(
    SsalddelSignatureRequirement Requirement,
    SsalddelSignatureCaptureResult Signature);
