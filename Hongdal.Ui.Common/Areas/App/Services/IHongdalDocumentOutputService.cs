using Hongdal.Contracts.Common.Documents;

namespace Hongdal.Ui.Common.Areas.App.Services;

public interface IHongdalDocumentOutputService
{
    HongdalDocumentOutput CreateWaybill(HongdalWaybillDocumentDraft draft);

    HongdalDocumentOutput CreateInboundExpectedItems(HongdalExpectedItemDocumentDraft draft);

    HongdalDocumentOutput CreateOutboundExpectedItems(HongdalExpectedItemDocumentDraft draft);
}
