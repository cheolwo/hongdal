using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.CommonContents;

namespace HongdalAdmin.Services;

public sealed class 백오피스조회Service : I백오피스Service
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;
    private readonly 백오피스메모리Service _memory;

    public 백오피스조회Service(HttpClient httpClient, 관리자인증세션Service session, 백오피스메모리Service memory)
    {
        _httpClient = httpClient;
        _session = session;
        _memory = memory;
    }

    public async Task<관리자대시보드요약응답> 대시보드조회Async(CancellationToken cancellationToken = default)
    {
        return await _memory.대시보드조회Async(cancellationToken);
    }

    public async Task<IReadOnlyList<화주운송의뢰응답>> 의뢰목록조회Async(string? 결제상태 = null, string? 배차상태 = null, CancellationToken cancellationToken = default)
    {
        return await _memory.의뢰목록조회Async(결제상태, 배차상태, cancellationToken);
    }

    public async Task<IReadOnlyList<공개화물요약응답>> 공개화물요약조회Async(CancellationToken cancellationToken = default)
    {
        return await _memory.공개화물요약조회Async(cancellationToken);
    }

    public async Task<화주운송의뢰응답?> 의뢰상세조회Async(string requestId, CancellationToken cancellationToken = default)
    {
        return await _memory.의뢰상세조회Async(requestId, cancellationToken);
    }

    public async Task<화주운송의뢰응답?> 의뢰취소환불처리Async(string requestId, CancellationToken cancellationToken = default)
    {
        return await _memory.의뢰취소환불처리Async(requestId, cancellationToken);
    }

    public async Task<IReadOnlyList<결제목록응답>> 결제목록조회Async(string? 결제상태 = null, string? 의뢰Id = null, CancellationToken cancellationToken = default)
    {
        return await _memory.결제목록조회Async(결제상태, 의뢰Id, cancellationToken);
    }

    public async Task<토스결제환경응답> 토스결제환경조회Async(CancellationToken cancellationToken = default)
    {
        return await _memory.토스결제환경조회Async(cancellationToken);
    }

    public async Task<IReadOnlyList<배차대기응답>> 배차대기목록조회Async(CancellationToken cancellationToken = default)
    {
        return await _memory.배차대기목록조회Async(cancellationToken);
    }

    public async Task<배차대기응답?> 배차대기상태변경Async(long id, string status, CancellationToken cancellationToken = default)
    {
        return await _memory.배차대기상태변경Async(id, status, cancellationToken);
    }

    public async Task 배차대기삭제Async(long id, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.DeleteAsync($"api/v1/dispatch/wait/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<기사목록응답>> 기사목록조회Async(string? 운행상태 = null, string? 활동지역검색어 = null, CancellationToken cancellationToken = default)
    {
        return await _memory.기사목록조회Async(운행상태, 활동지역검색어, cancellationToken);
    }

    public async Task<기사상세응답?> 기사상세조회Async(string driverId, CancellationToken cancellationToken = default)
    {
        return await _memory.기사상세조회Async(driverId, cancellationToken);
    }

    public async Task<IReadOnlyList<기사배차내역응답>> 기사배차내역조회Async(string driverId, CancellationToken cancellationToken = default)
    {
        return await _memory.기사배차내역조회Async(driverId, cancellationToken);
    }

    public async Task<IReadOnlyList<기사월정산관리응답>> 기사월정산목록조회Async(int? year = null, int? month = null, string? driverId = null, CancellationToken cancellationToken = default)
    {
        return await _memory.기사월정산목록조회Async(year, month, driverId, cancellationToken);
    }

    public async Task<IReadOnlyList<운송진행응답>> 운송진행목록조회Async(string? 상태 = null, CancellationToken cancellationToken = default)
    {
        return await _memory.운송진행목록조회Async(상태, cancellationToken);
    }

    public async Task<IReadOnlyList<운송이벤트로그응답>> 운송이벤트조회Async(string? requestId = null, CancellationToken cancellationToken = default)
    {
        return await _memory.운송이벤트조회Async(requestId, cancellationToken);
    }

    public async Task<IReadOnlyList<업체관리응답>> 업체목록조회Async(string? 상태 = null, CancellationToken cancellationToken = default)
    {
        return await _memory.업체목록조회Async(상태, cancellationToken);
    }

    public async Task<IReadOnlyList<화주관리응답>> 화주목록조회Async(CancellationToken cancellationToken = default)
    {
        return await _memory.화주목록조회Async(cancellationToken);
    }

    public async Task<IReadOnlyList<파일POD응답>> 파일POD목록조회Async(string? fileType = null, string? requestId = null, CancellationToken cancellationToken = default)
    {
        return await _memory.파일POD목록조회Async(fileType, requestId, cancellationToken);
    }

    public async Task<IReadOnlyList<문서정책요약응답>> 문서정책목록조회Async(CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var result = await _httpClient.GetFromJsonAsync<List<문서정책요약응답>>("api/v1/admin/documents/policies", cancellationToken);
        return result ?? [];
    }

    public async Task<문서정책요약응답?> 문서정책수정Async(string documentCode, 문서정책수정요청 request, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PutAsJsonAsync($"api/v1/admin/documents/policies/{Uri.EscapeDataString(documentCode.Trim())}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<문서정책요약응답>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<문서조회요약응답>> 문서목록조회Async(string? documentCode = null, string? requestId = null, string? status = null, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var query = "api/v1/admin/documents";
        var args = new List<string>();
        if (!string.IsNullOrWhiteSpace(documentCode)) args.Add($"documentCode={Uri.EscapeDataString(documentCode.Trim())}");
        if (!string.IsNullOrWhiteSpace(requestId)) args.Add($"requestId={Uri.EscapeDataString(requestId.Trim())}");
        if (!string.IsNullOrWhiteSpace(status)) args.Add($"status={Uri.EscapeDataString(status.Trim())}");
        if (args.Count > 0) query += "?" + string.Join("&", args);

        var result = await _httpClient.GetFromJsonAsync<List<문서조회요약응답>>(query, cancellationToken);
        return result ?? [];
    }

    public async Task<IReadOnlyList<문서조회로그요약응답>> 문서로그목록조회Async(long? documentId = null, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var query = "api/v1/admin/documents/logs";
        if (documentId.HasValue)
        {
            query += $"?documentId={documentId.Value}";
        }

        var result = await _httpClient.GetFromJsonAsync<List<문서조회로그요약응답>>(query, cancellationToken);
        return result ?? [];
    }

    public async Task<문서조회요약응답?> 문서업로드Async(Stream fileStream, string fileName, string contentType, string documentCode, string documentName, string requestId, long? transportId = null, bool? encrypt = null, bool? allowDownload = null, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        content.Add(fileContent, "File", fileName);
        content.Add(new StringContent(requestId ?? string.Empty), "의뢰Id");
        if (transportId.HasValue) content.Add(new StringContent(transportId.Value.ToString()), "배송운송Id");
        content.Add(new StringContent(documentCode), "문서코드");
        content.Add(new StringContent(documentName), "문서명");
        if (encrypt.HasValue) content.Add(new StringContent(encrypt.Value.ToString().ToLowerInvariant()), "암호화여부");
        if (allowDownload.HasValue) content.Add(new StringContent(allowDownload.Value.ToString().ToLowerInvariant()), "다운로드허용여부");

        var response = await _httpClient.PostAsync("api/v1/admin/documents", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<문서조회요약응답>(cancellationToken: cancellationToken);
    }

    public async Task<byte[]?> 문서다운로드Async(long id, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        return await _httpClient.GetByteArrayAsync($"api/v1/admin/documents/{id}/download", cancellationToken);
    }

    public async Task<파일POD응답?> 파일POD상태변경Async(Guid id, string uploadStatus, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PatchAsJsonAsync($"api/v1/admin/files/pod/{id}/status", new 파일POD상태변경요청
        {
            UploadStatus = uploadStatus
        }, cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<파일POD응답>(cancellationToken: cancellationToken);
    }

    public async Task<파일POD응답?> 파일POD업로드Async(Stream fileStream, string fileName, string contentType, string fileType, string? requestId, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        content.Add(fileContent, "File", fileName);
        content.Add(new StringContent(fileType), "FileType");
        if (!string.IsNullOrWhiteSpace(requestId)) content.Add(new StringContent(requestId.Trim()), "RequestId");

        var response = await _httpClient.PostAsync("api/v1/admin/files/pod/upload", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<파일POD응답>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<관리자공통콘텐츠요약응답>> 공통콘텐츠목록조회Async(CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var result = await _httpClient.GetFromJsonAsync<List<관리자공통콘텐츠요약응답>>("api/v1/admin/common-contents", cancellationToken);
        return result ?? [];
    }

    public async Task<관리자공통콘텐츠상세응답?> 공통콘텐츠상세조회Async(long id, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        return await _httpClient.GetFromJsonAsync<관리자공통콘텐츠상세응답>($"api/v1/admin/common-contents/{id}", cancellationToken);
    }

    public async Task<관리자공통콘텐츠상세응답?> 공통콘텐츠등록Async(관리자공통콘텐츠저장요청 request, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PostAsJsonAsync("api/v1/admin/common-contents", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<관리자공통콘텐츠상세응답>(cancellationToken: cancellationToken);
    }

    public async Task<관리자공통콘텐츠상세응답?> 공통콘텐츠수정Async(long id, 관리자공통콘텐츠저장요청 request, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PutAsJsonAsync($"api/v1/admin/common-contents/{id}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<관리자공통콘텐츠상세응답>(cancellationToken: cancellationToken);
    }

    public async Task 공통콘텐츠활성화변경Async(long id, bool enabled, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PatchAsync($"api/v1/admin/common-contents/{id}/active?enabled={enabled.ToString().ToLowerInvariant()}", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<공통콘텐츠보상정책Dto>> 공통콘텐츠보상정책목록조회Async(CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var result = await _httpClient.GetFromJsonAsync<List<공통콘텐츠보상정책Dto>>("api/v1/admin/common-contents/reward-policies", cancellationToken);
        return result ?? [];
    }

    public async Task<공통콘텐츠보상정책Dto?> 공통콘텐츠보상정책등록Async(공통콘텐츠보상정책Dto request, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PostAsJsonAsync("api/v1/admin/common-contents/reward-policies", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<공통콘텐츠보상정책Dto>(cancellationToken: cancellationToken);
    }

    private void ApplyAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            throw new InvalidOperationException("로그인이 필요합니다.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
    }
}

public sealed class 관리자대시보드요약응답
{
    public int 오늘의뢰수 { get; set; }
    public int 결제대기수 { get; set; }
    public int 결제완료수 { get; set; }
    public int 배차대기수 { get; set; }
    public int 배차확정수 { get; set; }
    public int 운송중수 { get; set; }
    public int 완료수 { get; set; }
    public int 취소환불수 { get; set; }
}

public sealed class 화주운송의뢰응답
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 의뢰상태 { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public string 정산상태 { get; set; } = string.Empty;
    public string 배차상태 { get; set; } = string.Empty;
    public string 운송방식 { get; set; } = string.Empty;
    public string 결제수단 { get; set; } = string.Empty;
    public string 정산시점 { get; set; } = string.Empty;
    public string 증빙방식 { get; set; } = string.Empty;
    public string 수납주체 { get; set; } = string.Empty;
    public bool 세금계산서필요 { get; set; }
    public bool 현금영수증필요 { get; set; }
    public string? 정산메모 { get; set; }
    public DateTime 생성일시 { get; set; }

    public string 픽업지 { get; set; } = string.Empty;
    public string 픽업상세지 { get; set; } = string.Empty;
    public decimal? 픽업위도 { get; set; }
    public decimal? 픽업경도 { get; set; }
    public string 하차지 { get; set; } = string.Empty;
    public string 하차상세지 { get; set; } = string.Empty;
    public decimal? 하차위도 { get; set; }
    public decimal? 하차경도 { get; set; }

    public decimal? 대기료 { get; set; }
    public decimal? 수작업비 { get; set; }
    public decimal? 할증 { get; set; }
    public decimal? 최종운임 { get; set; }

    public 의뢰요약? 요약 { get; set; }
}

public sealed class 의뢰요약
{
    public string 화물종류 { get; set; } = string.Empty;
    public string 픽업지 { get; set; } = string.Empty;
    public string 하차지 { get; set; } = string.Empty;
}

public sealed class 화주운송의뢰수정요청
{
    public string? 상태 { get; set; }
    public string? 결제상태 { get; set; }
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

public sealed class 토스결제환경응답
{
    public string ClientKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
}

public sealed class 배차대기응답
{
    public long Id { get; set; }
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화주Id { get; set; } = string.Empty;
    public int? 배차업무유형 { get; set; }
    public string? 원본의뢰유형 { get; set; }
    public string? 원본의뢰Id { get; set; }
    public string? 공동구매도착지유형코드 { get; set; }
    public bool? 공동구매기사세대배송여부 { get; set; }
    public string? 공동구매세대배송방식코드 { get; set; }
    public int? 공동구매세대배송건수 { get; set; }
    public string? 공동구매분배책임코드 { get; set; }
    public string 픽업_도로명주소 { get; set; } = string.Empty;
    public string 픽업_상세주소 { get; set; } = string.Empty;
    public decimal? 픽업_위도 { get; set; }
    public decimal? 픽업_경도 { get; set; }
    public string 하차_도로명주소 { get; set; } = string.Empty;
    public string 하차_상세주소 { get; set; } = string.Empty;
    public decimal? 하차_위도 { get; set; }
    public decimal? 하차_경도 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class 배차대기수정요청
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화주Id { get; set; } = string.Empty;
    public string 픽업_도로명주소 { get; set; } = string.Empty;
    public string 픽업_상세주소 { get; set; } = string.Empty;
    public decimal? 픽업_위도 { get; set; }
    public decimal? 픽업_경도 { get; set; }
    public string 하차_도로명주소 { get; set; } = string.Empty;
    public string 하차_상세주소 { get; set; } = string.Empty;
    public decimal? 하차_위도 { get; set; }
    public decimal? 하차_경도 { get; set; }
    public string 상태 { get; set; } = string.Empty;
}

public sealed class 기사목록응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public decimal? 최근위도 { get; set; }
    public decimal? 최근경도 { get; set; }
    public DateTime? 최근위치기록시각 { get; set; }
    public int 배차건수 { get; set; }
}

public sealed class 기사상세응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
    public DateTime? 등록일 { get; set; }
    public decimal? 최근위도 { get; set; }
    public decimal? 최근경도 { get; set; }
    public DateTime? 최근위치기록시각 { get; set; }
}

public sealed class 기사배차내역응답
{
    public long Id { get; set; }
    public string 배차명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public DateTime? 배차일 { get; set; }
    public string 픽업지 { get; set; } = string.Empty;
    public string 배송지 { get; set; } = string.Empty;
    public decimal? 배차점수 { get; set; }
    public string 실패사유 { get; set; } = string.Empty;
    public DateTime? 배차생성시각 { get; set; }
    public DateTime? 배차완료시각 { get; set; }
}

public sealed class 기사월정산관리응답
{
    public string 기사Id { get; set; } = string.Empty;
    public int 년도 { get; set; }
    public int 월 { get; set; }
    public int 배차건수 { get; set; }
    public decimal 이용료 { get; set; }
    public bool 월상한적용여부 { get; set; }
    public bool 결제완료 { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class 운송진행응답
{
    public long Id { get; set; }
    public string 운송번호 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public DateTime? 출발_픽업 { get; set; }
    public DateTime? 도착 { get; set; }
    public string 기사_운송자 { get; set; } = string.Empty;
    public string 출발지 { get; set; } = string.Empty;
    public string 도착지 { get; set; } = string.Empty;
    public decimal? 운임 { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class 운송이벤트로그응답
{
    public long Id { get; set; }
    public string 의뢰Id { get; set; } = string.Empty;
    public string 이벤트타입 { get; set; } = string.Empty;
    public DateTime 이벤트시각 { get; set; }
    public string 메타데이터 { get; set; } = string.Empty;
}

public sealed class 업체관리응답
{
    public long Id { get; set; }
    public string 업체명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 대표연락처 { get; set; } = string.Empty;
    public string 담당자 { get; set; } = string.Empty;
    public string 이메일 { get; set; } = string.Empty;
    public string 주소 { get; set; } = string.Empty;
    public string 정산결제조건 { get; set; } = string.Empty;
    public DateTime? 등록일 { get; set; }
}

public sealed class 화주관리응답
{
    public string 화주Id { get; set; } = string.Empty;
    public string 사용자명 { get; set; } = string.Empty;
    public string 이메일 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public int 의뢰건수 { get; set; }
    public DateTime? 최근의뢰일시 { get; set; }
    public string 거래상태 { get; set; } = string.Empty;
}

public sealed class 파일POD응답
{
    public Guid Id { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string UploadStatus { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 파일POD상태변경요청
{
    public string UploadStatus { get; set; } = string.Empty;
}

public sealed class 문서정책수정요청
{
    public bool 사용여부 { get; set; }
    public bool 암호화여부 { get; set; }
    public bool 다운로드허용여부 { get; set; }
    public bool 서명필요여부 { get; set; }
    public string 자동생성시점 { get; set; } = string.Empty;
    public string 조회가능역할목록Json { get; set; } = string.Empty;
    public int 보관일수 { get; set; }
    public bool 수정가능여부 { get; set; }
    public bool 감사로그여부 { get; set; }
}

public sealed class 문서정책요약응답
{
    public long Id { get; set; }
    public string 문서코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public bool 사용여부 { get; set; }
    public bool 암호화여부 { get; set; }
    public bool 다운로드허용여부 { get; set; }
    public bool 서명필요여부 { get; set; }
    public string 자동생성시점 { get; set; } = string.Empty;
    public string 조회가능역할목록Json { get; set; } = string.Empty;
    public int 보관일수 { get; set; }
    public bool 수정가능여부 { get; set; }
    public bool 감사로그여부 { get; set; }
    public DateTime 생성일시 { get; set; }
    public DateTime? 수정일시 { get; set; }
}

public sealed class 문서조회요약응답
{
    public long Id { get; set; }
    public string 의뢰Id { get; set; } = string.Empty;
    public long? 배송운송Id { get; set; }
    public string 문서코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public string 파일명 { get; set; } = string.Empty;
    public string 생성상태 { get; set; } = string.Empty;
    public bool 암호화됨 { get; set; }
    public bool 다운로드허용여부 { get; set; }
    public bool 수정가능여부 { get; set; }
    public DateTime 생성일시 { get; set; }
    public DateTime? 보관만료일시 { get; set; }
}

public sealed class 문서조회로그요약응답
{
    public long Id { get; set; }
    public long 문서Id { get; set; }
    public string 행위 { get; set; } = string.Empty;
    public string 사용자Id { get; set; } = string.Empty;
    public string 사용자명 { get; set; } = string.Empty;
    public string 역할명 { get; set; } = string.Empty;
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime 생성일시 { get; set; }
}

public sealed class 문서생성요청
{
    public string 의뢰Id { get; set; } = string.Empty;
    public long? 배송운송Id { get; set; }
    public string 문서코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public string 파일명 { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public bool? 암호화여부 { get; set; }
    public bool? 다운로드허용여부 { get; set; }
    public string? 생성자 { get; set; }
}

public sealed class 공개화물요약응답
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화물종류 { get; set; } = string.Empty;
    public int? 화물수량 { get; set; }
    public decimal? 화물중량Kg { get; set; }
    public string 운송방식 { get; set; } = string.Empty;
    public string 차량종류 { get; set; } = string.Empty;
    public string 의뢰상태 { get; set; } = string.Empty;
    public string 배차상태 { get; set; } = string.Empty;
    public DateTime 생성일시 { get; set; }
}
