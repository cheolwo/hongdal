# 운영 시장 프로필

## 목적

홍달의 한국 운영과 미국 운영은 앱이나 원장을 복제하지 않고, 공통 업무 흐름에 배포별 운영 정책을 적용하는 방식으로 분리한다.

다음 네 축은 서로 독립적으로 유지한다.

- 표시 언어: 현재 `ko-KR` 또는 `en-US`
- 운영 시장: 현재 `KR` 또는 `US`
- 운송 구간: 국내 운송, 수입, 수출, 국가 간 운송
- 법적 역할: 공동행동 촉진 플랫폼, 화주, 운송사, 허가 또는 권한을 확인한 전문 서비스 참여자

운영 시장을 바꿔도 표시 언어는 자동으로 바뀌지 않는다. 미국 서버를 한국어로 사용하거나 한국 서버를 영어로 사용하는 구성이 가능해야 한다.

## 플랫폼 운영자의 역할

홍달의 기본 역할 코드는 `CollectiveActionFacilitator`다. 운영자는 운송 주선업자가 되는 것을 제품의 전제로 삼지 않고, 사람들이 수요·의사·정보를 모아 공동구매, 공동수입, 수출입 준비를 시작할 수 있게 돕는다.

업무는 다음 세 단계로 구분한다.

1. `CommunityIntentCoordination`: 참여 의사, 수량, 조건, 쟁점과 정보를 모은다.
2. `QualifiedProviderParticipationRequest`: 운송주선업자, 운송사, 통관사 등 자격 사업자가 제안할 수 있도록 업무 요청을 게시한다.
3. `RegulatedTransportationArrangement`: 계약, 운송사 선정, 주선 실행처럼 자격과 책임이 필요한 행위를 수행한다.

앞의 두 단계는 플랫폼의 촉진 업무다. 세 번째 단계의 책임 코드는 `ParticipatingQualifiedServiceProvider`이며, 참여한 자격 사업자가 자기 명의와 책임으로 수행해야 한다.

배차에서도 추천과 확정을 분리한다. `CollectiveActionDispatchBoundaryPolicy`는 결정 출처를 다음과 같이 해석한다.

- `PlatformCandidateInformation`: 거리, 시간, 조건과 후보 정보를 제공하지만 배차를 확정할 수 없다.
- `ParticipatingDriverSelfAcceptance`: 인증된 기사와 선택된 기사가 같을 때 본인 수락으로 확정할 수 있다.
- `QualifiedServiceProviderConfirmation`: 현재 검증된 전문 사업자 참여자가 자기 책임으로 결정할 때만 확정할 수 있다.

알 수 없는 결정 출처는 `PlatformCandidateInformation`으로 처리해 확정을 차단한다. 추천 점수, 운영자 검토, AI 판단 사례도 그 자체로 기사 배정이나 운송계약 체결이 되지 않는다.

화면과 계약에서도 다음 경계를 유지한다.

- 홍달은 특정 운송사를 자기 명의로 확정하거나 운송계약의 당사자인 것처럼 표시하지 않는다.
- 전문 사업자의 제안, 자격 상태, 계약 주체, 수수료와 대금 수령 주체를 분리해 표시한다.
- 원장에는 공동 의사 형성 기록과 전문 사업자의 규제 업무 실행 기록을 별도 블록으로 남긴다.
- 커뮤니티 원장은 배차 후보와 진행 정보를 보여주지만 기사 확정과 운송 실행 상태를 RDB로 역전파하지 않는다. 참여자 실행 경로에서 확정된 결과가 RDB에 기록된 뒤 원장으로 동기화된다.
- 플랫폼 명칭만으로 법적 지위가 결정되는 것은 아니므로 실제 권한 행사, 계약과 대금 흐름도 이 경계를 따라야 한다.

## 공통 프로필

공통 기준은 `Hongdal.Contracts/Common/Operations/OperatingMarketProfiles.cs`에 둔다.

| 항목 | 한국 `KR` | 미국 `US` |
| --- | --- | --- |
| 통화 | KRW | USD |
| 거리 / 중량 | km / kg | mi / lb |
| 배포 기본 시간대 | Asia/Seoul | UTC, 배포 설정으로 지역 시간대 지정 |
| 주소 기본값 | 행안부 도로명주소 | Google Address Validation |
| 지도 기본값 | Naver Maps | Google Maps |
| 판매채널 기본값 | SmartStore, Coupang, 11st | Amazon, eBay, Shopify, Walmart, Etsy, TikTok Shop |
| 화물 주선 기본값 | 국내 운송 업무 | 권한을 확인한 브로커 파트너 흐름 |

표시 언어 코드는 `Hongdal.Contracts/Common/Localization/DisplayLanguageCodes.cs`에 별도로 둔다. 시장 프로필의 `FormattingCultureName`은 금액과 날짜의 기본 형식일 뿐 사용자가 선택한 표시 언어가 아니다.

## 적용 규칙

1. 국가 분기는 화면과 서비스마다 직접 작성하지 않고 공통 프로필 또는 시장 정책을 조회한다.
2. 금액, 주소, 거리, 중량을 저장할 때는 값과 함께 통화·국가·단위 코드를 보존한다.
3. 한국 전용 DTO나 외부 API를 미국 운영에서 암묵적으로 재사용하지 않는다.
4. 클라이언트가 보낸 국가 값으로 서버의 법률·외부 API 모듈을 바꾸지 않는다.
5. 규제 검증은 단순 참여자 ID가 아니라 참여 역할, 권한 참조, 확인 시각, 만료 시각, 요구사항별 확인 결과로 기록한다.

## 배포별 서비스 모듈

한국 서버와 미국 서버는 같은 공통 계약을 사용하지만 시작 시 하나의 시장 모듈만 등록한다.

```text
공통 인터페이스
  +-- KR 배포 -> KoreaOperatingMarketServiceModule
  +-- US 배포 -> UnitedStatesOperatingMarketServiceModule
```

배포 시장은 `OperatingMarket:MarketCode`로 설정한다. 환경 변수에서는 `OperatingMarket__MarketCode=KR` 또는 `OperatingMarket__MarketCode=US`를 사용한다. 지원하지 않는 값은 서버 시작 단계에서 오류로 처리한다.

| 공통 인터페이스 | 한국 모듈 | 미국 모듈 |
| --- | --- | --- |
| `IOperatingMarketAddressLookupAdapter` | 행안부 도로명주소 | Google Address Validation 연결 자리 |
| `IOperatingMarketFreightWorkflowPolicy` | 국내 전문 사업자 허가 점검 메타데이터 | 참여 브로커의 권한 강제 검증 |

주소 조회는 `IOperatingMarketAddressLookupService`를 공통 진입점으로 사용한다. 한 서버에는 해당 배포 시장의 주소 어댑터만 존재한다. 다른 시장의 주소 요청은 제공자 우회 없이 `MarketNotAvailableInDeployment`로 거절한다.

## 서버 컨텍스트와 클라이언트 인터페이스

운영 시장은 요청 claim이나 header가 아니라 서버 배포 설정으로 고정한다. `IOperatingMarketContextAccessor`는 시장과 배포 시간대를 반환한다.

클라이언트는 다음 공개 API로 현재 서버의 비민감 운영 정보를 읽는다.

```http
GET /api/v1/operations/market-profile
```

응답에는 국가, 통화, 형식 문화권, 시간대, 거리·중량 단위, 주소·지도 제공자, 지원 표시 언어, 플랫폼 역할, 공동 의사 형성 가능 여부, 전문 사업자 참여 요청 가능 여부, 규제 업무 실행 가능 여부가 포함된다. `PlatformCanConfirmDispatch`는 항상 `false`이고, 지원하는 참여자 결정 출처를 별도로 제공한다. 참여자 ID와 권한 번호는 포함하지 않는다.

## 화물 주선 규제 경계

`OperatingMarketFreightComplianceProfileCatalog`은 법률 판정을 대신하지 않고 서버가 확인해야 할 항목과 공식 근거 코드를 제공한다.

| 시장 | 집행 모드 | 확인 항목 |
| --- | --- | --- |
| 한국 | `AuditOnly` | 전문 사업자 참여자와 역할, 허가 참조, 검증 유효기간, 운송주선사업 허가 상태 |
| 미국 | `Required` | 전문 사업자 참여자와 역할, MC 등 권한 참조, 검증 유효기간, broker authority, financial security, process-agent designation |

한국의 `AuditOnly`는 기존 국내 흐름을 즉시 중단하지 않기 위한 전환 상태다. 따라서 `Allowed` 결과가 법적 허가를 확인했다는 뜻은 아니다. 실제 주선 운영 전에는 전문 사업자 허가 검증 저장소와 정기 재검증 절차를 연결하고 집행 모드를 강화해야 한다.

미국에서도 공동 의사 형성과 전문 사업자 참여 요청은 자격 사업자 없이 사용할 수 있다. 다만 참여 브로커가 실제 운송 주선을 수행하는 단계는 모든 확인 항목이 현재 유효해야 한다. 참여자 ID만 있는 기존 `VerifiedLicensedBrokerPartnerId` 설정은 호환 목적으로 읽지만, 참여 역할·권한·유효기간·요구사항 증거가 없으므로 규제 업무 실행을 허용하지 않는다.

배포 설정 예시는 다음과 같다.

```json
{
  "OperatingMarket": {
    "MarketCode": "US",
    "TimeZoneId": "America/Chicago",
    "FreightServiceProvider": {
      "ParticipantId": "configured-outside-source-control",
      "ParticipantRoleCode": "US.PropertyBroker",
      "AuthorityReference": "verified-authority-reference",
      "VerifiedAtUtc": "2026-07-01T00:00:00Z",
      "VerificationExpiresAtUtc": "2026-10-01T00:00:00Z",
      "SatisfiedRequirementCodes": [
        "US.BrokerAuthority.Active",
        "US.FinancialSecurity.Active",
        "US.ProcessAgentDesignation.Active"
      ]
    }
  }
}
```

운영 값은 저장소의 `appsettings.json`에 넣지 않고 배포 비밀 또는 별도 보호 설정에서 공급한다.

## 공식 근거

2026-07-17 확인 기준이다. 법령과 행정 요구는 변경될 수 있으므로 운영 전 다시 확인해야 한다.

- 한국: [화물자동차 운수사업법 제24조, 국가법령정보센터](https://www.law.go.kr/LSW/lsInfoP.do?chrClsCd=010202&lsId=&lsiSeq=286393&urlMode=lsInfoP)
- 미국: [FMCSA Broker Registration](https://www.fmcsa.dot.gov/registration/broker-registration)
- 미국: [FMCSA의 motor carrier, broker, freight forwarder 정의](https://www.fmcsa.dot.gov/faq/what-are-definitions-motor-carrier-broker-and-freight-forwarder-authorities)
- 미국: [FMCSA Broker and Freight Forwarder Financial Responsibility](https://www.fmcsa.dot.gov/registration/broker-and-freight-forwarder-financial-responsibility-rule-overview-and-compliance)
- 미국: [FMCSA Form BOC-3](https://www.fmcsa.dot.gov/registration/form-boc-3-designation-agents-service-process)

이 문서와 정책 코드는 법률 자문이나 자동 면허 판정이 아니다. 실제 서비스 출시 전 해당 관할의 변호사 또는 규제 전문가와 계약·운영 구조를 검토해야 한다.

## 현재 적용 범위

현재 적용된 범위는 다음과 같다.

- `KR` / `US` 공통 운영 프로필과 배포별 단일 서비스 모듈
- 표시 언어와 운영 시장의 독립 계약
- 시장별 주소 어댑터와 단위 변환
- 배포 시간대와 공개 런타임 프로필 API
- 운송 의뢰 생성을 `QualifiedProviderParticipationRequest`로 분류한 촉진 업무 경계
- 전문 사업자 참여 역할, 검증 상태, 만료, 요구사항별 증거 모델
- 플랫폼 후보 정보와 기사 본인 수락·전문 사업자 결정을 구분하는 배차 확정 경계
- 커뮤니티 원장에서 RDB 기사 확정·운송 실행 상태로의 역투영 차단

아직 남은 범위는 Google Address Validation 실제 연결, 전문 사업자 권한을 공식 데이터에서 주기적으로 재검증하는 저장소, 전문 사업자가 확정하는 별도 명령에서 `RegulatedTransportationArrangement`를 강제하는 연결, 한국 집행 모드 강화, 국가별 결제·정산 제공자 분리다. 기존 Naver, Toss, 국내 공공데이터처럼 한국 중심 서비스는 해당 업무를 리팩터링할 때 시장 모듈 경계 안으로 단계적으로 이동한다.
