using FluentResults;
using Hongdal.Contracts.Driver.Transport;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송상차완료CommandHandler : IRequestHandler<운송상차완료Command, Result<기사운송상태변경응답>>
{
    private readonly HongdalContext _db;
    private readonly I기사운송상태변경CommandExecutor _executor;
    private readonly I운송증빙첨부JsonWriter _attachmentWriter;

    public 운송상차완료CommandHandler(
        HongdalContext db,
        I기사운송상태변경CommandExecutor executor,
        I운송증빙첨부JsonWriter attachmentWriter)
    {
        _db = db;
        _executor = executor;
        _attachmentWriter = attachmentWriter;
    }

    public async Task<Result<기사운송상태변경응답>> Handle(운송상차완료Command request, CancellationToken cancellationToken)
    {
        var transport = await _db.배송_운송
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.기사_운송자 == request.기사Id, cancellationToken);
        if (transport is null)
        {
            return Result.Fail<기사운송상태변경응답>("운송을 찾을 수 없습니다.");
        }

        if (string.IsNullOrWhiteSpace(request.상차사진ObjectName))
        {
            return Result.Fail<기사운송상태변경응답>("상차 완료 사진 업로드가 확인되어야 상차 완료 처리할 수 있습니다.");
        }

        var receiptRequired = await _db.화주운송의뢰
            .AsNoTracking()
            .Where(x => x.의뢰Id == transport.운송번호)
            .Select(x => new
            {
                필요 = x.증빙방식 == "인수증" || x.결제수단.Contains("인수증"),
                서명필수 = x.요청사항.Contains("서명필수")
                    || x.요청사항.Contains("서명 필수")
                    || x.정산메모.Contains("서명필수")
                    || x.정산메모.Contains("서명 필수")
            })
            .FirstOrDefaultAsync(cancellationToken);

        var isReceiptRequired = receiptRequired?.필요 == true;
        var isSignatureRequired = receiptRequired?.서명필수 == true;
        var evidenceMethod = ResolveEvidenceMethod(request);
        var hasCompleteSignature = HasReceiptEvidence(request);
        var hasDocumentPhotoEvidence = HasDocumentPhotoEvidence(request, evidenceMethod);
        var hasReceiptEvidence = hasCompleteSignature || hasDocumentPhotoEvidence;
        var hasPartialSignature = HasAnySignatureInput(request);

        if (isSignatureRequired && !hasReceiptEvidence)
        {
            return Result.Fail<기사운송상태변경응답>("인수증 정산 운송은 상차 완료 전에 직접 서명 또는 서명된 문서 사진 증빙이 필요합니다.");
        }

        if (!hasDocumentPhotoEvidence && hasPartialSignature && !hasCompleteSignature)
        {
            return Result.Fail<기사운송상태변경응답>("인수증 서명을 남기려면 인수자명, 인수자 서명, 기사 서명을 모두 입력해야 합니다.");
        }

        var receiptEvidence = isReceiptRequired
            ? new 운송상차인수증증빙(
                hasReceiptEvidence,
                isSignatureRequired,
                evidenceMethod,
                hasCompleteSignature ? request.인수자명!.Trim() : null,
                hasCompleteSignature ? request.인수자소속?.Trim() : null,
                hasCompleteSignature ? request.인수자서명!.Trim() : null,
                hasCompleteSignature ? request.기사서명!.Trim() : null,
                hasReceiptEvidence ? null : ResolveOmissionReason(request),
                request.상차사진ObjectName,
                request.상차사진Url)
            : null;

        return await _executor.실행Async(
            new 기사운송상태변경요청(
                request.기사Id,
                request.Id,
                request.참여자Id,
                request.실행역할,
                "상차완료",
                nameof(운송상차완료됨Event),
                context => new 운송상차완료됨Event(
                    request.기사Id,
                    context.운송.Id,
                    context.운송.운송번호,
                    context.운송.출발지,
                    context.운송.도착지,
                    context.이전상태,
                    context.운송.상태,
                    context.발생시각Utc,
                    context.TraceId,
                    receiptEvidence),
                (entity, changedAt) => _attachmentWriter.추가(
                    entity,
                    new 운송증빙첨부(
                        "pickup-complete-photo",
                        request.상차사진ObjectName,
                        request.상차사진Url,
                        request.기사Id,
                        changedAt,
                        new Dictionary<string, object?>
                        {
                            ["receiptEvidenceMethod"] = request.인수증증빙방식?.Trim(),
                            ["receiptConfirmed"] = request.인수증확인완료,
                            ["receiptSignatureOmitted"] = request.인수증서명생략확인
                        }))),
            cancellationToken);
    }

    private static bool HasReceiptEvidence(운송상차완료Command request)
        => request.인수증확인완료
           && !string.IsNullOrWhiteSpace(request.인수자명)
           && !string.IsNullOrWhiteSpace(request.인수자서명)
           && !string.IsNullOrWhiteSpace(request.기사서명);

    private static bool HasDocumentPhotoEvidence(운송상차완료Command request, string evidenceMethod)
        => string.Equals(evidenceMethod, "문서사진", StringComparison.Ordinal)
           && request.인수증확인완료
           && !string.IsNullOrWhiteSpace(request.상차사진ObjectName);

    private static bool HasAnySignatureInput(운송상차완료Command request)
        => request.인수증확인완료
           || !string.IsNullOrWhiteSpace(request.인수자명)
           || !string.IsNullOrWhiteSpace(request.인수자서명)
           || !string.IsNullOrWhiteSpace(request.기사서명);

    private static string ResolveOmissionReason(운송상차완료Command request)
        => request.인수증서명생략확인
            ? string.IsNullOrWhiteSpace(request.인수증서명생략사유)
                ? "현장 합의에 따라 상차 인수 서명 없이 진행"
                : request.인수증서명생략사유.Trim()
            : "서명 조건 없음";

    private static string ResolveEvidenceMethod(운송상차완료Command request)
    {
        var method = request.인수증증빙방식?.Trim();
        return string.IsNullOrWhiteSpace(method) ? "직접서명" : method;
    }
}
