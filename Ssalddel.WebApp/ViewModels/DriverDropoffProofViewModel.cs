using Microsoft.AspNetCore.Components.Forms;
using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Services;

namespace Ssalddel.WebApp.ViewModels;

public sealed class DriverDropoffProofViewModel(
    Func<long> transportId,
    Func<long, 운송증빙단계, string, string, byte[], CancellationToken, Task<기사운송사진업로드결과>> uploadPhoto,
    Func<기사운송사진업로드결과, CancellationToken, Task> completeDropoff,
    DriverTransportProofCommandRunner run,
    Action<string, DriverTransportProofMessageTone> publishStatus) : 조립ViewModelBase
{
    private string? _previewUrl;
    private string? _fileName;
    private 기사운송사진업로드결과? _upload;

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
        return run("하차 사진을 업로드했습니다.", async cancellationToken =>
        {
            var image = await DriverTransportProofImageReader.ReadAsync(file, cancellationToken);
            await UploadImageCoreAsync(image, cancellationToken);
        });
    }

    public Task UploadImageAsync(DriverTransportProofImage image)
    {
        ClearEvidence();
        return run(
            "하차 사진을 업로드했습니다.",
            cancellationToken => UploadImageCoreAsync(image, cancellationToken));
    }

    public Task CompleteAsync()
    {
        if (Upload is null)
        {
            publishStatus(
                "하차 사진 업로드가 완료되어야 하차 완료 처리를 할 수 있습니다.",
                DriverTransportProofMessageTone.Warning);
            return Task.CompletedTask;
        }

        var upload = Upload;
        return run(
            "하차 완료 상태를 서버에 반영했습니다.",
            cancellationToken => completeDropoff(upload, cancellationToken));
    }

    public void Reset() => ClearEvidence();

    private async Task UploadImageCoreAsync(
        DriverTransportProofImage image,
        CancellationToken cancellationToken)
    {
        var upload = await uploadPhoto(
            transportId(),
            운송증빙단계.하차,
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
