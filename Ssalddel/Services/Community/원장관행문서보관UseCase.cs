using System.Text;
using System.Text.Json;
using FluentResults;
using Ssalddel.Contracts.Common.Documents;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.Services.Documents;

namespace Ssalddel.Services.Community;

public interface I원장관행문서보관UseCase
{
    Task<Result<원장관행문서보관응답>> 보관Async(
        string 원장Id,
        string 현재UserId,
        string 문서종류코드,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Application,
    "사용자가 선택한 원장 관행 문서 HTML 초안을 기존 암호화·보관·감사로그 문서관리 모듈에 저장합니다.",
    ContractType = typeof(I원장관행문서보관UseCase),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite | SsalddelCodeEffect.ObjectStorageWrite,
    Boundary = "수동 보관만 수행하며 문서 발행, 서명, 신고, 외부 전송 또는 운영 상태 전이를 수행하지 않습니다.")]
public sealed class 원장관행문서보관UseCase : I원장관행문서보관UseCase
{
    public const string 문서정책코드 = 원장관행문서정책코드.검토초안;

    private readonly I원장관행문서초안UseCase _초안UseCase;
    private readonly I문서관리Service _문서관리Service;

    public 원장관행문서보관UseCase(
        I원장관행문서초안UseCase 초안UseCase,
        I문서관리Service 문서관리Service)
    {
        _초안UseCase = 초안UseCase;
        _문서관리Service = 문서관리Service;
    }

    public async Task<Result<원장관행문서보관응답>> 보관Async(
        string 원장Id,
        string 현재UserId,
        string 문서종류코드,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(문서종류코드))
        {
            return 실패("보관할 문서 종류 코드가 필요합니다.", 400);
        }

        var 초안결과 = await _초안UseCase.생성Async(
            원장Id,
            현재UserId,
            문서종류코드,
            cancellationToken);
        if (초안결과.IsFailed)
        {
            return Result.Fail<원장관행문서보관응답>(초안결과.Errors);
        }

        var 초안 = 초안결과.Value.문서목록.Single();
        try
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(초안.Html));
            var 저장문서 = await _문서관리Service.CreateDocumentAsync(
                new 문서생성요청
                {
                    의뢰Id = 초안결과.Value.원장Id,
                    문서코드 = 문서정책코드,
                    문서명 = 초안.문서명,
                    파일명 = 초안.파일명,
                    ContentType = 초안.ContentType,
                    암호화여부 = true,
                    다운로드허용여부 = true,
                    생성자 = 현재UserId,
                    문서분류코드 = 문서분류Resolver.Resolve(문서정책코드, 초안.문서종류코드),
                    생명주기상태코드 = 초안.상태코드 == 원장관행문서초안상태코드.입력필요
                        ? 문서생명주기상태코드.입력필요
                        : 문서생명주기상태코드.검토준비,
                    원천원장Id = 초안결과.Value.원장Id,
                    원천원장종류코드 = 초안결과.Value.원장템플릿Key,
                    원천원장Revision = 초안결과.Value.원장Revision,
                    원천문서종류코드 = 초안.문서종류코드,
                    템플릿버전 = "1.0",
                    생성모드코드 = 초안.생성모드코드,
                    발급주체코드 = 초안.발급주체코드,
                    외부발급원본대체가능여부 = 초안.외부발급원본대체가능여부,
                    구조화스냅샷Json = JsonSerializer.Serialize(new
                    {
                        초안.문서종류코드,
                        초안.초안문서번호,
                        초안.상태코드,
                        초안.원천원장Revision,
                        확인필드목록 = 초안.필드목록.Select(field => new
                        {
                            field.필드코드,
                            field.확인됨
                        }),
                        품목행수 = 초안.품목행목록.Count,
                        필수입력누락수 = 초안.필수입력누락목록.Count,
                        경고수 = 초안.경고목록.Count
                    }),
                    관련StableId목록Json = JsonSerializer.Serialize(
                        new[]
                        {
                            문서StableId.만들기(문서StableId종류코드.커뮤니티원장, 초안결과.Value.원장Id),
                            문서StableId.만들기(문서StableId종류코드.문서초안, 초안.초안문서번호)
                        })
                },
                stream,
                cancellationToken);
            if (저장문서 is null)
            {
                return 실패("문서관리 모듈이 문서 초안을 저장하지 못했습니다.", 500);
            }

            return Result.Ok(new 원장관행문서보관응답
            {
                저장문서Id = 저장문서.Id,
                원장Id = 초안결과.Value.원장Id,
                원장Revision = 초안결과.Value.원장Revision,
                문서종류코드 = 초안.문서종류코드,
                문서명 = 저장문서.문서명,
                파일명 = 저장문서.파일명,
                생성상태 = 저장문서.생성상태,
                문서분류코드 = 저장문서.문서분류코드,
                생명주기상태코드 = 저장문서.생명주기상태코드,
                내용Sha256 = 저장문서.내용Sha256,
                암호화됨 = 저장문서.암호화됨,
                다운로드허용여부 = 저장문서.다운로드허용여부
            });
        }
        catch (InvalidOperationException exception)
        {
            return 실패(exception.Message, 409);
        }
    }

    private static Result<원장관행문서보관응답> 실패(string message, int statusCode)
        => Result.Fail<원장관행문서보관응답>(
            new Error(message).WithMetadata("StatusCode", statusCode));
}
