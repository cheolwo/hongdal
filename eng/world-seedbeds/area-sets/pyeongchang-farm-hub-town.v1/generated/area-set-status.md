<!-- 이 문서는 AreaSet 상태 renderer가 다시 생성하는 산출물입니다. 직접 수정하지 마십시오. -->

# 평창 Farm–Hub–Town 지역 세계 상태

- 고유 식별자: `area-set:sim:pyeongchang:farm-hub-town.v1`
- 정의 개정: `1`
- 정의 SHA-256: `851141dc672c00eaeaa095bf65601b1be682dc44ba4a5a2f4da5addb72acf637`
- 사람 문서 SHA-256: `2b24a300d198168daabf3b8734375c635ea6f8bcbd8a263355256951bcf10221`
- Area / ScenarioRoute / Graph / 관계: `3 / 2 / 5 / 4`

```text
AreaSet
├─ 대관령면 Farm [PartialUnresolved] Tile=4 Placement=5 Unresolved=3
├─ Farm–Hub 회랑 [Declared] Tile=0 Placement=0 Unresolved=1
├─ 진부면 Hub [Declared] Tile=0 Placement=0 Unresolved=1
├─ Hub–Town 회랑 [Declared] Tile=0 Placement=0 Unresolved=1
└─ 평창읍 Town [Declared] Tile=0 Placement=0 Unresolved=1
```

## Graph 실행 상태

| 경관 Graph | 상태 | Tile | Node | Edge | 배치 | 외부 연결 | 미해결 | Graph SHA-256 |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| 대관령면 Farm<br>`landscape-graph:sim:pyeongchang:daegwallyeong-farm.v1` | `PartialUnresolved` | 4 | 5 | 3 | 5 | 0 | 3 | `9c3b09c7fc59bd98a4a0102f6e08a1538e050aa2fbc3d2d0fc0becdb459ecc84` |
| Farm–Hub 회랑<br>`landscape-graph:sim:pyeongchang:farm-hub-corridor.v1` | `Declared` | 0 | 0 | 0 | 0 | 0 | 1 | `18aaaa20a800e8c163df149ae6b73bc6a814dabdc634a38146327892f3345c31` |
| 진부면 Hub<br>`landscape-graph:sim:pyeongchang:jinbu-hub.v1` | `Declared` | 0 | 0 | 0 | 0 | 0 | 1 | `b5989abd453e6f200d95f5972e66ddb49bc592ae7b62af015668a5ec74706106` |
| Hub–Town 회랑<br>`landscape-graph:sim:pyeongchang:hub-town-corridor.v1` | `Declared` | 0 | 0 | 0 | 0 | 0 | 1 | `60051420c05ce8eda416b08faef56bf4eff702cf5fc041894e8328b0300d0ed7` |
| 평창읍 Town<br>`landscape-graph:sim:pyeongchang:pyeongchang-town.v1` | `Declared` | 0 | 0 | 0 | 0 | 0 | 1 | `b15d7bf640ffcdfbc974ba2c737c7ff88bcd82601add8c6a214978b784e4740e` |

`Declared`와 `PartialUnresolved`는 자료 부족을 꾸며내지 않고 남긴 상태다. Unity의 플레이어별 `Prepared / Active / Cached`는 이 서버 빌드 상태와 별도로 관리한다.
