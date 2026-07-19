using Ssalddel.Contracts.CommonContents;

namespace SsalddelApp.Services.CommonContents;

public interface I화주공통콘텐츠Service
{
    Task<살뜰위젯콘텐츠Dto?> 혜택콘텐츠조회Async(CancellationToken cancellationToken = default);
    Task<살뜰위젯콘텐츠Dto?> 공지콘텐츠조회Async(CancellationToken cancellationToken = default);
}