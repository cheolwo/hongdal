# Town AreaSet 구성 패턴

Town은 주거, 생활광장, 시장, 주민 서비스와 수령이 하나의 보행 생활권으로 읽혀야 하는 업무 영역이다.

## 기준안 — 저층 생활광장·시장형

```text
저층 주거·시장
  ↓
순환 시장
  ↓
오염 통제·구호
  ↓
주민 서비스
  ↓
마트 운영·수령
  ↓
생활권 복귀
```

| 역할 슬롯 | 선택 H3 | 플레이 의미 |
| --- | --- | --- |
| LowriseMarket | `lowrise-market-town` | 주거·광장·시장 진입 |
| CircularMarket | `circular-market-town` | 반품·회수·순환시장 |
| ContaminationRelief | `town-contamination-relief` | 통제·구호·안전 복귀 |
| ResidentService | `town-resident-service-loop` | 생활 서비스·공동수령 |
| MarketFulfillment | `town-market-fulfillment` | 입고·피킹·포장·수령 |

## 변형안 — 오염 통제·구호형

통제·구호 구역을 중앙 허브로 두고 주거와 시장 동선을 분리한다. 구성 패턴은 사건 자체가 아니며 전환 여부는 추후 서버 선택 상태가 결정한다.

## 관련 WI

`WI-MARKET-02~05`, `WI-ORDER-03~06`을 수용한다.
