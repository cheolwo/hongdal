using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services;

namespace Hongdal.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/files/pod")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 파일POD관리Controller : ControllerBase
{
    private readonly IGoogleCloudStorageService _googleCloudStorageService;
    private readonly IAdminFilePodStore _store;

    public 파일POD관리Controller(IGoogleCloudStorageService googleCloudStorageService, IAdminFilePodStore store)
    {
        _googleCloudStorageService = googleCloudStorageService;
        _store = store;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> 업로드([FromForm] 파일POD업로드요청 request, CancellationToken cancellationToken)
    {
        if (request == null || request.File == null)
        {
            return BadRequest("file is required");
        }

        if (request.File.Length <= 0)
        {
            return BadRequest("empty file is not allowed");
        }

        if (string.IsNullOrWhiteSpace(request.FileType))
        {
            return BadRequest("fileType is required");
        }

        await using var stream = request.File.OpenReadStream();
        var uploadResult = await _googleCloudStorageService.UploadAsync(
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.Folder,
            cancellationToken);

        var metadata = _store.Add(new AdminFilePodMetadata(
            Id: Guid.NewGuid(),
            FileType: request.FileType.Trim(),
            RequestId: request.RequestId?.Trim() ?? string.Empty,
            BucketName: uploadResult.BucketName,
            ObjectName: uploadResult.ObjectName,
            Url: uploadResult.PublicUrl,
            OriginalFileName: request.File.FileName,
            UploadStatus: "업로드완료",
            UploadedAtUtc: DateTime.UtcNow,
            UpdatedAtUtc: DateTime.UtcNow));

        return Ok(new 파일POD응답
        {
            Id = metadata.Id,
            FileType = metadata.FileType,
            RequestId = metadata.RequestId,
            BucketName = metadata.BucketName,
            ObjectName = metadata.ObjectName,
            Url = metadata.Url,
            OriginalFileName = metadata.OriginalFileName,
            UploadStatus = metadata.UploadStatus,
            UploadedAtUtc = metadata.UploadedAtUtc,
            UpdatedAtUtc = metadata.UpdatedAtUtc
        });
    }

    [HttpGet]
    public IActionResult 목록조회([FromQuery] string? fileType, [FromQuery] string? requestId)
    {
        var items = _store.List(fileType, requestId)
            .Select(x => new 파일POD응답
            {
                Id = x.Id,
                FileType = x.FileType,
                RequestId = x.RequestId,
                BucketName = x.BucketName,
                ObjectName = x.ObjectName,
                Url = x.Url,
                OriginalFileName = x.OriginalFileName,
                UploadStatus = x.UploadStatus,
                UploadedAtUtc = x.UploadedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToList();

        return Ok(items);
    }

    [HttpPatch("{id:guid}/status")]
    public IActionResult 업로드상태변경(Guid id, [FromBody] 파일POD상태변경요청 request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UploadStatus))
        {
            return BadRequest("uploadStatus is required");
        }

        var updated = _store.UpdateStatus(id, request.UploadStatus.Trim());
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(new 파일POD응답
        {
            Id = updated.Id,
            FileType = updated.FileType,
            RequestId = updated.RequestId,
            BucketName = updated.BucketName,
            ObjectName = updated.ObjectName,
            Url = updated.Url,
            OriginalFileName = updated.OriginalFileName,
            UploadStatus = updated.UploadStatus,
            UploadedAtUtc = updated.UploadedAtUtc,
            UpdatedAtUtc = updated.UpdatedAtUtc
        });
    }
}

public sealed class 파일POD업로드요청
{
    public IFormFile File { get; set; } = null!;
    public string FileType { get; set; } = string.Empty;
    public string? RequestId { get; set; }
    public string? Folder { get; set; }
}

public sealed class 파일POD상태변경요청
{
    public string UploadStatus { get; set; } = string.Empty;
}

public sealed class 파일POD응답
{
    public Guid Id { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string UploadStatus { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
