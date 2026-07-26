using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;

namespace Ssalddel.Services.Orderer;

/// <summary>
/// 공동수입 준비 업무가 앞 단계의 공동구매 집단을 읽기 위한 Port입니다.
/// 같은 서버에서는 Mongo 저장소 Adapter를 사용하고, 서버가 분리되면 HTTP나 메시지 기반 Adapter로 교체할 수 있습니다.
/// </summary>
public interface I공동수입준비SourceGroupReader
{
    Task<공동구매자동집단응답?> 조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 공동수입 준비 Business Case를 조회하고 저장하기 위한 전용 Port입니다.
/// 공동수입 Process Manager가 커뮤니티 저장소 구현을 직접 알지 않게 합니다.
/// </summary>
public interface I공동수입준비BusinessCaseStore
{
    Task<커뮤니티원장Dto?> 조회Async(
        string caseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<커뮤니티원장Dto>> 목록조회Async(
        커뮤니티원장조회조건 query,
        CancellationToken cancellationToken = default);

    Task<커뮤니티원장Dto> 저장Async(
        커뮤니티원장저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 공동수입 준비 판단에 참고할 공공데이터 배치 상태를 읽는 Port입니다.
/// 배치 실행이나 설정 변경은 하지 않고 현재 근거 상태만 반환합니다.
/// </summary>
public interface I공동수입준비EvidenceBatchReader
{
    IReadOnlyList<공동구매수요모집Os배치작업응답> 조회();
}

internal sealed class 공동수입준비LocalSourceGroupReader(
    I공동구매자동집단화저장소 저장소) : I공동수입준비SourceGroupReader
{
    public Task<공동구매자동집단응답?> 조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default)
        => 저장소.집단조회Async(자동집단Id, cancellationToken);
}

internal sealed class 공동수입준비LocalBusinessCaseStore(
    I커뮤니티원장저장소 저장소) : I공동수입준비BusinessCaseStore
{
    public Task<커뮤니티원장Dto?> 조회Async(
        string caseId,
        CancellationToken cancellationToken = default)
        => 저장소.원장조회Async(caseId, cancellationToken);

    public Task<IReadOnlyList<커뮤니티원장Dto>> 목록조회Async(
        커뮤니티원장조회조건 query,
        CancellationToken cancellationToken = default)
        => 저장소.원장목록조회Async(query, cancellationToken);

    public Task<커뮤니티원장Dto> 저장Async(
        커뮤니티원장저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
        => 저장소.원장저장Async(request, updatedBy, cancellationToken);
}

internal sealed class 공동수입준비LocalEvidenceBatchReader(
    I공동구매수요모집BatchCatalog catalog) : I공동수입준비EvidenceBatchReader
{
    public IReadOnlyList<공동구매수요모집Os배치작업응답> 조회()
        => catalog.조회().작업목록
            .Where(item => !string.Equals(
                item.작업코드,
                공동구매수요모집Os배치작업코드.모집마감장기정체점검,
                StringComparison.Ordinal))
            .ToArray();
}
