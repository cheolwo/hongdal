# Command/Event 리팩토링 원칙

## 목적
지금 여기 Hongdal 서버에서 우리는 핵심 업무 상태 변경과 사후처리를 분리해서, Handler 비대화를 줄이고 유지보수 속도를 높인다.

## 기본 원칙
1. CommandHandler는 사용자의 명령을 검증하고 핵심 도메인 상태를 변경한다.
2. CommandHandler는 상태 변경 저장이 끝난 뒤 의미 있는 사건(Event)을 발행한다.
3. EventHandler는 이미 발생한 사건에 대한 사후처리를 담당한다.

## CommandHandler에 남기는 범위
- 입력 검증
- 권한 확인
- 핵심 상태 전이
- 금액/정책 확정
- DB 저장
- 트랜잭션 처리
- 중복 처리 방지

## EventHandler로 분리하는 범위
- 푸시/SMS/SNS 발송
- 운영 로그/감사 로그 기록
- 위젯/화면 상태 갱신
- 통계 적재
- 추천 큐 후속 갱신
- 혜택 자격 발급

## 경계 판단 기준
- 실패하면 업무 상태가 깨지는 처리: CommandHandler 또는 동일 트랜잭션에 둔다.
- 실패해도 재시도로 복구 가능한 처리: EventHandler로 분리한다.

## 결제-배차 경계 예외
Hongdal 정책상 `결제완료된 의뢰만 배차대기에 진입`이 핵심 규칙이므로, 결제 승인과 배차대기 생성은 핵심 트랜잭션 경계로 유지한다.

반면 아래는 EventHandler로 분리한다.
- 결제완료 알림
- 운영 로그
- 통계 반영
- 비핵심 위젯 갱신

## 1차 적용 범위 (2026-07)
- `배차수락CommandHandler`
  - 핵심 처리만 유지: 검증, 상태 변경, 저장
  - 분리: 수락 로그 적재/큐 전환/운영 로그 → `배차수락됨Event` 구독 핸들러
- `배차거절CommandHandler`
  - 핵심 처리만 유지: 거절 저장
  - 분리: 큐 전환/운영 로그 → `배차거절됨Event` 구독 핸들러

## 2차 적용 범위 (2026-07)
- `운송인수완료CommandHandler`
  - 핵심 처리만 유지: 운송 조회, 상태 전이, 저장
  - 분리: 인수증 자동 생성/운영 로그 → `운송인수완료됨Event` 구독 핸들러

## 3차 적용 범위 (2026-07)
- `운송상차지도착CommandHandler`
  - 핵심 처리만 유지: 운송 조회, 상태 전이, 저장
  - 분리: 운영 로그 → `운송상차지도착됨Event` 구독 핸들러
- `운송상차완료CommandHandler`
  - 핵심 처리만 유지: 운송 조회, 상태 전이, 저장
  - 분리: 운영 로그 → `운송상차완료됨Event` 구독 핸들러
- `운송하차지도착CommandHandler`
  - 핵심 처리만 유지: 운송 조회, 상태 전이, 저장
  - 분리: 운영 로그 → `운송하차지도착됨Event` 구독 핸들러

## 다음 적용 우선순위
1. 결제 후속(알림/통계/운영 로그) 이벤트 분리 강화
2. 콘텐츠 시청 완료 후 혜택/알림/통계 분리
3. 운송 문제신고 후속 알림/운영로그 분리
4. Outbox 기반 재시도 일관화

## 화주 앱 적용 범위 (2026-07)

MAUI 화주 앱의 샘플/프로토타입 서비스에도 동일한 경계를 적용한다. 현재는 앱 내부 `IAppCommandHandler` / `IAppEventHandler` 기반 경량 인프라를 사용하며, 서버 전환 시 MediatR 또는 Outbox 기반 발행기로 교체할 수 있게 둔다.

### 적용 완료

- `AddShipperRequestCommand`
  - 핵심 처리: 운송의뢰 저장
  - 이벤트: `ShipperRequestAddedEvent`
  - 후속 처리: 이벤트 로그, 수출입 흐름이면 `RequestCustomsHsReviewCommand`로 HS 검토 요청 생성

- `ProcessCommerceOrderCommand`
  - 핵심 처리: 외부 주문 중복 방지, 국내/해외 분류, 출고 예약, 피킹 계획, 창고 출고 알림 생성
  - 이벤트: `CommerceOrderProcessedEvent`
  - 후속 처리: 이벤트 로그

- `CreateChannelListingCommand`
  - 핵심 처리: 채널출품 생성, 채널별 payload 준비, 동기화 상태 갱신
  - 이벤트: `ChannelListingCreatedEvent`
  - 후속 처리: 이벤트 로그

- `CreateReconsignmentOrderCommand`
  - 핵심 처리: 재고 차감, 재위탁 운송의뢰 생성
  - 이벤트: `ReconsignmentOrderCreatedEvent`
  - 후속 처리: 이벤트 로그

- `RequestCustomsHsReviewCommand`
  - 핵심 처리: 수출입 흐름 판정, HS 후보 생성, 관세사 검토 요청 생성, 중복 방지
  - 이벤트: `CustomsHsReviewRequestedEvent`
  - 후속 처리: 이벤트 로그

- `AssignCustomsBrokerCommand`
  - 핵심 처리: 관세사 배정 상태 변경
  - 이벤트: `CustomsBrokerAssignedEvent`
  - 후속 처리: 이벤트 로그

- `CompleteCustomsHsReviewCommand`
  - 핵심 처리: HS 코드 확정, 검토 완료 상태 변경
  - 이벤트: `CustomsHsReviewCompletedEvent`
  - 후속 처리: 이벤트 로그
