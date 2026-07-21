using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components.Forms;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class CommunityLedgerNodeActionViewModel : ObservableObject
{
    private readonly ICommunityLedgerNodeActionService _actionService;
    private PlatformCommunityLedgerNodeActionResponse? _pendingAction;
    private IBrowserFile? _evidenceFile;
    private string? _actionStatusMessage;
    private bool _evidenceConfirmed;
    private bool _isExecuting;
    private bool _actionSucceeded;

    public CommunityLedgerNodeActionViewModel(ICommunityLedgerNodeActionService actionService)
    {
        _actionService = actionService;
    }

    public PlatformCommunityLedgerNodeActionResponse? PendingAction
    {
        get => _pendingAction;
        private set
        {
            if (SetProperty(ref _pendingAction, value))
            {
                OnPropertyChanged(nameof(CanExecutePendingAction));
            }
        }
    }

    public IBrowserFile? EvidenceFile
    {
        get => _evidenceFile;
        private set
        {
            if (SetProperty(ref _evidenceFile, value))
            {
                OnPropertyChanged(nameof(CanExecutePendingAction));
            }
        }
    }

    public bool EvidenceConfirmed
    {
        get => _evidenceConfirmed;
        set
        {
            if (SetProperty(ref _evidenceConfirmed, value))
            {
                OnPropertyChanged(nameof(CanExecutePendingAction));
            }
        }
    }

    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            if (SetProperty(ref _isExecuting, value))
            {
                OnPropertyChanged(nameof(CanExecutePendingAction));
            }
        }
    }

    public string? ActionStatusMessage
    {
        get => _actionStatusMessage;
        private set => SetProperty(ref _actionStatusMessage, value);
    }

    public bool ActionSucceeded
    {
        get => _actionSucceeded;
        private set => SetProperty(ref _actionSucceeded, value);
    }

    public bool CanExecutePendingAction
        => PendingAction is not null
           && !IsExecuting
           && (!PendingAction.사진필수여부 || EvidenceFile is not null && EvidenceConfirmed);

    public void Begin(PlatformCommunityLedgerNodeActionResponse action)
    {
        ArgumentNullException.ThrowIfNull(action);
        PendingAction = action;
        EvidenceFile = null;
        EvidenceConfirmed = false;
        ActionSucceeded = false;
        ActionStatusMessage = null;
    }

    public void Cancel()
    {
        PendingAction = null;
        EvidenceFile = null;
        EvidenceConfirmed = false;
    }

    public void Reset()
    {
        Cancel();
        ActionSucceeded = false;
        ActionStatusMessage = null;
    }

    public void SelectEvidence(IBrowserFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Size > CommunityLedgerEvidencePolicy.MaxFileBytes)
        {
            RejectEvidence("증빙 사진은 8MB 이하만 선택할 수 있습니다.");
            return;
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            RejectEvidence("이미지 형식의 증빙 파일을 선택해 주세요.");
            return;
        }

        EvidenceFile = file;
        ActionStatusMessage = null;
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var action = PendingAction;
        if (action is null || !CanExecutePendingAction)
        {
            return false;
        }

        IsExecuting = true;
        ActionStatusMessage = null;
        try
        {
            CommunityLedgerEvidenceUploadResult? evidence = null;
            if (action.사진필수여부 && EvidenceFile is not null)
            {
                evidence = await _actionService.상차증빙업로드Async(action, EvidenceFile, cancellationToken);
            }

            var result = await _actionService.실행Async(action, evidence, cancellationToken);
            ActionSucceeded = true;
            ActionStatusMessage = $"{action.표시명} 처리가 완료되었습니다. 현재 상태는 {result.상태}입니다.";
            PendingAction = null;
            EvidenceFile = null;
            EvidenceConfirmed = false;
            return true;
        }
        catch (Exception ex)
        {
            ActionSucceeded = false;
            ActionStatusMessage = ex.Message;
            return false;
        }
        finally
        {
            IsExecuting = false;
        }
    }

    private void RejectEvidence(string message)
    {
        EvidenceFile = null;
        ActionSucceeded = false;
        ActionStatusMessage = message;
    }
}
