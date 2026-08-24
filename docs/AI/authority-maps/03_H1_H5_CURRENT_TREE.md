# H1~H5 현재 재고와 선택 트리

> 기준일: 2026-08-22
> 이 문서는 H의 서로 다른 세 대장을 섞지 않고, 현재 첫 기준 플레이에 실제로 선택된 Farm·Nature 트리를 보여준다.

## 먼저 구분할 세 수량

| 관점 | 현재 수량 | 의미 | 권위 원천 |
| --- | ---: | --- | --- |
| 설계 지식 재고 | H1 84 / H2 37 / H3 20 / H4 6 | 팩·게임 기획에서 도출한 위치 독립 후보 전체 | `catalog.v3.json` `simulation-world-spatial-design-knowledge.r3` |
| 실행용 공간 자원 지도 | H1 5 / H2 0 / H3 5 / H4 1 | 초기 WI 공간 모판·평창 AreaSet 실행 리소스 지도 | `spatial-hierarchy-levels.json` |
| Nature·Farm 기준 플레이 추적 부분집합 | H1 17 / H2 12 / H3 8 / H4 2 | 현재 기준 플레이가 직접·지원으로 사용하는 설계 지식 | `gameplay-spatial-completion.v1.json` |

84/37/20/6은 “현재 Scene에 배치된 수량”이 아니고, 5/0/5/1은 전체 설계 재고가 아니다.

## 상태 코드

| 코드 | 의미 |
| --- | --- |
| `ApprovedReference` | 기존 기준 표현 또는 조립 참조로 승인된 설계 지식 |
| `CandidateForReview` | 다음 사람 검토 후보 |
| `ExploratoryInventory` | 조립·탐색 가능한 설계 재고이며 E5 세계 발현을 뜻하지 않음 |
| `IdeaInventory` | 플레이·표현 맥락은 있으나 강한 계약이나 검토가 더 필요한 아이디어 재고 |
| `ActualE5Bound` | 호환 대장의 공간 조립 상태 코드. 특정 AreaSet·Graph 결속을 뜻하지만 이것만으로 E5 세계 발현을 주장하지 않음 |

## Farm 선택 트리

```text
H4 농업 생산·후처리권 [ExploratoryInventory]
├─ H3 고지대 농장 경관 [ExploratoryInventory]
│  ├─ H2 고지대 생산 블록 [ExploratoryInventory]
│  │  ├─ H1 농업 생산구획 [ApprovedReference]
│  │  └─ H1 숲 경계형 농장 전환 공간 [ExploratoryInventory]
│  ├─ H2 농장 작업·출하 블록 [ExploratoryInventory]
│  │  ├─ H1 수확·집하 작업마당 [ApprovedReference]
│  │  ├─ H1 농장 시설 정비 공간 [ExploratoryInventory]
│  │  └─ H1 농장 상차·출입 공간 [ApprovedReference]
│  └─ H2 숲 경계 농장 블록 [ExploratoryInventory]
│     ├─ H1 숲 경계형 농장 전환 공간 [ExploratoryInventory]
│     ├─ H1 자연 탐색·완충 공간 [ExploratoryInventory]
│     └─ H1 농업 생산구획 [ApprovedReference]
├─ H3 농가·생산·후처리 생활 경관 [ApprovedReference]
│  ├─ H2 농가·작업지원 생활 블록 [ApprovedReference]
│  │  ├─ H1 농가 귀환·작업자 대기 [CandidateForReview]
│  │  ├─ H1 농기구 보관 [CandidateForReview]
│  │  └─ H1 농장 시설 정비 [ExploratoryInventory]
│  ├─ H2 종자·농기구 준비 블록 [ExploratoryInventory]
│  │  ├─ H1 농기구 보관 [CandidateForReview]
│  │  └─ H1 종자 준비 [ExploratoryInventory]
│  ├─ H2 세척·선별·포장 블록 [IdeaInventory]
│  │  ├─ H1 수확물 임시 적치 [ExploratoryInventory]
│  │  ├─ H1 농산물 세척 [IdeaInventory]
│  │  ├─ H1 농산물 선별 [IdeaInventory]
│  │  └─ H1 수확·집하 작업마당 [ApprovedReference]
│  └─ H2 농장 작업·출하 블록 [ExploratoryInventory]
├─ H3 농장 사건 격리·회복 경관 [ExploratoryInventory]
│  ├─ H2 사건 점검·격리 블록 [ExploratoryInventory]
│  │  ├─ H1 수확물 노출 점검 [ExploratoryInventory]
│  │  ├─ H1 사고 수확물 격리 [IdeaInventory]
│  │  └─ H1 기상 보호 적치 [ExploratoryInventory]
│  └─ H2 손실 회복·복원 인계 블록 [ExploratoryInventory]
│     ├─ H1 사고 수확물 격리 [IdeaInventory]
│     ├─ H1 손실 복구·재작업 [IdeaInventory]
│     └─ H1 자연권 복구 자재 인계 [ExploratoryInventory]
└─ H3 Farm 계절 생산·출하 순환 경관 [ExploratoryInventory]
   ├─ H2 관수·급수 관리 블록 [ExploratoryInventory]
   ├─ H2 집중 수확·집하 블록 [ExploratoryInventory]
   └─ H2 농장 작업·출하 블록 [ExploratoryInventory]
```

Farm H4 청사진 자체는 `ExploratoryInventory`지만, 호환 공간 대장에서 `area-set:sim:pyeongchang:farm-production.v1`이 H3 네 개와 함께 `Available / ActualE5Bound`로 결속돼 있다. 이는 E4·E5 판정에 넣을 수 있는 조건부 공간 증거이며, WI 발생원·권위 전이·Task/Effect·결과·후속 선택을 대신하지 않는다.

## Nature 선택 트리

```text
H4 Nature 생활·탐험권 [ExploratoryInventory]
├─ H3 자연 생활·위협·회복 경관 [CandidateForReview]
│  ├─ H2 자연 위협 추적·대피 블록 [CandidateForReview]
│  │  ├─ H1 위협 관찰 초소 [CandidateForReview]
│  │  ├─ H1 사건 흔적 조사 구역 [CandidateForReview]
│  │  └─ H1 긴급 후퇴 길목 [CandidateForReview]
│  └─ H2 자연 복원·안전 회복 블록 [CandidateForReview]
│     ├─ H1 정화·복구 작업 공간 [CandidateForReview]
│     └─ H1 안전 회복 야영지 [CandidateForReview]
├─ H3 자연 탐색길·대피망 경관 [ExploratoryInventory]
│  ├─ H2 자연 탐색·대피 블록 [ExploratoryInventory]
│  │  ├─ H1 자연 탐색 출발지 [ExploratoryInventory]
│  │  ├─ H1 전망·관찰 공간 [IdeaInventory]
│  │  └─ H1 임시 대피 공간 [IdeaInventory]
│  └─ H2 산림·수변 완충 블록 [ExploratoryInventory]
│     ├─ H1 자연 탐색·완충 공간 [ExploratoryInventory]
│     └─ H1 숲 경계형 농장 전환 공간 [ExploratoryInventory]
└─ H3 Nature 생활핵·조우·방어 폐루프 [ExploratoryInventory]
   ├─ H2 자연 안전 생활핵 블록 [ExploratoryInventory]
   ├─ H2 자연 몬스터 조우·이탈 블록 [ExploratoryInventory]
   └─ H2 자연 야간 방어 블록 [ExploratoryInventory]
```

Nature도 `area-set:sim:pyeongchang:nature-home.v1`과 H3 세 개가 호환 공간 대장에서 `Available / ActualE5Bound`로 결속돼 있다. WI-NATURE-01~04는 E4 실행 문맥 부분 결속·E5 세계 발현 부분 상태다. WI-NATURE-05~11은 실제 H1·H2·H3·LandscapeGraph 직접 결속, WI-NATURE-12는 진행 작업 공간 문맥의 Contextual 결속으로 E4 `ContextBound`·E5 `ManifestationPartial`이다. 이 공간 근거와 PlayMode 한 건만으로 전체 Nature E7이나 현실 지형·Hosted 완료를 주장하지 않는다.

## 현재 공간 조립 호환 대장과 H5

| AreaSet | 역할 | 공간 조립 상태 | 포함 Graph 수 | 현재 사용 판단 |
| --- | --- | --- | ---: | --- |
| `nature-home.v1` | NatureHome | Available | 3 | 첫 기준 플레이 엄격 대상 |
| `farm-production.v1` | Farm | Available | 4 | 첫 기준 플레이 엄격 대상 |
| `logistics-hub.v1` | Hub 호환 ID `CityHub` | Available | 4 | 독립 Hub 내부 플레이는 아직 미완료 |
| `town-market.v1` | Town | Available | 5 | 경고형 후속 대상 |

H5 `world-layout:sim:pyeongchang:nature-farm-hub-town.v1`은 네 AreaSet과 세 물리 회랑을 `ScenarioLocalMeters`에 배치한다. `Nature→Farm`, `Farm→Hub`, `Hub→Town` 회랑이 존재하지만 이는 작성 Scenario의 조건부 공간 조립 증거다. 개발 우선순위를 자동으로 Farm→Hub로 만들지 않으며 E5 WI 세계 발현, 현실 지형·E7 플레이도 증명하지 않는다.

## Synty 표현 재료 역색인

Nature·Farm·Town·City·Construction 팩의 2,346개 전수 재고와 H1~H3 관련성은 [Synty 5팩 자산과 H1~H3 연결 지도](../../Architecture/Synty5팩자산-H1-H3연결지도.md)를 따른다. Synty 자산은 H의 표현 후보이며 이 문서의 선택 상태나 WI의 E4·E5 상태를 자동으로 바꾸지 않는다. Construction은 독립 H가 아니라 공사·격리·복구 공통 상태층이다.

## 권위 원천

- [`catalog.v3.json`](../../../eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json)
- [`actual-e5-spatial.v1.json`](../../../eng/world-seedbeds/generated/actual-e5-spatial.v1.json)
- [`h5-world-layout.v1.json`](../../../eng/world-seedbeds/generated/h5-world-layout.v1.json)
- [게임플레이–공간 완성도](../generated/gameplay-spatial-completion.md)
