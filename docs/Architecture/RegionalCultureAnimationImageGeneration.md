# 지역문화 3D 애니메이션 이미지 순차 생성

## 목적

한국 17개, 미국 50개 주, 중국 31개 성급 지역의 지역문화 이미지를 지역별 10장씩 준비한다. 전체 목표는 98개 지역, 980장이다.

이미지는 실사 사진이나 평면 관광 일러스트가 아니라 따뜻한 시네마틱 3D 애니메이션 스틸로 통일한다. 다만 생성 이미지는 문화적 맥락을 돕는 표현물이며, 역사적 사실·상품 원산지·공공기관의 보증을 증명하는 자료로 사용하지 않는다.

## 공통 시각 언어

- 스타일 코드는 `CinematicStylized3D`다.
- 부드럽지만 설득력 있는 입체 형태, 손으로 만든 듯한 표면 질감, 전경·중경·후경의 깊이를 사용한다.
- 따뜻한 빛과 차가운 주변광을 함께 써서 시간대와 생활감을 표현한다.
- 실사 사진, 평면 2D 편집 삽화, 특정 스튜디오·작가·기존 작품·캐릭터의 모사는 제외한다.
- 기존 영상 프로젝트의 시네마틱한 입체감은 참고하되 우주 구체, 마법 에너지, 판타지 문양은 지역문화 이미지에 옮기지 않는다.
- 텍스트, 가격, 간판 문구, 로고, 국기, 지도, 정치 상징, 관인, 워터마크와 식별 가능한 실존 인물을 생성하지 않는다.
- 16:9 화면을 기본으로 하고 핵심 인물과 문화 요소는 중앙 4:3 안전 영역 안에 둔다.

## 지역별 10개 장면

| 순서 | 장면 | 표현 범위 |
| --- | --- | --- |
| 01 | 하루의 시작 | 지형, 주거와 작은 일터가 연결되는 아침 |
| 02 | 시장과 음식 | 재료 선택·손질·조리, 특산물은 보조 요소로 제한 |
| 03 | 공예와 생업 | 현재 이어지는 공예·수리·일, 근거 없는 옛 도구 금지 |
| 04 | 지형과 이동 | 실제 지역 프롬프트에 있는 해안·강·산·평야·도시와 일상 이동 |
| 05 | 집과 세대 | 열린 생활 공간에서 나누는 생활 기술과 식사 |
| 06 | 계절의 일 | 계절·날씨 속 평범한 일, 근거 없는 축제·의례 금지 |
| 07 | 건축 돌봄 | 현재 쓰이는 건축과 생활공간의 이용·관리 |
| 08 | 배움과 만들기 | 도서관·공방·학교·공동체 공간의 생활 학습 |
| 09 | 저녁 공동체 | 이웃의 일과 정리·대화·식사, 과도한 공연·광고 금지 |
| 10 | 현재의 연속성 | 세대와 전통 기술·현대 생활이 사람의 활동으로 이어지는 장면 |

장면마다 같은 지역의 다른 이미지와 주제·시간대·구도가 겹치지 않게 한다. 랜드마크, 복식, 민족, 음식 또는 관광 문구 하나를 지역 전체의 정체성으로 고정하지 않는다.

## 승인과 생성 흐름

```text
ResearchDraft v2
  -> 공식 지역 원천 검토
  -> 고정관념 위험 검토
  -> ApprovedForGeneration
  -> KR, US, CN 순서로 한 지역 선택
  -> scene-01 ... scene-10 순차 작업
  -> 외부 생성
  -> 저장소 업로드
  -> 완료 URL과 진행률 조회
```

승인 API는 공식 원천과 고정관념 위험을 모두 검토했다는 명시적 표시와 20자 이상의 검토 메모를 요구한다. `ResearchDraft`를 자동 승인하지 않으며, 이미 검토된 프롬프트는 seed 갱신으로 덮어쓰지 않는다.

생성 대상 식별자는 `{region-key}--scene-{01..10}`으로 고정한다. 완료되었거나 진행 중인 장면은 다시 등록하지 않고, 실패 장면은 관리자가 `IncludeFailed=true`로 명시했을 때만 재시도한다.

## 실행 경계

기본 설정은 외부 비용과 대량 생성을 막기 위해 비활성이다. 다음 조건을 모두 충족해야 외부 작업을 등록한다.

1. `RegionalCultureImageGeneration:Enabled=true`
2. `SsalddelExecution:Mode=Operational`
3. `GeminiImage:Enabled=true`와 `GeminiImage:ApiKey` 구성
4. 해당 지역 프롬프트의 `ApprovedForGeneration`
5. `RequiresEvidenceReview=false`
6. 진행 중인 다른 지역문화 이미지 작업이 없음
7. 일일 생성 등록 한도 미도달

기본값은 한 주기 1장, 하루 10장, 5분 간격이다. 한 장이 완료 또는 실패되기 전에는 다음 장면을 등록하지 않는다. 운영 설정은 소스에 secret을 넣지 않고 환경 변수나 로컬 전용 설정으로 구성한다.

| 설정 | 기본값 | 의미 |
| --- | ---: | --- |
| `RegionalCultureImageGeneration__Enabled` | `false` | 순차 생성 Worker 활성화 |
| `RegionalCultureImageGeneration__TargetImagesPerRegion` | `10` | 지역별 목표 장수, 현재 최대 10 |
| `RegionalCultureImageGeneration__MaxNewJobsPerCycle` | `1` | 한 주기 신규 작업 수 |
| `RegionalCultureImageGeneration__MaxDailySubmissions` | `10` | UTC 기준 일일 등록 상한 |
| `RegionalCultureImageGeneration__IntervalMinutes` | `5` | Worker 확인 간격 |
| `RegionalCultureImageGeneration__CountryOrder` | `KR,US,CN` | 국가 처리 우선순위 |
| `RegionalCultureImageGeneration__AspectRatio` | `16:9` | 생성 종횡비 |
| `RegionalCultureImageGeneration__Resolution` | `1K` | 생성 해상도 |

## 관리자 API

기본 route는 `api/v1/admin/content/information/regional-culture/image-generation`이며 서버 관리자 전용이다.

- `GET /progress?countryCode=KR|US|CN`: 지역별 10개 슬롯의 누락·대기·진행·완료·실패 상태 조회
- `POST /prompts/{regionKey}/approve`: 공식 근거·고정관념 위험 검토 후 생성 승인
- `POST /next`: 승인된 첫 지역의 다음 누락 장면을 bounded 생성 대기열에 등록

외부 Provider가 반환한 원본 URL을 그대로 장기 공개하지 않고 기존 이미지 작업의 저장소 업로드 경계를 거친다. 실패 이유는 관리자 진행 현황에서만 확인하며, API key와 외부 요청 원문은 응답·로그에 남기지 않는다.

## 현재 검증 범위

서울 장면 01을 공통 스타일의 기준 샘플로 로컬 생성해 입체감, 질감, 빛, 비관광 엽서 구도를 확인했다. 이 샘플은 화면에 연결하거나 공식 근거로 공개하지 않았고 `artifacts/local/`에만 보관한다.

서버 자동화는 98개 조사 초안을 모두 생성 승인한 상태로 간주하지 않는다. 국가별·지역별 공식 원천 연결과 사람의 검토가 끝난 지역부터 순차적으로 활성화한다.
