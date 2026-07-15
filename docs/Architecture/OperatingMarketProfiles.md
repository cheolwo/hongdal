# 운영 시장 프로필

## 목적

홍달의 한국 운영과 미국 운영은 앱이나 원장을 복제하지 않고, 공통 업무 흐름에 운영 시장 정책을 적용하는 방식으로 분리한다.

다음 네 축은 서로 독립적으로 유지한다.

- 표시 언어: 한국어 또는 영어
- 운영 시장: `KR` 또는 `US`
- 운송 구간: 국내 운송, 수입, 수출, 국가 간 운송
- 법적 역할: 소프트웨어 제공자, 화주, 운송사, 면허 보유 브로커 파트너

운영 시장을 바꿔도 표시 언어는 자동으로 바뀌지 않는다. 미국에서 한국어를 사용하거나 한국에서 영어를 사용하는 구성이 가능해야 한다.

## 공통 프로필

공통 기준은 `Hongdal.Contracts/Common/Operations/OperatingMarketProfiles.cs`에 둔다.

| 항목 | 한국 `KR` | 미국 `US` |
| --- | --- | --- |
| 통화 | KRW | USD |
| 거리 / 중량 | km / kg | mi / lb |
| 주소 기본값 | 행안부 도로명주소 | Google Address Validation |
| 지도 기본값 | Naver Maps | Google Maps |
| 판매채널 기본값 | SmartStore, Coupang, 11st | Amazon, eBay, Shopify, Walmart, Etsy, TikTok Shop |
| 화물 주선 기본값 | 국내 운송 업무 | 면허 보유 브로커 파트너 흐름 |

이 값들은 기능 활성화 여부가 아니라 시장별 기본 정책이다. 실제 외부 API 사용 가능 여부는 서버 설정과 자격 증명으로 별도 판단한다.

## 적용 규칙

1. 국가 분기는 화면과 서비스마다 직접 작성하지 않고 공통 프로필 또는 시장 정책을 조회한다.
2. 금액, 주소, 거리, 중량을 저장할 때는 값과 함께 통화·국가·단위 코드를 보존한다.
3. 한국 전용 DTO나 API에 연결된 화면은 미국 운영에서 그대로 재사용하지 않는다.
4. 미국 화물 주선 실행은 홍달 자체 권한을 가정하지 않고 면허 보유 브로커 파트너 흐름을 기본으로 한다.
5. 판매채널, 지도, 주소 제공자는 운영 시장의 기본값을 따르되 계정별 명시 설정으로 재정의할 수 있게 확장한다.

## 배포별 서비스 모듈

한국 서버와 미국 서버는 같은 공통 계약을 사용하지만, 시작 시 하나의 시장 모듈만 등록한다.

```text
공통 인터페이스
  ├─ KR 배포 -> KoreaOperatingMarketServiceModule
  └─ US 배포 -> UnitedStatesOperatingMarketServiceModule
```

배포 시장은 `OperatingMarket:MarketCode`로 설정한다. 환경 변수에서는 `OperatingMarket__MarketCode=KR` 또는 `OperatingMarket__MarketCode=US`를 사용한다. 설정이 없으면 한국 개발 환경과의 호환을 위해 `KR`을 사용하며, 지원하지 않는 값은 서버 시작 단계에서 오류로 처리한다.

`IOperatingMarketServiceModule`은 현재 다음 공통 인터페이스의 구현을 시장별로 등록한다.

| 공통 인터페이스 | 한국 모듈 | 미국 모듈 |
| --- | --- | --- |
| `IOperatingMarketAddressLookupAdapter` | 행안부 도로명주소 | Google Address Validation 연결 자리 |
| `IOperatingMarketFreightWorkflowPolicy` | 국내 운송 업무 정책 | 면허 보유 브로커 파트너 정책 |

주소 조회는 `IOperatingMarketAddressLookupService`를 공통 진입점으로 사용한다. 한 서버에는 해당 배포 시장의 주소 어댑터만 존재한다. 한국 서버에 `US` 주소 요청을 보내거나 미국 서버에 `KR` 주소 요청을 보내면 다른 제공자로 우회하지 않고 `MarketNotAvailableInDeployment`를 반환한다. 미국 주소 어댑터는 Google Address Validation을 실제로 연결하기 전까지 `ProviderNotConfigured`를 반환한다.

## 서버 컨텍스트

운영 시장은 요청 claim이나 header가 아니라 서버 배포 설정으로 고정한다. `IOperatingMarketContextAccessor`는 `Deployment` 출처와 함께 그 시장만 반환한다. 따라서 사용자가 보낸 `X-Hongdal-Operating-Market` 값으로 한국 서버가 미국 서비스처럼 동작하거나 그 반대가 되는 경로는 열지 않는다.

표시 언어와 계정별 선호 시장은 별도 사용자 설정으로 유지할 수 있지만, 서버의 구현 모듈 선택 권한은 갖지 않는다. 이 구분은 뷰의 언어 선택과 실제 운영·법률·외부 API 경계를 분리하기 위한 것이다.

## 미국 화물 주선 경계

미국 모듈의 `IOperatingMarketFreightWorkflowPolicy`는 소프트웨어 작업 공간만 사용하는 흐름과 실제 운송 주선을 요청하는 흐름을 구분한다. 실제 운송 주선을 요청할 때 서버 배포 설정에 `VerifiedLicensedBrokerPartnerId`가 없으면 `VerifiedLicensedBrokerPartnerRequired`로 중단한다. 이 값은 클라이언트 요청에서 받지 않으며, 검증 절차를 마친 뒤 미국 서버의 `OperatingMarket__VerifiedLicensedBrokerPartnerId` 환경 변수로만 공급한다.

이 정책은 면허를 검증하는 기능 자체가 아니라, 검증된 파트너가 없는 실행을 막는 서버 측 경계다. 현재 주 화물 운송 의뢰 생성과 일괄 등록은 이 정책을 통과하며, 미국 배포의 기본 상태에서는 실제 의뢰 생성이 중단된다. 파트너 ID를 배포 설정에 넣기 전에 FMCSA 등록 상태, 계약 유효성, 보증과 보험 등 필요한 검증을 수행하는 별도 파트너 등록 모듈이 추가되어야 한다.

## 현재 적용 범위

현재는 공통 계약의 `KR` / `US` 프로필, 배포별 단일 모듈 선택, 시장별 주소 어댑터, 주 화물 운송 의뢰 생성에 적용된 미국 운송 주선 실행 경계, `km` / `mi` 및 `kg` / `lb` 변환기까지 적용되어 있다. 기존 HongdalApp 화면, 메뉴, 로컬 프로필 저장 방식은 변경하지 않는다.

아직 모든 기존 서비스를 시장 모듈로 옮긴 것은 아니다. Naver, Toss, 국내 공공데이터처럼 기존 공통 DI에 등록된 한국 중심 서비스는 해당 업무를 리팩터링할 때 모듈 경계 안으로 단계적으로 이동해야 한다.

다음 구현 범위는 Google Address Validation 실제 연결, 검증된 미국 브로커 파트너 등록부, 파트너 계약·배차·정산 워크플로우다. 그 뒤 별도 요청에 따라 뷰가 배포 프로필을 읽도록 연결한다.
