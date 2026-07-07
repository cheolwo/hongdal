using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MediatR;
using Hongdal.Controllers;
using Hongdal.Application.Driver.Transport;
using 홍달.도메인.공통;
using Hongdal.Contracts.Driver.Transport;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Driver.Progress05
{
    [HongdalApiVersion(HongdalProductVersion.V1_0)]
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/transports")]
    public sealed class 기사운송진행Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 기사운송진행Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 목록조회()
        {
            var driverId = 현재기사Id();
            var items = await _sender.Send(new 운송목록조회Query(driverId));

            return Ok(items);
        }

        [HttpGet("current")]
        public async Task<IActionResult> 현재조회()
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 운송현재조회Query(driverId));

            if (result is null) return this.ToNotFoundProblem("현재 운송 정보를 찾을 수 없습니다.");
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 상세조회(long id)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 운송상세조회Query(driverId, id));

            if (result is null) return this.ToNotFoundProblem("운송 정보를 찾을 수 없습니다.");
            return Ok(result);
        }

        [HttpPost("{id:long}/arrive-pickup")]
        public async Task<IActionResult> 상차지도착(long id)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 운송상차지도착Command(driverId, id));
            return this.ToActionResult(result);
        }

        [HttpPost("{id:long}/pickup-complete")]
        public async Task<IActionResult> 상차완료(long id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] 기사운송상차완료요청? request)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 운송상차완료Command(driverId, id)
            {
                상차사진ObjectName = request?.상차사진ObjectName,
                상차사진Url = request?.상차사진Url,
                인수증증빙방식 = request?.인수증증빙방식,
                인수자명 = request?.인수자명,
                인수자소속 = request?.인수자소속,
                인수자서명 = request?.인수자서명,
                기사서명 = request?.기사서명,
                인수증확인완료 = request?.인수증확인완료 ?? false,
                인수증서명생략확인 = request?.인수증서명생략확인 ?? false,
                인수증서명생략사유 = request?.인수증서명생략사유
            });
            return this.ToActionResult(result);
        }

        [HttpPost("{id:long}/arrive-dropoff")]
        public async Task<IActionResult> 하차지도착(long id)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 운송하차지도착Command(driverId, id));
            return this.ToActionResult(result);
        }

        [HttpPost("{id:long}/complete")]
        public async Task<IActionResult> 완료(long id)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 운송인수완료Command(driverId, id));
            return this.ToActionResult(result);
        }

        [HttpPost("{id:long}/report-issue")]
        public async Task<IActionResult> 문제신고(long id, [FromBody] 기사운송문제신고요청 request)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 운송문제신고Command(driverId, id, request.사유, request.메모));

            return this.ToActionResult(result);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
