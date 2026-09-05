> 참고 색인: 괘·효 순서는 자료의 위치이며 이야기·제작·실행 순서가 아니다. 현행 기준은 [스토리 영감과 플레이 진행 분리](../../Architecture/스토리영감과플레이진행분리.md)다. 아래 상태·순차 관문은 이전 기획 이력으로 보존한다.

# 역경 64괘 기반 게임 스토리 기획 색인

> 이 문서는 `hexagram-story-production.json`에서 자동 생성된다. 직접 수정하지 않는다.

- 공부·저작 순서: `MainCampaignKingWenHexagramAndBottomToTopLineOrder`
- 문답 방식: `BroadHexagramStoryThenOneLineAdaptationAtATime` / 기존 기획 참조: `TechnicalAppendixReferenceOnly`
- 문답 순서: 괘의 의미와 큰 이야기 제안 → 사용자와 줄기 합의 → 효사 원문·의미 대조 → 각색 차이를 기록한 사건 문답 → 주체·WI·H 요구사항.
- [64괘 플레이 스토리 큰 줄기 제안](../Planning/스토리/PLAN-STORY-HEXAGRAM-SEQUENCE-001/괘의미별-플레이스토리-큰줄기.md)은 Proposed이며 기존 승인 효·제작 커서·Runtime·Evidence를 자동 변경하지 않는다.
- 정식 제작 커서: `HEX-01-QIAN`
- 선행 표본 문답 커서: `HEX-04-MENG-L3` / 다음 표본 효: `HEX-04-MENG-L4`
- Runtime 캠페인 상태: `NotEstablished`
- 실제 플레이 순서: `ContinuousHexagramChapterWithInterHexagramFreeStay`
- 기반층: `HEX-01-QIAN, HEX-02-KUN` / 구체 서사 시작: `HEX-03-ZHUN`
- 시각 동반 시작: `HEX-03-ZHUN` / 형식: `HexagramUpperLowerTrigramLineDirectionMeaningQuestion`
- 괘: `64` / 효 이야기 슬롯: `384` / 효사 기획 ID: `384` / StoryApproved 괘: `1` / ActiveStoryDialogue 효: `1`

제1괘 건과 제2괘 곤은 여섯 개의 짧은 실제 플레이 비트로 구성하는 서막 캠페인이고, 제3괘 수뢰둔부터 본격 캠페인이 시작된다. 정식 제작 커서는 중천건이며 산수몽 육삼은 순서를 건너뛰지 않는 선행 표본 문답이다.
제3괘부터 기획 문답에는 육효 괘상, 상괘·하괘, 효를 아래에서 위로 읽는 방향, 의미 요약과 질문 하나를 함께 보여 준다. 이 시각 자료는 기획 보조이며 Evidence를 승격하지 않는다.

## 전체 순서

| 순번 | 괘 | 안정 ID | 상괘 / 하괘 | 공부 상태 | 이야기 상태 | 효 상태 | 다음 괘 |
| ---: | --- | --- | --- | --- | --- | --- | --- |
| 1 | ䷀ 乾 · 중천건 | `HEX-01-QIAN` | ☰ 건 / ☰ 건 | `Active` | `StorySeeded` | StorySeeded 6 | `HEX-02-KUN` |
| 2 | ䷁ 坤 · 중지곤 | `HEX-02-KUN` | ☷ 곤 / ☷ 곤 | `Locked` | `StorySeeded` | StorySeeded 6 | `HEX-03-ZHUN` |
| 3 | ䷂ 屯 · 수뢰둔 | `HEX-03-ZHUN` | ☵ 감 / ☳ 진 | `Locked` | `StoryApproved` | StoryApproved 6 | `HEX-04-MENG` |
| 4 | ䷃ 蒙 · 산수몽 | `HEX-04-MENG` | ☶ 간 / ☵ 감 | `Locked` | `ActiveStoryDialogue` | ActiveStoryDialogue 1, StoryApproved 2, StorySeeded 3 | `HEX-05-XU` |
| 5 | ䷄ 需 · 수천수 | `HEX-05-XU` | ☵ 감 / ☰ 건 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-06-SONG` |
| 6 | ䷅ 訟 · 천수송 | `HEX-06-SONG` | ☰ 건 / ☵ 감 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-07-SHI` |
| 7 | ䷆ 師 · 지수사 | `HEX-07-SHI` | ☷ 곤 / ☵ 감 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-08-BI` |
| 8 | ䷇ 比 · 수지비 | `HEX-08-BI` | ☵ 감 / ☷ 곤 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-09-XIAO-CHU` |
| 9 | ䷈ 小畜 · 풍천소축 | `HEX-09-XIAO-CHU` | ☴ 손 / ☰ 건 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-10-LU` |
| 10 | ䷉ 履 · 천택리 | `HEX-10-LU` | ☰ 건 / ☱ 태 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-11-TAI` |
| 11 | ䷊ 泰 · 지천태 | `HEX-11-TAI` | ☷ 곤 / ☰ 건 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-12-PI` |
| 12 | ䷋ 否 · 천지비 | `HEX-12-PI` | ☰ 건 / ☷ 곤 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-13-TONG-REN` |
| 13 | ䷌ 同人 · 천화동인 | `HEX-13-TONG-REN` | ☰ 건 / ☲ 리 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-14-DA-YOU` |
| 14 | ䷍ 大有 · 화천대유 | `HEX-14-DA-YOU` | ☲ 리 / ☰ 건 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-15-QIAN-MODESTY` |
| 15 | ䷎ 謙 · 지산겸 | `HEX-15-QIAN-MODESTY` | ☷ 곤 / ☶ 간 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-16-YU` |
| 16 | ䷏ 豫 · 뇌지예 | `HEX-16-YU` | ☳ 진 / ☷ 곤 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-17-SUI` |
| 17 | ䷐ 隨 · 택뢰수 | `HEX-17-SUI` | ☱ 태 / ☳ 진 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-18-GU` |
| 18 | ䷑ 蠱 · 산풍고 | `HEX-18-GU` | ☶ 간 / ☴ 손 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-19-LIN` |
| 19 | ䷒ 臨 · 지택림 | `HEX-19-LIN` | ☷ 곤 / ☱ 태 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-20-GUAN` |
| 20 | ䷓ 觀 · 풍지관 | `HEX-20-GUAN` | ☴ 손 / ☷ 곤 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-21-SHI-HE` |
| 21 | ䷔ 噬嗑 · 화뢰서합 | `HEX-21-SHI-HE` | ☲ 리 / ☳ 진 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-22-BI-GRACE` |
| 22 | ䷕ 賁 · 산화비 | `HEX-22-BI-GRACE` | ☶ 간 / ☲ 리 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-23-BO` |
| 23 | ䷖ 剝 · 산지박 | `HEX-23-BO` | ☶ 간 / ☷ 곤 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-24-FU` |
| 24 | ䷗ 復 · 지뢰복 | `HEX-24-FU` | ☷ 곤 / ☳ 진 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-25-WU-WANG` |
| 25 | ䷘ 無妄 · 천뢰무망 | `HEX-25-WU-WANG` | ☰ 건 / ☳ 진 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-26-DA-CHU` |
| 26 | ䷙ 大畜 · 산천대축 | `HEX-26-DA-CHU` | ☶ 간 / ☰ 건 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-27-YI` |
| 27 | ䷚ 頤 · 산뢰이 | `HEX-27-YI` | ☶ 간 / ☳ 진 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-28-DA-GUO` |
| 28 | ䷛ 大過 · 택풍대과 | `HEX-28-DA-GUO` | ☱ 태 / ☴ 손 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-29-KAN` |
| 29 | ䷜ 坎 · 중수감 | `HEX-29-KAN` | ☵ 감 / ☵ 감 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-30-LI` |
| 30 | ䷝ 離 · 중화리 | `HEX-30-LI` | ☲ 리 / ☲ 리 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-31-XIAN` |
| 31 | ䷞ 咸 · 택산함 | `HEX-31-XIAN` | ☱ 태 / ☶ 간 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-32-HENG` |
| 32 | ䷟ 恆 · 뇌풍항 | `HEX-32-HENG` | ☳ 진 / ☴ 손 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-33-DUN` |
| 33 | ䷠ 遯 · 천산둔 | `HEX-33-DUN` | ☰ 건 / ☶ 간 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-34-DA-ZHUANG` |
| 34 | ䷡ 大壯 · 뇌천대장 | `HEX-34-DA-ZHUANG` | ☳ 진 / ☰ 건 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-35-JIN` |
| 35 | ䷢ 晉 · 화지진 | `HEX-35-JIN` | ☲ 리 / ☷ 곤 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-36-MING-YI` |
| 36 | ䷣ 明夷 · 지화명이 | `HEX-36-MING-YI` | ☷ 곤 / ☲ 리 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-37-JIA-REN` |
| 37 | ䷤ 家人 · 풍화가인 | `HEX-37-JIA-REN` | ☴ 손 / ☲ 리 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-38-KUI` |
| 38 | ䷥ 睽 · 화택규 | `HEX-38-KUI` | ☲ 리 / ☱ 태 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-39-JIAN` |
| 39 | ䷦ 蹇 · 수산건 | `HEX-39-JIAN` | ☵ 감 / ☶ 간 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-40-XIE` |
| 40 | ䷧ 解 · 뇌수해 | `HEX-40-XIE` | ☳ 진 / ☵ 감 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-41-SUN` |
| 41 | ䷨ 損 · 산택손 | `HEX-41-SUN` | ☶ 간 / ☱ 태 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-42-YI-BENEFIT` |
| 42 | ䷩ 益 · 풍뢰익 | `HEX-42-YI-BENEFIT` | ☴ 손 / ☳ 진 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-43-GUAI` |
| 43 | ䷪ 夬 · 택천쾌 | `HEX-43-GUAI` | ☱ 태 / ☰ 건 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-44-GOU` |
| 44 | ䷫ 姤 · 천풍구 | `HEX-44-GOU` | ☰ 건 / ☴ 손 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-45-CUI` |
| 45 | ䷬ 萃 · 택지췌 | `HEX-45-CUI` | ☱ 태 / ☷ 곤 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-46-SHENG` |
| 46 | ䷭ 升 · 지풍승 | `HEX-46-SHENG` | ☷ 곤 / ☴ 손 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-47-KUN-DISTRESS` |
| 47 | ䷮ 困 · 택수곤 | `HEX-47-KUN-DISTRESS` | ☱ 태 / ☵ 감 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-48-JING` |
| 48 | ䷯ 井 · 수풍정 | `HEX-48-JING` | ☵ 감 / ☴ 손 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-49-GE` |
| 49 | ䷰ 革 · 택화혁 | `HEX-49-GE` | ☱ 태 / ☲ 리 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-50-DING` |
| 50 | ䷱ 鼎 · 화풍정 | `HEX-50-DING` | ☲ 리 / ☴ 손 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-51-ZHEN` |
| 51 | ䷲ 震 · 중뢰진 | `HEX-51-ZHEN` | ☳ 진 / ☳ 진 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-52-GEN` |
| 52 | ䷳ 艮 · 중산간 | `HEX-52-GEN` | ☶ 간 / ☶ 간 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-53-JIAN-GRADUAL` |
| 53 | ䷴ 漸 · 풍산점 | `HEX-53-JIAN-GRADUAL` | ☴ 손 / ☶ 간 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-54-GUI-MEI` |
| 54 | ䷵ 歸妹 · 뇌택귀매 | `HEX-54-GUI-MEI` | ☳ 진 / ☱ 태 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-55-FENG` |
| 55 | ䷶ 豐 · 뇌화풍 | `HEX-55-FENG` | ☳ 진 / ☲ 리 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-56-LU-TRAVELER` |
| 56 | ䷷ 旅 · 화산려 | `HEX-56-LU-TRAVELER` | ☲ 리 / ☶ 간 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-57-XUN` |
| 57 | ䷸ 巽 · 중풍손 | `HEX-57-XUN` | ☴ 손 / ☴ 손 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-58-DUI` |
| 58 | ䷹ 兌 · 중택태 | `HEX-58-DUI` | ☱ 태 / ☱ 태 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-59-HUAN` |
| 59 | ䷺ 渙 · 풍수환 | `HEX-59-HUAN` | ☴ 손 / ☵ 감 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-60-JIE` |
| 60 | ䷻ 節 · 수택절 | `HEX-60-JIE` | ☵ 감 / ☱ 태 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-61-ZHONG-FU` |
| 61 | ䷼ 中孚 · 풍택중부 | `HEX-61-ZHONG-FU` | ☴ 손 / ☱ 태 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-62-XIAO-GUO` |
| 62 | ䷽ 小過 · 뇌산소과 | `HEX-62-XIAO-GUO` | ☳ 진 / ☶ 간 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-63-JI-JI` |
| 63 | ䷾ 既濟 · 수화기제 | `HEX-63-JI-JI` | ☵ 감 / ☲ 리 | `Locked` | `StoryUnopened` | Unmapped 6 | `HEX-64-WEI-JI` |
| 64 | ䷿ 未濟 · 화수미제 | `HEX-64-WEI-JI` | ☲ 리 / ☵ 감 | `Locked` | `StoryUnopened` | Unmapped 6 | 없음 |

## 정식 공부 커서

### ䷀ 乾 · 중천건 ($([0].stableId))

- 중심 주체: `Adventurer`
- 이야기 규모: `ShortPlayablePrologue`
- 하위 사건 정책: `ReferenceLowerLevelPlansWithoutAbsorbingAllEvents`

| 효 | 안정 ID | 효사 기획 ID | 상태 | 고전 의미 요약 | 이야기 질문 |
| --- | --- | --- | --- | --- | --- |
| 初九 · 1/6 | `HEX-01-QIAN-L1` | `PLAN-STORY-HEX01-LINE-001` | `StorySeeded` | 때가 이르기 전에는 잠재된 힘을 함부로 쓰지 않는다. | 낯선 몸과 환경을 먼저 살피고 성급한 행동을 멈출 수 있는가? |
| 九二 · 2/6 | `HEX-01-QIAN-L2` | `PLAN-STORY-HEX01-LINE-002` | `StorySeeded` | 잠재된 힘이 들판에 나타나 관계와 방향을 찾기 시작한다. | 안전한 범위에서 첫 행동을 드러내고 타인의 흔적을 알아볼 수 있는가? |
| 九三 · 3/6 | `HEX-01-QIAN-L3` | `PLAN-STORY-HEX01-LINE-003` | `StorySeeded` | 쉼 없이 힘쓰되 위험을 경계해야 허물이 없다. | 행동을 이어가면서도 시간과 피로, 밤의 위험을 함께 살필 수 있는가? |
| 九四 · 4/6 | `HEX-01-QIAN-L4` | `PLAN-STORY-HEX01-LINE-004` | `StorySeeded` | 도약할지 머물지 시험하는 경계에서는 신중한 선택에 허물이 없다. | 위험한 지형 앞에서 나아감과 물러남을 상황에 맞게 선택할 수 있는가? |
| 九五 · 5/6 | `HEX-01-QIAN-L5` | `PLAN-STORY-HEX01-LINE-005` | `StorySeeded` | 힘이 제자리를 얻으면 넓은 시야와 좋은 만남이 열린다. | 익힌 행동을 이용해 시야를 넓히고 사람이 사는 방향을 찾을 수 있는가? |
| 上九 · 6/6 | `HEX-01-QIAN-L6` | `PLAN-STORY-HEX01-LINE-006` | `StorySeeded` | 힘이 지나치면 후회가 생기므로 스스로 한계를 알아야 한다. | 가능성을 과신한 행동의 대가를 겪고 속도를 조절할 수 있는가? |

## 승인된 첫 구체 프로토타입

제3괘 수뢰둔의 여섯 이야기는 StoryApproved다. 각 효는 다른 효의 개발 완료를 기다리지 않고 기술 별책으로 인계할 수 있으며, 승인만으로 WI·H·E를 승격하지 않는다.

| 효 | 효사 기획 ID | 프로토타입 이야기 | 연결 기획 |
| --- | --- | --- | --- |
| 初九 · 1/6 | `PLAN-STORY-HEX03-LINE-001` | 나무꾼 모험가가 부러진 농장 손도끼를 회수하고 개인 손도끼로 벌목·운반·울타리 수리를 완결한 뒤, 재회한 한스에게 사실을 확인받아 휴식·식사·소량 보관 체류권을 얻는다. | PLAN-STORY-FIRST-FARM-DISCOVERY-001, PLAN-STORY-HEX03-CAMPAIGN-001, PLAN-STORY-HEX03-LINE-001, PLAN-STORY-IDEA-MAP-LEARNING-001 |
| 六二 · 2/6 | `PLAN-STORY-HEX03-LINE-002` | 플레이어가 밭을 안쪽에 둔 농장 경계 순환로를 한스와 돌고, 이상 흔적을 독단 추격하지 않은 채 해지기 전에 함께 귀환해 약속 이행 기록을 남긴다. | PLAN-STORY-FIRST-FARM-DISCOVERY-001, PLAN-STORY-HEX03-CAMPAIGN-001, PLAN-STORY-HEX03-LINE-002, PLAN-STORY-IDEA-MAP-LEARNING-001 |
| 六三 · 3/6 | `PLAN-STORY-HEX03-LINE-003` | 숲닭의 깃털·발자국과 큰 무언가가 만든 눌린 식생을 별도 관찰 사실로 기록하고, 배후를 확정하지 않은 채 숲 경계표지에서 한스와 귀환한다. | PLAN-STORY-FIRST-FARM-DISCOVERY-001, PLAN-STORY-HEX03-CAMPAIGN-001, PLAN-STORY-HEX03-LINE-003, PLAN-STORY-IDEA-MAP-LEARNING-001 |
| 六四 · 4/6 | `PLAN-STORY-HEX03-LINE-004` | 한스가 대화로 생활주택과 지정 감자밭 H1 한 구역의 제한 관리권을 즉시 위임하고, 농장 전체 소유·철거·확장은 계속 금지한다. | PLAN-STORY-FIRST-FARM-DISCOVERY-001, PLAN-STORY-HEX03-CAMPAIGN-001, PLAN-STORY-HEX03-LINE-004, PLAN-STORY-IDEA-MAP-LEARNING-001 |
| 九五 · 5/6 | `PLAN-STORY-HEX03-LINE-005` | 플레이어가 방치된 2.5×2.5m 감자 구획을 관리 상태로 회복하고 한스와 생활주택을 완전히 공동 수리한다. 숲닭 가축화는 선택 활동으로 남는다. | PLAN-STORY-FIRST-FARM-DISCOVERY-001, PLAN-STORY-HEX03-CAMPAIGN-001, PLAN-STORY-HEX03-LINE-005, PLAN-STORY-IDEA-MAP-LEARNING-001 |
| 上六 · 6/6 | `PLAN-STORY-HEX03-LINE-006` | 플레이어와 한스가 숲 쪽 단일 병목으로 밀려온 야수 무리를 공동 방어한다. 생활주택·감자밭·울타리 중 실제 행동으로 지키지 못한 하나에 반드시 복구 가능한 부분 손실이 남는다. | PLAN-STORY-FIRST-FARM-DISCOVERY-001, PLAN-STORY-HEX03-CAMPAIGN-001, PLAN-STORY-HEX03-LINE-006, PLAN-STORY-IDEA-MAP-LEARNING-001 |

## HUD Presentation E4 준비 계약

- 기본 표시: `䷀ 乾 · 중천건 · 초구 · 1/6`
- 위치: `TopRight` / 상태: `Hidden, Current, Completed`
- 미래 항목: `DoNotExpose` / 괘상 fallback: `BundledSpriteForU4DC0ToU4DFFWhenFontUnsupported`
- 실제 Unity HUD와 Evidence 승격은 포함하지 않는다.
