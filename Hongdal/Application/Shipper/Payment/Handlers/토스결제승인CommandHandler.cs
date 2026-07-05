using System.Text.Json;
using FluentResults;
using Hongdal.Application.Shipper.Payment.Events;
using Hongdal.Contracts.Shipper.Payment;
using 홍달.도메인.설정;
using 홍달.Services.Payments;
using 홍달.도메인.결제;

namespace Hongdal.Application.Shipper.Payment;

public sealed class 토스결제승인CommandHandler : IRequestHandler<토스결제승인Command, Result<토스결제승인응답>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HongdalContext _db;
    private readonly I공통결제Service _paymentService;
    private readonly I콘텐츠혜택계산Service _benefitService;

    public 토스결제승인CommandHandler(HongdalContext db, I공통결제Service paymentService, I콘텐츠혜택계산Service benefitService)
    {
        _db = db;
        _paymentService = paymentService;
        _benefitService = benefitService;
    }

    public async Task<Result<토스결제승인응답>> Handle(토스결제승인Command request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentKey))
        {
            return Result.Fail<토스결제승인응답>("paymentKey is required");
        }

        if (string.IsNullOrWhiteSpace(request.OrderId))
        {
            return Result.Fail<토스결제승인응답>("orderId is required");
        }

        if (request.Amount <= 0)
        {
            return Result.Fail<토스결제승인응답>("amount must be greater than 0");
        }

        var payment = await _db.결제.FirstOrDefaultAsync(x => x.OrderId == request.OrderId, cancellationToken);
        if (payment is null)
        {
            return Result.Fail<토스결제승인응답>("결제 요청을 찾을 수 없습니다.");
        }

        if (payment.결제금액 != request.Amount)
        {
            return Result.Fail<토스결제승인응답>("결제 금액이 일치하지 않습니다.");
        }

        if (payment.결제상태 == 상태값.결제상태.결제완료)
        {
            return Result.Ok(new 토스결제승인응답
            {
                결제Id = payment.결제Id,
                의뢰Id = payment.의뢰Id,
                OrderId = payment.OrderId,
                PaymentKey = payment.PaymentKey ?? string.Empty,
                결제상태 = payment.결제상태,
                결제응답 = payment.Toss응답Json ?? string.Empty
            });
        }

        var confirmResult = await _paymentService.결제승인Async(
            payment.결제제공자,
            new 결제승인요청(request.PaymentKey, request.OrderId, request.Amount),
            cancellationToken);

        if (!confirmResult.IsSuccess)
        {
            payment.공통결제상태 = 결제공통정의.결제상태.실패;
            payment.원본응답Json = confirmResult.ResponseJson;
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Fail<토스결제승인응답>(confirmResult.ResponseJson);
        }

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        payment.PaymentKey = request.PaymentKey;
        payment.외부거래번호 = confirmResult.ExternalTransactionNo;
        payment.결제수단 = string.IsNullOrWhiteSpace(confirmResult.PaymentMethod) ? payment.결제수단 : confirmResult.PaymentMethod;
        payment.결제상태 = 상태값.결제상태.결제완료;
        payment.공통결제상태 = 결제공통정의.결제상태.승인완료;
        payment.Toss응답Json = confirmResult.ResponseJson;
        payment.원본응답Json = confirmResult.ResponseJson;
        payment.승인일시 = DateTime.UtcNow;

        var eventPayload = new 결제승인완료Event(
            payment.Id,
            payment.결제Id,
            payment.결제대상유형,
            payment.대상Id,
            payment.결제제공자,
            payment.결제금액,
            payment.통화,
            payment.승인일시 ?? DateTime.UtcNow);

        _db.결제승인완료Outbox.Add(new 결제승인완료Outbox
        {
            결제레코드Id = payment.Id,
            결제Id = payment.결제Id,
            결제대상유형 = payment.결제대상유형,
            대상Id = payment.대상Id,
            결제제공자 = payment.결제제공자,
            결제금액 = payment.결제금액,
            통화 = payment.통화,
            승인일시Utc = payment.승인일시 ?? DateTime.UtcNow,
            PayloadJson = JsonSerializer.Serialize(eventPayload, JsonOptions),
            처리상태 = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _benefitService.보상사용처리Async(payment.화주Id, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return Result.Ok(new 토스결제승인응답
        {
            결제Id = payment.결제Id,
            의뢰Id = payment.의뢰Id,
            OrderId = payment.OrderId,
            PaymentKey = payment.PaymentKey ?? string.Empty,
            결제상태 = payment.결제상태,
            결제응답 = confirmResult.ResponseJson
        });
    }
    }
