# 문답 파일 데이터베이스

문답 원문을 옮기거나 새 DB 서버를 설치하지 않고, JSON 관계 색인으로 질문·주제·깊이·원문 절을 검색한다. 답변의 단일 원본은 주제 Markdown이며 JSON은 재생성 가능한 조회 자료다.

## D416 다섯 영역의 방향과 미답변 조회

D417/D418은 기존 원문의 명시적인 `의미 식별자 | 상태` 표를 읽는다. 일반 참조표·본문에 등장한 ID는 자동 등록하지 않고, 잘못된/누락 상태와 중복 ID는 거부한다. `ConfirmedDirection / FutureExtension`은 현재 방향 상태와 미래 구현이라는 한정을 같은 원문 행에 보존한다. Ready/E/권한으로 변환하지 않는다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id first-person-exploration-entry
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id perspective-scale-wi-classification
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id optional-auto-hunting-operations
./eng/planning-inquiries/manage-inquiry-search.ps1 -Wi -Id WI-FARM-04
```

[시점 대조 보고](../../docs/Reports/탐험운영시점-D417-기존구현대조-2026-08-31.md)는 D162의 기존3P 승인·D417 새 기본 방향과 WI5 사례를 분리한다. 시점 공통/특화는 중첩 가능하며 전용 판정이나 105WI 전수 구현을 자동 생성하지 않는다.

다영역 D416 방향은 `ConfirmedDirection`, Town 첫 참여는 후속 D428에서 `Asked`를 대체한 `Confirmed`로 조회한다. NPC 공방의 배움·소량 제작 참여만 확정됐으며 Recipe·비용·시간·실패와 실제 구현 완료는 아니다. 과거 r1의 미답변 이력은 보존한다. [다섯 영역 대조/첫 기술 후속](../../docs/Reports/다영역-병행개발-준비대조-2026-08-31.md)에서 기존 승인·코드·자산·실제 화면의 차이를 읽는다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id multi-area-choice-parallel-development
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id town-brewing-first-participation
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id town-brewing-reality-detail-first-purpose -OpenOnly
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id town-brewing-first-participation
```

## 파일 역할

- `sources.json`: 기존 구현 원장 참조, 보조 원문, 구현 원장 밖의 후속 Q에 대한 탐색 메타데이터. 실제 등록 범위는 이 파일과 생성 색인을 조회하며, 고정된 마지막 질문 번호를 별도로 관리하지 않는다. 답변 전문이나 구현 승인을 복제하지 않는다.
- `manage-inquiry-search.ps1`: 생성·신선도 검증·검색. 외부 서비스·DB·Unity 불필요.
- [생성 JSON](../../docs/AI/generated/planning-inquiry-search.json): `sources`(경로/hash), `questions`(번호/의미 ID·주제·깊이·원문 연결), `sections`(줄 번호·제목·검색용 원문 발췌).
- [집중 시험](../tests/planning-inquiry-search.ps1): 번호 보존·중복·미답변·누락 원문·오래된 색인·검색 필터를 검증한다.

## 사용

저장소 루트에서 실행한다. 키워드는 공백으로 나누며 모두 포함하는 결과를 찾는다. 질문 번호는 `Q059`, `Q-059` 모두 허용한다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Mode Write
./eng/planning-inquiries/manage-inquiry-search.ps1 -Mode Validate
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id Q059
./eng/planning-inquiries/manage-inquiry-search.ps1 -Text '중단 보존' -Topic nature-resource-construction
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id construction-cancel-material-preview
./eng/planning-inquiries/manage-inquiry-search.ps1 -OpenOnly -Text '운반 용기'
./eng/planning-inquiries/manage-inquiry-search.ps1 -Depth D4 -Limit 20
./eng/tests/planning-inquiry-search.ps1
```

검색 전에 모든 입력 hash와 재생성 결과를 대조한다. 원문/등록/도구 변경이나 색인 직접 수정은 `StaleOrModifiedIndex`로 차단하며 `Write` 후 `Validate`한다. 자동 검색은 어떤 기획·실행 원장도 수정하지 않는다. 파일 hash는 조회 당시 사본이며 병렬 작성 중에는 작성 완료 후 다시 생성한다.

## 해석 경계

- Q001~339는 기존 구현 원장의 분류·기본 Confirmed 및 질문별 재정의를 따른다. 실행 상태는 `implementationLookupRef`로 원장을 다시 읽는다. 질문 확정은 승인 기획·WI 구현·E 승격과 다르다.
- Q340 이후 후속 기록은 `supplements`와 원문을 함께 확인한다. 등록됐다는 이유만으로 답변 확정을 뜻하지 않으며 원문의 질문별 상태를 따른다. 새 답변 시 원문과 보조 메타데이터를 함께 갱신하고, 확인된 질문은 `OpenOnly`에서 제외한다.
- 원문에서 ‘질문 식별’ 뒤에 백틱으로 표시한 의미 ID 형태의 후속 질문도 추출한다. 기존 의미 ID와 전역 Q 번호는 재번호화하지 않는다.
- D403처럼 같은 절에 `- 조사·정리 방향 식별: `로 시작하는 별도 의미 ID가 있으면 그 행의 명시 상태만 읽는다. 주 질문의 상태를 물려받거나 본문의 역사적 언급을 현재 방향으로 등록하지 않는다. 같은 절을 공유하는 근거와 별개 질문 ID를 구분한다.
- 원문 표의 첫째/둘째 Q 열, Q 범위 묶음, Q 제목, Q로 시작하는 불릿을 발췌한다. `DirectExcerptAvailable`은 기록 위치를 찾았다는 뜻이며 전체 질문 본문 복구나 답변 완료를 뜻하지 않는다. Q272~274의 본문 소실/추측 금지도 그대로 검색된다.
- `HistoricalArchive`는 당시 상세 답변이다. 현재 주제 원문과 [결정 대장](../../docs/AI/DECISIONS.md)의 대체 관계를 읽고 현재 규칙으로 사용할지 판단한다. 최신 revision 숫자만으로 의미 충돌을 자동 해결하지 않는다.
- `SectionReviewLead`의 미정·보류 표시는 검토 후보다. 과거 미정이 뒤에서 해결됐을 수 있으므로 미답변으로 단정하지 않는다. 검색은 키워드 기반이며 의미상 중복을 완벽히 판정하는 AI 판정기가 아니다.
- `Unclassified` 깊이는 추정하지 않은 값이다. D 깊이와 실제 E는 별개이며 이 색인에는 E를 복제하지 않는다.

## 다음 질문 절차

기존 답변을 WI 의미로 정리할 때는 [문답 기반 보편 WI 계층 정리](../../docs/Architecture/PlayableLoops/문답기반보편WI계층정리.md)를 참고한다. 이 문서는 기존 관계 대장의 조회·검토 자료이며 원문·WI 실행 승인 원장을 대체하지 않는다. 새 질문은 [문답 정밀화 체계](../../docs/Architecture/PlayableLoop문답정밀화체계.md)의 D398에 따라 지금·여기·나·너·이렇게의 상황에서 핵심 질문 하나와 추천·대가를 제시하고 답변에 따라 심화한다. D353/D397의 10개 기본은 대체됐으며 과거 묶음 승인 해석은 보존한다. `harvest-ready-grace-window`는 Q378의 별도 의미 후속 `Asked`이며 추천·정확 시간은 미승인이다.

1. 후보 주제의 키워드와 기존 Q/의미 ID를 검색한다.
2. 현재 주제 원문·연결된 과거 답변·후속 결정을 비교한다.
3. 이미 답했다면 질문하지 않고 개발의 구현/증거 공백으로 돌린다.
4. 실제 미답변이나 새로 생긴 선택만 제시한다. 같은 세부 분야의 질문을 연속 세 개 넘기지 않는 기존 균형 원칙을 유지한다.
5. 사용자의 답을 원문에 반영한 뒤 색인 재생성·검증한다. 새 의미 ID는 원문에, 구현 원장 밖의 새 Q는 `supplements`에 등록한다. 승인된 개발 자료의 hash를 자동 변경하지 않는다.

새 DB 서버, SQLite 이중 원장, 문답 파일 이동, 생성 JSON 직접 편집은 필요 없다.

## 발전 군집·테크트리 기획 조회 — D402

[승인 의미 원문](../../docs/AI/문답기반-발전군집과테크트리-2026-08-31.md)의 두 의미 ID를 같은 `extraSources`/검색기에 연결한다. 별도 기술 DB·관계 스키마·UI는 만들지 않는다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id inquiry-progression-clusters
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id tech-tree-progressive-disclosure
```

첫 항목은 `ConfirmedDirection`, 두 번째는 `Confirmed`다. 큰 방향 선공개와 세부 발견·학습 공개를 구분하며 실제 학습/해금/사용 코드 승인으로 읽지 않는다. 원문의 기존 건물 테크트리·Q080/084/085·절기 문답 링크를 재사용한다. 현재 공간 관계의 포함/외형 지원을 기술 필수 선행·대안·병행 관계로 전용하지 않는다. 해당 관계의 새 기계 판독 구조는 별도 미배분이며 두 항목의 일곱 칸 대조는 `Unreviewed`다. 수확 여유와 부재 중 마법 보호의 `Asked` 상태는 유지한다.

## 한국 24절기·제철 조사 조회 — D403

[조사 계획](../../docs/AI/24절기-제철자료-조사와기획연결-2026-08-31.md)과 기존 절기 문답 내용r4를 같은 입력에서 읽는다. `-Id korean-24-solar-terms-planning`은 명칭·순서 기준 Confirmed, `-Id solar-term-seasonal-food-research`는 자료 조사 ConfirmedDirection이다. 현실의 제철/월별 추천/파종/수확/어획/출하는 서로 다르며 게임 내 절기 길이·가격·수확 성공으로 변환하지 않는다. 연구 자료의 실제 확보와 조회 등록·E/제품 적용은 별개다. 원천자료는 기존 GameData의 한정 연구 파일을 참조하며 이 검색기에 제철 권위 원장을 복제하지 않는다.

## 문답·H 공간·시각 근거 역조회

`sources.json.spatialRelationsRef`는 선택적 확장이다. 같은 생성 JSON 안의 `spatial`을 기존 검색기가 읽으며 별도 DB나 검색 서버는 없다.

- `spatial-relations.json`은 원문 대조를 마친 **관계·관점 해석의 단일 입력**이다. 답변 권위 원문을 대체하지 않는다. H 전목록·상하 포함·표현 지원은 기존 catalog/정의에서 생성하므로 여기 중복 작성하지 않는다.
- `spatial-query.ps1`은 기존 관리자의 내부 확장이다. 단독 실행기가 아니다. 질문의 원문 경로·hash·행·해당 Q 식별자와 이미지의 정확 manifest 항목·경로·hash를 검사한다.
- `QuestionRequiresRole`은 공간 역할 요구, `QuestionSupportsH`는 검토된 기존 H 지원 후보다. 질문이 해당 H ID의 승인·구현을 직접 확정했다는 뜻이 아니다. `SupportsRequirementCandidate`는 아직 미등록인 요구에 대한 기존 H의 지원 후보다.
- `ContainsRequired`/`ContainsOptional`은 정의에 직접 기록된 포함이다. 상위 경로의 `derived`/`edgeKinds`와 후보 연결의 `basisKind`를 보존한다. 실제 AreaSet의 H 연결이 없으면 이름으로 추론하지 않는다.
- `ExpressionSupports`는 기능 H1과 표현 카드의 지원 관계, `VisualEvidenceFor`는 특정 시점 자료다. 과거 H1 조립·공유 UI·개별 Prefab·공유 Game View를 구분하며 같은 이미지는 한 근거로 여러 H에 연결한다. 현재 정의 동등성·Runtime/E는 자동 확인하지 않는다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Spatial -Id Q143
./eng/planning-inquiries/manage-inquiry-search.ps1 -HId h1-stock:farm-production
./eng/planning-inquiries/manage-inquiry-search.ps1 -HId h2-candidate:highland-production
./eng/planning-inquiries/manage-inquiry-search.ps1 -Spatial -Text '급수'
./eng/planning-inquiries/manage-inquiry-search.ps1 -Gap NoImage -Limit 1000
./eng/planning-inquiries/manage-inquiry-search.ps1 -Gap UnreviewedH -Limit 1000
./eng/planning-inquiries/manage-inquiry-search.ps1 -Gap UnmappedRequirements
./eng/planning-inquiries/manage-inquiry-search.ps1 -Gap ReviewRequired -Limit 1000
```

`NoImage`는 이번 근거 목록에 이미지가 없다는 뜻이지 실제 조립·자산이 전 세계에 없다는 판정이 아니다. 후보 미등록과 H 신규 등록도 분리한다. 시각 원본은 승인된 Unity `artifacts/local` 경로만 읽는다. 그 경로가 없는 다른 환경에서는 근거 가용성/입력 오류를 숨기지 않으며 모바일 전달 성공으로 처리하지 않는다.

## 전체 기획의 네 조건과 순환 조회 — D391~D395

동일 관계 입력의 선택적 `circulation`에 **시간/공간/플레이어/대상/선택·WI/결과/다음 선택**을 결속한다. 앞 네 칸은 동시 문맥이며 순서대로 해야 할 행동이 아니다. 다음 선택은 여러 가능성·반복·중단 복귀를 문장으로 보존하고 자동 실행 순서를 만들지 않는다.

사람이 읽는 제목은 **지금(시간) / 여기(공간) / 나(플레이어) / 너(대상) / 이렇게(선택·WI) / 결과 / 다음 선택**이다(D394). 기존 JSON 필드·ID는 바꾸지 않는다. D395의 문서→코드/시험→자산 후보→Unity→결과 왕복은 같은 질문·기획 판본·WI의 기존 명세/근거를 대조하는 책임이다. 이 색인의 주제별 WI 문맥이나 공유 이미지는 실제 연결 완료 증거가 아니다. 현재 코드/시험·Prefab 적합성·Unity 입력의 질문별 정확 연결은 별도 미검증이며 조회 결과의 `implementationVerified=false`를 유지한다. 자산 조사는 E2 목록/E4 적합성, 실제 연결 E5·품질 E6·플레이 E7로 구분하고 직렬 강제나 일괄 실행을 하지 않는다.

- 모든 기존 Q·의미 ID는 같은 일곱 칸으로 조회한다. 대조하지 않은 칸은 `Unreviewed`다. `Undetermined`(원문 미정/미기재), `NotApplicable`(사유 있는 좁은 비적용), `EvidenceMissing`(소실 근거), `InterpretationProposal`과 합치지 않는다.
- `Explicit` 칸에도 세부 미정이 함께 적힐 수 있다. 칸 전체 완료 플래그가 아니며 생성 문서는 함께 기록된 미정도 검토 목록에 표시한다.
- `sourceDecisionState`와 기존 `indexDecisionState`를 보존한다. 질문별 관계 대조 수는 원문 전체의 모든 의미/현재 구현 검증 수가 아니다.
- 기존 PlayableUnit `planningGate` 목록과 `docs/Architecture/PlayableLoops` Markdown 전목록을 함께 조회한다. 질문 색인 밖 기획·연구·목록은 `InventoryOnly`이며 원문 재정리 완료가 아니다. 폴더 밖 모든 저장소 기획을 조사했다고 확대하지 않는다.
- `topicImplementationContext`의 WI/Loop는 기존 주제 범위다. 해당 질문 하나의 정확한 구현 연결이 아니므로 `TopicContextNotExactQuestionImplementation`을 유지한다. 실제 등록/증거는 원장을 다시 읽는다.
- D393의 Sky/LH·상태창·대상 안내는 원문 위치/hash를 가진 `DocumentationReferenceOnly`다. 모든 타이머를 Sky로 옮기거나 LH에 의미 설계 권위를 주지 않으며 실제 제품 상태 공급·소비는 미검증이다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id Q371
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Topic farm-barracks-defense -Limit 1000
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id Q385
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Text WI-FARM-04 -Limit 1000
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Unreviewed -Limit 1000
```

생성/검증은 저장소 루트에서 같은 입력 사본으로 실행한다. 기본 `Write`는 JSON만 갱신하며 두 문서도 갱신할 때 정확 경로를 전달한다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Mode Write -SpatialMarkdownPath docs/AI/generated/planning-inquiry-spatial-links.md -CirculationMarkdownPath docs/AI/generated/planning-inquiry-circulation.md
./eng/planning-inquiries/manage-inquiry-search.ps1 -Mode Validate -SpatialMarkdownPath docs/AI/generated/planning-inquiry-spatial-links.md -CirculationMarkdownPath docs/AI/generated/planning-inquiry-circulation.md
./eng/tests/planning-inquiry-search.ps1
./eng/tests/planning-inquiry-spatial-search.ps1
```

[공간 연결표](../../docs/AI/generated/planning-inquiry-spatial-links.md), [전체 기획 목차·주제별 순환·검토 공백](../../docs/AI/generated/planning-inquiry-circulation.md), [기술 검증·실제 조회 결과](../../docs/Reports/문답공간색인-개발구현-2026-08-31.md)를 함께 읽는다. 입력 hash·관계·도구·열거 파일의 추가/변경이 있으면 기존 신선도 검사를 다시 통과해야 한다. 원문 hash가 바뀌었다고 수동 검토 근거를 자동 갱신하지 않는다.

## D408 발견형 인과 기록 조회

D409의 `discovery-cue-weather-readability`는 현행 Confirmed이며 `-Id discovery-cue-weather-readability -OpenOnly` 결과는 0이다. r4 Asked는 이전 이력이다. 전투 날씨 활용은 FutureExtension이고 수확 여유/부재 중 마법 보호의 기존 Asked는 보존한다. 목록454개와 일곱칸 정밀대조65개는 별개이며 날씨 항목의 등록만으로 정밀대조·실제 구현을 완료하지 않는다.

기존 첫플레이 문답 내용r3의 두 의미 ID와 Q079/085/090/098/109를 같은 `circulation.reviews`에서 읽는다. 새 검색기·게임 단계·WI/H를 만들지 않는다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id discovery-led-play-causality
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id inquiry-causal-flow-reuse
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id Q090
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id Q109
```

기록01은 문서화한 첫 사례이고 필수 플레이 첫 순서가 아니다. 발견 단서→선택한 접근/관찰→달라진 판독→더 살피기/상호작용/돌아가기의 관계만 읽는다. D408 당시 춘분·숲은 예시였고 D411/412에서 숲 가장자리↔농장 외곽·춘분 무렵을 **기획 기준**으로 선정했다. 게임 시작일/낮·날씨/스폰·첫 자산은 여전히 미정이다. 표현 이동은 WI-NATURE-05 도끼 획득과 별개다. Q090 도구 정밀조사와 Q098 토양 상세 공개는 모든 사물의 자동해금으로 일반화하지 않는다. Q109는 다음 파종을 자동 실행하지 않는다. [당시 대조 보고](../../docs/Reports/D408-발견형인과기록-조회검증-2026-08-31.md)와 [전량 원문 대조](../../docs/Reports/D410-잔여문답-전량원문대조-2026-08-31.md)를 구분한다.

## D415 눈 없는 봄·춘분 현재 조회

[보완본 r3](../../docs/AI/북부춘분-굶주린농장발견-기획보완-2026-08-31.md)가 기존4+새4 의미ID의 단일 원문이며 첫플레이 r10은 링크만 한다. 현재466항목/875발췌다. 역할분담은 Confirmed로 변경됐고 눈 없는 봄은 Confirmed, 낮은 압박·작은 기여/식사·인벤토리 연결은 ConfirmedDirection이다. 식량 부족 원인·정확 비용/보상/기한·가방 규칙을 확정한 것이 아니다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id farm-encounter-spring-without-snow
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id hungry-farm-first-meal-role-split -OpenOnly
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id northern-spring-snow-conifer-survey
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id farm-meal-player-inventory-connection
```

역할분담 OpenOnly 결과는0이다. 눈 조사 행의 Confirmed는 D414 역사 상태로 남기되 D415 이후 추가 눈 조사·배치 준비 중단을 근거/관점에 표시한다. 조사 자료 삭제·새 게임 구현·E 완료로 해석하지 않는다. [D415 검증](../../docs/Reports/춘분의봄-D415-문답색인검증-2026-08-31.md)을 참고한다.

## D414 북부 춘분 후속 원문 조회 — 당시 이력

[북부 보완본 r2](../../docs/AI/북부춘분-굶주린농장발견-기획보완-2026-08-31.md)가 새 네 의미ID의 단일 원문이다. 첫 플레이 내용r9는 이를 링크하며 답변을 중복 등록하지 않는다. 기존458개와 새4개를 구분하여462개로 조회한다. 아래 D410 수치는 당시 완료 이력이다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id hungry-farm-npc-local-knowledge-help
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id hungry-farm-first-meal-role-split -OpenOnly
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id northern-spring-snow-conifer-survey
```

동결 r2에서 NPC 단서 Confirmed·침엽수/눈 파일조사 Confirmed·역할분담 Asked다. 이후 대화 답변은 다음 원문 판본에서 인수한다. `질문 식별`의 기존 호환은 보존하고 명시적 `- 의미 식별자:`도 읽으며, 단순 본문 언급이나 이름 일치만으로 새 공간 관계·H·WI를 만들지 않는다. [검증과 남은 연결](../../docs/Reports/북부춘분-D414-문답색인통합-2026-08-31.md)을 참고한다.

## D410 잔여 문답 전량 대조

시작 미검토389개를 기존65개와 겹치지 않게 원문 대조하고 D411~413 새 유입4개를 별도로 연결했다. 현재458개 모두 일곱 칸으로 조회되며, Q272~274 소실3개는 `EvidenceMissing`이다. `SourceCompared`는 읽기 완료이지 원문의 미정·해석 제안이나 구현 공백을 해소한 상태가 아니다. 문답 밖 기획39개는 목록 확보 범위이므로 모든 기획 의미 검토 완료로 확대하지 않는다.

동결 아카이브의 실제 답변은 해당 Q의 정확 `directExcerptRefs`와 일치할 때만 proof로 재사용한다. 파일hash·행·앵커·질문 신원 검사는 유지하며 생성 링크도 proof의 실제 원문을 연다. 같은 주제나 이름만으로 직접 관계를 만들지 않는다. [검토 범위·주제별 흐름·남은 선택](../../docs/Reports/D410-잔여문답-전량원문대조-2026-08-31.md)을 참고한다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id Q195
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Id Q272
./eng/planning-inquiries/manage-inquiry-search.ps1 -Circulation -Unreviewed -Limit 1000
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id spring-equinox-herb-crop-research
```

## WI 중심 읽기 — D396

같은 입력의 선택적 `wiView`는 공식 WI 대장과 Loop/명세를 읽는다. 새 WI 수기 목록이 아니며 문답을 삭제하거나 강제 WI 링크하지 않는다. [WI 전체 읽기](../../docs/AI/generated/planning-inquiry-wi-preparation.md)에서 일곱 칸·원문/코드 참조·주제 문답·H·기존 E4 후보/공백을 찾는다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Wi -Id WI-FARM-04
./eng/planning-inquiries/manage-inquiry-search.ps1 -Wi -Text '방문자' -Limit 1000
./eng/planning-inquiries/manage-inquiry-search.ps1 -Mode Write -WiMarkdownPath docs/AI/generated/planning-inquiry-wi-preparation.md
./eng/planning-inquiries/manage-inquiry-search.ps1 -Mode Validate -WiMarkdownPath docs/AI/generated/planning-inquiry-wi-preparation.md
./eng/tests/planning-inquiry-wi-view.ps1
```

WI의 기존 `implementation/integration`과 Loop의 `maturityTracks`는 별개다. Farm WI04 원장 E3/E6와 Loop LogicE3/PresentationE1/통합E1을 나란히 표시하며 합성하지 않는다. 작업 명세는 Loop 문맥이므로 그 내용을 모든 자식 WI의 완료로 복제하지 않는다. `ExistingCatalogProjectionNotFullMeaningReview`/`NotAssessed`는 자동 읽기 뷰의 검토 상한이다. 실제 준비 코드·시험과 적합성 완료는 개별 기술보고/명세로 추가 확인한다. 없는 후보·미등록/미승인 행위·미연결은 제외하지 않는다.

### D398~D401 단일 후속 조회

현재 절기 원문 내용r5의 `seasonal-landscape-appearance`와 `seasonal-spatial-engine-coordination`도 `-Id`로 조회한다. 두 항목은 ConfirmedDirection이며 같은 발췌를 공유하되 각 행의 명시 상태를 읽는다. LH 준비 표기·표현 hash와 실제 Renderer/LOD 색 변경은 별개다. [D405 파일 조사](../../docs/Reports/절기경관-D405-기존소비경로조사-2026-08-31.md), [D403 제한 자료 인수](../../docs/Research/GameData/D403/development-intake.r1.md)를 참고한다. D406 실제 화면 진단은 별도 실행 인계이고 문답 항목 수를 임의로 늘리지 않는다.

```powershell
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id harvest-ready-grace-window -OpenOnly
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id seasonal-time-context-direction
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id seasonal-sowing-outside-window
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id seasonal-greenhouse-protection-limit
./eng/planning-inquiries/manage-inquiry-search.ps1 -Id seasonal-magic-protection-unattended -OpenOnly
```

수확 여유/마법진 부재 중 보호 후속은 Asked다. 적기를 조금 놓친 파종은 D400, 첫 비닐하우스 보호 한계는 D401에서 Confirmed로 바뀌었으며 이전 Asked와 구분한다. 절기 문맥·시설 대응·계절별 야생 변화·마법 보호는 ConfirmedDirection이다. 새 Q 번호나 WI 등록·시간/가격 규칙은 만들지 않는다. 확정/방향 항목은 OpenOnly에서 제외하며 같은 절의 명시 상태 행 또는 방향 제목을 읽는다. 기존 Q089 보류·Q378 확정과 별개이고 승인 의미는 항상 원문을 우선한다.

## D439 기획 문서의 로컬 MySQL 정리

[기준 문서](../../docs/Architecture/기획판본-서버반입파이프라인.md)와 [첫 작성 예](../../docs/AI/기획판본-D439-기존벌목관계-첫로컬반입.md)를 사용한다. 같은 Markdown의 `planning-release` 블록에 작성자가 판단한 WI·객체·시각 관계를 적고 검사한다. 기존 검색기는 그대로 사용하며 이 도구는 자연어의 의미를 자동 결정하지 않는다.

PowerShell 7에서 저장소 루트를 작업 디렉터리로 사용한다.

```powershell
$document = 'docs/AI/기획판본-D439-기존벌목관계-첫로컬반입.md'
./eng/planning-inquiries/manage-planning-release.ps1 -Mode Validate -DocumentPath $document
$packet = ./eng/planning-inquiries/manage-planning-release.ps1 -Mode Write -DocumentPath $document | ConvertFrom-Json
./eng/planning-inquiries/manage-planning-release.ps1 -Mode Check -DocumentPath $document
./eng/tests/planning-release.ps1
```

Validate는 파일 쓰기0, Write는 `artifacts/local/planning-releases`의 불변 사본만 만들고 Check는 현재 문서와 다시 대조한다. 같은 ID/판본에 다른 내용을 덮지 않는다. 모든 명령은 이 단계에서 DB/HTTP 호출0이다. 원문 hash·WI/근거 참조·중복 키·형식/필수 항목·동일 판본 충돌을 검사한다. 출력 `localImport=NotReady`면 `gaps`를 먼저 읽으며 일부 관계를 몰래 생략해 저장하지 않는다.

**로컬 저장은 API 없이 기존 UseCase를 직접 호출한다.** 개발의 MySQL 쓰기 슬롯과 먼저 조율한다. 새 정의/자산 선택/DDL 없이 기존 WI 필드 근거와 기존 객체 정의 참조만 첫 반입을 지원한다. 일반 문서 표준은 더 넓지만 첫 저장 기능의 상한은 별도다.

```powershell
dotnet build eng/planning-inquiries/local-import/PlanningLocalImport.csproj --artifacts-path artifacts/local/validation/planning-release-d439/build
$importer = 'artifacts/local/validation/planning-release-d439/build/bin/PlanningLocalImport/debug/PlanningLocalImport.dll'
./eng/tests/planning-release.ps1 -LocalImporterPath $importer
$approvalHash = (Get-FileHash docs/Architecture/기획판본-서버반입파이프라인.md -Algorithm SHA256).Hash
$run = 'artifacts/local/validation/planning-release-d439/local-' + [guid]::NewGuid().ToString('N')
dotnet $importer preview (Get-Location).Path $packet.outputRef $packet.packetSha256 ($run + '/preview') $approvalHash
# 위 읽기 결과와 정확 DB 대상을 확인한 뒤에만 명시 저장한다.
dotnet $importer apply (Get-Location).Path $packet.outputRef $packet.packetSha256 ($run + '/apply') $approvalHash
```

preview는 기존 `hongdal-mysql-1`/`hongdal_dev`를 읽기 조회하고 apply는 기존 트랜잭션/멱등·재조회 경계로 관계를 추가한다. localhost:13306·Compose·비root 계정을 확인하며 접속 비밀/원 docker 출력은 로그나 문서에 넣지 않는다. `local-maintenance:planning-d439`는 명시 로컬 유지보수 주체이며 앱 로그인이나 원격 운영 권한이 아니다. 실행 결과는 별도 claim/result에 남기고 실패한 기존 경로를 덮지 않는다.

새 WI/객체 후보 등록·독립 Markdown 근거만의 저장·자산 구성/선택은 기존 해당 저장 기능의 후속 결속 대상이다. 저장된 관계는 편집 자료이며 실제 World/Session/게임 객체 생성이 아니다. 전체 105 WI나 전수 자산 저장 완료를 이 도구 성공으로 선언하지 않는다.
