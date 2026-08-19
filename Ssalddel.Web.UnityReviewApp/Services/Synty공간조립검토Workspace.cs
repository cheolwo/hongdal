using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Web.UnityReviewApp.Services;

public sealed class Synty공간조립검토Workspace(
    ISynty공간조립모바일검토Client reviewClient)
{
    private readonly HashSet<string> _offlineQueuedItems = new(StringComparer.Ordinal);

    public Synty공간조립검토함Response? Inbox { get; private set; }
    public HashSet<string> SelectedIssues { get; } = new(StringComparer.Ordinal);
    public int CurrentIndex { get; private set; }
    public int CaptureIndex { get; private set; }
    public int OfflinePendingCount { get; private set; }
    public string Note { get; set; } = string.Empty;
    public string? Message { get; private set; }
    public bool MessageIsError { get; private set; }
    public bool Loading { get; private set; }
    public bool Busy { get; private set; }
    public bool Loaded { get; private set; }
    public bool ParkingConfirmed { get; private set; }
    public bool ImageExpanded { get; private set; }
    public bool ImageLoadFailed { get; private set; }
    public int ImageRetryRevision { get; private set; }

    public Synty공간조립검토항목Dto? CurrentItem
        => Inbox?.Items.Count > CurrentIndex && CurrentIndex >= 0
            ? Inbox.Items[CurrentIndex]
            : null;

    public Synty공간조립검토촬영Dto? CurrentCapture
        => CurrentItem?.Composition.Captures.Count > CaptureIndex && CaptureIndex >= 0
            ? CurrentItem.Composition.Captures[CaptureIndex]
            : null;

    public bool DecisionDisabled
        => !ParkingConfirmed
           || Busy
           || CurrentItem is null
           || CurrentItem.Composition.Captures.Count == 0
           || _offlineQueuedItems.Contains(CurrentItem.ReviewItemStableId);

    public bool CanMoveNext => Inbox is not null && CurrentIndex < Inbox.Items.Count - 1;

    public string ImageRenderKey
        => $"{CurrentCapture?.CaptureUploadId}:{CurrentCapture?.ImageSha256}:{ImageRetryRevision}";

    public async Task InitializeAsync(bool mayLoadInbox)
    {
        OfflinePendingCount = await reviewClient.오프라인대기수조회Async();
        if (mayLoadInbox && !Loaded)
        {
            await LoadInboxAsync();
        }
    }

    public void ConfirmParking()
    {
        ParkingConfirmed = true;
        SetMessage("주차 검토 모드를 열었습니다. 출발 전에는 화면을 닫아 주세요.");
    }

    public async Task LoadInboxAsync()
    {
        if (Busy)
        {
            return;
        }

        Loading = true;
        Busy = true;
        Message = null;
        try
        {
            Inbox = await reviewClient.검토함조회Async(take: 100);
            CurrentIndex = FindFirstReviewIndex(Inbox.Items);
            CaptureIndex = 0;
            ResetDraft();
            Loaded = true;
        }
        catch (Exception exception)
        {
            SetMessage($"검토함을 불러오지 못했습니다. {exception.Message}", true);
        }
        finally
        {
            Loading = false;
            Busy = false;
        }
    }

    public async Task SubmitDecisionAsync(string decisionCode)
    {
        var item = CurrentItem;
        if (DecisionDisabled || item is null)
        {
            return;
        }

        if (decisionCode == Synty공간조립검토결정Codes.NeedsRevision
            && SelectedIssues.Count == 0
            && string.IsNullOrWhiteSpace(Note))
        {
            SetMessage("수정 필요를 선택하려면 문제 꼬리표나 짧은 메모를 하나 남겨 주세요.", true);
            return;
        }

        Busy = true;
        Message = null;
        try
        {
            var request = new Synty공간조립검토결정Request
            {
                ExpectedRevision = item.Revision,
                IdempotencyKey = $"mobile:{item.ReviewItemStableId}:{item.Revision}:{Guid.NewGuid():N}",
                DecisionCode = decisionCode,
                IssueCodes = SelectedIssues.OrderBy(code => code, StringComparer.Ordinal).ToList(),
                Note = Note.Trim()
            };
            var result = await reviewClient.결정전송Async(item.ReviewItemStableId, request);
            if (result.Item is not null && Inbox is not null)
            {
                Inbox.Items[CurrentIndex] = result.Item;
                Inbox.ReadyCount = Math.Max(0, Inbox.ReadyCount - 1);
                Inbox.ReviewedCount++;
            }
            if (result.QueuedOffline)
            {
                _offlineQueuedItems.Add(item.ReviewItemStableId);
                OfflinePendingCount = await reviewClient.오프라인대기수조회Async();
            }
            SetMessage(result.Message);
            MoveNextAfterDecision();
        }
        catch (Synty공간조립검토HttpException exception)
        {
            SetMessage(
                exception.StatusCode == System.Net.HttpStatusCode.Conflict
                    ? "다른 검토 또는 새 촬영 묶음이 먼저 저장되었습니다. 새로고침 후 다시 확인해 주세요."
                    : $"검토 결과를 저장하지 못했습니다. {exception.Message}",
                true);
        }
        catch (Exception exception)
        {
            SetMessage($"검토 결과를 저장하지 못했습니다. {exception.Message}", true);
        }
        finally
        {
            Busy = false;
        }
    }

    public async Task SynchronizeOfflineAsync()
    {
        if (Busy)
        {
            return;
        }

        Busy = true;
        try
        {
            var result = await reviewClient.오프라인대기열동기화Async();
            OfflinePendingCount = result.PendingCount;
            SetMessage(
                result.ErrorMessage
                ?? $"오프라인 검토 {result.SynchronizedCount}건을 서버에 동기화했습니다.",
                result.ErrorMessage is not null);
            if (result.SynchronizedCount > 0)
            {
                _offlineQueuedItems.Clear();
                Busy = false;
                await LoadInboxAsync();
            }
        }
        finally
        {
            Busy = false;
        }
    }

    public void SelectCapture(int index)
    {
        if (CurrentItem is not null
            && index >= 0
            && index < CurrentItem.Composition.Captures.Count)
        {
            CaptureIndex = index;
            ResetImageState();
        }
    }

    public void OpenImage()
        => ImageExpanded = CurrentCapture is not null && !ImageLoadFailed;

    public void CloseImage() => ImageExpanded = false;

    public void HandleImageLoadError()
    {
        ImageLoadFailed = true;
        ImageExpanded = false;
    }

    public void RetryImage()
    {
        ImageLoadFailed = false;
        ImageRetryRevision++;
    }

    public void ToggleIssue(string issueCode)
    {
        if (!SelectedIssues.Add(issueCode))
        {
            SelectedIssues.Remove(issueCode);
        }
    }

    public void PreviousItem()
    {
        if (CurrentIndex <= 0)
        {
            return;
        }
        CurrentIndex--;
        CaptureIndex = 0;
        ResetImageState();
        ResetDraft();
    }

    public void NextItem()
    {
        if (!CanMoveNext)
        {
            return;
        }
        CurrentIndex++;
        CaptureIndex = 0;
        ResetImageState();
        ResetDraft();
    }

    private void MoveNextAfterDecision()
    {
        if (CanMoveNext)
        {
            CurrentIndex++;
            CaptureIndex = 0;
        }
        ResetImageState();
        ResetDraft();
    }

    private void ResetImageState()
    {
        ImageExpanded = false;
        ImageLoadFailed = false;
    }

    private void ResetDraft()
    {
        SelectedIssues.Clear();
        Note = string.Empty;
    }

    private void SetMessage(string message, bool isError = false)
    {
        Message = message;
        MessageIsError = isError;
    }

    private static int FindFirstReviewIndex(IReadOnlyList<Synty공간조립검토항목Dto> items)
    {
        for (var index = 0; index < items.Count; index++)
        {
            if (items[index].ReviewStateCode is Synty공간조립검토상태Codes.ReadyForReview
                or Synty공간조립검토상태Codes.Stale)
            {
                return index;
            }
        }
        return 0;
    }
}
