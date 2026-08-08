# Urban Market Primitive Vertical Slice

이 sample은 실제 Unity project가 없는 현재 repository에서도 도심마트 Presentation 계약을 보존하기 위한 importable sample이다.

## 포함 범위

- `도심마트SceneController`
- `도심마트View`
- 상품진열대 3개
- 상품상자, 가격표, 재고 상태와 정보 키오스크 View socket
- 상품 선택 시 상세 정보 panel
- `Simulated도심마트조회UseCase`를 사용하는 명시적 simulation fixture
- 기존 공개 aggregate `GET api/v1/orderer/mart/products`용 operational ApiClient·Mapper·Repository·UseCase
- VContainer에서 simulation과 operational 구성을 명시적으로 선택하는 LifetimeScope
- primitive scene을 생성하고 Inspector reference를 연결하는 Editor builder

결제, 주문 Command와 외부 asset은 포함하지 않는다. operational 모드는 서버가 공개 가능하다고 판정한 상품·판매가·판매 가능 수량·재고 기준시각만 읽으며 내부 창고 재고, 주소, 연락처, 결제·계약 정보는 요청하지 않는다.

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

실제 API를 연결할 때는 `도심마트LifetimeScope.ConfigureOperationalApi()` 또는 Inspector에서 operational 모드와 API origin을 지정한다. API 실패를 simulation fixture로 대체하지 않으며 DTO와 Repository를 View 또는 Controller에 전달하지 않는다. 상품 선택은 읽기 전용 상세 panel만 열고, 주문은 별도 확인 panel → server UseCase → canonical 재조회가 구현되기 전까지 실행하지 않는다.
