# Hongdal View-Controller 매핑 문서

지금 여기서는 기존 View와 Controller를 기준으로, 화면 1개 ↔ 주 대응 Controller 1개를 먼저 잡고 세부 요소를 API 함수에 다시 연결한다.

## 문서 목적
- DriverApp, ShipperApp, Server를 같은 축으로 읽는다.
- 화면에서 출발해도 Controller와 API를 바로 찾을 수 있게 한다.
- Controller에서 출발해도 연결된 화면과 상태 변화를 바로 찾을 수 있게 한다.
- 현재 샘플데이터 기반 화면과 실제 API 연결 화면을 분리해서 본다.

## 이번 리팩토링 범위
- 기존 View와 기존 Controller를 기준으로 문서화한다.
- 기존 파일명과 namespace는 바꾸지 않는다.
- 화면 내부 요소 ↔ Controller API 함수 매핑을 먼저 정리한다.
- 실제 서버 연결이 없는 화면은 메모리/샘플데이터 상태를 명시한다.

## 문서 구조
- `01_공통_매핑템플릿.md`
  - 모든 View 문서와 Controller 문서가 따를 공통 형식
- `DriverApp/`
  - 기사 화면 기준 View ↔ Controller 매핑
- `ShipperApp/`
  - 화주 화면 기준 View ↔ Controller 매핑
- `Server/`
  - Controller 기준 역방향 매핑
- `00_전체인덱스.md`
  - 전체 View ↔ Controller ↔ API 인덱스

## 상태 표기 규칙
- `API 연결` : 화면이 실제 서버 API를 호출한다.
- `샘플데이터` : 화면이 메모리/샘플데이터로만 동작한다.
- `혼합` : 일부는 API, 일부는 샘플데이터다.
- `후속연결` : 대응 API는 있으나 아직 화면에서 직접 연결하지 않았다.

## View 기준 매핑 원칙
1. 화면 목적을 먼저 적는다.
2. 주 대응 Controller 1개를 먼저 적는다.
3. 필요하면 보조 Controller를 추가한다.
4. 화면 요소별로 어떤 API 함수를 쓰는지 표로 연결한다.
5. 상태 변화와 예외 상태를 같은 문서 안에 넣는다.

## Controller 기준 매핑 원칙
1. Controller 목적과 담당 역할을 적는다.
2. 연결 View 목록을 먼저 적는다.
3. 각 API 함수별 Request/Response와 상태 영향을 적는다.
4. 샘플데이터 화면인지 실제 API 연결 화면인지 함께 적는다.

## 다음 단계
- DriverApp 화면별 매핑 문서 작성
- ShipperApp 화면별 매핑 문서 작성
- Server Controller 역방향 매핑 문서 작성
- 전체 인덱스 작성
