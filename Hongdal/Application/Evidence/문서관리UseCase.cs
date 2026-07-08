using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.ViewSettings;
using Microsoft.AspNetCore.Http;
using 홍달.Services.Audit;
using 홍달.Services.Documents;

namespace Hongdal.Application.Evidence;

public interface I문서관리UseCase
{
    Task<Result<IReadOnlyList<문서정책요약응답>>> 정책목록조회Async(CancellationToken cancellationToken);
    Task<Result<문서정책요약응답>> 정책수정Async(string documentCode, 문서정책수정요청? request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<문서조회요약응답>>> 목록조회Async(string? documentCode, string? requestId, string? status, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<문서조회로그요약응답>>> 로그목록조회Async(long? documentId, CancellationToken cancellationToken);
    Task<Result<문서조회요약응답>> 업로드Async(문서업로드Command command, CancellationToken cancellationToken);
    Task<Result<문서다운로드응답>> 다운로드Async(long id, 문서다운로드Context context, CancellationToken cancellationToken);
}

public sealed record 문서업로드Command(
    IFormFile? File,
    string? 의뢰Id,
    long? 배송운송Id,
    string? 문서코드,
    string? 문서명,
    bool? 암호화여부,
    bool? 다운로드허용여부,
    string? 생성자);

public sealed record 문서다운로드Context(
    string UserId,
    string UserName,
    string RoleName,
    string Route,
    string TraceId,
    string ClientIp,
    string UserAgent);

[HongdalApiWorkflow(HongdalWorkflow.DomesticTransport)]
[HongdalUseCase("문서 관리", Summary = "운송 증빙 문서 정책, 업로드, 다운로드와 감사 로그를 관리합니다.")]
[HongdalUseCaseActor(HongdalActor.PlatformOperator)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Include,
    "파일업로드UseCase",
    Condition = "문서 원본이나 첨부 파일을 업로드하는 경우",
    Summary = "문서 관리는 파일 수신과 저장 경로 결정을 포함합니다.")]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Include,
    "사용자행위로그조회UseCase",
    Condition = "문서 다운로드와 조회 이력을 추적하는 경우",
    Summary = "문서 다운로드는 감사 로그 기록과 추후 조회 가능성을 포함합니다.")]
public sealed class 문서관리UseCase : I문서관리UseCase
{
    private readonly I문서관리Service _documentService;
    private readonly I사용자행위로그Service _activityLogService;

    public 문서관리UseCase(I문서관리Service documentService, I사용자행위로그Service activityLogService)
    {
        _documentService = documentService;
        _activityLogService = activityLogService;
    }

    public async Task<Result<IReadOnlyList<문서정책요약응답>>> 정책목록조회Async(CancellationToken cancellationToken)
    {
        return Result.Ok(await _documentService.GetPoliciesAsync(cancellationToken));
    }

    public async Task<Result<문서정책요약응답>> 정책수정Async(string documentCode, 문서정책수정요청? request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentCode))
        {
            return Result.Fail<문서정책요약응답>("documentCode is required");
        }

        if (request is null)
        {
            return Result.Fail<문서정책요약응답>("request body is required");
        }

        var updated = await _documentService.UpdatePolicyAsync(documentCode.Trim(), request, cancellationToken);
        return updated is null
            ? Result.Fail<문서정책요약응답>(new Error("문서 정책을 찾을 수 없습니다.").WithMetadata("StatusCode", StatusCodes.Status404NotFound))
            : Result.Ok(updated);
    }

    public async Task<Result<IReadOnlyList<문서조회요약응답>>> 목록조회Async(
        string? documentCode,
        string? requestId,
        string? status,
        CancellationToken cancellationToken)
    {
        return Result.Ok(await _documentService.ListDocumentsAsync(documentCode, requestId, status, cancellationToken));
    }

    public async Task<Result<IReadOnlyList<문서조회로그요약응답>>> 로그목록조회Async(long? documentId, CancellationToken cancellationToken)
    {
        return Result.Ok(await _documentService.ListLogsAsync(documentId, cancellationToken));
    }

    public async Task<Result<문서조회요약응답>> 업로드Async(문서업로드Command command, CancellationToken cancellationToken)
    {
        if (command.File is null || command.File.Length <= 0)
        {
            return Result.Fail<문서조회요약응답>("file is required");
        }

        try
        {
            await using var stream = command.File.OpenReadStream();
            var created = await _documentService.CreateDocumentAsync(new 문서생성요청
            {
                의뢰Id = command.의뢰Id ?? string.Empty,
                배송운송Id = command.배송운송Id,
                문서코드 = command.문서코드 ?? string.Empty,
                문서명 = command.문서명 ?? string.Empty,
                파일명 = command.File.FileName,
                ContentType = command.File.ContentType,
                암호화여부 = command.암호화여부,
                다운로드허용여부 = command.다운로드허용여부,
                생성자 = command.생성자
            }, stream, cancellationToken);

            return created is null
                ? Result.Fail<문서조회요약응답>("문서 생성에 실패했습니다.")
                : Result.Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail<문서조회요약응답>(ex.Message);
        }
    }

    public async Task<Result<문서다운로드응답>> 다운로드Async(
        long id,
        문서다운로드Context context,
        CancellationToken cancellationToken)
    {
        var result = await _documentService.DownloadAsync(id, cancellationToken);
        if (result is null)
        {
            return Result.Fail<문서다운로드응답>(new Error("문서를 찾을 수 없습니다.").WithMetadata("StatusCode", StatusCodes.Status404NotFound));
        }

        await _activityLogService.기록Async(new 사용자행위로그기록
        {
            AppKey = App식별자.HongdalAdmin,
            UserId = context.UserId,
            UserName = context.UserName,
            RoleName = context.RoleName,
            ActionType = "Document",
            ActionName = "Download",
            Route = context.Route,
            TraceId = context.TraceId,
            IsSuccess = true,
            ClientIp = context.ClientIp,
            UserAgent = context.UserAgent,
            OccurredAtUtc = DateTime.UtcNow,
            MetadataJson = $"{{\"documentId\":{id}}}"
        }, cancellationToken);

        return Result.Ok(result);
    }
}
