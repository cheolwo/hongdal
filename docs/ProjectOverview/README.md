# 첨부 문서

이 폴더는 루트 `README.md`에서 덜어낸 상세 문서를 모아 둡니다. 처음에는 기술 구조보다 **현재 존재하는 화면과 캡처**를 먼저 봅니다.

루트 README는 홍달 1.0의 1페이지 요약만 유지합니다. 앱별 화면, 캡처 이미지, 화면 간 관계, 업무 흐름, 기술 설명은 이 첨부 문서에서 순서대로 봅니다.

## 먼저 볼 화면 문서

| 번호 | 문서 | 내용 |
| --- | --- | --- |
| 00 | [첨부 문서 목차](00-첨부문서목차.md) | 화면 문서부터 기술 문서까지 읽는 순서 |
| 01 | [app-page-catalog.md](app-page-catalog.md) | 각 앱 프로젝트에 실제로 선언된 `@page` 화면 전체 카탈로그와 캡처 PNG 링크 |
| 02 | [hongdal-v1-required-pages.md](hongdal-v1-required-pages.md) | 홍달 1.0 운송 흐름을 성립시키기 위해 필요한 화주, 기사, 관리자 화면 |
| 03 | [hongdal-v1-render-capture-summary.md](hongdal-v1-render-capture-summary.md) | 실제 화면 캡처 방식, 렌더링 확인 결과, 남은 검증 항목 |
| 04 | [workflow-app-screen-map.md](workflow-app-screen-map.md) | 여러 앱 화면이 하나의 업무 흐름을 완성하는 관계 |
| 05 | [screen-flows.md](screen-flows.md) | 화면의 버튼, 카드, 모드 전환이 다음 행동으로 이어지는 흐름 |

## 업무 흐름 문서

| 번호 | 문서 | 내용 |
| --- | --- | --- |
| 06 | [dispatch-flows.md](dispatch-flows.md) | 화물/용달 배차와 음식 배달 배차의 경계 |
| 07 | [warehouse-flows.md](warehouse-flows.md) | 입고, 적재, 출고, 주문 발생 시 창고 알림 흐름 |
| 08 | [orderer-group-commerce-flows.md](orderer-group-commerce-flows.md) | 공동주문, 해외 선적/통관, 국내 운송, 판매채널 출고 흐름 |
| 09 | [version-roadmap.md](version-roadmap.md) | 1.0부터 3.5까지의 단계별 제품 방향 |

## 기술 참고 문서

기술 용어와 내부 구조는 처음 화면을 파악한 뒤에 봅니다.

| 번호 | 문서 | 내용 |
| --- | --- | --- |
| T-01 | [workflow-api-policy.md](workflow-api-policy.md) | API를 화면과 업무 절차 기준으로 관리하는 기준 |
| T-02 | [DomesticCargoTransportOS.md](../Architecture/DomesticCargoTransportOS.md) | 국내 화물 운송을 운영하는 내부 기준 |
| T-03 | [EngineOverview.md](../Architecture/EngineOverview.md) | OS, 워크플로우, 엔진의 관계 |
| T-04 | [HIOPSAI.md](../Architecture/HIOPSAI.md) | 참여자 입장 해석과 배차 조율을 돕는 AI 방향 |
| T-05 | [OutboundBatchEngine.md](../Architecture/OutboundBatchEngine.md) | 출고 배치와 피킹 배치 판단 기준 |
| T-06 | [DispatchQueueResponsibility.md](../Architecture/DispatchQueueResponsibility.md) | 배차 상태 저장과 실행 자료의 책임 경계 |
| T-07 | [hiops-ai-judgment-cases.md](hiops-ai-judgment-cases.md) | AI 판단 보조를 만들기 위한 상황별 판단 사례 |
| T-08 | [glossary.md](glossary.md) | POD, BL, 3PL, 레그, RAG 같은 주요 용어 정의 |

## 관리 원칙

1. 루트 README에는 홍달 1.0과 대표 화면만 둔다.
2. 화면 캡처와 전체 페이지 카탈로그를 첨부 문서의 앞순위에 둔다.
3. OS, 엔진, AI, API 같은 기술 설명은 뒤쪽 참고 문서로 둔다.
4. 새 화면을 추가하면 `app-page-catalog.md`와 캡처 이미지부터 갱신한다.
5. 화면 간 상태 전파나 시퀀스는 `workflow-app-screen-map.md`에 둔다.
