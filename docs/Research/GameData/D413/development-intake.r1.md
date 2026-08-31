# D413 개발 인수 — 춘분 식물·농작물 제한 자료

2026-08-31. [전문 보고](README.md)·[구조 근거](evidence.r1.json)·[제출 manifest](manifest.r1.json)를 세 파일 그대로 인수했다. 전문 파일의 `developmentAcceptance=Pending`은 제출 당시 상태로 보존하며 개발 인수는 이 문서에서 별도로 기록한다. 게임 입력·새 상품/종/WI 등록이 아니다.

## 검증한 범위

- 제출3파일 SHA/길이 및 manifest 자기제외2행 일치, UTF-8/LF 바이트 그대로 복사했다. 개발이 공유참조18개와 전문 worktree 기존28파일을 다시 대조했다.
- source4/item3/claim15의 ID22개 중복·참조 및 기간/정확날짜 null·미승인 게임해석을 확인했다. 직접/파생 춘분대응0, 약용분류 입증0, Prefab 종 연결0, 새 원자료/사진 저장0이다.
- 개발도 공식4페이지 추출본문을 열었다. [웹진 달래](https://rda.go.kr/webzine/2022/04/sub4-1.html)의 이른봄 서식 설명과 [NCPMS 달래](https://ncpms.rda.go.kr/npms/WeedsInfoSearchDtlR.np?wdCode=W00000056)의 학명·4~5월 개화·서식지는 별개 근거이며 통칭 간 종 동일성은 미확정이다.
- [농사로 감자](https://www.nongsaro.go.kr/portal/ps/psb/psbl/workScheduleDtl.ps?cntntsNo=30699&menuId=PS00087&sKidofcomdtySeCode=FC)의 작형별 파종/수확 표를 재대조했다. [농사로 완두](https://www.nongsaro.go.kr/portal/ps/psz/psza/contentSub.ps?cntntsNo=228670&menuId=PS03172&sSeCode=335001&totalSearchYn=Y)는 추출 목록과 연속 설명문 배열 차이를 확인했고 원문을 교정하지 않았다. 그림 자체 판독은 하지 않았다.
- 감자/NCPMS의 2유형, 완두의 4유형 이용표시와 웹진 개별조건 미확인을 분리한다. 목적별 법률 판단이나 상업 제품·사진·원문 반입 허가가 아니다. 공개 본문 확인 중 인증조치 요구는 없었으며 API/계정/키는 사용하지 않았다. 웹 도구의 캐시일 수 있어 새 원서버 바이트 확보로 표현하지 않는다.

근거는 `artifacts/local/validation/d410-circulation-review/d413-intake.json`이다. 문서 범위 Fast/Task는 D410 통합 보고에서 함께 기록한다. 게임 코드/Runtime/Editor 검증0.

## 후속 북부·추위·잔설 조건

동결 D413 이후 사용자가 말한 **북부·추위·잔설의 숲 가장자리/농장**은 이후 기획 조건이다. 정확 지역·고도는 미정이며, 중부/남부 또는 평난지 봄 작기를 그 장면에 즉시 적용하지 않는다. 굶주린 NPC·사냥/채집 선택을 이 연구가 승인하거나 구현한 것으로 해석하지 않는다. 기존 연구를 폐기/소급 재작성하거나 새 대량수집하지 않았다.

다음 기획에서 가상 지역/고도·노지/시설 문맥, 관찰 식생과 작업 가능한 작물, 통칭-종 확인수준을 정하면 필요한 차이만 조사한다. 기존 감자 Identity/Profile·210005 작업군·30699 콘텐츠·공식품목코드 Unlinked와 Q089 미정은 보존한다. 현실 생육기간을 게임10~15분으로 바꾸지 않는다.
