using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Services;

namespace Ssalddel.WebApp.ViewModels;

/// <summary>
/// Web 상세 route의 인증, feature flag와 서버 원장 재조회를 담당합니다.
/// 표현과 책임별 화면은 Ui.Common Screen이 맡습니다.
/// </summary>
public sealed class ShipperRequestDetailPageViewModel
{
    private const string DomesticTransportFeatureKey = "DomesticTransportWorkflow";

    private readonly 화주결제정산Service _settlementService;
    private readonly WebAuthSessionService _authSession;
    private readonly ICommunityProcurementClient _workflowMetadata;

    public ShipperRequestDetailPageViewModel(
        화주결제정산Service settlementService,
        WebAuthSessionService authSession,
        ICommunityProcurementClient workflowMetadata)
    {
        _settlementService = settlementService;
        _authSession = authSession;
        _workflowMetadata = workflowMetadata;
    }

    public ShipperRequestDetailPageState State { get; } = new()
    {
        SourceBoundaryMessage = "Web과 모바일이 같은 서버 운송 의뢰 endpoint와 ID를 다시 조회합니다."
    };

    public async Task LoadAsync(
        string? requestId,
        bool created = false,
        bool showMessage = true,
        CancellationToken cancellationToken = default)
    {
        if (State.IsBusy)
        {
            return;
        }

        var normalized = NormalizeRequestId(requestId);
        State.LookupRequestId = normalized ?? string.Empty;
        State.Created = created;
        State.Request = null;
        State.RequiresLogin = false;

        if (normalized is null)
        {
            State.IsWorkflowEnabled = null;
            SetStatus("등록 결과에 표시된 운송 의뢰 ID를 입력해 주세요.", ShipperRequestDetailMessageTone.Info);
            return;
        }

        State.IsBusy = true;
        if (showMessage)
        {
            SetStatus("서버에서 운송 의뢰 원장을 조회하는 중입니다.", ShipperRequestDetailMessageTone.Info);
        }

        try
        {
            if (!await EnsureWorkflowEnabledAsync(cancellationToken))
            {
                return;
            }

            await _authSession.RestoreAsync();
            if (!_authSession.IsLoggedIn)
            {
                State.RequiresLogin = true;
                SetStatus("운송 의뢰 상세는 로그인 후 조회할 수 있습니다.", ShipperRequestDetailMessageTone.Warning);
                return;
            }

            var source = await _settlementService.의뢰조회Async(normalized, cancellationToken);
            State.Request = ShipperRequestDetailSnapshot.FromContract(source);
            State.LookupRequestId = source.의뢰Id;
            if (showMessage)
            {
                SetStatus($"{source.의뢰Id} 원장을 서버에서 다시 조회했습니다.", ShipperRequestDetailMessageTone.Success);
            }
        }
        catch (Exception ex)
        {
            State.Request = null;
            State.RequiresLogin = IsAuthenticationFailure(ex);
            if (IsFeatureDisabledFailure(ex))
            {
                State.IsWorkflowEnabled = false;
            }

            SetStatus(ResolveFailureMessage(ex), ResolveFailureTone(ex));
        }
        finally
        {
            State.IsBusy = false;
        }
    }

    private async Task<bool> EnsureWorkflowEnabledAsync(CancellationToken cancellationToken)
    {
        try
        {
            var metadata = await _workflowMetadata.GetVersionWorkflowMetadataAsync(cancellationToken);
            State.IsWorkflowEnabled = metadata.Flags.TryGetValue(DomesticTransportFeatureKey, out var enabled)
                                      && enabled;
            if (State.IsWorkflowEnabled != true)
            {
                SetStatus("현재 배포 환경에서는 국내 운송 의뢰 조회가 비활성화되어 있습니다.", ShipperRequestDetailMessageTone.Warning);
                return false;
            }

            return true;
        }
        catch
        {
            State.IsWorkflowEnabled = null;
            SetStatus("운송 의뢰 조회 가능 여부를 확인하지 못했습니다. 잠시 후 다시 시도해 주세요.", ShipperRequestDetailMessageTone.Error);
            return false;
        }
    }

    private void SetStatus(string message, ShipperRequestDetailMessageTone tone)
    {
        State.StatusMessage = message;
        State.StatusTone = tone;
    }

    private static string? NormalizeRequestId(string? requestId)
        => string.IsNullOrWhiteSpace(requestId) ? null : requestId.Trim();

    private static bool IsAuthenticationFailure(Exception exception)
        => exception is HttpRequestException { StatusCode: System.Net.HttpStatusCode.Unauthorized }
           || exception.Message.Contains("서버 인증이 필요", StringComparison.Ordinal);

    private static bool IsFeatureDisabledFailure(Exception exception)
        => exception.Message.Contains("FeatureDisabled", StringComparison.OrdinalIgnoreCase);

    private static ShipperRequestDetailMessageTone ResolveFailureTone(Exception exception)
        => exception is HttpRequestException
           {
               StatusCode: System.Net.HttpStatusCode.NotFound
                   or System.Net.HttpStatusCode.Unauthorized
                   or System.Net.HttpStatusCode.Forbidden
           }
            || IsAuthenticationFailure(exception)
                ? ShipperRequestDetailMessageTone.Warning
                : ShipperRequestDetailMessageTone.Error;

    private static string ResolveFailureMessage(Exception exception)
    {
        if (IsFeatureDisabledFailure(exception))
        {
            return "현재 배포 환경에서는 국내 운송 의뢰 조회가 비활성화되어 있습니다.";
        }

        if (IsAuthenticationFailure(exception))
        {
            return "로그인이 만료되었거나 필요합니다. 다시 로그인한 뒤 조회해 주세요.";
        }

        if (exception is HttpRequestException { StatusCode: System.Net.HttpStatusCode.Forbidden })
        {
            return "이 운송 의뢰를 조회할 권한이 없습니다.";
        }

        if (exception is HttpRequestException { StatusCode: System.Net.HttpStatusCode.NotFound })
        {
            return "운송 의뢰를 찾을 수 없거나 현재 계정의 조회 대상이 아닙니다.";
        }

        return "운송 의뢰를 불러오지 못했습니다. 서버 연결 상태를 확인한 뒤 다시 시도해 주세요.";
    }
}
