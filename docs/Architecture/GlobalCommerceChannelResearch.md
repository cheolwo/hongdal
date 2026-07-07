# Global Commerce Channel Research

## 조사 기준

- 확인일: 2026-07-03
- 기준: 공식 개발자 문서 또는 공식 개발자 포털
- 목적: 홍달 화주 앱의 `판매상품 -> 채널출품` 구조에 붙일 해외 판매채널 후보를 정리한다.

## 1차 구현 후보

### Shopify

- 공식 문서: Shopify Admin GraphQL API
- 상품 모델: Product, Product Variant, Media, Inventory 관련 객체
- 인증/권한: Shopify app OAuth와 Admin API access scope
- 홍달 적합도: 높음
- 이유: 독립몰 계정 단위라 `판매채널계정` 모델과 잘 맞고, API 문서와 버전 관리가 안정적이다.

### Amazon

- 공식 문서: Amazon Selling Partner API
- 핵심 API: Listings Items API, Product Type Definitions API
- 인증/권한: SP-API app authorization, Login with Amazon, AWS SigV4
- 홍달 적합도: 높지만 난도 높음
- 이유: 글로벌 확장성은 가장 크지만 marketplaceId, sellerId, productType별 JSON Schema, 리전 처리까지 필요하다.

### eBay

- 공식 문서: eBay Sell Inventory API
- 출품 흐름: inventory item 생성, offer 생성, offer publish
- 인증/권한: OAuth 2.0
- 홍달 적합도: 높음
- 이유: `판매상품 -> 채널출품` 모델을 `inventoryItem -> offer -> publish` 흐름에 대응하기 쉽다.

## 후속 후보

### Walmart Marketplace

- 공식 문서: Walmart Marketplace APIs
- API 영역: item setup, inventory, orders, pricing
- 특징: 미국 마켓플레이스 확장에 중요하지만 셀러 온보딩과 자격 요건 확인이 먼저 필요하다.

### Etsy

- 공식 문서: Etsy Open API v3
- API 영역: listings, inventory, orders, shop management
- 특징: 수공예/디자인/소규모 브랜드 상품에 적합하다.

### TikTok Shop

- 공식 문서: TikTok Shop Partner Center Open API
- API 영역: product, category/attribute, media, order, fulfillment
- 특징: 콘텐츠 기반 판매 채널로 유망하지만 국가별 지원 범위와 권한 확인이 필요하다.

### Shopee

- 공식 문서: Shopee Open Platform
- API 영역: product, order, shop, listing, marketing
- 특징: 동남아/대만 진출 후보로 중요하다.

### Lazada

- 공식 문서: Lazada Open Platform
- API 영역: Product API, Cross Border Product API, Seller API, Order API
- 특징: Shopee와 함께 동남아 판매 확장 후보로 볼 수 있다.

## 코드 반영 범위

현재 반영된 내용은 실제 외부 호출이 아니라 채널 카탈로그와 payload 초안 생성 계층이다.

- `CommerceChannelKeys`: 해외 채널 키 추가
- `CommerceChannelCatalog`: Shopify, Amazon, eBay, Walmart, Etsy, TikTok Shop, Shopee, Lazada 추가
- `ShopifyProductPayloadBuilder`: Shopify 상품 생성 payload 초안
- `AmazonSpApiProductPayloadBuilder`: Amazon SP-API Listings Items payload 초안
- `AmazonExportReadinessPlanner`: Amazon 출품 전에 수입 참여자 자격, 국내 물류 이력, 후기 사용 동의, 이미지형 상세페이지/광고 소재, 수출 HS 검토, 관세사 수임/비용, 서류, 재고 예약, 출고 배치, 국제배송, 반품/정산 정책을 확인하는 골격
- `EbayInventoryProductPayloadBuilder`: eBay inventory item/offer workflow payload 초안

실제 API 클라이언트 구현은 다음 순서가 적절하다.

1. Shopify Admin GraphQL client
2. eBay OAuth + Inventory API client
3. Amazon SP-API auth + Listings Items client
