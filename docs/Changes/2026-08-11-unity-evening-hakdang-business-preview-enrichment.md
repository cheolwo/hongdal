# Unity 저녁 학당 업무 Preview 보강

## 결과

기존 저녁 학당에서 습득한 바보의 `BeginnerMind`와 전차의 `IntegratedProgress`를 다음 날 업무 Preview가 소비할 수 있는 순수 보강 계층을 추가했다. 카드는 canonical 업무 상태를 바꾸지 않고 플레이어가 보는 미확인 질문 또는 물류 milestone만 보강한다.

## 흐름

```text
Simulation수출항만인수PreviewSnapshot JSON
  → Unity 수출항만인수PreviewApiModel
  → 수출항만인수학당PreviewAdapter
  → 플레이어 ActiveRuleCodes
  → 오늘의 FocusedRuleCode 한 장 선택
  → canonical 업무 Preview 입력
  → 저녁학당업무Preview보강Projector
  → 바보: RevealedUnknowns
     전차: MilestoneEvidence
  → 원본 Preview·수량·계보·허용 intent는 불변
```

## 구현 범위

- `저녁학당업무Preview보강Input`은 Preview stable ID·expected revision·업무 단계·상품·수량·단위·canonical source lineage를 보존한다.
- 보유하지 않은 규칙을 `FocusedRuleCode`로 지정하면 거부한다.
- 여러 규칙을 보유하더라도 한 번에 지정한 한 장만 보강에 적용한다.
- 바보는 미확인 질문만 반환하고 전차는 순서가 고정된 milestone만 반환한다.
- 반환 객체는 canonical state와 허용 intent를 바꿀 수 없다는 flag를 항상 `false`로 둔다.
- 서버 route `api/simulation/v1/sessions/{sessionStableId}/export-port-receipt-previews`와 실제 JSON field를 Unity transport model에 고정했다.
- test에서 서버 contract를 JSON round-trip하고 Unity runtime assembly의 참조 목록을 검사해, runtime project가 서버 contract assembly를 참조하지 않으면서 wire parity를 지키는지 검증한다.
- 항만 인수 adapter는 `product:potato`, Cargo 300kg, KGM, Cargo·HarvestLot·PackageLot·배분·인계·시설·Decision·Task 계보를 보존한다.
- 서버 block reason이 하나라도 있거나 필수 운영 경계 일곱 개가 빠지면 카드 효과 적용 전에 거부한다.

## 경계와 남은 작업

- 현재 구현은 engine-independent Unity data core의 순수 projection이다.
- 실제 `EXPORT-PORT-RECEIVING-1` wire DTO adapter까지 연결했지만 HTTP client·repository live 호출은 아직 없다.
- Unity Scene·Presenter·Game View는 변경하지 않았다.
- 게시 승인 catalog, Notion·Blob publication snapshot과 LLM provider는 호출하거나 구현하지 않았다.
- 다음 우선순위는 검수된 콘텐츠만 runtime catalog에 게시하는 `CARD-BIZ-0`이다.

## 검증

- 변경 전 저녁 학당 집중 테스트: 11/11 통과
- P0 공통 보강기까지 집중 테스트: 16/16 통과
- P1 실제 항만 인수 wire adapter까지 집중 테스트: 22/22 통과
- P0·P1 신규 test: 각각 5개·6개, 합계 11개
- Unity .NET 전체 테스트: 350/350 통과
- scoped Fast: build와 `git diff --check` 통과
- scoped Task: solution build 통과, 전체 테스트는 기존 route·metadata·CSS 기대 7건으로 4,501/4,508 통과
- 화면 변경 없음
- 실제 서버 live 호출·LLM/provider 호출·commit·push 없음
