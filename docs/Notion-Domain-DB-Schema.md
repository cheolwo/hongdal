# Ssalddel Domain 기반 Notion DB 스키마 문서

이 문서는 `Ssalddel/Domain` 엔티티를 기준으로 Notion Database를 동일 구조로 수동 생성할 때 바로 복붙해서 쓸 수 있게 정리한 문서입니다.

## 1) 공통 생성 규칙

- DB 이름은 **C# `[Table("...")]` 값** 기준으로 생성
- `id`는 Notion 기본 `ID`가 아니라 **별도 Number 속성**으로 생성 (백엔드 PK 동기화 용도)
- 시간 컬럼(`*_at`, `*_time`)은 Notion `Date` 타입
- 문자열 컬럼은 기본 `Text` 타입
- 금액/거리/점수는 `Number` 타입(필요 시 소수 허용)
- 상태값 컬럼은 `Select` 권장 (허용값은 아래 상태값 섹션 참고)

---

## 2) 상태값(Select 옵션) 기준

### 기사운행상태
- 대기
- 운행중

### 배차대기상태
- 대기
- 확정

### 의뢰상태
- 생성됨

### 결제상태
- 결제대기
- 결제완료
- 환불됨

### 배차상태
- 미시작
- 대기
- 매칭중

### 운송방식(화주운송의뢰.transport_type)
- 혼적
- 독차

---

## 3) 테이블별 스키마

## 3.1 `배달기사`

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| notion_page_id | string | Text | Y | "" |
| 기사명 | string | Title | Y | "" |
| 기사Id | string | Text | Y | "" |
| 상태 | string | Select | Y | 활동중 |
| 연락처 | string | Text | Y | "" |
| 차량 | string | Select/Text | Y | 오토바이 |
| 운행상태 | string | Select | Y | 대기 |
| 주_활동지역 | string | Text | Y | "" |
| 메모 | string | Text | Y | "" |
| 등록일 | DateTime? | Date | N | null |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.2 `driver_shifts` (기사근무)

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| driver_id | string | Text | Y | "" |
| start_mode | string | Select | Y | "" (immediate/reserved 사용 권장) |
| started_at | DateTime? | Date | N | null |
| start_location | string | Text | Y | "" |
| return_destination | string? | Text | N | null |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.3 `driver_location_history` (기사위치기록)

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| driver_id | string | Text | Y | "" |
| latitude | decimal | Number | Y | - |
| longitude | decimal | Number | Y | - |
| accuracy_m | decimal? | Number | N | null |
| recorded_at | DateTime | Date | Y | now |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.4 `기사월정산`

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| driver_id | string | Text | Y | "" |
| year | int | Number | Y | - |
| month | int | Number | Y | - |
| dispatch_count | int | Number | Y | 0 |
| usage_fee | decimal | Number | Y | 0 |
| is_paid | bool | Checkbox | Y | false |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.5 `shipper_requests` (화주운송의뢰)

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| request_id | string | Title/Text | Y | "" |
| shipper_id | string | Text | Y | "" |
| cargo_type | string | Text | Y | "" |
| cargo_description | string | Text | Y | "" |
| cargo_quantity | int? | Number | N | null |
| cargo_weight_kg | decimal? | Number | N | null |
| cargo_volume_cbm | decimal? | Number | N | null |
| cargo_fragile | bool | Checkbox | Y | false |
| cargo_temperature | string | Select/Text | Y | 상온 |
| transport_type | string | Select | Y | 혼적 |
| pricing_config_id | long? | Number | N | null |
| pickup_address | string | Text | Y | "" |
| pickup_address_detail | string | Text | Y | "" |
| pickup_latitude | decimal? | Number | N | null |
| pickup_longitude | decimal? | Number | N | null |
| pickup_contact_name | string | Text | Y | "" |
| pickup_contact_phone | string | Text | Y | "" |
| pickup_window_start | DateTime | Date | Y | - |
| pickup_window_end | DateTime | Date | Y | - |
| dropoff_address | string | Text | Y | "" |
| dropoff_address_detail | string | Text | Y | "" |
| dropoff_latitude | decimal? | Number | N | null |
| dropoff_longitude | decimal? | Number | N | null |
| dropoff_contact_name | string | Text | Y | "" |
| dropoff_contact_phone | string | Text | Y | "" |
| dropoff_window_start | DateTime? | Date | N | null |
| dropoff_window_end | DateTime? | Date | N | null |
| service_level | string | Select/Text | Y | "" |
| request_text | string | Text | Y | "" |
| waiting_fee | decimal? | Number | N | null |
| manual_fee | decimal? | Number | N | null |
| surcharge | decimal? | Number | N | null |
| final_fare | decimal? | Number | N | null |
| client_request_id | string | Text | Y | "" |
| status | string | Select | Y | 생성됨 |
| payment_status | string | Select | Y | 결제대기 |
| dispatch_status | string | Select | Y | 미시작 |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.6 배차 대기 필드 (`운송원장` 통합)

`배차_대기`는 별도 테이블로 유지하지 않고 `운송원장`에 통합한다. 아래 필드는 운송원장이 배차 대기, 추천 잠금, 공개배차, 확정 상태를 처리할 때 쓰는 확장 필드다.

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| request_id | string | Title/Text | Y | "" |
| shipper_id | string | Text | Y | "" |
| pickup_address | string | Text | Y | "" |
| pickup_address_detail | string | Text | Y | "" |
| pickup_latitude | decimal? | Number | N | null |
| pickup_longitude | decimal? | Number | N | null |
| dropoff_address | string | Text | Y | "" |
| dropoff_address_detail | string | Text | Y | "" |
| dropoff_latitude | decimal? | Number | N | null |
| dropoff_longitude | decimal? | Number | N | null |
| status | string | Select | Y | 대기 |
| business_type | int | Select/Number | Y | 용달운송 |
| source_type | string | Select/Text | Y | "" |
| source_request_id | string | Text | Y | "" |
| queue_stage | int | Select/Number | Y | 계획배차 |
| exposure_state | int | Select/Number | Y | 계획대기 |
| current_recommended_driver_id | string? | Text | N | null |
| recommendation_started_at | DateTime? | Date | N | null |
| recommendation_expires_at | DateTime? | Date | N | null |
| recommendation_round | int | Number | Y | 0 |
| confirmed_driver_id | string? | Text | N | null |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.7 `기사배차`

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| notion_page_id | string | Text | Y | "" |
| 배차Id | long? | Number | N | null |
| 배차명 | string | Title/Text | Y | "" |
| 상태 | string | Select | Y | 배차대기 |
| 배차일 | DateTime? | Date | N | null |
| 배달기사_id | long? | Number | N | null |
| 픽업지 | string | Text | Y | "" |
| 배송지 | string | Text | Y | "" |
| 기본요금 | long? | Number | N | null |
| 거리추가_요금 | long? | Number | N | null |
| 주문Id | long? | Number | N | null |
| 기사Id | long? | Number | N | null |
| 잠금여부 | bool | Checkbox | Y | false |
| 잠금시각 | DateTime? | Date | N | null |
| 시도횟수 | int? | Number | N | null |
| 픽업거리_m | int? | Number | N | null |
| 픽업예상시간_sec | int? | Number | N | null |
| 배차점수 | decimal? | Number | N | null |
| 실패사유 | string | Text | Y | "" |
| 메모 | string | Text | Y | "" |
| 배차생성시각 | DateTime? | Date | N | null |
| 배차완료시각 | DateTime? | Date | N | null |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.8 `운송원장`

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| 운송번호 | string | Title/Text | Y | "" |
| 상태 | string | Select | Y | 배차대기 |
| 출발_픽업 | DateTime? | Date | N | null |
| 도착 | DateTime? | Date | N | null |
| 기사_운송자 | string | Text | Y | "" |
| 출발지 | string | Text | Y | "" |
| 도착지 | string | Text | Y | "" |
| 운임 | decimal? | Number | N | null |
| 첨부_json | string | Text | Y | [] |
| 메모 | string | Text | Y | "" |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.9 `운송이벤트`

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| request_id | string | Text | Y | "" |
| event_type | string | Select/Text | Y | "" |
| event_time | DateTime | Date | Y | now |
| metadata | string | Text | Y | "" |

## 3.10 `운임구성`

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| request_id | string | Text | Y | "" |
| 기본운임 | decimal | Number | Y | 0 |
| 거리운임 | decimal | Number | Y | 0 |
| 할증 | decimal | Number | Y | 0 |
| 대기료 | decimal | Number | Y | 0 |
| 수작업비 | decimal | Number | Y | 0 |
| 최종운임 | decimal | Number | Y | 0 |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.11 `차량단가`

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| 차량종류 | string | Title/Select | Y | "" |
| 기본운임 | decimal | Number | Y | 0 |
| Km당단가 | decimal | Number | Y | 0 |
| 야간할증 | decimal | Number | Y | 0 |
| 우천할증 | decimal | Number | Y | 0 |
| 최소운임 | decimal | Number | Y | 0 |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.12 `업체`

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| notion_page_id | string | Text | Y | "" |
| 업체명 | string | Title | Y | "" |
| 상태 | string | Select | Y | 거래중 |
| 대표_연락처 | string | Text | Y | "" |
| 담당자 | string | Text | Y | "" |
| 이메일 | string | Email/Text | Y | "" |
| 주소 | string | Text | Y | "" |
| 정산_결제_조건 | string | Text | Y | "" |
| 첨부_json | string | Text | Y | [] |
| 메모 | string | Text | Y | "" |
| 등록일 | DateTime? | Date | N | null |
| created_at | DateTime | Date | Y | now |
| updated_at | DateTime | Date | Y | now |

## 3.13 `결제`

| 컬럼명 | C# 타입 | Notion 타입 권장 | 필수 | 기본값 |
|---|---|---|---|---|
| id | long | Number | Y | - |
| payment_id | string | Title/Text | Y | "" |
| request_id | string | Text | Y | "" |
| shipper_id | string | Text | Y | "" |
| pg_provider | string | Select/Text | Y | TossPayments |
| payment_method | string | Select/Text | Y | 미정 |
| payment_status | string | Select | Y | 결제대기 |
| amount | int | Number | Y | 0 |
| order_id | string | Text | Y | "" |
| payment_key | string? | Text | N | null |
| toss_response_json | string? | Text | N | null |
| created_at | DateTime | Date | Y | now |
| approved_at | DateTime? | Date | N | null |

---

## 4) 빠른 생성 체크리스트

1. 위 순서대로 DB 생성
2. 각 DB의 `id` Number 컬럼 생성
3. 상태 컬럼은 Select로 만들고 상태값 옵션 등록
4. 날짜 컬럼은 모두 Date로 생성
5. `request_id`, `payment_id`, `운송번호`, `기사명`, `업체명` 등은 조회 편의를 위해 타이틀/주요 표시 컬럼으로 설정

---

## 5) 참고 소스

- `Ssalddel/Domain/**/*.cs`
- `Ssalddel/Domain/공통/상태값.cs`
