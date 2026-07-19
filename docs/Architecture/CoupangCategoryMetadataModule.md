# Coupang Category Metadata Module

## 목적

살뜰은 쿠팡 판매자 Open API 키가 없는 단계에서도 쿠팡 카테고리 연동을 준비할 수 있어야 한다.
따라서 실제 API 호출보다 먼저 공식 문서의 경로, 요청 본문, 응답 형식을 공유 계약 DTO로 정리한다.

이 모듈은 쿠팡 파트너스 수익화 링크와 직접 결합하지 않는다. 파트너스 링크는 운영비 후원 링크 카탈로그에서 다루고, 이 문서는 판매자 상품 등록에 필요한 쿠팡 카테고리 메타데이터 준비 범위만 다룬다.

## 공식 API 표면

현재 계약으로 고정한 API 표면은 다음 네 가지다.

| 용도 | Method | Path | 요청 본문 | 응답 DTO |
| --- | --- | --- | --- | --- |
| 전체 노출 카테고리 목록 | GET | `/v2/providers/seller_api/apis/api/v1/marketplace/meta/display-categories` | 없음 | `CoupangDisplayCategoryTreeResponse` |
| 특정 카테고리 하위 조회 | GET | `/v2/providers/seller_api/apis/api/v1/marketplace/meta/display-categories/{displayCategoryCode}` | 없음 | `CoupangDisplayCategoryTreeResponse` |
| 카테고리 메타정보 조회 | GET | `/v2/providers/seller_api/apis/api/v1/marketplace/meta/category-related-metas/display-category-codes/{displayCategoryCode}` | 없음 | `CoupangCategoryMetaResponse` |
| 상품명 기반 카테고리 추천 | POST | `/v2/providers/openapi/apis/api/v1/categorization/predict` | `CoupangCategoryPredictionRequest` | `CoupangCategoryPredictionResponse` |

## DTO 경계

DTO는 `Ssalddel.Contracts.Common.Sales` 안에 둔다.

- `CoupangCategoryApiContractCatalog`: 엔드포인트 키, HTTP 메서드, 경로, 요청/응답 계약명, 공식 문서 URL
- `CoupangDisplayCategoryTreeResponse`: 쿠팡 노출 카테고리 트리 응답
- `CoupangDisplayCategoryNodeDto`: 카테고리 코드, 이름, 상태, 하위 카테고리
- `CoupangCategoryMetaResponse`: 카테고리별 상품 등록 메타정보 응답
- `CoupangCategoryMetaDataDto`: 단일상품 가능 여부, 옵션, 상품고시, 구비서류, 인증정보, 허용 상품 상태
- `CoupangCategoryPredictionRequest`: 상품명, 상세설명, 브랜드, 속성, 판매자 SKU
- `CoupangCategoryPredictionResponse`: 추천 결과 타입, 추천 카테고리 코드, 추천 카테고리명

쿠팡 문서 예시에는 `displayCategoryCode`와 `displayItemCategoryCode`가 함께 나타난다. 살뜰 DTO는 둘을 모두 보관하고, 내부 사용 시 `EffectiveDisplayCategoryCode`로 하나의 카테고리 코드처럼 다룬다.

## 키 없는 단계에서 가능한 일

- 공식 문서 기반 DTO와 엔드포인트 메타데이터 정리
- 수동 JSON/엑셀 import를 위한 카테고리 캐시 테이블 설계
- 상품명 기반 카테고리 추천 요청 화면의 입력 항목 설계
- 카테고리 메타정보를 원장 블록, 판매채널 블록, 상품 등록 준비 화면에서 참조할 수 있는 구조 설계

## 키가 생긴 뒤 붙일 일

- WING Open API Access Key, Secret Key, VendorId 저장소
- HMAC Authorization 생성기
- 카테고리 목록/하위 카테고리/메타정보/추천 API HTTP client
- 동기화 작업: 전체 카테고리 스냅샷, 변경 감지, 비활성 카테고리 보관
- 판매 상품 등록 payload mapper: 카테고리 메타정보의 필수 옵션, 상품고시, 인증정보를 상품 등록 초안에 반영

## 참고 문서

- https://developers.coupangcorp.com/hc/en-us/articles/360033400814-How-to-get-category-list
- https://developers.coupangcorp.com/hc/en-us/articles/360034035753-How-to-get-categories
- https://developers.coupangcorp.com/hc/ko/articles/360034035713-%EC%B9%B4%ED%85%8C%EA%B3%A0%EB%A6%AC-%EB%A9%94%ED%83%80%EC%A0%95%EB%B3%B4-%EC%A1%B0%ED%9A%8C
- https://developers.coupangcorp.com/hc/ko/articles/360033509234-%EC%B9%B4%ED%85%8C%EA%B3%A0%EB%A6%AC-%EC%B6%94%EC%B2%9C
