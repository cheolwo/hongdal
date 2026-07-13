using MediatR;

namespace Hongdal.Application.Community;

public sealed record 커뮤니티게시글등록됨Event(long 게시글Id) : INotification;
