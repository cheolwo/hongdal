# 문답 파일 데이터베이스

문답 원문을 옮기거나 새 DB 서버를 설치하지 않고, JSON 관계 색인으로 질문·주제·깊이·원문 절을 검색한다. 답변의 단일 원본은 주제 Markdown이며 JSON은 재생성 가능한 조회 자료다.

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
- 원문 표의 첫째/둘째 Q 열, Q 범위 묶음, Q 제목, Q로 시작하는 불릿을 발췌한다. `DirectExcerptAvailable`은 기록 위치를 찾았다는 뜻이며 전체 질문 본문 복구나 답변 완료를 뜻하지 않는다. Q272~274의 본문 소실/추측 금지도 그대로 검색된다.
- `HistoricalArchive`는 당시 상세 답변이다. 현재 주제 원문과 [결정 대장](../../docs/AI/DECISIONS.md)의 대체 관계를 읽고 현재 규칙으로 사용할지 판단한다. 최신 revision 숫자만으로 의미 충돌을 자동 해결하지 않는다.
- `SectionReviewLead`의 미정·보류 표시는 검토 후보다. 과거 미정이 뒤에서 해결됐을 수 있으므로 미답변으로 단정하지 않는다. 검색은 키워드 기반이며 의미상 중복을 완벽히 판정하는 AI 판정기가 아니다.
- `Unclassified` 깊이는 추정하지 않은 값이다. D 깊이와 실제 E는 별개이며 이 색인에는 E를 복제하지 않는다.

## 다음 질문 절차

기존 답변을 WI 의미로 정리할 때는 [문답 기반 보편 WI 계층 정리](../../docs/Architecture/PlayableLoops/문답기반보편WI계층정리.md)를 참고한다. 이 문서는 기존 관계 대장의 조회·검토 자료이며 원문·WI 실행 승인 원장을 대체하지 않는다. 새 질문은 [문답 정밀화 체계](../../docs/Architecture/PlayableLoop문답정밀화체계.md)의 약 10개 질문+추천 답안 검토 묶음을 기본으로 한다.

1. 후보 주제의 키워드와 기존 Q/의미 ID를 검색한다.
2. 현재 주제 원문·연결된 과거 답변·후속 결정을 비교한다.
3. 이미 답했다면 질문하지 않고 개발의 구현/증거 공백으로 돌린다.
4. 실제 미답변이나 새로 생긴 선택만 제시한다. 같은 세부 분야의 질문을 연속 세 개 넘기지 않는 기존 균형 원칙을 유지한다.
5. 사용자의 답을 원문에 반영한 뒤 색인 재생성·검증한다. 새 의미 ID는 원문에, 구현 원장 밖의 새 Q는 `supplements`에 등록한다. 승인된 개발 자료의 hash를 자동 변경하지 않는다.

새 DB 서버, SQLite 이중 원장, 문답 파일 이동, 생성 JSON 직접 편집은 필요 없다.
