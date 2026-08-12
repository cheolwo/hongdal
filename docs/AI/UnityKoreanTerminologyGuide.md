# Unity 프로젝트 한국어 중심 용어·출력 지침

## 1. 목적

Unity·서버 통합 프로젝트의 문서와 작업 보고를 코드를 모르는 사람도 이해할 수 있는 자연스러운 한국어로 작성한다. 영어를 기계적으로 제거하지 않고, 기술 고유명과 안정된 코드 계약은 보존하면서 프로젝트 자체 개념을 한국어로 먼저 설명한다.

이 문서는 사람이 읽는 표현의 단일 기준이다. 코드 식별자의 언어 기준은 [코드 탐색 메타데이터의 코드 명명 언어](../Architecture/SsalddelCodeMetadata.md#코드-명명-언어)를 함께 따른다.

## 2. 적용 대상

다음 내용은 기본적으로 한국어로 작성한다.

- 구현 결과와 진행 상황
- 설계·아키텍처 설명
- 검증 결과와 실패 원인
- README, 상태 문서, 제안서
- Game View 검증 설명
- 배치 객체의 역할과 데이터 공개 범위
- 서버의 최종 결정 권한과 시뮬레이션·실운영 경계
- TODO와 다음 우선순위
- 사람이 읽는 코드 주석과 테스트 결과 설명

영어 문장이나 영어 약어 나열을 기본 출력으로 사용하지 않는다.

## 3. 영어를 유지하는 기술 고유명

다음은 억지로 번역하지 않는다.

- 제품·도구: Unity, C#, .NET, ASP.NET, VContainer, Synty, Git, GitHub, Codex, MAUI, URP, NavMesh
- Unity·C# API: GameObject, MonoBehaviour, ScriptableObject, Transform, Animator, Rigidbody, CharacterController, Prefab, Scene, Game View, Play Mode, EditMode, UnityWebRequest
- 통신·표준: HTTP, API, DTO, JSON, REST, SHA-256, GUID
- 기존 클래스명·고유 식별자·API 경로·JSON 필드·저장 필드·오류 코드·단위·`.meta` GUID

기존 `SeedbedObjectRoot`, `MarketInventory`, `ShelfTask`, `SimulationWorldShell`, `CanonicalProductHarvestCargo` 같은 코드 이름은 호환성 근거 없이 바꾸지 않는다. 사람이 읽는 설명에서 먼저 한국어 의미를 적고 필요할 때 실제 코드 이름을 괄호나 코드 표기로 덧붙인다.

## 4. 프로젝트 기본 용어

| 기존 표현 | 기본 한국어 표현 |
| --- | --- |
| Manifest | 구성 대장 |
| Story | 업무 흐름 |
| Object | 배치 객체 |
| Seedbed Object | 모판 배치 객체 |
| Stable ID | 고유 식별자 |
| Binding, DataBinding | 데이터 연결 |
| Snapshot | 상태 사본 |
| Projection | 관점별 조회 결과 |
| Projector | 조회 결과 생성기 |
| Mapper | 변환기 |
| Visual Catalog | 시각 자산 대장 |
| Visual Key | 시각 자산 키 |
| Wrapper Prefab | 프로젝트용 Prefab |
| Socket | 연결 지점 |
| Footprint | 바닥 점유 영역 |
| Bounds | 외곽 범위 |
| Anchor | 배치 기준점 |
| Placement | 배치 |
| Scene Placement | Scene 배치 |
| Placement Receipt | 배치 검증 기록 |
| Gate | 통과 조건, 승격 조건 |
| Evidence | 검증 근거 |
| Runtime | 실행 상태 |
| Runtime Verified | 실행 검증 완료 |
| Preview | 미리보기 |
| Confirm | 확정 |
| Operational | 실운영 |
| Simulation | 시뮬레이션 |
| Research | 연구 |
| Authority | 최종 결정 권한 |
| Canonical | 기준 원장, 최종 기준 |
| Canonical Record | 기준 원장 기록 |
| Canonical Snapshot | 기준 상태 사본 |
| Lineage | 데이터 계보 |
| Cargo Lineage | 화물 계보 |
| Handoff | 인계 |
| Warehouse Handoff | 창고 인계 |
| Scope | 범위 |
| Authorization Scope | 권한 범위 |
| Disclosure Scope | 공개 범위 |
| Revision | 개정 번호, 상태 버전 |
| Source | 출처 |
| Reference Time | 기준 시각 |
| Last Success | 마지막 성공 |
| World Shell | 통합 World |
| Scene Shell | Scene 기본 틀 |
| Presentation | 표현 |
| Presentation Model | 표현 모델 |
| Presentation Layer | 표현 계층 |
| Interpretation | 해석 |
| Interpretation Layer | 해석 계층 |
| Data Layer | 데이터 계층 |
| Movement Presentation | 이동 표현 |
| Workflow | 업무 흐름 |
| Task | 작업 |
| Shelf Task | 진열 작업 |
| Inventory | 재고 |
| Market Inventory | 마트 재고 |
| Operator | 운영자 |
| Market Operator | 마트 운영자 |
| Resident | 주민 |
| Grouping Preview | 집단화 미리보기 |
| Orderer Group Summary | 주문자 집단 요약 |

의미가 불분명할 때만 문서의 첫 등장에 `구성 대장(Manifest)`처럼 영어를 한 번 병기한다. 같은 문서의 두 번째 등장부터는 한국어를 사용한다.

## 5. 모판 구조 설명 기준

사람에게 구조를 설명할 때는 클래스명 나열보다 다음 흐름을 먼저 사용한다.

```text
서버 구성 대장
    ↓
Unity용 데이터 변환
    ↓
현재 상태 사본
    ↓
시각 자산 대장
    ↓
프로젝트용 Prefab
    ↓
실제 World 배치
    ↓
배치 검증 기록
```

업무 흐름은 여러 데이터와 배치 객체가 어떻게 이어지는지를 설명한다. 배치 객체는 건물·시설·가구·차량·화물·인물처럼 Unity Scene에 독립적으로 놓을 수 있는 단위다. 업무 흐름 전체를 하나의 Prefab으로 취급하지 않는다.

## 6. O0~O6 설명 기준

단계 코드만 단독으로 쓰지 않고 의미를 함께 적는다.

- O0: 후보
- O1~O4: 계약·시각·구조 준비 단계
- O5: 모판 실행 검증 완료
- O6: 실제 World 배치 검증 완료

예: `배치 객체 15개 모두 O5 모판 실행 검증까지 완료했으며, 이 가운데 7개는 현재 통합 시뮬레이션 World Scene인 SimulationWorldShell에 배치하여 O6 실제 World 배치 검증까지 완료했다.`

## 7. 서버 사실·해석·Unity 표현의 구분

- 서버는 최종 사실과 업무 권한을 가진다.
- 해석 계층은 서버 상태가 현재 사용자와 상황에서 어떤 의미인지 해석한다.
- Unity 표현 계층은 그 결과를 건물·차량·사람·카드·애니메이션으로 보여준다.

Unity에서 차량이 도착하거나 상자가 이동한 화면은 실제 업무 완료의 근거가 아니다. 실제 완료 여부는 서버의 기준 원장을 다시 조회하여 확인한다.

`관점별 조회 결과`는 서버 원본 데이터를 사용자의 역할과 권한에 맞게 추려 만든 결과다. `상태 사본`은 특정 시점의 서버 또는 시뮬레이션 세계 상태를 한 번에 읽도록 묶은 데이터다. `데이터 계보`는 현재 값이 어떤 기록과 인계를 거쳐 형성됐는지 추적하는 연결이다.

## 8. 데이터 연결과 배치 검증 기록

데이터 연결은 사람이 이해하는 의미를 먼저 쓴다.

- 도심마트 건물 → 공개 상품 데이터와 연결
- 운영자 전용 진열대 → 마트 재고와 진열 작업 데이터(`MarketInventory`, `ShelfTask`)에 연결
- 농장 출하 Pallet → 수확 화물 데이터(`CanonicalProductHarvestCargo`)에 연결

배치 검증 기록은 특정 배치 객체를 어느 Scene·구역·배치 기준점에 놓았고 어떤 데이터와 연결했는지 검증하여 남긴 기록이다. 최소한 배치 객체, 대상 Scene, 구역, 배치 기준점, 데이터 연결과 검증 결과를 포함한다.

## 9. 코드 식별자 원칙

새 프로젝트 도메인 코드는 자연스러우면 `도심마트상태조회UseCase`, `조회Async`처럼 한국어 업무명과 영어 기술 역할을 조합할 수 있다. `MonoBehaviour`, `GameObject`, `Transform`, `Animator`, `UnityWebRequest`, `CancellationToken`, `Task`, `IReadOnlyList` 같은 기술어는 영어를 유지한다.

기존 코드·테스트 이름은 한국어화를 이유로 일괄 변경하지 않는다. 직렬화, API 계약, 저장 데이터, Scene·Prefab 참조가 깨지지 않는 좁은 변경에서만 별도로 검토한다.

## 10. 완료 보고 형식

가능하면 다음 순서를 사용한다.

1. 완료한 작업을 평이한 한국어 한 문장으로 요약
2. 완료 내용
3. 현재 구현 상태
4. 무엇을 검증했는지 설명한 검증 결과
5. 실운영 API·배포·commit·push 수행 여부
6. 다음 우선순위

`Manifest updated`, `Object promoted`, `Binding verified`처럼 영어 문장으로 보고하지 않는다. 각각 `구성 대장을 갱신했다`, `배치 객체를 다음 단계로 승격했다`, `데이터 연결을 검증했다`라고 쓴다.

테스트 숫자만 적지 말고 무엇을 검증했는지 함께 쓴다. Simulation과 실운영을 구분하고, Unity 화면 변화가 서버 업무 완료를 뜻한다고 표현하지 않는다.

## 11. 문서 수정 경계

[Unity 통합 모판 대응 모듈 구현 현황](../Architecture/UnityIntegratedSeedbedModuleStatus.md)과 [Unity 통합 모판·전시관 제안](../Architecture/UnityIntegratedSeedbedExhibitionProposal.md)의 설계 의미와 단계 구조는 유지한다. 용어 정리는 구조 재설계가 아니라 사람이 읽는 표현의 한국어화와 가독성 개선이다.

고유 식별자 값, API 경로, 클래스명, 테스트 이름, 파일명, JSON 필드와 저장 데이터는 근거 없이 변경하지 않는다.

## 12. 기본 출력 점검표

- 설명과 완료 보고를 한국어로 작성했는가
- 프로젝트 개념을 한국어로 먼저 설명했는가
- Unity·C#·표준 고유 기술명은 보존했는가
- 처음 한 번 필요한 경우에만 영어를 병기했는가
- 테스트 숫자와 검증 대상을 함께 설명했는가
- 서버 사실, 관점별 조회 결과, Unity 표현을 구분했는가
- 시뮬레이션과 실운영을 구분했는가
- Unity 애니메이션이나 화면을 서버 업무 완료 근거로 표현하지 않았는가
- 현재 구현 범위와 남은 범위를 평이하게 요약했는가

