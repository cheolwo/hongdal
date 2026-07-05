using System.Net;
using System.Text;
using Hongdal.Contracts.Common.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed class HongdalDocumentOutputService : IHongdalDocumentOutputService
{
    public HongdalDocumentOutput CreateWaybill(HongdalWaybillDocumentDraft draft)
    {
        var title = $"운송장 {draft.DocumentNo}";
        var html = BuildWaybillHtml(draft, title);
        var plainText = BuildPlainText(draft);

        return new HongdalDocumentOutput(
            $"hongdal-waybill-{SanitizeFileName(draft.DocumentNo)}.html",
            title,
            "text/html;charset=utf-8",
            html,
            plainText);
    }

    private static string BuildWaybillHtml(HongdalWaybillDocumentDraft draft, string title)
    {
        var receiptText = draft.ReceiptRequired ? "인수증 필요" : "인수증 없음";
        var html = new StringBuilder();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html lang=\"ko\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\">");
        html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        html.AppendLine($"<title>{E(title)}</title>");
        html.AppendLine("<style>");
        html.AppendLine("body{font-family:Arial,'Malgun Gothic',sans-serif;margin:0;color:#111827;background:#fff;}");
        html.AppendLine(".sheet{width:190mm;min-height:267mm;margin:0 auto;padding:14mm;box-sizing:border-box;}");
        html.AppendLine(".head{display:flex;justify-content:space-between;gap:16px;border-bottom:2px solid #111827;padding-bottom:10px;}");
        html.AppendLine(".title{font-size:26px;font-weight:800;}.docno{font-size:13px;color:#4b5563;text-align:right;}");
        html.AppendLine(".grid{display:grid;grid-template-columns:1fr 1fr;gap:12px;margin-top:16px;}");
        html.AppendLine(".box{border:1px solid #d1d5db;border-radius:6px;padding:12px;break-inside:avoid;}");
        html.AppendLine(".label{font-size:12px;color:#6b7280;font-weight:700;margin-bottom:6px;}.value{font-size:16px;font-weight:700;}");
        html.AppendLine(".meta{display:grid;grid-template-columns:repeat(3,1fr);gap:8px;margin-top:12px;}");
        html.AppendLine(".memo{white-space:pre-wrap;line-height:1.5;}.money{font-size:18px;font-weight:800;}");
        html.AppendLine("@media print{body{background:#fff}.sheet{width:auto;min-height:auto;margin:0;padding:0}.no-print{display:none!important}}");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<main class=\"sheet\">");
        html.AppendLine("<section class=\"head\">");
        html.AppendLine("<div><div class=\"title\">홍달 운송장</div><div>운송 접수 및 배차 확인용</div></div>");
        html.AppendLine($"<div class=\"docno\"><strong>{E(draft.DocumentNo)}</strong><br>{draft.CreatedAt:yyyy-MM-dd HH:mm}</div>");
        html.AppendLine("</section>");
        html.AppendLine("<section class=\"grid\">");
        AppendBox(html, "화물", draft.CargoName, $"{draft.TransportMode} · {draft.VehicleCondition}");
        AppendBox(html, "정산", draft.PaymentMethod, $"{receiptText} · 예상비용 {draft.ExpectedCost:N0}원");
        AppendBox(html, "상차", draft.PickupPlace, $"{draft.PickupAddress}{Environment.NewLine}{draft.PickupTime}");
        AppendBox(html, "하차", draft.DropoffPlace, $"{draft.DropoffAddress}{Environment.NewLine}{draft.DropoffTime}");
        html.AppendLine("</section>");
        html.AppendLine("<section class=\"meta\">");
        AppendBox(html, "예상 운임", $"{draft.ExpectedFare:N0}원", string.Empty, "money");
        AppendBox(html, "인수증", receiptText, string.Empty);
        AppendBox(html, "출력일", draft.CreatedAt.ToString("yyyy-MM-dd"), string.Empty);
        html.AppendLine("</section>");
        html.AppendLine("<section class=\"box\" style=\"margin-top:12px\">");
        html.AppendLine("<div class=\"label\">메모</div>");
        html.AppendLine($"<div class=\"memo\">{E(draft.Memo)}</div>");
        html.AppendLine("</section>");
        html.AppendLine("</main>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    private static void AppendBox(StringBuilder html, string label, string value, string subText, string valueClass = "value")
    {
        html.AppendLine("<section class=\"box\">");
        html.AppendLine($"<div class=\"label\">{E(label)}</div>");
        html.AppendLine($"<div class=\"{valueClass}\">{E(value)}</div>");
        if (!string.IsNullOrWhiteSpace(subText))
        {
            html.AppendLine($"<div style=\"margin-top:6px;color:#4b5563;white-space:pre-wrap\">{E(subText)}</div>");
        }

        html.AppendLine("</section>");
    }

    private static string BuildPlainText(HongdalWaybillDocumentDraft draft)
    {
        return string.Join(Environment.NewLine, new[]
        {
            $"운송장: {draft.DocumentNo}",
            $"화물: {draft.CargoName}",
            $"상차: {draft.PickupPlace} / {draft.PickupAddress} / {draft.PickupTime}",
            $"하차: {draft.DropoffPlace} / {draft.DropoffAddress} / {draft.DropoffTime}",
            $"정산: {draft.PaymentMethod} / {(draft.ReceiptRequired ? "인수증 필요" : "인수증 없음")}",
            $"예상 운임: {draft.ExpectedFare:N0}원",
            $"메모: {draft.Memo}"
        });
    }

    private static string E(string value) => WebUtility.HtmlEncode(value);

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "document" : safe;
    }
}

public static class HongdalDocumentOutputServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalDocumentOutputServices(this IServiceCollection services)
    {
        services.AddSingleton<IHongdalDocumentOutputService, HongdalDocumentOutputService>();
        return services;
    }
}
