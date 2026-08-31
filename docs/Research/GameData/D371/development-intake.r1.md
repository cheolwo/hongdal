# D371 시장 통찰 자료 조사 개발 인수

- 판본: `game-data-research.market-insight.d371.development-intake.r1`
- 검토일: 2026-08-30
- 판정: 조사 참고자료 인수. 상품 수집·저장·재노출·실행 구현 승인 아님.
- 전문 원문: [조사 보고서](README.md), [출처·참조 근거](evidence.r1.json), [제출 목록](manifest.r1.json).

## 인수와 원본 보존

전문 worktree의 `docs/Research/GameData/D371/` 문서 3개를 공유 저장소의 같은 상대 경로에 통합했다. UTF-8/BOM 없음/LF 원본 바이트를 보존했고 제출 목록의 두 파일 및 목록 자체 해시를 직접 대조했다. 현행 참조 23개 해시도 모두 일치했다. 전문의 로컬 artifacts·원자료는 복사하지 않았다. 이전 첫 Farm 제출 및 개발 인수 문서는 이번에 수정하지 않았다.

| 파일 | SHA256 |
| --- | --- |
| README.md | `BB0BBAB396C76180AE6DEAE8C8F2BC00D5D8C73F92D346A486C1E89ACF40881B` |
| evidence.r1.json | `BC2EC74D668EE4498F158FE0CDEDCC85351D1D82E43D122DAF39EA83D82B60A9` |
| manifest.r1.json | `467AB6642B9203047BA4112CA50994E7096BB31A18A7424F7E25503BC46145A3` |

## 개발 대조 결과

기존 Amazon 출품 준비와 상품 payload의 `RequiredFieldsPending`은 실제 수집 완료가 아니다. 실내 참고 관측의 Amazon/Alibaba 정규화 및 `TermsReviewStatus=Approved`/`ReferenceOnly` 관문은 가격·MOQ·동일 상품 대응을 보증하지 않는다. 내부 SKU·임시 LIST 식별자·판매 초안 MOQ를 외부 상품의 실측 사실로 쓰지 않는다. 기존 코드 재사용 경계를 유지하며 새 수집기나 병렬 DB는 만들지 않았다.

개발은 공식 Amazon 등록 안내에서 개발자·앱 등록과 앱 유형별 권한/인증 조건을 직접 확인했다. 실제 등록·계정 연결·동의·API 호출은 하지 않았다. [공식 등록 안내](https://developer-docs.amazon/sp-api/docs/sp-api-registration-overview).

개발은 정책 변경 안내의 2026-08-25 시행 및 저장 암호화·필요한 경우 30일 이내 삭제 요약을 확인했다. 이를 모든 자료의 30일 보관 허가나 Apify 수집 허가로 해석하지 않는다. 전체 AUP/DPP와 재노출 허용은 이번 검토로 확정하지 않았다. [공식 정책 변경 안내](https://developer.amazonservices.com/policy-update-for-sp-api).

Alibaba의 체계적 수집·상업 이용 제한과 거래 조건은 전문의 공식 페이지 브라우저 확인 보고를 인수했다. 개발의 별도 웹 조회는 메뉴만 반환하여 해당 본문을 독립 재확인하지 못했다. 따라서 전체 약관·허용 범위 법률 검토 완료로 표시하지 않는다. [공식 약관 페이지](https://rule.alibaba.com/rule/detail/2041.htm).

## 확정 방향과 남은 조건

[생존경제 문답](../../../Architecture/PlayableLoops/PlanningSessions/생존경제/survival-economy.inquiry.r1.md)의 D372/D373에 따른 선택형 현실 자료 열람과 상품 상세·거래 결과 상세 진입은 확정 방향이다. 이 선택을 다시 승인 대기로 돌리지 않는다. 조사 당시 최신 내용r8 해시는 `AA25AF041E0F9E0DA2A67C79E4F5598F48D7CDEDB803539FBA7E5C5725727FFF`다.

- 후속 기획 선택: 우선 대상 시장/상품, 구매 절감 참고와 재판매 참고 중 첫 목적, 구체 패널·링크·자료 없는 상태·갱신 범위.
- 확보할 근거: 허용 접근 경로, 보관·재표시 권리, 출처/시각, 동일·유사·미확정 상품 대응, MOQ·규격·품질·배송·세금·수수료·반품 등 비용 조건. 권리 증빙은 게임 기획 승인으로 대신하지 않는다.
- 기술 통합: 기존 관측/대응/동결 경계를 재사용하고 미확인 비용을 0으로 채우지 않는다. 데이터 권리와 상품 대응을 갖추기 전 실제 수익성이나 수집 성공을 표시하지 않는다.

추가 대량 조사·보험 조사·계정/키 연결은 배분하지 않았다. D363 배치 및 D359 벌목의 기존 승인 작업과 독립적인 문서 인수다.

## 증거 상한

이번 제출은 공식 안내 3건과 정적 소스 조사다. 상품/원자료 표본 0, 실제 상품쌍 0, 가격차·수익성 검증 0, API/Actor 실행 0이며 원자료 해시는 null이다. 해시·JSON·링크·문서 검증은 게임 빌드/시험·Runtime·Save·Scene·Game View·E 증거가 아니다. 코드·설정·DB·키·실행 원장·승인 해시·E 변경 및 commit/push 없음.
