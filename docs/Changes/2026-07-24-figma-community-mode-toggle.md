# Figma 커뮤·업무 모드 토글

## 결과

[살뜰 Figma 파일](https://www.figma.com/design/0KhuQLc1MleUBIQnARC21Z/ssalddle?node-id=0-1)의 `01A · Community Boards`에서 게시판 종류를 바꾸는 `업무` 스위치를 우측 상단에 추가했다.

- `업무 OFF`: 커뮤모드로 생활 게시판만 표시
- `업무 ON`: 업무모드로 업무 게시판 모음과 업무별 게시판 표시

기존 `생활 게시판 / 업무 게시판` 칩은 토글과 책임이 겹치므로 숨겼다. 게시판 검색과 각 모드 안의 세부 필터는 그대로 유지했다.

## 대응 화면

- `01A.01 · 게시판 모음 · 커뮤모드 · 업무 OFF`
- `01A.07 · 게시판 모음 · 업무모드 · 업무 ON`

![업무 토글 OFF와 ON 상태를 함께 보여 주는 Community 화면](../assets/changes/2026-07-24-figma-community-mode-toggle/community-mode-toggle.png)

## 확인

- 두 스위치가 각 AppBar의 우측 상단 영역 안에 들어가는지 확인했다.
- OFF 상태의 스위치 손잡이가 왼쪽, ON 상태가 오른쪽에 있는지 확인했다.
- 커뮤모드 화면에는 생활 게시판, 업무모드 화면에는 업무 게시판이 표시되는지 확인했다.
- 중복된 게시판 종류 칩이 숨겨졌는지 확인했다.
- Figma 구조 검사 결과 `issueCount: 0`을 확인했다.
- 이 기록은 Figma 설계 변경에 대한 시각 기록이며 애플리케이션 코드 변경은 포함하지 않는다.
