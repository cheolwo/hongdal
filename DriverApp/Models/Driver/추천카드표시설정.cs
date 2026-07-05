namespace DriverApp.Models.Driver
{
    public sealed class 추천카드표시설정
    {
        public 추천카드표시모드 표시모드 { get; set; } = 추천카드표시모드.표준;

        // nullable overrides: null => follow mode defaults
        public bool? 운송방식표시Override { get; set; }
        public bool? 시간조건표시Override { get; set; }
        public bool? 거리표시Override { get; set; }
        public bool? 차량조건표시Override { get; set; }
        public bool? 인수증표시Override { get; set; }
        public bool? 복귀거리표시Override { get; set; }
        public bool? 공차거리표시Override { get; set; }
        public bool? 추천사유표시Override { get; set; }
    }

    public sealed class 추천카드표시정책
    {
        public bool 운송방식표시 { get; init; }
        public bool 시간조건표시 { get; init; }
        public bool 거리표시 { get; init; }
        public bool 차량조건표시 { get; init; }
        public bool 인수증표시 { get; init; }
        public bool 복귀거리표시 { get; init; }
        public bool 공차거리표시 { get; init; }
        public bool 추천사유표시 { get; init; }
    }
}
