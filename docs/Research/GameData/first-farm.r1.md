# 첫 Farm 자료 조사 및 개발 인계

판본 `game-data-research.first-farm.r1` / 2026-08-30 / D366 `game-data-research.workflow.r1`. 기준 파일과 hash는 [baseline](baseline.r1.json), 자료별 조건과 표본은 [목록](sources.r1.json)을 따른다.

## 결론과 확보 범위

기존 감자 조회·보관·품목 대응·Simulation 승인 자료 소비 경로를 재사용할 수 있다. 새 수집기는 필요하지 않다. 다만 **자료를 읽을 수 있다는 사실과 상업 게임에 반입할 권리는 다르다.** 조사한 농사로 2자료는 상업 이용 제한 때문에 제품 반입 보류다. 공개 열람 가능한 농촌진흥청 보도자료 1건에서 생육기간 문단만 확보했다.

공식 출처는 아래 3개 자료로 제한했다. 탐색 검색 결과는 추가 검증·확보 자료로 세지 않았다. 동일 보도자료의 짧은 URL은 web 도구에서 오류였고 검색 결과의 원래 URL로 열람·HTTP 200을 확인했다. PDF/HWP·이미지·API 응답·기상/가격 관측은 확보하지 않았다.

| ID | 공식 자료 / 기관 | 직접 확인 | 권리·반입 판정 |
| --- | --- | --- | --- |
| GD-FARM-001 | [감자 농작업일정 30699](https://www.nongsaro.go.kr/portal/ps/psb/psbl/workScheduleDtl.ps?cntntsNo=30699&menuId=PS00087&sKidofcomdtySeCode=FC), 농촌진흥청 농사로 | 제목·간격·생육·수확 설명 및 페이지 권리 표시 | 공공누리 2유형: 출처표시·상업 이용금지. 열람만, 원문 저장/제품 반입 보류 |
| GD-FARM-002 | [감자 텃밭가꾸기 101612](https://www.nongsaro.go.kr/portal/ps/psz/psza/contentSub.ps?cntntsNo=101612&menuId=PS03172&sSeCode=335001), 농촌진흥청 농사로 | 2009-06-08 등록, 간격·수확 설명·권리 표시 | 공공누리 4유형: 출처표시·상업 이용금지·변경금지. 열람만, 원문·이미지·첨부 반입 보류 |
| GD-FARM-003 | [봄재배 씨감자, 이렇게 관리하세요](https://www.rda.go.kr/board/board.do?boardId=farmprmninfo&currPage=1&dataNo=100000807551&mode=updateCnt&prgId=day_farmprmninfoEntry&searchEDate=&searchKey=&searchOrgDeptKey=&searchOrgDeptVal=&searchSDate=&searchVal=), 농촌진흥청 / 국립식량과학원 | 2026-02-09 보도자료, 본문·공공누리 1유형 표시, 소표본 HTTP 200 | 출처표시 조건의 텍스트 검토 후보. 사진/첨부/다른 농사로 API에 이 조건을 확대하지 않음 |

## 작물 식별과 사실 후보

기존 `product:potato`를 유지한다. KAMIS `100/152`는 기존 코드상 Confirmed, AMS `Potatoes`와 HS4 `0701`은 Candidate, 농사로 상품 코드 관계는 Unlinked다. 농사로 콘텐츠 `30699`와 작업군 `210005`는 각각 문서·분류 식별자다. 이번 공개 페이지 제목 확인은 공식 상품 crosswalk 승인이나 운영 DB의 현행 관계 조회가 아니다.

GD-FARM-001의 한 줄 파종은 휴간 70~80cm·주간 20~25cm, 두 줄은 휴간 90~140cm·열간 30~40cm·주간 20~25cm로 나뉜다. 생육 적온 14~23℃와 괴경 비대 적온 14~18℃를 구분한다. 수확의 관찰 기준은 잎·줄기 황변 단계다. 출현·비대기에 필요한 물과 성숙·수확기 배수 문맥이 다르므로 강수량을 관수 횟수로 바꾸지 않는다. 이는 자료에 있는 사실 후보이며 승인된 간격/성장 프로필이 아니다. [근거](https://www.nongsaro.go.kr/portal/ps/psb/psbl/workScheduleDtl.ps?cntntsNo=30699&menuId=PS00087&sKidofcomdtySeCode=FC)

GD-FARM-002의 간격은 이랑 사이 75~80cm·씨감자 사이 20~25cm다. GD-FARM-001과 범위를 합치거나 평균내지 않는다. 오래된 자료이고 겨울 파종 표기가 `1월하순~1월중순`으로 나타나 해석이 불명확하다. 겨울 달력·과거 약제명을 교정 추정하거나 현재 재배 지침으로 전파하지 않는다. [근거](https://www.nongsaro.go.kr/portal/ps/psz/psza/contentSub.ps?cntntsNo=101612&menuId=PS03172&sSeCode=335001)

GD-FARM-003의 봄감자 생육기간은 파종부터 수확까지 90~100일, 출현부터 수확까지 70~80일이다. 전국 중 고랭지 일부를 제외한 봄재배 문맥이며 품종 하나의 실측 시험값이 아니다. 게시일과 조회일은 관측일이 아니다. 이 문단은 게임 10~15분 목표로 환산하지 않는다. [근거](https://www.rda.go.kr/board/board.do?boardId=farmprmninfo&currPage=1&dataNo=100000807551&mode=updateCnt&prgId=day_farmprmninfoEntry&searchEDate=&searchKey=&searchOrgDeptKey=&searchOrgDeptVal=&searchSDate=&searchVal=)

## 표본과 검증 한계

- 실제 파일: `artifacts/local/game-data-research/rda-100000807551-growth-duration.fragment.html`, 1,892 bytes, 원 응답 내 생육기간 HTML p 요소 하나. 내용 편집 없음, 전체 HTML 응답은 저장하지 않음.
- 조회: `2026-08-30T19:58:37.6698006+09:00`; SHA-256 `412AB3EE83AD834B48FE45508C8778363EA1BA4DA5DCEEC1E9A26CD1C4B4AC51`.
- 해시는 확보한 부분 파일만 식별한다. 사이트 전체·첨부·원래 전체 응답의 hash가 아니다. 조회 경위는 같은 폴더 `acquisition.json`에 있다.
- 공개 HTML은 키 없이 열람했다. 기존 농사로 API는 코드상 키 필요, 실제 API/키/DB를 조회하지 않았다. 무료 열람 외 API 요금·쿼터·재배포 권리는 미확인이다.
- 권리 판단은 각 페이지의 표시 확인 수준이다. 제한된 자료의 사실 추출이 상업 제품에서 허용되는 범위, 별도 허락과 귀속 표시 방식은 미확인이다. 원문·표·도판을 제품에 복제하거나 자체 요약으로 제한을 우회하지 않는다.
- 기후값·실제 농장 상태·현행 관측 수·전체 카탈로그 품질·Runtime·Save·Game View를 검증하지 않았다. 기존 테스트 파일은 경계 확인 참고이며 이번 실행 성공으로 보고하지 않는다.

## 연결 Q/WI와 개발에 필요한 판단

Q088 공식 재식 근거 우선, Q089 작형·지역·계절·파종 방식에 따른 프로필 선택 보류와 연결한다. WI-FARM-02 파종의 후속 검토 후보이며 현재 WI-FARM-01에 간격 선택을 넣지 않는다. 생육·수확은 WI-FARM-03/04와 연결하되 기존 승인 기획 `farm-crop-cycle.design.r1`의 CropCare → HarvestReady와 Fixture 수확 규칙을 유지한다. 해당 기획은 현실 재식거리 신규 적용·자연 성장 재설계를 이번 범위에서 제외한다.

Q397의 10~15분은 첫 직접 체험 결과 확인 목표다. 현실 생육 일수와 별도 축이며 강제 타이머·성장 배속·수면 배속·작물 공통 성장시간 승인으로 해석하지 않는다. 기상은 Sky Observation → 승인 입력 상태 사본 → Simulation 대기 → 표현 경계를 유지한다. Farm은 동일 권위 강수와 별도 관수 효과의 소비자다.

개발 검토 요청은 다음으로 한정한다.

1. 기존 농사로 프로필의 `PendingHumanReview`와 `CanPublishSimulationRule=false`를 보존하고 본 조사 링크를 근거 후보로 연결할지 판단.
2. 2·4유형 자료는 상업 반입 보류로 표시. 1유형 표본도 출처 귀속과 사람 검토를 거쳐야 하며 자동 승인 플래그를 켜지 않음.
3. Q089에서 첫 작형·지역·품종·파종 방식과 프로필 선택 주체를 결정할 필요가 있음. 이 조사에서 결정하지 않음.
4. 새 시간/성장 규칙이 필요하면 Q397과 기존 E5 명세 차이를 기획으로 반환. 현재 승인 개발은 계속 가능.

약초·처방·약효는 후속 큐다. D367 거래기회는 [별도 후속 사전 검토](trade-opportunity-next.r1.md)로 분리했다. 새 수집기·게임 효과·공통 원장 수정·commit/push는 없다.

## 검증 절차

공식 3자료 열람과 표본 HTTP 200 확인 외에 문서 링크/참조 경로, JSON 파싱·고유 ID·3건 상한, 실제 표본 크기/내용/hash, 미확보 자료의 null hash, 산출물 hash, 새 파일 공백 검사를 수행한다. 실행 결과는 `artifacts/local/game-data-research/validation.json`에 둔다. 표본 폴더 자체의 로컬 `.gitignore`로 원자료와 임시 검증 파일을 stage 대상에서 제외했다.

배정된 오래된 worktree에는 현행 `eng/validate-changes.ps1`가 없어 이를 실행하지 않는다. 공유 저장소의 검증 스크립트는 허용 경로 밖에 산출물을 쓸 수 있으므로 호출하지 않고 위의 제한된 문서 검증으로 대체한다. 문서 전용 작업으로 빌드·게임 테스트·Editor 검증은 미실행이다. 공유 참조 hash는 검사 시점 사이 변경도 확인하되 다른 담당의 활동 중인 저장소 전체가 불변이라고 인증하지 않는다.
