using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public interface I화주운송의뢰UseCase
{
    Task<IReadOnlyList<화주운송의뢰응답>> 의뢰목록조회Async(
        string? shipperId,
        string? status,
        string? paymentStatus,
        string? dispatchStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<공개화물요약응답>> 공개화물요약조회Async(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<차량추천응답> 차량추천Async(
        차량추천요청 request,
        CancellationToken cancellationToken = default);

    Task<Result<화주운송의뢰응답>> 의뢰생성Async(
        화주운송의뢰생성요청 request,
        CancellationToken cancellationToken = default);

    Task<화주운송의뢰응답?> 의뢰단건조회Async(
        string requestId,
        CancellationToken cancellationToken = default);

    Task<Result<화주운송의뢰응답>> 의뢰수정Async(
        string requestId,
        화주운송의뢰수정요청 request,
        CancellationToken cancellationToken = default);

    Task<Result> 의뢰삭제Async(
        string requestId,
        CancellationToken cancellationToken = default);

    Task<Result<화주운송의뢰일괄미리보기응답>> 일괄미리보기Async(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<Result<화주운송의뢰일괄등록결과응답>> 일괄등록Async(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<Result<화주운송의뢰일괄등록결과응답>> 일괄미리보기확정등록Async(
        화주운송의뢰일괄확정등록요청 request,
        CancellationToken cancellationToken = default);

    Task<Result<화주운송의뢰응답>> 현장지급처리Async(
        string requestId,
        화주운송의뢰현장지급처리요청 request,
        CancellationToken cancellationToken = default);

    Task<Result<화주운송의뢰응답>> 후불승인Async(
        string requestId,
        화주운송의뢰후불승인요청 request,
        CancellationToken cancellationToken = default);

    Task<Result<화주운송의뢰응답>> 인수증등록Async(
        string requestId,
        화주운송의뢰인수증등록요청 request,
        CancellationToken cancellationToken = default);
}

[HongdalApiWorkflow(HongdalWorkflow.DomesticTransport)]
[HongdalUseCase("화주 운송 의뢰 관리", Summary = "화주가 운송 의뢰를 등록, 수정, 결제 처리, 인수증 등록까지 진행합니다.")]
[HongdalUseCaseActor(HongdalActor.Shipper)]
[HongdalUseCaseActor(HongdalActor.Driver, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseActor(HongdalActor.Recipient, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "문서관리UseCase",
    Condition = "인수증 거래, 전자서명, POD 증빙이 필요한 경우",
    Summary = "운송 의뢰의 상차·하차 증빙을 문서 관리 흐름으로 확장합니다.")]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "파일업로드UseCase",
    Condition = "사진, 인수증, 서명 파일 같은 첨부 증빙이 제출되는 경우",
    Summary = "운송 증빙 파일을 플랫폼 저장소 업로드 흐름으로 확장합니다.")]
public sealed class 화주운송의뢰UseCase : I화주운송의뢰UseCase
{
    private readonly ISender _sender;
    private readonly I화주운송의뢰일괄등록파서Service _bulkParser;
    private readonly I차량추천Service _vehicleRecommendationService;
    private readonly I화주운송요금정책검토Service _farePolicyReviewService;

    public 화주운송의뢰UseCase(
        ISender sender,
        I화주운송의뢰일괄등록파서Service bulkParser,
        I차량추천Service vehicleRecommendationService,
        I화주운송요금정책검토Service farePolicyReviewService)
    {
        _sender = sender;
        _bulkParser = bulkParser;
        _vehicleRecommendationService = vehicleRecommendationService;
        _farePolicyReviewService = farePolicyReviewService;
    }

    public async Task<IReadOnlyList<화주운송의뢰응답>> 의뢰목록조회Async(
        string? shipperId,
        string? status,
        string? paymentStatus,
        string? dispatchStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        => await _sender.Send(new 의뢰목록조회Query(shipperId, status, paymentStatus, dispatchStatus, page, pageSize), cancellationToken);

    public async Task<IReadOnlyList<공개화물요약응답>> 공개화물요약조회Async(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        => await _sender.Send(new 공개화물요약조회Query(page, pageSize), cancellationToken);

    public async Task<차량추천응답> 차량추천Async(
        차량추천요청 request,
        CancellationToken cancellationToken = default)
        => await _vehicleRecommendationService.추천Async(request, cancellationToken);

    public async Task<Result<화주운송의뢰응답>> 의뢰생성Async(
        화주운송의뢰생성요청 request,
        CancellationToken cancellationToken = default)
    {
        var policyReview = _farePolicyReviewService.검토(request.요금옵션, request.결제예정금액);
        if (policyReview.정책위반)
        {
            var errors = policyReview.경고목록
                .Concat(policyReview.이벤트코드목록.Select(x => $"요금정책이벤트:{x}"));
            return Result.Fail<화주운송의뢰응답>(errors);
        }

        return await _sender.Send(BuildCreateCommand(request), cancellationToken);
    }

    public async Task<화주운송의뢰응답?> 의뢰단건조회Async(
        string requestId,
        CancellationToken cancellationToken = default)
        => await _sender.Send(new 의뢰단건조회Query(requestId), cancellationToken);

    public async Task<Result<화주운송의뢰응답>> 의뢰수정Async(
        string requestId,
        화주운송의뢰수정요청 request,
        CancellationToken cancellationToken = default)
        => await _sender.Send(new 의뢰수정Command(
            requestId,
            new 운송조건입력값(request.운송방식, request.차량종류, request.요금옵션?.서비스레벨),
            new 화물정보입력값(request.화물?.화물종류, request.화물?.설명, request.화물?.수량, request.화물?.중량Kg, request.화물?.부피Cbm, request.화물?.화물파손주의여부, request.화물?.온도조건),
            new 위치정보입력값(request.픽업?.주소?.도로명주소, request.픽업?.주소?.상세주소, request.픽업?.주소?.위도, request.픽업?.주소?.경도, request.픽업?.연락처?.이름, request.픽업?.연락처?.전화번호, request.픽업?.시간창?.시작일시, request.픽업?.시간창?.종료일시),
            new 위치정보입력값(request.하차?.주소?.도로명주소, request.하차?.주소?.상세주소, request.하차?.주소?.위도, request.하차?.주소?.경도, request.하차?.연락처?.이름, request.하차?.연락처?.전화번호, request.하차?.시간창?.시작일시, request.하차?.시간창?.종료일시),
            new 요청조건입력값(request.요금옵션?.요청사항),
            request.정산조건 is null ? null : new 정산조건입력값(request.결제수단, request.정산조건)), cancellationToken);

    public async Task<Result> 의뢰삭제Async(
        string requestId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new 의뢰삭제Command(requestId), cancellationToken);
        return result.IsSuccess
            ? Result.Ok()
            : Result.Fail(result.Errors);
    }

    public async Task<Result<화주운송의뢰일괄미리보기응답>> 일괄미리보기Async(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var parsed = await _bulkParser.ParseAsync(stream, fileName, cancellationToken);
        if (parsed.행목록.Count == 0 && parsed.오류목록.Count > 0)
        {
            return Result.Fail<화주운송의뢰일괄미리보기응답>(parsed.오류목록);
        }

        return await _sender.Send(new 화주운송의뢰일괄미리보기Command(parsed.행목록, parsed.오류목록), cancellationToken);
    }

    public async Task<Result<화주운송의뢰일괄등록결과응답>> 일괄등록Async(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var previewResult = await 일괄미리보기Async(stream, fileName, cancellationToken);
        if (previewResult.IsFailed)
        {
            return Result.Fail<화주운송의뢰일괄등록결과응답>(previewResult.Errors);
        }

        var confirmRows = previewResult.Value.행목록
            .Where(x => x.유효함)
            .Select(x => new 화주운송의뢰일괄확정등록행
            {
                행번호 = x.행번호,
                등록여부 = x.등록대상여부,
                최종선택차량종류 = x.최종선택차량종류,
                원본행 = x.원본행
            })
            .ToArray();

        return await _sender.Send(new 화주운송의뢰일괄확정등록Command(confirmRows), cancellationToken);
    }

    public async Task<Result<화주운송의뢰일괄등록결과응답>> 일괄미리보기확정등록Async(
        화주운송의뢰일괄확정등록요청 request,
        CancellationToken cancellationToken = default)
        => await _sender.Send(new 화주운송의뢰일괄확정등록Command(request.행목록), cancellationToken);

    public async Task<Result<화주운송의뢰응답>> 현장지급처리Async(
        string requestId,
        화주운송의뢰현장지급처리요청 request,
        CancellationToken cancellationToken = default)
        => await _sender.Send(new 화주운송의뢰현장지급처리Command(requestId, request.현장지급메모), cancellationToken);

    public async Task<Result<화주운송의뢰응답>> 후불승인Async(
        string requestId,
        화주운송의뢰후불승인요청 request,
        CancellationToken cancellationToken = default)
        => await _sender.Send(new 화주운송의뢰후불승인Command(requestId, request.승인메모), cancellationToken);

    public async Task<Result<화주운송의뢰응답>> 인수증등록Async(
        string requestId,
        화주운송의뢰인수증등록요청 request,
        CancellationToken cancellationToken = default)
        => await _sender.Send(new 화주운송의뢰인수증등록Command(requestId, request.인수증번호, request.등록메모), cancellationToken);

    private static 의뢰생성Command BuildCreateCommand(화주운송의뢰생성요청 request)
    {
        var cargo = request.화물;
        var pickup = request.픽업;
        var dropoff = request.하차;
        var pricing = request.요금옵션;

        return new 의뢰생성Command(
            request.화주Id,
            request.운송방식,
            request.차량종류,
            request.결제수단,
            request.결제예정금액,
            request.정산조건,
            cargo.화물종류,
            cargo.설명,
            cargo.수량,
            cargo.길이Mm,
            cargo.폭Mm,
            cargo.높이Mm,
            cargo.중량Kg,
            cargo.부피Cbm,
            cargo.팔레트개수,
            cargo.화물파손주의여부,
            cargo.온도조건,
            pickup?.주소.도로명주소 ?? string.Empty,
            pickup?.주소.상세주소,
            pickup?.주소.위도,
            pickup?.주소.경도,
            pickup?.연락처.이름 ?? string.Empty,
            pickup?.연락처.전화번호 ?? string.Empty,
            pickup?.시간창?.시작일시 ?? default,
            pickup?.시간창?.종료일시 ?? default,
            dropoff?.주소.도로명주소 ?? string.Empty,
            dropoff?.주소.상세주소,
            dropoff?.주소.위도,
            dropoff?.주소.경도,
            dropoff?.연락처.이름 ?? string.Empty,
            dropoff?.연락처.전화번호 ?? string.Empty,
            dropoff?.시간창?.시작일시,
            dropoff?.시간창?.종료일시,
            pricing?.서비스레벨,
            pricing?.요청사항,
            pricing?.대기료,
            pricing?.수작업비,
            pricing?.할증,
            request.클라이언트요청Id,
            request.결제상태);
    }
}
