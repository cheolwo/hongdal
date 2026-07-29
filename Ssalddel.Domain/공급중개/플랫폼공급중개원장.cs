using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ssalddel.Contracts.Common.ContractManagement;
using Ssalddel.Contracts.Common.Metadata;

namespace 살뜰.도메인.공급중개;

[Table("플랫폼공급조건계약")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformSupplyBrokerage,
    SsalddelCodeLayer.Domain,
    "공급자와 플랫폼이 합의한 공통 공급조건을 보존합니다.",
    FlowOrder = 20,
    Boundary = "플랫폼은 공급조건 계약의 관리·중개자이며 판매자나 재판매자가 아닙니다.")]
public sealed class 플랫폼공급조건계약
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("client_request_id")]
    public Guid 클라이언트요청Id { get; set; }

    [Column("contract_number")]
    [MaxLength(100)]
    public string 계약번호 { get; set; } = string.Empty;

    [Column("supplier_key")]
    [MaxLength(160)]
    public string 공급자Key { get; set; } = string.Empty;

    [Column("supplier_name")]
    [MaxLength(200)]
    public string 공급자명 { get; set; } = string.Empty;

    [Column("contract_document_version")]
    [MaxLength(80)]
    public string 계약문서버전 { get; set; } = string.Empty;

    [Column("status_code")]
    [MaxLength(40)]
    public string 상태코드 { get; set; } = 플랫폼공급계약상태코드.초안;

    [Column("effective_from_utc")]
    public DateTime 유효시작Utc { get; set; }

    [Column("effective_until_utc")]
    public DateTime 유효종료Utc { get; set; }

    [Column("currency_code")]
    [MaxLength(3)]
    public string 통화코드 { get; set; } = "KRW";

    [Column("settlement_terms")]
    [MaxLength(1000)]
    public string 정산조건 { get; set; } = string.Empty;

    [Column("return_terms")]
    [MaxLength(1000)]
    public string 반품조건 { get; set; } = string.Empty;

    [Column("platform_role_code")]
    [MaxLength(60)]
    public string 플랫폼역할코드 { get; set; } = 공급중개역할코드.개별발주중개;

    [Column("platform_is_seller")]
    public bool 플랫폼판매자여부 { get; set; }

    [Column("platform_is_reseller")]
    public bool 플랫폼재판매자여부 { get; set; }

    [Column("contract_evidence_reference")]
    [MaxLength(500)]
    public string 계약체결근거참조 { get; set; } = string.Empty;

    [Column("created_by_user_id")]
    [MaxLength(450)]
    public string 생성자UserId { get; set; } = string.Empty;

    [Column("created_at_utc")]
    public DateTime 생성시각Utc { get; set; }

    [Column("activated_at_utc")]
    public DateTime? 활성화시각Utc { get; set; }

    [Column("updated_at_utc")]
    public DateTime 수정시각Utc { get; set; }

    public List<플랫폼공급조건계약품목> 품목목록 { get; set; } = [];
}

[Table("플랫폼공급조건계약품목")]
public sealed class 플랫폼공급조건계약품목
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("supply_agreement_id")]
    public Guid 공급계약Id { get; set; }

    [Column("contract_item_key")]
    [MaxLength(160)]
    public string 계약품목Key { get; set; } = string.Empty;

    [Column("sku")]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    [Column("item_name")]
    [MaxLength(200)]
    public string 품목명 { get; set; } = string.Empty;

    [Column("supply_unit")]
    [MaxLength(100)]
    public string 공급단위 { get; set; } = string.Empty;

    [Column("contract_unit_price", TypeName = "decimal(18,2)")]
    public decimal 계약단가 { get; set; }

    [Column("minimum_order_quantity", TypeName = "decimal(18,3)")]
    public decimal 최소발주수량 { get; set; }

    [Column("maximum_order_quantity", TypeName = "decimal(18,3)")]
    public decimal? 최대발주수량 { get; set; }

    [Column("origin_label")]
    [MaxLength(200)]
    public string 원산지표시 { get; set; } = string.Empty;

    [Column("storage_condition")]
    [MaxLength(100)]
    public string 보관조건 { get; set; } = string.Empty;

    [Column("allowed_organization_types")]
    [MaxLength(100)]
    public string 허용조직유형Csv { get; set; } = string.Empty;

    public 플랫폼공급조건계약 공급계약 { get; set; } = null!;

    public bool 조직유형허용(string organizationTypeCode)
        => 허용조직유형목록().Contains(organizationTypeCode, StringComparer.Ordinal);

    public IReadOnlyList<string> 허용조직유형목록()
        => 허용조직유형Csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(공급이용조직유형코드.지원됨)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
}

[Table("공급계약이용등록")]
public sealed class 공급계약이용등록
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("client_request_id")]
    public Guid 클라이언트요청Id { get; set; }

    [Column("supply_agreement_id")]
    public Guid 공급계약Id { get; set; }

    [Column("organization_type_code")]
    [MaxLength(40)]
    public string 조직유형코드 { get; set; } = string.Empty;

    [Column("organization_reference_key")]
    [MaxLength(160)]
    public string 조직참조Key { get; set; } = string.Empty;

    [Column("operator_user_id")]
    [MaxLength(450)]
    public string 운영자UserId { get; set; } = string.Empty;

    [Column("contract_document_version")]
    [MaxLength(80)]
    public string 계약문서버전 { get; set; } = string.Empty;

    [Column("status_code")]
    [MaxLength(40)]
    public string 상태코드 { get; set; } = 공급계약이용상태코드.이용중;

    [Column("agreement_use_consent")]
    public bool 공급계약이용동의 { get; set; }

    [Column("separate_order_confirmation_consent")]
    public bool 개별발주별도확인동의 { get; set; }

    [Column("guidance_version")]
    [MaxLength(32)]
    public string 안내버전 { get; set; } = string.Empty;

    [Column("created_at_utc")]
    public DateTime 등록시각Utc { get; set; }

    [Column("updated_at_utc")]
    public DateTime 수정시각Utc { get; set; }

    public 플랫폼공급조건계약 공급계약 { get; set; } = null!;
}

[Table("조직개별공급발주")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformSupplyBrokerage,
    SsalddelCodeLayer.Domain,
    "음식점 또는 살들마트가 자기 명의로 공급자에게 제출한 개별 발주를 보존합니다.",
    FlowOrder = 30,
    Boundary = "발주는 공급자에게 전달되는 당사자 간 주문이며 플랫폼 매출·재고·입고를 자동 생성하지 않습니다.")]
public sealed class 조직개별공급발주
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("client_request_id")]
    public Guid 클라이언트요청Id { get; set; }

    [Column("agreement_participation_id")]
    public Guid 공급계약이용등록Id { get; set; }

    [Column("supply_agreement_id")]
    public Guid 공급계약Id { get; set; }

    [Column("supply_agreement_item_id")]
    public Guid 공급계약품목Id { get; set; }

    [Column("buyer_organization_type_code")]
    [MaxLength(40)]
    public string 구매조직유형코드 { get; set; } = string.Empty;

    [Column("buyer_organization_reference_key")]
    [MaxLength(160)]
    public string 구매조직참조Key { get; set; } = string.Empty;

    [Column("requested_by_user_id")]
    [MaxLength(450)]
    public string 요청자UserId { get; set; } = string.Empty;

    [Column("contract_number_snapshot")]
    [MaxLength(100)]
    public string 계약번호Snapshot { get; set; } = string.Empty;

    [Column("contract_document_version_snapshot")]
    [MaxLength(80)]
    public string 계약문서버전Snapshot { get; set; } = string.Empty;

    [Column("supplier_key_snapshot")]
    [MaxLength(160)]
    public string 공급자KeySnapshot { get; set; } = string.Empty;

    [Column("supplier_name_snapshot")]
    [MaxLength(200)]
    public string 공급자명Snapshot { get; set; } = string.Empty;

    [Column("item_name_snapshot")]
    [MaxLength(200)]
    public string 품목명Snapshot { get; set; } = string.Empty;

    [Column("sku_snapshot")]
    [MaxLength(100)]
    public string SKUSnapshot { get; set; } = string.Empty;

    [Column("supply_unit_snapshot")]
    [MaxLength(100)]
    public string 공급단위Snapshot { get; set; } = string.Empty;

    [Column("order_quantity", TypeName = "decimal(18,3)")]
    public decimal 발주수량 { get; set; }

    [Column("supplier_accepted_quantity", TypeName = "decimal(18,3)")]
    public decimal? 공급자수락수량 { get; set; }

    [Column("contract_unit_price_snapshot", TypeName = "decimal(18,2)")]
    public decimal 계약단가Snapshot { get; set; }

    [Column("order_amount_snapshot", TypeName = "decimal(18,2)")]
    public decimal 발주금액Snapshot { get; set; }

    [Column("currency_code_snapshot")]
    [MaxLength(3)]
    public string 통화코드Snapshot { get; set; } = "KRW";

    [Column("requested_delivery_at_utc")]
    public DateTime 희망납품일Utc { get; set; }

    [Column("delivery_destination_reference_key")]
    [MaxLength(200)]
    public string 납품지참조Key { get; set; } = string.Empty;

    [Column("status_code")]
    [MaxLength(50)]
    public string 상태코드 { get; set; } = 개별공급발주상태코드.공급자제출됨;

    [Column("platform_role_code")]
    [MaxLength(60)]
    public string 플랫폼역할코드 { get; set; } = 공급중개역할코드.개별발주중개;

    [Column("platform_is_seller")]
    public bool 플랫폼판매자여부 { get; set; }

    [Column("platform_is_reseller")]
    public bool 플랫폼재판매자여부 { get; set; }

    [Column("payment_executed")]
    public bool 결제실행됨 { get; set; }

    [Column("inventory_reserved")]
    public bool 재고예약됨 { get; set; }

    [Column("inbound_created")]
    public bool 입고생성됨 { get; set; }

    [Column("individual_order_confirmed")]
    public bool 개별발주확인 { get; set; }

    [Column("supplier_is_seller_confirmed")]
    public bool 공급자판매자확인 { get; set; }

    [Column("platform_is_broker_confirmed")]
    public bool 플랫폼중개자확인 { get; set; }

    [Column("guidance_version")]
    [MaxLength(32)]
    public string 안내버전 { get; set; } = string.Empty;

    [Column("supplier_response_evidence_reference")]
    [MaxLength(500)]
    public string? 공급자응답근거참조 { get; set; }

    [Column("supplier_response_recorded_by_user_id")]
    [MaxLength(450)]
    public string? 공급자응답기록자UserId { get; set; }

    [Column("submitted_at_utc")]
    public DateTime 제출시각Utc { get; set; }

    [Column("supplier_responded_at_utc")]
    public DateTime? 공급자응답시각Utc { get; set; }

    [Column("updated_at_utc")]
    public DateTime 수정시각Utc { get; set; }

    public 공급계약이용등록 공급계약이용등록 { get; set; } = null!;

    public 플랫폼공급조건계약 공급계약 { get; set; } = null!;

    public 플랫폼공급조건계약품목 공급계약품목 { get; set; } = null!;
}
