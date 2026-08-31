# 기존 자료 체계 재사용 재고

판본 `game-data-research.reuse-inventory.r1`. 2026-08-30 공유 저장소 소스 정적 확인. 경로 기준은 `C:/Users/user/source/repos/Hongdal`; 아래 파일 hash는 [baseline](baseline.r1.json)에 있다. 실제 운영 자료 현황이나 수집 실행 성공을 의미하지 않는다.

| 책임 | 기존 파일 후보 | 확인한 재사용 경계 / 이번 상태 |
| --- | --- | --- |
| API 목록 | `Ssalddel/Services/External/PublicData/PublicDataApiMetadataCatalog.cs` | 기존 API 메타데이터 시작점. 모든 Farm 공급자가 이 파일에 직접 등록됐다고 가정하지 않음 |
| 공통 Source Catalog | `Ssalddel/Services/External/PublicData/Foundation/ExternalDataSourceCatalog.cs` | 기존 API 어댑터 + IExternalDataSourceRegistration. SourceId/DatasetId 중복 거부, 기본 수집 비활성, RedistributionAllowed=false |
| Farm 공급자 등록 | `Ssalddel/Services/External/PublicData/Agriculture/FarmRealityDataSources.cs` | 농사로 일정/재해, KAMIS, AMS 참조 정의가 이미 있음. 별도 중복 등록 금지. 공통 ApiKeyQuery 표기는 AMS 실제 Basic 인증과 다르므로 실행 클라이언트를 우선 확인 |
| 공통 수집 실행 | `Ssalddel/Services/External/PublicData/Foundation/ExternalDataIngestionRuntime.cs` | opt-in 수집·원문/hash·정규화 계보 기반. 이번 미실행 |
| 농사로 | `Ssalddel/Services/AgriculturalFisheries/Information/Nongsaro공공데이터Module.cs` | 승인된 XML operation 카탈로그·클라이언트 재사용, 키 필요. 공개 HTML 열람과 API 이용조건 별개 |
| 감자 근거 조회 | `Ssalddel/Services/AgriculturalFisheries/Information/농사로감자생육요구ProfileQuery.cs` | 목록·상세·시기 조회, 정확한 제목/분류 검증, 6개 근거 Topic, PendingHumanReview·규칙 게시 금지. 수치 규칙 생성기가 아님 |
| 감자 보관 | `Ssalddel/Services/AgriculturalFisheries/Information/Nongsaro감자ProfileArchiveService.cs` | 출처 hash·revision·명시적 Simulation 승인 저장. 수집 함수는 DB 쓰기를 포함하므로 이번 호출 금지 |
| 식품 식별 | `Ssalddel/Services/AgriculturalFisheries/Information/공통식품품목IdentityCatalog.cs` | product:potato 아래 공급자 관계 상태를 분리. 농사로 210005는 상품 코드가 아님 |
| KAMIS | `Ssalddel/Services/AgriculturalFisheries/Information/KamisJsonClient.cs`, `KamisPriceArchiveService.cs`, `KamisPriceUnitProvenance.cs` | 인증·JSON·관측/수집 Run·단위 출처 재사용. 원 포장과 비교단위를 혼합하지 않음. API/DB 미조회 |
| AMS | `Ssalddel/Services/AgriculturalFisheries/Information/UsdaAmsMarketNewsClient.cs`, `UsdaAms시장가격ArchiveService.cs` | API key를 Basic 인증에 사용, 보고서/시장 단계·단위·통화 분리. 공개 사업체 Directory와 NASS는 별개 |
| 비교/품목 후보 | `Ssalddel/Services/AgriculturalFisheries/Information/Kamis중심UsdaAms품목MappingCatalog.cs`, `Kamis중심UsdaAms가격비교QueryService.cs` | Potatoes 후보, 세부 시장·품종·등급 검토. 직접 차액/이익 계산기로 해석하지 않음 |
| HS 후보 | `Ssalddel/Services/External/PublicData/FoodPriceCrosswalkCatalog.cs` | HS4 0701 후보를 국가 HSK/HTS 확정 세번으로 승격하지 않음 |
| 기상 | `Ssalddel/Services/AgriculturalFisheries/Information/기상청Asos일관측Client.cs` | kma-asos-daily, 관측일·지점 검증, 응답 hash, 선택 공간 문맥, 키 필요. 새 수집 없이 Sky 소유로 인계 |
| 공유 읽기 | `Ssalddel.Simulation.Persistence/SimulationSharedPublicDataPersistence.cs` | NoTracking·저장 차단 인터셉터. 현재 공개 조회는 KAMIS 관측; 모든 농사로/AMS 읽기를 제공한다고 확대하지 않음 |
| 현실 근거 동기화 | `Ssalddel.Simulation.Application/SimulationFarmRealityEvidenceService.cs` | 승인 운영 자료를 읽어 Simulation 파생 근거로 명시 동기화. ContextProposal 외 효과 금지 |
| 동결 | `Ssalddel.Simulation.Application/SimulationRealityContextService.cs` | 승인·정규화 자료를 읽는 파일 기반 카탈로그와 TryFreeze. 세션/Tick/Unity에서 Provider 호출 금지. 본 조사 JSON은 이 실행 스키마가 아님 |

농사로·KAMIS·AMS의 기관/기준일/단위/품목 관계는 공급자마다 보존한다. 기상은 [Sky 기준](C:/Users/user/source/repos/Hongdal/docs/Architecture/SkyEngine세계대기표현계층.md), 공공데이터는 [공통 수집 기준](C:/Users/user/source/repos/Hongdal/docs/Architecture/ExternalPublicDataServerIngestionFoundation.md)을 따른다. 참조 카탈로그 등록을 실행 어댑터 준비·수집 허용·재배포 허용으로 간주하지 않는다.
