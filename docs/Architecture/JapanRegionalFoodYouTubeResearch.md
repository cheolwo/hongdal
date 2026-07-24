# 일본 지역 음식 YouTube 조사

## 목적

일본 각지를 실제 여행하며 음식·시장·식당·지역 문화를 기록한 YouTube 영상을 `JP-01`~`JP-47` 도도부현 탐색 근거 후보로 수집한다. 영상은 지역 음식과 재료를 발견하는 사회적 문맥이며, MAFF GI·향토요리·생산통계나 관세청 실적을 대체하지 않는다.

영상 제목·설명·채널 소개처럼 공개된 metadata만 기본 수집한다. 영상 파일, 음원, thumbnail 원본이나 자막 전문은 저장하지 않는다. 자막·프레임 분석은 권리와 수집 허용이 확인되고 `YouTube:AutomaticIngredientRecognitionEnabled=true`인 경우에만 별도 검토한다.

## 1차 채널

조사 확인일은 2026-07-24다. 구독자 수와 조회 수는 자주 바뀌므로 카탈로그의 안정 필드로 저장하지 않는다.

| 채널 | 안정 식별자 | 언어 | 활용 초점 | 한계 |
| --- | --- | --- | --- | --- |
| [TabiEats](https://www.youtube.com/@tabieats) | `UC6Je3-ZV_x38NqQAxKiCCyQ` | 영어 | 일본 지역 음식, 편의점·판매 식품, 해외 시청자 반응 | 편의점 제품과 지역 생산품을 구분 |
| [しやごちゃんねる＠グルメ旅](https://www.youtube.com/@shiyago) | `UCL_ZYXcb07HM4wobK58PcEA` | 일본어 | 각지의 향토 음식·식당을 실제 방문한 먹거리 여행 | 식당 소재지를 원재료 산지로 해석하지 않음 |
| [日本列島まるっと旅](https://www.youtube.com/@marutto_tabi) | `UCXxYVUTiVq-_RDNt5VPGGug` | 일본어 | 지방도시와 현지 추천 음식·관광지 | 추천 표현은 공식 대표성 근거가 아님 |
| [たっちゃんねる](https://www.youtube.com/@boutacchan) | `UCYuVuLCtgrkQ6znA_ShdvXA` | 일본어 | 지역별 혼자 여행, 음식점·주점 방문 | 음주·외식 중심 영상은 수입 상품 후보와 분리 |
| [もーりーチャンネル](https://www.youtube.com/@morrytravel) | `UCfNVLQ0xYJyjHVEdlOyuHcQ` | 일본어 | 47개 도도부현·섬·비주류 여행지와 지역 음식 | 음식 전문 채널이 아니므로 보조 근거로 사용 |
| [Abroad in Japan](https://www.youtube.com/@AbroadinJapan) | `UCHL9bfHTxCMi-7vfxQ-AYtg` | 영어 | 도호쿠 중심 일본 지역 음식·문화와 해외 관심 | 연출·협찬·오락 요소를 공식 지역 설명과 구분 |

しやご의 공식 사이트는 게시 내용을 `먹고 마시며 돌아다니기`, 국내외 여행으로 설명하고 지역 관광기관과의 취재 사례를 공개한다. `日本列島まるっと旅`의 공식 채널 설명은 유명 관광지뿐 아니라 각 도도부현의 지방도시를 방문해 현지 추천 음식과 관광지를 소개한다고 명시한다. KADOKAWA는 もーりー 채널 운영자가 47개 도도부현과 국내 섬 100곳을 방문했고 각 지역의 음식도 다룬다고 소개한다.

## 교차검증 채널

독립 제작자 영상만으로 지역 대표성을 확정하지 않는다. 다음 채널은 발견 범위를 넓히는 교차검증 후보지만 독립 YouTuber seed와 분리한다.

| 구분 | 예시 | 역할 |
| --- | --- | --- |
| 지역 음식 방송 | [秘密のケンミンSHOW極](https://www.youtube.com/@kenmin_kiwami) | 도도부현별 음식명·지역 관행 후보 확인 |
| 지역 뉴스 | [日テレNEWS](https://www.youtube.com/@ntv_news) 및 지방 방송사 | 생산자·시장·지역 행사 보조 근거 |
| 관광기관 | 도도부현·시정촌 공식 관광 채널 | 장소·행사·공식 지역명 확인 |
| 정부 원천 | MAFF 향토요리·GI·전통식품 | 최종 지역·생산 범위와 공식 명칭 확인 |

방송 영상의 가격·판매 상태는 촬영 당시 값일 수 있다. 재편집·순위·AI 음성 채널은 실제 방문과 원출처가 확인되지 않으면 수집 채널로 승인하지 않는다.

## 수집과 지역 연결

```text
Channel ID
  → uploads playlist
  → 공개 video ID·제목·설명·게시시각
  → 도도부현/시정촌 명시 표현 후보
  → 음식·재료·제품명 후보
  → MAFF 향토요리/GI와 교차검증
  → 검토된 JP-01~JP-47 관계
  → 필요할 때만 HS 후보·무역통계 연결
```

1. handle은 표시·탐색용이고 영속 관계에는 channel ID를 사용한다.
2. 제목에 `京都`가 있어도 촬영 장소, 음식의 전승지역, 원재료 산지는 각각 별도 관계로 저장한다.
3. 음식점 방문은 `visited-place`, 음식명은 `food-mention`, 포장 상품은 `product-mention`, 생산자 취재는 `operator-mention`으로 구분한다.
4. 협찬·광고·제휴 표시는 수집 가능한 metadata 범위에서 함께 보존하고, 미확인은 미확인으로 남긴다.
5. 채널·영상·후보 수는 수요나 수입 가능성을 뜻하지 않는다. 상품 후보 승인과 공동구매 의향은 사람의 검토 뒤 별도 원장에 기록한다.

## API key와 quota

자동 수집에는 `YouTube:ApiKey`가 필요하다. tracked 설정에 넣지 않고 환경변수 `YouTube__ApiKey` 또는 secret store를 사용한다.

- `channels.list(forHandle=...)`로 handle을 안정 channel ID와 uploads playlist에 연결한다.
- 이후 검색 API 대신 uploads playlist의 `playlistItems.list`를 증분 조회한다.
- Google 공식 문서 기준 `channels.list`와 `playlistItems.list`는 요청당 quota 1이고, `search.list`는 별도 일일 100회 제한이 있으므로 신규 채널 발견에만 제한적으로 사용한다.
- tracked 기본값은 `YouTube:Enabled=false`다. 로컬 비밀 설정처럼 명시적으로 활성화한 환경에서만 동기화를 실행한다.

## JP 전용 일회성 동기화

전체 웹 서버와 다른 Quartz 작업을 시작하지 않고 기존 `IYouTube채널감시Service`를 호출하려면 다음 명령을 사용한다.

```powershell
dotnet run --project Ssalddel -- --sync-youtube-country=JP --YouTube:SeedFoodResearchCatalog=true
```

키는 명령행에 넣지 않고 로컬 비밀 설정에서 읽는다. 2026-07-24 실행에서는 일본 카탈로그 채널 6개와 최근 영상 120건을 조회해 기존 20건을 제외한 100건을 초기 기준선으로 저장했다. 같은 명령을 다시 실행했을 때 추가 저장은 0건이었다.

## 다음 조사 순서

1. 1차 채널의 최근 공개 영상에서 도도부현 명시율과 음식명 추출 정확도를 표본 검증한다.
2. 도도부현별 영상이 비는 지역은 지역 방송사·관광기관과 지역 거주 제작자로 보완한다.
3. 동일 음식이 여러 현에 등장하면 MAFF 전승지역·GI 생산범위를 확인하고 단일 지역으로 강제하지 않는다.
4. 포장 제품·생산자·시장 취재 영상만 별도 상품 후보로 보내고 식당 메뉴는 문화 탐색 후보로 유지한다.
