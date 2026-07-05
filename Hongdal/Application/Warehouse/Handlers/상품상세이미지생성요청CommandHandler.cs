using FluentResults;
using MediatR;
using System.Text.Json;
using Hongdal.Application.CommandProcessing;
using 홍달.도메인.판매;
using 홍달.Services.Images;

namespace Hongdal.Application.Warehouse;

public sealed class 상품상세이미지생성요청CommandHandler : IRequestHandler<상품상세이미지생성요청Command, Result<long>>
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I참여자실행권한검사 _권한검사;
    private readonly I샘플이미지생성Service _이미지생성Service;

    public 상품상세이미지생성요청CommandHandler(
        HongdalContext db,
        ICurrentUserAccessor currentUserAccessor,
        I참여자실행권한검사 권한검사,
        I샘플이미지생성Service 이미지생성Service)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
        _권한검사 = 권한검사;
        _이미지생성Service = 이미지생성Service;
    }

    public async Task<Result<long>> Handle(상품상세이미지생성요청Command request, CancellationToken cancellationToken)
    {
        if (!_권한검사.Try검증(
                _currentUserAccessor.UserId,
                _currentUserAccessor.Role,
                request.참여자Id,
                request.실행역할,
                out var 오류메시지))
        {
            return Result.Fail<long>(오류메시지 ?? "상품 상세 이미지 생성 요청 권한이 없습니다.");
        }

        var 사용가능자산 = await _db.상품물류자산
            .Where(x => x.상품Id == request.상품Id)
            .Where(x => x.상세이미지사용가능여부)
            .Where(x => x.자산유형 == 상품자산유형.검품사진
                        || x.자산유형 == 상품자산유형.포장사진
                        || x.자산유형 == 상품자산유형.라벨사진
                        || x.자산유형 == 상품자산유형.실측사진
                        || x.자산유형 == 상품자산유형.구성품사진
                        || x.자산유형 == 상품자산유형.상세이미지생성원본)
            .OrderBy(x => x.등록시각)
            .Take(8)
            .ToListAsync(cancellationToken);

        if (사용가능자산.Count == 0)
        {
            return Result.Fail<long>("상세이미지 생성에 사용할 물류자산이 없습니다.");
        }

        var now = DateTimeOffset.UtcNow;
        var sourceRefs = 사용가능자산.Select(x => new
        {
            x.Id,
            자산유형 = x.자산유형.ToString(),
            x.파일Url,
            x.설명,
            등록시각 = x.등록시각
        }).ToList();

        var 참조Json = JsonSerializer.Serialize(sourceRefs);

        var prompt = BuildPrompt(사용가능자산);

        var task = new 상품상세이미지생성작업
        {
            상품Id = request.상품Id,
            주문Id = request.주문Id,
            통관절차Id = request.통관절차Id,
            요청자Id = request.요청자Id,
            상태 = 상세이미지생성상태.프롬프트생성완료,
            생성프롬프트 = prompt,
            원본자산참조Json = 참조Json,
            생성시각 = now
        };

        _db.상품상세이미지생성작업.Add(task);
        await _db.SaveChangesAsync(cancellationToken);

        var 생성요청 = new 이미지생성요청
        {
            이미지용도 = 홍달.도메인.공통.생성이미지용도.상품상세페이지생성이미지,
            대상타입 = 상품상세이미지생성작업대상Resolver.대상타입값,
            대상식별자 = task.Id.ToString(),
            제목 = $"상품 {request.상품Id} 상세이미지",
            설명 = prompt,
            추가맥락 = 참조Json,
            종횡비 = "1:1",
            해상도 = "1K",
            샘플데이터여부 = false
        };

        var imageJob = await _이미지생성Service.생성요청Async(생성요청, cancellationToken);

        task.관련생성이미지작업Id = imageJob.Id;
        task.상태 = 상세이미지생성상태.이미지생성요청중;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(task.Id);
    }

    private static string BuildPrompt(IReadOnlyList<상품물류자산> assets)
    {
        var assetLines = assets
            .Select(x => $"- {x.자산유형}: {x.파일Url}")
            .ToArray();

        var context = string.Join("\n", assetLines);

        return $"""
Create Korean e-commerce product detail images based on verified logistics assets.
Preserve actual product identity, shape, labels, package condition and included components.
Do not invent unverified features or fake certifications.
Reference assets:
{context}
""";
    }
}
