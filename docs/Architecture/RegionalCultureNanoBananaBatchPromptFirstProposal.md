# 지역문화 Nano Banana Batch 프롬프트 우선 제작 제안서

## 결론

지역문화 이미지는 API를 먼저 호출하지 않는다. 지역별 10개 장면을 로컬 프롬프트 팩으로 작성하고 공식 근거·고정관념 위험·장면 중복을 사람이 검토한 뒤에만 Nano Banana Batch 요청으로 변환한다.

첫 파일럿은 [`kr-seoul.v1.json`](../Content/RegionalCultureImagePrompts/packs/kr-seoul.v1.json)이다. 현재 상태는 `ResearchDraft`이며 API를 호출하지 않는다.

## 홍익학당 영상 저장소에서 가져온 운영 원칙

- 제작 제안서와 장면별 프롬프트를 먼저 저장소에 남긴다.
- 프롬프트의 인물·행동·장소·카메라·조명·유지 요소를 검토한 다음 생성한다.
- Batch 결과를 전부 최종본으로 보지 않고 초안 선별 후 필요한 장면만 단건 호출로 보정한다.
- 프롬프트 key와 version을 고정해 같은 입력의 중복 비용을 막는다.

지역문화에는 영상의 인물 연속성 대신 `지형·현재 생활·문화적 근거·고정관념 방지`를 핵심 검토 축으로 사용한다.

## 로컬 팩 구조

지역마다 JSON 한 파일을 둔다.

```text
docs/Content/RegionalCultureImagePrompts/packs/
  kr-seoul.v1.json
  kr-busan.v1.json
  ...
```

한 팩은 다음을 포함한다.

- 국가·지역 stable key와 prompt version
- Batch 모델, 16:9, 1K 같은 생성 조건
- 모든 장면에 적용되는 공통 시각 언어
- 공식 자료 검토 checklist
- 고정관념·오표현 방지 목록
- `scene-01`부터 `scene-10`까지 구체적인 장면 프롬프트

상태는 다음처럼 관리한다.

```text
ResearchDraft
  -> 공식 근거 검토
  -> 고정관념·중복·시대 혼합 검토
  -> ApprovedForBatch
  -> Batch 제출
  -> 결과 수집
  -> A/B/C 선별
  -> 선택 장면만 단건 보정
```

서버의 `지역문화이미지BatchPromptPackCompiler`는 `ApprovedForBatch`가 아닌 팩을 외부 요청으로 변환하지 않는다.

## Batch API 경계

Google 공식 문서상 Batch API는 비동기 처리이며 목표 처리 시간은 24시간 이내다. 일반 호출 대비 50% 비용으로 안내되고, 전체 요청 크기가 20MB 미만인 작은 묶음은 inline request가 적합하다. 지역별 10장은 text-only inline Batch 한 건으로 묶는다.

현재 단건 생성은 `interactions`의 `gemini-3.1-flash-image`를 사용한다. Batch API는 `generateContent` 경계이므로 단건 설정을 그대로 재사용하지 않는다. Batch 모델은 별도 설정으로 관리하고, 실제 계정에서 지원 여부를 확인한 모델만 활성화한다. 공식 예제의 이미지 Batch 모델은 `gemini-3-pro-image-preview`다.

Batch 작업은 앞으로 다음 상태를 영속화해야 한다.

- pack ID, prompt version과 각 장면 key
- prompt hash와 제출 모델
- 외부 Batch job name과 상태
- 항목별 성공·실패·결과 MIME type
- Blob Storage object name과 공개 URL
- 재시도 횟수와 마지막 오류

API key, Base64 이미지 원문과 전체 외부 응답은 DB·로그·문서에 남기지 않는다.

## 구현 우선순위

1. 한국 17개 지역의 로컬 프롬프트 팩 작성과 검토
2. 로컬 팩 schema·중복·10장 완결성 검증
3. Batch job/item 원장과 inline 제출·상태 조회 adapter
4. 결과를 Azure Blob Storage에 저장하고 항목별 완료 상태 반영
5. 저비용 초안 A/B/C 선별
6. 선택된 B만 단건 Nano Banana 보정
7. 한국 검증 뒤 미국 50개 주, 중국 31개 성급 지역으로 확대

기존 DB seed 프롬프트는 지역 조사 초안으로 유지한다. 새 로컬 팩은 실제 생성 직전의 장면별 제작 문서이며, 팩이 없는 지역은 Batch 생성 대상이 아니다.

## 이번 단계

- 서울 10장 로컬 프롬프트 팩을 작성했다.
- 팩 parser와 10장 완결성 검증을 추가했다.
- `ResearchDraft`는 Batch 요청으로 컴파일되지 않도록 했다.
- 외부 API 호출과 비용 발생은 하지 않았다.

## 98개 지역 생성 준비 manifest

한국 17개 시·도, 미국 50개 주, 중국 31개 성급 지역의 생성 전 범위는 [`research-readiness.v1.json`](../Content/RegionalCultureImagePrompts/research-readiness.v1.json)으로 고정한다.

- 총 98개 `RegionKey`와 국가별 공식 원천 key를 seed와 대조한다.
- 모든 지역의 기본 상태는 `ResearchDraft`, `generationAuthorized=false`다.
- 한 지역을 승인하려면 같은 국가의 등록된 공식 원천 key를 최소 2개 실제로 검토하고 검토 메모를 남긴다.
- 국가 공통 원천은 조사 시작점일 뿐이다. 성·주·시도별 원문과 현재 생활문화 표현을 확인하기 전에는 승인하지 않는다.
- 이미지 생성, Batch 제출, 외부 작업 등록은 이번 준비 단계에 포함하지 않는다.

## 공식 참고

- [Gemini Batch API](https://ai.google.dev/gemini-api/docs/batch-api)
- [Gemini image generation](https://ai.google.dev/gemini-api/docs/image-generation)
