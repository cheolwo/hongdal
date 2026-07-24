using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public abstract class 무역확장원장생성ViewModel
{
    public bool 처리중 { get; protected set; }
    public string? 오류 { get; protected set; }
    public 무역확장원장응답? 결과 { get; protected set; }

    protected async Task 실행Async(
        Func<Task<무역확장원장응답?>> action)
    {
        처리중 = true;
        오류 = null;
        try
        {
            결과 = await action();
            if (결과 is null)
            {
                오류 = "원장을 생성하지 못했습니다. 원천 원장과 권한을 확인해 주세요.";
            }
        }
        catch (Exception exception)
        {
            오류 = exception.Message;
        }
        finally
        {
            처리중 = false;
        }
    }
}

public sealed class 개별수입원장생성ViewModel(I무역확장원장Client client)
    : 무역확장원장생성ViewModel
{
    public string 주문원장Id { get; set; } = string.Empty;
    public 개별수입원장생성요청 요청 { get; } = new();

    public Task 생성Async()
    {
        요청.요청멱등키 = 멱등키(요청.요청멱등키, "individual-import");
        return 실행Async(() => client.개별수입생성Async(주문원장Id, 요청));
    }

    private static string 멱등키(string value, string prefix)
        => string.IsNullOrWhiteSpace(value) ? $"{prefix}:{Guid.NewGuid():N}" : value.Trim();
}

public sealed class 개별수출원장생성ViewModel(I무역확장원장Client client)
    : 무역확장원장생성ViewModel
{
    public string 주문원장Id { get; set; } = string.Empty;
    public 개별수출원장생성요청 요청 { get; } = new();

    public Task 생성Async()
    {
        요청.요청멱등키 = string.IsNullOrWhiteSpace(요청.요청멱등키)
            ? $"individual-export:{Guid.NewGuid():N}"
            : 요청.요청멱등키.Trim();
        return 실행Async(() => client.개별수출생성Async(주문원장Id, 요청));
    }
}

public sealed class 공동수출원장생성ViewModel(I무역확장원장Client client)
    : 무역확장원장생성ViewModel
{
    public 공동수출원장생성요청 요청 { get; } = new();
    public string 개별수출원장Ids입력 { get; set; } = string.Empty;

    public Task 생성Async()
    {
        요청.요청멱등키 = string.IsNullOrWhiteSpace(요청.요청멱등키)
            ? $"group-export:{Guid.NewGuid():N}"
            : 요청.요청멱등키.Trim();
        요청.개별수출원장Ids = 개별수출원장Ids입력
            .Split(['\r', '\n', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return 실행Async(() => client.공동수출생성Async(요청));
    }
}
