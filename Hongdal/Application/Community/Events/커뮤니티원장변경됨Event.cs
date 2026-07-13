using Hongdal.Services.Community;
using MediatR;

namespace Hongdal.Application.Community.Events;

public sealed record 커뮤니티원장변경됨Event(
    커뮤니티원장Dto 원장,
    string 변경유형,
    string 변경자,
    커뮤니티원장상태변경요청? 상태변경요청,
    DateTime 발생시각Utc,
    string EventId) : INotification;

public static class 커뮤니티원장변경유형
{
    public const string 저장 = "저장";
    public const string 상태변경 = "상태변경";
}
