# 원천별 주기성 데이터 전용 게시판

## 변경

- KAMIS, MFDS, USDA, 관세청 수입 평균단가를 각각 운영자 전용 공개 게시판으로 분리했다.
- 정기 글은 원천별 `CanonicalBoardKey`가 가리키는 게시판에 한 번만 저장한다.
- 기존 `정보·시세` 등 관계 게시판에는 DB 글을 복제하지 않고, 상단의 `대표 안내` 카드가 전용 게시판의 `주기성` 목록으로 이동한다.
- 기존 자동 글은 안정된 `sourceKey + periodKey` 작성자 키로 다시 실행될 때 새 행을 만들지 않고 Category만 전용 게시판으로 옮긴다.
- KAMIS 초안은 `KAMIS 가격 데이터`, USDA 초안은 `USDA 가격 데이터`, 중국 권역·미국 주별 MFDS 초안은 `MFDS 수입식품 데이터`를 사용한다.
- `관세청 수입단가 데이터`는 현재 요청 시 조회 원천의 단일 게시 위치만 마련했으며, 새 자동 배치 일정은 활성화하지 않았다.
- MFDS 전용 게시판에서 중국·미국 국가 필터와 `전체글 / 일반글 / 주기성` 분류를 함께 제공한다.
- YouTube는 이번 원천 게시판과 관계 안내 대상에서도 제외했다.

## 화면

- 정보·시세 desktop: [대표 안내 카드](../assets/changes/2026-07-24-periodic-data-source-boards/information-prices-guides-desktop.png)
- 정보·시세 390px: [가로 스크롤 대표 안내 카드](../assets/changes/2026-07-24-periodic-data-source-boards/information-prices-guides-mobile.png)
- MFDS desktop: [전용 게시판과 국가·주기성 필터](../assets/changes/2026-07-24-periodic-data-source-boards/mfds-periodic-board-desktop.png)
- MFDS 390px: [전용 게시판 모바일](../assets/changes/2026-07-24-periodic-data-source-boards/mfds-periodic-board-mobile.png)

로컬 API는 기존 개발 DB의 Data Protection 키 불일치로 일부 목록 조회가 실패했지만, 게시판 계약을 받은 Web 화면에서 전용 게시판·대표 안내 route·국가 및 주기성 필터의 실제 렌더를 확인했다.
