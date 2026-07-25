# 커뮤니티 전체 피드 미디어와 반응형 동영상 재생

## 결과

- 전체 피드 카드가 게시글의 기존 첨부 계약을 사용해 이미지와 동영상을 구분해 표시한다.
- 이미지는 최대 4장을 1열 또는 2열 반응형 grid로 보여 주고, 동영상이 있으면 대표 동영상 하나를 우선 표시한다.
- 동영상은 viewport 안에 55% 이상 들어온 항목 중 화면 중앙에 가장 가까운 하나만 음소거 상태로 자동 재생한다. 다른 동영상은 즉시 일시정지한다.
- 사용자가 controls로 직접 멈춘 동영상은 화면을 벗어나기 전까지 다시 강제로 재생하지 않는다.
- `prefers-reduced-motion`, data saver 또는 비활성 document 상태에서는 자동 재생하지 않는다.
- 새 page가 이어 붙거나 피드가 다시 조회되면 observer 대상을 갱신하고, 제거된 video element는 관찰 대상에서 정리한다.

## 작성·저장 경계

- 커뮤니티 글쓰기에서 JPG·PNG·WebP·GIF와 함께 MP4·WebM을 선택할 수 있다.
- 게시글당 기존 최대 5개 제한을 유지한다.
- 이미지는 파일당 5MB, 동영상은 파일당 15MB로 client와 server가 같은 한도를 적용한다.
- 기존 `PlatformCommunityPostAttachmentResponse`와 공개 attachment route를 재사용하고 별도 미디어 DTO나 저장 경로는 만들지 않았다.

## 화면

간접 확인 — 공통 Razor UI와 `SsalddelApp` Windows target은 빌드됐으나, unpackaged 실행 파일이 이 환경에서 창을 만들지 않아 실제 PNG는 남기지 않았다. 현재 데이터에 동영상 첨부가 존재하는지도 실제 실행 화면에서 확인하지 못했다.

## 확인

- `Community전체FeedMediaTests`: 동영상 우선 선택, 이미지 최대 4장, 미지원 형식 제외, 반응형 autoplay 조건 확인
- `CommunityPostComposerViewModelTests`: MP4·WebM 선택과 동영상 15MB 제한 확인
- `CommunityPostAttachmentUseCaseTests`: 이미지 5MB·동영상 15MB server 검증과 MP4 저장 확인
- 관련 targeted test 29개 통과
- `SsalddelApp` `net10.0-windows10.0.19041.0` build 통과, warning 0 / error 0
- `Directory.Build.props`에서 `artifacts/`를 build 입력에서 제외해 validation 산출물의 재귀 유입 방지
