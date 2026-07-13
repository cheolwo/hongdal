# Screen Flows

화면에 보이는 버튼과 카드가 내부적으로 어떻게 처리되는지 정리합니다. 일부 화면은 아직 샘플 서비스로 상태를 갱신하며, 서버 연동 시 같은 지점에서 Command/API 호출로 바뀝니다.

## 통합 홈과 3단계 업무 진입

`UnifiedHome`은 저장된 역할에 맞춰 역할 홈을 선택합니다. 사용자는 역할별 사방괘에서 영역을 고르고, 다이어그램에서 노드 관계를 파악한 뒤 노드 행동 메뉴로 구체 데이터 페이지를 엽니다. 자세한 규칙은 [3단계 내비게이션](../Architecture/ThreeStageClientNavigation.md)에서 봅니다.

```mermaid
flowchart TD
    A["우측 상단 사람 버튼"] --> B["역할 선택"]
    B --> C["1단계 · 역할별 사방괘 갱신"]
    C --> D["사방괘 방향 선택"]
    D --> E["2단계 · 관련 원장 다이어그램"]
    E -->|기본 클릭| F["노드 요약·관계·진행도"]
    E -->|우클릭·길게 누르기·⋮| G["노드 행동 메뉴"]
    G --> H["3단계 · 목록·상세·작업 페이지"]
    H -->|뒤로| E
```

현재 일부 사방괘 방향은 3단계 라우트로 바로 이동한다. 다이어그램 진입 문맥과 공통 노드 행동 메뉴를 연결한 뒤 위 흐름을 완료 상태로 판정한다.

## DriverApp 추천과 배차 처리

DriverApp 홈의 추천 목록, 추천 상세, 배차 처리 화면은 같은 추천 의뢰 데이터를 기준으로 움직입니다.
네이티브 지도 홈에서는 `IDriverRecommendationNotificationService`가 추천 수신 소스를 감싸고, 현재 샘플 구현은 같은 추천 데이터를 FCM/SignalR/폴링 수신처럼 노출합니다.

```mermaid
flowchart TD
    A["DriverApp 홈 / 추천 목록"] --> B["IDriverSampleDataService.추천의뢰목록"]
    B --> N["IDriverRecommendationNotificationService"]
    N --> M["네이티브 지도 추천 수신 패널"]
    B --> C["추천 상세 화면"]
    C --> D["배차 처리 화면"]
    M --> E
    D --> E{"기사 선택"}
    E -->|수락| F["Accept: 배차상태=수락, 상태=수락완료"]
    E -->|보류| G["Hold: 배차상태=보류, 상태=검토중"]
    E -->|거절| H["Reject: 배차상태=거절, 상태=추천제외"]
    F --> K{"수락 이후"}
    K -->|계속 진행| I["Changed 이벤트 / 화면 갱신"]
    K -->|취소| L["CancelAccepted: 배차상태=수락취소, 상태=재배차필요"]
    G --> I
    H --> I
    L --> I
    I --> J["서버 전환 시 Command 처리와 배차 이벤트 발행"]
```

서버 전환 시 배차 결정 후속 처리는 다음 골격을 따른다.

| 결정 | 서버 후속 처리 |
| --- | --- |
| 수락 | `POST api/v1/driver/dispatch-actions/{requestId}/accept`, 추천 잠금, 배차 후보 생성, 화주 수락 알림 |
| 보류 | 기사 개인 검토 상태 유지, 서버 배차 확정은 하지 않음 |
| 수락 취소 | `POST api/v1/driver/dispatch-actions/{requestId}/cancel-acceptance`, 추천 잠금 해제, 재배차 열기, 화주 취소 알림, 취소 사유 감사 기록 |
| 거절 | `POST api/v1/driver/dispatch-actions/{requestId}/reject`, 해당 기사 후보 제외, 후보 기사 재계산, 거절 사유 감사/추천 품질 보정 |

`배차수락취소Command`는 배차 큐를 다시 `대기/배차추천/추천대기`로 되돌리고, 기존 기사는 제외 후보로 남긴 뒤 다음 추천 후보를 찾는 골격을 가진다.

## DriverApp 상차/하차 완료 사진 처리

상차 완료와 하차 완료 화면은 모바일 카메라에서 사진을 받은 뒤 완료 처리와 연결됩니다.

```mermaid
flowchart TD
    A["상차 화면 또는 하차 화면"] --> B["카메라 촬영 / 사진 선택"]
    B --> C["DriverTransportCompletionPhoto 생성"]
    C --> D["IDriverTransportCompletionPhotoService.CompleteWithPhotoAsync"]
    D --> E{"구현 방식"}
    E -->|Sample| F["driver-transports/{id}/pickup-complete 또는 dropoff-complete 경로 계산"]
    E -->|HTTP| G["POST api/v1/files/upload"]
    G --> H["commandName + referenceId + 파일 저장"]
    H --> I{"사진 종류"}
    I -->|상차| J["POST api/v1/driver/transports/{id}/pickup-complete"]
    I -->|하차| K["POST api/v1/driver/transports/{id}/complete"]
    J --> L["운송 상태 갱신 / 이벤트 후속처리"]
    K --> L
```

## WarehouseManagerApp 작업 진입

창고 앱의 입고와 포장 흐름은 먼저 휴대폰 번호 뒤 8자리로 작업자를 확인하고, 다음 화면에서 작업대 바코드를 확인합니다.

```mermaid
flowchart TD
    A["입고/출고/포장 작업 시작 화면"] --> B["휴대폰 번호 뒤 8자리 입력"]
    B --> C["IWarehouseWorkEntryGateService.VerifyAsync"]
    C --> D{"작업자/역할 검증"}
    D -->|실패| X["오류 표시 / 다음 화면 차단"]
    D -->|성공| E{"공정 유형"}
    E -->|입고| F["입고 작업대 바코드 화면"]
    E -->|포장| G["포장 작업대 바코드 화면"]
    E -->|출고/기타| H["작업 보드 이동"]
    F --> I["WB:IN-* 작업대 확인"]
    G --> J["WB:PK-* 작업대 확인"]
    I --> K["상품 바코드 스캔"]
    K --> L["입고 묶음 바코드 스캔"]
    L --> M["입고 예정 매칭 / 현장 임시 입고 편입"]
    M --> N["입고 검수 화면"]
    J --> H
```
