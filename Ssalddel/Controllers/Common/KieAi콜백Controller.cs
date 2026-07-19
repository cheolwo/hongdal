using System.Text.Json;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Images;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[ApiController]
[Route("api/v1/kie-ai")]
public class KieAi콜백Controller : ControllerBase
{
    private readonly IKieAi콜백UseCase _useCase;

    public KieAi콜백Controller(IKieAi콜백UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost("callback")]
    public async Task<IActionResult> 콜백([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        var result = await _useCase.처리Async(payload, cancellationToken);
        return result.IsSuccess
            ? Ok(new { accepted = result.Value.Accepted, processed = result.Value.Processed })
            : this.ToActionResult(result);
    }
}
