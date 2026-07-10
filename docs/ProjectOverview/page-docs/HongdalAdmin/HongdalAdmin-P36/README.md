# HongdalAdmin-P36 - 연락처 통합 검색

[전체 화면 문서](../../README.md) / [HongdalAdmin 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

캡처 대기: `docs/ProjectOverview/assets/app-pages/HongdalAdmin/HongdalAdmin-P36.png`

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | HongdalAdmin |
| 페이지 ID / 제목 | HongdalAdmin-P36 - 연락처 통합 검색 |
| 라우트 | /contact-search |
| 소스 파일 | [HongdalAdmin/Components/Pages/ContactSearch.razor](../../../../../HongdalAdmin/Components/Pages/ContactSearch.razor) |
| 분류 | 운영 |
| 1.0 필수 연결 | 운영 보조 |
| 캡처 상태 | 캡처 대기 |

## 왜 필요한가

관리자가 전화번호 뒤 8자리만 알고 있을 때, 해당 번호와 연결된 사람이 기사인지, 화주인지, 창고 관리자 또는 창고 관계자인지 빠르게 확인하기 위해 필요하다. 현장 문의, 상차 담당자 부재, 하차 담당자 부재, 정산 문의처럼 사람을 먼저 찾아야 하는 운영 상황을 줄이는 보조 화면이다.

## 사용자와 참여자

주 사용자: 관리자, 운영자 / 보조 참여자: 기사, 화주, 창고 관리자, 주문자

이 화면은 운송 상태를 직접 전진시키지 않는다. 대신 여러 역할에 흩어진 연락처와 관련 원장을 한 화면에서 모아, 다음에 어느 업무 화면으로 이동해야 하는지 판단하게 한다.

## 화면에서 다루는 일

- 주 책임: 전화번호 뒤 8자리 기준 사용자·역할·관련 원장 통합 조회
- 사용자가 확인해야 하는 것: 사용자명, 역할, 연락처 출처, 기사 정보, 화주 요약, 창고 참여, 최근 의뢰
- 사용자가 조작해야 하는 것: 전화번호 뒤 8자리 입력과 검색
- 화면 밖으로 넘길 일: 기사 상태 변경, 의뢰 상태 변경, 정산 처리, 문서 검수는 이 화면에서 처리하지 않고 해당 운영 화면으로 넘긴다.

## 다른 화면과의 관계

- 이전 화면: [HongdalAdmin-P35 - 보조 기능 설정](../HongdalAdmin-P35/)
- 다음 화면: [HongdalAdmin-P90 - 템플릿/샘플성 날씨 화면](../HongdalAdmin-P90/)
- 함께 보는 화면: [HongdalAdmin-P32 - 기사 목록/관리](../HongdalAdmin-P32/), [HongdalAdmin-P33 - 파트너 관리](../HongdalAdmin-P33/), [HongdalAdmin-P22 - 운송 상세 원장](../HongdalAdmin-P22/)
- 상위 화면: 없음
- 하위 화면: 없음

연락처 검색 결과에서 기사로 확인되면 기사 목록/운행 현황을 보고, 화주로 확인되면 의뢰 상세 또는 파트너 관리로 이동한다. 창고 참여가 확인되면 창고/입고/출고 관련 화면에서 후속 확인을 한다.

## API 경로와 코드 연결

- 화면 소스: [HongdalAdmin/Components/Pages/ContactSearch.razor](../../../../../HongdalAdmin/Components/Pages/ContactSearch.razor)
- 클라이언트 서비스/계약: [HongdalAdmin/Services/I백오피스Service.cs](../../../../../HongdalAdmin/Services/I백오피스Service.cs), [HongdalAdmin/Services/백오피스조회Service.cs](../../../../../HongdalAdmin/Services/백오피스조회Service.cs), [HongdalAdmin/Services/백오피스메모리Service.cs](../../../../../HongdalAdmin/Services/백오피스메모리Service.cs), [HongdalAdmin/Services/백오피스응답Dtos.cs](../../../../../HongdalAdmin/Services/백오피스응답Dtos.cs)
- 서버 계약: [Hongdal.Contracts/Admin/Management/관리자연락처검색Dtos.cs](../../../../../Hongdal.Contracts/Admin/Management/관리자연락처검색Dtos.cs)

| 구분 | 메서드 | API 경로 | 클라이언트/문서 근거 | 서버 근거 |
| --- | --- | --- | --- | --- |
| 클라이언트 서비스 | GET | `api/v1/admin/contact-search?phoneLast8={phoneLast8}` | [HongdalAdmin/Services/백오피스조회Service.cs](../../../../../HongdalAdmin/Services/백오피스조회Service.cs) | `GET api/v1/admin/contact-search` [Hongdal/Controllers/Admin/06_관리/관리자연락처검색Controller.cs](../../../../../Hongdal/Controllers/Admin/06_관리/관리자연락처검색Controller.cs) |
| 서버 Query | - | - | [Hongdal/Application/Admin/Management/Queries/관리자연락처검색Query.cs](../../../../../Hongdal/Application/Admin/Management/Queries/관리자연락처검색Query.cs) | [Hongdal/Application/Admin/Management/Handlers/관리자연락처검색QueryHandler.cs](../../../../../Hongdal/Application/Admin/Management/Handlers/관리자연락처검색QueryHandler.cs) |

검증할 때는 `010-1111-2222`처럼 전체 번호를 넣어도 숫자만 추출해 뒤 8자리로 검색되는지, `11112222`처럼 뒤 8자리만 넣어도 같은 결과가 나오는지 확인한다.

## 보안과 개인정보 점검

이 화면은 개인정보 조회 성격이 강하므로 서버 관리자 권한 안에서만 열려야 한다. 전화번호 전체가 아니라 뒤 8자리를 검색키로 쓰되, 결과에는 실제 연락처가 표시될 수 있으므로 관리자 인증, 접근 로그, 캡처 마스킹 기준을 함께 확인한다.

캡처를 남길 때는 실제 고객 연락처, 주소, 사업자번호가 노출되지 않도록 개발용 샘플 데이터 또는 마스킹된 데이터를 사용한다.

## 캡처와 문서 상태

현재 화면은 코드와 API가 추가되었고, 문서에는 캡처 예정 경로만 남겨 둔다. 다음 캡처 작업에서 `HongdalAdmin-P36.png`를 생성하면 이 README의 화면 캡처 섹션을 인라인 이미지로 바꾼다.

## 보완 메모

- 운영 데이터가 커지면 전화번호 정규화 컬럼과 인덱스를 추가하는 것을 검토한다.
- 연락처 조회는 감사 로그 대상이므로, 관리자 활동 로그와 연결할 수 있는지 후속 검토한다.
- 1.0 필수 운송 흐름을 직접 전진시키는 화면은 아니지만, 현장 문의와 예외 복구를 돕는 운영 보조 화면으로 관리한다.
