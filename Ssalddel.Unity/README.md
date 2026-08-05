# Ssalddel Unity Simulation Data

살뜰 Unity 농업·유통·경영 시뮬레이션의 데이터 우선 코어다. Unity scene이나 `UnityEngine`에 의존하지 않는 로컬 UPM package이며 `netstandard2.1`로도 빌드된다.

## 현재 구현

- Server DTO와 공유 assembly를 참조하지 않는 Unity API model
- API model을 game model로 바꾸는 명시적 Mapper
- stable ID, schema, 단위, provenance, 품목 mapping과 package hash 검증
- `Live`, `Cached`, `Fixture`, `Invalid`, `Failed`를 구분하는 `DataManager`
- 같은 scenario package와 Command로 같은 결과를 만드는 농업 simulation engine
- 성장, 수분, 생산비, 수확과 일반판매·공동판매 비교
- 실제 주문·참여·서버 원장을 만들지 않는 `SIMULATED` 경계

## 구조

```text
Runtime/
  ApiModels/    Unity가 HTTP JSON을 받는 transport model
  Mapping/      ApiModel → GameModel 변환과 호환성 판정
  Data/         catalog, provenance, validation, DataManager
  Simulation/   Command, Event, state와 결정적 계산
```

`Ssalddel.Unity.Data.asmdef`는 `noEngineReferences=true`이므로 데이터 코어가 GameObject나 scene에 의존하지 않는다. Unity project를 만들면 이 폴더를 local package로 추가하고 별도 presentation assembly가 이 package를 참조한다.

## 검증

```powershell
dotnet build Ssalddel.Unity/Ssalddel.Unity.csproj
dotnet test Ssalddel.Unity.Tests/Ssalddel.Unity.Tests.csproj
```

golden fixture는 `Ssalddel.Unity.Tests/Fixtures/potato-basic-kr-001.v1.json`에 있다. 이 값은 KAMIS나 기상청의 실제 관측값이 아니라 실제 contract 형태를 검증하는 교육용 `Fixture`다.

현재 환경에는 Unity Editor가 설치되어 있지 않아 Unity Test Runner, PlayMode, scene 렌더링은 아직 검증하지 않았다.
