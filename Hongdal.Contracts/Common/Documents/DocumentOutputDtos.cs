namespace Hongdal.Contracts.Common.Documents;

public sealed record HongdalWaybillDocumentDraft(
    string DocumentNo,
    string CargoName,
    string TransportMode,
    string VehicleCondition,
    string PickupPlace,
    string PickupAddress,
    string PickupTime,
    string DropoffPlace,
    string DropoffAddress,
    string DropoffTime,
    string PaymentMethod,
    bool ReceiptRequired,
    decimal ExpectedFare,
    decimal ExpectedCost,
    string Memo,
    DateTimeOffset CreatedAt);

public sealed record HongdalDocumentOutput(
    string FileName,
    string Title,
    string ContentType,
    string Html,
    string PlainText);
