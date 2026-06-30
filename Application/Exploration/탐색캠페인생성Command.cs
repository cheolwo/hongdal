using FluentResults;
using Hongdal.Contracts.Common.Exploration;
using 탐색캠페인응답Dto = Hongdal.Contracts.Common.Exploration.탐색캠페인응답;

namespace Hongdal.Application.Exploration;

public sealed record 탐색캠페인생성Command(탐색캠페인생성요청 요청) : IRequest<Result<탐색캠페인응답Dto>>;
