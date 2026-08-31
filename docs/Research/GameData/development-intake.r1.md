# 첫 Farm 자료 묶음 개발 인수

- 판본: `game-data-research.development-intake.r1`, 2026-08-30.
- 판정: **조사 참고자료 인수 / 제품·실행 자료 채택 보류**. 원문7파일의 당시 Pending/개발 인수 대기 표기는 전문 제출 시점 이력이다. 이 문서는 별도 실행 대장이나 Approved 전문 연구가 아니다.
- 제출 담당: `01a0524f-72e7-75e1-986a-d149951e1240`; 개발 검토 후 기획 `01a025cf-0772-7251-b842-156a20e7483e`로 반환한다.

## 인수 파일과 바이트 계보

제출 루트는 `C:/Users/user/.codex/worktrees/ea96/Hongdal/docs/Research/GameData/`다. [목록](README.md), [첫 Farm](first-farm.r1.md), [재사용 재고](reuse-inventory.r1.md), [자료 목록](sources.r1.json), [기준선](baseline.r1.json), [제출 manifest](manifest.r1.json), [교역 사전 검토](trade-opportunity-next.r1.md)를 공유 저장소 같은 상대 경로에 연구 문서로만 반영했다. 원자료·임시 검증 파일은 복사하지 않았다.

제출 manifest SHA256 `C9FF7003AF75AE4FB75B55FD706D94E11FC9F5684D59C9CC19A61E4DCCE10DFE` 및 내부6파일 hash6/6은 원 작업트리에서 직접 일치 확인했다. 통합7파일은 LF 정규화 텍스트7/7 동일이며 아래2개만 CRLF→LF로 바이트 hash가 달라졌다. 나머지5개는 바이트 hash도 동일하다. 제출 manifest의 hash는 원 작업트리 바이트를 가리키며 통합본 검증값으로 조용히 바꾸지 않았다.

| 파일 | 제출 SHA256 | 통합 LF SHA256 |
| --- | --- | --- |
| baseline.r1.json | `6E0A2ABBC6B45F3C91B72E939E8649F91093324B79ADCDA0BB44196DA2FB8FCD` | `A3A521EBFCD8D95A49666C64CD40FC6D593B299A8BA84C345E56737CFBD975C8` |
| manifest.r1.json | `C9FF7003AF75AE4FB75B55FD706D94E11FC9F5684D59C9CC19A61E4DCCE10DFE` | `0B69BDB90CC20E3BB00CC2370561165E003A68FBAD8EF162F21E46248BC45A82` |

표본은 원 작업트리 `artifacts/local/game-data-research/rda-100000807551-growth-duration.fragment.html`의 1,892bytes/SHA `412AB3EE83AD834B48FE45508C8778363EA1BA4DA5DCEEC1E9A26CD1C4B4AC51` 일치만 읽기 확인했다. 전체 응답·API·사진·첨부의 hash가 아니다. 표본 내용/원자료는 공유 저장소에 반입하지 않았다.

## 기준선과 재사용 확인

배정 HEAD는 공유보다617커밋 오래된 조사 환경이며 그 코드나 설정을 통합하지 않았다. 전문이 기록한 현행 공유 참조41파일 중 개발 검토 시38개 동일, 차이3개는 DECISIONS/CURRENT_WORK/생존경제 문답이다. 문답 최신r3 SHA `3CCB7B180070B60D00F126891E2E4F27F1DB24C2C041ED89415737269AB8944C`와 D368을 대조했으며 최초r2 hash는 당시 기록으로 보존한다. 전문 마감 이후 개발의 CURRENT_WORK 추가 갱신도 전체 불변으로 보고하지 않는다.

개발이 실제 코드에서 농사로 조회의 `Unlinked`·`PendingHumanReview`·게시허용 false, 가격 비교의 직접비교/차액 false, RealityContext 파일 reader의 스키마·AreaSet·동결 경계를 재확인했다. 이번 sources JSON은 실행 카탈로그가 아니며 `CanPublishSimulationRule`/승인 상태를 변경하지 않는다. 기존 수집·Archive·QueryService·표준 품목 대응을 재사용하고 새 수집기·DB·키 조회를 만들지 않았다.

## 공식 출처와 사용조건 검토 수준

개발도 [농사로 일정30699](https://www.nongsaro.go.kr/portal/ps/psb/psbl/workScheduleDtl.ps?cntntsNo=30699&menuId=PS00087&sKidofcomdtySeCode=FC), [텃밭101612](https://www.nongsaro.go.kr/portal/ps/psz/psza/contentSub.ps?cntntsNo=101612&menuId=PS03172&sSeCode=335001), [농촌진흥청 보도자료](https://www.rda.go.kr/board/board.do?boardId=farmprmninfo&currPage=1&dataNo=100000807551&mode=updateCnt&prgId=day_farmprmninfoEntry&searchEDate=&searchKey=&searchOrgDeptKey=&searchOrgDeptVal=&searchSDate=&searchVal=)를 열람했다. 농사로 두 자료의 서로 다른 재식거리 문맥은 확인했으나 도구의 추출 텍스트만으로 공공누리 유형 표시를 재확인하지 못했다. 전문의 페이지 표시 확인 기록(2/4/1유형)을 독립 법률 검토 완료로 올리지 않는다.

따라서 30699/101612 제품 반입 보류를 유지한다. 1유형으로 보고된 보도자료도 출처 귀속·적용 범위·사람 검토 전 실행자료로 채택하지 않는다. 사진/첨부/API에 페이지 조건을 확대하지 않으며 권리 공백은 사용자 취향 선택으로 대신 해결할 수 없다.

## 기획 판단과 기술 후속의 분리

| 구분 | 반환 사항 | 현재 개발 영향 |
| --- | --- | --- |
| Q089 | 후속 현실 재식 프로필의 작형·지역·품종·방식과 선택 주체 | 기존 E5의 현실 간격/자연성장 재설계 제외 유지. 현 승인 개발을 막지 않음 |
| Q397 | 첫 체험10~15분과 현실 파종/출현~수확 일수는 다른 시간축 | 환산·배속·CropCare 변경 없음. 새 시간 규칙이 필요할 때 별도 명세 검토 |
| Q399 | 첫 품목/시장·국가/게임지역 대응·해석·갱신·상세 노출 범위 미정 | 기존 관측 표본/출처 URL·단위·권리 근거가 먼저 필요. 이름/국가/환율만으로 차익 계산 금지 |
| 기술 | 비교 DTO의 SourceKey/RecordKey에서 원문·수집Run·SlugId 계보 연결 및 비밀 없는 공개 URL 검증 | 현재 데이터 수/최신일/가격은 미조회. 기존 담당 제공 표본을 재사용하고 중복 수집하지 않음 |
| 권리 | 제한 자료의 허용 범위와 KAMIS/AMS 원문 재노출 조건 | 허락/이용조건 증거 확인 대상이며 새 보험 조사·현실 금융으로 확대하지 않음 |

이번 통합은 문서·JSON·hash·정적 코드 대조다. 게임 빌드/시험/Runtime/Save/GameView는 수행하지 않았고 E·실행 원장·승인 기획·Scene 변경과 commit/push가 없다. 추가 자료 수집은 자동 시작하지 않는다.
