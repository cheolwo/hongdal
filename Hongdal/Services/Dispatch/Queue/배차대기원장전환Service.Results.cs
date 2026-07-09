using 홍달.도메인.배차;

namespace 홍달.Services.Dispatch.Queue
{
    public sealed partial class 배차대기원장전환Service
    {
        private static 배차대기원장전환결과 대상없음(string requestId, string? driverId = null)
            => 배차대기원장전환결과.전환안됨(
                requestId,
                배차대기원장전환결과코드.대상없음,
                "배차대기 원장 데이터를 찾을 수 없습니다.",
                driverId);

        private static 배차대기원장전환결과 대기상태아님(배차대기 queue, string? driverId = null)
            => 전환안됨(
                queue,
                배차대기원장전환결과코드.대기상태아님,
                "배차대기가 대기 상태가 아니라 전환하지 않았습니다.",
                driverId);

        private static 배차대기원장전환결과 전환됨(배차대기 queue, string code, string message, string? driverId = null)
            => 배차대기원장전환결과.전환됨(queue.의뢰Id, code, message, driverId ?? queue.현재추천대상기사Id ?? queue.확정기사Id);

        private static 배차대기원장전환결과 전환안됨(배차대기 queue, string code, string message, string? driverId = null)
            => 배차대기원장전환결과.전환안됨(queue.의뢰Id, code, message, driverId ?? queue.현재추천대상기사Id ?? queue.확정기사Id);
    }
}
