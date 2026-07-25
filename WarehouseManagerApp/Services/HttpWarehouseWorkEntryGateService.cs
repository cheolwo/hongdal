using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace WarehouseManagerApp.Services;

public sealed class HttpWarehouseWorkEntryGateService(
    ISsalddelJsonApiClient client) : IWarehouseWorkEntryGateService
{
    public async Task<WarehouseWorkOperatorVerificationResult> VerifyAsync(
        string processCode,
        string phoneLastEightDigits,
        CancellationToken cancellationToken = default)
    {
        var suffix = new string((phoneLastEightDigits ?? string.Empty).Where(char.IsDigit).ToArray());
        if (suffix.Length != 8)
        {
            return new WarehouseWorkOperatorVerificationResult(
                false,
                string.Empty,
                string.Empty,
                "확인용 휴대폰 번호 뒤 8자리를 입력해 주세요.");
        }

        var response = await client.SendAsync<창고작업진입확인요청, 창고작업진입확인응답>(
            HttpMethod.Post,
            "api/v1/warehouse-operations/work-entry/verify",
            new 창고작업진입확인요청 { ProcessCode = processCode },
            "창고 작업자 HR 역할 확인",
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("창고 작업자 확인 응답이 비어 있습니다.");

        return new WarehouseWorkOperatorVerificationResult(
            response.IsAllowed,
            response.OperatorName,
            response.RoleName,
            response.Message);
    }
}
