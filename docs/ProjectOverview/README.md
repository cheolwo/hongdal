# Project Overview Docs

이 폴더는 루트 `README.md`에서 덜어낸 상세 로드맵, 업무 흐름, Mermaid 다이어그램을 관리합니다.

루트 README는 프로젝트의 핵심 요약만 유지하고, 긴 설명과 흐름도는 아래 문서에서 주제별로 관리합니다.

## 문서 목록

| 문서 | 내용 |
| --- | --- |
| [version-roadmap.md](version-roadmap.md) | 1.0부터 3.5까지의 단계별 제품 방향과 기능 판단 기준 |
| [dispatch-flows.md](dispatch-flows.md) | 화물/용달 배차 엔진과 음식 배달 배차 엔진의 경계 |
| [screen-flows.md](screen-flows.md) | 앱 화면의 버튼, 카드, 모드 전환이 내부 처리로 이어지는 흐름 |
| [warehouse-flows.md](warehouse-flows.md) | 입고, 적재, 출고, 주문 발생 시 창고 알림 흐름 |
| [orderer-group-commerce-flows.md](orderer-group-commerce-flows.md) | 주문자 집단 공동주문, 해외 선적/통관, 국내 물류대행 입고, 판매채널, 출고 배치, 입주민 우선 고용 흐름 |

## 관련 아키텍처 문서

| 문서 | 내용 |
| --- | --- |
| [OutboundBatchEngine.md](../Architecture/OutboundBatchEngine.md) | 단일/복수 상품 주문을 단일/복수 창고 출고 배치로 나누는 계획 계층 |

## 관리 원칙

1. README에는 핵심 요약만 둔다.
2. Mermaid 다이어그램은 이 폴더의 주제별 문서에 둔다.
3. 1.0 릴리즈 판단은 [version-roadmap.md](version-roadmap.md)를 기준으로 한다.
4. 화면과 API 연결 흐름은 [screen-flows.md](screen-flows.md)에 둔다.
