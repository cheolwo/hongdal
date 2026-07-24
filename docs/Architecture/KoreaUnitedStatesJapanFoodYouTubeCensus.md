# 한국·미국·일본 음식 YouTube 채널 전수조사

## 목적과 범위

조사 기준일은 2026-07-24다. 공개 YouTube Data API는 특정 국가의 모든 채널을 열거하는 기능을 제공하지 않으므로 여기서 `전수조사`는 다음의 재현 가능한 후보군 조사다.

1. 국가별 8개 음식 검색식을 `type=channel`, 음식 topic, 지역·언어 조건으로 실행한다.
2. 검색 결과를 channel ID로 국가 안에서 중복 제거한다.
3. `channels.list`의 공식 channel ID, handle, uploads playlist, 채널 국가 표기를 확인한다.
4. uploads playlist의 최근 1~3개 공개 영상 제목과 게시일로 음식 관련성과 활동성을 표본 검증한다.
5. 지역 코드만 일치하거나 다른 국가 채널, 일반 예능, 재편집·자동 생성 의심 채널은 자동 승인하지 않는다.
6. 운영에 유용한 채널만 정적 카탈로그에 승격하고 나머지는 후보군으로 남긴다.

YouTube API의 `regionCode`는 해당 국가에서 볼 수 있는 검색 결과를 뜻하며 채널 운영자의 소재국을 보장하지 않는다. 채널의 `snippet.country`도 운영자가 입력하지 않으면 비어 있으므로 언어, 설명, handle, 최근 영상과 함께 판단한다.

## 검색식과 후보 규모

| 국가 | 언어 | 검색식 |
| --- | --- | --- |
| KR | `ko` | `한국 음식 요리`, `한국 맛집 여행`, `한국 길거리 음식`, `먹방 음식 리뷰`, `한국 요리 레시피`, `전국 맛집 여행`, `식품 리뷰 신제품`, `수산물 육류 요리` |
| US | `en` | `American food travel`, `USA regional food`, `American cooking channel`, `street food USA`, `American regional cuisine`, `American restaurant travel show`, `food science cooking`, `meat seafood cooking USA` |
| JP | `ja` | `日本 グルメ 旅`, `日本 郷土料理`, `日本 食べ歩き`, `日本 料理 チャンネル`, `日本 ご当地グルメ 旅`, `日本 食品 レビュー`, `日本 魚料理`, `日本 食文化` |

각 검색식의 상위 25개를 수집했다. 국가 간에는 같은 채널이 중복될 수 있으므로 아래 고유 후보 수를 세 국가 전체 합계로 해석하지 않는다.

| 국가 | 검색 결과 슬롯 | 국가 내 고유 channel ID | 채널 국가 일치 | 다른 국가 표기 | 국가 미표기 |
| --- | ---: | ---: | ---: | ---: | ---: |
| KR | 200 | 190 | 129 | 18 | 43 |
| US | 200 | 175 | 85 | 36 | 54 |
| JP | 200 | 172 | 120 | 5 | 47 |

검색과 검증에는 기존 로컬 `YouTube:ApiKey`를 사용했다. 키 값은 source, tracked config, 명령행과 조사 문서에 기록하지 않는다.

## 운영 카탈로그 결과

이번 조사 전 카탈로그에 있던 한·미·일 채널에 활동성과 역할이 확인된 32개를 추가했다. 현재 운영 카탈로그는 KR 21개, US 22개, JP 18개다. 다른 국가 항목까지 포함한 전체 카탈로그는 66개다.

### 한국 신규 10개

| 채널 | 역할 | 최근 업로드 확인 |
| --- | --- | --- |
| [찐푸드 JJin Food](https://www.youtube.com/@jjinfood) | 길거리 음식·대량 조리·생산 현장 | 2026-07 |
| [끼룩푸드](https://www.youtube.com/@seagullfood) | 길거리 음식·지역 외식 생산 | 2026-07 |
| [트래블 푸드](https://www.youtube.com/@travelfood2981) | 전통시장·지역 음식 여행 | 2026-07 |
| [애주가TV참PD](https://www.youtube.com/@참) | 가공식품·주류 안주·신제품 리뷰 | 2026-07 |
| [[윤이련]50년 요리비결](https://www.youtube.com/@50food) | 전통 한식·저장 음식 | 2025-12 |
| [흑백리뷰](https://www.youtube.com/@흑백리뷰) | 국내외 가공식품 비교 | 2026-07 |
| [이 남자의 cook](https://www.youtube.com/@cook5162) | 가정식·계절 재료 | 2026-07 |
| [한식푸우](https://www.youtube.com/@koreanfood99) | 한식·배달음식 소비 반응 | 2026-07 |
| [수부해TV](https://www.youtube.com/@subuhae) | 제철 수산물·손질·조리 | 2026-07 |
| [EBS 최고의 요리비결](https://www.youtube.com/@ebs_best.cooking.secrets) | 전문가 조리법 교차검증 | 2026-02 |

### 미국 신규 10개

| 채널 | 역할 | 최근 업로드 확인 |
| --- | --- | --- |
| [Nick DiGiovanni](https://www.youtube.com/@nickdigiovanni) | 세계 음식·대중 조리 | 2026-07 |
| [Tasty](https://www.youtube.com/@buzzfeedtasty) | 다문화 조리법·가공식품 | 2026-07 |
| [Guga Foods](https://www.youtube.com/@gugafoods) | 육류·숙성·조리 실험 | 2026-07 |
| [Tasting History](https://www.youtube.com/@tastinghistory) | 역사 음식·식문화 | 2026-07 |
| [Cowboy Kent Rollins](https://www.youtube.com/@cowboykentrollins) | 미국 남서부·BBQ | 2026-07 |
| [Townsends](https://www.youtube.com/@townsends) | 미국 초기 식문화·보존식 | 2026-07 |
| [Miss Mina](https://www.youtube.com/@missminaoh) | 도시·지역 음식 여행, 협찬 표본 | 2026-06 |
| [Ethan Chlebowski](https://www.youtube.com/@ethanchlebowski) | 조리과학·가격·대체재 | 2026-07 |
| [Meat Church BBQ](https://www.youtube.com/@meatchurchbbq) | BBQ 육류·시즈닝·장비 | 2026-07 |
| [America's Best Restaurants](https://www.youtube.com/@americasbestrestaurants) | 주별 독립 식당·지역 메뉴 | 2026-07 |

### 일본 신규 12개

| 채널 | 역할 | 최근 업로드 확인 |
| --- | --- | --- |
| [料理研究家リュウジ](https://www.youtube.com/@ryuji825) | 가공식품 리뷰·일상 조리 | 2026-07 |
| [Koh Kentetsu Kitchen](https://www.youtube.com/@kohkentetsukitchen) | 일본 가정식·계절 재료 | 2026-07 |
| [料理研究家ゆかり](https://www.youtube.com/@yukariskitchen3689) | 지역·계절 가정식 | 2026-07 |
| [笠原将弘の料理のほそ道](https://www.youtube.com/@sanpiryoron) | 전문 일식·제철 재료 | 2026-07 |
| [さばけるチャンネル](https://www.youtube.com/@sabakeru) | 어종별 손질·수산 교육 | 2026-03 |
| [漁師ちゃんねる](https://www.youtube.com/@fisherman_japan) | 어업 현장·선상 식사 | 2026-05 |
| [日本料理アカデミー](https://www.youtube.com/@japaneseculinaryacademy91) | 일본요리 공식 교차검증 | 2026-06 |
| [SugoUma Japan](https://www.youtube.com/@sugoumajapanfood) | 가쓰오부시·참치 등 생산 현장 | 2026-07 |
| [休日ひとりグルメ旅](https://www.youtube.com/@solo_eats) | 후쿠이·요코하마·하코네 지역 음식 | 2026-07 |
| [メシ時々旅〜北海道〜](https://www.youtube.com/@meshitabi-hokkaido) | 홋카이도 지역 음식 | 2026-07 |
| [たか / 九州グルメ旅](https://www.youtube.com/@kyushufood) | 사가·구마모토 등 규슈 음식 | 2026-07 |
| [孤独のまちこ・ひとり旅](https://www.youtube.com/@machikosolotrip) | 나가사키·나고야 등 지역 음식 | 2026-03 |

## 재검토 대상

- `Future Neighbor`는 최근 공개 업로드가 2024-07-26으로 확인됐다.
- `日本列島まるっと旅`는 최근 공개 업로드가 2023-10-29로 확인됐다.
- 두 채널 모두 과거 자료의 조사 가치는 남아 있으므로 자동 비활성화하지 않는다. 신규 업로드 감시 목적과 archive 조사 목적을 분리해 관리자가 검토한다.
- 방송사·기관 채널은 독립 제작자와 분리해 대표성 교차검증에 사용한다.
- 협찬·광고 영상은 공개 metadata에서 확인되는 표기를 보존하고, 구매 수요의 자연 발생 근거로 단독 사용하지 않는다.

## DB 동기화

전체 서버와 다른 Quartz 작업을 시작하지 않고 국가별 카탈로그를 초기화·동기화한다.

```powershell
dotnet run --project Ssalddel -- --sync-youtube-country=KR --YouTube:SeedFoodResearchCatalog=true
dotnet run --project Ssalddel -- --sync-youtube-country=US --YouTube:SeedFoodResearchCatalog=true
dotnet run --project Ssalddel -- --sync-youtube-country=JP --YouTube:SeedFoodResearchCatalog=true
```

초기 동기화 영상은 기준선으로 저장해 신규 업로드 알림을 만들지 않는다. 같은 명령을 다시 실행했을 때 동일 video ID는 중복 저장하지 않는다.

2026-07-24 실제 실행 결과는 다음과 같다. KR에는 카탈로그 21개 외에 기존 수동 등록 채널 1개가 있어 DB 처리 채널이 22개다.

| 국가 | DB 채널 | 수신 영상 | 최초 추가 | 기존 채널 신규 업로드 | 즉시 재실행 추가 |
| --- | ---: | ---: | ---: | ---: | ---: |
| KR | 22 | 440 | 202 | 2 | 0 |
| US | 22 | 440 | 201 | 1 | 0 |
| JP | 18 | 360 | 240 | 0 | 0 |
| 합계 | 62 | 1,240 | 643 | 3 | 0 |

## API 근거

- [YouTube `search.list`](https://developers.google.com/youtube/v3/docs/search/list): 채널 검색, 음식 topic, 지역·언어 조건과 일일 검색 한도
- [YouTube `channels.list`](https://developers.google.com/youtube/v3/docs/channels/list): channel ID, handle, uploads playlist와 채널 metadata 확인
- [YouTube channel resource](https://developers.google.com/youtube/v3/docs/channels): `snippet.country`와 `brandingSettings.channel.country` 의미
