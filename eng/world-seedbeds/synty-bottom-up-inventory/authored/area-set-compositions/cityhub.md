# City/Hub AreaSet 구성 패턴

City/Hub는 도착한 화물이 하차, 검수, 보관, 피킹과 출고를 거쳐 다음 지역으로 이어지는 물류 업무 영역이다.

## 기준안 — 입고·보관·출고 선형

```text
입고·하차
  ↓
품질·격리·저온 보관
  ↓
정비·비상 운영
  ↓
출고 이행·상차
  ↓
입고 운영 복귀
```

| 역할 슬롯 | 선택 H3 | 플레이 의미 |
| --- | --- | --- |
| InboundOutbound | `jinbu-hub` | 하차·검수·보관·기본 출고 |
| ResilientStorage | `resilient-logistics-hub` | 격리·저온·우회 보관 |
| MaintenanceEmergency | `hub-maintenance-emergency-loop` | 정비·비상 복구 |
| FulfillmentOperations | `hub-fulfillment-operations` | 피킹·출고 준비·상차 |

## 변형안 — 격리·비상 우회형

격리·복원력 보관을 중앙에 두고 정비 구역을 통해 출고 구역으로 우회한다. 기존 입고 화물과 신규 출고의 동선 충돌을 줄이는 설계 후보다.

## 관련 WI

`WI-LOG-04~05`, `WI-001~002`, `WI-HUB-03~06`을 수용한다.
