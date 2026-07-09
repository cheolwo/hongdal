using Hongdal.Contracts.CommonContents;

namespace HongdalAdmin.Services;

public sealed partial class 백오피스메모리Service
{
    public Task<IReadOnlyList<관리자공통콘텐츠요약응답>> 공통콘텐츠목록조회Async(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<관리자공통콘텐츠요약응답>>(_commonContents.OrderByDescending(x => x.Id).ToArray());

    public Task<관리자공통콘텐츠상세응답?> 공통콘텐츠상세조회Async(long id, CancellationToken cancellationToken = default)
    {
        _commonContentDetails.TryGetValue(id, out var detail);
        return Task.FromResult(detail);
    }

    public Task<관리자공통콘텐츠상세응답?> 공통콘텐츠등록Async(관리자공통콘텐츠저장요청 request, CancellationToken cancellationToken = default)
    {
        var nextId = _commonContents.Count == 0 ? 1 : _commonContents.Max(x => x.Id) + 1;
        var detail = BuildDetail(nextId, request);
        _commonContentDetails[nextId] = detail;
        UpsertSummary(detail);
        return Task.FromResult<관리자공통콘텐츠상세응답?>(detail);
    }

    public Task<관리자공통콘텐츠상세응답?> 공통콘텐츠수정Async(long id, 관리자공통콘텐츠저장요청 request, CancellationToken cancellationToken = default)
    {
        if (!_commonContentDetails.TryGetValue(id, out var existing))
        {
            return Task.FromResult<관리자공통콘텐츠상세응답?>(null);
        }

        var updated = BuildDetail(id, request, existing.생성시각);
        _commonContentDetails[id] = updated;
        UpsertSummary(updated);
        return Task.FromResult<관리자공통콘텐츠상세응답?>(updated);
    }

    public Task 공통콘텐츠활성화변경Async(long id, bool enabled, CancellationToken cancellationToken = default)
    {
        if (_commonContentDetails.TryGetValue(id, out var detail))
        {
            detail.활성화여부 = enabled;
            UpsertSummary(detail);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<공통콘텐츠보상정책Dto>> 공통콘텐츠보상정책목록조회Async(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<공통콘텐츠보상정책Dto>>(_commonContentRewardPolicies.OrderByDescending(x => x.Id).ToArray());

    public Task<공통콘텐츠보상정책Dto?> 공통콘텐츠보상정책등록Async(공통콘텐츠보상정책Dto request, CancellationToken cancellationToken = default)
    {
        var nextId = _commonContentRewardPolicies.Count == 0 ? 1 : _commonContentRewardPolicies.Max(x => x.Id) + 1;
        request.Id = nextId;
        _commonContentRewardPolicies.RemoveAll(x => x.Id == request.Id);
        _commonContentRewardPolicies.Add(request);
        return Task.FromResult<공통콘텐츠보상정책Dto?>(request);
    }

    private 관리자공통콘텐츠상세응답 BuildDetail(long id, 관리자공통콘텐츠저장요청 request, DateTimeOffset? createdAt = null)
    {
        공통콘텐츠보상정책Dto? policy = null;
        if (request.보상정책Id.HasValue)
        {
            policy = _commonContentRewardPolicies.FirstOrDefault(x => x.Id == request.보상정책Id.Value);
        }

        return new 관리자공통콘텐츠상세응답
        {
            Id = id,
            제목 = request.제목,
            설명 = request.설명,
            콘텐츠유형 = request.콘텐츠유형,
            이미지Url = request.이미지Url,
            영상Url = request.영상Url,
            외부링크Url = request.외부링크Url,
            노출위치 = request.노출위치,
            기사노출 = request.기사노출,
            화주노출 = request.화주노출,
            운영자노출 = request.운영자노출,
            활성화여부 = request.활성화여부,
            노출시작시각 = request.노출시작시각,
            노출종료시각 = request.노출종료시각,
            보상정책 = policy,
            생성시각 = createdAt ?? DateTimeOffset.UtcNow
        };
    }

    private void UpsertSummary(관리자공통콘텐츠상세응답 detail)
    {
        _commonContents.RemoveAll(x => x.Id == detail.Id);
        _commonContents.Add(new 관리자공통콘텐츠요약응답
        {
            Id = detail.Id,
            제목 = detail.제목,
            콘텐츠유형 = detail.콘텐츠유형,
            노출위치 = detail.노출위치,
            활성화여부 = detail.활성화여부,
            생성시각 = detail.생성시각
        });
    }
}
