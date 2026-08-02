# 낮 업무·밤 알아차림 커뮤니티 세계지도

## 화면 변화

- `/community/home` 세계지도에 `낮 · 생활과 업무`와 `밤 · 알아차림과 성찰` 데이터셋 전환을 추가했다. 내부 route 호환을 위해 `night-learning` key는 유지하지만 화면은 학습 주제 선택보다 가벼운 알아차림을 먼저 표현한다.
- 낮 지도는 기존 지역 문화·가격·유통 관측을 업무 판단 근거로 유지한다.
- 밤 지도는 기존 `YouTube지식성찰채널Catalog`의 공개 학습 채널과 `ScriptureDecorationCatalog`의 경전·고전 공개 재생목록을 국가별로 보여 준다.
- 밤 모드는 `/community/home?dataset=night-learning`으로 공유할 수 있고 낮으로 돌아가면 질의가 제거된다.
- 밤 지도의 위치는 자료 제공 채널의 국가 맥락일 뿐 사상·경전의 기원, 신앙 분류나 우열을 뜻하지 않는다.
- 화면의 지도 renderer는 런타임 설정이 준비되면 Google Maps JavaScript API를 사용한다. 낮과 밤에 별도 지도를 만들지 않고 같은 지도 인스턴스의 Data 레이어와 스타일만 교체한다.
- Google 지도 키가 없거나 외부 API 로드에 실패하면 현재 SVG 개략도와 국가 선택 버튼을 그대로 표시한다.
- 분야 필터를 추가해 낮에는 `지역 문화`와 `가격·시장`, 밤에는 `생각·성찰 자료`와 `경전·고전 자료`를 한 지도 안에서 켜고 끌 수 있다. 원형과 마름모, 분야별 색을 함께 사용해 색상만으로 구분하지 않는다.
- `GET /api/v1/community/world-map/observations`가 `stableId`, 출처, 근거 확인 시각, 상세 경로와 안정적인 `revision`을 반환한다. 화면은 30초마다 현재 dataset을 다시 확인한다.
- 첫 snapshot은 즉시 연결하지만 이후 revision 변경은 지도 위치를 자동으로 움직이지 않고 `새 자료 지도에 반영` 대기열에 둔다. 사용자가 버튼을 눌렀을 때만 같은 지도 Data 레이어를 갱신한다.
- 서버 snapshot 연결이 실패하면 성공처럼 숨기지 않고 자동 갱신 실패를 알린 뒤, 첫 연결 전에는 내장 공개 카탈로그를, 연결 뒤에는 마지막 snapshot을 유지한다.

## Google 지도 운영 구성

- 브라우저 API 키는 source·tracked config에 저장하지 않는다. 배포 결과에 `globalThis.ssalddelRuntimeConfig.googleMapsBrowserApiKey`를 주입하거나 `ssalddel-google-maps-browser-key` meta 값을 배포 단계에서 주입한다.
- 운영 키에는 해당 Web origin의 HTTP referrer restriction과 Maps JavaScript API restriction을 함께 적용한다. 역할 WebApp별 origin이 다르면 키도 분리한다.
- Google Cloud project의 Maps JavaScript API 활성화와 billing 연결은 별도 운영 승인 작업이다. 이번 변경은 외부 API 활성화나 결제 설정을 수행하지 않았다.
- 런타임 키 값은 로그·화면 capture·변경 기록에 포함하지 않는다.

## 개인정보·추천 경계

- 종교·철학·국적을 사용자 점수, 신뢰 점수나 추천 순위에 사용하지 않는다. 지도에서 자료를 알아차린 사실도 관심 등록·동의·추천 신호로 간주하지 않는다.
- 공개 자료 연결은 진리·권위·공식 제휴를 보증하지 않는다.
- 밤 데이터셋은 읽기 전용이며 주문·계약·배차·자동 가입을 만들지 않는다.

## 실제 렌더링

WebApp과 공개 observation API를 함께 실행해 낮 snapshot 10건과 밤 snapshot 7건이 화면에 연결되는 것을 확인했다. 낮 지도에서 `가격·시장` 레이어를 끄면 observation 카드가 문화 자료 6건으로 줄었고, 밤 버튼을 누르면 `/community/home?dataset=night-learning`으로 이동해 `생각·성찰 자료`와 `경전·고전 자료` 레이어로 바뀌었다. 두 상태 모두 지도 container는 한 개, SVG 대체 지도도 한 개였고 브라우저 console 오류는 없었다. 이 capture는 Google 브라우저 키를 주입하기 전 SVG 대체 화면이며, 실제 Google base map 로드는 운영 키가 없어 미검증이다.

![분야별 레이어가 연결된 밤 알아차림 지도](../assets/changes/2026-08-02-day-night-community-map/layered-night-map.png)

## 설계 원본과의 관계

대상 Figma 파일·node가 현재 작업 문맥에 제공되지 않아 Figma에는 반영하지 않았다. 이번 변경은 실행 route, 기존 검토 카탈로그와 실제 렌더를 기준으로 기록한다.

## 검증

- `CommunityWorldMapHomeCompositionTests` 9개와 `커뮤니티세계지도조회UseCaseTests` 4개, 합계 13개 통과
- 테스트 실행 과정에서 `Ssalddel.Contracts`, `Ssalddel.Ui.Common`, `Ssalddel`, `Ssalddel.WebApp`, `Ssalddel.Tests` 빌드 성공
- 공개 API의 낮 10건·밤 7건 snapshot과 같은 자료의 안정 revision 확인
- 낮 레이어 필터, 밤 route 전환, 분야별 카드와 상세 근거 링크 직접 렌더 확인
- 단일 지도 container 1개, SVG fallback 1개, 브라우저 console error 0건 확인
- Google Maps 어댑터의 단일 container, 단일 map 생성, Data 레이어 교체, 런타임 키 경계와 source 내 키 부재를 구성 test로 확인
- Google Maps 실제 tile·Data 레이어 렌더는 승인된 브라우저 키가 없어 미검증
