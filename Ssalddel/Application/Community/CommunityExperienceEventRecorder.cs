using Ssalddel.Services.Community;
using Microsoft.Extensions.Logging;

namespace Ssalddel.Application.Community;

public interface ICommunityExperienceEventRecorder
{
    Task RecordAsync(
        CommunityExperienceAwardRequest request,
        string 기록범위,
        CancellationToken cancellationToken);
}

public sealed class CommunityExperienceEventRecorder : ICommunityExperienceEventRecorder
{
    private readonly ICommunityExperienceAwardService _experienceAwardService;
    private readonly ILogger<CommunityExperienceEventRecorder> _logger;

    public CommunityExperienceEventRecorder(
        ICommunityExperienceAwardService experienceAwardService,
        ILogger<CommunityExperienceEventRecorder> logger)
    {
        _experienceAwardService = experienceAwardService;
        _logger = logger;
    }

    public async Task RecordAsync(
        CommunityExperienceAwardRequest request,
        string 기록범위,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _experienceAwardService.RecordAsync(request, cancellationToken);
            if (!result.처리됨)
            {
                _logger.LogDebug(
                    "{ExperienceScope} 경험치 적립 생략. EventCode={EventCode} SourceKind={SourceKind} SourceId={SourceId} Reason={Reason}",
                    기록범위,
                    request.EventCode,
                    request.SourceKind,
                    request.SourceId,
                    result.사유);
                return;
            }

            _logger.LogInformation(
                "{ExperienceScope} 경험치 적립 이벤트 기록. EventCode={EventCode} SourceKind={SourceKind} SourceId={SourceId} Experience={Experience}",
                기록범위,
                result.EventCode,
                request.SourceKind,
                request.SourceId,
                result.BaseExperience);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "{ExperienceScope} 경험치 적립 이벤트 처리 중 예외가 발생했습니다. EventCode={EventCode} SourceKind={SourceKind} SourceId={SourceId}",
                기록범위,
                request.EventCode,
                request.SourceKind,
                request.SourceId);
        }
    }
}
