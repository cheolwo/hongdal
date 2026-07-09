using 홍달.도메인.공통;
using 홍달.도메인.배차;

namespace Hongdal.Application.Driver.DispatchAction;

public static class 배차응답가능정책
{
    public static bool 추천수락가능(배차대기 queue, string 기사Id, DateTime 기준시각Utc)
        => 추천응답가능(queue, 기사Id, 기준시각Utc);

    public static bool 추천거절가능(배차대기 queue, string 기사Id, DateTime 기준시각Utc)
        => 추천응답가능(queue, 기사Id, 기준시각Utc);

    public static bool 공개배차수락가능(배차대기 queue)
        => 대기상태인가(queue)
           && queue.배차큐단계 == 상태값.배차큐단계.공개배차
           && queue.배차노출상태 == 상태값.배차노출상태.공개중
           && queue.확정기사Id is null;

    public static bool 수락취소가능(배차대기 queue, string 기사Id)
    {
        var acceptedByDriver = string.Equals(queue.확정기사Id, 기사Id, StringComparison.Ordinal)
                               || string.Equals(queue.현재추천대상기사Id, 기사Id, StringComparison.Ordinal);
        var cancelableState = queue.상태 == 상태값.배차대기상태.확정
                              || queue.배차큐단계 == 상태값.배차큐단계.확정;

        return cancelableState && acceptedByDriver;
    }

    private static bool 추천응답가능(배차대기 queue, string 기사Id, DateTime 기준시각Utc)
        => 대기상태인가(queue)
           && queue.배차큐단계 == 상태값.배차큐단계.배차추천
           && queue.배차노출상태 == 상태값.배차노출상태.추천중
           && string.Equals(queue.현재추천대상기사Id, 기사Id, StringComparison.Ordinal)
           && queue.추천만료시각.HasValue
           && queue.추천만료시각 > 기준시각Utc;

    private static bool 대기상태인가(배차대기 queue)
        => queue.상태 == 상태값.배차대기상태.대기;
}
