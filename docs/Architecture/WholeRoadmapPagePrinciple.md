# 전체 로드맵 조화형 페이지 원칙

## 목적

Ssalddel의 페이지 하나는 특정 버전의 고립된 화면이 아니라 `0.0`에서 최종 배포 목표 `3.5`까지 이어지는 업무 흐름의 한 node다. 현재 화면이 제공하는 기능은 현재 완성 단계와 기능 플래그로 제한하지만, 식별자·원장·상태·역할 인계는 후속 버전과 단절되지 않게 설계한다.

현재 완성 단계는 `SsalddelProductRoadmapCatalog.CurrentDeliveryVersion`, 최종 배포 목표는 `SsalddelProductRoadmapCatalog.DeploymentTargetVersion`을 단일 코드 기준으로 사용한다.

## 페이지 구현 기준

새 페이지 또는 의미 있는 페이지 리팩토링은 구현 전에 다음을 정한다.

1. **업무 node**: 페이지가 소유하는 한 가지 업무 질문과 결과
2. **도입 버전**: 이 node가 처음 필요한 제품 버전
3. **선행 근거**: 앞 단계에서 전달받는 stable ID, 동의, 출처와 원장 상태
4. **후속 인계**: `3.5`까지 어떤 역할과 원장이 이 결과를 소비하는지
5. **역할 투영**: 주문자·화주·기사·창고 등 역할별로 같은 상태를 어떻게 다르게 표시하는지
6. **실행 경계**: 읽기, Simulation, platform persistence, 외부 효과를 구분하는 기능 플래그와 권한
7. **실패와 철회**: loading, empty, error, retry, disabled와 철회 가능한 상태
8. **검증 근거**: route, PageViewModel, contract, API/UseCase, 원장 재조회와 소비 역할 테스트

## 코드 구조

```text
Route
  -> Page/Screen
    -> PageViewModel
      -> Feature ViewModel 또는 Workflow Session
        -> Client
          -> Controller API
            -> UseCase/Command
              -> Domain/Infrastructure
                -> Ledger/Event/Outbox
```

- Route는 파라미터와 navigation만 조립한다.
- PageViewModel은 페이지 수명, 표시 상태와 하위 workflow를 조립한다.
- 여러 페이지에서 유지되는 초안은 PageViewModel이 아니라 별도 Workflow Session으로 둔다.
- 실제 상태 변경은 API/UseCase/Command가 권한과 현재 원장을 검증한 뒤 수행한다.
- 성공 뒤 같은 stable ID의 원장을 다시 조회해 다른 역할 앱과 동일한 상태를 표시한다.

## 버전과 배포 경계

- 최종 배포 목표는 `3.5`다.
- 현재 완성 단계가 `0.0`이라는 사실은 최종 목표를 축소하지 않는다.
- 미래 버전의 연결점은 contract와 원장에 보존하되, 해당 기능 플래그가 꺼져 있으면 UI와 API 실행을 열지 않는다.
- 기능 플래그가 켜져 있어도 `SsalddelExecution:Mode=Simulation`이면 결제, 계약, 신고, 자동 배차와 외부 정산을 운영 효과로 실행하지 않는다.
- `3.5` 배포 후보는 `Ssalddel.v3.5.slnx`, 3.5 Compose profile과 전체 릴리즈 게이트를 함께 통과해야 한다.

## 회귀 방지

페이지 capability에는 stable `PageKey`, `IntroducedVersion`, workflow와 feature key, 상호작용 경계, 인증·외부 효과 여부를 기록한다. 여러 프로젝트를 통과하는 기능은 `SsalddelCodeMetadataAttribute`의 `FeatureKey`, `Layer`, `FlowOrder`, `Effects`, `Boundary`로 같은 세로 흐름을 검색할 수 있어야 한다.

화면 단위 테스트만으로 완료를 주장하지 않는다. 최소한 현재 버전의 동작, 후속 인계 contract, 비활성 기능 차단, 동일 원장 재조회와 `3.5` solution 조립 호환성을 함께 확인한다.
