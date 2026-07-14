# 전통시장 공공데이터 모듈

## 목적

홍달의 지역 기반 기능을 직장 관리자 중심이 아니라 전통시장과 소상공인 중심으로 확장하기 위한 기준정보 모듈이다. 공공데이터를 홍달의 소유 데이터처럼 직접 수정하지 않고, 원본 기준일과 동기화 이력을 보존한 읽기 모델로 관리한다.

## 공식 데이터 원천

- 제공기관: 소상공인시장진흥공단
- 데이터: [소상공인시장진흥공단_전통시장현황_20250722](https://www.data.go.kr/data/15052837/fileData.do?recommendDataYn=Y)
- 원본 기준일: 2025-07-22
- 갱신 주기: 연간
- 핵심 식별자: 시장코드
- 주요 항목: 시장명, 시장 유형, 지번·도로명 주소, 시도, 시군구, 23개 편의·안전·물류시설 보유 여부

시설 보유 여부는 공공데이터 기준일의 참고 정보다. 현재 운영 여부를 보증하거나 입점 사업자의 권한을 증명하는 자료로 사용하지 않는다.

## 모듈 경계

`TraditionalMarketDbContext`는 기존 주문·운송·커뮤니티용 `HongdalContext`와 분리한다. 물리적으로는 같은 MySQL 연결을 사용하지만 다음 전용 테이블과 마이그레이션 이력을 가진다.

- `public_data_traditional_markets`: 전통시장 기준정보와 시설 현황
- `public_data_traditional_market_sync_runs`: 수집 성공·실패 및 변경 건수
- `__EFMigrationsHistory_TraditionalMarkets`: 모듈 전용 EF Core 마이그레이션 이력

원본에서 사라진 시장은 삭제하지 않고 `IsActive=false`로 전환하여 과거 커뮤니티나 거래 참조가 끊기지 않게 한다.

## API

| 용도 | 메서드와 경로 | 권한 |
|---|---|---|
| 시장 검색 | `GET /api/v1/traditional-markets` | 공개 |
| 시장 상세 | `GET /api/v1/traditional-markets/{marketCode}` | 공개 |
| 공공데이터 전체 동기화 | `POST /api/v1/traditional-markets/sync` | 서버 관리자 |

검색은 키워드, 시도, 시군구, 시장 유형, 공동물류창고 및 전용주차장 보유 여부를 지원한다. 응답의 `CommunityScopeKey`는 `traditional-market:{시장코드}` 형식이며 이후 시장 단위 커뮤니티의 안정적인 범위 키로 사용할 수 있다.

## 설정

기존 `PublicData` 설정을 재사용한다. 인증키는 저장소에 커밋하지 않고 로컬 설정이나 환경 변수로 주입한다.

```json
{
  "PublicData": {
    "DataGoKrServiceKey": "로컬 인증키",
    "TraditionalMarket": {
      "BaseUrl": "https://api.odcloud.kr",
      "ApiPath": "/api/15052837/v1/uddi:1fd54eb7-0565-4755-8ec7-a70931b6dc77",
      "DatasetKey": "semas-traditional-market-status",
      "SourceReferenceDate": "2025-07-22",
      "PageSize": 1000
    }
  }
}
```

`TraditionalMarket:ServiceKey`가 있으면 해당 값을 우선하고, 없으면 `DataGoKrServiceKey`, 마지막으로 기존 `ServiceKey`를 사용한다.

## 사용자별 활용 방향

- 소상공인: 소속 시장 선택, 시장 시설 확인, 시장 단위 커뮤니티 진입
- 방문자·주민: 지역과 시설 조건으로 시장 탐색
- 물류 담당자: 공동물류창고와 전용주차장 정보를 초기 참고 신호로 사용하되 실제 배차 가능 여부는 별도 운영 데이터로 판단
- 플랫폼 운영자: 연간 원본 갱신 시 수동 동기화 실행 및 변경·비활성화 건수 확인

향후 입점 사업자 권한은 공공데이터가 아니라 사업자 인증과 시장 운영주체 승인으로 별도 모델링한다.

## 생활권 공동구매 물류 거점

전통시장 기준정보와 실제 물류 운영 승인은 분리한다. 모든 시장은 주소와 시설 정보를 가진 후보가 될 수 있지만, `traditional_market_logistics_hubs`에 등록되어 시범운영 또는 활성 상태가 된 시장만 공동구매 물류 거점으로 공개한다.

거점 상태는 `Candidate → UnderReview → Pilot → Active` 순서로 진행한다. 운영 중단은 `Paused`, 종료는 `Closed`로 관리한다. 시범운영 또는 활성 전환에는 다음 조건이 필요하다.

- 상인회 또는 운영주체 지정 및 동의
- 현장 확인 완료
- 묶음 입고와 검수·분류 지원
- 주민 수령 또는 근거리 배송 중 하나 이상 지원
- 일일 공동구매 처리 용량과 생활권 서비스 반경 설정

공공데이터의 공동물류창고·전용주차장 표시는 후보 판단의 참고값일 뿐 자동 승인 조건으로 사용하지 않는다. 현재 응답은 `traditional-market-hub:{시장코드}` 형식의 `HubReferenceKey`를 제공한다. 공동구매 원장과의 직접 연결은 이후 이행 배치 기능에서 이 키를 참조해 추가하며, 현재 모듈은 거점 기준정보와 운영 가능 상태를 제공한다.

| 용도 | 메서드와 경로 | 권한 |
|---|---|---|
| 공개 거점 검색 | `GET /api/v1/traditional-market-logistics-hubs` | 공개, Pilot·Active만 |
| 공개 거점 상세 | `GET /api/v1/traditional-market-logistics-hubs/{marketCode}` | 공개, Pilot·Active만 |
| 전체 후보·상태 조회 | `GET /api/v1/admin/traditional-market-logistics-hubs` | 서버 관리자 |
| 후보 등록·운영조건 수정 | `PUT /api/v1/admin/traditional-market-logistics-hubs/{marketCode}` | 서버 관리자 |
| 상태 전환 | `POST /api/v1/admin/traditional-market-logistics-hubs/{marketCode}/status` | 서버 관리자 |

## 아파트-상인회 생활권 협의체

전통시장 물류 거점의 운영 승인과 별도로, 아파트 대표와 전통시장 상인회 대표가 수입·수출 희망 품목을 함께 검토하는 협의체를 둔다. 이 기능은 플랫폼이 거래를 주선하거나 계약을 자동 체결하는 기능이 아니다. 양측 대표가 제안과 의견을 기록하고, 이후 공동주문·수입 물류 워크플로우가 참고할 수 있는 합의 근거를 만드는 기능이다.

협의체는 다음 순서로 진행한다.

1. 아파트 대표 또는 상인회 대표가 상대 대표를 지정해 협의체를 만든다.
2. 요청자는 생성과 동시에 참여 수락 상태가 되고, 상대 대표는 별도 수락을 남긴다.
3. 양측 수락이 완료되면 협의체 상태가 `협의중`으로 바뀐다.
4. 참여 대표는 수입 또는 수출 안건을 만들고 품목, 수량, 국가, 기간, 물류 조건, 예상 금액과 통관 검토 필요 여부를 기록한다.
5. 양측 대표가 각각 `동의`, `보완요청`, `반대` 중 하나를 남긴다. 양측이 모두 동의한 경우에만 안건이 `합의`가 된다.

`traditional-market-council:{협의체Id}`와 `traditional-market-trade-agenda:{안건Id}` 참조 키는 게시글, 원장 또는 공동주문 실행 계획이 협의 결과를 연결할 때 사용한다. 합의는 주문·계약·통관 신고를 자동 생성하지 않는다.

| 용도 | 메서드와 경로 | 권한 |
|---|---|---|
| 내가 참여한 협의체 | `GET /api/v1/traditional-market-councils/mine` | 로그인 사용자 |
| 협의체 상세 | `GET /api/v1/traditional-market-councils/{councilId}` | 양측 대표 |
| 협의체 생성과 상대 대표 초대 | `POST /api/v1/traditional-market-councils` | 로그인 사용자 |
| 초대 수락 | `POST /api/v1/traditional-market-councils/{councilId}/accept` | 지정된 대표 |
| 수입·수출 안건 생성 | `POST /api/v1/traditional-market-councils/{councilId}/agendas` | 참여 수락이 끝난 양측 대표 |
| 안건 결정 | `POST /api/v1/traditional-market-councils/{councilId}/agendas/{agendaId}/decisions` | 양측 대표 |

저장 테이블은 `traditional_market_neighborhood_councils`와 `traditional_market_trade_agendas`이며, 전통시장 모듈의 `TraditionalMarketDbContext`와 전용 마이그레이션 이력을 사용한다. 대표 사용자 ID는 협의체 내부 역할 판정에만 사용하며 전역 회원 역할을 새로 만들지 않는다.
