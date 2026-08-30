# Nature 지식 습득 E5 공간·배치 연구

## 식별과 상태

- 연구 ID: `study:nature-player-knowledge:spatial-placement.r1`
- 연구 revision: `nature-player-knowledge.spatial-placement-study.r1`
- 대상: `playable-loop:nature-basic-herbal-recovery.v1` / `WI-ACTOR-03`
- 상태: `Accepted` (공간 의도·측정/선택 절차 승인, 실제 배치·시각 승인 아님)
- 승인 근거·검토: 2026-08-30 기획 스레드의 [E5 계획](../기존WI세계발현E5개발계획.md) 사용자 구현 승인과 코드·E4 준비 계약·보유 파일 대조.

## 질문·재고·대안

첫 지식 획득을 UI 목록만으로 끝내지 않고, 폐야영지의 기록을 발견하고 읽은 뒤 내 지식으로 남기는 공간 경험에 연결한다. 이미 구현된 `Simulation플레이어지식Service`, `ISimulationPlayerKnowledgeRuntime`, Recipe 카드/E4 준비 투영과 현재 Nature 이동·조작을 재사용한다.

UI 전용은 공간 발현을 확인할 수 없고, 새 독서 애니메이션은 첫 E5에 불필요한 Rig/손 접촉 의존성을 만든다. 따라서 **실제 열린 책 + 읽기 기준점 + 기존 카드**를 선택한다. 책은 줍거나 제거하지 않고 이미 앎 상태를 표시한다.

## H·자산·측정 기준선

- H1은 폐야영지의 `ReadableKnowledgeSource`다. 기존 야영지 기능 공간 안에서 기존 탁자/상자 등 승인 지지면 위에 배치한다. Farm/Town 방문이나 새 건설이 선행 조건이 아니다.
- H2는 기존 야영지의 접근로와 휴식 기능 공간 관계만 재사용한다. H3 또는 패턴 재고를 새로 승인하지 않는다.
- VisualKey: `Knowledge.Recipe.Record.OpenBook`, 대체 종이 `Knowledge.Recipe.Record.LoosePaper`.
- 주 자산: `Assets/Synty/PolygonTown/Prefabs/Props/SM_Prop_BookOpen_01.prefab`. 2026-08-30 직접 파일 SHA256은 `AF10CBABDE92389DAC19DF6CA560864E936277DEF6B70494937DB8E74CBCC3C3`다. 이전 E4 문자열 fingerprint `708b9837...`와 값이 다르므로 동일 근거로 간주하지 않고 단위·meta/의존성을 확인해 새 후보 기준선으로 봉인한다. 과거 증거 hash를 덮어쓰지 않는다.
- City/Generic 종이·Construction 클립보드는 기존 E4 대체 재고를 보존하지만 실제 기능 분류·크기 확인 전 주 자산을 자동 대체하지 않는다. 주 자산 부적합이면 해당 배치만 차단한다.
- primitive 표식+카드는 오류 안내용 fallback이며 이 연구의 실제 책 E5 완료를 대체하지 않는다.

고정된 임의 월드 좌표나 가상의 실측값을 쓰지 않는다. 아래 **기존 설정·실측에 의한 결정 절차**를 구현 기준으로 고정한다.

1. canonical Scene/현재 야영지 조립 결과에서 지지 Collider와 플레이어 접근 방향을 찾는다. 기존 기준점과 조립 고유 식별자를 기록한다.
2. 실제 활성 Renderer의 로컬 Bounds와 지지면을 측정하고, 기존 표면 정렬 Utility의 여유값을 사용해 책의 하단을 지지면 위에 맞춘다. 임의 확대 없이 원본 축척을 우선하며 실제 가독성 실패는 연구 피드백으로 남긴다.
3. 지지면의 유효 중앙 Slot부터 StableId 순으로 검사하고 책 Bounds 전체가 지지 영역 안에 놓이는 첫 후보를 선택한다. 후보가 없으면 빈 Slot/차단으로 남긴다. 책 앞면은 기존 접근 방향을 향한다.
4. 선택 Collider는 측정 Bounds를 감싸고 주 통행선·Player Collider와 겹치지 않게 한다. 상호작용 거리는 기존 Nature 입력/권위 접근 계약의 값을 그대로 사용한다. 실제 값·설정 경로는 배치 검증 기록에 봉인한다.
5. 현재 1인칭/3인칭 조작에서 출처 선택→Preview→Confirm을 사용할 수 있어야 한다. 카메라를 강제로 바꾸거나 이동을 잠그지 않는다. 손 접촉/페이지 넘기기 동작은 요구하지 않는다.

## 논리·저장·표현 영향

지식 원장은 Session·Actor에 귀속한다. Confirm 결과, ActionRecord, CommandLog, 카드 SourceRevision을 같은 권위 변경에 결속한다. 접근이 사라졌거나 revision이 달라지면 Confirm은 무변경 거부하고 재조회한다. 이미 학습한 Recipe는 중복 추가되지 않는다.

기존 Save/Replay는 지식 상태와 명령 멱등 정보, 이 책이 가리키는 안정된 출처/처방 관계를 보존한다. 물리 Prefab 경로를 Simulation 규칙에 넣지 않는다. Restore는 같은 지식을 읽어 카드를 재생성하며 책은 같은 조립 고유 식별자로 한 번만 나타난다. 다른 약초 WI는 활성화하지 않는다.

## 검증·충돌·무효화

- 자동검사: Bounds·지지·통로·Collider·Anchor 결속, 동일 후보 입력의 결정성, 같은 revision 소비, 권위 무변경인 표현, 취소/재조회·중복 Confirm·저장 재진입.
- 실제 Scene: 책 식별·접근·선택·학습 전후 카드, 저장 후 재방문, Console와 대표 Game View. 아직 실행/시각 검증을 한 것이 아니다.
- 건물 연구는 새 외피가 없어 NotRequired, 애니메이션은 실제 책+기존 카드만 사용하므로 NotRequired다. 공간/배치만 이 연구에 Required로 결속한다.
- 출처 계약, 지지면/동선, 후보 파일·의존성 fingerprint, 카메라·접근 규칙이 바뀌면 해당 E4 기준선을 다시 연다. 실제 책이 접근·식별 불가능하면 E5 통과시키지 않는다.
