namespace 홍달.도메인.화물
{
    public class 화물요구조건
    {
        public string 의뢰Id { get; set; } = string.Empty;

        public int? 화물길이Mm { get; set; }

        public int? 화물폭Mm { get; set; }

        public int? 화물높이Mm { get; set; }

        public int? 화물무게Kg { get; set; }

        public int? 팔레트개수 { get; set; }

        public bool 비맞으면안됨 { get; set; }

        public bool 냉장필요 { get; set; }

        public bool 냉동필요 { get; set; }

        public bool 리프트필요 { get; set; }

        public bool 측면상하차필요 { get; set; }

        public bool 장재물 { get; set; }

        public bool 혼적허용 { get; set; }

        public bool 독차필수 { get; set; }

        public string 주의사항 { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
