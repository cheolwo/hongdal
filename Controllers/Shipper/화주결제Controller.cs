using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services;
using 홍달.도메인.공통;
using 홍달.도메인.결제;
using 홍달.도메인.배차;

namespace Hongdal.Controllers.Shipper
{
    [ApiController]
    [Route("api/v1/payments")]
    public class 화주결제Controller : ControllerBase
    {
        private readonly HongdalContext _db;
        private readonly ITossPaymentsService _tossPaymentsService;
        private readonly TossPaymentsOptions _options;

        public 화주결제Controller(HongdalContext db, ITossPaymentsService tossPaymentsService, IConfiguration configuration)
        {
            _db = db;
            _tossPaymentsService = tossPaymentsService;
            _options = configuration.GetSection(TossPaymentsOptions.SectionName).Get<TossPaymentsOptions>() ?? new TossPaymentsOptions();
        }

        [HttpGet]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 결제목록조회(
            [FromQuery] string? 결제상태,
            [FromQuery] string? 의뢰Id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var query = _db.결제.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(결제상태))
            {
                var status = 결제상태.Trim();
                query = query.Where(x => x.결제상태 == status);
            }

            if (!string.IsNullOrWhiteSpace(의뢰Id))
            {
                var requestId = 의뢰Id.Trim();
                query = query.Where(x => x.의뢰Id == requestId);
            }

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new 결제목록응답
                {
                    결제Id = x.결제Id,
                    의뢰Id = x.의뢰Id,
                    화주Id = x.화주Id,
                    결제금액 = x.결제금액,
                    결제수단 = x.결제수단,
                    결제상태 = x.결제상태,
                    OrderId = x.OrderId,
                    PaymentKey = x.PaymentKey,
                    Toss응답Json = x.Toss응답Json,
                    생성일시Utc = x.CreatedAt,
                    승인일시Utc = x.승인일시
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("toss/config")]
        [Authorize(Policy = "서버관리자전용")]
        public IActionResult 토스결제환경조회()
        {
            return Ok(new 토스결제환경응답
            {
                ClientKey = _options.ClientKey,
                BaseUrl = _options.BaseUrl,
                IsConfigured = !string.IsNullOrWhiteSpace(_options.ClientKey) && !string.IsNullOrWhiteSpace(_options.SecretKey)
            });
        }

        [HttpPost("toss/prepare")]
        public async Task<IActionResult> 토스결제준비([FromBody] 토스결제준비요청 request)
        {
            if (request == null) return BadRequest("request body is required");
            if (string.IsNullOrWhiteSpace(request.의뢰Id)) return BadRequest("의뢰Id is required");
            if (request.Amount <= 0) return BadRequest("amount must be greater than 0");

            var shipperRequest = await _db.화주운송의뢰.FirstOrDefaultAsync(x => x.의뢰Id == request.의뢰Id);
            if (shipperRequest == null)
            {
                return NotFound("의뢰를 찾을 수 없습니다.");
            }

            if (!string.Equals(shipperRequest.배차상태, 상태값.배차상태.상차완료, StringComparison.Ordinal))
            {
                return BadRequest("상차완료 이후에만 결제를 진행할 수 있습니다.");
            }

            if (shipperRequest.결제상태 == 상태값.결제상태.결제완료)
            {
                return BadRequest("이미 결제완료된 의뢰입니다.");
            }

            var requestedAmount = request.Amount > 0 ? request.Amount : shipperRequest.결제예정금액 ?? 0;
            if (requestedAmount <= 0)
            {
                return BadRequest("결제금액이 유효하지 않습니다.");
            }

            var existingPendingPayment = await _db.결제
                .Where(x => x.의뢰Id == shipperRequest.의뢰Id && x.결제상태 == 상태값.결제상태.결제대기 && x.결제금액 == request.Amount)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (existingPendingPayment != null)
            {
                return Ok(new 토스결제준비응답
                {
                    결제Id = existingPendingPayment.결제Id,
                    의뢰Id = existingPendingPayment.의뢰Id,
                    OrderId = existingPendingPayment.OrderId,
                    Amount = existingPendingPayment.결제금액,
                    ClientKey = _options.ClientKey
                });
            }

            var orderId = 주문번호생성();
            var payment = new 결제
            {
                결제Id = Guid.NewGuid().ToString("N"),
                의뢰Id = shipperRequest.의뢰Id,
                화주Id = shipperRequest.화주Id,
                결제금액 = requestedAmount,
                결제수단 = shipperRequest.결제수단,
                OrderId = orderId,
                결제상태 = 상태값.결제상태.결제대기,
                CreatedAt = DateTime.UtcNow
            };

            shipperRequest.결제상태 = 상태값.결제상태.결제대기;
            shipperRequest.UpdatedAt = DateTime.UtcNow;

            await _db.결제.AddAsync(payment);
            await _db.SaveChangesAsync();

            return Ok(new 토스결제준비응답
            {
                결제Id = payment.결제Id,
                의뢰Id = payment.의뢰Id,
                OrderId = payment.OrderId,
                Amount = payment.결제금액,
                ClientKey = _options.ClientKey
            });
        }

        [HttpPost("toss/confirm")]
        public async Task<IActionResult> 토스결제승인([FromBody] 토스결제승인요청 request)
        {
            if (request == null) return BadRequest("request body is required");
            if (string.IsNullOrWhiteSpace(request.PaymentKey)) return BadRequest("paymentKey is required");
            if (string.IsNullOrWhiteSpace(request.OrderId)) return BadRequest("orderId is required");
            if (request.Amount <= 0) return BadRequest("amount must be greater than 0");

            var payment = await _db.결제.FirstOrDefaultAsync(x => x.OrderId == request.OrderId);
            if (payment is null)
            {
                return NotFound("결제 요청을 찾을 수 없습니다.");
            }

            if (payment.결제금액 != request.Amount)
            {
                return BadRequest("결제 금액이 일치하지 않습니다.");
            }

            if (payment.결제상태 == 상태값.결제상태.결제완료)
            {
                return Ok(new 토스결제승인응답
                {
                    결제Id = payment.결제Id,
                    의뢰Id = payment.의뢰Id,
                    OrderId = payment.OrderId,
                    PaymentKey = payment.PaymentKey ?? string.Empty,
                    결제상태 = payment.결제상태,
                    결제응답 = payment.Toss응답Json ?? string.Empty
                });
            }

            var confirmResult = await _tossPaymentsService.ConfirmAsync(new TossConfirmApiRequest(
                request.PaymentKey,
                request.OrderId,
                request.Amount));

            if (!confirmResult.IsSuccess)
            {
                return BadRequest(confirmResult.ResponseJson);
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            payment.PaymentKey = request.PaymentKey;
            payment.결제수단 = string.IsNullOrWhiteSpace(confirmResult.PaymentMethod) ? payment.결제수단 : confirmResult.PaymentMethod;
            payment.결제상태 = 상태값.결제상태.결제완료;
            payment.Toss응답Json = confirmResult.ResponseJson;
            payment.승인일시 = DateTime.UtcNow;

            var shipperRequest = await _db.화주운송의뢰.FirstOrDefaultAsync(x => x.의뢰Id == payment.의뢰Id);
            if (shipperRequest == null)
            {
                return NotFound("의뢰를 찾을 수 없습니다.");
            }

            shipperRequest.결제상태 = 상태값.결제상태.결제완료;
            shipperRequest.배차상태 = 상태값.배차상태.매칭중;
            shipperRequest.UpdatedAt = DateTime.UtcNow;

            var existingQueue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == shipperRequest.의뢰Id);
            if (existingQueue == null)
            {
                _db.배차대기.Add(new 배차대기
                {
                    의뢰Id = shipperRequest.의뢰Id,
                    화주Id = shipperRequest.화주Id,
                    픽업_도로명주소 = shipperRequest.픽업_도로명주소,
                    픽업_상세주소 = shipperRequest.픽업_상세주소,
                    픽업_위도 = shipperRequest.픽업_위도,
                    픽업_경도 = shipperRequest.픽업_경도,
                    하차_도로명주소 = shipperRequest.하차_도로명주소,
                    하차_상세주소 = shipperRequest.하차_상세주소,
                    하차_위도 = shipperRequest.하차_위도,
                    하차_경도 = shipperRequest.하차_경도,
                    상태 = 상태값.배차대기상태.대기,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new 토스결제승인응답
            {
                결제Id = payment.결제Id,
                의뢰Id = payment.의뢰Id,
                OrderId = payment.OrderId,
                PaymentKey = payment.PaymentKey ?? string.Empty,
                결제상태 = payment.결제상태,
                결제응답 = confirmResult.ResponseJson
            });
        }

        private static string 주문번호생성()
        {
            return $"hongdal_{Guid.NewGuid():N}";
        }
    }

    public sealed class 결제목록응답
    {
        public string 결제Id { get; set; } = string.Empty;
        public string 의뢰Id { get; set; } = string.Empty;
        public string 화주Id { get; set; } = string.Empty;
        public int 결제금액 { get; set; }
        public string 결제수단 { get; set; } = string.Empty;
        public string 결제상태 { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string? PaymentKey { get; set; }
        public string? Toss응답Json { get; set; }
        public DateTime 생성일시Utc { get; set; }
        public DateTime? 승인일시Utc { get; set; }
    }

    public sealed class 토스결제준비요청
    {
        public string 의뢰Id { get; set; } = string.Empty;
        public int Amount { get; set; }
    }

    public sealed class 토스결제준비응답
    {
        public string 결제Id { get; set; } = string.Empty;
        public string 의뢰Id { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string ClientKey { get; set; } = string.Empty;
    }

    public sealed class 토스결제환경응답
    {
        public string ClientKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public bool IsConfigured { get; set; }
    }

    public sealed class 토스결제승인요청
    {
        public string PaymentKey { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public int Amount { get; set; }
    }

    public sealed class 토스결제승인응답
    {
        public string 결제Id { get; set; } = string.Empty;
        public string 의뢰Id { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string PaymentKey { get; set; } = string.Empty;
        public string 결제상태 { get; set; } = string.Empty;
        public string 결제응답 { get; set; } = string.Empty;
    }
}
