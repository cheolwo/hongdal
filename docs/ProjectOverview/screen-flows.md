# Screen Flows

화면에 보이는 버튼과 카드가 내부적으로 어떻게 처리되는지 정리합니다. 일부 화면은 아직 샘플 서비스로 상태를 갱신하며, 서버 연동 시 같은 지점에서 Command/API 호출로 바뀝니다.

## 공통 홈 모드 전환

`PlatformCommunityHome`은 앱별 홈을 커뮤니티 모드와 업무 모드로 나눕니다. 모드 버튼은 공통 상태 서비스의 `IsWorkMode` 값을 바꾸고, 각 앱은 같은 상태값을 보고 커뮤니티 콘텐츠 또는 업무 콘텐츠를 렌더링합니다.

```mermaid
flowchart TD
    A["사용자: 커뮤니티/업무 모드 버튼 선택"] --> B["PlatformModeBar / NavMenu"]
    B --> C["PlatformHomeModeStateService.SetWorkMode"]
    C --> D["Changed 이벤트 발행"]
    D --> E["PlatformCommunityHome 상태 동기화"]
    E --> F{"IsWorkMode"}
    F -->|false| G["커뮤니티 게시판 / 공지 / 공유글 표시"]
    F -->|true| H["앱별 업무 대시보드 표시"]
```

## DriverApp 추천과 배차 처리

DriverApp 홈의 추천 목록, 추천 상세, 배차 처리 화면은 같은 추천 의뢰 데이터를 기준으로 움직입니다.

```mermaid
flowchart TD
    A["DriverApp 홈 / 추천 목록"] --> B["IDriverSampleDataService.추천의뢰목록"]
    B --> C["추천 상세 화면"]
    C --> D["배차 처리 화면"]
    D --> E{"기사 선택"}
    E -->|수락| F["Accept: 배차상태=수락, 상태=수락완료"]
    E -->|보류| G["Hold: 배차상태=보류, 상태=검토중"]
    E -->|거절| H["Reject: 배차상태=거절, 상태=추천제외"]
    F --> I["Changed 이벤트 / 화면 갱신"]
    G --> I
    H --> I
    I --> J["서버 전환 시 Command 처리와 배차 이벤트 발행"]
```

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
    K --> L["입고 예정 매칭 / 현장 임시 입고 편입"]
    L --> M["입고 검수 화면"]
    J --> H
```
