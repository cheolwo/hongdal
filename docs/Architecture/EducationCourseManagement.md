# 교육과정 관리 모듈

## 조사 기준

2026-07-13 기준으로 아래 공개 자료를 확인했다.

- [홍익학당 교육과정 신청](https://hihd.imweb.me/68)
- [온라인 신사과정 안내](https://hihd.imweb.me/mentor002)
- [홍익학당 교육과정 신청서](https://docs.google.com/forms/d/e/1FAIpQLScip-w28uexEyFyTM2_WuKajREh7J_pNbdvhGFV4yGD0_Om-A/viewform)
- 온라인 신사과정 페이지에 첨부된 `수련체험기`와 `상담과제` PDF

홍달은 홍익학당과 공식 연동된 시스템이 아니다. `hongik-academy-shinsa-online` preset은 공개 자료에서 확인한 입력 구조를 서버 모듈로 옮긴 관리 초안이며, 실제 운영 전에는 해당 기관의 최신 동의문과 운영 규칙을 다시 확인해야 한다.

## 확인한 입력 구조

입교 신청서는 회원 가입 여부, 이름, 별명, 이메일, 전화번호, 성별, 출생연도, 해외 거주 국가, 입교서약 동의, 개인정보 수집 및 이용 동의, 개인정보 제3자 제공 동의를 받는다.

온라인 신사과정은 `참나각성`, `양심성찰`, `호흡수련`, `독서스터디`로 구성된다. 안내 페이지는 3개월 동안 과목별 3회 이상 참석하고 수련체험기를 3회 이상 제출하는 것을 심사 기준으로 제시한다.

수련체험기는 다음 영역을 기록한다.

- 참나각성 수련시간과 내용
- 현재 호흡 초수, 호흡수련 시간과 내용
- 양심성찰 사안
- 몰입, 사랑, 정의, 예절, 성실, 지혜에 관한 성찰
- 결론

상담과제는 성명, 별명, 작성일자, 과정명을 식별 정보로 두고 `아공`, `법공`, `구공` 필기 내용을 제출한다. 홍달에서는 성명과 과정명을 답변마다 반복 저장하지 않고 등록 관계로 식별한다.

## 저장 구조

| 테이블 | 책임 |
| --- | --- |
| `education_courses` | 과정 기본 정보와 운영 상태 |
| `education_course_subjects` | 과정별 과목과 최소 참석 횟수 |
| `education_course_forms` | 과정별 양식, 버전, 동적 필드 정의 |
| `education_course_applications` | 입교 신청, 동의 이력, 심사 상태 |
| `education_course_enrollments` | 승인된 참여자와 담당 멘토 |
| `education_course_attendances` | 과목별 회차 참석 기록 |
| `education_course_submissions` | 수련체험기와 상담과제 제출 및 확인 상태 |

이름, 별명, 이메일, 전화번호, 성별, 출생연도, 거주 국가, 심사 메모, 과제 답변과 확인 메모는 `IPersonalDataEncryptionService`를 거쳐 암호문으로 저장한다. 과정·과목·양식 메타데이터는 검색 가능한 일반 컬럼으로 유지한다.

## API 경계

공개 API는 활성 교육과정과 양식 정의만 조회한다.

- `GET /api/v1/education/courses`
- `GET /api/v1/education/courses/{과정코드}`

로그인 사용자는 입교 신청, 자신의 신청 조회, 진행현황 조회, 과제 제출과 개인정보 삭제를 수행한다.

- `POST /api/v1/education/applications`
- `GET /api/v1/education/applications/mine`
- `DELETE /api/v1/education/applications/{신청Id}/personal-data`
- `GET /api/v1/education/enrollments/{등록Id}/progress`
- `POST /api/v1/education/enrollments/{등록Id}/submissions`

서버관리자는 과정·과목·양식 정의와 신청 목록을 관리한다.

- `PUT /api/v1/admin/education/courses/{과정코드}`
- `POST /api/v1/admin/education/presets/hongik-academy-shinsa-online`
- `GET /api/v1/admin/education/applications`

`교육과정멘토`와 `서버관리자`는 신청 심사, 참석 기록, 과제 확인을 수행한다. 학교의 `선생님`과 외부 현장체험의 `현장체험지도자` 역할은 이 과정 운영 권한에 자동 포함되지 않는다.

- `PUT /api/v1/education/operations/applications/{신청Id}/review`
- `PUT /api/v1/education/operations/enrollments/{등록Id}/attendances`
- `PUT /api/v1/education/operations/submissions/{제출Id}/review`

## 양식 메타데이터 원칙

양식 필드는 `Key`, 라벨, 유형, 필수 여부, 최대길이, 표시순서, 섹션, 선택목록으로 정의한다. 서버는 제출 시 아래를 검증한다.

- 정의되지 않은 답변 Key 거부
- 필수 필드 누락 거부
- 숫자, 참거짓, 단일선택, 날짜, 이메일, 전화번호 형식 검증
- 회원 자격 확인과 필수 동의처럼 `true`여야 하는 참거짓 필드 검증
- 필드별 최대길이와 전체 답변 크기 제한

클라이언트는 이 메타데이터로 입력 UI를 동적으로 만들 수 있지만, API 경로와 권한은 서버 Controller에 고정한다. 양식 메타데이터가 임의의 API를 호출하도록 만들지 않는다.
