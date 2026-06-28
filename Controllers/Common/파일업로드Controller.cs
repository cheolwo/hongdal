using Microsoft.AspNetCore.Mvc;
using 홍달.Services;
using Hongdal.Contracts.Common;

namespace Hongdal.Controllers.Common
{
    [ApiController]
    [Route("api/v1/files")]
    public class 파일업로드Controller : ControllerBase
    {
        private readonly IGoogleCloudStorageService _googleCloudStorageService;

        public 파일업로드Controller(IGoogleCloudStorageService googleCloudStorageService)
        {
            _googleCloudStorageService = googleCloudStorageService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> 업로드([FromForm] 파일업로드요청 request, CancellationToken cancellationToken)
        {
            if (request == null || request.File == null)
            {
                return BadRequest("file is required");
            }

            if (request.File.Length <= 0)
            {
                return BadRequest("empty file is not allowed");
            }

            await using var stream = request.File.OpenReadStream();
            var result = await _googleCloudStorageService.UploadAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.Folder,
                cancellationToken);

            return Ok(new 파일업로드응답
            {
                BucketName = result.BucketName,
                ObjectName = result.ObjectName,
                Url = result.PublicUrl
            });
        }
    }

}
