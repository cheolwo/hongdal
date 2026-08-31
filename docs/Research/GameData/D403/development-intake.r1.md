# D403 개발 인수 — 제한 조사 참고자료

2026-08-31, `development-intake.d403.r1`. [전문 보고](README.md)·[구조자료](evidence.r1.json)·[제출 목록](manifest.r1.json)의 원바이트를 공유 폴더에 인수했다. 연구 참고자료의 인수이며 제품 반입·게임 규칙·RequiredAccepted 연구·E 승격은 아니다. 제출물의 `developmentAcceptance=Pending`은 원인계 당시 값으로 보존하고 개발 판정은 이 문서에 둔다.

## 원문과 독립 대조

- README `481EA8A212E77D81E5E5A98C2562E2719266A1C49C4771D55B2406A0ED84CA1D` / 12,849byte.
- evidence `C0B5E0EF10BC4CE533F8CBD60585A7EB5DF0EECF73F025B463D20A40336D8087` / 55,773byte.
- manifest `AA058E97D288816D9B49CFE8DB69870AC4DABBAD796C805CBF16D84330AB3FB9` / 595byte. 자기 제외 두 파일 검증이며 이 인수 문서는 포함하지 않는다.
- 개발은 전문 보고/구조자료를 읽고 제출3hash·길이를 대조했다. 전문의 최초 승인4hash와 후속 references 사본을 구별했다. D405/D406의 공유 문서 변경을 최초 조사 실패나 연구 범위 확대 근거로 쓰지 않는다.
- [문체부](https://www.mcst.go.kr/usr/child/cultureStory/season/custom.jsp?pTab=2)의 24명칭 순환, [우주항공청](https://www.kasa.go.kr/prog/bbsArticle/BBSMSTR_000000000010/view.do?bbsId=BBSMSTR_000000000010&nttId=B000000001860Pe2zT3)의 2026년 월력요항 발표/첨부 안내, [농사로 감자](https://www.nongsaro.go.kr/portal/ps/psb/psbl/workScheduleDtl.ps?cntntsNo=30699&menuId=PS00087&sKidofcomdtySeCode=FC)의 네 작형 표, [8월 목록](https://www.nongsaro.go.kr/portal/ps/psr/psrb/monthFdLst.ps)의 고추/풋콩, [해수부 발표](https://www.mof.go.kr/doc/ko/selectDoc.do?bbsSeq=10&docSeq=62991&menuSeq=971)의 선정월/새우류 표현을 개발이 직접 재열람했다. 문화자료의 개략 날짜·15일 표현은 정확 시각의 대체 근거로 채택하지 않았다.

## 인수 범위와 미확보

24명칭·식품6·기간16을 서로 다른 의미로 보존한다. 감자 8작기 구간과 기존 생육기간1, 고추/풋콩 목록2, 새우류 두 항목의 선정/제철4, 한치 선정1이다. 품목6은 상세 생육 프로필6이나 독립 선정6을 뜻하지 않는다. 감자만 기존 상품 식별자를 재사용하며 나머지 상품·학명·작형·지역 미확인은 null이다.

2026년 정확24시각·시간대/정밀도 원문, 직접/파생 품목 절기 대응, 현실→게임 환산은 미확보다. 연구가 기록한 KASI 오류를 반복하지 않았으며 로그인 필요로 바꾸지 않았다. 현재 공개자료 범위에서 사용자 인증조치 요청은 없다. 후속 실제 접근 제한은 [D404 운영](../../../Architecture/게임자료조사전문운영.md)에 따라 먼저 보고한다.

농사로/우주항공청 본문의 공공누리2유형 표시를 확인했으며 제품 이용 허락으로 확대하지 않는다. 해수부/문체부 개별 재배포 조건 미확인은 그대로다. 원자료 새 저장/사진 복사/첨부 다운로드·API/DB/Provider/키·새 상품/WI/H 등록/게임 적용은 없다. 전문 artifacts7개와 기존 표본은 자동 복사하지 않았다.

풋콩 `identityLimit`의 “Repeated 고추 list labels” 문구는 공통 목록 설명의 잔존이며 풋콩을 고추로 매핑하는 필드가 아니다. 원제출은 보존하고 두 항목의 정식 상품 연결은 모두 미확인으로 소비한다. 신규 대응 코드나 프로필을 만들 근거로 사용하지 않는다.

## 후속

개발 인수 검사51항목(제출hash/명칭·순환/null/자료참조·기존11파일 포함) 통과. 최종 관련16문서 로컬경로414/414, 범위26파일 Fast `artifacts/local/validation/20260831-120836` 통과(GuidanceOnly, 빌드/게임시험 생략). 결과는 `artifacts/local/validation/d405-inquiry-integration/intake-validation-final.json`이며 문서 경로 확인을 전체 웹·앵커 의미나 게임 동작 검증으로 확대하지 않는다.

기존 `product:potato`·농사로 Profile/Archive·Source Catalog와 승인 현실문맥 서비스는 재사용 위치다. 연구 JSON 자체가 Runtime 입력 형식은 아니며 자동 로드하지 않는다. Q089 문맥 선택, 달력/게임 길이와 시설 효과·두 Asked는 기존 기획 상태로 남긴다. 동일 자료를 다시 수집하지 않고 필요한 부분만 별도 승인 범위에서 보완한다. D406 실제 화면 작업 및 D396 독립 준비의 선행 차단으로 두지 않는다.
