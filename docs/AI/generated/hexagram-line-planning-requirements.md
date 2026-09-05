# 효사별 기획·WI·H 요구사항 색인

> 이 문서는 `hexagram-line-planning-requirements.json`에서 자동 생성된다. 직접 수정하지 않는다.

- 등록 효사 기획 ID: `384`
- 요구사항을 연 효사: `6` / 현재 문답: `0` / Seeded: `0`
- 개발 인계 가능: `6`

효사 기획은 필요한 주체·WI·H를 선언하지만 자동 생성하지 않는다. 이야기 승인과 개발 준비를 분리하며, 개발 Goal 하나는 WI 하나만 소유한다.

| 효사 기획 | 효 | 기획 | 요구사항 | 인계 | 원문 적합 | Graph Map | 배치 맵 | 공백 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `PLAN-STORY-HEX03-LINE-001` | `HEX-03-ZHUN-L1` | `StoryApproved` | `Resolved` | `ReadyForDevelopment` | `Aligned` | `UpdateExisting` | `Required` |  |
| `PLAN-STORY-HEX03-LINE-002` | `HEX-03-ZHUN-L2` | `StoryApproved` | `Resolved` | `ReadyForDevelopment` | `Partial` | `UpdateExisting` | `Required` |  |
| `PLAN-STORY-HEX03-LINE-003` | `HEX-03-ZHUN-L3` | `StoryApproved` | `Resolved` | `ReadyForDevelopment` | `Aligned` | `UpdateExisting` | `Required` |  |
| `PLAN-STORY-HEX03-LINE-004` | `HEX-03-ZHUN-L4` | `StoryApproved` | `Resolved` | `ReadyForDevelopment` | `Partial` | `UpdateExisting` | `Required` |  |
| `PLAN-STORY-HEX03-LINE-005` | `HEX-03-ZHUN-L5` | `StoryApproved` | `Resolved` | `ReadyForDevelopment` | `Aligned` | `UpdateExisting` | `Required` |  |
| `PLAN-STORY-HEX03-LINE-006` | `HEX-03-ZHUN-L6` | `StoryApproved` | `Resolved` | `ReadyForDevelopment` | `Aligned` | `CreateSubgraph` | `Required` |  |

## 요구사항 요약

### `PLAN-STORY-HEX03-LINE-001`

- Story Beat: `story-beat:hex03-line001:establish-guest-place-through-fence-repair`
- 주체: `PlayerActor`=Required/ExistingReused, `HansNpc`=Required/ExistingReused, `HansGuestRightsGrant`=Required/ExistingReused, `FarmhouseGuestSpace`=Required/ExistingReused
- WI: `ConfirmFenceRepairAttributionAndGrantGuestRights`=Required/ExistingReused, `RecoverBrokenFarmAxePrerequisite`=Required/ExistingReused, `FirstWoodcuttingPrerequisite`=Required/ExistingReused, `CollectRepairTimberPrerequisite`=Required/ExistingReused, `RepairFarmFencePrerequisite`=Required/ExistingReused, `StorePersonalResourcesFollowUp`=Optional/CandidateNeedsReview
- H: `H1:FarmLivingHomeWithGuestAnchors`=Required/ExistingReused, `H1:FarmProductionPlotContext`=Optional/CandidateNeedsReview, `H2:ForestEdgeFarm`=Required/ExistingReused, `H3:NoHigherAreaRequired`=NotApplicable/NotApplicable, `H4:NoHigherAreaRequired`=NotApplicable/NotApplicable

### `PLAN-STORY-HEX03-LINE-002`

- Story Beat: `story-beat:hex03-line002:return-together-after-boundary-patrol`
- 주체: `PlayerActor`=Required/ExistingReused, `HansNpc`=Required/ExistingReused, `FarmBoundaryPatrolRoute`=Required/ExistingReused
- WI: `PatrolFarmBoundaryLoopWithHans`=Required/ExistingReused, `RecordReturnTogetherBeforeDusk`=Required/ExistingReused
- H: `H1:FarmToolStorage`=Optional/CandidateNeedsReview, `H2:ForestEdgeFarm`=Required/ExistingReused, `H3:NoHigherAreaRequired`=NotApplicable/NotApplicable, `H4:NoHigherAreaRequired`=NotApplicable/NotApplicable

### `PLAN-STORY-HEX03-LINE-003`

- Story Beat: `story-beat:hex03-line003:refuse-unguided-pursuit`
- 주체: `PlayerActor`=Required/ExistingReused, `ThreatTraceTarget`=Required/ExistingReused
- WI: `InvestigateTrace`=Required/ExistingReused, `ChooseThreatResponse`=Required/ExistingReused
- H: `H1:IncidentTrace`=Required/ExistingReused, `H1:EmergencyRetreat`=Required/ExistingReused, `H2:ThreatResponse`=Required/ExistingReused, `H3:NoHigherAreaRequired`=NotApplicable/NotApplicable, `H4:NoHigherAreaRequired`=NotApplicable/NotApplicable

### `PLAN-STORY-HEX03-LINE-004`

- Story Beat: `story-beat:hex03-line004:receive-bounded-house-and-plot-authority`
- 주체: `PlayerActor`=Required/ExistingReused, `HansNpcAuthorityGrantor`=Required/ExistingReused, `FarmhouseManagementAuthorityTarget`=Required/ExistingReused, `PotatoPlotManagementAuthorityTarget`=Required/ExistingReused
- WI: `GrantBoundedFarmhouseAndPlotManagementAuthority`=Required/ExistingReused, `InvestigateThreatTraceAndReturnPrerequisite`=Required/ExistingReused
- H: `H1:FarmLivingHomeAuthorityBoundary`=Required/ExistingReused, `H1:PotatoCultivationPlotAuthorityBoundary`=Required/ExistingReused, `H2:ForestEdgeFarm`=Required/ExistingReused, `H3:NoHigherAreaRequired`=NotApplicable/NotApplicable, `H4:NoHigherAreaRequired`=NotApplicable/NotApplicable

### `PLAN-STORY-HEX03-LINE-005`

- Story Beat: `story-beat:hex03-line005:restore-one-plot-and-farmhouse`
- 주체: `PlayerWorkActor`=Required/ExistingReused, `HansFarmhouseRepairPartner`=Required/ExistingReused, `NeglectedPotatoPlotTarget`=Required/ExistingReused, `DamagedFarmhouseTarget`=Required/ExistingReused
- WI: `RestoreNeglectedPotatoPlot`=Required/ExistingReused, `RepairFarmhouseTogether`=Required/ExistingReused, `CompleteSmallFarmOrder`=NotApplicable/NotApplicable, `OptionalForestFowlDomestication`=Optional/NewDefinitionRequired
- H: `H1:ManagedPotatoCultivationPlot`=Required/ExistingReused, `H1:FullyRepairedFarmLivingHome`=Required/ExistingReused, `H2:ForestEdgeFarm`=Required/ExistingReused, `H3:NoHigherAreaRequired`=NotApplicable/NotApplicable, `H4:NoRegionalAggregateRequired`=NotApplicable/NotApplicable

### `PLAN-STORY-HEX03-LINE-006`

- Story Beat: `story-beat:hex03-line006:recognize-limit-after-partial-farm-loss`
- 주체: `PlayerActor`=Required/ExistingReused, `HansDefenseAndDebriefNpc`=Required/ExistingReused, `DisplacedBeastGroup`=Required/ExistingReused, `FarmhouseDefenseObjectiveTarget`=Required/ExistingReused, `FarmProductionObjectiveTarget`=Required/ExistingReused, `FenceDefenseObjectiveTarget`=Required/ExistingReused, `WorldThreatRule`=Required/ExistingReused
- WI: `ChooseThreatResponse`=Required/ExistingReused, `PrepareOneTickFarmMultiObjectiveDefense`=Required/ExistingReused, `ResolvePartialFarmDefenseLoss`=Required/ExistingReused, `RecoverAndReturnAfterPartialDefense`=Required/ExistingReused, `DebriefFarmCrisisLearningNeeds`=Required/ExistingReused
- H: `H1:FarmhouseDefenseObjective`=Required/ExistingReused, `H1:FarmProductionObjective`=Required/ExistingReused, `H1:FarmFenceDefenseChokepoint`=Required/ExistingReused, `H2:FarmBoundaryDefenseRecovery`=Required/ExistingReused, `H3:ForestEdgeLivingFarmCampaign`=Required/ExistingReused, `H4:NoRegionalAggregateRequired`=NotApplicable/NotApplicable
