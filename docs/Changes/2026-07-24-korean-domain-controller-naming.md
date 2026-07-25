# Controller 업무 용어 한국어화

| 항목 | 내용 |
| --- | --- |
| 날짜 | 2026-07-24 |
| 커밋 | 미커밋 |
| 변경 축 | 기술 역할 영어 유지, 업무 용어 한국어화, API metadata 호환 |
| 화면 변경 | 화면 없음 |
| 시각 증거 | 해당 없음 |

## 명명 기준

- `Controller`, `UseCase`, `Service`, `DTO`, `Command`, `Event`, `Handler`는 영어로 유지한다.
- 공동구매, 생산자연결, 협의, 이행계획, 차량추천, 공공데이터조회와 음식점탐색은
  한국어로 표현한다.
- Route, query 이름, JSON 계약과 기존 원장 `EndpointKey`는 변경하지 않는다.

## 주문자 적용

| 이전 코드 이름 | 변경 코드 이름 |
| --- | --- |
| `DomesticGroupPurchaseNegotiationsController` | `국내공동구매협의Controller` |
| `DomesticGroupPurchaseProducerConnectionsController` | `국내공동구매생산자연결Controller` |
| `DomesticGroupPurchaseFulfillmentPlansController` | `국내공동구매이행계획Controller` |
| `DomesticGroupPurchaseVehicleRecommendationsController` | `국내공동구매차량추천Controller` |
| `PublicDataLookupController` | `공공데이터조회Controller` |
| `RestaurantSearchPolicyPublicController` | `음식점탐색공개정책Controller` |

각 공개 동작도 `협의이력조회`, `발주초안생성`, `생산자후보검색`,
`공급적합성미리보기`, `공동주택단지검색`처럼 업무 의미를 한국어로 바꿨다.
기존 한국어 Controller에 남아 있던 `List`, `Get`, `Create`, `CastVote`, `Lookup`,
`Convert`, `Resolve`도 `목록조회`, `상세조회`, `생성`, `투표`,
`문서관리번호조회`, `원장전환`, `물류흐름결정`으로 정리했다. 주입 필드는
`_useCase`, `_service`, `_store`처럼 책임을 알 수 없는 이름 대신
`_자동집단화UseCase`, `_생산자연결Service`, `_이행계획Store`처럼 업무 의미와
영어 기술 접미사를 함께 사용한다.

## 기사·화주·창고 적용

- 화주 Controller와 대부분의 기사 Controller는 이미 한국어 업무명을 사용하고 있어
  유지했다.
- 남아 있던 `FoodDeliveryDriverController`와 영문 action은
  `음식배달기사업무Controller`, `업무공간조회`, `제안수락`, `픽업완료`,
  `경로조회` 등으로 변경했다.
- 창고 진입점은 다음과 같이 변경했다.

| 이전 코드 이름 | 변경 코드 이름 |
| --- | --- |
| `WarehouseOperationsController` | `창고작업Controller` |
| `WarehousePerspectiveReadController` | `창고업무관점조회Controller` |
| `LoadingPerspectiveReadController` | `상차업무관점조회Controller` |
| `UnloadingPerspectiveReadController` | `하차업무관점조회Controller` |

## Common·Admin 적용

`Common`에서는 커뮤니티, 공동조달, 농수산정보, 주문 관점, 인사 지원,
육류 수입 준비, 판매채널, 전통시장, 비자 지원과 업무 관계 Controller를
한국어 업무명으로 변경했다. 대표적인 변경은 다음과 같다.

| 이전 코드 이름 | 변경 코드 이름 |
| --- | --- |
| `AgriculturalFisheriesInformationController` | `농수산정보Controller` |
| `CommunityPostOpportunitiesController` | `커뮤니티게시글참여기회Controller` |
| `HrRoleApplicationsController` | `인사역할지원Controller` |
| `SalesChannelsController` | `판매채널Controller` |
| `WorkRelationshipSnapshotsController` | `업무관계SnapshotController` |

`Admin`에서는 콘텐츠, 배차 AI 검토, 인사, 정산과 전통시장 운영 Controller를
같은 기준으로 변경했다.

| 이전 코드 이름 | 변경 코드 이름 |
| --- | --- |
| `CommunityAuthoringImagesController` | `커뮤니티작성이미지Controller` |
| `DomesticCargoDispatchAIReviewController` | `국내화물배차AI검토Controller` |
| `HrEmploymentContractsController` | `고용계약Controller` |
| `SocialInsuranceFilingsController` | `사회보험신고Controller` |
| `PlatformProfitReturnsController` | `플랫폼이익환원Controller` |

`VersionFeatureFlagsController`, `MobilePushInstallationsController`,
`SampleImagesController`처럼 업무명이 아니라 기술 책임만 나타내는 Controller는
영어로 유지했다. `AI`, `Admin`, `Card`, `Archive`, `Event`, `YouTube` 같은 기술
역할과 고유명도 혼합 이름에서 원 표기를 유지한다.

`Orderer`, `Driver`, `Shipper`, `Warehouse`, `Common`, `Admin`의 대상 공개 action은
test에서 한국어 업무명 또는 허용된 기술 접두사 사용을 검사한다.

## 호환 경계

`SsalddelApiContractNameAttribute`는 코드 이름과 외부 metadata 식별자를 분리한다.
따라서 기존 `DomesticGroupPurchaseFulfillmentPlansController.CreateOrderDraft` 같은
`EndpointKey`와 원장 template 연결은 유지된다. 이 특성은 Route나 권한을 바꾸지 않는다.

## 검증

- API 업무 분류와 버전 metadata 테스트
- 기존 `EndpointKey`와 Route 유지 테스트
- 주문자·기사·화주·창고·공통·관리자 action 명명 규칙 테스트
- 영향 프로젝트 Fast·Task 검증
