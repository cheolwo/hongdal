# Unity Synty City Pack 도입 체크리스트

## 목적

City Pack은 Ssalddel 업무 모델이 아니라 교체 가능한 Unity Presentation 리소스다. 원본 prefab에 stable ID, 권한, 주문·계약 Command나 Simulation 계산을 넣지 않는다.

## 도입 전 완료 상태

- `ConceptCardView`와 Concept·Status·Reason·Action skin 경계
- 공동주택 대표 NPC 선택과 7장 card deck
- 대표 wrapper의 별도 `VisualRoot`, selection collider, NavMeshAgent와 Animator socket
- `Speed`와 `WaitForManagerReview` Mecanim parameter
- manager desk와 `market.entrance → market.manager-desk` waypoint
- 임시 Scene의 NavMeshData와 wiring EditMode 3/3
- 물류센터의 차량 접근·입고 Dock·검수·보관 4영역 overview와 건물·화물 `VisualRoot`
- Unity core 222/222 회귀와 물류센터 imported sample EditMode 3/3

위 항목은 구매 전 asset-neutral 준비 단계의 상태다. 아래 도입 결과에서 별도 실험 Scene을 저장해 실제 asset 교체를 검증했다.

## 실제 도입 결과 (2026-08-09)

- City Pack은 `C:\Users\user\ssalddel\Assets\Synty\PolygonCity`에 원본 구조로 import했다.
- 프로젝트 전용 builder는 원본 prefab을 수정하지 않고 기존 primitive sample을 만든 뒤 `VisualRoot`만 교체한다.
- 도심마트 저장 Scene에 상점·공동주택·대표·관리자·책상·진열대를 배치했다.
- 물류센터 저장 Scene에 시설 facade·차량·pallet·상자를 배치했다.
- 대표·관리자 캐릭터의 Humanoid Avatar와 기존 View socket 연결을 검증했다.
- City Pack의 `Synty/Generic_Basic` shader가 현재 PC URP asset에서 오류 shader로 대체되지 않음을 확인했다.
- 정적 builder validation, 마트·물류센터 Play Mode와 두 imported sample EditMode를 통과했다.
- 마트 Play Mode에서 발견한 `도심마트ManagerPresentationContext` VContainer 등록 누락을 canonical sample과 imported copy에 보완했다.
- City Pack에는 농장 토양·작물 tile과 AnimationClip·AnimatorController가 없어 FARM-2~FARM-5와 실제 걷기·작업 animation에는 사용하지 않는다.

저장 Scene:

- `Assets/Ssalddel/Experiments/SyntyCityPackIntegration/UrbanMarketCityPackVerticalSlice.unity`
- `Assets/Ssalddel/Experiments/SyntyCityPackIntegration/UrbanLogisticsCityPackVerticalSlice.unity`

## 구매 후 첫 교체 범위

1. [x] City Pack 원본 폴더 유지
2. [x] 대표와 관리자 prefab을 별도 Scene instance로 배치
3. [x] `공동주택대표NpcView.VisualRoot` 아래 외형만 교체
4. [x] 물류센터 건물·화물 `VisualRoot`와 운송 차량 외형 교체
5. [x] Humanoid Avatar와 기존 Animator socket 확인
6. [x] wrapper의 collider·NavMeshAgent와 asset scale 분리 유지
7. [x] 저장된 마트 Scene의 Game View와 Play Mode 확인
8. [x] 저장된 물류센터 Scene의 Game View·Play Mode와 handoff View 회귀 확인
9. [ ] 실제 walk/work clip 연결과 이동 중 Animator 재생 확인
10. [ ] Android target material·draw call·메모리 기록

## 통과 기준

- Synty 이름·prefab path가 Data, Interpretation, Simulation과 서버 계약에 나타나지 않는다.
- 외형을 primitive로 되돌려도 같은 stable ID, 카드와 업무 상태가 유지된다.
- NPC 도착과 animation event는 주문·계약·결제 Command를 실행하지 않는다.
- Operational API 실패를 Simulation fixture로 대체하지 않는다.
- 구매한 asset의 license와 seat 범위를 저장소 밖 운영 기록으로 확인한다.

## 이번 단계에서 제외

- Shops Pack과 Shopping Plaza 구매
- 실제 주문·계약·결제 연결
- Synty 원본 prefab 수정
- 전체 Zone 일괄 교체
- 농장 토양·작물 asset 대체
- Android build와 제품 배포
