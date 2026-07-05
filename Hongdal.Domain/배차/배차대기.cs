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

        [Column("business_type")]
        public int 배차업무유형 { get; set; } = 상태값.배차업무유형.용달운송;

        [Column("source_type")]
        public string 원본의뢰유형 { get; set; } = "CargoTransport";

        [Column("source_request_id")]
        public string 원본의뢰Id { get; set; } = string.Empty;

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

        // 큐 단계 및 노출 상태 관리 (MLFQ 스타일 상태 머신 확장을 위한 필드)
        [Column("queue_stage")]
        public int 배차큐단계 { get; set; } = 상태값.배차큐단계.계획배차;

        [Column("exposure_state")]
        public int 배차노출상태 { get; set; } = 상태값.배차노출상태.계획대기;

        [Column("current_recommended_driver_id")]
        public string? 현재추천대상기사Id { get; set; }

        [Column("recommendation_started_at")]
        public DateTime? 추천시작시각 { get; set; }

        [Column("recommendation_expires_at")]
        public DateTime? 추천만료시각 { get; set; }

        [Column("recommendation_round")]
        public int 추천라운드 { get; set; }

        [Column("plan_attempts")]
        public int 계획배차시도횟수 { get; set; }

        [Column("last_rejected_driver_id")]
        public string? 마지막거절기사Id { get; set; }

        [Column("public_transition_at")]
        public DateTime? 공개전환시각 { get; set; }

        [Column("confirmed_driver_id")]
        public string? 확정기사Id { get; set; }

        [Timestamp]
        [Column("row_version")]
        public byte[]? RowVersion { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
