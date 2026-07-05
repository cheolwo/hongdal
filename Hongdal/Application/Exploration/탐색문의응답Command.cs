using FluentResults;
using Hongdal.Contracts.Common.Exploration;

namespace Hongdal.Application.Exploration;

public sealed record 탐색문의응답Command(string 대상UserId, string 대상역할, long 탐색캠페인Id, 탐색문의응답요청 요청) : IRequest<Result>;
