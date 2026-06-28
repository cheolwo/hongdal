using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hongdal.Contracts.Shipper.Request;
using 홍달.Data;
using 홍달.Services;
using 홍달.도메인.공통;
using 홍달.도메인.화주;
using 홍달.도메인.화물;

namespace Hongdal.Controllers.Shipper.Request01
{
    [ApiController]
    [Route("api/v1/shipper/requests")]
    [Authorize]
    public class 화주운송의뢰Controller : ControllerBase
    {
        private static readonly string[] AllowedPaymentStatuses = 상태값.결제상태.허용값;
        private readonly HongdalContext _db;
        private readonly IGeocodingService _geocodingService;

        public 화주운송의뢰Controller(HongdalContext db, IGeocodingService geocodingService)
        {
            _db = db;
            _geocodingService = geocodingService;
        }

        [HttpGet]
        public async Task<IActionResult> 의뢰목록조회(
            [FromQuery] string? shipperId,
            [FromQuery] string? status,
            [FromQuery] string? paymentStatus,
            [FromQuery] string? dispatchStatus,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var query = _db.화주운송의뢰.AsQueryable();
            if (!string.IsNullOrWhiteSpace(shipperId)) query = query.Where(r => r.화주Id == shipperId);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(r => r.상태 == status);
            if (!string.IsNullOrWhiteSpace(paymentStatus)) query = query.Where(r => r.결제상태 == paymentStatus);
            if (!string.IsNullOrWhiteSpace(dispatchStatus)) query = query.Where(r => r.배차상태 == dispatchStatus);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => 응답변환(r))
                .ToListAsync();

            return Ok(items);
        }

        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<IActionResult> 공개화물요약조회(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var items = await _db.화주운송의뢰
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new 공개화물요약응답
                {
                    의뢰Id = r.의뢰Id,
                    화물종류 = r.화물종류,
                    화물수량 = r.화물수량,
                    화물중량Kg = r.화물중량Kg,
                    운송방식 = r.운송방식,
                    차량종류 = r.차량종류,
                    의뢰상태 = r.상태,
                    배차상태 = r.배차상태,
                    생성일시 = r.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> 의뢰생성([FromBody] 화주운송의뢰생성요청 req)
        {
            if (req == null) return BadRequest("request body is required");
            if (string.IsNullOrWhiteSpace(req.화물.화물종류)) return BadRequest("화물종류 is required");
            if (req.픽업 == null) return BadRequest("픽업 정보 is required");
            if (string.IsNullOrWhiteSpace(req.픽업.주소.도로명주소)) return BadRequest("픽업 주소 도로명주소 is required");
            if (string.IsNullOrWhiteSpace(req.픽업.연락처.전화번호)) return BadRequest("픽업 연락처 전화번호 is required");
            if (req.픽업.시간창 == null) return BadRequest("픽업 시간창 is required");
            if (req.픽업.시간창.시작일시 >= req.픽업.시간창.종료일시) return BadRequest("pickup.window.startAt must be before endAt");
            if (!string.IsNullOrWhiteSpace(req.클라이언트요청Id) && string.IsNullOrWhiteSpace(req.화주Id)) return BadRequest("화주Id is required when clientRequestId is provided");

            var clientRequestId = req.클라이언트요청Id?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(clientRequestId))
            {
                var duplicate = await _db.화주운송의뢰
                    .FirstOrDefaultAsync(r => r.화주Id == req.화주Id && r.클라이언트요청Id == clientRequestId);
                if (duplicate != null)
                {
                    return Conflict(new { code = "DUPLICATE_REQUEST", message = "동일한 클라이언트요청Id로 이미 생성된 의뢰가 있습니다." });
                }
            }

            var paymentStatus = string.IsNullOrWhiteSpace(req.결제상태) ? 상태값.결제상태.결제대기 : req.결제상태.Trim();
            if (!AllowedPaymentStatuses.Contains(paymentStatus))
            {
                return BadRequest($"결제상태 must be one of: {string.Join(", ", AllowedPaymentStatuses)}");
            }

            var (pickupLat, pickupLng) = await 좌표해결(req.픽업.주소.도로명주소, req.픽업.주소.상세주소);
            var (dropoffLat, dropoffLng) = await 좌표해결(req.하차?.주소?.도로명주소, req.하차?.주소?.상세주소);

            var entity = new 화주운송의뢰
            {
                의뢰Id = Guid.NewGuid().ToString(),
                화주Id = req.화주Id ?? string.Empty,
                화물종류 = req.화물.화물종류,
                화물설명 = req.화물.설명 ?? string.Empty,
                화물수량 = req.화물.수량,
                화물중량Kg = req.화물.중량Kg,
                화물부피Cbm = req.화물.부피Cbm,
                화물파손주의여부 = req.화물.화물파손주의여부,
                화물온도조건 = req.화물.온도조건 ?? "상온",
                운송방식 = req.운송방식 ?? "혼적",
                차량종류 = req.차량종류 ?? string.Empty,
                결제수단 = req.결제수단 ?? "카드",
                결제예정금액 = req.결제예정금액,
                픽업_도로명주소 = req.픽업.주소.도로명주소,
                픽업_상세주소 = req.픽업.주소.상세주소 ?? string.Empty,
                픽업_위도 = pickupLat,
                픽업_경도 = pickupLng,
                픽업_연락처_이름 = req.픽업.연락처.이름,
                픽업_연락처_전화번호 = req.픽업.연락처.전화번호,
                픽업_시간창_시작일시 = req.픽업.시간창.시작일시,
                픽업_시간창_종료일시 = req.픽업.시간창.종료일시,
                하차_도로명주소 = req.하차?.주소?.도로명주소 ?? string.Empty,
                하차_상세주소 = req.하차?.주소?.상세주소 ?? string.Empty,
                하차_위도 = dropoffLat,
                하차_경도 = dropoffLng,
                하차_연락처_이름 = req.하차?.연락처?.이름 ?? string.Empty,
                하차_연락처_전화번호 = req.하차?.연락처?.전화번호 ?? string.Empty,
                하차_시간창_시작일시 = req.하차?.시간창?.시작일시,
                하차_시간창_종료일시 = req.하차?.시간창?.종료일시,
                서비스레벨 = req.요금옵션?.서비스레벨 ?? string.Empty,
                요청사항 = req.요금옵션?.요청사항 ?? string.Empty,
                대기료 = req.요금옵션?.대기료,
                수작업비 = req.요금옵션?.수작업비,
                할증 = req.요금옵션?.할증,
                클라이언트요청Id = clientRequestId,
                상태 = 상태값.의뢰상태.생성됨,
                결제상태 = paymentStatus,
                배차상태 = 상태값.배차상태.미시작,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.AddAsync(entity);
            await UpsertCargoRequirementAsync(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(의뢰단건조회), new { requestId = entity.의뢰Id }, new 화주운송의뢰응답
            {
                의뢰Id = entity.의뢰Id,
                의뢰상태 = entity.상태,
                결제상태 = entity.결제상태,
                배차상태 = entity.배차상태,
                생성일시 = entity.CreatedAt,
                요약 = new 화주운송의뢰응답.요약DTO
                {
                    화물종류 = entity.화물종류,
                    픽업지 = entity.픽업_도로명주소,
                    하차지 = entity.하차_도로명주소
                }
            });
        }

        [HttpGet("{requestId}")]
        public async Task<IActionResult> 의뢰단건조회(string requestId)
        {
            var entity = await _db.화주운송의뢰.FirstOrDefaultAsync(r => r.의뢰Id == requestId);
            if (entity == null) return NotFound();
            return Ok(응답변환(entity));
        }

        [HttpPut("{requestId}")]
        public async Task<IActionResult> 의뢰수정(string requestId, [FromBody] 화주운송의뢰수정요청 req)
        {
            if (req == null) return BadRequest("request body is required");

            var entity = await _db.화주운송의뢰.FirstOrDefaultAsync(r => r.의뢰Id == requestId);
            if (entity == null) return NotFound();

            var updated = false;

            if (req.화물 != null)
            {
                if (!string.IsNullOrWhiteSpace(req.화물.화물종류)) entity.화물종류 = req.화물.화물종류;
                if (req.화물.설명 != null) entity.화물설명 = req.화물.설명;
                if (req.화물.수량.HasValue) entity.화물수량 = req.화물.수량;
                if (req.화물.중량Kg.HasValue) entity.화물중량Kg = req.화물.중량Kg;
                if (req.화물.부피Cbm.HasValue) entity.화물부피Cbm = req.화물.부피Cbm;
                entity.화물파손주의여부 = req.화물.화물파손주의여부;
                if (req.화물.온도조건 != null) entity.화물온도조건 = req.화물.온도조건;
                updated = true;
            }

            if (req.픽업 != null)
            {
                if (req.픽업.주소 != null)
                {
                    if (!string.IsNullOrWhiteSpace(req.픽업.주소.도로명주소)) entity.픽업_도로명주소 = req.픽업.주소.도로명주소;
                    if (req.픽업.주소.상세주소 != null) entity.픽업_상세주소 = req.픽업.주소.상세주소;
                    if (req.픽업.주소.위도.HasValue) entity.픽업_위도 = req.픽업.주소.위도;
                    if (req.픽업.주소.경도.HasValue) entity.픽업_경도 = req.픽업.주소.경도;
                }
                if (req.픽업.연락처 != null)
                {
                    if (!string.IsNullOrWhiteSpace(req.픽업.연락처.이름)) entity.픽업_연락처_이름 = req.픽업.연락처.이름;
                    if (!string.IsNullOrWhiteSpace(req.픽업.연락처.전화번호)) entity.픽업_연락처_전화번호 = req.픽업.연락처.전화번호;
                }
                if (req.픽업.시간창 != null)
                {
                    if (req.픽업.시간창.시작일시 >= req.픽업.시간창.종료일시) return BadRequest("pickup.window.startAt must be before endAt");
                    entity.픽업_시간창_시작일시 = req.픽업.시간창.시작일시;
                    entity.픽업_시간창_종료일시 = req.픽업.시간창.종료일시;
                }
                updated = true;
            }

            if (req.하차 != null)
            {
                if (req.하차.주소 != null)
                {
                    if (!string.IsNullOrWhiteSpace(req.하차.주소.도로명주소)) entity.하차_도로명주소 = req.하차.주소.도로명주소;
                    if (req.하차.주소.상세주소 != null) entity.하차_상세주소 = req.하차.주소.상세주소;
                    if (req.하차.주소.위도.HasValue) entity.하차_위도 = req.하차.주소.위도;
                    if (req.하차.주소.경도.HasValue) entity.하차_경도 = req.하차.주소.경도;
                }
                if (req.하차.연락처 != null)
                {
                    if (!string.IsNullOrWhiteSpace(req.하차.연락처.이름)) entity.하차_연락처_이름 = req.하차.연락처.이름;
                    if (!string.IsNullOrWhiteSpace(req.하차.연락처.전화번호)) entity.하차_연락처_전화번호 = req.하차.연락처.전화번호;
                }
                if (req.하차.시간창 != null)
                {
                    if (req.하차.시간창.시작일시 >= req.하차.시간창.종료일시) return BadRequest("dropoff.window.startAt must be before endAt");
                    entity.하차_시간창_시작일시 = req.하차.시간창.시작일시;
                    entity.하차_시간창_종료일시 = req.하차.시간창.종료일시;
                }
                updated = true;
            }

            if (req.요금옵션 != null)
            {
                if (req.요금옵션.서비스레벨 != null) entity.서비스레벨 = req.요금옵션.서비스레벨;
                if (req.요금옵션.요청사항 != null) entity.요청사항 = req.요금옵션.요청사항;
                if (req.요금옵션.대기료.HasValue) entity.대기료 = req.요금옵션.대기료;
                if (req.요금옵션.수작업비.HasValue) entity.수작업비 = req.요금옵션.수작업비;
                if (req.요금옵션.할증.HasValue) entity.할증 = req.요금옵션.할증;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(req.결제상태))
            {
                var paymentStatus = req.결제상태.Trim();
                if (!AllowedPaymentStatuses.Contains(paymentStatus))
                {
                    return BadRequest($"결제상태 must be one of: {string.Join(", ", AllowedPaymentStatuses)}");
                }
                entity.결제상태 = paymentStatus;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(req.운송방식))
            {
                entity.운송방식 = req.운송방식;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(req.상태))
            {
                entity.상태 = req.상태;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(req.배차상태))
            {
                entity.배차상태 = req.배차상태;
                updated = true;
            }

            if (!updated) return BadRequest("수정할 필드를 하나 이상 제공해야 합니다.");

            entity.UpdatedAt = DateTime.UtcNow;
            await UpsertCargoRequirementAsync(entity);
            await _db.SaveChangesAsync();

            return Ok(응답변환(entity));
        }

        [HttpDelete("{requestId}")]
        public async Task<IActionResult> 의뢰삭제(string requestId)
        {
            var entity = await _db.화주운송의뢰.FirstOrDefaultAsync(r => r.의뢰Id == requestId);
            if (entity == null) return NotFound();

            _db.화주운송의뢰.Remove(entity);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private async Task<(decimal? lat, decimal? lng)> 좌표해결(string? roadAddress, string? detailAddress)
        {
            if (string.IsNullOrWhiteSpace(roadAddress)) return (null, null);

            var query = roadAddress;
            if (!string.IsNullOrWhiteSpace(detailAddress)) query += " " + detailAddress;

            var location = await _geocodingService.GeocodeAsync(query);
            if (!location.HasValue) return (null, null);
            return (location.Value.lat, location.Value.lng);
        }

        private static 화주운송의뢰응답 응답변환(화주운송의뢰 entity)
        {
            return new 화주운송의뢰응답
            {
                의뢰Id = entity.의뢰Id,
                의뢰상태 = entity.상태,
                결제상태 = entity.결제상태,
                배차상태 = entity.배차상태,
                운송방식 = entity.운송방식,
                차량종류 = entity.차량종류,
                결제수단 = entity.결제수단,
                결제예정금액 = entity.결제예정금액,
                생성일시 = entity.CreatedAt,
                픽업지 = entity.픽업_도로명주소,
                픽업상세지 = entity.픽업_상세주소,
                픽업위도 = entity.픽업_위도,
                픽업경도 = entity.픽업_경도,
                하차지 = entity.하차_도로명주소,
                하차상세지 = entity.하차_상세주소,
                하차위도 = entity.하차_위도,
                하차경도 = entity.하차_경도,
                대기료 = entity.대기료,
                수작업비 = entity.수작업비,
                할증 = entity.할증,
                최종운임 = entity.최종운임,
                요약 = new 화주운송의뢰응답.요약DTO
                {
                    화물종류 = entity.화물종류,
                    픽업지 = entity.픽업_도로명주소,
                    하차지 = entity.하차_도로명주소
                }
            };
        }

        private async Task UpsertCargoRequirementAsync(화주운송의뢰 entity)
        {
            var cargo = await _db.화물요구조건.FirstOrDefaultAsync(x => x.의뢰Id == entity.의뢰Id);
            if (cargo == null)
            {
                cargo = new 화물요구조건 { 의뢰Id = entity.의뢰Id };
                _db.화물요구조건.Add(cargo);
            }

            var mergedText = string.Join(' ', new[] { entity.운송방식, entity.서비스레벨, entity.요청사항, entity.화물종류, entity.화물설명 }
                .Where(x => !string.IsNullOrWhiteSpace(x))!);

            cargo.화물무게Kg = entity.화물중량Kg.HasValue ? (int?)Math.Ceiling(entity.화물중량Kg.Value) : null;
            cargo.화물길이Mm = entity.화물길이Mm;
            cargo.화물폭Mm = entity.화물폭Mm;
            cargo.화물높이Mm = entity.화물높이Mm;
            cargo.팔레트개수 = entity.화물팔레트개수;
            cargo.비맞으면안됨 = mergedText.Contains("비", StringComparison.OrdinalIgnoreCase) || mergedText.Contains("방수", StringComparison.OrdinalIgnoreCase);
            cargo.냉장필요 = string.Equals(entity.화물온도조건, "냉장", StringComparison.OrdinalIgnoreCase);
            cargo.냉동필요 = string.Equals(entity.화물온도조건, "냉동", StringComparison.OrdinalIgnoreCase);
            cargo.리프트필요 = mergedText.Contains("리프트", StringComparison.OrdinalIgnoreCase);
            cargo.측면상하차필요 = mergedText.Contains("측면", StringComparison.OrdinalIgnoreCase);
            cargo.장재물 = mergedText.Contains("장재물", StringComparison.OrdinalIgnoreCase);
            cargo.혼적허용 = !mergedText.Contains("단독", StringComparison.OrdinalIgnoreCase);
            cargo.독차필수 = mergedText.Contains("단독", StringComparison.OrdinalIgnoreCase);
            cargo.주의사항 = entity.화물설명;
            cargo.UpdatedAt = DateTime.UtcNow;
            cargo.CreatedAt = cargo.CreatedAt == default ? DateTime.UtcNow : cargo.CreatedAt;
            entity.화물길이Mm = cargo.화물길이Mm;
            entity.화물폭Mm = cargo.화물폭Mm;
            entity.화물높이Mm = cargo.화물높이Mm;
            entity.화물팔레트개수 = cargo.팔레트개수;
        }
    }

}
