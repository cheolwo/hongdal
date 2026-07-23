# Ssalddel 공통 UI 작업 지침

이 폴더에서는 저장소 루트 `AGENTS.md`와 함께 아래 UI 원칙을 적용한다.

## 구조와 재사용

- 새 공용 흐름은 우선 `SsalddelApp`과 `Ssalddel.Ui.Common`에 통합하고, 기존 전문 앱은 명시적 요청 없이 삭제하거나 축소하지 않는다.
- 공통 셸은 커뮤니티, 업무, 다이어그램, 정보 흐름을 연결하며 기본 navigation은 `사방괘 -> 다이어그램 -> 구체 데이터 페이지`다.
- MAUI Blazor는 View, ViewModel, navigation 책임을 나누고 기존 MVVM CommunityToolkit 패턴을 따른다.
- shared component와 workflow를 먼저 재사용하고 platform 기능만 adapter로 분리한다.

## 사용자 경험과 검증

- 모바일 목록은 넓은 table보다 compact card와 detail 전환을 우선한다.
- 초기 필수 데이터를 먼저 표시하고 loading, empty, error, retry, disabled 상태를 제공한다.
- desktop/mobile에서 텍스트 잘림, 겹침, 터치 영역, 고정 navigation, drawer, dialog, diagram 연결선을 확인한다.
- 시각 변경은 실제 렌더링으로 검증하고 개인정보·주소·연락처·계좌·결제 식별자·위치·증빙 원본은 마스킹한다.
- 대표 PNG는 `docs/assets/changes/`로 옮기고 임시 capture와 raw output은 `artifacts/local/`에 둔다.
- 공통 UI 변경은 server build만으로 끝내지 않고 최소 한 소비 client를 함께 검증한다.

화면 구조는 `docs/Architecture/ThreeStageClientNavigation.md`, 화면 색인은 `docs/ProjectOverview/00-첨부문서목차.md`를 따른다.
