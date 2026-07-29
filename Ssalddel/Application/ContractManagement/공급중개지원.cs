using FluentResults;
using Microsoft.AspNetCore.Http;
using Ssalddel.Contracts.Common.ContractManagement;
using 살뜰.도메인.공급중개;

namespace Ssalddel.Application.ContractManagement;

public interface I공급조직접근Accessor
{
    string? 조직참조Key조회(string organizationTypeCode);
}

internal static class 공급중개Results
{
    internal static Result<T> Unauthorized<T>()
        => Failure<T>("로그인 사용자 인증 정보가 필요합니다.", StatusCodes.Status401Unauthorized);

    internal static Result<T> Forbidden<T>(string message)
        => Failure<T>(message, StatusCodes.Status403Forbidden);

    internal static Result<T> BadRequest<T>(string message)
        => Failure<T>(message, StatusCodes.Status400BadRequest);

    internal static Result<T> NotFound<T>(string message)
        => Failure<T>(message, StatusCodes.Status404NotFound);

    internal static Result<T> Conflict<T>(string message)
        => Failure<T>(message, StatusCodes.Status409Conflict);

    private static Result<T> Failure<T>(string message, int statusCode)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", statusCode));
}

internal static class 공급중개Mapper
{
    internal static 플랫폼공급계약응답 ToResponse(
        플랫폼공급조건계약 agreement,
        string? organizationTypeCode = null)
        => new()
        {
            공급계약Id = agreement.Id,
            계약번호 = agreement.계약번호,
            공급자Key = agreement.공급자Key,
            공급자명 = agreement.공급자명,
            계약문서버전 = agreement.계약문서버전,
            상태코드 = agreement.상태코드,
            유효시작Utc = agreement.유효시작Utc,
            유효종료Utc = agreement.유효종료Utc,
            통화코드 = agreement.통화코드,
            정산조건 = agreement.정산조건,
            반품조건 = agreement.반품조건,
            플랫폼역할코드 = agreement.플랫폼역할코드,
            플랫폼판매자여부 = agreement.플랫폼판매자여부,
            플랫폼재판매자여부 = agreement.플랫폼재판매자여부,
            품목목록 = agreement.품목목록
                .Where(item => string.IsNullOrWhiteSpace(organizationTypeCode)
                               || item.조직유형허용(organizationTypeCode))
                .OrderBy(item => item.품목명, StringComparer.Ordinal)
                .Select(ToResponse)
                .ToArray()
        };

    internal static 플랫폼공급계약품목응답 ToResponse(플랫폼공급조건계약품목 item)
        => new()
        {
            공급계약품목Id = item.Id,
            계약품목Key = item.계약품목Key,
            SKU = item.SKU,
            품목명 = item.품목명,
            공급단위 = item.공급단위,
            계약단가 = item.계약단가,
            최소발주수량 = item.최소발주수량,
            최대발주수량 = item.최대발주수량,
            원산지표시 = item.원산지표시,
            보관조건 = item.보관조건,
            허용조직유형목록 = item.허용조직유형목록()
        };

    internal static 공급계약이용등록응답 ToResponse(공급계약이용등록 participation)
        => new()
        {
            공급계약이용등록Id = participation.Id,
            공급계약Id = participation.공급계약Id,
            조직유형코드 = participation.조직유형코드,
            조직참조Key = participation.조직참조Key,
            계약문서버전 = participation.계약문서버전,
            상태코드 = participation.상태코드,
            등록시각Utc = participation.등록시각Utc
        };

    internal static 개별공급발주응답 ToResponse(조직개별공급발주 order)
        => new()
        {
            개별공급발주Id = order.Id,
            공급계약Id = order.공급계약Id,
            공급계약품목Id = order.공급계약품목Id,
            계약번호Snapshot = order.계약번호Snapshot,
            계약문서버전Snapshot = order.계약문서버전Snapshot,
            공급자KeySnapshot = order.공급자KeySnapshot,
            공급자명Snapshot = order.공급자명Snapshot,
            구매조직유형코드 = order.구매조직유형코드,
            구매조직참조Key = order.구매조직참조Key,
            품목명Snapshot = order.품목명Snapshot,
            SKUSnapshot = order.SKUSnapshot,
            공급단위Snapshot = order.공급단위Snapshot,
            발주수량 = order.발주수량,
            공급자수락수량 = order.공급자수락수량,
            계약단가Snapshot = order.계약단가Snapshot,
            발주금액Snapshot = order.발주금액Snapshot,
            통화코드Snapshot = order.통화코드Snapshot,
            희망납품일Utc = order.희망납품일Utc,
            납품지참조Key = order.납품지참조Key,
            상태코드 = order.상태코드,
            플랫폼역할코드 = order.플랫폼역할코드,
            플랫폼판매자여부 = order.플랫폼판매자여부,
            플랫폼재판매자여부 = order.플랫폼재판매자여부,
            결제실행됨 = order.결제실행됨,
            재고예약됨 = order.재고예약됨,
            입고생성됨 = order.입고생성됨,
            공급자응답근거참조 = order.공급자응답근거참조,
            제출시각Utc = order.제출시각Utc,
            공급자응답시각Utc = order.공급자응답시각Utc
        };
}
