# 앱 맥락별 50개 이미지 자산 생성 제안서

## 결론

실행 프로젝트 수만큼 50개를 기계적으로 복제하지 않고, **13개 독립 업무 앱 팩×각 50개 = 총 650개 장면**을 기준으로 한다. MAUI와 Web이 같은 업무 화면을 표시하는 경우는 같은 팩을 재사용하고 중복 이미지를 생성하지 않는다.

바로 650개를 최고 품질로 생성하지 않는다. 먼저 앱별 대표 5개씩 65개를 저비용 초안으로 생성해 시각 언어를 검증한 뒤, 650개 초안을 완성하고 실제 화면에 곧 사용할 자산만 선별 고품질 보정한다.

이 문서는 생성 제안과 2026-08-01 파일럿 실행 기록을 함께 관리한다.

## 2026-08-01 구현 결정과 현재 상태

사용자 확인에 따라 실제 생성 provider는 **Google Gemini Nano Banana Batch API**로 결정했다. 초안은 `gemini-3.1-flash-lite-image`, `1K`, `generateContent` JSONL Batch를 사용하고 선별 자산만 별도 검토 후 상위 모델로 보정한다.

- [파일럿 catalog](../Content/AppContextImagePrompts/catalog.v1.json)에 13개 팩·5장, 총 65개 프롬프트를 등록하고 사용자 검토 뒤 `ApprovedForBatch`로 전환했다.
- `ResearchDraft -> ApprovedForBatch`는 사람이 프롬프트와 예상 비용을 검토한 뒤 변경한다.
- 제출은 `ApprovedForBatch`, `SsalddelExecution:Mode=Operational`, `GeminiImageBatch:Enabled=true`, API key, `--confirm-billable=true`를 모두 요구한다.
- 홍익학당 로컬 `.env`의 `GEMINI_API_KEY`는 [실행 script](../../eng/invoke-app-image-batch.ps1)가 현재 process에만 전달한다. 키를 Hongdal 파일·인자·로그·manifest에 복사하지 않는다.
- Batch job manifest와 다운로드 결과는 `artifacts/local/app-image-batches/`에만 저장한다.
- 2026-08-01에 13개 Batch job, 총 65장을 제출·완료·다운로드했다. 모든 job은 `BATCH_STATE_SUCCEEDED`, 결과 오류는 0건이며 파일과 SHA-256 검증도 65/65 통과했다.
- 생성 결과는 JPG 65개, 약 44.8MB다. 표본 검토에서 세계지도 커뮤니티 장면은 의도에 부합했지만 가격·단위 비교 장면 일부에 불필요한 영문 제목과 작은 왜곡 문자가 보여 전체 650장 확장 전 품질 선별과 `문자 없음` 프롬프트 보강이 필요하다.
- 같은 날 [확장 catalog](../Content/AppContextImagePrompts/catalog.expansion.v2.json)의 13개 팩×45장, 총 585장을 추가 제출·완료·다운로드했다. 기존 파일럿과 합쳐 팩당 50장, 총 650장이고 모든 manifest 결과와 SHA-256 검증이 650/650 통과했다.
- 전체 결과는 JPG 650개, 약 495.2MB다. 확장 프롬프트에서는 제목·장면 번호를 모델 입력에서 제거하고 문자·숫자·가상 UI 금지를 강화했지만, 표본 중 일부 창고·관리자 장면에서는 여전히 임의 문자가 생성됐다. 앱 통합 전 A/B/C 선별과 문자 포함 장면 제외가 필요하다.

현재 설정의 1K Lite Batch 출력 단가 `$0.0168`을 적용한 예상 출력 비용은 파일럿 65장 `$1.0920`, 확장 585장 `$9.8280`, 전체 650장 `$10.9200`이다. 이 값은 `2026-08-01` 공식 가격 기준이며 실제 청구에는 입력 token, 재시도, 세금과 환율이 별도로 반영될 수 있다. 다음 제출 직전에 공식 가격을 다시 확인한다.

```powershell
# 외부 호출 없는 팩 검증·비용 미리보기
powershell -NoProfile -ExecutionPolicy Bypass -File eng/invoke-app-image-batch.ps1 `
  -Mode Preview `
  -PromptPack docs/Content/AppContextImagePrompts/packs/community-shipper.pilot.v1.json

# ApprovedForBatch 변경과 최종 비용 확인 후에만 사용
powershell -NoProfile -ExecutionPolicy Bypass -File eng/invoke-app-image-batch.ps1 `
  -Mode Submit `
  -PromptPack docs/Content/AppContextImagePrompts/packs/community-shipper.pilot.v1.json `
  -ConfirmBillable
```

## 현재 저장소 기준

[코드 프로젝트별 전체 페이지 카탈로그](../ProjectOverview/app-page-catalog.md)는 8개 클라이언트 프로젝트를 주요 색인으로 다룬다. 현재 코드에는 `FDriverApp`, `SellerApp`과 분리된 세 관리자 호스트가 추가로 존재한다. 따라서 이 제안서는 2026-08-01 코드를 기준으로 다음 처럼 분류한다.

- API, Domain, Infrastructure, test, 공통 UI library는 앱 팩 대상이 아니다.
- `Ssalddel.Web.*App`과 `Ssalddel.WebApp`은 같은 업무 팩을 소비하는 Web 호스트로 본다.
- `SsalddelRestaurantDesktop`은 `RestaurantDeskApp` 팩을 재사용한다.
- 앱 아이콘과 스플래시는 브랜딩 자산이므로 아래 50개 문맥 이미지 수량에 포함하지 않는다.

현재 실제 문맥 자산은 `Ssalddel.Ui.Common/wwwroot/images/regions`의 지역 이미지 6개와 `images/vehicles`의 차량 SVG 6개를 제외하면 많지 않다. 대부분의 앱에는 기본 MAUI 아이콘·스플래시만 있다. 이번 작업은 기존 스크린샷을 대체하는 작업이 아니라, 온보딩·정보 카드·빈 상태·업무 설명에 쓸 문맥 자산을 보강하는 작업이다.

## 13개 기준 팩과 Web 재사용

| 팩 key | 기준 앱·호스트 | 주요 문맥 | Web·레거시 재사용 | 목표 |
| --- | --- | --- | --- | ---: |
| `community-shipper` | `SsalddelApp` | 커뮤니티, 화주, 운송 의뢰, 창고·판매 진입 | `Ssalddel.Web.CommunityApp`, `Ssalddel.Web.ShipperApp`, `Ssalddel.WebApp` | 50 |
| `freight-driver` | `DriverApp` | 화물 기사, 추천, 상·하차, 운행, 정산 | `Ssalddel.Web.DriverApp`, `Ssalddel.WebApp` | 50 |
| `food-driver` | `FDriverApp` | 음식 픽업·배달, 온도·포장, 인계 | 공통 차량·커뮤니티 자산만 재사용 | 50 |
| `orderer` | `OrdererApp` | 공동구매, 음식·마트·화물 주문, 수입 준비 | `Ssalddel.Web.OrdererApp`, `Ssalddel.WebApp` | 50 |
| `warehouse` | `WarehouseManagerApp` | 입고, 검수, 스캔, 재고, 피킹·포장·출고 | `Ssalddel.Web.WarehouseApp`, `Ssalddel.WebApp` | 50 |
| `restaurant-desk` | `RestaurantDeskApp` | 주문 수신, 조리 준비, 식재료, 리뷰, 배차 주소 | `SsalddelRestaurantDesktop`, `Ssalddel.WebApp` | 50 |
| `seller` | `SellerApp` | 상품, 리스팅, 재고, 주문, 판매채널 | `Ssalddel.WebApp` 판매 화면 | 50 |
| `human-resources` | `HumanResourcesManagerApp` | 역할 지원·배정, 계약, 교육, 급여 준비 | 없음 | 50 |
| `community-admin-web` | `SsalddelAdmin` | 기존 통합 관리자의 커뮤니티·공개자료 중심 운영 | 신규 업무 패키지로 이동한 운송·배달 자산은 제외 | 50 |
| `community-admin-mobile` | `SsalddelAdminApp` | 모바일 출처 검토, 글쓰기, 커뮤니티 운영, 반야 선별 | 운영 근거는 Web 관리자와 공유 | 50 |
| `food-delivery-admin` | `SsalddelAdmin`의 `/admin/food-delivery` | 음식 주문, 조리, 배차 검토, 전달·예외 | 통합관리자 `BusinessPackageAdmin.razor` | 50 |
| `freight-delivery-admin` | `SsalddelAdmin`의 `/admin/freight-delivery` | 화물 의뢰, 배차, 기사·차량, 운송, POD·정산 | 통합관리자 `BusinessPackageAdmin.razor` | 50 |
| `order-warehouse-admin` | `SsalddelAdmin`의 `/admin/order-warehouse` | 주문·출고, 입고·재고, 피킹·포장, 운송 인계 | 통합관리자 `BusinessPackageAdmin.razor` | 50 |
| **합계** | **13개 팩** |  |  | **650** |

세 관리자 이미지 팩은 별도 실행 프로젝트를 만들지 않고 `SsalddelAdmin`의 통합 업무 패키지 경로에서 사용한다. 팩 key는 이미지 분류와 기존 DB 참조 호환을 위해 유지한다. `HumanResourcesManagerApp`은 현재 페이지가 하나이므로 대표 장면을 우선 검수한다.

## 팩별 50개 장면 구성

각 번호는 안정적인 `scene key`가 된다. 화면에서 이미지를 교체해도 scene key는 유지하고 prompt version만 증가시킨다.

### 1. `community-shipper` — SsalddelApp

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–08` | 8 | 커뮤니티·지역문화 | 세계지도 탐색, 지역 시장, 생산자·소비자의 공개 대화, 공공데이터 근거 확인 |
| `09–16` | 8 | 농수산물·식재료 | 과일, 채소, 곡물, 수산물, 식재료 품질·포장 비교 |
| `17–22` | 6 | 가격·유통·무역 정보 | 도매시장 관측, 국가별 가격 비교, 항만·내륙 유통 관계 |
| `23–28` | 6 | 화주·운송 의뢰 | 화물 조건 입력, 차량 후보, 상·하차 조건, 운송 상태 확인 |
| `29–34` | 6 | 입고·창고 | 입고 예정, 스캔, 재고, 피킹·포장 인계 |
| `35–38` | 4 | 판매·상품 | 상품 등록, 채널 판매, 주문 처리, 수요 확인 |
| `39–42` | 4 | 꾸미기·개인화 | 커뮤니티 테마, 장식 자산, 나만의 공간 |
| `43–46` | 4 | empty·error·retry | 검색 결과 없음, 자료 미수집, 오프라인, 재시도 |
| `47–50` | 4 | 온보딩 | 커뮤니티→역할→다이어그램→자료 페이지 여정 |

### 2. `freight-driver` — DriverApp

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–08` | 8 | 차량·도로 문맥 | 1t 카고, 냉장차, 윙바디, 도심·산지·항만·산단 도로 |
| `09–16` | 8 | 상차·픽업 | 도크 접차, 로프·팔레트, 수량 확인, 안전 상차 |
| `17–24` | 8 | 하차·인계·POD | 도착 확인, 비식별 서명, 인수 상태, 손상 예외 |
| `25–30` | 6 | 안전·날씨 | 비·눈·폭염·야간, 휴식, 안전조끼, 운전 중 휴대폰 금지 |
| `31–36` | 6 | 경로·교통 | 우회, 통행제한, 주차 진입, 조착 예정, 위치 최신성 |
| `37–40` | 4 | 정산·수익 | 운임 구성, 월정산, 수수료 안내, 지급 보류 |
| `41–44` | 4 | 알림·empty | 추천 없음, 예약 없음, 알림 없음, 연결 오류 |
| `45–47` | 3 | 온보딩 | 운행 시작, 추천 검토, 증빙 인계 |
| `48–50` | 3 | 커뮤니티 | 기사 정보 공유, 개별 의뢰, 안전 경험 나눔 |

### 3. `food-driver` — FDriverApp

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–10` | 10 | 음식점 픽업 | 주문번호 확인, 대기, 포장 상태, 여러 건 분리 |
| `11–20` | 10 | 고객 배달 | 공동현관, 오피스, 주택, 야간, 비대면 인계 |
| `21–28` | 8 | 포장·온도 | 온·냉 분리, 커피 방지, 보온가방, 주문 훼손 예방 |
| `29–34` | 6 | 건물·길찾기 | 동·호수 확인, 접근 불가, 주차, 엘리베이터, 도보 구간 |
| `35–40` | 6 | 안전·날씨 | 헬멧, 전조등, 비·눈·폭염, 휴식, 주행 중 휴대폰 금지 |
| `41–44` | 4 | 전달 확인 | 문 앞 안전 배치, 직접 전달, 오배달 방지, 예외 보고 |
| `45–47` | 3 | 수익·empty | 배달 없음, 정산 대기, 운행 종료 |
| `48–50` | 3 | 온보딩 | 픽업→배달→인계 세 단계 |

### 4. `orderer` — OrdererApp

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–10` | 10 | 상품·식재료 | 농수산물, 포장 단위, 산지 근거, 보관 조건 |
| `11–18` | 8 | 공동구매 | 개별 의향, 모집, 조건 비교, 이의, 족선, 함께 주문 |
| `19–26` | 8 | 음식 주문 | 메뉴, 재료, 요청 사항, 준비 상태, 배달 상태 |
| `27–32` | 6 | 마트·화물 주문 | 장보기, 묶음 주문, 화물 조건, 수령 방식 |
| `33–38` | 6 | 같이 수입 준비 | 공급자, 비용, 물류, HS·HTS 검토, 전문가 인계 |
| `39–42` | 4 | 배송 옵션 | 개별 수령, 공동 집결, 시간창, 배송 범위 |
| `43–46` | 4 | 이력·empty | 주문 없음, 취소·철회, 재주문, 자료 미확인 |
| `47–50` | 4 | 온보딩 | 근거 확인→개별 선택→동의→재조회 |

### 5. `warehouse` — WarehouseManagerApp

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–08` | 8 | 입고·검수 | 입고 예정, 도크, 수량·외관·온도 검수 |
| `09–16` | 8 | 스캔·라벨 | 상품, 팔레트, 로트, 위치, 오스캔, 라벨 훼손 |
| `17–24` | 8 | 재고·보관 | 일반·냉장·냉동, FIFO, 재고 이동, 재고 부족 |
| `25–32` | 8 | 피킹·포장 | 피킹 배치, 검수, 포장재, 합포장, 상차 준비 |
| `33–38` | 6 | 출고·인계 | 출고 예정, 문서, 차량 접차, 운송 패키지 인계 |
| `39–42` | 4 | 안전·장비 | 지게차 동선, 안전조끼, 적재 한도, 오염 방지 |
| `43–46` | 4 | empty·status | 작업 없음, 스캔 장애, 입고 지연, 재고 불일치 |
| `47–50` | 4 | 온보딩 | 입고→검수→재고→출고 |

### 6. `restaurant-desk` — RestaurantDeskApp

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–10` | 10 | 음식·메뉴 | 한식, 분식, 샐러드, 면, 구이, 디저트를 정직한 구성으로 표현 |
| `11–18` | 8 | 주문함·조리 | 신규 주문, 준비 시간, 주방 작업대, 완료 대기 |
| `19–26` | 8 | 식재료·공급 | 채소, 육류, 수산물, 양념, 냉장 보관, 공급 요청 |
| `27–32` | 6 | 포장·인계 | 용기, 봉인, 온도 분리, 기사 호출, 전달 확인 |
| `33–38` | 6 | 매장·리뷰 | 매장 분위기, 혼잡, 칭찬·불만, 리뷰 답변 준비 |
| `39–42` | 4 | 배차 주소 | 매장 후문, 상가 진입, 주차 위치, 픽업 모호함 |
| `43–46` | 4 | 설정·empty | 주문 없음, 대기 없음, 알림 오류, 준비시간 설정 |
| `47–50` | 4 | 온보딩 | 주문 수신→조리→포장→전달 |

### 7. `seller` — SellerApp

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–10` | 10 | 상품 대표 | 농수산물, 가공식품, 포장 상품의 스튜디오·사용 맥락 |
| `11–18` | 8 | 리스팅·카테고리 | 대표·상세·구성품, 배경 색상, 안전 crop |
| `19–26` | 8 | 재고 | 재고 있음·부족·품절, 로트, 보관, 예약 재고 |
| `27–32` | 6 | 주문 처리 | 주문 수신, 피킹, 포장, 취소, 반품, 완료 |
| `33–38` | 6 | 판매채널 | 오픈마켓, 자사몰, 오프라인, 채널 동기화, 오류 |
| `39–42` | 4 | 수요·기회 | 주문자 수요, 지역 트렌드, 공동구매 제안, 재주문 |
| `43–46` | 4 | empty·status | 상품 없음, 주문 없음, 채널 미연결, 동기화 지연 |
| `47–50` | 4 | 온보딩 | 상품→리스팅→채널→주문 |

### 8. `human-resources` — HumanResourcesManagerApp

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–08` | 8 | 역할 군 | 커뮤니티 운영, 주문, 창고, 운송, 배달, 판매, 음식점, 전문 검토 |
| `09–16` | 8 | 지원·온보딩 | 지원, 철회, 검토, 배정, 시작, 수습, 협업, 종료 |
| `17–24` | 8 | 문서·계약 | 역할 설명, 동의, 계약 초안, 전자서명, 보관, 갱신, 종료 |
| `25–30` | 6 | 일정·급여 | 근무 일정, 시간, 수당, 급여 일정, 확인, 보류 |
| `31–36` | 6 | 교육 | 안전, 개인정보, 고객응대, 식품·창고, 운송, 운영 교육 |
| `37–40` | 4 | 검토·동의 | 역할 적합성, 명시적 동의, 이의, 철회 |
| `41–44` | 4 | empty·status | 지원 없음, 검토 대기, 문서 미확인, 배정 종료 |
| `45–50` | 6 | 포용적 협업 | 다양한 연령·신체 조건의 근로자, 접근성, 협업, 휴식, 상호 존중 |

### 9. `community-admin-web` — SsalddelAdmin

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–10` | 10 | 게시판·콘텐츠 | 게시판 목록, 정보글, 모집, 질문, 완료 사례, 고정·숨김 |
| `11–18` | 8 | 안전·모더레이션 | 신고, 차단, 스팸, 반복 게시, 이의, 감사 기록 |
| `19–26` | 8 | 공공데이터·출처 | 지역문화, 농수산물 가격, 공식 기관, 출처 시각, 재검토 |
| `27–32` | 6 | 사용자 지원 | 정보 조회, 공개 범위, 연락처 동의, 삭제·복구, 이의 |
| `33–38` | 6 | 원장·workflow | 가원장, 공동 원장, 다이어그램, 역할 인계, 완료 환류 |
| `39–42` | 4 | 정책·감사 | 기능 노출, 권한, 감사 로그, 운영 장애 |
| `43–46` | 4 | empty·error | 자료 없음, 출처 미확인, 권한 없음, 재시도 |
| `47–50` | 4 | 온보딩 | 커뮤니티 운영→안전→공공데이터→감사 |

### 10. `community-admin-mobile` — SsalddelAdminApp

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–10` | 10 | 출처 검토 | 공식 기관, 자료 시각, 라이선스, 지역, 제한, 재확인 |
| `11–18` | 8 | 커뮤니티 운영 | 게시판, 신고, 댓글, 첨부, 공개 범위, 이의 |
| `19–26` | 8 | 콘텐츠 작성 | 문맥 분할, 프롬프트, 이미지 선택, 첨부, 초안, 미발행 |
| `27–32` | 6 | 반야·문화 선별 | 자료 비교, 중복 제거, 현재 행정구역·문화권 구분, 사람 검토 |
| `33–38` | 6 | 무역 준비 | 공급자, 비용, 물류, HS·HTS, 전문가 확인, 실행 금지 |
| `39–42` | 4 | 운영 상태 | 수집, 작성, 검토, 보류 |
| `43–46` | 4 | empty·error | 수집 없음, 정보 부족, 저장 실패, 재시도 |
| `47–50` | 4 | 모바일 온보딩 | 출처 확인→초안→검토→보류·승인 |

### 11. `food-delivery-admin` — SsalddelAdmin `/admin/food-delivery`

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–10` | 10 | 주문 운영 | 접수, 조리 준비, 완료, 픽업, 배달, 취소, 지연 |
| `11–20` | 10 | 배차 검토 | 후보 기사, 거리, 수량, 온도, 묶음, 사람 확정, 거절 |
| `21–28` | 8 | 음식점·준비 | 주방 혼잡, 예상 시간, 포장, 품목 누락, 재조리 |
| `29–34` | 6 | 기사 가용성 | 접속, 휴식, 거리, 운행 중, 알림 지연, 부족 |
| `35–40` | 6 | 예외·안전 | 오배달, 누락, 누수, 주소 모호, 연락 불가, 사고 보류 |
| `41–44` | 4 | 증빙·Outbox | 전달 확인, 이벤트 대기, 재처리, 감사 기록 |
| `45–47` | 3 | empty·error | 주문 없음, 후보 없음, 조회 오류 |
| `48–50` | 3 | 온보딩 | 주문→준비→배차→전달 |

### 12. `freight-delivery-admin` — SsalddelAdmin `/admin/freight-delivery`

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–08` | 8 | 화물 의뢰 | 화물, 상·하차지, 차량 조건, 시간창, 예외 |
| `09–16` | 8 | 배차 대기·검토 | 후보 부족, 추천 중, 잠금, 승인, 거절, 만료 |
| `17–24` | 8 | 기사·차량 | 운행 상태, 위치 최신성, 차종, 정비, 자격, 휴식 |
| `25–32` | 8 | 운송 단계 | 수락, 출발, 상차, 이동, 하차, 완료, 예외, 취소 |
| `33–38` | 6 | POD·문서 | 상차·하차 사진, 인수, 손상, 문서 연결, 검수 |
| `39–42` | 4 | 정산·예외 | 입금 대기, 기사 정산, 분쟁, 보류 |
| `43–46` | 4 | empty·error | 의뢰 없음, 후보 없음, 증빙 누락, 서버 오류 |
| `47–50` | 4 | 온보딩 | 의뢰→배차→운송→증빙·정산 |

### 13. `order-warehouse-admin` — SsalddelAdmin `/admin/order-warehouse`

| scene | 수 | 용도 | 대표 장면 |
| --- | ---: | --- | --- |
| `01–10` | 10 | 주문·출고 | 주문 접수, 할당, 출고 요청, 준비, 보류, 인계 |
| `11–20` | 10 | 재고·피킹·포장 | 재고 선택, 피킹 배치, 검수, 합포장, 라벨, 완료 |
| `21–28` | 8 | 입고·검수 | 입고 예정, 도착, 수량, 외관, 온도, 예외, 적치 |
| `29–34` | 6 | 운송 인계 | 출고 완료, 화물 의뢰 생성 후보, 차량 접차, 상차, 추적 |
| `35–40` | 6 | 문서·증빙 | 입고, 검수, 출고, 상차, 인수, 감사 문서 |
| `41–44` | 4 | 예외 | 재고 부족, 포장 훼손, 인계 실패, 운송 지연 |
| `45–47` | 3 | empty·error | 작업 없음, 출고 없음, 연결 오류 |
| `48–50` | 3 | 온보딩 | 주문→재고→피킹·포장→운송 인계 |

## 공통 시각 언어

이미지는 하나의 미술 스타일을 모든 화면에 강제하지 않고 다음 세 계열로 구분한다.

| 계열 | 적용 대상 | 시각 언어 |
| --- | --- | --- |
| `CultureEditorial` | 커뮤니티, 지역문화, 식재료, 상품 이야기 | 따뜻한 자연광, 현재의 생활 공간, 차분한 스타일라이즈드 3D·편집 이미지 |
| `OperationalDiagram` | 운송, 창고, 배달, 관리자, 인사 | 깔끔한 2.5D·아이소메트릭, 업무 주체·물건·상태를 명확히 분리, 장식보다 설명 우선 |
| `ProductStudio` | 주문자·판매자의 상품·메뉴 | 과장 없는 스튜디오·사용 맥락, 실제 포장 단위를 오인하게 하는 문구·로고·수치 금지 |

모든 계열의 공통 기준은 다음과 같다.

- 이미지 안에 읽을 수 있는 글자, 숫자, UI, QR, 로고, 인장, 인증 표시를 넣지 않는다.
- 실제 사업체, 생산자, 기사, 주소, 전화번호, 차량번호를 재현하지 않는다.
- 실제 POD, 세금계산서, 계약서, 통관 서류, 인증서처럼 보이는 가짜 증빙을 만들지 않는다.
- 운전 중 휴대폰 사용, 무보호 상차, 혼재 오염, 위험한 적재 같은 잘못된 행동을 정상 작업으로 묘사하지 않는다.
- 국적, 언어, 연령, 성별, 가족 형태, 종교, 경제력을 역할 자격이나 신뢰의 대리 표현으로 사용하지 않는다.
- 날씨, 지역, 식재료, 음식, 차량은 한국·미국 등 표시 맥락의 실제 조건과 맞는지 검토한다.
- 생성 이미지는 실제 상품, 현장, 사진, 가격, 품질, 계약, 신고, 증빙이 아니라는 표시와 사람 검토 상태를 manifest에 남긴다.

## 비율·파일·번들 기준

| 용도 | 기본 비율 | 원본 | 배포본 | 주요 위치 |
| --- | --- | --- | --- | --- |
| 히어로·온보딩 | `16:9` | 1K 이상 PNG | WebP/AVIF | 홈, 역할 소개, 업무 여정 |
| 정보·업무 카드 | `4:3` | 1K 이상 PNG | WebP/AVIF | 목록, 설명, 상태 카드 |
| 상품·역할 타일 | `1:1` | 1K 이상 PNG | WebP/AVIF | 상품, 메뉴, 역할 선택 |
| 모바일 여정 | `3:4` | 1K 이상 PNG | WebP/AVIF | 모바일 온보딩, 세로 카드 |
| empty·object | `4:3` 또는 `1:1` | 투명 PNG | WebP/PNG | empty, error, retry, 작은 오브젝트 |

팩당 권장 구성은 `16:9 10개 + 4:3 20개 + 1:1 10개 + 3:4 6개 + 투명 4개 = 50개`다. 앱 특성에 따라 개별 scene의 비율은 조정할 수 있지만 팩 전체의 종류별 수량은 먼저 고정한다.

어두운·밝은 테마를 위해 동일 장면을 두 번 생성하지 않는다. 중립 배경과 가장자리 여백을 유지하고 CSS overlay로 테마 대비를 맞춘다.

## 프롬프트 팩과 manifest

지역문화 자산의 기존 [Nano Banana Batch 프롬프트 우선 제안](RegionalCultureNanoBananaBatchPromptFirstProposal.md)에서 정의한 `프롬프트 선행 → 사람 검토 → Batch → A/B/C 선별 → 선택 장면 단건 보정` 원칙을 재사용한다. 다만 지역 고정관념 검토와 앱 업무 안전 검토는 서로 다르므로 같은 schema를 그대로 재사용하지 않고 상태 전이와 외부 제출 경계만 공유한다.

제안 경로는 다음과 같다.

```text
docs/Content/AppContextImagePrompts/
  catalog.v1.json
  packs/
    community-shipper.v1.json
    freight-driver.v1.json
    ...
    order-warehouse-admin.v1.json
```

각 scene은 다음을 가진다.

```text
AssetId
AppPackKey
SceneKey
CategoryCode
IntendedRoutes[]
IntendedComponentRole
PromptVersion
PromptKo / PromptEn
VisualStyleCode
AspectRatio / Resolution
SafeCrop
NegativePromptRules[]
EvidenceOrContextReferences[]
GenerationStatus
Provider / Model / GeneratedAtUtc
ReviewStatus / ReviewedBy / ReviewedAtUtc
AltTextKo / AltTextEn
MasterObjectName / OptimizedFiles[]
PerceptualHash / DuplicateGroupKey
```

상태는 다음 순서를 사용한다.

```text
ResearchDraft
  -> ContextReviewed
  -> PromptApproved
  -> DraftGenerated
  -> SelectedA | SelectedB | RejectedC
  -> Refined
  -> ApprovedForApp
  -> Optimized
  -> Integrated
  -> RenderVerified
```

`PromptApproved`가 아닌 scene은 Batch나 단건 API에 제출하지 않는다. 프롬프트나 비율이 바뀌면 기존 생성 결과 선택을 해제하고 version을 올린다.

## 저장과 배포

원본, 검토, 앱 배포를 분리한다.

| 단계 | 저장 위치 | 기준 |
| --- | --- | --- |
| 프롬프트·manifest | `docs/Content/AppContextImagePrompts/` | stable key, version, 검토 이력을 Git으로 보존 |
| raw 초안·외부 응답 | `artifacts/local/app-context-images/{run-key}/` | commit 금지, 재생성 가능한 임시 자산 |
| 선정 원본 | object storage | Base64·외부 응답은 버리고 원본 파일과 metadata만 보관 |
| 앱 배포본 | 각 앱 `wwwroot/images/context/{pack-key}/` | WebP/AVIF/PNG 최적화본, manifest에서만 참조 |
| 여러 앱 공통 소자산 | `Ssalddel.Ui.Common/wwwroot/images/context/shared/` | 실제로 3개 이상 호스트가 같은 파일을 쓸 때만 이동 |

모든 650개를 `Ssalddel.Ui.Common`에 넣어 모든 MAUI 앱에 포함시키지 않는다. 각 앱은 자신의 팩과 작은 공통 fallback만 번들한다. 팩당 최적화 배포 용량은 우선 15MB 이하를 목표로 한다.

## 생성 순서

### 0단계: 제안 승인

- 13개 팩 경계와 650개 수량을 확정한다.
- 시각 계열 3개와 기본 비율을 확정한다.
- 앱별 프롬프트 팩을 `ResearchDraft`로 작성한다.
- 외부 호출이나 비용 발생은 하지 않는다.

### 1단계: 앱별 5개 스타일 파일럿

- 팩당 히어로, 업무 카드, 상품·역할 타일, empty, 모바일 장면을 하나씩 고른다.
- 총 65개를 저비용 1K 초안으로 만든다.
- 색감, 인물 표현, 상품 정확성, 업무 안전, crop을 팩별로 검토한다.

### 2단계: 650개 저비용 초안

- 파일럿 승인 후 팩별 50개를 독립 Batch로 나눈다.
- 한 팩의 실패가 다른 팩의 재시도를 막지 않게 한다.
- prompt hash와 scene key로 중복 제출을 막는다.
- 현재 가격과 모델 지원 여부는 실제 제출 직전 공식 정보로 다시 확인한다.

### 3단계: A/B/C 선별

- `A`: 즉시 앱 통합 가능
- `B`: 구도나 상품·업무 정확성을 부분 보정
- `C`: 중복, 오표현, 안전 위반, 증빙 오인 위험으로 폐기 후 재생성

팩당 먼저 사용할 10개, 총 130개를 우선 선별한다.

### 4단계: 선택 장면만 고품질 보정

- A는 최적화만 수행할 수 있다.
- B는 직전 장면·스타일 참조를 유지한 단건 보정을 수행한다.
- 650개 모두를 premium 재생성하지 않는다.
- 새 유료 제출은 제출 목록·예상 수량·모델·비율을 보여 주고 명시적 확인 후 실행한다.

### 5단계: 앱 통합과 렌더 검증

- 이미지를 연결할 화면과 컴포넌트를 manifest에 먼저 지정한다.
- MAUI와 Web에서 같은 scene key의 crop·alt text·fallback을 확인한다.
- 실제 앱을 렌더링해 텍스트 가독성, 스크롤, 모바일 메모리, 어두운 테마를 확인한다.
- build 통과만으로 실제 화면 검증을 대체하지 않는다.

## 우선순위

| 순서 | 팩 | 이유 |
| ---: | --- | --- |
| 1 | `community-shipper`, `community-admin-web`, `community-admin-mobile` | 현재 0.0 커뮤니티·공공데이터 집중과 일치 |
| 2 | `orderer`, `warehouse`, `freight-driver`, `food-driver`, `restaurant-desk`, `seller` | 실제 업무 페이지가 여러 개 존재하며 즉시 적용 가능 |
| 3 | `human-resources` | 현재 1페이지이므로 5개 파일럿 후 페이지 확장과 함께 생성 |
| 4 | `food-delivery-admin`, `freight-delivery-admin`, `order-warehouse-admin` | 현재 공통 진입점만 있어 패키지별 실제 화면 분리가 선행되어야 함 |

## 품질·안전 검수표

| 검수 축 | 통과 기준 |
| --- | --- |
| 맥락 일치 | 앱의 실제 라우트·상태·사용자 역할과 맞는다. |
| 사실 경계 | 생성 이미지를 실제 상품·사진·가격·품질·증빙으로 표현하지 않는다. |
| 업무 안전 | 운전·상하차·창고·식품 안전에 어긋나는 장면을 정상으로 묘사하지 않는다. |
| 개인정보 | 실제 얼굴, 주소, 전화번호, 차량번호, 계좌, 문서 식별자가 없다. |
| 표현 공정성 | 국적·언어·성별·연령·종교·경제력을 역할·신뢰의 대리 지표로 쓰지 않는다. |
| 중복 | 같은 팩·다른 팩과 구도·색감·주요 오브젝트가 지나치게 같지 않다. |
| 모바일 crop | 16:9·4:3 자산에서 중요 인물·물건이 안전 영역 안에 있다. |
| 접근성 | 한국어·영어 alt text가 이미지의 목적과 핵심 장면을 간결히 설명한다. |
| 기술 품질 | 손·차량·상품·문자 왜곡, 투명 가장자리, 불필요한 watermark가 없다. |

## 수량 검증

| 팩 | 계획 수량 |
| --- | ---: |
| community-shipper | 50 |
| freight-driver | 50 |
| food-driver | 50 |
| orderer | 50 |
| warehouse | 50 |
| restaurant-desk | 50 |
| seller | 50 |
| human-resources | 50 |
| community-admin-web | 50 |
| community-admin-mobile | 50 |
| food-delivery-admin | 50 |
| freight-delivery-admin | 50 |
| order-warehouse-admin | 50 |
| **합계** | **650** |

## 이번 단계의 완료 기준

- 13개 팩과 650개 목표가 정확히 합산된다.
- Web·MAUI 재사용 경계가 명시되어 중복 생성을 막는다.
- 팩별 scene 범위와 화면 용도가 정의된다.
- 저비용 파일럿과 선별 보정 순서가 정의된다.
- 실제 생성, 통합, 커밋, 배포는 하지 않는다.

## 다음 승인 후 작업

1. `docs/Content/AppContextImagePrompts/catalog.v1.json` schema와 13개 pack 초안을 작성한다.
2. 각 pack의 50개 scene에 실제 route·component 참조를 붙인다.
3. 고정관념, 업무 안전, 개인정보, 중복 검증을 실행한다.
4. 앱별 대표 5개를 `PromptApproved` 후 저비용 파일럿으로 생성한다.
5. 파일럿을 검토한 뒤에만 팩별 50개 Batch 제출 범위와 비용을 다시 확인한다.
