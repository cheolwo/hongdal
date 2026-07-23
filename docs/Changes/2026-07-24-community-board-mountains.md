# 0.0~3.5 간괘 게시판 산맥 확정

## 결과

- Command/Event 하나당 하나였던 활동 게시판을 버전 흐름별 일곱 게시판으로 통합했다.
- 일반 대화 게시판과 시스템 활동 게시판의 책임을 분리했다.
- 각 활동 게시판에 Command 수, Event 수와 관련 Web·App 페이지를 명시했다.
- 게시판 모음과 공용 커뮤니티 게시판 선택 화면에 간괘 `☶` 산 표현을 적용했다.
- 활동 카드를 펼쳐 Command·Event 이름과 페이지 Route를 점검하고, 실제 Web 목록 Route는 바로 열 수 있게 했다.
- 각 업무 게시판의 고정 안내 글이 전체 Command·Event·Page 연결을 설명하도록 변경했다.
- 기존 Command/Event별 게시판 key와 표시명은 별칭으로 보존해 과거 글을 새 게시판에서 함께 조회한다.

## 시각 확인

간접 확인. Razor와 CSS 조립, Web Release 빌드와 구조 테스트로 확인했으며 브라우저 캡처와 실제 배포는 수행하지 않았다.

## 검증

- `eng/validate-changes.ps1 -Level Fast -Paths ...`: 통과
- `eng/validate-changes.ps1 -Level Task -Paths ...`: 통과
- `WarehouseManagerApp` Windows Release 빌드: 경고 0개, 오류 0개

## 기준 문서

- [간괘 게시판 산맥과 Command·Event·Page 묶음](../Architecture/CommunityBoardMountainCatalog.md)
