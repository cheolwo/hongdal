using Ssalddel.Contracts.Admin.Management;

namespace Ssalddel.Application.Admin.Management;

public sealed record 관리자연락처검색Query(string 전화번호뒤8자리) : IRequest<관리자연락처검색응답>;
