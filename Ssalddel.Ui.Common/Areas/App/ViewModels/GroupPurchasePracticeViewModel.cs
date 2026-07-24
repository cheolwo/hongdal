using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class GroupPurchasePracticeViewModel(I공동구매체험Client client)
{
    public IReadOnlyList<공동구매체험시나리오응답> Scenarios { get; private set; } = [];
    public 공동구매체험시나리오응답? SelectedScenario { get; private set; }
    public 공동구매체험응답? Result { get; private set; }
    public decimal DesiredQuantity { get; set; }
    public string TopicCode { get; private set; } = 공동구매체험대화주제코드.처음참여;
    public bool IsBusy { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;

    public bool HasStarted => Result is not null;
    public bool CanAdvance => Result is { 완료여부: false } && !IsBusy;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (Scenarios.Count > 0 || IsBusy)
        {
            return;
        }

        await ExecuteAsync(async token =>
        {
            Scenarios = await client.시나리오목록Async(token);
            SelectedScenario = Scenarios.FirstOrDefault();
            DesiredQuantity = SelectedScenario?.기본희망수량 ?? 1;
        }, cancellationToken);
    }

    public void SelectScenario(string scenarioId)
    {
        var selected = Scenarios.FirstOrDefault(item =>
            string.Equals(item.시나리오Id, scenarioId, StringComparison.Ordinal));
        if (selected is null)
        {
            return;
        }

        SelectedScenario = selected;
        DesiredQuantity = selected.기본희망수량;
        Result = null;
        ErrorMessage = string.Empty;
    }

    public void SelectTopic(string topicCode)
    {
        TopicCode = 공동구매체험대화주제코드.정규화(topicCode);
        Result = null;
        ErrorMessage = string.Empty;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
        => SimulateAsync(0, string.Empty, cancellationToken);

    public Task AdvanceAsync(CancellationToken cancellationToken = default)
        => Result is null
            ? StartAsync(cancellationToken)
            : SimulateAsync(Result.현재라운드 + 1, Result.세션Id, cancellationToken);

    public Task RestartAsync(CancellationToken cancellationToken = default)
        => SimulateAsync(0, string.Empty, cancellationToken);

    private async Task SimulateAsync(
        int round,
        string sessionId,
        CancellationToken cancellationToken)
    {
        if (SelectedScenario is null)
        {
            ErrorMessage = "연습할 공동구매 시나리오를 선택해 주세요.";
            return;
        }

        await ExecuteAsync(async token =>
        {
            Result = await client.시뮬레이션Async(new 공동구매체험요청
            {
                세션Id = sessionId,
                시나리오Id = SelectedScenario.시나리오Id,
                내희망수량 = Math.Max(1, DesiredQuantity),
                라운드 = round,
                대화주제코드 = TopicCode
            }, token) ?? throw new InvalidOperationException("공동구매 연습 결과가 비어 있습니다.");
        }, cancellationToken);
    }

    private async Task ExecuteAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await action(cancellationToken);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
