# 문화교통 0.0 확장·파이프라인·완결 제안서

- 기준일: 2026-08-04
- 기준 범위: 현재 checkout의 최근 18개 로컬 커밋과 작업 트리
- 제품 기준: [0.0 집중 로드맵](focus-roadmap.md), [0.0 Release Checklist](checklist.md)
- 주 작업영역: `regional-culture-public-data`, `community-foundation`

## 제안 요약

현재 변경은 `/community/home`을 중심으로 지역문화, 관광, 가격, KOSIS, 해외제조업소, 수산 구역, HongikAcademy 콘텐츠, 역할별 원장 관점과 운송 시뮬레이션을 한 지도에 조립하는 데 성공했다. HS6 식품 가격 카드도 국내 시장 관측과 국가별 수입통계의 단위·시장 단계 차이를 보존한 채 별도 탐색 경로로 연결되었다.

다음 우선순위는 새로운 레이어의 수를 늘리는 일이 아니다. **공식 자료를 반복 수집하고, 검증하고, 영속 snapshot으로 발행하고, 지도와 게시판에서 같은 근거를 재사용하고, 사용자가 대화와 공동 원장으로 이어 갔다가 완료 사례로 돌아오는 한 바퀴를 닫는 일**이다.

따라서 다음 세 방향을 제안한다.

1. **확장**: 지도에서 더 많은 정보를 보여 주는 확장보다 `근거 상세 → 질문·제안 → 대화 → 동의 → 원장 초안`으로 이어지는 제품 확장을 우선한다.
2. **파이프라인화**: 현재의 개별 client, `BackgroundService`, command line, Quartz job을 공통 수집·검증·발행 파이프라인으로 정렬한다.
3. **완결**: 0.0-A~E의 미완료 release gate를 실제 영속성, 재시작, 모바일, 신고·삭제, 복구 증거로 닫는다.

## 현재까지 확보한 기반

| 영역 | 확보한 기반 | 현재 증거 | 남은 경계 |
| --- | --- | --- | --- |
| 통합 지도 | 공개 관측, 역할별 추천 레이어, 원장 관점 레이어, 사용자 재선택 | Web build·집중 test·`/community/home` 브라우저 렌더 | 선택 공공데이터의 실제 payload는 source 활성화와 server snapshot이 필요 |
| 선택 공공데이터 | 관광·온라인가격·KOSIS를 독립 client와 독립 snapshot으로 갱신하고 실패한 source만 이전 성공값 유지 | client·투영 test와 일회성 live probe | snapshot이 process memory이므로 재시작·다중 인스턴스·감사 이력에 약함 |
| 가격·품목 근거 | HS6 카드, KAMIS 범위, 국가별 수입통계 상태, 월별 추세, 대표 이미지 | 서비스·UI test와 `/information/food-ingredients` 실제 렌더 | HSK10 확정, 검역·표시·수입 가능성 판단으로 오인되지 않게 준비 단계 유지 |
| 지역문화 이미지 | 연구 준비 pack, 생성·검토 worker, 명시적 Blob·DB publish command | readiness test와 게시 명령 경계 | 생성 비용 승인, 사람 검토, 권리·출처 확인, 게시 승인 단계를 운영 절차로 고정해야 함 |
| 수산·해양 | 공식 어획구역 원문을 해양 타일로 집계하고 원문 hash·수집시각 보존 | 수집·캐시·presentation test와 실제 API 응답 | 좌표·경계·실시간 조업 정보가 아니라는 한계를 계속 유지해야 함 |
| 운송 표현 | 고정 fixture 기반 `SIMULATED` 화물·항공·해상 경로와 정지·접근성 | Google 지도 위 실제 브라우저 렌더 | 실제 추적·배차·운영 식별정보 pipeline으로 전환하면 안 됨 |
| 배치 기반 | Quartz 수집·자동 게시, `SourceKey + PeriodKey` 멱등 게시 구조 | 기존 KAMIS·USDA·커뮤니티 editorial job과 test | 새 지도 source는 별도 메모리 refresh loop여서 동일한 운영·재처리 모델에 아직 합류하지 않음 |

현재 작업 트리에는 역할 문자열 정규화와 역할별 기본 레이어 보강이 미커밋 상태로 존재한다. 이 제안서는 해당 변경을 완료됐다고 간주하지 않으며, 별도 검증·커밋 단위로 취급한다.

## 1. 확장하면 좋은 부분

### 1.1 지도에서 공동행동으로 이어지는 연결

각 공개 관측의 상세 panel에 다음 동작을 같은 stable ID로 연결한다.

- `근거 자세히 보기`: 출처, 원문 URL, 수집시각, 기준시각, 단위, 지역, 갱신주기, 제한을 표시
- `이 근거로 질문하기`: canonical 게시판의 새 글 초안을 만들되 자동 게시하지 않음
- `같이 확인할 사람 모집`: 비구속 참여 의사와 필요한 역할만 기록
- `원장 초안 만들기`: 참여·연락처·공개 범위 동의 전에는 가원장만 생성
- `완료 사례로 돌아가기`: 완료 원장에서 개인정보를 줄인 사례 초안을 만들고 사용자 확인 뒤에만 게시

지도는 상대 추천, 주문, 계약, 배차를 확정하는 화면이 아니라 **공개 근거를 공동 대화의 시작점으로 바꾸는 화면**으로 확장한다.

### 1.2 역할별 화면을 권한과 분리한 채 공통화

현재 Web 역할 관점은 좋은 탐색 기본값이다. 다음 단계에서는 역할 분류, 레이어 추천, 범례와 상세 panel 계약을 Web 전용 조립에서 공통 ViewModel로 옮겨 MAUI에서도 같은 의미를 사용하도록 한다.

- 역할은 표시 우선순위만 바꾸고 권한을 부여하지 않는다.
- 여러 역할을 가진 사용자는 현재 선택한 관점을 명시적으로 바꿀 수 있게 한다.
- 사용자가 바꾼 레이어 상태는 계정 설정과 기기 임시 상태를 구분한다.
- Web·Android·iOS가 같은 dataset key와 observation stable ID를 사용한다.

### 1.3 정보 밀도보다 근거 품질 확장

새 source는 다음 조건을 만족할 때만 기본 지도 후보로 올린다.

- 공식·공개 원천과 재사용 조건을 확인할 수 있음
- 출처·기준시각·단위·지역·갱신주기·제한을 계약에 담을 수 있음
- 개인 위치, 재고, 공급 가능 여부, 계약 조건으로 오인되지 않음
- 기존 source와 비교할 때 통화·단위·시장 단계가 정렬되지 않으면 순위·절감액을 만들지 않음
- 실패 시 빈 성공이나 sample fallback을 만들지 않음

확장 후보의 우선순위는 지역 음식·재료의 공식 근거, 지역 문화기관, 공개 시장·물가 맥락 순으로 두고, 실제 선박·항공기·차량 위치는 공개 홈 확장 범위에서 제외한다.

### 1.4 운영자용 출처 상태 화면

사용자용 지도와 별도로 source 운영 상태를 제공한다.

- 마지막 성공·실패 시각, 다음 실행 시각, 자료 기준일, 레코드 수
- 현재 공개 중인 snapshot version과 원문 hash
- 실패 원인 분류, 재시도 가능 여부, 연속 실패 횟수
- stale·quarantine·disabled 상태
- 수동 재실행은 운영 권한과 감사 기록을 요구

이 화면은 credential 값이나 원문 개인정보를 노출하지 않는다.

## 2. 파이프라인으로 만들 부분

### 2.1 공공데이터 수집·투영 파이프라인

기존 Quartz와 `SourceKey + PeriodKey` 멱등성 모델을 재사용해 아래 단계를 source 공통 계약으로 만든다.

```text
Source 등록
  → 수집 실행과 원문 metadata 기록
  → 형식·단위·기준시각·지역 검증
  → 실패 자료 quarantine
  → versioned raw/normalized snapshot 영속 저장
  → 공개 projection 생성
  → 지도·상세 API가 같은 snapshot version 조회
  → canonical 게시판 요약 초안 또는 멱등 게시
  → 실행 결과·freshness·오류 관측
```

핵심 규칙은 다음과 같다.

- 실행 키는 `SourceKey + PeriodKey + SchemaVersion`으로 고정한다.
- 수집 성공과 공개 승인을 분리한다. 새 snapshot 검증 실패 시 마지막 성공본을 유지한다.
- source별 실패가 다른 source의 snapshot을 지우지 않게 한다.
- 지도 요청 경로에서 외부 API를 직접 호출하지 않는다.
- process memory store를 영속 snapshot repository로 교체하고 시작 시 마지막 공개 version을 복원한다.
- 다중 인스턴스에서는 Quartz persistent store 또는 분산 lease로 단일 실행을 보장한다.
- 재처리 결과는 멱등해야 하며 같은 기간의 커뮤니티 글을 중복 생성하지 않는다.

첫 전환 대상은 관광·온라인가격·KOSIS 세 source다. 이미 독립 client와 투영 경계가 있어 pipeline 계약을 검증하기 가장 작고 안전한 slice다. 다음으로 경기 축산, KAMIS, USDA, 해양수산 파일 source를 같은 실행 원장에 합류시킨다.

### 2.2 근거에서 커뮤니티 게시까지의 편집 파이프라인

자동 게시를 source 수집 job 안에서 직접 수행하지 않고 다음 단계로 분리한다.

1. 수집 성공 snapshot을 기준으로 `게시 후보` 생성
2. 동일 `SourceKey + PeriodKey` 후보 중복 제거
3. 출처·단위·지역·한계 문구 검증
4. canonical 게시판 하나에만 저장
5. 관련 게시판은 복제 대신 canonical 글 link 생성
6. source별 설정이 켜진 경우에만 자동 발행, 그 외에는 운영 검토 대기
7. 수정·숨김·재발행 이력을 감사 기록에 보존

이 pipeline은 기존 community editorial Quartz job과 publisher를 확장하고, 공공데이터 수집 성공 여부와 게시 성공 여부를 서로 다른 상태로 기록한다.

### 2.3 지역문화 이미지 제작 파이프라인

이미지 작업은 비용·권리·품질 판단이 있으므로 완전 자동화하지 않는다.

```text
공식 지역 근거 조사
  → prompt/readiness pack
  → 생성 대상·비용 승인
  → 이미지 생성
  → 사람의 지역성·왜곡·권리·텍스트 혼입 검토
  → 승인 manifest
  → 명시적 Blob·DB publish
  → 지도·게시판 연결
  → 교체·철회 이력 보존
```

기존 `--confirm-billable=true`와 `--confirm-storage-write=true` 경계를 유지한다. schedule은 조사·readiness 생성까지만 허용하고, 비용 발생 제출과 공개 게시에는 별도 승인을 요구한다.

### 2.4 검증·릴리즈 파이프라인

기능 slice마다 다음 증거를 자동 수집한다.

- contract·metadata 호환과 source schema test
- 수집 재실행·부분 실패·마지막 성공본 유지·중복 방지 test
- `Ssalddel.v0.0.slnx` build와 영향 test의 실제 실행 수
- migration apply와 마지막 공개 snapshot 복원 test
- source 비활성 상태의 0.0 독립 실행 test
- Web·Android·iOS의 loading·empty·error·retry·disabled 상태
- Web 화면은 최종 URL, visible state, console 오류와 실제 PNG
- 결과는 `artifacts/local/validation/`에 보존하고 변경 기록에는 대표 근거만 연결

운영 credential이 필요한 live probe는 기본 CI와 분리한 승인형 profile로 두고, 값은 User Secrets 또는 배포 secret store에서만 공급한다.

## 3. 완결해야 할 부분

### P0. 현재 변경 묶음의 통합 상태 확정

- 원격보다 앞선 18개 커밋과 현재 미커밋 역할 분류 변경을 별도 상태로 정리한다.
- `Ssalddel.v0.0.slnx` 기준 Fast·Task 검증을 실행하고 실제 test 수를 남긴다.
- 관광·온라인가격·KOSIS가 활성화된 server와 `/community/home`을 함께 실행해 실제 marker payload, freshness, source detail을 확인한다.
- Web에서 확인된 레이어와 MAUI 공통 화면의 차이를 명시한다.

완료 기준은 로컬 commit, build/test, browser runtime, push·배포를 각각 구분해 증거를 남기는 것이다.

### P1. 선택 공공데이터의 영속 폐쇄 루프

- 메모리 snapshot을 DB 또는 object storage 기반 versioned snapshot으로 교체
- 원문 metadata, normalized payload, validation result, 공개 version을 분리 저장
- 재시작 뒤 같은 version 복원
- 부분 실패·재시도·동시 실행·중복 실행 test
- 지도와 canonical 게시판이 같은 snapshot version을 참조

이 단계가 끝나야 현재 공공데이터 지도 작업을 `0.0-B 영속 커뮤니티`의 완료 근거로 사용할 수 있다.

### P2. 지도 → 대화 → 원장 → 사례 한 바퀴

- observation stable ID로 게시글 초안 생성
- 명시적 참여·연락처·공개 범위 동의
- 가원장 또는 공동 원장 생성과 재조회
- 다이어그램과 참여자·역할·상태 일치
- 완료 원장의 비식별 사례 초안 생성
- 사용자 공개 승인 뒤 canonical 사례 게시
- 같은 사례에서 새 대화 또는 원장 시작

주문·결제·계약·배차·정산은 이 0.0 흐름에서 생성하지 않는다.

### P3. 안전·운영·복구

- 신고·차단·운영자 숨김·고정·이의 처리 감사 기록
- rate limit과 반복 자동 게시 방지
- 개인정보 열람·수정·철회·삭제·보유기간 API와 화면
- exact GPS·주소·연락처·계정 식별자의 공개 projection 차단 test
- migration, backup, restore, snapshot rollback, 오류 알림 훈련

### P4. 다중 클라이언트 release gate

- 0.5 이후 feature가 모두 꺼진 구성에서 0.0 홈·게시판·공공데이터 실행
- Android·iOS·Web의 같은 stable ID deep link
- loading·empty·error·retry·disabled와 네트워크 복구
- 인증 전 공개 탐색과 인증 후 참여 흐름
- 역할 관점 변경이 권한이나 자동 상대 선택으로 이어지지 않는지 확인

## 권장 실행 순서

### 현재 구현을 반영한 우선순위

기존 순서의 `지도 근거 상세과 질문 초안`과 가원장 폐쇄 루프를 완료했고, 신청 개인정보도 서버 증적·철회·세 Command Gate까지 닫았다. 이제 관리자 검토 화면보다 사용자가 매번 들어오는 `/community/home` 지도의 신뢰성·재조회·탐색 연속성을 먼저 닫는다. RSS 검토는 독립 관리자 기능이 아니라 뉴스 레이어에 승인된 근거를 공급하는 후속 source pipeline으로 둔다.

| 단계 | 우선순위 | 세로 slice | 완료 판정 |
| --- | --- | --- | --- |
| 지금-0 | A0 | 현재 작업 트리 capability 기준선 | build/test/browser/commit/push 상태를 기능별로 분리하고 0.0 비활성 구성 확인 |
| 완료 | A1 | 지도 근거 게시글 상세·참여 시작 동의 → 가원장 | 서로 다른 참여자, 동일 계정 중복 방지, 관심 철회, 세 필수 확인, 멱등 생성과 같은 게시글 재조회까지 검증 |
| 완료 | A2 | 신청 개인정보 동의 서버 원장 | 서버가 버전·시각·범위·주체·문안 hash·철회를 저장하고 지도 출발 미동의 신청을 거부 |
| 지금-1 | M1 | 지도 source versioned snapshot | 관광·온라인가격·KOSIS부터 재시작 복원, 마지막 성공본, freshness와 source version 확인 |
| 지금-2 | M2 | 지도 탐색 상태와 stable deep link | 국가·레이어·마커·observation 선택을 URL로 복원하고 새로고침·뒤로가기에서 같은 공개 근거 재조회 |
| 다음-1 | M3 | 마커 근거 → 질문·게시글 재조회 | 질문 초안과 게시글 상세가 같은 observation stable ID와 snapshot version을 유지 |
| 다음-2 | M4 | 뉴스/RSS 지도 보강 | 승인된 후보만 언론사 마커 상세·canonical 게시판에 연결하고 자동 게시와 원문 복제를 금지 |
| 후속-1 | S1 | 신고·삭제·철회·backup/restore | 개인정보와 운영 조치가 감사 가능하고 실패 주입 뒤 복구됨 |
| 후속-2 | S2 | 완료 사례 환류와 다중 client | 비식별 사례 공개 승인과 Web·Android·iOS deep link 검증 |

새 source와 이미지 확대는 위 공통 흐름을 우회하지 않는 경우에만 착수한다. 실제 주문·결제·계약·배차·정산과 외부 partner 호출은 이 우선순위의 운영 Gate가 닫히기 전 기본 비활성으로 유지한다.

| 순서 | 세로 slice | 주요 산출물 | 완료 판정 |
| --- | --- | --- | --- |
| 1 | 선택 공공데이터 snapshot 영속화 | snapshot entity/repository, migration, refresh job, 지도 조회 API/test | 재시작·재실행 뒤 같은 공개 version, 부분 실패 시 마지막 성공본 유지 |
| 2 | 지도 상태 URL화 | country·layer·marker·observation query contract와 복원 test | 공유 URL·새로고침·뒤로가기 뒤 같은 선택과 상세 표시 |
| 3 | 지도 근거 상세과 질문 재조회 | stable observation detail, draft link, canonical board relation | 지도 근거와 글 초안·게시글이 같은 source/version을 참조 |
| 4 | 지도 반응형·접근성·성능 | viewport 기준 marker 제한, 모바일 bottom sheet, keyboard·reduced-motion 검증 | desktop·390px에서 loading·empty·error·retry와 선택 상태가 겹치지 않음 |
| 5 | 뉴스/RSS source 연결 | 후보 영속 상태, 승인된 기사 metadata, 언론사 마커·게시판 relation | 관리자 화면을 방문하지 않아도 지도 상세에서 최신 승인 근거와 기준 시각 확인 |
| 6 | 공동 원장 환류와 안전 gate | 동의 상태, 원장 생성·재조회, 사례 초안, 신고·삭제·복구 | 대화에서 사례까지 한 바퀴가 중복 없이 연결 |

## 하지 말아야 할 확장

- 지도 layer마다 별도 scheduler, 별도 in-memory cache, 별도 실행 모드를 추가하는 것
- 공공 가격을 공급 가능 수량, 재고, 계약 가격 또는 절감액으로 바꾸는 것
- 역할 추천을 권한, 신뢰 점수, 상대 자동 선택이나 배차 근거로 사용하는 것
- 실제 선박·항공기·차량·개인의 순간 위치를 공개 홈에 연결하는 것
- 수집 실패를 sample marker나 빈 성공 게시글로 숨기는 것
- 이미지 생성·저장·게시를 하나의 무승인 자동 job으로 합치는 것
- 0.0 release gate를 닫기 전에 0.5 이후 외부 효과를 기본 활성화하는 것

## 최종 제안

다음 구현은 `M1`인 **관광·온라인가격·KOSIS 지도 snapshot 영속화** 한 slice다. 지도 요청 중 외부 source를 직접 호출하지 않고, 마지막 성공본과 source version·기준 시각·freshness를 같은 API에서 재조회하게 한다. 그다음 `M2` 국가·레이어·마커 선택의 stable deep link를 닫고, RSS 후보 원장은 뉴스 마커를 실제 승인 근거로 보강하는 `M4`에서 연결한다.

이 순서를 따르면 지금까지 만든 지도와 공공데이터 자산이 데모 화면에 머물지 않고, 문화교통 0.0의 핵심인 **출처 있는 정보 → 대화 → 동의된 공동행동 → 원장 → 사례 환류**의 운영 가능한 제품 흐름이 된다.
