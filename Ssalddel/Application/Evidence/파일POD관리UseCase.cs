using FluentResults;
using Ssalddel.ApiMetadata;
using Microsoft.AspNetCore.Http;
using Ssalddel.Services.Storage;
using 살뜰.Services.Storage.Local;

namespace Ssalddel.Application.Evidence;

public interface I파일POD관리UseCase
{
    Task<Result<파일POD응답>> 업로드Async(파일POD업로드Command command, CancellationToken cancellationToken);
    Result<IReadOnlyList<파일POD응답>> 목록조회(string? fileType, string? requestId);
    Result<파일POD응답> 업로드상태변경(Guid id, string? uploadStatus);
}

public sealed record 파일POD업로드Command(
    IFormFile? File,
    string? FileType,
    string? RequestId);

public sealed record 파일POD응답(
    Guid Id,
    string FileType,
    string RequestId,
    string BucketName,
    string ObjectName,
    string Url,
    string OriginalFileName,
    string UploadStatus,
    DateTime UploadedAtUtc,
    DateTime UpdatedAtUtc);

[SsalddelApiWorkflow(SsalddelWorkflow.DomesticTransport)]
[SsalddelUseCase("파일 POD 관리", Summary = "운영자가 운송 증빙 파일을 업로드하고 상태를 조정합니다.")]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator)]
[SsalddelUseCaseRelation(
    SsalddelUseCaseRelationKind.Include,
    "파일업로드UseCase",
    Condition = "POD 파일을 플랫폼 저장소에 업로드하는 경우",
    Summary = "POD 관리는 파일 업로드와 저장 위치 결정을 포함합니다.")]
[SsalddelUseCaseRelation(
    SsalddelUseCaseRelationKind.Extend,
    "문서관리UseCase",
    Condition = "POD가 운송 증빙 문서 원장으로 보관되어야 하는 경우",
    Summary = "POD 파일을 문서 정책, 다운로드, 감사 로그 흐름으로 확장합니다.")]
public sealed class 파일POD관리UseCase : I파일POD관리UseCase
{
    private readonly IObjectStorageService _objectStorageService;
    private readonly ICommandFileStoragePathResolver _pathResolver;
    private readonly IAdminFilePodStore _store;

    public 파일POD관리UseCase(
        IObjectStorageService objectStorageService,
        ICommandFileStoragePathResolver pathResolver,
        IAdminFilePodStore store)
    {
        _objectStorageService = objectStorageService;
        _pathResolver = pathResolver;
        _store = store;
    }

    public async Task<Result<파일POD응답>> 업로드Async(파일POD업로드Command command, CancellationToken cancellationToken)
    {
        if (command.File is null)
        {
            return Result.Fail<파일POD응답>("file is required");
        }

        if (command.File.Length <= 0)
        {
            return Result.Fail<파일POD응답>("empty file is not allowed");
        }

        if (string.IsNullOrWhiteSpace(command.FileType))
        {
            return Result.Fail<파일POD응답>("fileType is required");
        }

        await using var stream = command.File.OpenReadStream();
        var fileType = command.FileType.Trim();
        var requestId = command.RequestId?.Trim() ?? string.Empty;
        var folder = _pathResolver.ResolveAdminFilePodFolder(fileType, requestId);
        var uploadResult = await _objectStorageService.UploadAsync(
            stream,
            command.File.FileName,
            command.File.ContentType,
            folder,
            ObjectStorageAccess.Private,
            cancellationToken);

        var metadata = _store.Add(new AdminFilePodMetadata(
            Id: Guid.NewGuid(),
            FileType: fileType,
            RequestId: requestId,
            BucketName: uploadResult.ContainerName,
            ObjectName: uploadResult.ObjectName,
            Url: uploadResult.Url,
            OriginalFileName: command.File.FileName,
            UploadStatus: "업로드완료",
            UploadedAtUtc: DateTime.UtcNow,
            UpdatedAtUtc: DateTime.UtcNow));

        return Result.Ok(ToResponse(metadata));
    }

    public Result<IReadOnlyList<파일POD응답>> 목록조회(string? fileType, string? requestId)
    {
        var items = _store.List(fileType, requestId)
            .Select(ToResponse)
            .ToList();

        return Result.Ok<IReadOnlyList<파일POD응답>>(items);
    }

    public Result<파일POD응답> 업로드상태변경(Guid id, string? uploadStatus)
    {
        if (string.IsNullOrWhiteSpace(uploadStatus))
        {
            return Result.Fail<파일POD응답>("uploadStatus is required");
        }

        var updated = _store.UpdateStatus(id, uploadStatus.Trim());
        return updated is null
            ? Result.Fail<파일POD응답>(new Error("파일 POD 정보를 찾을 수 없습니다.").WithMetadata("StatusCode", StatusCodes.Status404NotFound))
            : Result.Ok(ToResponse(updated));
    }

    private static 파일POD응답 ToResponse(AdminFilePodMetadata metadata)
    {
        return new 파일POD응답(
            metadata.Id,
            metadata.FileType,
            metadata.RequestId,
            metadata.BucketName,
            metadata.ObjectName,
            metadata.Url,
            metadata.OriginalFileName,
            metadata.UploadStatus,
            metadata.UploadedAtUtc,
            metadata.UpdatedAtUtc);
    }
}
