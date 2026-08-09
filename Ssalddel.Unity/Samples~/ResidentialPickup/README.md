# Residential Pickup Primitive Vertical Slice

주거공동체 공동수령 공간 하나를 유지하면서 서버가 승인한 역할에 따라 같은 object를 주문자의 `내 수령 상품` 또는 운송자의 `내 하차 대상`으로 표현하는 sample이다.

## 포함 범위

- 개인정보가 제거된 `ResidentialPickupPerspective` 계약
- 주문자 본인 수령과 운송자 배정 하차를 분리한 고정 API route
- `SimulatedFixture`와 인증 token 기반 operational client
- stable ID 기반 공동수령 Point View와 역할별 badge
- Orderer/Transporter 역할 전환 socket
- VContainer composition root
- primitive Scene Builder와 저장 후 wiring validator

주소, 상세주소, 연락처, 사용자 ID, 주문번호, 결제·계약 정보는 Unity projection에 포함하지 않는다. 역할 전환은 다른 역할의 권한을 획득하는 기능이 아니며 operational 모드에서는 선택한 역할에 대응하는 고정 server endpoint가 실제 인증 관계를 다시 검증한다.

현재 interaction은 읽기 전용이다. Unity 클릭이나 도착으로 수령 확인 또는 하차 완료 Command를 호출하지 않는다.

```text
Ssalddel/Samples/Create Residential Pickup Primitive Scene
Ssalddel/Samples/Validate Residential Pickup Primitive Scene
```
