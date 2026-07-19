using Ssalddel.Contracts.Common.Documents;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface ISsalddelDocumentOutputService
{
    SsalddelDocumentOutput CreateWaybill(SsalddelWaybillDocumentDraft draft);

    SsalddelDocumentOutput CreateInboundExpectedItems(SsalddelExpectedItemDocumentDraft draft);

    SsalddelDocumentOutput CreateOutboundExpectedItems(SsalddelExpectedItemDocumentDraft draft);
}
