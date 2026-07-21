using Microsoft.AspNetCore.Components.Forms;
using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Services;

namespace Ssalddel.WebApp.ViewModels;

public sealed class DriverPickupProofViewModel(
    Func<long> transportId,
    Func<long, 운송증빙단계, string, string, byte[], CancellationToken, Task<기사운송사진업로드결과>> uploadPhoto,
    Func<기사운송사진업로드결과, 기사상차인수증입력, CancellationToken, Task> completePickup,
    DriverTransportProofCommandRunner run,
    Action<string, DriverTransportProofMessageTone> publishStatus) : 조립ViewModelBase
{
    private string? _previewUrl;
    private string? _fileName;
    private 기사운송사진업로드결과? _upload;
    private string _receiptEvidenceMethod = "사진";
    private string? _recipientName;
    private string? _recipientOrganization;
    private string? _recipientSignature;
    private string? _driverSignature;
    private bool _receiptConfirmed;
    private bool _signatureOmitted;
    private string? _signatureOmissionReason;

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

    public string ReceiptEvidenceMethod
    {
        get => _receiptEvidenceMethod;
        set => SetProperty(ref _receiptEvidenceMethod, value);
    }

    public string? RecipientName
    {
        get => _recipientName;
        set => SetProperty(ref _recipientName, value);
    }

    public string? RecipientOrganization
    {
        get => _recipientOrganization;
        set => SetProperty(ref _recipientOrganization, value);
    }

    public string? RecipientSignature
    {
        get => _recipientSignature;
        set => SetProperty(ref _recipientSignature, value);
    }

    public string? DriverSignature
    {
        get => _driverSignature;
        set => SetProperty(ref _driverSignature, value);
    }

    public bool ReceiptConfirmed
    {
        get => _receiptConfirmed;
        set => SetProperty(ref _receiptConfirmed, value);
    }

    public bool SignatureOmitted
    {
        get => _signatureOmitted;
        set => SetProperty(ref _signatureOmitted, value);
    }

    public string? SignatureOmissionReason
    {
        get => _signatureOmissionReason;
        set => SetProperty(ref _signatureOmissionReason, value);
    }

    public Task UploadAsync(IBrowserFile file)
    {
        ClearEvidence();
        return run("상차 사진을 업로드했습니다.", async cancellationToken =>
        {
            var image = await DriverTransportProofImageReader.ReadAsync(file, cancellationToken);
            await UploadImageCoreAsync(image, cancellationToken);
        });
    }

    public Task UploadImageAsync(DriverTransportProofImage image)
    {
        ClearEvidence();
        return run(
            "상차 사진을 업로드했습니다.",
            cancellationToken => UploadImageCoreAsync(image, cancellationToken));
    }

    public Task CompleteAsync()
    {
        if (Upload is null)
        {
            publishStatus(
                "상차 사진 업로드가 완료되어야 상차 완료 처리를 할 수 있습니다.",
                DriverTransportProofMessageTone.Warning);
            return Task.CompletedTask;
        }

        var upload = Upload;
        var receipt = BuildReceiptInput();
        return run(
            "상차 완료 상태를 서버에 반영했습니다.",
            cancellationToken => completePickup(upload, receipt, cancellationToken));
    }

    public void Reset()
    {
        ClearEvidence();
        ReceiptEvidenceMethod = "사진";
        RecipientName = null;
        RecipientOrganization = null;
        RecipientSignature = null;
        DriverSignature = null;
        ReceiptConfirmed = false;
        SignatureOmitted = false;
        SignatureOmissionReason = null;
    }

    private async Task UploadImageCoreAsync(
        DriverTransportProofImage image,
        CancellationToken cancellationToken)
    {
        var upload = await uploadPhoto(
            transportId(),
            운송증빙단계.상차,
            image.FileName,
            image.ContentType,
            image.Bytes,
            cancellationToken);
        PreviewUrl = image.PreviewUrl;
        FileName = image.FileName;
        Upload = upload;
    }

    private 기사상차인수증입력 BuildReceiptInput()
        => new()
        {
            인수증증빙방식 = ReceiptEvidenceMethod,
            인수자명 = RecipientName,
            인수자소속 = RecipientOrganization,
            인수자서명 = RecipientSignature,
            기사서명 = DriverSignature,
            인수증확인완료 = ReceiptConfirmed,
            인수증서명생략확인 = SignatureOmitted,
            인수증서명생략사유 = SignatureOmitted ? SignatureOmissionReason : null
        };

    private void ClearEvidence()
    {
        PreviewUrl = null;
        FileName = null;
        Upload = null;
    }
}
