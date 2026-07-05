using Hongdal.Contracts.Shipper.ImportFood;

namespace Hongdal.Application.Shipper.ImportFood;

public sealed record 해외제조업소조회Query(
    int 페이지번호,
    int 한페이지결과수,
    string 데이터형식,
    string? 해외제조업소명,
    string? 식품구분명,
    string? 국가명) : IRequest<해외제조업소조회화면응답>;
