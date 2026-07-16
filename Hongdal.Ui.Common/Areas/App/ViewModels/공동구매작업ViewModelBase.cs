using CommunityToolkit.Mvvm.ComponentModel;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

/// <summary>
/// 공동구매 하위 ViewModel에 동일한 실행, 오류, 완료 상태를 제공합니다.
/// </summary>
public abstract class 공동구매작업ViewModelBase : ObservableObject
{
    private Api작업상태 _상태;
    private string? _오류메시지;
    private string? _성공메시지;
    private DateTimeOffset? _마지막완료시각;

    public Api작업상태 상태
    {
        get => _상태;
        private set
        {
            if (!SetProperty(ref _상태, value))
            {
                return;
            }

            OnPropertyChanged(nameof(처리중));
            OnPropertyChanged(nameof(성공함));
            OnPropertyChanged(nameof(오류발생));
            OnPropertyChanged(nameof(취소됨));
        }
    }

    public string? 오류메시지
    {
        get => _오류메시지;
        private set => SetProperty(ref _오류메시지, value);
    }

    public string? 성공메시지
    {
        get => _성공메시지;
        private set => SetProperty(ref _성공메시지, value);
    }

    public DateTimeOffset? 마지막완료시각
    {
        get => _마지막완료시각;
        private set => SetProperty(ref _마지막완료시각, value);
    }

    public bool 처리중 => 상태 == Api작업상태.처리중;
    public bool 성공함 => 상태 == Api작업상태.성공;
    public bool 오류발생 => 상태 == Api작업상태.실패;
    public bool 취소됨 => 상태 == Api작업상태.취소됨;

    public void 작업상태초기화()
    {
        if (처리중)
        {
            return;
        }

        오류메시지 = null;
        성공메시지 = null;
        마지막완료시각 = null;
        상태 = Api작업상태.대기;
    }

    protected bool 유효성실패(string message)
    {
        오류메시지 = message;
        성공메시지 = null;
        마지막완료시각 = DateTimeOffset.Now;
        상태 = Api작업상태.실패;
        return false;
    }

    protected async Task<bool> 작업실행Async(
        Func<CancellationToken, Task> action,
        string successMessage,
        CancellationToken cancellationToken = default,
        Func<Exception, string>? errorMessageFactory = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (처리중)
        {
            return false;
        }

        오류메시지 = null;
        성공메시지 = null;
        마지막완료시각 = null;
        상태 = Api작업상태.처리중;

        try
        {
            await action(cancellationToken);
            성공메시지 = successMessage;
            상태 = Api작업상태.성공;
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            상태 = Api작업상태.취소됨;
            throw;
        }
        catch (Exception ex)
        {
            오류메시지 = errorMessageFactory?.Invoke(ex) ?? ex.Message;
            상태 = Api작업상태.실패;
            return false;
        }
        finally
        {
            마지막완료시각 = DateTimeOffset.Now;
        }
    }
}
