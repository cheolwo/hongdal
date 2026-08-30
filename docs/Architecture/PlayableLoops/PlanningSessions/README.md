# PlayableLoop 문답 기록 경로

이 폴더는 하나의 긴 대화 기록에 서로 다른 게임 구성요소를 계속 추가하지 않고, 질문의 주 상태 소유자에 따라 문답을 나누어 저장한다.

## 기록 원칙

1. 새 질문은 `Q-{주제}-D{1~5}-{주제내연번} · 전체 Q-{전체연번}`을 사용한다. 기존 `Q-001` 형식은 전체 호환 번호로 유지하며 이동 때문에 다시 번호를 부여하지 않는다.
2. `D1~D5`는 질문 깊이와 예상 E 준비 범위를 함께 보여 주지만 실제 Evidence 승격은 아니다. 각 문답 문서는 예상 범위와 실제 Logic·Presentation·통합 E를 나란히 기록한다.
3. 한 질문의 권위 답변은 한 주제 문서에만 둔다. 다른 주제는 링크만 건다.
4. 질문이 둘 이상의 분야에 영향을 주면 플레이어가 실제로 선택하거나 상태가 바뀌는 주제에 기록한다.
5. 전문 연구 결과는 문답 문서에 복제하지 않고 연구 문서 revision을 참조한다.
6. 개발 인계는 주제 문서의 확정 범위만 별도 승인 기획서와 작업 명세로 동결한다.

## 현재 routing

| 주제 | 권위 문답 문서 | 이관 질문 | 다음 기록 위치 |
| --- | --- | --- | --- |
| Nature 거점·수면·날씨·방어 | [Nature 거점·수면 문답](Nature거점수면/nature-shelter-sleep.inquiry.r1.md) | Q-001~005, Q-023~035, Q-132, Q-141, Q-149, Q-153, Q-156 | 같은 파일 |
| 플레이어 내면·명상·계획 | [플레이어 내면·명상 문답](플레이어내면명상/player-mind-meditation.inquiry.r1.md) | Q-006~022, Q-040~044, Q-065~067, Q-122~125, Q-196~198 | 같은 파일 |
| Nature 자원·LandUse·건설 | [Nature 자원·건설 문답](Nature자원건설/nature-resource-construction.inquiry.r1.md) | Q-036~039, Q-051~060, Q-134 | 같은 파일 |
| 약초·Recipe·조합 제작 | [약초 Recipe 제작 문답](약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md) | Q-045~050, Q-061~064, Q-068~071, Q-131, Q-133, Q-142, Q-150, Q-157, Q-269~296, Q-340~346 | Q-347 답변 대기, 같은 파일 |
| 저장·Load·재진입 | [저장·재진입 문답](저장재진입/save-load-runtime.inquiry.r1.md) | Q-072~076, Q-139~140 | 같은 파일 |
| 영역별 건물·공간·배치·협력 | [건물·공간·배치 문답](건물공간배치/building-spatial-placement.inquiry.r1.md) | Q-077~121, Q-126~130, Q-136, Q-143~144, Q-146~147, Q-155, Q-297~339 | 같은 파일 |
| Town 주문·입고·회랑 안전화 | [Town 주문 수령 문답](Town주문수령/town-order-pickup.inquiry.r1.md) | Q-135, Q-137~138, Q-145, Q-148, Q-151~152, Q-154, Q-158~160 | 같은 파일 |
| 지역 오행·몬스터·개척 준비 | [지역 오행·몬스터 문답](지역오행몬스터/region-five-elements-monster.inquiry.r1.md) | Q-161~195 | 같은 파일 |
| 공동체 편입·손님·원격 응대 | [공동체 편입·손님 문답](공동체편입방문/community-membership-visitor.inquiry.r1.md) | Q-199~219 | 같은 파일 |
| 시스템 보조 건물 배치 | [시스템 보조 배치 문답](배치엔진보조/building-placement-assistance.inquiry.r1.md) | Q-220~222 | 첫 대상 건물 선택 뒤 같은 파일 |
| Farm 병영·방위·분대 운영 | [Farm 병영·방위 문답](Farm병영방위/farm-barracks-defense.inquiry.r1.md) | 전체 Q-223~239 | 주제 기획서 합성 전까지 같은 파일 |
| Hub 영역별 수요·재고 할당·출고 준비 | [Hub 수요·분배 문답](Hub수요분배/hub-demand-allocation.inquiry.r1.md) | 전체 Q-240~250 | Q-250 재개 시 같은 파일 |
| 생존경제·생산·소비·비축 | [생존경제 문답](생존경제/survival-economy.inquiry.r1.md) | 전체 Q-251~266 | 첫 독립 적용 Area 선택 뒤 같은 파일 |
| Solo 업무 위임·예외 | [Solo 업무 위임 문답](솔로업무위임/solo-work-delegation.inquiry.r1.md) | 전체 Q-267~268 | Q-268부터 같은 파일 |

## 기획 이미지

- [PlayableLoop 기획 이미지 색인](기획이미지색인.md)은 문답의 공간·배치·UI 후보와 Synty 기능군을 연결한다.
- 기획 이미지는 실제 Prefab·Scene·Play Mode·Game View 증거가 아니며, Presentation E4 후보 조사와 E5 실제 배치 검증을 대신하지 않는다.

## 오디오 요구사항

- [PlayableLoop 오디오 요구사항 대장](../오디오요구사항대장.md)은 문답에서 발견한 효과음·환경음·배경음·음성 요구와 생성·구매·녹음 후보, Unity 결속과 실제 청취 상태를 한곳에서 관리한다.
- 주제 문답에는 플레이어에게 전달할 의미와 `AudioRequirementStableId`만 남기고, 파일 출처·이용조건·hash·구현·청취 상태는 중앙 대장을 갱신한다.
- AI 생성 서비스나 자산 판매처는 교체 가능한 출처다. WI·H·상태 사본과 연결되는 의미 기반 `AudioCueCode`는 유지한다.
- 오디오 요구를 기록하거나 후보 파일을 만든 사실만으로 Presentation E를 승격하지 않는다.

## 전체 정리 상태

- [PlayableLoop 문답 정리 상태판](문답정리상태판.md)은 Q-001~339의 주제별 위치, 번호 예외, 실제 Evidence와 다음 공백을 요약한다.
- [Q-001~Q-198 주제·깊이 색인](Q001-Q198주제깊이색인.md)은 초기 문답을 주 상태 소유 주제와 `D1~D5`의 주 깊이로 전수 분류한다.
- [Q-001~Q-339 반영·구현 점검 원장](../../../AI/generated/playable-loop-inquiry-implementation-scope.md)은 각 질문을 정확히 한 번 포함해 `기획 기록 / 기획서 결속 / 구현 / 자동시험 / Runtime / Evidence`, 연결 대상과 다음 차단을 보여 준다. Q 번호는 대화 계보를 찾는 열람 순서이자 WI 후보를 빠짐없이 추출하는 `Q-001→Q-339` 첫 순회 순서다. 추출한 기존 WI와 신규 후보를 StableId로 정규화한 뒤에만 활성 PlayableUnit·WI 의존성을 선택한다. 구현은 `E7→E1` 영향 검토 후 가장 이른 미완료 책임에서 시작하되, Logic·Presentation 발견에 따라 그 E를 다시 여는 왕복 순환이며 질문 번호순 코딩이나 단일 `E1→E7` 통과가 아니다. 같은 원장의 `작은 구현 묶음`은 여러 Q를 한 WI·한 책임으로 합성하고 `Active / WaitingForApprovedRevision / ParkedCandidate / ImplementedParked / Completed` 상태를 분리하며, 활성 코딩 WIP를 최대 하나로 검사한다. 기계 단일 원본은 `eng/execution-ledgers/playable-loop-inquiry-implementation-scope.json`이며 문답 답변 전문을 복제하지 않는다.
- 최신 확인 기획 문답 `Q-340~Q-346`은 약초 회복 폐루프의 1인칭 탐색·채집, 자산 준비와 첫 물 확보를 정밀화한 범위다. Q-347은 물 운반 용기 질문으로 답변 대기다. 현재 Q-001~Q-339 구현 원장에는 아직 이관하지 않았으며, 승인 기획 revision과 원장 범위 확장 전에는 신규 WI 구현이나 E 승격 근거로 사용하지 않는다.
- 상태판은 답변 전문을 소유하지 않는다. 결정의 권위는 위 routing의 주제 문답 문서에 있다.

## 개발 뼈대 인계

- Q-001~Q-130의 주제별 코드 연결과 아직 실행 규칙으로 승격하지 않은 범위는 [Q-001~Q-130 개발 뼈대 인계](../../Q001-Q130개발뼈대인계.md)를 따른다.
- 문답 범위가 넓어져도 활성 Goal과 WI WIP는 각각 1을 유지하며, 공통 비실행 정책을 추가한 사실만으로 개별 PlayableLoop의 E를 승격하지 않는다.

## 호환 아카이브

- [기존 Nature Night Day2 통합 기록](nature-night-day2.inquiry.r1.md)은 Q-001~Q-075의 상세 조사와 당시 상태 사본을 보존하는 동결 아카이브다.
- 새 답변이나 질문은 아카이브에 추가하지 않는다.
- 기존 승인 문서가 아카이브의 특정 Q를 근거로 삼은 참조는 판본 호환을 위해 유지한다.
