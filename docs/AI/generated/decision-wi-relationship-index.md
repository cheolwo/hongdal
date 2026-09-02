# 결정과 WI 양방향 관계 색인

> [DECISIONS.md](../DECISIONS.md), [결정 분야별 전수 색인](decision-field-index.md), [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json)을 읽어 생성한다. 직접 수정하지 않는다.

## 판정 경계

- `ExplicitMention`: 결정 본문에 현재 공식 WI ID가 직접 적혔거나 같은 접두사의 숫자 범위로 명시된 관계다.
- 분야·제목·순번이 비슷하다는 이유로 연결하지 않는다. 명시 관계가 없으면 `미명시`로 남긴다.
- 관계는 기획 탐색용이다. 승인·구현·E 단계·Unity 배치·실행 증거를 뜻하지 않는다.
- 공식 WI 대장에 없는 표기는 아래 비정규 표기 표에서 따로 확인한다.

## 전수 요약

- 결정 **553개**, 공식 WI **105개**, 명시 관계 **76쌍**
- WI가 명시된 결정 **26개**, 결정을 명시적으로 연결한 WI **40개**
- 비정규 WI 표기 **2종 / 3건**
- WI 대장 판본: `simulation-world-interactions.r43`

## 결정에서 WI 보기

| 전역 결정 | 분야별 결정 | 분야 / 주제 | 결정 | 공식 WI 명시 | 상태 |
| --- | --- | --- | --- | --- | --- |
| [D-001](../DECISIONS.md#L59) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-001` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | Unity 개발 순서는 제품 버전에 종속하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-002](../DECISIONS.md#L66) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-002` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | Unity는 전체 도메인을 World 관점에서 통합한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-003](../DECISIONS.md#L75) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-003` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | 운영 상태의 최종 권위는 서버다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-004](../DECISIONS.md#L84) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-004` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | Simulation과 Operational 상태를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-005](../DECISIONS.md#L93) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-005` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | Sensor는 단일 관측 projection을 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-006](../DECISIONS.md#L102) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-006` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | Git 저장소 문서를 AI 공용 기억으로 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-007](../DECISIONS.md#L116) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-007` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | 외부 시각 asset은 View wrapper 뒤에 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-008](../DECISIONS.md#L123) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-008` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | DbSet과 Unity Controller를 1:1로 대응하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-009](../DECISIONS.md#L132) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-009` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | 첫 Presentation vertical slice는 도심마트다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-010](../DECISIONS.md#L141) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-010` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | 차량 중심 차고가 아니라 도심 물류센터를 Zone으로 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-011](../DECISIONS.md#L150) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-011` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | Unity Presentation composition root는 VContainer를 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-012](../DECISIONS.md#L159) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-012` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | World는 공유하고 Role Perspective를 겹친다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-013](../DECISIONS.md#L173) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-013` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | NPC 이동은 업무 상태의 Presentation이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-014](../DECISIONS.md#L184) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-014` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | 농장 운영 aggregate와 공개 작물 기준을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-015](../DECISIONS.md#L195) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-015` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | Unity 심화 개발 단위는 Zone 업무 흐름이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-016](../DECISIONS.md#L206) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-016` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | Unity 읽기 흐름은 Data·Interpretation·Presentation을 기본으로 한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-017](../DECISIONS.md#L223) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-017` | `ARCHITECTURE` / `UNITY-WORLD-FOUNDATION` | WorldState와 identity·runtime 경계를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-018](../DECISIONS.md#L236) | `D-DATA-MARKET-SUPPLY-001` | `DATA` / `MARKET-SUPPLY` | 비교 가격의 파생값은 단계간 가격차로 표현한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-019](../DECISIONS.md#L247) | `D-DATA-MARKET-SUPPLY-002` | `DATA` / `MARKET-SUPPLY` | Interpretation은 Shared World와 Perspective 단계로 나눈다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-020](../DECISIONS.md#L258) | `D-DATA-MARKET-SUPPLY-003` | `DATA` / `MARKET-SUPPLY` | Data 조회는 Session·World·Authorization scope에 묶는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-021](../DECISIONS.md#L269) | `D-DATA-MARKET-SUPPLY-004` | `DATA` / `MARKET-SUPPLY` | 외부·공공 데이터는 서버 수집·정규화 경계를 통과한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-022](../DECISIONS.md#L280) | `D-DATA-MARKET-SUPPLY-005` | `DATA` / `MARKET-SUPPLY` | 외부 공급자 단계는 계약 조사와 실제 연결을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-023](../DECISIONS.md#L291) | `D-DATA-MARKET-SUPPLY-006` | `DATA` / `MARKET-SUPPLY` | 첫 실제 농업 공급자는 World Bank 최신 경지면적 한 건으로 제한한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-024](../DECISIONS.md#L300) | `D-DATA-MARKET-SUPPLY-007` | `DATA` / `MARKET-SUPPLY` | 도심마트 첫 운영 업무는 진열 보충으로 3계층 migration한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-025](../DECISIONS.md#L311) | `D-DATA-MARKET-SUPPLY-008` | `DATA` / `MARKET-SUPPLY` | 도심마트 관리자 우선순위보다 재고 할당 무결성을 먼저 보강한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-026](../DECISIONS.md#L322) | `D-DATA-MARKET-SUPPLY-009` | `DATA` / `MARKET-SUPPLY` | UM5 뒤 도심마트 공급 계약 경영 Simulation을 우선한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-027](../DECISIONS.md#L333) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-001` | `ARCHITECTURE` / `OPERATIONS-SIMULATION` | 운영 서버와 게임 Simulation 서버를 물리 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-028](../DECISIONS.md#L344) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-002` | `ARCHITECTURE` / `OPERATIONS-SIMULATION` | 공급계약 Simulation 전에 지역 수요와 주문 객체를 명시한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-029](../DECISIONS.md#L356) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-003` | `ARCHITECTURE` / `OPERATIONS-SIMULATION` | 공동주택 주문자 집단은 기존 공동구매 원장과 개별 주문 집계를 재사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-030](../DECISIONS.md#L368) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-004` | `ARCHITECTURE` / `OPERATIONS-SIMULATION` | 공동주택 대표의 사회적 context·업무 권한·NPC 표현을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-031](../DECISIONS.md#L380) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-005` | `ARCHITECTURE` / `OPERATIONS-SIMULATION` | Unity 업무 학습은 공통 Concept Card Presentation 문법으로 제공한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-032](../DECISIONS.md#L394) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-006` | `ARCHITECTURE` / `OPERATIONS-SIMULATION` | 도심마트 관리자 30초 업무 Queue와 우선순위 점수를 제거한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-033](../DECISIONS.md#L404) | `D-PRESENTATION-WORLD-REGION-ASSET-001` | `PRESENTATION` / `WORLD-REGION-ASSET` | Farm·Town·City를 독립 Presentation Region으로 구성하고 이동망으로 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-034](../DECISIONS.md#L416) | `D-PRESENTATION-WORLD-REGION-ASSET-002` | `PRESENTATION` / `WORLD-REGION-ASSET` | Town과 City 사이에 다중 origin 지역 물류허브를 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-035](../DECISIONS.md#L428) | `D-PRESENTATION-WORLD-REGION-ASSET-003` | `PRESENTATION` / `WORLD-REGION-ASSET` | Synty animation은 실제 source를 검증하고 공용 Presentation adapter로 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-036](../DECISIONS.md#L438) | `D-DATA-PRODUCT-ASSET-IDENTITY-001` | `DATA` / `PRODUCT-ASSET-IDENTITY` | 공통 상품 stable ID와 출처별 품목코드를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-037](../DECISIONS.md#L452) | `D-DATA-PRODUCT-ASSET-IDENTITY-002` | `DATA` / `PRODUCT-ASSET-IDENTITY` | 다품목 승격과 Farm asset 대응을 별도 검토 축으로 유지한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-038](../DECISIONS.md#L464) | `D-SIMULATION-SETTLEMENT-ECONOMY-001` | `SIMULATION` / `SETTLEMENT-ECONOMY` | 정착지 경영·분쟁 Simulation은 공통 World와 경제 인과를 먼저 닫는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-039](../DECISIONS.md#L476) | `D-SIMULATION-SETTLEMENT-ECONOMY-002` | `SIMULATION` / `SETTLEMENT-ECONOMY` | Simulation save는 versioned package와 Command replay로 검증한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-040](../DECISIONS.md#L488) | `D-SIMULATION-SETTLEMENT-ECONOMY-003` | `SIMULATION` / `SETTLEMENT-ECONOMY` | 정착지 초기 경제는 scenario 입력과 독립 원장 지표로 구성한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-041](../DECISIONS.md#L500) | `D-SIMULATION-SETTLEMENT-ECONOMY-004` | `SIMULATION` / `SETTLEMENT-ECONOMY` | 수확 판로 영향과 비축은 서버 계산 후보로 먼저 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-042](../DECISIONS.md#L512) | `D-SIMULATION-SETTLEMENT-ECONOMY-005` | `SIMULATION` / `SETTLEMENT-ECONOMY` | World Map과 정착지 내부는 같은 Simulation snapshot의 관찰 규모다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-043](../DECISIONS.md#L522) | `D-SIMULATION-SETTLEMENT-ECONOMY-006` | `SIMULATION` / `SETTLEMENT-ECONOMY` | 수확 판로 Confirm은 capacity 예약이고 Task 완료 Tick은 경제 원장 적용이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-044](../DECISIONS.md#L534) | `D-SIMULATION-SETTLEMENT-ECONOMY-007` | `SIMULATION` / `SETTLEMENT-ECONOMY` | World navigation은 상위 선택을 보존하고 하위 선택만 해제한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-045](../DECISIONS.md#L547) | `D-SIMULATION-SETTLEMENT-ECONOMY-008` | `SIMULATION` / `SETTLEMENT-ECONOMY` | Synty 에셋은 자동 원본 목록과 사람의 연구 기록을 분리해 승격한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-046](../DECISIONS.md#L558) | `D-SIMULATION-SETTLEMENT-ECONOMY-009` | `SIMULATION` / `SETTLEMENT-ECONOMY` | Unity 판로 adapter는 서버 Preview 입력과 후보 Task 의미만 구성한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-047](../DECISIONS.md#L569) | `D-SIMULATION-SETTLEMENT-ECONOMY-010` | `SIMULATION` / `SETTLEMENT-ECONOMY` | Unity 연구 Scene 파일명은 한국어 목적 이름을 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-048](../DECISIONS.md#L580) | `D-SIMULATION-SETTLEMENT-ECONOMY-011` | `SIMULATION` / `SETTLEMENT-ECONOMY` | 정착지 1차 미술은 semantic VisualKey와 고정 Presentation 시간으로 구성한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-049](../DECISIONS.md#L591) | `D-SIMULATION-SETTLEMENT-ECONOMY-012` | `SIMULATION` / `SETTLEMENT-ECONOMY` | Unity 정착지 상호작용은 Simulation authority 응답만 reconcile한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-050](../DECISIONS.md#L602) | `D-SIMULATION-SETTLEMENT-ECONOMY-013` | `SIMULATION` / `SETTLEMENT-ECONOMY` | Cargo 이동은 공통 WorldTick Task와 원재고 예약을 함께 보존한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-051](../DECISIONS.md#L611) | `D-PLANNING-CODE-NAMING-001` | `PLANNING` / `CODE-NAMING` | Unity C# 이름은 한국어 업무 의미와 영어 기술 역할을 조합한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-052](../DECISIONS.md#L622) | `D-SIMULATION-LOGISTICS-TRADE-001` | `SIMULATION` / `LOGISTICS-TRADE` | 운영 API의 업무 규칙은 순수 공통 계층을 거쳐 Simulation에 적용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-053](../DECISIONS.md#L632) | `D-SIMULATION-LOGISTICS-TRADE-002` | `SIMULATION` / `LOGISTICS-TRADE` | Simulation 화물운송은 Cargo 이동과 업무 상태 원장을 분리해 결합한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-054](../DECISIONS.md#L642) | `D-SIMULATION-LOGISTICS-TRADE-003` | `SIMULATION` / `LOGISTICS-TRADE` | Simulation 같이주문은 명시적 개별 의향을 보존한 모집 결과 원장이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-055](../DECISIONS.md#L652) | `D-SIMULATION-LOGISTICS-TRADE-004` | `SIMULATION` / `LOGISTICS-TRADE` | Simulation 음식배달의 전달과 주문자 수령 확인을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-056](../DECISIONS.md#L662) | `D-SIMULATION-LOGISTICS-TRADE-005` | `SIMULATION` / `LOGISTICS-TRADE` | 주민 소비는 주문 이행에서 이미 차감된 시장재고를 다시 차감하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-057](../DECISIONS.md#L672) | `D-SIMULATION-LOGISTICS-TRADE-006` | `SIMULATION` / `LOGISTICS-TRADE` | 수출 준비 검사는 운영 수출이 아니라 실패를 보존하는 Simulation 후보 원장이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-058](../DECISIONS.md#L682) | `D-SIMULATION-LOGISTICS-TRADE-007` | `SIMULATION` / `LOGISTICS-TRADE` | 수출 재작업은 실패 원장을 덮어쓰지 않는 새 검사 시도다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-059](../DECISIONS.md#L692) | `D-SIMULATION-LOGISTICS-TRADE-008` | `SIMULATION` / `LOGISTICS-TRADE` | Cargo 준비 완료는 배송대행지 인계나 차량 출발이 아니다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-060](../DECISIONS.md#L702) | `D-SIMULATION-LOGISTICS-TRADE-009` | `SIMULATION` / `LOGISTICS-TRADE` | 배송대행지 Simulation 인계와 물류 이동 시작을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-061](../DECISIONS.md#L712) | `D-SIMULATION-LOGISTICS-TRADE-010` | `SIMULATION` / `LOGISTICS-TRADE` | 수출 Cargo 물류 이동은 기존 출고 예약을 승계한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-062](../DECISIONS.md#L722) | `D-SIMULATION-LOGISTICS-TRADE-011` | `SIMULATION` / `LOGISTICS-TRADE` | 항만 준비시설 도착과 인수 완료를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-063](../DECISIONS.md#L732) | `D-SIMULATION-LOGISTICS-TRADE-012` | `SIMULATION` / `LOGISTICS-TRADE` | 수출 준비성 검토는 자기 진술형 Simulation 후보로 제한한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-064](../DECISIONS.md#L742) | `D-SIMULATION-LOGISTICS-TRADE-013` | `SIMULATION` / `LOGISTICS-TRADE` | 수출 선적 계획은 비교 가능한 추정 후보이며 재정을 바꾸지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-065](../DECISIONS.md#L752) | `D-SIMULATION-LOGISTICS-TRADE-014` | `SIMULATION` / `LOGISTICS-TRADE` | 수출 실행 결과는 seed 기반으로 숨겨 두고 기존 예상 매출과 정산한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-066](../DECISIONS.md#L762) | `D-SIMULATION-LOGISTICS-TRADE-015` | `SIMULATION` / `LOGISTICS-TRADE` | 수확물 판로 카드는 기존 원장의 읽기 projection만 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-067](../DECISIONS.md#L772) | `D-SIMULATION-LOGISTICS-TRADE-016` | `SIMULATION` / `LOGISTICS-TRADE` | Unity 판로 결과 카드는 서버 읽기 projection을 한국어로만 표현한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-068](../DECISIONS.md#L782) | `D-SIMULATION-LOGISTICS-TRADE-017` | `SIMULATION` / `LOGISTICS-TRADE` | Unity 판로 재접속은 session과 결과 목록의 동일 revision을 원자적으로 적용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-069](../DECISIONS.md#L792) | `D-SIMULATION-LOGISTICS-TRADE-018` | `SIMULATION` / `LOGISTICS-TRADE` | Unity 판로 작업 재개는 session Task의 남은 Tick만 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-070](../DECISIONS.md#L802) | `D-SIMULATION-LOGISTICS-TRADE-019` | `SIMULATION` / `LOGISTICS-TRADE` | 플레이 경영 시간은 명시적 턴 마감으로만 진행한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-072](../DECISIONS.md#L812) | `D-DATA-TIME-PUBLIC-DATA-001` | `DATA` / `TIME-PUBLIC-DATA` | 문화 턴 카드는 지역·기간·공식 원천·달력·효과 규칙이 완전할 때만 게시한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-071](../DECISIONS.md#L822) | `D-SIMULATION-LOGISTICS-TRADE-020` | `SIMULATION` / `LOGISTICS-TRADE` | Unity 다중 판로 카드는 object-Lot 명시 mapping으로만 선택한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-073](../DECISIONS.md#L832) | `D-DATA-TIME-PUBLIC-DATA-002` | `DATA` / `TIME-PUBLIC-DATA` | Unity 에셋 현실 관측은 연구 해석과 Simulation에서 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-074](../DECISIONS.md#L844) | `D-DATA-TIME-PUBLIC-DATA-003` | `DATA` / `TIME-PUBLIC-DATA` | KAMIS 대응 작물은 모판에서 연구한 뒤 Farm Scene으로 승격한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-075](../DECISIONS.md#L854) | `D-DATA-TIME-PUBLIC-DATA-004` | `DATA` / `TIME-PUBLIC-DATA` | 공공 관측 출처표와 에셋 연결표는 분리하고 모판 문맥으로 선택한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-076](../DECISIONS.md#L864) | `D-DATA-TIME-PUBLIC-DATA-005` | `DATA` / `TIME-PUBLIC-DATA` | 농사 생육은 일수 대신 환경 Snapshot의 제한 요인과 스트레스로 진행한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-077](../DECISIONS.md#L874) | `D-DATA-TIME-PUBLIC-DATA-006` | `DATA` / `TIME-PUBLIC-DATA` | Unity 턴 마감은 Confirm 뒤 canonical session을 다시 조회한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-078](../DECISIONS.md#L884) | `D-DATA-TIME-PUBLIC-DATA-007` | `DATA` / `TIME-PUBLIC-DATA` | 턴 카드는 분야별 모판에서 검증한 뒤 서버 덱으로 승격한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-079](../DECISIONS.md#L894) | `D-DATA-TIME-PUBLIC-DATA-008` | `DATA` / `TIME-PUBLIC-DATA` | 농사로 작업군·콘텐츠·canonical 상품 관계를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-080](../DECISIONS.md#L904) | `D-DATA-TIME-PUBLIC-DATA-009` | `DATA` / `TIME-PUBLIC-DATA` | 기상청 ASOS 일관측은 지점·날짜·원문 단위로 보존한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-081](../DECISIONS.md#L914) | `D-SIMULATION-WORLD-OBJECT-RULES-001` | `SIMULATION` / `WORLD-OBJECT-RULES` | 통합 모판·전시관의 Scene 이식 단위는 업무 장면이 아니라 개별 Object다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-082](../DECISIONS.md#L926) | `D-SIMULATION-WORLD-OBJECT-RULES-002` | `SIMULATION` / `WORLD-OBJECT-RULES` | Simulation 생산·소비는 부호가 아니라 자원 변동 유형과 효과 묶음으로 기록한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-083](../DECISIONS.md#L938) | `D-SIMULATION-WORLD-OBJECT-RULES-003` | `SIMULATION` / `WORLD-OBJECT-RULES` | 규칙은 업무·해석·표현·상호작용 계층과 세부 영역으로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-084](../DECISIONS.md#L950) | `D-SIMULATION-WORLD-OBJECT-RULES-004` | `SIMULATION` / `WORLD-OBJECT-RULES` | 감자 생산의 첫 기준 단위는 명시적 면적을 가진 단일 Tile 재배 단위다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-085](../DECISIONS.md#L962) | `D-SIMULATION-WORLD-OBJECT-RULES-005` | `SIMULATION` / `WORLD-OBJECT-RULES` | 수요·예약·주문 이행·주민 소비는 서로 다른 자원 단계다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-086](../DECISIONS.md#L974) | `D-SIMULATION-WORLD-OBJECT-RULES-006` | `SIMULATION` / `WORLD-OBJECT-RULES` | 운송은 상차·이동 자원 소비·하차·인수 확인을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-087](../DECISIONS.md#L986) | `D-SIMULATION-WORLD-OBJECT-RULES-007` | `SIMULATION` / `WORLD-OBJECT-RULES` | 창고는 인수·검수·적치·보관·피킹·출고와 용량을 함께 기록한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-088](../DECISIONS.md#L998) | `D-SIMULATION-WORLD-OBJECT-RULES-008` | `SIMULATION` / `WORLD-OBJECT-RULES` | Unity 표현 규칙은 영역별 출력 채널과 구현 상태를 대장으로 관리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-089](../DECISIONS.md#L1010) | `D-SIMULATION-WORLD-OBJECT-RULES-009` | `SIMULATION` / `WORLD-OBJECT-RULES` | 통합 전시관의 규칙 실험대는 미리보기와 서버 재조회 결과를 비교한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-090](../DECISIONS.md#L1022) | `D-SIMULATION-WORLD-OBJECT-RULES-010` | `SIMULATION` / `WORLD-OBJECT-RULES` | Unity 감자 생산 실험대는 서버 효과를 재계산하지 않고 변환한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-091](../DECISIONS.md#L1034) | `D-SIMULATION-WORLD-OBJECT-RULES-011` | `SIMULATION` / `WORLD-OBJECT-RULES` | 다품목 Unity 모판은 서버가 보장하는 연결 깊이를 품목별로 구분한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-092](../DECISIONS.md#L1046) | `D-SIMULATION-WORLD-OBJECT-RULES-012` | `SIMULATION` / `WORLD-OBJECT-RULES` | Unity 운영 API Client는 공통 전송 계층과 인증 경계를 재사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-093](../DECISIONS.md#L1056) | `D-SIMULATION-WORLD-OBJECT-RULES-013` | `SIMULATION` / `WORLD-OBJECT-RULES` | 게임 세계 Simulation 서버를 실제 운영 전 예행연습 서버로 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-094](../DECISIONS.md#L1068) | `D-SIMULATION-WORLD-OBJECT-RULES-014` | `SIMULATION` / `WORLD-OBJECT-RULES` | Simulation 서버는 수집된 공공데이터 DB를 읽기 전용으로 공유한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-095](../DECISIONS.md#L1080) | `D-SIMULATION-WORLD-OBJECT-RULES-015` | `SIMULATION` / `WORLD-OBJECT-RULES` | 운영자 전용 재고 Shelf는 주소 지정 가능한 피킹 위치 단위다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-096#1](../DECISIONS.md#L1089) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-001` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | SimulationWorldShell의 플레이어 카메라는 Presentation 전용 입력 모듈이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-096#2](../DECISIONS.md#L1098) | `D-GAMEPLAY-TAROT-001` | `GAMEPLAY` / `TAROT` | 일반 타로를 경영 게임의 기본 덱으로 두고 학당 카드는 선택형 확장으로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-097](../DECISIONS.md#L1110) | `D-GAMEPLAY-TAROT-002` | `GAMEPLAY` / `TAROT` | 타로 규칙은 기존 업무 규칙에 보정선을 제공하는 상위 시뮬레이션 규칙이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-098](../DECISIONS.md#L1122) | `D-GAMEPLAY-TAROT-003` | `GAMEPLAY` / `TAROT` | 일반 타로 뽑기는 seed·턴·덱 개정 번호·선택 이력으로 결정한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-099](../DECISIONS.md#L1134) | `D-GAMEPLAY-TAROT-004` | `GAMEPLAY` / `TAROT` | 타로 객체 관계와 현재 강조 상태를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-100](../DECISIONS.md#L1146) | `D-WORLD-SPATIAL-DATA-PRESENTATION-001` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | 공간 World는 고정 Tile·Area·AreaSet과 통계 구성 대장으로 반복 생성한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-101](../DECISIONS.md#L1157) | `D-WORLD-SPATIAL-DATA-PRESENTATION-002` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | 행정구역별 건물은 출처별 DB 원장을 먼저 구축하고 World에 투영한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-102](../DECISIONS.md#L1169) | `D-WORLD-SPATIAL-DATA-PRESENTATION-003` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | 건축물 공식 주용도와 상위 경관 Category를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-103](../DECISIONS.md#L1179) | `D-WORLD-SPATIAL-DATA-PRESENTATION-004` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | 건축물 형태의 공식값·단순 계산값·Synty 표현값을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-104](../DECISIONS.md#L1189) | `D-WORLD-SPATIAL-DATA-PRESENTATION-005` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | 건물 안의 상호는 공개 인허가 사업장과 보수적인 주소 연결로 표현한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-105](../DECISIONS.md#L1199) | `D-WORLD-SPATIAL-DATA-PRESENTATION-006` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | 공유 공공데이터 DB와 Simulation World 파생 관계 DB를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-106](../DECISIONS.md#L1217) | `D-WORLD-SPATIAL-DATA-PRESENTATION-007` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | Unity 공간 실행과 Synty 경관 실행을 독립 파이프라인으로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-107](../DECISIONS.md#L1229) | `D-WORLD-SPATIAL-DATA-PRESENTATION-008` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | Simulation 상태는 의미 기반 렌더링 의도를 거쳐 URP 표현으로 번역한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-108](../DECISIONS.md#L1241) | `D-WORLD-SPATIAL-DATA-PRESENTATION-009` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | 공간 규칙과 Simulation 규칙은 개정 가능한 객체 표현 결합 원장에서 만난다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-109](../DECISIONS.md#L1253) | `D-WORLD-SPATIAL-DATA-PRESENTATION-010` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | 평창군 Unity 공간 표현은 전체 원장을 보존하고 건물 종류별 하나로 축약한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-110](../DECISIONS.md#L1267) | `D-WORLD-SPATIAL-DATA-PRESENTATION-011` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | Simulation 서버는 운영 서버의 컨테이너 관례를 따르되 DB 권한과 migration을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-111](../DECISIONS.md#L1279) | `D-WORLD-SPATIAL-DATA-PRESENTATION-012` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | Simulation World 파생 DB는 업무 규칙의 관계와 계보를 집결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-112](../DECISIONS.md#L1291) | `D-WORLD-SPATIAL-DATA-PRESENTATION-013` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | Unity UI 구현 전 Figma 근거 UI 기획 원장을 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-113](../DECISIONS.md#L1301) | `D-WORLD-SPATIAL-DATA-PRESENTATION-014` | `WORLD` / `SPATIAL-DATA-PRESENTATION` | UI는 규칙 식별자가 아니라 객체–업무 규칙 연결을 통해 조립한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-114](../DECISIONS.md#L1311) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-002` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | Tile과 경관 완결 영역의 책임을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-115](../DECISIONS.md#L1321) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-003` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | 경관 품질은 Synty 연결 뒤 독립 Rendering Profile로 일괄 적용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-116](../DECISIONS.md#L1329) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-004` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | 플레이어 경관 탐색은 Simulation 권위와 분리한 표현 전용 입력이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-117](../DECISIONS.md#L1343) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-005` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | NPC 직업·권한은 운영 인증이 아니라 Simulation 조직·역량·위임 규칙으로 실행한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-118](../DECISIONS.md#L1355) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-006` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | UI 행동은 실행 가능한 호출 계약과 확정 뒤 재조회를 함께 제공한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-119](../DECISIONS.md#L1367) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-007` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | 입고 검수 완료와 적재 완료를 다른 상태·행동으로 관리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-120](../DECISIONS.md#L1377) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-008` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | Figma·MAUI·Unity는 디자인 의미를 공유하고 렌더러 구현은 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-121](../DECISIONS.md#L1387) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-009` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | Unity 최종 실행은 SimulationWorldShell 하나에 통합한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-122](../DECISIONS.md#L1397) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-010` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | 1인칭 월드는 고정 L2 타일 창과 자료 상태를 따라 동적으로 준비한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-123](../DECISIONS.md#L1409) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-011` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | 타일 안전 창과 카메라 시야 기반 표현 우선순위를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-124](../DECISIONS.md#L1421) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-012` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | 월드 API는 행정동·법정동 파생 Projection을 먼저 읽는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-125](../DECISIONS.md#L1431) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-013` | `PRESENTATION` / `WORLD-CAMERA-STREAMING` | L2 Runtime은 상세 3×3·활성 5×5·준비 9×9의 예산형 창을 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-126](../DECISIONS.md#L1443) | `D-GAMEPLAY-TEAM-COMBAT-001` | `GAMEPLAY` / `TEAM-COMBAT` | 생존 타로는 안전 거점 전원 합의 뒤 다음 Tick에만 적용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-127](../DECISIONS.md#L1455) | `D-GAMEPLAY-TEAM-COMBAT-002` | `GAMEPLAY` / `TEAM-COMBAT` | 세계 사건은 서버 원장에 먼저 확정하고 Unity는 개정 기반 표현 자료만 읽는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-128](../DECISIONS.md#L1467) | `D-GAMEPLAY-TEAM-COMBAT-003` | `GAMEPLAY` / `TEAM-COMBAT` | 농장 생존은 플레이어·NPC 노동과 회복 가능한 위협을 같은 Session 원장에 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-129](../DECISIONS.md#L1479) | `D-GAMEPLAY-TEAM-COMBAT-004` | `GAMEPLAY` / `TEAM-COMBAT` | 같은 Simulation 팀은 별도 요청 없이 서로 관찰하되 조작 권한을 공유하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-130](../DECISIONS.md#L1493) | `D-GAMEPLAY-TEAM-COMBAT-005` | `GAMEPLAY` / `TEAM-COMBAT` | Simulation 역할은 고정 직업이 아니라 팀 공동 카드와 현재 활동에서 파생한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-131](../DECISIONS.md#L1505) | `D-GAMEPLAY-TEAM-COMBAT-006` | `GAMEPLAY` / `TEAM-COMBAT` | 역할 카드 규칙 정의와 현재 장착 상태의 DB 책임을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-132](../DECISIONS.md#L1515) | `D-GAMEPLAY-TEAM-COMBAT-007` | `GAMEPLAY` / `TEAM-COMBAT` | Simulation Session 저장 자료는 별도 DB에 보존하고 Command 재생으로 복원한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-133](../DECISIONS.md#L1527) | `D-GAMEPLAY-TEAM-COMBAT-008` | `GAMEPLAY` / `TEAM-COMBAT` | 농사·영역 발견 보상은 개인 수집 카드 원장으로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-134](../DECISIONS.md#L1541) | `D-GAMEPLAY-TEAM-COMBAT-009` | `GAMEPLAY` / `TEAM-COMBAT` | 활동별 기본 시점은 편의 정책이며 사용자의 허용된 수동 전환을 막지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-135](../DECISIONS.md#L1553) | `D-GAMEPLAY-TEAM-COMBAT-010` | `GAMEPLAY` / `TEAM-COMBAT` | 1인칭과 3인칭은 전환 전용 카메라로 연속 보간한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-136](../DECISIONS.md#L1563) | `D-GAMEPLAY-TEAM-COMBAT-011` | `GAMEPLAY` / `TEAM-COMBAT` | 전투는 서버 권위 단일 박자로 판정하고 시점별 이점은 일반 허용 구간에만 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-137](../DECISIONS.md#L1572) | `D-GAMEPLAY-TEAM-COMBAT-012` | `GAMEPLAY` / `TEAM-COMBAT` | 1인칭 영웅 성과는 한 명령창 동안만 주변 전선의 전술 기회가 된다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-138](../DECISIONS.md#L1581) | `D-GAMEPLAY-TEAM-COMBAT-013` | `GAMEPLAY` / `TEAM-COMBAT` | 분대 이동은 서버 판정의 교체 가능한 표현이며 기준점과 대형 슬롯을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-139](../DECISIONS.md#L1590) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-001` | `ARCHITECTURE` / `REFACTOR-DATA-ASSET` | Simulation·Unity 구조 리팩토링은 외부 계약을 보존한 채 검증 경계부터 진행한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-140](../DECISIONS.md#L1599) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-002` | `ARCHITECTURE` / `REFACTOR-DATA-ASSET` | 코드 탐색 특성이 원본이고 생성 코드 지도는 검증되는 파생 자료다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-141](../DECISIONS.md#L1610) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-003` | `ARCHITECTURE` / `REFACTOR-DATA-ASSET` | 1인칭 전투 마우스 입력은 전투 진입과 서버 판정 반응으로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-142](../DECISIONS.md#L1619) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-004` | `ARCHITECTURE` / `REFACTOR-DATA-ASSET` | 기본 생존 장은 경관 산책 중심이며 직접 전투는 계절 방어의 선택 경로다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-143](../DECISIONS.md#L1628) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-005` | `ARCHITECTURE` / `REFACTOR-DATA-ASSET` | 지역 공공데이터는 원본을 보존하고 LOD별 대표 요약과 상세 조회로 나눈다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-144](../DECISIONS.md#L1637) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-006` | `ARCHITECTURE` / `REFACTOR-DATA-ASSET` | Synty 팩은 기술 대장·팩별 기준·의미 구성·검토 계획을 거쳐 Scene에 적용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-145](../DECISIONS.md#L1646) | `D-EVIDENCE-SPATIAL-MATURITY-001` | `EVIDENCE` / `SPATIAL-MATURITY` | 미완료 작업은 증거 단계 원장으로 관리하고 중앙 L2 실자료부터 종단 완결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-146](../DECISIONS.md#L1656) | `D-EVIDENCE-SPATIAL-MATURITY-002` | `EVIDENCE` / `SPATIAL-MATURITY` | AreaSet은 문서 중심 상위 컨테이너이고 LandscapeGraph는 독립 조립·스트리밍 단위다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-147](../DECISIONS.md#L1666) | `D-EVIDENCE-SPATIAL-MATURITY-003` | `EVIDENCE` / `SPATIAL-MATURITY` | 공간과 Simulation은 세계 상호작용 단위로 종단 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-148](../DECISIONS.md#L1676) | `D-EVIDENCE-SPATIAL-MATURITY-004` | `EVIDENCE` / `SPATIAL-MATURITY` | 세계 상호작용 단위의 기본 구현 완료는 E3이고 실세계 승격은 별도로 관리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-149](../DECISIONS.md#L1686) | `D-EVIDENCE-SPATIAL-MATURITY-005` | `EVIDENCE` / `SPATIAL-MATURITY` | WI E3 승격은 핵심 인과선·공통 규칙·문서·전체 회귀 순으로 나눈다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-150](../DECISIONS.md#L1694) | `D-EVIDENCE-SPATIAL-MATURITY-006` | `EVIDENCE` / `SPATIAL-MATURITY` | E4 승격은 WI 공간 폐루프와 Graph 계보를 기준으로 개별 판정한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-151](../DECISIONS.md#L1704) | `D-EVIDENCE-SPATIAL-MATURITY-007` | `EVIDENCE` / `SPATIAL-MATURITY` | E4~E7은 장소·경관·공공데이터·실제 플레이로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-152](../DECISIONS.md#L1714) | `D-EVIDENCE-SPATIAL-MATURITY-008` | `EVIDENCE` / `SPATIAL-MATURITY` | E4는 WI 공간 모판이고 E5는 실제 지역 경관 조립이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-153](../DECISIONS.md#L1725) | `D-EVIDENCE-SPATIAL-MATURITY-009` | `EVIDENCE` / `SPATIAL-MATURITY` | E 증거 단계와 H 공간 포함 계층을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-154](../DECISIONS.md#L1735) | `D-EVIDENCE-SPATIAL-MATURITY-010` | `EVIDENCE` / `SPATIAL-MATURITY` | 모판을 H1~H4 상향 조립 공간 구성 재고로 확장한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-155](../DECISIONS.md#L1746) | `D-EVIDENCE-SPATIAL-MATURITY-011` | `EVIDENCE` / `SPATIAL-MATURITY` | Synty 상향식 공간 재고는 공식 H 계층과 분리해 축적한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-156](../DECISIONS.md#L1756) | `D-EVIDENCE-SPATIAL-MATURITY-012` | `EVIDENCE` / `SPATIAL-MATURITY` | 기준 경관 문법은 검토된 조립법으로 H1~H4 설계 후보를 유도한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-157](../DECISIONS.md#L1766) | `D-EVIDENCE-SPATIAL-MATURITY-013` | `EVIDENCE` / `SPATIAL-MATURITY` | 오픈 월드는 고정 좌표 경계가 아니라 H4 의도와 H3·H2 Streaming Coverage로 연다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-158](../DECISIONS.md#L1777) | `D-EVIDENCE-SPATIAL-MATURITY-014` | `EVIDENCE` / `SPATIAL-MATURITY` | LH 엔진은 L 해상도와 H 의미 권위를 직교시키고 승인 H4 안에서 결정적으로 선행 생성한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-159](../DECISIONS.md#L1787) | `D-EVIDENCE-SPATIAL-MATURITY-015` | `EVIDENCE` / `SPATIAL-MATURITY` | WI별 E/H 성립 상태는 후보 계보와 실행 증거를 분리해 LH 인계 입력으로 생성한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-160](../DECISIONS.md#L1797) | `D-EVIDENCE-SPATIAL-MATURITY-016` | `EVIDENCE` / `SPATIAL-MATURITY` | 싱글 플레이 LH 지도 생성은 로컬 엔진을 기본 권위로 하고 서버 연결은 선택 동기화로 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-161](../DECISIONS.md#L1807) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-001` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | 음식·화물 배달은 NPC 경로 이동을 기본 수행 방식으로 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-162](../DECISIONS.md#L1817) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-002` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | Nature 생활권은 주인공의 상시 체류 세계이고 Farm·Town·City/Hub는 전문 경관 인스턴스다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-163](../DECISIONS.md#L1827) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-003` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | 전문 경관의 미해결 사건은 경로별 자연권 위협으로 전파한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-164](../DECISIONS.md#L1837) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-004` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | 상향식 공간 재고는 Nature 위협·회복 카드부터 작은 묶음으로 확장한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-165](../DECISIONS.md#L1846) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-005` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | 사건 대응 H1은 Nature에서 시작해 Farm과 Town으로 이어지는 H2 우선순위로 조립한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-166](../DECISIONS.md#L1855) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-006` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | Nature·Farm·Town·City/Hub는 각각 독립 AreaSet 후보로 상향 조립한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-167](../DECISIONS.md#L1865) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-007` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | Farm AreaSet 후보는 생산 흐름과 사건 격리·회복 흐름을 함께 포함한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-168](../DECISIONS.md#L1874) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-008` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | Town AreaSet 후보는 시장 생활 흐름과 오염 통제·주민 구호 흐름을 함께 포함한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-169](../DECISIONS.md#L1883) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-009` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | H1~H4는 위치 독립 공간 설계 계층이며 공공데이터 결속은 E6에서만 수행한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-170](../DECISIONS.md#L1892) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-010` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | 게임 기획 묶음이 H 재고 범위를 통제하고 H에서 WI의 E 부족분을 유도한다 | `WI-ORDER-04` | `ExplicitCanonicalLink` |
| [D-171](../DECISIONS.md#L1902) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-011` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | Nature 위협 대응의 예상 플레이 네 동사를 독립 E1 WI 계약으로 고정한다 | `WI-NATURE-01`, `WI-NATURE-02`, `WI-NATURE-03`, `WI-NATURE-04` | `ExplicitCanonicalLink` |
| [D-172](../DECISIONS.md#L1912) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-012` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | Nature H 설계는 반복 플레이 폐루프와 계획 용량을 먼저 봉인한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-173](../DECISIONS.md#L1922) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-013` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | 자연권 위협 관찰은 기존 결정·작업·효과 원장을 재사용해 E3로 완결한다 | `WI-NATURE-01` | `ExplicitCanonicalLink` |
| [D-174](../DECISIONS.md#L1932) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-014` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | 자연권 긴급 후퇴는 선행 위협 근거와 경로 예약으로 E3를 닫는다 | `WI-NATURE-02`, `WI-NATURE-04` | `ExplicitCanonicalLink` |
| [D-175](../DECISIONS.md#L1942) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-015` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | 자연권 복원은 관찰된 원인 전체 해결 후에만 자재를 소비한다 | `WI-NATURE-03` | `ExplicitCanonicalLink` |
| [D-176](../DECISIONS.md#L1953) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-016` | `GAMEPLAY` / `NATURE-THREAT-RECOVERY` | 파티 회복은 후퇴 또는 복원 효과 후 탐색을 재개하는 E3 행위다 | `WI-NATURE-04` | `ExplicitCanonicalLink` |
| [D-177](../DECISIONS.md#L1964) | `D-WORLD-H-LH-ASSET-COMPOSITION-001` | `WORLD` / `H-LH-ASSET-COMPOSITION` | Nature 팩 중심 상시 체류 세계를 심리 영역으로 정의하고 두 발전소 인과를 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-178](../DECISIONS.md#L1976) | `D-WORLD-H-LH-ASSET-COMPOSITION-002` | `WORLD` / `H-LH-ASSET-COMPOSITION` | Construction 팩은 공통 조립층이며 두 발전소는 기존 Nature H2·H3를 확장한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-179](../DECISIONS.md#L1987) | `D-WORLD-H-LH-ASSET-COMPOSITION-003` | `WORLD` / `H-LH-ASSET-COMPOSITION` | 다섯 Synty 팩은 H 승격 전에 전수 기술 대장과 의미 자산군으로 관리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-180](../DECISIONS.md#L1998) | `D-WORLD-H-LH-ASSET-COMPOSITION-004` | `WORLD` / `H-LH-ASSET-COMPOSITION` | 휴대폰 공간 조립 검토는 주차 후 후보 선별이며 최종 Scene 승인이 아니다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-181](../DECISIONS.md#L2009) | `D-WORLD-H-LH-ASSET-COMPOSITION-005` | `WORLD` / `H-LH-ASSET-COMPOSITION` | Synty Web 검토 v2는 불변 촬영 영수증과 부모 bundle 계보로 재촬영을 닫는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-182](../DECISIONS.md#L2021) | `D-WORLD-H-LH-ASSET-COMPOSITION-006` | `WORLD` / `H-LH-ASSET-COMPOSITION` | Unity 산출물 검토 WebApp은 일반 업무 WebApp과 물리 프로젝트를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-183](../DECISIONS.md#L2030) | `D-WORLD-H-LH-ASSET-COMPOSITION-007` | `WORLD` / `H-LH-ASSET-COMPOSITION` | Synty 검토 폐루프는 저장·화면 상태·전송·촬영 조립 책임을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-184](../DECISIONS.md#L2040) | `D-WORLD-H-LH-ASSET-COMPOSITION-008` | `WORLD` / `H-LH-ASSET-COMPOSITION` | Unity 산출물 검토 앱은 기존 Azure VM에서 별도 경로·배포 묶음으로 운영한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-185](../DECISIONS.md#L2050) | `D-WORLD-H-LH-ASSET-COMPOSITION-009` | `WORLD` / `H-LH-ASSET-COMPOSITION` | H1~H4 Unity 조합물은 선택 Root 촬영 영수증으로 모바일 검토하되 공간 권위를 만들지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-186](../DECISIONS.md#L2060) | `D-WORLD-H-LH-ASSET-COMPOSITION-010` | `WORLD` / `H-LH-ASSET-COMPOSITION` | Unity 산출물 검토는 역할별 VM과 분리한 무료 대상 VM의 최소 Docker 스택으로 운영한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-187](../DECISIONS.md#L2071) | `D-WORLD-H-LH-ASSET-COMPOSITION-011` | `WORLD` / `H-LH-ASSET-COMPOSITION` | H1은 인지 부품이고 H2는 첫 공간 조합 판단 단위다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-188](../DECISIONS.md#L2081) | `D-WORLD-H-LH-ASSET-COMPOSITION-012` | `WORLD` / `H-LH-ASSET-COMPOSITION` | H 조립·게임플레이 추적·E 증거·완주 상태는 독립 축으로 관리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-189](../DECISIONS.md#L2091) | `D-WORLD-H-LH-ASSET-COMPOSITION-013` | `WORLD` / `H-LH-ASSET-COMPOSITION` | H2·H3와 이론 E5 공간 생산은 사람 검토를 차단 관문으로 사용하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-190](../DECISIONS.md#L2101) | `D-WORLD-H-LH-ASSET-COMPOSITION-014` | `WORLD` / `H-LH-ASSET-COMPOSITION` | 이론 공간 공급과 실제 플레이 공간 완성 사이에 독립 완료 상태를 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-191](../DECISIONS.md#L2111) | `D-WORLD-H-LH-ASSET-COMPOSITION-015` | `WORLD` / `H-LH-ASSET-COMPOSITION` | H2·H3는 StableId를 보존하고 팩 주도 패턴 이름을 별도로 가진다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-192](../DECISIONS.md#L2123) | `D-WORLD-H-LH-ASSET-COMPOSITION-016` | `WORLD` / `H-LH-ASSET-COMPOSITION` | 팩 단독 H2는 팩 내부 H3보다 먼저 게임 기획 AreaSet에 대기 결속한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-193](../DECISIONS.md#L2132) | `D-WORLD-H-LH-ASSET-COMPOSITION-017` | `WORLD` / `H-LH-ASSET-COMPOSITION` | 팩 내부 H3가 준비되면 AreaSet의 임시 H2 직접 참조를 H3 계보로 대체한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-194](../DECISIONS.md#L2142) | `D-WORLD-H-LH-ASSET-COMPOSITION-018` | `WORLD` / `H-LH-ASSET-COMPOSITION` | Nature–Town 혼합 경관은 선택 계보로 만들고 실제 E5 배치를 자동 생성하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-195](../DECISIONS.md#L2152) | `D-WORLD-H-LH-ASSET-COMPOSITION-019` | `WORLD` / `H-LH-ASSET-COMPOSITION` | H2는 배치 가능한 물리 블록이고 H3는 배치 가능한 구역 조립안이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-196](../DECISIONS.md#L2163) | `D-WORLD-H-LH-ASSET-COMPOSITION-020` | `WORLD` / `H-LH-ASSET-COMPOSITION` | 실제 E5는 네 전용 AreaSet과 하나의 Network로 결속하고 모든 이론 H3의 처리를 명시한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-197](../DECISIONS.md#L2174) | `D-WORLD-H-LH-ASSET-COMPOSITION-021` | `WORLD` / `H-LH-ASSET-COMPOSITION` | 지역 위협·회복과 카드 효과는 서버 권위 v5 인과 원장으로 계산한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-198](../DECISIONS.md#L2184) | `D-WORLD-H-LH-ASSET-COMPOSITION-022` | `WORLD` / `H-LH-ASSET-COMPOSITION` | H 공간 공장은 모든 계층에서 명시적 연결점 의미와 방향성 흐름을 재귀 검증한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-199](../DECISIONS.md#L2195) | `D-WORLD-H-LH-ASSET-COMPOSITION-023` | `WORLD` / `H-LH-ASSET-COMPOSITION` | LH는 스트리밍 범위와 셀 내용을 분리하고 L과 H를 조회 관계로만 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-200](../DECISIONS.md#L2206) | `D-WORLD-H-LH-ASSET-COMPOSITION-024` | `WORLD` / `H-LH-ASSET-COMPOSITION` | H2·H3 재고는 팩별 수량이 아니라 게임플레이 공간 수요로 증산한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-201](../DECISIONS.md#L2215) | `D-EVIDENCE-WORLD-PLAY-LOOPS-001` | `EVIDENCE` / `WORLD-PLAY-LOOPS` | E8은 NPC 생활세계의 자율 행동 폐루프를 검증한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-202](../DECISIONS.md#L2227) | `D-EVIDENCE-WORLD-PLAY-LOOPS-002` | `EVIDENCE` / `WORLD-PLAY-LOOPS` | H5는 권위 상대 공간이며 E6는 선택형 현실 결속이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-203](../DECISIONS.md#L2239) | `D-EVIDENCE-WORLD-PLAY-LOOPS-003` | `EVIDENCE` / `WORLD-PLAY-LOOPS` | DEM·도로는 공통 필수 자료가 아니라 현실 결속 프로필의 선택 요구다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-207](../DECISIONS.md#L2249) | `D-EVIDENCE-WORLD-PLAY-LOOPS-007` | `EVIDENCE` / `WORLD-PLAY-LOOPS` | E6는 AreaSet 정밀 몰입 성숙도이며 GIS 결속은 독립 선택 축이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-204](../DECISIONS.md#L2260) | `D-EVIDENCE-WORLD-PLAY-LOOPS-004` | `EVIDENCE` / `WORLD-PLAY-LOOPS` | 전투 맵은 H5의 확대가 아니라 지역 문맥에서 결정적으로 파생한 독립 공간이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-205](../DECISIONS.md#L2272) | `D-EVIDENCE-WORLD-PLAY-LOOPS-005` | `EVIDENCE` / `WORLD-PLAY-LOOPS` | H5 통합 생활세계는 정적 장소와 Session 가변 시설을 결합한 WI 폐루프로 구현한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-206](../DECISIONS.md#L2285) | `D-EVIDENCE-WORLD-PLAY-LOOPS-006` | `EVIDENCE` / `WORLD-PLAY-LOOPS` | 소규모 현장 전투와 대규모 파생 전장은 같은 서버 전투 원장을 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-208](../DECISIONS.md#L2297) | `D-EVIDENCE-WORLD-PLAY-LOOPS-008` | `EVIDENCE` / `WORLD-PLAY-LOOPS` | 카드 서랍은 의미 투영을 통합하되 원장·권위·실행 책임을 통합하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-209](../DECISIONS.md#L2308) | `D-EVIDENCE-WORLD-PLAY-LOOPS-009` | `EVIDENCE` / `WORLD-PLAY-LOOPS` | 첫 E7 플레이 폐루프는 네이처 탐험·접근 조우·현장 대응·탐험 복귀다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-210](../DECISIONS.md#L2318) | `D-EVIDENCE-WORLD-PLAY-LOOPS-010` | `EVIDENCE` / `WORLD-PLAY-LOOPS` | 첫 네이처 실제 공간은 기존 생활핵·조우·방어 H3를 완전한 복귀 폐루프로 조립한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-211](../DECISIONS.md#L2328) | `D-GAMEPLAY-TAROT-REALITY-SPATIAL-001` | `GAMEPLAY` / `TAROT-REALITY-SPATIAL` | 바보는 항상 활성인 타로 여정 루트이고 현재 메이저 아르카나는 그 아래의 가변 문맥이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-212](../DECISIONS.md#L2338) | `D-GAMEPLAY-TAROT-REALITY-SPATIAL-002` | `GAMEPLAY` / `TAROT-REALITY-SPATIAL` | E6 현실 자료는 세션 시작 상태 사본으로 동결하고 Unity에는 게임 현상을 우선 표현한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-213](../DECISIONS.md#L2349) | `D-GAMEPLAY-TAROT-REALITY-SPATIAL-003` | `GAMEPLAY` / `TAROT-REALITY-SPATIAL` | AreaSet 구성 패턴은 H2·H3 재고를 역할 슬롯으로 조립하는 위치 독립 제작 계약이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-214](../DECISIONS.md#L2360) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-001` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | Farm·Hub·City 독립 우선, 경로 연결 후속 | 미명시 | `NoExplicitCanonicalLink` |
| [D-215](../DECISIONS.md#L2369) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-002` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | E는 증거 성숙도이고 G는 다음 E로 올리는 관리 체계다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-216](../DECISIONS.md#L2380) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-003` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | 권위 지도 묶음은 플레이 목적부터 실행 증거까지 잇는 navigation 기준이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-217](../DECISIONS.md#L2389) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-004` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | 배치 통제 계층은 H 공간 의미와 분리하고 Player 실측 크기를 기준으로 한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-218](../DECISIONS.md#L2399) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-005` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | 게임 개발은 현재 목표에서 증거와 다음 판단까지 같은 업무 순서를 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-219](../DECISIONS.md#L2409) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-006` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | Nature는 생존 생활거점을 1차 플레이로 두고 심리 회복을 그 결과에 결합한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-220](../DECISIONS.md#L2421) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-007` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | WorldTick·권위 실시간·표현 실시간·BattleTick을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-221](../DECISIONS.md#L2431) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-008` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | Simulation Core는 게임 세계이고 Server는 Hosted Host다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-222](../DECISIONS.md#L2442) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-009` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | 의미 있는 게임 작업은 E9 목표부터 하향 분해하고 E1부터 상향 검증한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-223](../DECISIONS.md#L2453) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-010` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | E6는 E5 세계를 E7 전에 정제하는 관문이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-224](../DECISIONS.md#L2463) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-011` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | 기존 통합 이력은 보존하고 새 작업은 운영·Simulation·Unity 책임으로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-225](../DECISIONS.md#L2473) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-012` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | 게임 작업은 플레이어 선택 폐루프를 E·WI·H 전 단계의 공통 관점으로 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-226](../DECISIONS.md#L2484) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-013` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | 지역 사건·Nature 위협·전투 결과·지역 발전을 독립 모듈로 잇는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-227](../DECISIONS.md#L2494) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-014` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | 최근 개발 철학의 반복 경계를 프로젝트 불변 골격으로 고정한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-228](../DECISIONS.md#L2504) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-015` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | 게임 개발 기준 문서는 질문별 단일 책임을 갖고 대체 경로는 호환 안내로 남긴다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-229](../DECISIONS.md#L2513) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-016` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | E9와 E1 사이는 한 번 통과하는 파이프라인이 아니라 안정될 때까지 반복 왕복한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-230](../DECISIONS.md#L2523) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-017` | `ARCHITECTURE` / `PLAYABLE-DEVELOPMENT` | 게임 코드에는 E 증거 상태가 아니라 E 검토 책임을 메타데이터로 남긴다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-231](../DECISIONS.md#L2534) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-001` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 플레이어가 확정하는 Nature 생존 행동은 정식 WI로 관리한다 | `WI-NATURE-01`, `WI-NATURE-02`, `WI-NATURE-03`, `WI-NATURE-04`, `WI-NATURE-05`, `WI-NATURE-06`, `WI-NATURE-07`, `WI-NATURE-08`, `WI-NATURE-09`, `WI-NATURE-10`, `WI-NATURE-11` | `ExplicitCanonicalLink` |
| [D-232](../DECISIONS.md#L2544) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-002` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | E 성숙도의 주어는 WI이며 공간은 조건부 발현 증거다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-233](../DECISIONS.md#L2555) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-003` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 진행 작업 취소는 예약을 반환하는 독립 WI이며 원래 공간 문맥을 이어받는다 | `WI-NATURE-05`, `WI-NATURE-06`, `WI-NATURE-07`, `WI-NATURE-08`, `WI-NATURE-09`, `WI-NATURE-10`, `WI-NATURE-11`, `WI-NATURE-12` | `ExplicitCanonicalLink` |
| [D-234](../DECISIONS.md#L2565) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-004` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | PlayableLoop와 EvidencePackage를 WI·H·E 사이의 공식 연결 객체로 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-235](../DECISIONS.md#L2576) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-005` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 메이저 아르카나 방향은 카드가 아니라 활성화 인스턴스에 귀속한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-236](../DECISIONS.md#L2587) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-006` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | Marketplace 상품 관측은 Item 효과 근거이고 Synty는 범주형 외형이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-237](../DECISIONS.md#L2597) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-007` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 현장 전투 참여 방식은 관찰 운영과 직접 개입을 같은 권위 원장에서 잠근다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-238](../DECISIONS.md#L2609) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-008` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 영역 건물 발전은 다섯 독립 누적 트리로 관리하고 Nature부터 실제 구현한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-239](../DECISIONS.md#L2620) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-009` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 플레이 폐루프는 Core·Extension 자식과 영역·세계 집계로 완결 판정한다 | `WI-CARD-01`, `WI-HUB-05`, `WI-HUB-06`, `WI-WORLD-06` | `ExplicitCanonicalLink` |
| [D-240](../DECISIONS.md#L2632) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-010` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 플레이어 활동은 역할이 아니라 현장 원정·영역 운영·영역 제조의 선택 가능한 세 갈래로 분류한다 | `WI-NATURE-16` | `ExplicitCanonicalLink` |
| [D-241](../DECISIONS.md#L2643) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-011` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 운영 유래 반복 WI는 NPC 루틴이고 플레이어는 정책과 예외를 통제한다 | `WI-HUB-03`, `WI-HUB-04`, `WI-HUB-05`, `WI-HUB-06` | `ExplicitCanonicalLink` |
| [D-242](../DECISIONS.md#L2654) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-012` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 배치 통제의 주 성립 축은 H1에서 H4 준비도로 올라간다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-243](../DECISIONS.md#L2666) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-013` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 발산과 수렴은 수치 균형이 아니라 양방향 플레이 인계로 조화를 판정한다 | `WI-NATURE-16`, `WI-NATURE-17` | `ExplicitCanonicalLink` |
| [D-244](../DECISIONS.md#L2676) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-014` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | WI는 한국어 기능명을 먼저 표시하고 안정 고유 식별자는 보조 표기로 유지한다 | `WI-NATURE-07` | `ExplicitCanonicalLink` |
| [D-245](../DECISIONS.md#L2686) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | WI는 한 의도와 하나의 주요 권위 결과만 소유하고 절차는 별도 흐름이 조립한다 | `WI-HUB-04`, `WI-HUB-05`, `WI-LOG-02`, `WI-LOG-03`, `WI-LOG-04`, `WI-LOG-05`, `WI-NATURE-07`, `WI-NATURE-11`, `WI-ORDER-02`, `WI-ORDER-03`, `WI-ORDER-04`, `WI-WORLD-07` | `ExplicitCanonicalLink` |
| [D-246](../DECISIONS.md#L2698) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-016` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | WI 음양 사분면은 행동 목적과 실제 수행 주체를 직교 결합한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-247](../DECISIONS.md#L2710) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-017` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 배치 결과는 플레이어 감각 표현축에서 교차 검증한다 | `WI-NATURE-05`, `WI-NATURE-06` | `ExplicitCanonicalLink` |
| [D-248](../DECISIONS.md#L2722) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-018` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | Codex 장기 Goal은 PlayableUnit 하나를 소유하고 WI WIP 1로 진행한다 | `WI-NATURE-05` | `ExplicitCanonicalLink` |
| [D-249](../DECISIONS.md#L2735) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-019` | `GAMEPLAY` / `WI-LOOP-PROGRESSION` | 거점 성찰은 승인 자료를 읽는 플레이어 선택이며 시청 보상이 아니다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-250](../DECISIONS.md#L2748) | `D-EVIDENCE-PRESENTATION-INTEGRATION-001` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | 플레이 폐루프의 논리와 표현 성숙도를 분리하고 낮은 단계로 통합 판정한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-251](../DECISIONS.md#L2758) | `D-EVIDENCE-PRESENTATION-INTEGRATION-002` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | 표현 E4~E7 승격은 공통 검증 모듈과 기능별 조건 모듈을 통과한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-252](../DECISIONS.md#L2769) | `D-EVIDENCE-PRESENTATION-INTEGRATION-003` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | LH의 3×3·5×5·9×9는 고정 계층이 아니라 기본 동적 맵 창 프로필이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-253](../DECISIONS.md#L2779) | `D-EVIDENCE-PRESENTATION-INTEGRATION-004` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | 동적 셀 활성화 전에 객체의 표면·간격·가시 하단을 별도 관문으로 검증한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-254](../DECISIONS.md#L2791) | `D-EVIDENCE-PRESENTATION-INTEGRATION-005` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | Sky Engine은 세계 공통 대기 권위와 카메라 전역 표현을 잇는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-255](../DECISIONS.md#L2802) | `D-EVIDENCE-PRESENTATION-INTEGRATION-006` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | LH 불규칙 지형은 교체 가능한 표면 상태 사본으로 조립한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-256](../DECISIONS.md#L2814) | `D-EVIDENCE-PRESENTATION-INTEGRATION-007` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | LH는 지면·셀을 준비하고 Sky 뒤 실외·실내 배치엔진이 표현을 조립한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-257](../DECISIONS.md#L2826) | `D-EVIDENCE-PRESENTATION-INTEGRATION-008` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | 엔진 상호작용은 Logic·Presentation을 같은 WI 명령으로 묶는 통합 관문이다 | `WI-NATURE-13`, `WI-NATURE-14`, `WI-NATURE-15` | `ExplicitCanonicalLink` |
| [D-258](../DECISIONS.md#L2837) | `D-EVIDENCE-PRESENTATION-INTEGRATION-009` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | Synty 표현은 A/B/C 완전성이 아니라 PlayableUnit의 플레이 순간으로 모듈화한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-259](../DECISIONS.md#L2849) | `D-EVIDENCE-PRESENTATION-INTEGRATION-010` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | Synty 팩 출처와 게임 기능 모듈을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-260](../DECISIONS.md#L2859) | `D-EVIDENCE-PRESENTATION-INTEGRATION-011` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | PlayableUnit 수직 성숙도는 E7에서 끝나고 E8~E10은 수평 증거로 판정한다 | `WI-NATURE-14` | `ExplicitCanonicalLink` |
| [D-261](../DECISIONS.md#L2871) | `D-EVIDENCE-PRESENTATION-INTEGRATION-012` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | 물품 획득과 장착을 보편 WI로 분리하고 능력은 장착 상태에서 파생한다 | `WI-ACTOR-01`, `WI-ACTOR-02`, `WI-NATURE-05`, `WI-WORLD-06` | `ExplicitCanonicalLink` |
| [D-262](../DECISIONS.md#L2883) | `D-EVIDENCE-PRESENTATION-INTEGRATION-013` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | 자연 방향광은 URP Lit와 표현 검증 기록으로 강화한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-263](../DECISIONS.md#L2895) | `D-EVIDENCE-PRESENTATION-INTEGRATION-014` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | Synty 자산 설계 분류는 한국어를 먼저 쓰고 Stable Code를 보존한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-264](../DECISIONS.md#L2905) | `D-EVIDENCE-PRESENTATION-INTEGRATION-015` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | E1~E9는 판정 주체를 바꾸되 Logic·Presentation 왕복을 유지한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-265](../DECISIONS.md#L2917) | `D-EVIDENCE-PRESENTATION-INTEGRATION-016` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | E8 조화 묶음은 소유 AreaAggregate의 Core 일부를 선택할 수 있다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-266](../DECISIONS.md#L2926) | `D-EVIDENCE-PRESENTATION-INTEGRATION-017` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | E8은 개별 폐루프 안정, E9는 영역 조화와 사람 승인으로 판정한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-267](../DECISIONS.md#L2940) | `D-EVIDENCE-PRESENTATION-INTEGRATION-018` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | 권위 행위 기록은 엔진과 분리하고 분야 성장은 효과 계보에서 파생한다 | `WI-REVIEW-01` | `ExplicitCanonicalLink` |
| [D-268](../DECISIONS.md#L2954) | `D-EVIDENCE-PRESENTATION-INTEGRATION-019` | `EVIDENCE` / `PRESENTATION-INTEGRATION` | 행위 파이프라인은 E Logic·Presentation과 E8~E10의 공통 통합 관문이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-269](../DECISIONS.md#L2965) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-001` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 주제 기획 승인은 새 PlayableLoop Goal보다 앞선다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-270](../DECISIONS.md#L2976) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-002` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 집중 판정은 WI Task에 종속하고 명상은 횡단 성장축으로 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-271](../DECISIONS.md#L2987) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-003` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 모든 플레이어 WI를 집중 Profile로 분류하고 명상 성장은 행위 원장 계보에 결속한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-272](../DECISIONS.md#L2998) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-004` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | Solo를 유지하면서 공식 지속 세계와 서버 권위 비공개 협동방을 분리한다 | `WI-NATURE-06` | `ExplicitCanonicalLink` |
| [D-273](../DECISIONS.md#L3012) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-005` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 전술 시점은 자기 캐릭터 선택·이동과 카메라 탐색 입력을 분리한다 | `WI-NATURE-05` | `ExplicitCanonicalLink` |
| [D-274](../DECISIONS.md#L3025) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-006` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 다섯 영역 위치를 H5 상대좌표로 고정하고 City는 예약 상태로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-275](../DECISIONS.md#L3036) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-007` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 플레이 폐루프 기획과 개발은 승인 기획서를 저장소 인계면으로 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-276](../DECISIONS.md#L3048) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-008` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 구체 설계는 전문 심화 연구로 분기한 뒤 PlayableLoop에 재결속한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-277](../DECISIONS.md#L3060) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-009` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 짧은 정차·대기 문답을 PlayableLoop 기획의 공식 Draft 절차로 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-278](../DECISIONS.md#L3071) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-010` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 명상은 실행 WI가 아닌 비실행 상위 WI군으로 구체 플레이어 행위를 묶는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-279](../DECISIONS.md#L3081) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-011` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | Farm 작업 참여는 초기 Solo 가능성과 후속 협력 성장을 공통 비실행 정책으로 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-280](../DECISIONS.md#L3092) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-012` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | Presentation E4는 적용 가능한 자산·배치 후보를 E5 준비 인계로 남긴다 | `WI-ACTOR-03` | `ExplicitCanonicalLink` |
| [D-281](../DECISIONS.md#L3103) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-013` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 문답 질문은 주제·깊이·전체 번호를 함께 표시하고 깊이별 Evidence 준비 전망을 제공한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-282](../DECISIONS.md#L3115) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-014` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 신규 Synty 환경·애니메이션 팩은 WI·H 표현 원천으로 분리 결속한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-283](../DECISIONS.md#L3127) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-015` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | WI 후보 등록은 상위 분류·특화·결과 투영과 실행 행동을 구별한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-284](../DECISIONS.md#L3138) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-016` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | Goal·WI 고정 WIP 상한을 없애고 실제 의존성과 변경 소유권으로 병렬 개발한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-285](../DECISIONS.md#L3151) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-017` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 전문 작업 결과는 개발에서 통합·검증한 뒤 기획으로 반환한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-286](../DECISIONS.md#L3163) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-018` | `PLANNING` / `GOAL-INQUIRY-HANDOFF` | 기존 WI를 Session·배치·저장에 연결하는 E5 전달을 우선한다 | `WI-ACTOR-03`, `WI-COMMUNITY-VISITOR-STAY`, `WI-FARM-01`, `WI-FARM-02`, `WI-FARM-03`, `WI-FARM-04` | `ExplicitCanonicalLink` |
| [D-287](../DECISIONS.md#L3176) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-001` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 행동 체력은 자연·휴식·물품으로 회복하고 성장에 따라 최대치를 늘린다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-288](../DECISIONS.md#L3187) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-002` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 자연 체력 회복은 걷기·대기 중 허용하고 노동·질주·전투 중 중단한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-289](../DECISIONS.md#L3197) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-003` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 제자리 휴식은 시설 없이 허용하고 안전한 오두막은 회복 효율을 높인다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-290](../DECISIONS.md#L3208) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-004` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 휴식은 이동·작업·피격 시 즉시 중단하고 이미 회복한 체력은 유지한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-291](../DECISIONS.md#L3219) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-005` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 체력 회복 속도는 세 단계로 구분하고 Farm 반복 시험으로 조정한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-292](../DECISIONS.md#L3229) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-006` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 휴식은 버튼 한 번으로 시작하고 유지 입력을 요구하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-293](../DECISIONS.md#L3239) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-007` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 전투 중 휴식 시작을 금지하고 전투 종료 후 다시 허용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-294](../DECISIONS.md#L3249) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-008` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 위협 접근은 오두막 회복 우대만 해제하고 실제 전투·피격은 휴식을 종료한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-295](../DECISIONS.md#L3259) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-009` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 자연 회복은 걷기·대기 조건이 되면 별도 지연 없이 재개한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-296](../DECISIONS.md#L3269) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-010` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 불은 휴식의 필수가 아니라 추가 회복 요소로 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-297](../DECISIONS.md#L3279) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-011` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 야외 모닥불 곁의 휴식에도 불의 추가 회복을 적용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-298](../DECISIONS.md#L3289) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-012` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | View 캡처는 월드·공간·배치 담당이 실행하고 개발이 통합한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-299](../DECISIONS.md#L3298) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-013` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 여러 불의 회복 효과는 합산하지 않고 가장 효과적인 하나만 적용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-300](../DECISIONS.md#L3307) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-014` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 막힌 벽 너머의 열원은 휴식 추가 회복에 적용하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-301](../DECISIONS.md#L3316) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-015` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 휴식 중 불 효과와 벽 차단 이유를 작은 아이콘·짧은 문구로 안내한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-302](../DECISIONS.md#L3325) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-016` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 행동 체력이 가득 차면 휴식을 끝내고 다음 행동은 플레이어가 선택한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-303](../DECISIONS.md#L3334) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-017` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 포션 한 개를 소비하면 행동 체력을 즉시 일정량 회복한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-304](../DECISIONS.md#L3343) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-018` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 행동 체력 포션은 전투 중 사용을 허용하고 재사용 대기를 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-305](../DECISIONS.md#L3352) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-019` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 체력이 가득 차면 포션 소비를 막고 부족할 때만 최대치까지 회복한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-306](../DECISIONS.md#L3361) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-020` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 포션 획득은 탐험·제작·거래의 선택 경로이며 고정 순서를 강제하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-307](../DECISIONS.md#L3370) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-021` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 선택한 실제 활동으로 행동 체력이 성장하며 활동 다양성을 의무화하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-308](../DECISIONS.md#L3379) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-022` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 성장으로 최대 체력이 늘어나면 현재 체력도 증가분만큼 보충한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-309](../DECISIONS.md#L3388) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-023` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 활동 경험에 따른 최대치 자동 성장에 체력과 마나를 포함한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-310](../DECISIONS.md#L3397) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-024` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 마나 최대치는 명상과 집중한 일상 행동으로도 성장한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-311](../DECISIONS.md#L3406) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-025` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 마나 성장 시 최대치 증가분만 현재 마나에 보충한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-312](../DECISIONS.md#L3415) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-026` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 마나는 자연 회복되고 명상 중에는 더 빠르게 회복된다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-313](../DECISIONS.md#L3424) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-027` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 전투 중 마나 자연 회복은 유지하고 명상 회복은 제한한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-314](../DECISIONS.md#L3433) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-028` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 명상은 버튼 한 번으로 시작하고 행동·피격으로 종료한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-315](../DECISIONS.md#L3442) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-029` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 마나가 가득 차도 명상은 유지하고 짧게 알린다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-316](../DECISIONS.md#L3451) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-030` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 명상 중 같은 버튼으로 명상을 끝낸다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-317](../DECISIONS.md#L3460) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-031` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 회복 상태는 작게 표시하고 속도와 효과는 상세에서 보여준다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-318](../DECISIONS.md#L3469) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-032` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 행동 자원이 부족하면 부족량과 회복 방법을 안내한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-319](../DECISIONS.md#L3478) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-033` | `GAMEPLAY` / `PLAYER-RECOVERY-RESOURCES` | 행동 체력 자연 회복을 독립된 첫 실행 범위로 승인한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-320](../DECISIONS.md#L3487) | `D-GAMEPLAY-CONSTRUCTION-CANCEL-001` | `GAMEPLAY` / `CONSTRUCTION-CANCEL` | Farm 건물 실측 크기를 수용하도록 부지를 넓힌다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-321](../DECISIONS.md#L3496) | `D-GAMEPLAY-CONSTRUCTION-CANCEL-002` | `GAMEPLAY` / `CONSTRUCTION-CANCEL` | 건설 취소 재료 반환을 난이도별로 구분한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-322](../DECISIONS.md#L3506) | `D-GAMEPLAY-CONSTRUCTION-CANCEL-003` | `GAMEPLAY` / `CONSTRUCTION-CANCEL` | 노말 건설 취소는 약85% 재료 반환 방향으로 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-323](../DECISIONS.md#L3515) | `D-GAMEPLAY-CONSTRUCTION-CANCEL-004` | `GAMEPLAY` / `CONSTRUCTION-CANCEL` | 미사용 재료와 실제 시공 사용분의 취소 반환을 구분한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-324](../DECISIONS.md#L3524) | `D-GAMEPLAY-CONSTRUCTION-CANCEL-005` | `GAMEPLAY` / `CONSTRUCTION-CANCEL` | 건설 취소 확정 전에 보존·회수·손실 재료를 나눠 안내한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-325](../DECISIONS.md#L3533) | `D-PLANNING-INQUIRY-SEARCH-001` | `PLANNING` / `INQUIRY-SEARCH` | 문답 원문을 보존하고 파일 기반 검색 색인으로 재개한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-326](../DECISIONS.md#L3542) | `D-GAMEPLAY-HERBAL-TEA-001` | `GAMEPLAY` / `HERBAL-TEA` | 첫 약초차 반복은 기존 냄비로 물을 운반하고 컵으로 마신다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-327](../DECISIONS.md#L3551) | `D-GAMEPLAY-HERBAL-TEA-002` | `GAMEPLAY` / `HERBAL-TEA` | 물 확보량은 고정 제작 1회분이 아니라 용기별 용량을 따른다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-328](../DECISIONS.md#L3560) | `D-GAMEPLAY-HERBAL-TEA-003` | `GAMEPLAY` / `HERBAL-TEA` | 물 보관 가능 여부와 용량을 구분한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-329](../DECISIONS.md#L3569) | `D-GAMEPLAY-HERBAL-TEA-004` | `GAMEPLAY` / `HERBAL-TEA` | 달이기 중 다른 행동을 허용하고 약초 포션의 수요 공급을 후속 확장한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-330](../DECISIONS.md#L3578) | `D-GAMEPLAY-HERBAL-TEA-005` | `GAMEPLAY` / `HERBAL-TEA` | 자기 돌봄이 타인의 이로움으로 확장되는 자리이타를 기획 방향으로 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-331](../DECISIONS.md#L3587) | `D-GAMEPLAY-HERBAL-TEA-006` | `GAMEPLAY` / `HERBAL-TEA` | 처방별 연속 가열·돌봄 차이를 후반 전문 제작으로 확장한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-332](../DECISIONS.md#L3596) | `D-GAMEPLAY-HERBAL-TEA-007` | `GAMEPLAY` / `HERBAL-TEA` | 첫 약초차의 따뜻함 감정·날숨 표현과 자유 행동을 유지한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-333](../DECISIONS.md#L3605) | `D-GAMEPLAY-HERBAL-TEA-008` | `GAMEPLAY` / `HERBAL-TEA` | 약초차·탕에 마법 술식을 추가하는 물약 제작을 구분한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-334](../DECISIONS.md#L3614) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-001` | `GAMEPLAY` / `IDEA-NPC-INQUIRY` | 관심·명상에서 얻는 이데아의 편린을 NPC 인연 가능성과 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-335](../DECISIONS.md#L3623) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-002` | `GAMEPLAY` / `IDEA-NPC-INQUIRY` | 상태창형 UI에서 관심 분야 카드를 선택한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-336](../DECISIONS.md#L3632) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-003` | `GAMEPLAY` / `IDEA-NPC-INQUIRY` | 관심·행동 기록을 근거로 방문 NPC 후보를 결정한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-337](../DECISIONS.md#L3641) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-004` | `GAMEPLAY` / `IDEA-NPC-INQUIRY` | 호출당 외부 비용 없는 로컬 LLM/RAG 대화 지원 기반 설치를 승인한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-338](../DECISIONS.md#L3650) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-005` | `GAMEPLAY` / `IDEA-NPC-INQUIRY` | 방문 NPC는 분야 화두를 꺼내고 관련 거래 기회를 제공한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-339](../DECISIONS.md#L3659) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-006` | `GAMEPLAY` / `IDEA-NPC-INQUIRY` | 기획 답변을 기록한 뒤 다음 핵심 질문을 이어서 제시한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-340](../DECISIONS.md#L3668) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-001` | `GAMEPLAY` / `CREDIT-MULTIPLAYER` | NPC 물품 외상을 도시 은행·계좌 상환 경로와 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-341](../DECISIONS.md#L3677) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-002` | `GAMEPLAY` / `CREDIT-MULTIPLAYER` | 연체 영향을 유예·계좌 정지·신용·상단 인지로 단계화한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-342](../DECISIONS.md#L3686) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-003` | `GAMEPLAY` / `CREDIT-MULTIPLAYER` | 계좌 거래 정지 중에도 입금·채무 상환을 허용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-343](../DECISIONS.md#L3695) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-004` | `GAMEPLAY` / `CREDIT-MULTIPLAYER` | 연체 해소 후 계좌 제한을 풀고 신용은 점진 회복한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-344](../DECISIONS.md#L3704) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-005` | `GAMEPLAY` / `CREDIT-MULTIPLAYER` | 첫 약초차는 불 꺼짐 중 재료·진척을 보존한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-345](../DECISIONS.md#L3712) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-006` | `GAMEPLAY` / `CREDIT-MULTIPLAYER` | 후반 개척 지역의 선택형 멀티플레이에 거래 신용을 연계한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-346](../DECISIONS.md#L3720) | `D-GAMEPLAY-HERBAL-CONTENT-001` | `GAMEPLAY` / `HERBAL-CONTENT` | 첫 약초차의 체온 회복과 질병 관련 효과를 시간적으로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-347](../DECISIONS.md#L3729) | `D-GAMEPLAY-HERBAL-CONTENT-002` | `GAMEPLAY` / `HERBAL-CONTENT` | 같은 첫 약초차의 지속 효과는 중첩하지 않고 시간을 갱신한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-348](../DECISIONS.md#L3738) | `D-GAMEPLAY-HERBAL-CONTENT-003` | `GAMEPLAY` / `HERBAL-CONTENT` | 식은 첫 약초차의 질병 효과를 유지하고 재가열을 허용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-349](../DECISIONS.md#L3747) | `D-GAMEPLAY-HERBAL-CONTENT-004` | `GAMEPLAY` / `HERBAL-CONTENT` | 마개 달린 휴대 용기에 약초차를 담아 탐험에 가져간다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-350](../DECISIONS.md#L3756) | `D-GAMEPLAY-HERBAL-CONTENT-005` | `GAMEPLAY` / `HERBAL-CONTENT` | 약초차 음용 후 빈 휴대 용기를 유지해 재사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-351](../DECISIONS.md#L3765) | `D-GAMEPLAY-HERBAL-CONTENT-006` | `GAMEPLAY` / `HERBAL-CONTENT` | 다른 종류의 차로 교체하기 전에 기존 내용물을 비운다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-352](../DECISIONS.md#L3774) | `D-GAMEPLAY-HERBAL-CONTENT-007` | `GAMEPLAY` / `HERBAL-CONTENT` | 한 번 음용할 때 1회분만 소비하고 나머지를 보존한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-353](../DECISIONS.md#L3783) | `D-GAMEPLAY-HERBAL-CONTENT-008` | `GAMEPLAY` / `HERBAL-CONTENT` | 질문 약 10개와 추천 답안을 묶어 검토·승인한 뒤 개발에 인계한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-354](../DECISIONS.md#L3793) | `D-GAMEPLAY-HERBAL-CONTENT-009` | `GAMEPLAY` / `HERBAL-CONTENT` | 보편 데우기 WI 아래 물·식은 차 데우기를 적용 사례로 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-355](../DECISIONS.md#L3802) | `D-GAMEPLAY-HERBAL-CONTENT-010` | `GAMEPLAY` / `HERBAL-CONTENT` | 기존 문답과 WI를 보편 행위·적용 사례·조합 관계로 정리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-356](../DECISIONS.md#L3811) | `D-GAMEPLAY-HERBAL-CONTENT-011` | `GAMEPLAY` / `HERBAL-CONTENT` | HB-01의 Q368~377 추천안을 전체 승인한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-357](../DECISIONS.md#L3819) | `D-GAMEPLAY-FARM-DELEGATION-001` | `GAMEPLAY` / `FARM-DELEGATION` | FB-01 농사 생활·위임의 수정 답변과 나머지 추천안을 승인한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-358](../DECISIONS.md#L3827) | `D-PRESENTATION-ANIMATION-WORKFLOW-001` | `PRESENTATION` / `ANIMATION-WORKFLOW` | Blender 제작은 애니메이션 전문 담당, 플레이 기획은 기획 스레드에 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-359](../DECISIONS.md#L3836) | `D-PRESENTATION-ANIMATION-WORKFLOW-002` | `PRESENTATION` / `ANIMATION-WORKFLOW` | 구매한 Synty 캐릭터의 도끼 벌목 동작 하나를 제작한다 | `WI-NATURE-06` | `ExplicitCanonicalLink` |
| [D-360](../DECISIONS.md#L3843) | `D-PRESENTATION-ANIMATION-WORKFLOW-003` | `PRESENTATION` / `ANIMATION-WORKFLOW` | Nature 기초 폐루프의 기획 문서 연결을 복원한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-361](../DECISIONS.md#L3849) | `D-PRESENTATION-ANIMATION-WORKFLOW-004` | `PRESENTATION` / `ANIMATION-WORKFLOW` | 기존 WI와 보유 애니메이션을 대조해 부족한 표현부터 제작한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-362](../DECISIONS.md#L3855) | `D-WORLD-LANDSCAPE-PLACEMENT-LH-001` | `WORLD` / `LANDSCAPE-PLACEMENT-LH` | LS-01 경관 구성 Q387~396 추천안을 전체 채택한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-363](../DECISIONS.md#L3862) | `D-WORLD-LANDSCAPE-PLACEMENT-LH-002` | `WORLD` / `LANDSCAPE-PLACEMENT-LH` | 배치 엔진 고도화를 우선하고 LH는 공간 실행을 지원한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-364](../DECISIONS.md#L3869) | `D-WORLD-LANDSCAPE-PLACEMENT-LH-003` | `WORLD` / `LANDSCAPE-PLACEMENT-LH` | Simulation·배치·LH를 하나의 공간 실행 파이프라인에서 조율한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-365](../DECISIONS.md#L3877) | `D-GAMEPLAY-FOCUS-RESEARCH-001` | `GAMEPLAY` / `FOCUS-RESEARCH` | 첫 농사 목표 시간과 선택형 집중 타이밍 | 미명시 | `NoExplicitCanonicalLink` |
| [D-366](../DECISIONS.md#L3884) | `D-GAMEPLAY-FOCUS-RESEARCH-002` | `GAMEPLAY` / `FOCUS-RESEARCH` | 선택형 집중 실패 무손실과 자료 조사 전문 담당 | 미명시 | `NoExplicitCanonicalLink` |
| [D-367](../DECISIONS.md#L3890) | `D-GAMEPLAY-TRADE-REALITY-001` | `GAMEPLAY` / `TRADE-REALITY` | 기존 가격 자료의 해석을 통한 게임 교역 기회 | 미명시 | `NoExplicitCanonicalLink` |
| [D-368](../DECISIONS.md#L3897) | `D-GAMEPLAY-TRADE-REALITY-002` | `GAMEPLAY` / `TRADE-REALITY` | 교역 위험을 상단 보험과 직접 경로 개입으로 다룬다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-369](../DECISIONS.md#L3904) | `D-GAMEPLAY-TRADE-REALITY-003` | `GAMEPLAY` / `TRADE-REALITY` | 첫 보험 보장 범위와 현실에 닿는 운영 경험 | 미명시 | `NoExplicitCanonicalLink` |
| [D-370](../DECISIONS.md#L3911) | `D-GAMEPLAY-TRADE-REALITY-004` | `GAMEPLAY` / `TRADE-REALITY` | 작업 결과는 짧게 요약하고 상세는 선택해서 펼친다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-371](../DECISIONS.md#L3917) | `D-GAMEPLAY-TRADE-REALITY-005` | `GAMEPLAY` / `TRADE-REALITY` | 현실 자료에 연결된 게임 경험을 시장 통찰로 확장한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-372](../DECISIONS.md#L3923) | `D-GAMEPLAY-TRADE-REALITY-006` | `GAMEPLAY` / `TRADE-REALITY` | 게임의 재미를 우선하고 현실 자료는 선택해서 열람한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-373](../DECISIONS.md#L3929) | `D-GAMEPLAY-TRADE-REALITY-007` | `GAMEPLAY` / `TRADE-REALITY` | 현실 자료 열람은 상품·거래 결과 상세에서 진입한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-374](../DECISIONS.md#L3935) | `D-GAMEPLAY-TRADE-REALITY-008` | `GAMEPLAY` / `TRADE-REALITY` | 게임 상품을 먼저 만들고 현실 자료의 제공은 운영자가 검토한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-375](../DECISIONS.md#L3941) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-001` | `OPERATIONS` / `OVERNIGHT-VISUAL-DEV` | 승인된 개발을 야간 반복 검증으로 이어간다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-376](../DECISIONS.md#L3947) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-002` | `OPERATIONS` / `OVERNIGHT-VISUAL-DEV` | 야간 보완에 월드 경계와 배경 연속성을 포함한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-377](../DECISIONS.md#L3953) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-003` | `OPERATIONS` / `OVERNIGHT-VISUAL-DEV` | 전체 문답을 E5 세계 통합 Queue로 정리해 가능한 묶음을 완성한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-378](../DECISIONS.md#L3960) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-004` | `OPERATIONS` / `OVERNIGHT-VISUAL-DEV` | 화면의 부유 흰색 객체와 네모난 면 겹침을 정리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-379](../DECISIONS.md#L3965) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-005` | `OPERATIONS` / `OVERNIGHT-VISUAL-DEV` | 도끼 접촉 타격음과 완료 후 나무 넘어짐을 보완한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-380](../DECISIONS.md#L3971) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-006` | `OPERATIONS` / `OVERNIGHT-VISUAL-DEV` | 보유 자산과 확정 기획을 대조해 애니메이션·리깅 정밀화 계획을 먼저 만든다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-381](../DECISIONS.md#L3977) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-007` | `OPERATIONS` / `OVERNIGHT-VISUAL-DEV` | 벌목 개발이 장기 정체되면 농장·약초의 준비된 플레이 구간으로 전환한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-382](../DECISIONS.md#L3983) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-008` | `OPERATIONS` / `OVERNIGHT-VISUAL-DEV` | 야간 목표를 여러 분야의 시각 진척 비교로 넓힌다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-383](../DECISIONS.md#L3989) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-009` | `OPERATIONS` / `OVERNIGHT-VISUAL-DEV` | 애니메이션 담당을 동작 품질과 시각 성과 중심으로 운영한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-384](../DECISIONS.md#L3995) | `D-PLANNING-PROJECT-IDENTITY-001` | `PLANNING` / `PROJECT-IDENTITY` | 프로젝트 표시명과 GitHub 저장소를 Mirror거울로 변경한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-385](../DECISIONS.md#L4003) | `D-PRESENTATION-HERBAL-PROP-001` | `PRESENTATION` / `HERBAL-PROP` | 좁은 목의 병 대신 조리 냄비로 읽히는 보유 Prefab을 적용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-386](../DECISIONS.md#L4009) | `D-EVIDENCE-PRESENTATION-E4-E5-001` | `EVIDENCE` / `PRESENTATION-E4-E5` | Presentation E단계에 최소 공통 모듈과 실제 구현·증거를 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-387](../DECISIONS.md#L4017) | `D-EVIDENCE-PRESENTATION-E4-E5-002` | `EVIDENCE` / `PRESENTATION-E4-E5` | 보유 Synty 자산 조사를 Presentation E4의 명시적 준비 과정으로 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-388](../DECISIONS.md#L4024) | `D-EVIDENCE-PRESENTATION-E4-E5-003` | `EVIDENCE` / `PRESENTATION-E4-E5` | 논리가 선행한 기존 기능에도 자산 조사와 표현 준비를 적용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-389](../DECISIONS.md#L4031) | `D-EVIDENCE-PRESENTATION-E4-E5-004` | `EVIDENCE` / `PRESENTATION-E4-E5` | Presentation E4→E5의 연결 사전검사와 인계를 공통화한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-390](../DECISIONS.md#L4038) | `D-EVIDENCE-PRESENTATION-E4-E5-005` | `EVIDENCE` / `PRESENTATION-E4-E5` | 전체 배치 기준과 개별 E5 성립·후속 조화의 책임을 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-391](../DECISIONS.md#L4046) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-001` | `PLANNING` / `PLAYER-CENTERED-INQUIRY` | 시간·공간·플레이어·대상 관점으로 기존 기획과 WI를 정리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-392](../DECISIONS.md#L4054) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-002` | `PLANNING` / `PLAYER-CENTERED-INQUIRY` | 네 관점과 WI 순환으로 전체 기획 이관을 우선한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-393](../DECISIONS.md#L4062) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-003` | `PLANNING` / `PLAYER-CENTERED-INQUIRY` | 네 관점에 Sky·LH/배치·플레이어 상태·대상 시스템을 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-394](../DECISIONS.md#L4069) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-004` | `PLANNING` / `PLAYER-CENTERED-INQUIRY` | 기획의 공통 안내 말을 지금·여기·나·너·이렇게로 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-395](../DECISIONS.md#L4076) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-005` | `PLANNING` / `PLAYER-CENTERED-INQUIRY` | 같은 기획 패턴을 문서·코드·자산·Unity 검증과 결과 기록으로 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-396](../DECISIONS.md#L4084) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-006` | `PLANNING` / `PLAYER-CENTERED-INQUIRY` | WI 전체를 중심으로 E4까지 문서·코드·Synty 준비를 진행한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-397](../DECISIONS.md#L4092) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-007` | `PLANNING` / `PLAYER-CENTERED-INQUIRY` | 실제 문답도 지금·여기·나·너·이렇게의 상황과 선택으로 이어간다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-398](../DECISIONS.md#L4099) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-008` | `PLANNING` / `PLAYER-CENTERED-INQUIRY` | 한 회차에 질문 하나로 상황과 선택을 깊게 탐구한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-399](../DECISIONS.md#L4106) | `D-GAMEPLAY-SEASON-TECH-TREE-001` | `GAMEPLAY` / `SEASON-TECH-TREE` | 시간성을 절기 중심으로 읽고 농사·계절 상품·물류에 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-400](../DECISIONS.md#L4113) | `D-GAMEPLAY-SEASON-TECH-TREE-002` | `GAMEPLAY` / `SEASON-TECH-TREE` | 계절의 야생 변화와 재배 시설 대응을 기획에 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-401](../DECISIONS.md#L4120) | `D-GAMEPLAY-SEASON-TECH-TREE-003` | `GAMEPLAY` / `SEASON-TECH-TREE` | 재배 추위 대응을 난방과 마법진까지 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-402](../DECISIONS.md#L4127) | `D-GAMEPLAY-SEASON-TECH-TREE-004` | `GAMEPLAY` / `SEASON-TECH-TREE` | 발전 수단을 기능 군집과 테크트리로 관리하고 정보를 점진 공개한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-403](../DECISIONS.md#L4134) | `D-GAMEPLAY-SEASON-TECH-TREE-005` | `GAMEPLAY` / `SEASON-TECH-TREE` | 한국 24절기를 기준으로 농수산물 제철 자료를 조사·연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-404](../DECISIONS.md#L4142) | `D-GAMEPLAY-SEASON-TECH-TREE-006` | `GAMEPLAY` / `SEASON-TECH-TREE` | 자료조사에서 로그인 등 접근 조치가 필요하면 먼저 보고한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-405](../DECISIONS.md#L4149) | `D-GAMEPLAY-SEASON-TECH-TREE-007` | `GAMEPLAY` / `SEASON-TECH-TREE` | 절기별 산·숲 경관을 월드맵·배치·LH 협력으로 표현한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-406](../DECISIONS.md#L4157) | `D-WORLD-WORLDMAP-PROPOSAL-001` | `WORLD` / `WORLDMAP-PROPOSAL` | 초기 산재 표시를 정리하고 월드맵 기반·실제 보행을 우선한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-407](../DECISIONS.md#L4165) | `D-WORLD-WORLDMAP-PROPOSAL-002` | `WORLD` / `WORLDMAP-PROPOSAL` | 네 업무영역을 지형지물로 구분하는 월드맵 제안을 작성한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-408](../DECISIONS.md#L4173) | `D-STORY-FIRST-DISCOVERY-001` | `STORY` / `FIRST-DISCOVERY` | 발견형 구도를 첫 인과 기록으로 남기고 기존 문답을 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-409](../DECISIONS.md#L4181) | `D-STORY-FIRST-DISCOVERY-002` | `STORY` / `FIRST-DISCOVERY` | 날씨별 발견 난도를 확정하고 전투 활용은 후속으로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-410](../DECISIONS.md#L4188) | `D-STORY-FIRST-DISCOVERY-003` | `STORY` / `FIRST-DISCOVERY` | 잔여 문답을 현재 관점으로 전량 대조하고 리팩토링한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-411](../DECISIONS.md#L4196) | `D-STORY-FIRST-DISCOVERY-004` | `STORY` / `FIRST-DISCOVERY` | 숲 가장자리와 농장 외곽부터 플레이 경험을 구체화한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-412](../DECISIONS.md#L4204) | `D-STORY-FIRST-DISCOVERY-005` | `STORY` / `FIRST-DISCOVERY` | 춘분을 기획 기준으로 삼고 보유 경관 자산을 먼저 조사한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-413](../DECISIONS.md#L4212) | `D-STORY-FIRST-DISCOVERY-006` | `STORY` / `FIRST-DISCOVERY` | 춘분 무렵의 약초와 농작물 자료를 경관 기획과 병행 조사한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-414](../DECISIONS.md#L4220) | `D-STORY-FIRST-DISCOVERY-007` | `STORY` / `FIRST-DISCOVERY` | 북부 춘분의 굶주린 농장 발견과 상호 도움·눈과 침엽수 배경을 기획한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-415](../DECISIONS.md#L4229) | `D-STORY-FIRST-DISCOVERY-008` | `STORY` / `FIRST-DISCOVERY` | 눈 없는 춘분의 봄으로 전환하고 부담 없는 식사 협력과 가방 필요를 반영한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-416](../DECISIONS.md#L4239) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-001` | `GAMEPLAY` / `PERSPECTIVE-FOCUS` | 여러 영역의 선택형 플레이를 함께 설계하고 승인된 개발을 병행한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-417](../DECISIONS.md#L4248) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-002` | `GAMEPLAY` / `PERSPECTIVE-FOCUS` | 직접 탐험의 1인칭과 넓은 운영 시점·현실 업무 보조를 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-418](../DECISIONS.md#L4257) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-003` | `GAMEPLAY` / `PERSPECTIVE-FOCUS` | WI를 시점의 스케일별 공통·특화·전용으로 구분하고 독립 발전을 관리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-419](../DECISIONS.md#L4266) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-004` | `GAMEPLAY` / `PERSPECTIVE-FOCUS` | 1인칭 직접 타이밍 수행을 선택형 추가 효과에 특화한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-420](../DECISIONS.md#L4272) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-005` | `GAMEPLAY` / `PERSPECTIVE-FOCUS` | 집중 타이밍은 플레이어가 선택한 작업에서만 제시한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-421](../DECISIONS.md#L4278) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-006` | `GAMEPLAY` / `PERSPECTIVE-FOCUS` | 반복 동작마다 집중에 재도전하고 성공으로 작업 효율을 높인다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-426](../DECISIONS.md#L4286) | `D-DATA-GAME-OBJECT-ASSET-DB-001` | `DATA` / `GAME-OBJECT-ASSET-DB` | 개체 종류와 개별 기록의 시각 자산 대응을 여러 분야에 구현한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-427](../DECISIONS.md#L4295) | `D-DATA-GAME-OBJECT-ASSET-DB-002` | `DATA` / `GAME-OBJECT-ASSET-DB` | 농장 건물과 밭은 이격·정렬하고 상점 진열대는 실내에 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-428](../DECISIONS.md#L4302) | `D-DATA-GAME-OBJECT-ASSET-DB-003` | `DATA` / `GAME-OBJECT-ASSET-DB` | NPC 양조 공방에서 배우고 국내 현실 양조장 자료를 선택형으로 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-429](../DECISIONS.md#L4310) | `D-DATA-GAME-OBJECT-ASSET-DB-004` | `DATA` / `GAME-OBJECT-ASSET-DB` | 공공자료 조사는 기존 서버 수집과 Docker MySQL 축적까지를 기본으로 한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-430](../DECISIONS.md#L4318) | `D-DATA-GAME-OBJECT-ASSET-DB-005` | `DATA` / `GAME-OBJECT-ASSET-DB` | 기존 DbSet 개체·레코드와 KAMIS 코드의 실제 시각 대응을 먼저 재검증한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-431](../DECISIONS.md#L4325) | `D-DATA-GAME-OBJECT-ASSET-DB-006` | `DATA` / `GAME-OBJECT-ASSET-DB` | Synty 자산 목록과 개체 대응 관계를 MySQL에 먼저 구축한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-432](../DECISIONS.md#L4332) | `D-DATA-GAME-OBJECT-ASSET-DB-007` | `DATA` / `GAME-OBJECT-ASSET-DB` | 개체별 자산 할당을 실제 이미지로 비교·검토할 수 있게 한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-433](../DECISIONS.md#L4339) | `D-DATA-GAME-OBJECT-ASSET-DB-008` | `DATA` / `GAME-OBJECT-ASSET-DB` | 프리팹 이미지 전담 조사를 만들고 Azure 비공개 보관·웹 열람으로 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-434](../DECISIONS.md#L4347) | `D-DATA-GAME-OBJECT-ASSET-DB-009` | `DATA` / `GAME-OBJECT-ASSET-DB` | 게임 객체를 역할별 여러 자산으로 구성하여 MySQL 관계로 관리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-435](../DECISIONS.md#L4354) | `D-DATA-GAME-OBJECT-ASSET-DB-010` | `DATA` / `GAME-OBJECT-ASSET-DB` | WI의 플레이 관점에서 게임 객체를 추출하여 기존 MySQL 정의와 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-436](../DECISIONS.md#L4361) | `D-DATA-GAME-OBJECT-ASSET-DB-011` | `DATA` / `GAME-OBJECT-ASSET-DB` | 새 문답 확대보다 기존 요청을 사용자 확인 가능한 결과로 우선 마무리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-437](../DECISIONS.md#L4368) | `D-DATA-GAME-OBJECT-ASSET-DB-012` | `DATA` / `GAME-OBJECT-ASSET-DB` | 보유 Synty 팩 전체의 자산 목록을 조사하여 MySQL에 축적한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-438](../DECISIONS.md#L4375) | `D-DATA-GAME-OBJECT-ASSET-DB-013` | `DATA` / `GAME-OBJECT-ASSET-DB` | WI 전수 객체 조사를 쉬고 있는 자료 담당에 분담하고 개발이 DB 등록을 통합한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-439](../DECISIONS.md#L4382) | `D-DATA-GAME-OBJECT-ASSET-DB-014` | `DATA` / `GAME-OBJECT-ASSET-DB` | 표준 기획 문서의 판단을 로컬 저장 기능으로 MySQL 관계에 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-440](../DECISIONS.md#L4389) | `D-DATA-GAME-OBJECT-ASSET-DB-015` | `DATA` / `GAME-OBJECT-ASSET-DB` | 근거가 충분한 자산을 Codex가 역할별 자동 할당하고 개발 작업이 구현한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-441](../DECISIONS.md#L4397) | `D-DATA-GAME-OBJECT-ASSET-DB-016` | `DATA` / `GAME-OBJECT-ASSET-DB` | 기존 Synty 기능 분류를 재사용하여 DB 검색과 자동 할당을 보완한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-442](../DECISIONS.md#L4404) | `D-WORLD-GRAPH-MAP-E6-001` | `WORLD` / `GRAPH-MAP-E6` | 실제 월드 배치 전에 패턴 조합과 배치 규칙을 정밀화하고 격리 미리보기로 검토한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-443](../DECISIONS.md#L4413) | `D-WORLD-GRAPH-MAP-E6-002` | `WORLD` / `GRAPH-MAP-E6` | 기존 노드·엣지 구조를 재사용해 패턴 관계와 연결 의도를 검사한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-444](../DECISIONS.md#L4421) | `D-WORLD-GRAPH-MAP-E6-003` | `WORLD` / `GRAPH-MAP-E6` | 그래프 맵을 기획서의 지금·여기·나·너·이렇게와 연결하여 검토한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-445](../DECISIONS.md#L4427) | `D-WORLD-GRAPH-MAP-E6-004` | `WORLD` / `GRAPH-MAP-E6` | 그래프 맵의 플레이 관계와 세부 배치 규칙을 두 상세 수준으로 문서화한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-446](../DECISIONS.md#L4434) | `D-WORLD-GRAPH-MAP-E6-005` | `WORLD` / `GRAPH-MAP-E6` | 그래프 맵 기반 정밀화를 E5 준비·입증의 공식 주력 절차로 채택한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-447](../DECISIONS.md#L4442) | `D-WORLD-GRAPH-MAP-E6-006` | `WORLD` / `GRAPH-MAP-E6` | 공식 준비·입증 절차를 E6 정제와 필요한 현실 근거 결속까지 확장한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-448](../DECISIONS.md#L4450) | `D-OPERATIONS-GIT-COMMIT-001` | `OPERATIONS` / `GIT-COMMIT` | 누적 변경을 정합화하고 소유가 확인된 맥락별로 커밋한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-449](../DECISIONS.md#L4457) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-001` | `GAMEPLAY` / `HUB-REALITY-LOGISTICS` | 허브를 발견하는 3인칭 도입과 광역 노드 관찰을 기획한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-450](../DECISIONS.md#L4465) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-002` | `GAMEPLAY` / `HUB-REALITY-LOGISTICS` | 허브를 데이터 통합과 공통 개발 검증의 중심 사례로 활용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-451](../DECISIONS.md#L4473) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-003` | `GAMEPLAY` / `HUB-REALITY-LOGISTICS` | 허브를 화물차·기사 NPC가 오가는 물류센터로 표현하고 창고 상세를 두 시점에서 함께 읽는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-452](../DECISIONS.md#L4480) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-004` | `GAMEPLAY` / `HUB-REALITY-LOGISTICS` | 현실 물류 자료를 게임용 허브 물류 사본으로 변환하고 선택형 상세에서 대응 근거를 보여준다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-453](../DECISIONS.md#L4488) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-005` | `GAMEPLAY` / `HUB-REALITY-LOGISTICS` | 첫 현실 연계 물류 신호는 입출고 추세·불균형·품목군 비중의 상대 단계로 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-454](../DECISIONS.md#L4495) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-006` | `GAMEPLAY` / `HUB-REALITY-LOGISTICS` | 플레이어가 접촉한 게임 데이터의 현실 대응 보고서를 선택적으로 생성·통지할 수 있게 한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-455](../DECISIONS.md#L4503) | `D-INTERACTION-QUEST-001` | `INTERACTION` / `QUEST` | 허브 설명과 도움 요청은 물음표·느낌표 표시를 사용자가 선택할 때만 연다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-456](../DECISIONS.md#L4510) | `D-INTERACTION-QUEST-002` | `INTERACTION` / `QUEST` | 물음표는 설명, 느낌표는 도움 요청으로 구분하고 함께 있으면 통합 표시를 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-457](../DECISIONS.md#L4517) | `D-INTERACTION-QUEST-003` | `INTERACTION` / `QUEST` | 색과 보조표식이 다른 물음표로 정보·퀘스트 획득을 구분하고 느낌표는 완료 확인에 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-458](../DECISIONS.md#L4524) | `D-INTERACTION-QUEST-004` | `INTERACTION` / `QUEST` | 퀘스트 정산 뒤 World 완료 표시는 제거하고 기록과 실제 후속 의뢰 표시를 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-459](../DECISIONS.md#L4531) | `D-INTERACTION-QUEST-005` | `INTERACTION` / `QUEST` | 의미 있는 금색 의뢰 완료는 개인 회복 기여를 만들고 검증된 공동체 기여로 집계한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-460](../DECISIONS.md#L4539) | `D-INTERACTION-QUEST-006` | `INTERACTION` / `QUEST` | 개인 회복은 공동체 회복의 기본층에 반영하고 공공 문제 해결은 더 강한 기여층으로 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-461](../DECISIONS.md#L4547) | `D-INTERACTION-QUEST-007` | `INTERACTION` / `QUEST` | 공동체 회복 기본층은 실제 소속·체류·관측 구성원만 집계한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-462](../DECISIONS.md#L4554) | `D-INTERACTION-QUEST-008` | `INTERACTION` / `QUEST` | 파란 물음표는 일반 퀘스트, 금색 물음표는 메인 퀘스트로 두고 일반 경험으로 메인을 준비한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-463](../DECISIONS.md#L4562) | `D-INTERACTION-QUEST-009` | `INTERACTION` / `QUEST` | 퀘스트에서 게임 문제 해결과 대응 현실 자료 이해를 함께 제공한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-464](../DECISIONS.md#L4570) | `D-INTERACTION-QUEST-010` | `INTERACTION` / `QUEST` | 동적 퀘스트는 그래프 호환 뼈대를 남기고 현재 퀘스트 문답을 마감해 개발에 인계한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-465](../DECISIONS.md#L4578) | `D-PLANNING-DISCOVERY-PLAN-001` | `PLANNING` / `DISCOVERY-PLAN` | 발견 사실은 자동 기록하고 계획 채택은 플레이어가 직접 선택한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-466](../DECISIONS.md#L4586) | `D-PLANNING-DISCOVERY-PLAN-002` | `PLANNING` / `DISCOVERY-PLAN` | 현재 실행 가능한 발견만 계획 후보로 추천한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-467](../DECISIONS.md#L4594) | `D-PLANNING-DISCOVERY-PLAN-003` | `PLANNING` / `DISCOVERY-PLAN` | 활성 계획과 가까운 실행 가능 후보를 먼저 보여준다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-468](../DECISIONS.md#L4602) | `D-PLANNING-DISCOVERY-PLAN-004` | `PLANNING` / `DISCOVERY-PLAN` | 관심 분야는 관련 이데아와 실행 기회를 더 쉽게 알아차리게 한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-469](../DECISIONS.md#L4610) | `D-EVIDENCE-E5-CONTEXT-001` | `EVIDENCE` / `E5-CONTEXT` | E5는 대상 WI에 적용되는 E1~E4 전체 맥락을 소비해 실제 결속한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-470](../DECISIONS.md#L4618) | `D-STORY-MAIN-STORY-001` | `STORY` / `MAIN-STORY` | 메인 스토리는 지금·여기·나·너·이렇게 하위 기획과 WI를 의미로 결속한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-471](../DECISIONS.md#L4625) | `D-STORY-MAIN-STORY-002` | `STORY` / `MAIN-STORY` | 흑막상인 원작을 메인 스토리 기준으로 삼고 원문 확인 전 각색을 확정하지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-472](../DECISIONS.md#L4634) | `D-PLANNING-EVIDENCE-GOVERNANCE-001` | `PLANNING` / `EVIDENCE-GOVERNANCE` | 기획 스레드는 개발 목표를 소유하거나 중간 완료를 기다리지 않는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-473](../DECISIONS.md#L4642) | `D-PLANNING-EVIDENCE-GOVERNANCE-002` | `PLANNING` / `EVIDENCE-GOVERNANCE` | 상위 E는 같은 후보의 모든 하위 E를 누적 소비해 성립한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-474](../DECISIONS.md#L4650) | `D-STORY-YODONG-001` | `STORY` / `YODONG` | 요동성 방어를 메인 스토리의 첫 장기 목표로 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-475](../DECISIONS.md#L4661) | `D-STORY-YODONG-002` | `STORY` / `YODONG` | 요동성 첫 장의 위협·인과·결과·복구 기준선을 닫는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-476](../DECISIONS.md#L4671) | `D-GAMEPLAY-YODONG-001` | `GAMEPLAY` / `YODONG` | 요동성 방어의 기본은 내정·전투 혼합 대응이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-477](../DECISIONS.md#L4680) | `D-GAMEPLAY-YODONG-002` | `GAMEPLAY` / `YODONG` | 플레이 유형을 먼저 고르지 않고 실제 활동 기록이 요동성 방비를 바꾼다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-478](../DECISIONS.md#L4690) | `D-GAMEPLAY-YODONG-003` | `GAMEPLAY` / `YODONG` | 첫 방어의 군내 역할은 평소 키운 역량에서 주로 정한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-479](../DECISIONS.md#L4699) | `D-GAMEPLAY-YODONG-004` | `GAMEPLAY` / `YODONG` | 추천 역할과 다른 행동의 한계는 실제 능력 데이터와 결과로 드러낸다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-480](../DECISIONS.md#L4708) | `D-PRESENTATION-COMBAT-RISK-001` | `PRESENTATION` / `COMBAT-RISK` | 몬스터 이름 색으로 현재 조건의 상대 위험을 보여 준다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-481](../DECISIONS.md#L4717) | `D-PRESENTATION-COMBAT-RISK-002` | `PRESENTATION` / `COMBAT-RISK` | 개인과 분대의 상대 위험을 구분하고 실제 보급 기여를 분대에 반영한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-482](../DECISIONS.md#L4726) | `D-PRESENTATION-COMBAT-RISK-003` | `PRESENTATION` / `COMBAT-RISK` | 전투 중 실제 상태 변화에 따라 개인·분대 위험 색을 갱신한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-483](../DECISIONS.md#L4735) | `D-INTERACTION-COMBAT-COMMAND-001` | `INTERACTION` / `COMBAT-COMMAND` | 위험 색은 대응을 추천하지 않고 실행 가능한 전술 선택만 연다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-484](../DECISIONS.md#L4744) | `D-INTERACTION-COMBAT-COMMAND-002` | `INTERACTION` / `COMBAT-COMMAND` | 빠른 전술 메뉴에는 현재 실행 가능한 명령만 표시한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-485](../DECISIONS.md#L4753) | `D-GAMEPLAY-BATTLE-PREPARATION-001` | `GAMEPLAY` / `BATTLE-PREPARATION` | 전투 전에 후퇴·증원·보급 선택의 실제 조건을 마련한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-486](../DECISIONS.md#L4762) | `D-PLANNING-DECISION-NAMING-001` | `PLANNING` / `DECISION-NAMING` | 전역 이력 번호와 분야별 결정 ID를 함께 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-487](../DECISIONS.md#L4772) | `D-PLANNING-DECISION-WI-RELATION-001` | `PLANNING` / `DECISION-WI-RELATION` | 결정 분야 분류와 공식 WI 대장을 양방향 관계 색인으로 연결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-488](../DECISIONS.md#L4781) | `D-PLANNING-GRAPH-MAP-HANDOFF-001` | `PLANNING` / `GRAPH-MAP-HANDOFF` | 승인 기획을 Graph Map 작업에 판본 인계하고 최종 결과만 기획에 반환한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-489](../DECISIONS.md#L4790) | `D-PLANNING-GRAPH-MAP-DEVELOPMENT-HANDOFF-001` | `PLANNING` / `GRAPH-MAP-DEVELOPMENT-HANDOFF` | Graph Map의 작은 구현 후보를 기존 Goal·WI·작업 명세에 결속해 개발로 인계한다 | `WI-FARM-01` | `ExplicitCanonicalLink` |
| [D-490](../DECISIONS.md#L4799) | `D-STORY-PROTAGONIST-001` | `STORY` / `PROTAGONIST` | 플레이어는 병약한 소가주의 몸에 빙의한 SSS급 연금술사다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-491](../DECISIONS.md#L4807) | `D-GAMEPLAY-ALCHEMY-ECONOMY-001` | `GAMEPLAY` / `ALCHEMY-ECONOMY` | 플레이어는 핵심 연금술을 직접 하고 일상 운영은 NPC에게 위임한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-492](../DECISIONS.md#L4814) | `D-STORY-YODONG-SUCCESSION-001` | `STORY` / `YODONG-SUCCESSION` | 현 가주의 전사는 원래 역사지만 플레이어의 축적 준비로 바꿀 수 있다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-493](../DECISIONS.md#L4822) | `D-GAMEPLAY-YODONG-CRISIS-001` | `GAMEPLAY` / `YODONG-CRISIS` | 요동성은 정확한 countdown 대신 징후와 주기적 위기로 최전방의 긴장을 만든다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-494](../DECISIONS.md#L4829) | `D-GAMEPLAY-WINTER-LOGISTICS-001` | `GAMEPLAY` / `WINTER-LOGISTICS` | 겨울 전쟁의 난방·방한·의료·보급 준비가 전투 인력을 보호한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-495](../DECISIONS.md#L4836) | `D-STORY-DUAL-PROTAGONIST-001` | `STORY` / `DUAL-PROTAGONIST` | 새 게임에서 모험가와 소가주 중 빙의 대상을 고르고 다른 인물은 주요 NPC로 남긴다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-496](../DECISIONS.md#L4845) | `D-STORY-DUAL-PROTAGONIST-002` | `STORY` / `DUAL-PROTAGONIST` | 모험가와 소가주는 불신 속 제한 협력으로 시작하고 실제 기록에 따라 관계가 달라진다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-497](../DECISIONS.md#L4853) | `D-GAMEPLAY-SETTLEMENT-SUPPLY-001` | `GAMEPLAY` / `SETTLEMENT-SUPPLY` | 영지 인구에서 자원 수요를 파생하고 부족분은 생산·교역·Hub 조달로 충족한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-498](../DECISIONS.md#L4861) | `D-GAMEPLAY-SETTLEMENT-SUPPLY-002` | `GAMEPLAY` / `SETTLEMENT-SUPPLY` | 자원별 평시 비축일수에 계절·위기 보정치를 적용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-499](../DECISIONS.md#L4869) | `D-GAMEPLAY-SETTLEMENT-SUPPLY-003` | `GAMEPLAY` / `SETTLEMENT-SUPPLY` | 부족 물자는 모든 집단의 최소 생존선을 보호한 뒤 긴급도에 따라 배분한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-500](../DECISIONS.md#L4877) | `D-GAMEPLAY-SETTLEMENT-SUPPLY-004` | `GAMEPLAY` / `SETTLEMENT-SUPPLY` | 모든 최소 생존선을 지킬 수 없으면 보호 우선순위와 예상 손실을 공개해 예외 배분한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-501](../DECISIONS.md#L4885) | `D-GAMEPLAY-SETTLEMENT-SUPPLY-005` | `GAMEPLAY` / `SETTLEMENT-SUPPLY` | 전원 보호를 우선하고 불가능할 때 생명 수 최대화 뒤 필수 기능을 보존한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-502](../DECISIONS.md#L4893) | `D-GAMEPLAY-SETTLEMENT-SUPPLY-006` | `GAMEPLAY` / `SETTLEMENT-SUPPLY` | 새 배분 규칙은 기존 정책과 결과를 비교하고 시험한 뒤 채택하거나 폐기한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-503](../DECISIONS.md#L4901) | `D-INTERACTION-EMBODIED-STORY-CHOICE-001` | `INTERACTION` / `EMBODIED-STORY-CHOICE` | 스토리 선택은 카드가 아니라 3인칭 현장 행동과 순서로 기록한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-504](../DECISIONS.md#L4909) | `D-INTERACTION-EMBODIED-STORY-CHOICE-002` | `INTERACTION` / `EMBODIED-STORY-CHOICE` | 행동형 스토리 선택에는 countdown 대신 현장 징후의 부드러운 시간 압박을 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-505](../DECISIONS.md#L4918) | `D-PLANNING-AUDIO-REQUIREMENT-001` | `PLANNING` / `AUDIO-REQUIREMENT` | 기획 중 발견한 소리는 중앙 오디오 요구사항 대장에 별도 상태로 기록한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-506](../DECISIONS.md#L4926) | `D-INTERACTION-EMBODIED-STORY-CHOICE-003` | `INTERACTION` / `EMBODIED-STORY-CHOICE` | 시간 기반 압박은 신속한 대응으로 현장 목표를 모두 해결할 수 있게 설계한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-507](../DECISIONS.md#L4934) | `D-STORY-ADVENTURER-POWER-GROWTH-001` | `STORY` / `ADVENTURER-POWER-GROWTH` | 모험가 시작은 개인의 신뢰에서 동료·거점·연결망으로 세력이 점진적으로 확장된다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-508](../DECISIONS.md#L4942) | `D-INTERACTION-EMBODIED-STORY-CHOICE-004` | `INTERACTION` / `EMBODIED-STORY-CHOICE` | 현장 NPC에게 생존자 응급처치와 마차·화물 보호를 맡길 수 있다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-509](../DECISIONS.md#L4950) | `D-STORY-ADVENTURER-FIRST-MEETING-001` | `STORY` / `ADVENTURER-FIRST-MEETING` | 모험가는 Nature와 Town을 오가다 경비대장에게 발탁되어 병영학교에서 소가주를 만난다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-510](../DECISIONS.md#L4960) | `D-STORY-ADVENTURER-POWER-GROWTH-002` | `STORY` / `ADVENTURER-POWER-GROWTH` | 모험가의 첫 세력은 관계를 유지하며 함께 이동하는 소규모 NPC 동행대다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-511](../DECISIONS.md#L4969) | `D-INTERACTION-NPC-FIELD-AUTONOMY-001` | `INTERACTION` / `NPC-FIELD-AUTONOMY` | 현장 NPC는 별도 지시가 없어도 안전한 범위의 기본 응급처치를 수행한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-512](../DECISIONS.md#L4977) | `D-GAMEPLAY-COMPANION-PARTY-COMPOSITION-001` | `GAMEPLAY` / `COMPANION-PARTY-COMPOSITION` | NPC별 적성과 역할 조합에 따라 소규모 동행대의 장점과 공백이 달라진다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-513](../DECISIONS.md#L4986) | `D-STORY-ADVENTURER-IDEA-SIGHT-001` | `STORY` / `ADVENTURER-IDEA-SIGHT` | 모험가는 이데아의 편린을 통해 검술의 본질을 꿰뚫어보는 시각을 가진다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-514](../DECISIONS.md#L4995) | `D-STORY-ADVENTURER-FIRST-MISSION-001` | `STORY` / `ADVENTURER-FIRST-MISSION` | 경비대장은 모험가에게 병영학교 입교 대신 첫 호송대 호위 임무를 부탁한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-515](../DECISIONS.md#L5004) | `D-INTERACTION-NPC-FIELD-AUTONOMY-002` | `INTERACTION` / `NPC-FIELD-AUTONOMY` | 희귀 약품은 기본 승인제로 두되 현장을 떠나기 전에 NPC 자율 사용 정책을 설정한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-516](../DECISIONS.md#L5013) | `D-GAMEPLAY-COMPANION-PARTY-COMPOSITION-002` | `GAMEPLAY` / `COMPANION-PARTY-COMPOSITION` | 모험가 동행대의 기본 전투 조합은 플레이어를 포함한 근거리 1명과 원거리 1명이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-517](../DECISIONS.md#L5021) | `D-STORY-ADVENTURER-POWER-GROWTH-003` | `STORY` / `ADVENTURER-POWER-GROWTH` | NPC 관계의 중심은 선물과 대화가 아니라 실제 공동 행동과 약속 이행 기록이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-518](../DECISIONS.md#L5029) | `D-STORY-ADVENTURER-IDEA-SIGHT-002` | `STORY` / `ADVENTURER-IDEA-SIGHT` | 병영·마법학교·마탑 교육의 공통 목표는 이데아를 직관해 현실에 드러내는 것이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-519](../DECISIONS.md#L5038) | `D-STORY-ADVENTURER-IDEA-SIGHT-003` | `STORY` / `ADVENTURER-IDEA-SIGHT` | 경비대장의 소개로 만난 훈련 교관의 시범 동작에서 첫 검술 이데아 후보를 직관한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-520](../DECISIONS.md#L5047) | `D-STORY-ADVENTURER-FIRST-MISSION-002` | `STORY` / `ADVENTURER-FIRST-MISSION` | 첫 호송 화물은 국방 경비 거점으로 보내는 의약품과 보존식량이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-521](../DECISIONS.md#L5056) | `D-STORY-ADVENTURER-IDEA-SIGHT-004` | `STORY` / `ADVENTURER-IDEA-SIGHT` | 모험가의 핵심 재능은 이데아를 직관하고 원리를 자기 방식으로 흡수·재구성하는 능력이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-522](../DECISIONS.md#L5065) | `D-PLANNING-PROJECT-IDENTITY-002` | `PLANNING` / `PROJECT-IDENTITY` | 메인 스토리 기획서의 표시 이름은 Mirror로 한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-523](../DECISIONS.md#L5072) | `D-GAMEPLAY-COMPANION-PARTY-COMPOSITION-003` | `GAMEPLAY` / `COMPANION-PARTY-COMPOSITION` | 자주 쓰는 동행대 구성을 프리셋으로 저장하고 출발 전에 현재 조건을 다시 확인한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-524](../DECISIONS.md#L5081) | `D-STORY-ADVENTURER-FIRST-MISSION-003` | `STORY` / `ADVENTURER-FIRST-MISSION` | 경비대 호송 의뢰 전에 한스 농장에서 공동 행동으로 친분을 쌓는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-525](../DECISIONS.md#L5090) | `D-STORY-ADVENTURER-FIRST-MISSION-004` | `STORY` / `ADVENTURER-FIRST-MISSION` | 한스와의 첫 공동 행동은 농장 침입 흔적 조사와 무너진 울타리 임시 보수다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-526](../DECISIONS.md#L5098) | `D-INTERACTION-NPC-FIELD-AUTONOMY-003` | `INTERACTION` / `NPC-FIELD-AUTONOMY` | 호송대 중상자는 현장 책임자의 기록 아래 사전 허용된 의약품을 긴급 사용한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-527](../DECISIONS.md#L5107) | `D-INTERACTION-EMBODIED-STORY-CHOICE-005` | `INTERACTION` / `EMBODIED-STORY-CHOICE` | 중요한 현장 결정은 실행 가능한 선택 카드로 안내하고 실제 행동으로 성립시킨다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-528](../DECISIONS.md#L5116) | `D-STORY-ADVENTURER-IDEA-SIGHT-005` | `STORY` / `ADVENTURER-IDEA-SIGHT` | 교관의 기본기 시범을 직관하면 미숙련 스킬이 상태창에 조용히 등록된다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-529](../DECISIONS.md#L5126) | `D-STORY-ADVENTURER-FIRST-MISSION-005` | `STORY` / `ADVENTURER-FIRST-MISSION` | 한스와의 첫 신뢰는 플레이어의 자발적인 벌목과 무보수 울타리 수리에서 시작한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-530](../DECISIONS.md#L5135) | `D-INTERACTION-EMBODIED-STORY-CHOICE-006` | `INTERACTION` / `EMBODIED-STORY-CHOICE` | 선택 카드 열람은 안전한 상황에서 정지하고 시간 민감 사건에서는 감속한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-531](../DECISIONS.md#L5143) | `D-STORY-ADVENTURER-FIRST-MISSION-006` | `STORY` / `ADVENTURER-FIRST-MISSION` | 첫 울타리 수리는 현장 손도끼 발견과 선택 카드 뒤 직접 행동으로 완결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-532](../DECISIONS.md#L5153) | `D-STORY-ADVENTURER-FIRST-MISSION-007` | `STORY` / `ADVENTURER-FIRST-MISSION` | 현장 손도끼 사용과 한스 집 옆 추가 목재 적재는 선택 카드로 분리한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-533](../DECISIONS.md#L5162) | `D-STORY-ADVENTURER-FIRST-MISSION-008` | `STORY` / `ADVENTURER-FIRST-MISSION` | 한스는 플레이어가 떠난 뒤 집 옆의 추가 목재를 발견한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-534](../DECISIONS.md#L5171) | `D-INTERACTION-EMBODIED-STORY-CHOICE-007` | `INTERACTION` / `EMBODIED-STORY-CHOICE` | 플레이어는 이동 중 변화된 농장을 먼저 보고 한스의 후속 표식을 선택한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-535](../DECISIONS.md#L5180) | `D-PRESENTATION-FARM-HANS-HOUSE-001` | `PRESENTATION` / `FARM-HANS-HOUSE` | 여분 목재로 수리되는 첫 농장 변화는 한스의 집이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-536](../DECISIONS.md#L5189) | `D-PRESENTATION-FARM-HANS-HOUSE-002` | `PRESENTATION` / `FARM-HANS-HOUSE` | 한스의 손상된 집은 원본을 보존한 Blender 전용 복사본 후보로 만든다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-537](../DECISIONS.md#L5198) | `D-STORY-ADVENTURER-FIRST-MISSION-009` | `STORY` / `ADVENTURER-FIRST-MISSION` | 수리된 한스의 집은 경비대 임무를 지원하는 첫 생활 거점이 된다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-538](../DECISIONS.md#L5207) | `D-STORY-HANS-HIDDEN-MASTER-001` | `STORY` / `HANS-HIDDEN-MASTER` | 한스는 첫 경계 순찰에서 정체를 숨긴 은둔고수의 면모를 드러낸다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-539](../DECISIONS.md#L5216) | `D-STORY-HANS-HIDDEN-MASTER-002` | `STORY` / `HANS-HIDDEN-MASTER` | 한스가 집 수리를 미룬 이유는 무능이나 체력 부족이 아니라 능글맞은 무심함이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-540](../DECISIONS.md#L5225) | `D-STORY-HANS-HIDDEN-MASTER-003` | `STORY` / `HANS-HIDDEN-MASTER` | 한스는 감사를 능글맞게 돌려 말하고 식사·잠자리로 답한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-541](../DECISIONS.md#L5233) | `D-STORY-HANS-HIDDEN-MASTER-004` | `STORY` / `HANS-HIDDEN-MASTER` | 한스 집에는 오래됐지만 잘 관리된 무기 하나가 선택형 정체 단서로 남는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-542](../DECISIONS.md#L5242) | `D-PRESENTATION-HANS-WEAPON-001` | `PRESENTATION` / `HANS-WEAPON` | 한스의 관리된 무기는 검으로 정하고 Synty Prefab 후보를 E4에서 조사한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-543](../DECISIONS.md#L5251) | `D-STORY-FARM-BOUNDARY-THREAT-001` | `STORY` / `FARM-BOUNDARY-THREAT` | 첫 경계 순찰의 마수 무리는 깊은 숲의 더 큰 위협에 밀려 내려왔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-544](../DECISIONS.md#L5260) | `D-PRESENTATION-HANS-WEAPON-002` | `PRESENTATION` / `HANS-WEAPON` | 한스의 검은 첫 순찰에서 사용하지 않고 이후의 진짜 위기에 남겨 둔다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-545](../DECISIONS.md#L5269) | `D-STORY-FARM-BOUNDARY-THREAT-002` | `STORY` / `FARM-BOUNDARY-THREAT` | 영역을 넓히는 거대 마수는 요동성 적대 조직이 이용하는 전위 위협이다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-546](../DECISIONS.md#L5278) | `D-STORY-ADVENTURER-IDEA-SIGHT-006` | `STORY` / `ADVENTURER-IDEA-SIGHT` | 첫 Town 공방 경험은 견습의 생산 실패 원인을 직관해 복구를 돕는다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-547](../DECISIONS.md#L5287) | `D-GAMEPLAY-HUB-INFERENCE-QUEST-001` | `GAMEPLAY` / `HUB-INFERENCE-QUEST` | 허브의 미도착 화물은 플레이 중 모은 단서를 직접 추론해 해결한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-548](../DECISIONS.md#L5296) | `D-GAMEPLAY-HUB-RECOVERY-MEDITATION-001` | `GAMEPLAY` / `HUB-RECOVERY-MEDITATION` | 미도착 화물 문제 예방은 플레이어와 NPC의 회복을 높이고 플레이어의 명상 준비에 가중한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-549](../DECISIONS.md#L5306) | `D-GAMEPLAY-MEDITATION-INSPIRATION-001` | `GAMEPLAY` / `MEDITATION-INSPIRATION` | 회복이 높은 때의 명상은 실제 획득한 영감과 이데아의 편린 산출량을 증폭한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-550](../DECISIONS.md#L5315) | `D-GAMEPLAY-MEDITATION-RECOVERY-LOOP-001` | `GAMEPLAY` / `MEDITATION-RECOVERY-LOOP` | 증폭 명상은 회복을 소모하지 않고 적절한 완료 뒤 회복을 더 높인다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-551](../DECISIONS.md#L5324) | `D-GAMEPLAY-COMBAT-MIND-FOCUS-001` | `GAMEPLAY` / `COMBAT-MIND-FOCUS` | 강적 조우의 정신적 위협에는 전투 중 집중 타이밍으로 회복을 지키고 되올릴 수 있다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-552](../DECISIONS.md#L5334) | `D-INTERACTION-COMBAT-FOCUS-TRIGGER-001` | `INTERACTION` / `COMBAT-FOCUS-TRIGGER` | 전투 집중 기회는 위협이 크게 변하는 핵심 순간에만 짧게 연다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-553](../DECISIONS.md#L5342) | `D-GAMEPLAY-COMBAT-DIVIDE-CONQUER-001` | `GAMEPLAY` / `COMBAT-DIVIDE-CONQUER` | 강대한 적의 종합 역량은 구성 노드와 지원 연결을 실제로 분리해 낮출 수 있다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-554](../DECISIONS.md#L5352) | `D-PRESENTATION-SYNTY-SURVEY-HANDOFF-001` | `PRESENTATION` / `SYNTY-SURVEY-HANDOFF` | 최근 확정 기획의 Synty 표현 요구를 선별해 개발·전문 조사로 인계한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-555](../DECISIONS.md#L5361) | `D-PRESENTATION-FARM-BOUNDARY-BEAST-001` | `PRESENTATION` / `FARM-BOUNDARY-BEAST` | 첫 경계 마수는 동물형·야수형 의미를 유지하고 현재 사람형 후보는 미확보 보류한다 | 미명시 | `NoExplicitCanonicalLink` |
| [D-556](../DECISIONS.md#L5369) | `D-GAMEPLAY-FIVE-ELEMENT-RECOVERY-PURPOSE-001` | `GAMEPLAY` / `FIVE-ELEMENT-RECOVERY-PURPOSE` | 게임의 상위 목적은 나와 상대의 오행 순환을 회복시켜 광복기 가능성을 넓히는 것이다 | 미명시 | `NoExplicitCanonicalLink` |

## WI에서 결정 보기

| WI | 그룹 | 기능 | 구현 / 통합 | 연결된 결정 | 상태 |
| --- | --- | --- | --- | --- | --- |
| `WI-ACTOR-01` | `ACTOR` | 물품 획득 | `Done:E3 / InProgress:E5` | `D-261` / `D-EVIDENCE-PRESENTATION-INTEGRATION-012` | `ExplicitDecisionLink` |
| `WI-ACTOR-02` | `ACTOR` | 장착 상태 변경 | `Done:E3 / InProgress:E5` | `D-261` / `D-EVIDENCE-PRESENTATION-INTEGRATION-012` | `ExplicitDecisionLink` |
| `WI-ACTOR-03` | `ACTOR` | 지식 습득 | `Done:E3 / InProgress:E4` | `D-280` / `D-PLANNING-GOAL-INQUIRY-HANDOFF-012`<br>`D-286` / `D-PLANNING-GOAL-INQUIRY-HANDOFF-018` | `ExplicitDecisionLink` |
| `WI-ACTOR-CONSUME` | `ACTOR` | 물품 섭취 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-ACTOR-PLAN-SET` | `ACTOR` | 개인 계획 설정 | `Done:E4 / InProgress:E4` | 미명시 | `NoExplicitDecisionLink` |
| `WI-CARD-01` | `CARD` | 현재 세계의 메이저 아르카나 활성화 | `Done:E3 / InProgress:E4` | `D-239` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-009` | `ExplicitDecisionLink` |
| `WI-CITY-01` | `CITY` | 도심 서비스 수요 확정 | `NotStarted:E1 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-CITY-02` | `CITY` | 도심 서비스용 지역 재고 배정 | `NotStarted:E1 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-CITY-03` | `CITY` | 도심 주민 서비스 처리 | `NotStarted:E1 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-CITY-04` | `CITY` | 도심 서비스 결과 확인 | `NotStarted:E1 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-COMMUNITY-VISITOR-STAY` | `COMMUNITY` | 방문자 임시 체류 결정 | `Done:E4 / InProgress:E4` | `D-286` / `D-PLANNING-GOAL-INQUIRY-HANDOFF-018` | `ExplicitDecisionLink` |
| `WI-COMMUNITY-COOPERATION-PROPOSE` | `COMMUNITY` | 공동체 협력 제안 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-COMMUNITY-ENTRANCE-POLICY-SET` | `COMMUNITY` | 공동체 출입 정책 설정 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-COMMUNITY-HIRE` | `COMMUNITY` | NPC 고용 확정 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-COMMUNITY-MEMBERSHIP-CONFIRM` | `COMMUNITY` | 공동체 정식 편입 확정 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-COMMUNITY-REMOTE-RESPONSE` | `COMMUNITY` | 원격 응대 지시 확정 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-COMMUNITY-SUPPORT-MISSION-JOIN` | `COMMUNITY` | 공동 지원 임무 참여 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-GUEST-PERMISSION-SET` | `COMMUNITY` | 손님 활동 권한 설정 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-CON-01` | `CON` | 영역 건물 건설 확정 | `Done:E3 / Done:E7` | 미명시 | `NoExplicitDecisionLink` |
| `WI-CON-BLUEPRINT-PLACE` | `CON` | 건설 청사진 배치 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-CON-DEMOLISH` | `CON` | 건설물 해체 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-CON-MATERIAL-DEPOSIT` | `CON` | 건설 재료 투입 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-CON-WORK-CONTRIBUTE` | `CON` | 건설 시공 기여 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-FARM-01` | `FARM` | 경작지 밭갈이 | `Done:E3 / InProgress:E6` | `D-286` / `D-PLANNING-GOAL-INQUIRY-HANDOFF-018`<br>`D-489` / `D-PLANNING-GRAPH-MAP-DEVELOPMENT-HANDOFF-001` | `ExplicitDecisionLink` |
| `WI-FARM-02` | `FARM` | 경작지 씨앗 파종 | `Done:E3 / InProgress:E6` | `D-286` / `D-PLANNING-GOAL-INQUIRY-HANDOFF-018` | `ExplicitDecisionLink` |
| `WI-FARM-03` | `FARM` | 농작물 생육 관리 | `Done:E3 / InProgress:E6` | `D-286` / `D-PLANNING-GOAL-INQUIRY-HANDOFF-018` | `ExplicitDecisionLink` |
| `WI-FARM-04` | `FARM` | 익은 농작물 수확 | `Done:E3 / InProgress:E6` | `D-286` / `D-PLANNING-GOAL-INQUIRY-HANDOFF-018` | `ExplicitDecisionLink` |
| `WI-FARM-05` | `FARM` | 수확물 집하장 모으기 | `Done:E3 / InProgress:E6` | 미명시 | `NoExplicitDecisionLink` |
| `WI-FARM-06` | `FARM` | 출하 물량 포장 | `Done:E3 / InProgress:E6` | 미명시 | `NoExplicitDecisionLink` |
| `WI-FARM-DEFENSE-MOBILIZE` | `FARM` | 방위 분대 소집 | `Done:E4 / InProgress:E4` | 미명시 | `NoExplicitDecisionLink` |
| `WI-SQUAD-ASSIGN` | `FARM` | 경비 초소 분대 배정 | `Done:E3 / InProgress:E3` | 미명시 | `NoExplicitDecisionLink` |
| `WI-SQUAD-SUPPLY` | `FARM` | 경비 분대 식량·장비 보급 | `Done:E3 / InProgress:E3` | 미명시 | `NoExplicitDecisionLink` |
| `WI-FARM-DEFENSE-RESOLVE` | `FARM` | Farm 방어 성공 결과 발현 | `Done:E3 / InProgress:E3` | 미명시 | `NoExplicitDecisionLink` |
| `WI-FARM-DEFENSE-RETURN` | `FARM` | Farm 방위 분대 초소 귀환 인계 | `Done:E3 / InProgress:E3` | 미명시 | `NoExplicitDecisionLink` |
| `WI-FARM-FIELD-BOUNDARY-CONFIRM` | `FARM` | 밭 경계 확정 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-FARM-SOIL-AMEND` | `FARM` | 토양 개량 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-FARM-WATER-TRANSFER` | `FARM` | 농업 용수 이송 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-001` | `HUB` | 입고 화물 검수 | `Done:E3 / InProgress:E4` | 미명시 | `NoExplicitDecisionLink` |
| `WI-002` | `HUB` | 검수 완료 화물 창고 적재 | `Done:E3 / InProgress:E4` | 미명시 | `NoExplicitDecisionLink` |
| `WI-HUB-03` | `HUB` | 출고 대상 재고 요청 | `Done:E3 / NotSelected:E1` | `D-241` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-011` | `ExplicitDecisionLink` |
| `WI-HUB-04` | `HUB` | 출고 대상 재고 피킹 | `Done:E3 / NotSelected:E1` | `D-241` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-011`<br>`D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-HUB-05` | `HUB` | 피킹 화물 포장 | `Done:E3 / NotSelected:E1` | `D-239` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-009`<br>`D-241` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-011`<br>`D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-HUB-06` | `HUB` | 출고 차량 상차 | `Done:E3 / NotSelected:E1` | `D-239` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-009`<br>`D-241` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-011` | `ExplicitDecisionLink` |
| `WI-HUB-DEMAND-ALLOCATE` | `HUB` | Hub 수요 재고 할당 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-HUB-SUPPLY-TASK-ACCEPT` | `HUB` | Hub 조달 과제 수락 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-LOG-01` | `LOG` | 출하 차량 상차 확정 | `Done:E3 / InProgress:E6` | 미명시 | `NoExplicitDecisionLink` |
| `WI-LOG-02` | `LOG` | 농장에서 출발 | `Done:E3 / InProgress:E6` | `D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-LOG-03` | `LOG` | 농장에서 물류 거점으로 화물 이동 | `Done:E3 / InProgress:E4` | `D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-LOG-04` | `LOG` | 물류 거점 도착 화물 하차 | `Done:E3 / InProgress:E4` | `D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-LOG-05` | `LOG` | 물류 거점 도착 화물 인수 | `Done:E3 / InProgress:E4` | `D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-MARKET-01` | `MARKET` | 물류 거점에서 마트로 운송 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-MARKET-02` | `MARKET` | 마트 도착 화물 인수 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-MARKET-03` | `MARKET` | 마트 입고 상품 검수 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-MARKET-04` | `MARKET` | 검수 상품 후방 창고 적재 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-MARKET-05` | `MARKET` | 매장 진열대 상품 보충 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-TOWN-DELIVERY-INSPECT` | `MARKET` | Town 납품 검수 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-TOWN-DELIVERY-RECEIVE` | `MARKET` | Town 납품 인수 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-TOWN-STOCK-PUTAWAY` | `MARKET` | Town 후방 재고 적재 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-TOWN-STOCK-REPLENISH` | `MARKET` | Town 재고 보충 주문 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-TOWN-SUPPLY-DISPATCH` | `MARKET` | Town 공급 운송 출발 확정 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-NATURE-01` | `NATURE` | 자연 지역 위험 징후 확인 | `Done:E3 / Done:E7` | `D-171` / `D-GAMEPLAY-NATURE-THREAT-RECOVERY-011`<br>`D-173` / `D-GAMEPLAY-NATURE-THREAT-RECOVERY-013`<br>`D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001` | `ExplicitDecisionLink` |
| `WI-NATURE-02` | `NATURE` | 안전 거점으로 긴급 후퇴 | `Done:E3 / NotSelected:E1` | `D-171` / `D-GAMEPLAY-NATURE-THREAT-RECOVERY-011`<br>`D-174` / `D-GAMEPLAY-NATURE-THREAT-RECOVERY-014`<br>`D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001` | `ExplicitDecisionLink` |
| `WI-NATURE-03` | `NATURE` | 훼손된 자연 경로 복원 | `Done:E3 / NotSelected:E1` | `D-171` / `D-GAMEPLAY-NATURE-THREAT-RECOVERY-011`<br>`D-175` / `D-GAMEPLAY-NATURE-THREAT-RECOVERY-015`<br>`D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001` | `ExplicitDecisionLink` |
| `WI-NATURE-04` | `NATURE` | 탐사대 안전 회복 | `Done:E3 / NotSelected:E1` | `D-171` / `D-GAMEPLAY-NATURE-THREAT-RECOVERY-011`<br>`D-174` / `D-GAMEPLAY-NATURE-THREAT-RECOVERY-014`<br>`D-176` / `D-GAMEPLAY-NATURE-THREAT-RECOVERY-016`<br>`D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001` | `ExplicitDecisionLink` |
| `WI-NATURE-05` | `NATURE` | 벌목 도끼 획득 | `Done:E3 / Done:E7` | `D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001`<br>`D-233` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-003`<br>`D-247` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-017`<br>`D-248` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-018`<br>`D-261` / `D-EVIDENCE-PRESENTATION-INTEGRATION-012`<br>`D-273` / `D-PLANNING-GOAL-INQUIRY-HANDOFF-005` | `ExplicitDecisionLink` |
| `WI-NATURE-06` | `NATURE` | 나무 벌목 작업 시작 | `Done:E3 / InProgress:E4` | `D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001`<br>`D-233` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-003`<br>`D-247` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-017`<br>`D-272` / `D-PLANNING-GOAL-INQUIRY-HANDOFF-004`<br>`D-359` / `D-PRESENTATION-ANIMATION-WORKFLOW-002` | `ExplicitDecisionLink` |
| `WI-NATURE-07` | `NATURE` | 오두막을 지을 터 선정 | `Done:E3 / InProgress:E4` | `D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001`<br>`D-233` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-003`<br>`D-244` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-014`<br>`D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-NATURE-08` | `NATURE` | 오두막 건설 작업 시작 | `Done:E3 / InProgress:E4` | `D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001`<br>`D-233` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-003` | `ExplicitDecisionLink` |
| `WI-NATURE-09` | `NATURE` | 오두막 안으로 들어가기 | `Done:E3 / InProgress:E4` | `D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001`<br>`D-233` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-003` | `ExplicitDecisionLink` |
| `WI-NATURE-10` | `NATURE` | 오두막 밖으로 나가기 | `Done:E3 / InProgress:E4` | `D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001`<br>`D-233` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-003` | `ExplicitDecisionLink` |
| `WI-NATURE-11` | `NATURE` | 황혼 위협 대응 방식 확정 | `Done:E3 / Done:E7` | `D-231` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-001`<br>`D-233` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-003`<br>`D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-NATURE-12` | `NATURE` | 진행 중 작업 취소 | `Done:E3 / InProgress:E4` | `D-233` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-003` | `ExplicitDecisionLink` |
| `WI-NATURE-13` | `NATURE` | 획득 자원 거점 보관 | `Done:E3 / Done:E7` | `D-257` / `D-EVIDENCE-PRESENTATION-INTEGRATION-008` | `ExplicitDecisionLink` |
| `WI-NATURE-14` | `NATURE` | 오두막에서 수면·새벽 맞기 | `Done:E3 / InProgress:E5` | `D-257` / `D-EVIDENCE-PRESENTATION-INTEGRATION-008`<br>`D-260` / `D-EVIDENCE-PRESENTATION-INTEGRATION-011` | `ExplicitDecisionLink` |
| `WI-NATURE-15` | `NATURE` | 다음 날 거점 확장 계획 선택 | `Done:E3 / InProgress:E6` | `D-257` / `D-EVIDENCE-PRESENTATION-INTEGRATION-008` | `ExplicitDecisionLink` |
| `WI-NATURE-16` | `NATURE` | 현장 보급 꾸러미 제작 | `Done:E3 / InProgress:E4` | `D-240` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-010`<br>`D-243` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-013` | `ExplicitDecisionLink` |
| `WI-NATURE-17` | `NATURE` | 현장 보급 제작 업무 위임 | `Done:E3 / InProgress:E4` | `D-243` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-013` | `ExplicitDecisionLink` |
| `WI-NATURE-18` | `NATURE` | 벌목 통나무 줍기 | `Done:E3 / Done:E7` | 미명시 | `NoExplicitDecisionLink` |
| `WI-CRAFT-BREW` | `NATURE` | 배합물 달이기 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-HEAT-SOURCE-STATE-CHANGE` | `NATURE` | 열원 상태 변경 | `Done:E4 / InProgress:E4` | 미명시 | `NoExplicitDecisionLink` |
| `WI-NATURE-HERB-GATHER` | `NATURE` | 약초 채집 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-NATURE-TRACE-INVESTIGATE` | `NATURE` | 자연 흔적 조사 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-ORDER-01` | `ORDER` | 주민 주문 확정 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-ORDER-02` | `ORDER` | 주문 상품 재고 예약 | `Done:E3 / NotSelected:E1` | `D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-ORDER-03` | `ORDER` | 주문 상품 피킹 | `Done:E3 / NotSelected:E1` | `D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-ORDER-04` | `ORDER` | 주문 상품 포장 | `Done:E3 / NotSelected:E1` | `D-170` / `D-GAMEPLAY-NATURE-THREAT-RECOVERY-010`<br>`D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-ORDER-05` | `ORDER` | 주문 상품 수령 준비 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-ORDER-06` | `ORDER` | 주민 주문 상품 수령 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-ORDER-07` | `ORDER` | 주민 상품 소비 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-REFLECT-01` | `REFLECT` | 승인 자료로 거점 성찰 확정 | `Done:E3 / NotSelected:E3` | 미명시 | `NoExplicitDecisionLink` |
| `WI-REVIEW-01` | `REVIEW` | NPC 업무 결과 검토 확정 | `InProgress:E2 / NotSelected:E1` | `D-267` / `D-EVIDENCE-PRESENTATION-INTEGRATION-018` | `ExplicitDecisionLink` |
| `WI-WORLD-01` | `WORLD` | NPC에게 반복 업무 배정 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-WORLD-02` | `WORLD` | NPC에게 업무 역량 위임 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-WORLD-03` | `WORLD` | 진행 중 세계 업무 취소 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-WORLD-04` | `WORLD` | 손상된 시설 수리 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-WORLD-05` | `WORLD` | 새로운 지역 발견 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-WORLD-06` | `WORLD` | 일행 역할 카드 장착 | `Done:E3 / NotSelected:E1` | `D-239` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-009`<br>`D-261` / `D-EVIDENCE-PRESENTATION-INTEGRATION-012` | `ExplicitDecisionLink` |
| `WI-WORLD-07` | `WORLD` | 세계 활동 상태 변경 | `Done:E3 / NotSelected:E1` | `D-245` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | `ExplicitDecisionLink` |
| `WI-WORLD-08` | `WORLD` | 하루 운영 턴 마감 | `Done:E3 / NotSelected:E1` | 미명시 | `NoExplicitDecisionLink` |
| `WI-COMBAT-DIRECT-CONTROL-SET` | `WORLD` | 직접 전투 조종 전환 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-COMBAT-TACTICAL-COMMAND` | `WORLD` | 분대 전술 명령 확정 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-EXPEDITION-DISPATCH` | `WORLD` | 탐사 임무 파견 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM` | `WORLD` | 목표 비축 미달 판매 확정 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-SURVIVAL-RATION-POLICY-SET` | `WORLD` | 생존 배급 정책 설정 | `NotStarted:E0 / NotSelected:E0` | 미명시 | `NoExplicitDecisionLink` |
| `WI-WORLD-RESOURCE-REGENERATE` | `WORLD` | 세계 자원 재생 | `Done:E4 / InProgress:E4` | 미명시 | `NoExplicitDecisionLink` |

## 비정규·현 대장 외 WI 표기

| 결정 | 표기 | 판정 |
| --- | --- | --- |
| `D-239` / `D-GAMEPLAY-WI-LOOP-PROGRESSION-009` | `WI-FARM-07` | `MissingFromCurrentCatalog` |
| `D-261` / `D-EVIDENCE-PRESENTATION-INTEGRATION-012` | `WI-WORLD-09` | `MissingFromCurrentCatalog` |
| `D-267` / `D-EVIDENCE-PRESENTATION-INTEGRATION-018` | `WI-WORLD-09` | `MissingFromCurrentCatalog` |
