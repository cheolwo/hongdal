# 4대보험 신고 준비 흐름

주문자 집단이 내부 입주민을 근로자로 고용하는 경우, 살뜰은 건강보험, 국민연금, 고용보험, 산재보험을 직접 신고하는 외부 API 클라이언트를 먼저 만들지 않는다. 초기 범위는 신고 대상 판정, EDI 또는 수기 제출 준비, 제출 결과 기록이다.

## 처리 원칙

- 실제 신고 제출은 국민건강보험 EDI, 국민연금 EDI, 고용/산재보험 토탈서비스, 4대사회보험 정보연계센터 같은 공식 채널에서 진행한다.
- 살뜰 서버는 근로계약, 주문자 집단 운영 주체, 사업자 검증 상태, 예상 근로시간을 기준으로 신고 준비 상태를 만든다.
- 월 근로시간, 계속근로 기간, 일용근로자 여부, 사업자/고용주 주체가 불명확한 경우에는 자동 제외하지 않고 `ManualReviewRequired`로 남긴다.
- 제출 이후 접수번호, 접수 상태, 반려 사유는 운영자가 기록한다.

## 주요 코드

| 항목 | 위치 |
| --- | --- |
| DTO | `Ssalddel.Contracts/Common/Hr/SocialInsuranceFilingDtos.cs` |
| 서비스 | `Ssalddel/Services/HumanResources/SocialInsuranceFilingService.cs` |
| 관리자 API | `Ssalddel/Controllers/Admin/HumanResources/사회보험신고Controller.cs` |
| 테스트 | `Ssalddel.Tests/Services/HumanResources/SocialInsuranceFilingServiceTests.cs` |

## API

| API | 용도 |
| --- | --- |
| `POST /api/v1/admin/hr-social-insurance-filings/assess` | 4대보험 신고 필요성 판정 |
| `POST /api/v1/admin/hr-social-insurance-filings` | EDI 또는 수기 제출 준비 플랜 생성 |
| `GET /api/v1/admin/hr-social-insurance-filings` | 신고 준비 플랜 목록 조회 |
| `GET /api/v1/admin/hr-social-insurance-filings/{id}` | 신고 준비 플랜 단건 조회 |
| `PATCH /api/v1/admin/hr-social-insurance-filings/{id}/status` | 제출/접수/반려 상태 기록 |

## 용어

| 용어 | 정의 |
| --- | --- |
| EDI | 공단 전자문서 신고 채널. 살뜰은 제출 준비 상태만 만들고 실제 제출은 운영자가 공식 채널에서 진행한다 |
| Manual | 수기 제출 또는 운영자 직접 확인이 필요한 흐름 |
| ManualReviewRequired | 자동 판정으로 제외하거나 신고 준비를 완료하기 어려워 노무/보험 검토가 필요한 상태 |
| Filing Plan | 신고 대상 판정 결과, 제출 채널, 접수번호, 접수/반려 결과를 묶어 둔 운영 기록 |
