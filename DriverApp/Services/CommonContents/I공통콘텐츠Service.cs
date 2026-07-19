using Ssalddel.Contracts.CommonContents;

namespace DriverApp.Services.CommonContents;

public interface I공통콘텐츠Service
{
    Task<살뜰위젯콘텐츠Dto?> 위젯콘텐츠조회Async(string 위치, CancellationToken cancellationToken = default);
    Task<long?> 시청시작Async(long 콘텐츠Id, int 영상전체초, CancellationToken cancellationToken = default);
    Task 시청진행저장Async(long 세션Id, int 현재시청초, CancellationToken cancellationToken = default);
    Task<콘텐츠시청완료Result?> 시청완료Async(long 세션Id, CancellationToken cancellationToken = default);
    Task<살뜰위젯콘텐츠Dto?> 위젯콘텐츠동기화Async(string 위치, CancellationToken cancellationToken = default);
}