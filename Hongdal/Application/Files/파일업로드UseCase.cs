using FluentResults;
using Hongdal.ApiMetadata;
using Microsoft.AspNetCore.Http;
using 홍달.Services.External.Google;
using 홍달.Services.Storage.Local;

namespace Hongdal.Application.Files;

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

[HongdalApiWorkflow(HongdalWorkflow.DomesticTransport)]
[HongdalUseCase("파일 업로드", Summary = "명령 실행이나 업무 증빙에 필요한 파일을 플랫폼 저장소에 업로드합니다.")]
[HongdalUseCaseActor(HongdalActor.Shipper)]
[HongdalUseCaseActor(HongdalActor.Driver)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
public sealed class 파일업로드UseCase : I파일업로드UseCase
{
    private readonly IGoogleCloudStorageService _googleCloudStorageService;
    private readonly ICommandFileStoragePathResolver _pathResolver;

    public 파일업로드UseCase(
        IGoogleCloudStorageService googleCloudStorageService,
        ICommandFileStoragePathResolver pathResolver)
    {
        _googleCloudStorageService = googleCloudStorageService;
        _pathResolver = pathResolver;
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
}
