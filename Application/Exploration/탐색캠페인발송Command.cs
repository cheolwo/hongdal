using FluentResults;
using Hongdal.Contracts.Common.Exploration;

namespace Hongdal.Application.Exploration;

public sealed record 탐색캠페인발송Command(string 개시자UserId, string 개시자역할, long 탐색캠페인Id, 탐색캠페인발송요청 요청) : IRequest<Result<탐색캠페인상세응답>>;
