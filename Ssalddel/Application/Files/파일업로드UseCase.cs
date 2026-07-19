using FluentResults;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using 살뜰.Services.External.Google;
using 살뜰.Services.Storage.Local;

namespace Ssalddel.Application.Files;

public interface I파일업로드UseCase
{
    Task<Result<파일업로드응답>> 업로드Async(파일업로드Command command, CancellationToken cancellationToken);
}

public sealed record 파일업로드Command(
    IFormFile? File,
    string? CommandName,
    string? ReferenceId);

public sealed record 파일업로드응답(
    string BucketName,
    string ObjectName,
    string Url);

[SsalddelApiWorkflow(SsalddelWorkflow.DomesticTransport)]
[SsalddelUseCase("파일 업로드", Summary = "명령 실행이나 업무 증빙에 필요한 파일을 플랫폼 저장소에 업로드합니다.")]
[SsalddelUseCaseActor(SsalddelActor.Shipper)]
[SsalddelUseCaseActor(SsalddelActor.Driver)]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator, SsalddelUseCaseActorRole.Supporting)]
public sealed class 파일업로드UseCase : I파일업로드UseCase
{
    private readonly IGoogleCloudStorageService _googleCloudStorageService;
    private readonly ICommandFileStoragePathResolver _pathResolver;
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 파일업로드UseCase(
        IGoogleCloudStorageService googleCloudStorageService,
        ICommandFileStoragePathResolver pathResolver,
        SsalddelContext db,
        ICurrentUserAccessor currentUserAccessor)
    {
        _googleCloudStorageService = googleCloudStorageService;
        _pathResolver = pathResolver;
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<파일업로드응답>> 업로드Async(파일업로드Command command, CancellationToken cancellationToken)
    {
        if (command.File is null)
        {
            return Result.Fail<파일업로드응답>("file is required");
        }

        if (command.File.Length <= 0)
        {
            return Result.Fail<파일업로드응답>("empty file is not allowed");
        }

        if (string.IsNullOrWhiteSpace(command.CommandName))
        {
            return Result.Fail<파일업로드응답>("commandName is required");
        }

        var authorizationResult = await ValidateReferenceOwnershipAsync(command, cancellationToken);
        if (authorizationResult.IsFailed)
        {
            return Result.Fail<파일업로드응답>(authorizationResult.Errors.Select(x => x.Message));
        }

        var folder = _pathResolver.ResolveCommandFolder(command.CommandName, command.ReferenceId);
        await using var stream = command.File.OpenReadStream();
        var result = await _googleCloudStorageService.UploadAsync(
            stream,
            command.File.FileName,
            command.File.ContentType,
            folder,
            cancellationToken);

        return Result.Ok(new 파일업로드응답(
            result.BucketName,
            result.ObjectName,
            result.PublicUrl));
    }

    private async Task<Result> ValidateReferenceOwnershipAsync(파일업로드Command command, CancellationToken cancellationToken)
    {
        if (!파일업로드권한정책.운송증빙업로드인가(command.CommandName))
        {
            return Result.Ok();
        }

        if (!long.TryParse(command.ReferenceId, out var transportId) || transportId <= 0)
        {
            return Result.Fail("운송 증빙 업로드에는 유효한 referenceId가 필요합니다.");
        }

        var transport = await _db.운송원장
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == transportId, cancellationToken);
        if (transport is null)
        {
            return Result.Fail("참조 운송을 찾을 수 없습니다.");
        }

        return 파일업로드권한정책.운송증빙업로드권한있음(transport, _currentUserAccessor.UserId, _currentUserAccessor.Role)
            ? Result.Ok()
            : Result.Fail("파일 업로드 권한이 없습니다.");
    }
}
