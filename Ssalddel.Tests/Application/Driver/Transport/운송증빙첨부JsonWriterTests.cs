using System.Text.Json.Nodes;
using Ssalddel.Application.Driver.Transport;
using 살뜰.도메인.운송;

namespace Ssalddel.Tests.Application.Driver.Transport;

public class 운송증빙첨부JsonWriterTests
{
    [Fact]
    public void 추가_운송예외_메타데이터를_첨부Json에_보존한다()
    {
        var writer = new 운송증빙첨부JsonWriter();
        var 운송 = new 운송원장 { 첨부_json = "[]" };

        writer.추가(
            운송,
            new 운송증빙첨부(
                "transport-field-exception",
                "proof/pickup-missing.jpg",
                "https://example.test/proof/pickup-missing.jpg",
                "driver-1",
                new DateTime(2026, 7, 9, 1, 2, 3, DateTimeKind.Utc),
                new Dictionary<string, object?>
                {
                    ["stage"] = "상차",
                    ["exceptionCode"] = "상차물건없음",
                    ["adminReviewRequired"] = true,
                    ["nextAction"] = "관리자 확인을 기다려 주세요."
                }));

        var attachments = JsonNode.Parse(운송.첨부_json)!.AsArray();
        var item = attachments[0]!.AsObject();

        Assert.Equal("transport-field-exception", item["kind"]!.GetValue<string>());
        Assert.Equal("상차", item["stage"]!.GetValue<string>());
        Assert.Equal("상차물건없음", item["exceptionCode"]!.GetValue<string>());
        Assert.True(item["adminReviewRequired"]!.GetValue<bool>());
    }
}
