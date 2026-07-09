using FluentResults;

namespace Hongdal.Application.Driver.Transport;

public static class 운송상차인수증증빙정책
{
    public static Result<운송상차인수증증빙?> 증빙생성(
        운송상차완료Command request,
        bool 인수증필요,
        bool 서명필수)
    {
        var evidenceMethod = ResolveEvidenceMethod(request);
        var hasCompleteSignature = HasReceiptEvidence(request);
        var hasDocumentPhotoEvidence = HasDocumentPhotoEvidence(request, evidenceMethod);
        var hasReceiptEvidence = hasCompleteSignature || hasDocumentPhotoEvidence;
        var hasPartialSignature = HasAnySignatureInput(request);

        if (서명필수 && !hasReceiptEvidence)
        {
            return Result.Fail<운송상차인수증증빙?>("인수증 정산 운송은 상차 완료 전에 직접 서명 또는 서명된 문서 사진 증빙이 필요합니다.");
        }

        if (!hasDocumentPhotoEvidence && hasPartialSignature && !hasCompleteSignature)
        {
            return Result.Fail<운송상차인수증증빙?>("인수증 서명을 남기려면 인수자명, 인수자 서명, 기사 서명을 모두 입력해야 합니다.");
        }

        if (!인수증필요)
        {
            return Result.Ok<운송상차인수증증빙?>(null);
        }

        return Result.Ok<운송상차인수증증빙?>(new 운송상차인수증증빙(
            hasReceiptEvidence,
            서명필수,
            evidenceMethod,
            hasCompleteSignature ? request.인수자명!.Trim() : null,
            hasCompleteSignature ? request.인수자소속?.Trim() : null,
            hasCompleteSignature ? request.인수자서명!.Trim() : null,
            hasCompleteSignature ? request.기사서명!.Trim() : null,
            hasReceiptEvidence ? null : ResolveOmissionReason(request),
            request.상차사진ObjectName,
            request.상차사진Url));
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
