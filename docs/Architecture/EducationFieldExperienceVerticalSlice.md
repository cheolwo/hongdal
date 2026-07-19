# 교육 현장 체험 활동 수직 기능

## 목적

학생이 살뜰 안에서 수행한 활동을 학교에 제출할 수 있는 증빙 묶음으로 정리한다. 플랫폼은 활동 기록과 제출 과정을 지원하지만 출석 인정 자체를 확정하지 않는다. 최종 출석 인정 여부는 원장 학교 범위와 일치하는 선생님의 결정으로만 기록한다.

## 원장 중심 흐름

MongoDB `community_ledgers`의 `education-field-experience` 원장이 기준 정보다.

1. 학생이 활동 계획 원장을 만든다.
2. 학생이 실제 활동 시간, 수행 역할, 학생이 알고 있는 확인자, 선택 증빙을 기록한다.
3. 학교 밖 활동을 돕는 현장체험지도자가 지정된 경우 별도 확인 API로 실제 활동 여부를 확인한다.
4. 원장에 지정된 보호자가 승인 또는 거절한다.
5. 활동 기록, 보호자 승인, 지정 지도자의 확인이 완료되면 학교 제출을 요청한다.
6. 제출 요청은 MongoDB `education_field_experience_submissions` 대기열에 영속화한다.
7. 문서 방식은 수동 제출 준비 상태가 된다.
8. 이메일 또는 API 방식은 운영 설정이 활성화된 경우 Worker가 전송한다.
9. 선생님 또는 서버 관리자가 학교 결정을 기록한다.

원장 변경 이벤트는 기존 커뮤니티 원장 이벤트 파이프라인을 그대로 타므로 블록 관계와 상태 감사 투영에도 반영된다.

## 원장 블록

| 블록 | 의미 |
| --- | --- |
| `education-student-plan` | 학생 표시명, 학교식별Key, 학교명, 학년·반 |
| `education-activity-plan` | 목표, 장소, 예정 시간, 계획 활동, 학교 제출처 |
| `education-activity-record` | 실제 수행 시간, 역할, 확인 메모, 증빙 파일 URL |
| `education-guardian-approval` | 보호자 승인 여부와 의견 |
| `education-school-submission` | 제출 방식과 제출 요청 이력 |
| `education-school-decision` | 교육기관의 출석 인정 결정과 문서 번호 |

## API

| Method | Path | 책임 |
| --- | --- | --- |
| `POST` | `/api/v1/education/field-experiences` | 학생 활동 원장 생성 |
| `GET` | `/api/v1/education/field-experiences/{ledgerId}` | 참여자 또는 학교 검토자 조회 |
| `POST` | `/api/v1/education/field-experiences/{ledgerId}/activity-records` | 학생 활동 기록 추가 |
| `POST` | `/api/v1/education/field-experiences/{ledgerId}/guardian-approval` | 등록 보호자 승인 |
| `POST` | `/api/v1/education/field-experiences/{ledgerId}/activity-records/{activityRecordId}/field-verification` | 지정 현장체험지도자의 실제 활동 확인 |
| `POST` | `/api/v1/education/field-experiences/{ledgerId}/submissions` | 문서·이메일·API 제출 예약 |
| `POST` | `/api/v1/education/field-experiences/{ledgerId}/school-decisions` | 교육기관 출석 인정 결정 기록 |

## 보안 경계

- 모든 API는 인증이 필요하다.
- 학생 활동 기록은 원장 생성자만 추가한다.
- 보호자 승인은 원장에 지정된 보호자만 수행한다.
- 원장 조회는 참여자와 교육기관 검토자에게만 허용한다.
- `선생님`은 학교 제출 검토와 출석 인정 결정만 담당한다.
- 선생님의 토큰 `school_id` 또는 `교육기관Key`가 원장의 `학교식별Key`와 일치해야 조회와 결정을 할 수 있다.
- `현장체험지도자`는 학교 밖 실제 활동을 확인하지만 출석 인정 결정을 할 수 없다.
- 현장 확인은 원장에 지정된 `현장체험지도자`만 수행한다.
- 학교 결정은 `선생님` 또는 `서버관리자` 역할만 수행한다.
- API 제출 URL은 요청 본문에서 받지 않는다. 서버 설정의 제출처 Key로만 선택해 SSRF를 막는다.
- 외부 API는 HTTPS만 허용하며 로컬 개발 주소만 HTTP를 허용한다.
- 자동 전송은 기본적으로 꺼져 있다.
- 교육 원장의 블록 내용과 개인 식별 서술, 다이어그램 정보는 MongoDB에만 저장하고 RDB 범용 블록 관계로 복제하지 않는다.

## 운영 설정

`EducationSubmissions` 설정에서 SMTP와 기관별 API 목적지를 등록한다. 비밀 값은 추적되는 `appsettings.json`이 아니라 무시되는 로컬 설정 또는 운영 비밀 저장소에 둔다.

```json
{
  "EducationSubmissions": {
    "자동전송활성화": false,
    "조회주기초": 30,
    "최대시도횟수": 5,
    "Smtp": {
      "Host": "smtp.example.com",
      "Port": 587,
      "EnableSsl": true,
      "UserName": "...",
      "Password": "...",
      "FromAddress": "no-reply@example.com"
    },
    "Api제출처": {
      "school-key": {
        "Url": "https://school.example.com/api/field-experiences",
        "ApiKeyHeaderName": "X-Api-Key",
        "ApiKey": "..."
      }
    }
  }
}
```

## 다음 수직 보완

학교별 실제 서식의 PDF 생성, 보호자 전자서명 강도, 교육기관별 API 계약, 개인정보 보존·파기 기간은 기관 협의가 끝난 뒤 어댑터로 추가한다. 교육기관 연동이 없더라도 문서 방식으로 증빙 묶음을 만들 수 있도록 유지한다.
