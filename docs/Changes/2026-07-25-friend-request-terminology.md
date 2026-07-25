# 업무 관계의 친구 요청 용어와 설계 기준

## 결과

- 사용자 표시 용어를 `친구 요청`, `친구 수락`, `친구 관계`로 통일했다.
- 업무를 함께한 사실은 `친구 후보 기록`일 뿐이며 자동 친구 관계가 아님을 화면과 ViewModel 오류·완료 문구에 명시했다.
- `/community/relationships`에서 업무 수행, 친구 후보 기록, 친구 요청, 친구 수락과 별도 연락처 공개 순서를 보여 준다.
- Controller, Command, Handler, UseCase, Domain entity와 테스트의 활성 코드 명칭을 친구 요청 중심으로 리팩터링했다.
- 기존 `/api/v1/connections` route와 `WorkRelationshipSnapshot` contract는 유지했다.
- 운영 DB table·column, 공개 직렬화 필드와 Outbox Event 이름은 기존 consumer와 저장 데이터 호환을 위해 유지했다.
- [업무 경험에서 친구 요청으로 이어지는 커뮤니티 설계 기준](../Architecture/FriendRequestCommunityDesignStandard.md)을 단일 기준으로 추가했다.

## 화면

간접 확인 — Windows MAUI 대상 빌드와 UI 조립 테스트는 통과했지만 로컬 실행 프로세스가 대상 창 핸들을 만들지 않아 실제 PNG를 생성하지 못했다.

변경된 화면 문구:

- 관계 화면 제목 → `친구 요청`
- 업무 관계 기록 → `친구 후보 기록`
- 요청 동작 → `친구 요청 보내기`
- 업무 로그와 자동 친구 관계의 분리, 친구 수락 뒤 별도 연락처 공개 경계

## 확인

- Fast 영향 범위 build·targeted test 통과
- `SsalddelApp` Windows `net10.0-windows10.0.19041.0` 빌드: 경고 0개, 오류 0개
- 친구 요청 화면 조립, 익명 상대 표시, 친구 요청 API와 ViewModel 상태 테스트 통과
- 사용자 표시 문구와 활성 파일명을 저장소 전체에서 다시 검색해 잔여 항목 0건을 확인했다.
- Task 전체 검증은 동시 작업 중인 `ProduceRegionalPriceComparisonViewModelTests`의 모델 불일치 컴파일 오류로 중단되었으며, 친구 요청 변경과는 별도 범위다.
- 실제 MAUI 창 렌더는 창 핸들 미생성으로 미확인
