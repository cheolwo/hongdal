using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using 홍달.도메인.공통;

namespace 홍달.도메인.결제
{
    [Table("결제")]
    public class 결제
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("payment_id")]
        public string 결제Id { get; set; } = string.Empty;

        [Column("request_id")]
        public string 의뢰Id { get; set; } = string.Empty;

        [Column("shipper_id")]
        public string 화주Id { get; set; } = string.Empty;

        [Column("pg_provider")]
        public string PG사 { get; set; } = "TossPayments";

        [Column("payment_method")]
        public string 결제수단 { get; set; } = "미정";

        [Column("payment_status")]
        public string 결제상태 { get; set; } = 상태값.결제상태.결제대기;

        [Column("amount")]
        public int 결제금액 { get; set; }

        [Column("order_id")]
        public string OrderId { get; set; } = string.Empty;

        [Column("payment_key")]
        public string? PaymentKey { get; set; }

        [Column("toss_response_json")]
        public string? Toss응답Json { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("approved_at")]
        public DateTime? 승인일시 { get; set; }
    }
}
