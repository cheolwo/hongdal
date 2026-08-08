# Urban Market Primitive Vertical Slice

이 sample은 실제 Unity project가 없는 현재 repository에서도 도심마트 Presentation 계약을 보존하기 위한 importable sample이다.

## 포함 범위

- `도심마트SceneController`
- `도심마트View`
- 상품진열대 3개
- 상품상자, 가격표, 재고 상태와 정보 키오스크 View socket
- 상품 선택 시 상세 정보 panel
- `Simulated도심마트조회UseCase`를 사용하는 명시적 simulation fixture
- primitive scene을 생성하고 Inspector reference를 연결하는 Editor builder

실제 서버 API, operational 재고, 결제, 주문과 외부 asset은 포함하지 않는다.

## 사용

1. Package Manager에서 `Ssalddel Simulation Data` package의 sample을 import한다.
2. Unity 메뉴에서 `Ssalddel/Samples/Create Urban Market Primitive Scene`을 실행한다.
3. 생성된 `Assets/Ssalddel/Scenes/UrbanMarketPrimitive.unity`를 연다.
4. PlayMode에서 진열대 3개, 상품·가격·재고·출처 표시와 상품 선택 panel을 확인한다.

CLI에서는 Unity project 경로를 지정하여 다음 Editor method를 실행할 수 있다.

```text
Unity -batchmode -quit -projectPath <UnityProject> \
  -executeMethod Ssalddel.Unity.Samples.UrbanMarket.Editor.도심마트PrimitiveSceneBuilder.CreateScene
```

실제 API를 연결할 때는 `도심마트LifetimeScope`의 `I도심마트조회UseCase` 등록을 Repository·Mapper 기반 구현으로 교체한다. DTO와 Repository를 View 또는 Controller에 전달하지 않는다.
