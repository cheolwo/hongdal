# API 버전 중심 분류를 업무 의미 중심으로 전환

## 결과

- 제품 버전은 Controller의 현재 책임이 아니라 최초 도입 이력으로 분리했다.
- 업무 영역, 사용자, 업무 동작, Workflow, Feature 경계를 서로 다른 의미로 조회할 수 있게 했다.
- 01 Community 주요 흐름과 02 Orderer, 03 Shipper, 04 Driver 전체 Controller, 05 Warehouse 주요 진입점에 역할과 업무 영역을 연결했다.
- 주문자·화주·기사 Controller는 역할별 공통 기반에서 사용자 의미를 물려받아 반복 표시를 줄였다.
- 기존 Workflow가 있는 Controller는 전환 기간 동안 한국어 업무 영역으로 안전하게 해석한다.
- API 목록 응답에도 업무 영역·사용자·업무 동작을 추가하고 기존 버전·Route·권한·Feature Flag 계약은 유지했다.

상세 기준은 [API 업무 의미 분류](../Architecture/ApiBusinessClassification.md)를 따른다.

## 화면

화면 없음. Controller metadata와 운영 조회 계약을 정리한 구조 리팩토링이다.

## 검증

- 역할 앱 Controller의 사용자·업무 영역 누락을 Architecture test로 검사한다.
- Feature 경계가 새 분류를 우선하고 기존 버전 Feature Key를 호환 경로로 읽는지 확인한다.
- API 목록에서 화주 운송 의뢰가 `운송 의뢰 / 화주 / 요청하기`로 반환되는지 확인한다.
