using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using 홍달.도메인.공통;

namespace 홍달.도메인.화주
{
    [Table("shipper_requests")]
    public class 화주운송의뢰
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("request_id")]
        public string 의뢰Id { get; set; } = string.Empty;

        [Column("shipper_id")]
        public string 화주Id { get; set; } = string.Empty;

        [Column("cargo_type")]
        public string 화물종류 { get; set; } = string.Empty;

        [Column("cargo_description")]
        public string 화물설명 { get; set; } = string.Empty;

        [Column("cargo_quantity")]
        public int? 화물수량 { get; set; }

        [Column("cargo_length_mm")]
        public int? 화물길이Mm { get; set; }

        [Column("cargo_width_mm")]
        public int? 화물폭Mm { get; set; }

        [Column("cargo_height_mm")]
        public int? 화물높이Mm { get; set; }

        [Column("cargo_pallet_count")]
        public int? 화물팔레트개수 { get; set; }

        [Column("cargo_weight_kg")]
        public decimal? 화물중량Kg { get; set; }

        [Column("cargo_volume_cbm")]
        public decimal? 화물부피Cbm { get; set; }

        [Column("cargo_fragile")]
        public bool 화물파손주의여부 { get; set; }

        [Column("cargo_temperature")]
        public string 화물온도조건 { get; set; } = "상온";

        [Column("transport_type")]
        public string 운송방식 { get; set; } = "혼적";

        [Column("vehicle_type")]
        public string 차량종류 { get; set; } = string.Empty;

        [Column("payment_method")]
        public string 결제수단 { get; set; } = "카드";

        [Column("estimated_payment_amount")]
        public int? 결제예정금액 { get; set; }

        [Column("pricing_config_id")]
        public long? 운임구성Id { get; set; }

        [Column("pickup_address")]
        public string 픽업_도로명주소 { get; set; } = string.Empty;

        [Column("pickup_address_detail")]
        public string 픽업_상세주소 { get; set; } = string.Empty;

        [Column("pickup_latitude")]
        public decimal? 픽업_위도 { get; set; }

        [Column("pickup_longitude")]
        public decimal? 픽업_경도 { get; set; }

        [Column("pickup_contact_name")]
        public string 픽업_연락처_이름 { get; set; } = string.Empty;

        [Column("pickup_contact_phone")]
        public string 픽업_연락처_전화번호 { get; set; } = string.Empty;

        [Column("pickup_window_start")]
        public DateTime 픽업_시간창_시작일시 { get; set; }

        [Column("pickup_window_end")]
        public DateTime 픽업_시간창_종료일시 { get; set; }

        [Column("dropoff_address")]
        public string 하차_도로명주소 { get; set; } = string.Empty;

        [Column("dropoff_address_detail")]
        public string 하차_상세주소 { get; set; } = string.Empty;

        [Column("dropoff_latitude")]
        public decimal? 하차_위도 { get; set; }

        [Column("dropoff_longitude")]
        public decimal? 하차_경도 { get; set; }

        [Column("dropoff_contact_name")]
        public string 하차_연락처_이름 { get; set; } = string.Empty;

        [Column("dropoff_contact_phone")]
        public string 하차_연락처_전화번호 { get; set; } = string.Empty;

        [Column("dropoff_window_start")]
        public DateTime? 하차_시간창_시작일시 { get; set; }

        [Column("dropoff_window_end")]
        public DateTime? 하차_시간창_종료일시 { get; set; }

        [Column("service_level")]
        public string 서비스레벨 { get; set; } = string.Empty;

        [Column("request_text")]
        public string 요청사항 { get; set; } = string.Empty;

        [Column("waiting_fee")]
        public decimal? 대기료 { get; set; }

        [Column("manual_fee")]
        public decimal? 수작업비 { get; set; }

        [Column("surcharge")]
        public decimal? 할증 { get; set; }

        [Column("final_fare")]
        public decimal? 최종운임 { get; set; }

        [Column("client_request_id")]
        public string 클라이언트요청Id { get; set; } = string.Empty;

        [Column("status")]
        public string 상태 { get; set; } = 상태값.의뢰상태.생성됨;

        [Column("payment_status")]
        public string 결제상태 { get; set; } = 상태값.결제상태.결제대기;

        [Column("dispatch_status")]
        public string 배차상태 { get; set; } = 상태값.배차상태.미시작;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
