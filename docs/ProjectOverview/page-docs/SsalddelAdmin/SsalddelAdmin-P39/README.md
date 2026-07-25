# SsalddelAdmin-P39 - 배차 AI 판단 사례

[전체 화면 문서](../../README.md) / [SsalddelAdmin 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

<img src="../../../assets/app-pages/SsalddelAdmin/SsalddelAdmin-P39.png" alt="SsalddelAdmin-P39 화면 캡처" width="720">

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | SsalddelAdmin |
| 페이지 ID / 제목 | SsalddelAdmin-P39 - 배차 AI 판단 사례 |
| 라우트 | /dispatch-ai-judgment-cases |
| 소스 파일 | [SsalddelAdmin/Components/Pages/DispatchAIJudgmentCases.razor](../../../../../SsalddelAdmin/Components/Pages/DispatchAIJudgmentCases.razor) |
| 분류 | 운영 |
| 2.0 운송 필수 연결 | 운영 보조 |
| 캡처 상태 | 완료 |

## 왜 필요한가

AI 배차 판단은 실제 운영자가 승인하거나 수정한 사례를 계속 축적해야 설명 가능성이 생긴다. 이 화면은 자동 생성된 후보 사례를 승인하거나 운영자가 직접 판단 사례를 입력해 RAG 검색에 반영하는 관리자 화면이다.

## 사용자와 참여자

주 사용자: 관리자, 운영자 / 보조 참여자: 배차 담당자, 정책 담당자

이 화면은 개별 운송을 배정하는 화면이 아니라, 배차 AI가 참고할 판단 사례 카탈로그를 관리한다.

## 화면에서 다루는 일

- 주 책임: 배차 AI 판단 사례 조회, 후보 승격, 직접 사례 입력
- 사용자가 확인해야 하는 것: 상황 요약, 판단 요약, 사용자 판정, 중용 판정, 키워드, 활성 여부
- 사용자가 조작해야 하는 것: 후보 사례 승격, 직접 사례 저장, RAG 반영 여부 선택
- 화면 밖으로 넘길 일: 실제 배차 승인과 운송 상태 변경은 국내화물/음식배달 AI 검토 화면과 운송 원장에서 처리한다.

## 다른 화면과의 관계

- 이전 화면: [SsalddelAdmin-P38 - 음식배달 AI 배차 검토](../SsalddelAdmin-P38/)
- 다음 화면: [SsalddelAdmin-P90 - 템플릿/샘플성 날씨 화면](../SsalddelAdmin-P90/)
- 함께 보는 화면: [SsalddelAdmin-P37 - 국내화물 AI 배차 검토](../SsalddelAdmin-P37/), [SsalddelAdmin-P38 - 음식배달 AI 배차 검토](../SsalddelAdmin-P38/), [SsalddelAdmin-P19 - 배차대기/추천 잠금 상태](../SsalddelAdmin-P19/)
- 상위 화면: 없음
- 하위 화면: 없음

국내화물과 음식배달 AI 검토 화면에서 나온 운영자 판단은 이 화면의 사례 카탈로그로 축적되고, 이후 AI 추천 근거 검색에 다시 사용된다.

## API 경로와 코드 연결

- 화면 소스: [SsalddelAdmin/Components/Pages/DispatchAIJudgmentCases.razor](../../../../../SsalddelAdmin/Components/Pages/DispatchAIJudgmentCases.razor)
- 클라이언트 서비스: [SsalddelAdmin/Services/DispatchAIJudgmentCaseAdminService.cs](../../../../../SsalddelAdmin/Services/DispatchAIJudgmentCaseAdminService.cs)
- 서버 계약: [Ssalddel.Contracts/Admin/Dispatch/DispatchAIJudgmentCaseDtos.cs](../../../../../Ssalddel.Contracts/Admin/Dispatch/DispatchAIJudgmentCaseDtos.cs)
- 서버 컨트롤러: [Ssalddel/Controllers/Admin/Dispatch/배차AI판단사례Controller.cs](../../../../../Ssalddel/Controllers/Admin/Dispatch/배차AI판단사례Controller.cs)

| 구분 | 메서드 | API 경로 | 클라이언트/문서 근거 | 서버 근거 |
| --- | --- | --- | --- | --- |
| 사례 카탈로그 | GET | `api/v1/admin/dispatch/ai-judgment-cases` | [DispatchAIJudgmentCaseAdminService.cs](../../../../../SsalddelAdmin/Services/DispatchAIJudgmentCaseAdminService.cs) | [배차AI판단사례Controller.cs](../../../../../Ssalddel/Controllers/Admin/Dispatch/배차AI판단사례Controller.cs) |
| 직접 사례 생성 | POST | `api/v1/admin/dispatch/ai-judgment-cases` | [DispatchAIJudgmentCaseAdminService.cs](../../../../../SsalddelAdmin/Services/DispatchAIJudgmentCaseAdminService.cs) | [배차AI판단사례Controller.cs](../../../../../Ssalddel/Controllers/Admin/Dispatch/배차AI판단사례Controller.cs) |
| 후보 사례 승격 | POST | `api/v1/admin/dispatch/ai-judgment-cases/promote-suggestion` | [DispatchAIJudgmentCaseAdminService.cs](../../../../../SsalddelAdmin/Services/DispatchAIJudgmentCaseAdminService.cs) | [배차AI판단사례Controller.cs](../../../../../Ssalddel/Controllers/Admin/Dispatch/배차AI판단사례Controller.cs) |

검증할 때는 후보 사례 목록, 직접 입력 폼, 저장/승격 버튼이 모두 렌더링되고, 메모리 데이터 모드에서도 문서용 샘플 사례가 표시되는지 확인한다.

## 보안과 개인정보 점검

판단 사례에는 지역, 기사 조건, 배차 실패 이유 같은 운영 민감 정보가 들어갈 수 있다. 운영 캡처에서는 특정 기사, 고객, 주소를 식별할 수 있는 표현을 샘플화하고, 사례 생성과 승격은 관리자 감사 로그 대상에 포함한다.

## 캡처와 문서 상태

현재 캡처는 개발용 관리자 인증 세션과 문서용 메모리 데이터를 붙여 실제 운영 화면까지 렌더링한 결과입니다.

이미지 파일을 다시 생성하면 이 README는 같은 경로의 이미지를 참조하므로 자동으로 최신 캡처를 보여줍니다.

## 보완 메모

- 사례 활성/비활성 정책이 생기면 이 문서의 화면 책임과 API 표를 갱신한다.
- 사례 품질 검수 흐름이 분리되면 하위 화면 또는 별도 운영 화면으로 문서화한다.
- RAG 검색 인덱스 반영 지연이 있으면 저장 후 사용자가 확인할 수 있는 상태 표시가 필요하다.
