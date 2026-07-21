using Microsoft.AspNetCore.Components.Forms;
using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Services;

namespace Ssalddel.WebApp.ViewModels;

public sealed record DriverTransportIssueReason(string Code, string Label, string Stage);

public sealed class DriverTransportIssueViewModel : 조립ViewModelBase
{
    public static readonly IReadOnlyList<DriverTransportIssueReason> DefaultReasons =
    [
        new("상차물건없음", "상차지에 물건이 없음", "상차"),
        new("수량불일치", "수량이 다름", "상차"),
        new("상차담당자부재", "상차 담당자 부재", "상차"),
        new("화물훼손", "화물 훼손", "상차"),
        new("하차지부재", "하차지 부재", "하차"),
        new("사진재촬영필요", "사진 재촬영 필요", "증빙"),
        new("증빙업로드실패", "증빙 업로드 실패", "증빙")
    ];

    private readonly Func<long> _transportId;
    private readonly Func<long, 운송증빙단계, string, string, byte[], CancellationToken, Task<기사운송사진업로드결과>> _uploadPhoto;
    private readonly Func<기사운송문제신고요청, CancellationToken, Task> _reportIssue;
    private readonly DriverTransportProofCommandRunner _run;
    private string _issueCode;
    private string? _memo;
    private bool _requireAdminReview = true;
    private string? _previewUrl;
    private string? _fileName;
    private 기사운송사진업로드결과? _upload;

    public DriverTransportIssueViewModel(
        Func<long> transportId,
        Func<long, 운송증빙단계, string, string, byte[], CancellationToken, Task<기사운송사진업로드결과>> uploadPhoto,
        Func<기사운송문제신고요청, CancellationToken, Task> reportIssue,
        DriverTransportProofCommandRunner run,
        IReadOnlyList<DriverTransportIssueReason>? reasons = null)
    {
        _transportId = transportId;
        _uploadPhoto = uploadPhoto;
        _reportIssue = reportIssue;
        _run = run;
        Reasons = reasons is { Count: > 0 } ? reasons : DefaultReasons;
        _issueCode = Reasons[0].Code;
    }

    public IReadOnlyList<DriverTransportIssueReason> Reasons { get; }

    public string IssueCode
    {
        get => _issueCode;
        set => SetProperty(ref _issueCode, value);
    }

    public string? Memo
    {
        get => _memo;
        set => SetProperty(ref _memo, value);
    }

    public bool RequireAdminReview
    {
        get => _requireAdminReview;
        set => SetProperty(ref _requireAdminReview, value);
    }

    public string? PreviewUrl
    {
        get => _previewUrl;
        private set => SetProperty(ref _previewUrl, value);
    }

    public string? FileName
    {
        get => _fileName;
        private set => SetProperty(ref _fileName, value);
    }

    public 기사운송사진업로드결과? Upload
    {
        get => _upload;
        private set => SetProperty(ref _upload, value);
    }

    public Task UploadAsync(IBrowserFile file)
    {
        ClearEvidence();
        return _run("예외 사진을 업로드했습니다.", async cancellationToken =>
        {
            var image = await DriverTransportProofImageReader.ReadAsync(file, cancellationToken);
            await UploadImageCoreAsync(image, cancellationToken);
        });
    }

    public Task UploadImageAsync(DriverTransportProofImage image)
    {
        ClearEvidence();
        return _run(
            "예외 사진을 업로드했습니다.",
            cancellationToken => UploadImageCoreAsync(image, cancellationToken));
    }

    public Task ReportAsync()
    {
        var reason = Reasons.FirstOrDefault(option => option.Code == IssueCode) ?? Reasons[0];
        var request = new 기사운송문제신고요청
        {
            단계 = reason.Stage,
            예외코드 = reason.Code,
            사유 = reason.Label,
            메모 = Memo,
            증빙ObjectName = Upload?.ObjectName,
            증빙Url = Upload?.Url,
            관리자확인요청 = RequireAdminReview
        };
        return _run(
            "예외 사유를 서버에 기록했습니다.",
            cancellationToken => _reportIssue(request, cancellationToken));
    }

    public void Reset()
    {
        IssueCode = Reasons[0].Code;
        Memo = null;
        RequireAdminReview = true;
        ClearEvidence();
    }

    private async Task UploadImageCoreAsync(
        DriverTransportProofImage image,
        CancellationToken cancellationToken)
    {
        var upload = await _uploadPhoto(
            _transportId(),
            운송증빙단계.예외,
            image.FileName,
            image.ContentType,
            image.Bytes,
            cancellationToken);
        PreviewUrl = image.PreviewUrl;
        FileName = image.FileName;
        Upload = upload;
    }

    private void ClearEvidence()
    {
        PreviewUrl = null;
        FileName = null;
        Upload = null;
    }
}
