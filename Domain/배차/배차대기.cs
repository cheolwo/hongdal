using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using 홍달.도메인.공통;

namespace 홍달.도메인.배차
{
    [Table("배차_대기")]
    public class 배차대기
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("request_id")]
        public string 의뢰Id { get; set; } = string.Empty;

        [Column("shipper_id")]
        public string 화주Id { get; set; } = string.Empty;

        [Column("pickup_address")]
        public string 픽업_도로명주소 { get; set; } = string.Empty;

        [Column("pickup_address_detail")]
        public string 픽업_상세주소 { get; set; } = string.Empty;

        [Column("pickup_latitude")]
        public decimal? 픽업_위도 { get; set; }

        [Column("pickup_longitude")]
        public decimal? 픽업_경도 { get; set; }

        [Column("dropoff_address")]
        public string 하차_도로명주소 { get; set; } = string.Empty;

        [Column("dropoff_address_detail")]
        public string 하차_상세주소 { get; set; } = string.Empty;

        [Column("dropoff_latitude")]
        public decimal? 하차_위도 { get; set; }

        [Column("dropoff_longitude")]
        public decimal? 하차_경도 { get; set; }

        [Column("status")]
        public string 상태 { get; set; } = 상태값.배차대기상태.대기;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
