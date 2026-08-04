# 선택 공공데이터 지도 snapshot 재시작 복원

- 날짜: 2026-08-04
- 화면: `/community/home`
- 범위: 관광·온라인가격·KOSIS 지도 source snapshot 저장, 복원, 버전 표시

## 변경 내용

- 기존 singleton 메모리 snapshot을 `App_Data/public-data-map/selected-sources-map-snapshot.v1.json`에 영속 저장합니다.
- 갱신 성공본은 임시 파일에 먼저 쓴 뒤 원자적으로 교체하며, 파일 저장이 실패하면 메모리의 현재 성공본도 성공한 것처럼 바꾸지 않습니다.
- 서버 시작 시 schema version, snapshot version과 저장 시각이 유효한 마지막 성공본만 복원합니다.
- 관광·온라인가격·KOSIS 중 일부 외부 source 갱신이 실패하면 기존 source별 성공본을 유지해 다음 snapshot에 포함합니다.
- snapshot 내용으로 SHA-256 기반 `selected-public-data-map.v1` 버전을 만들고 지도 observation API의 `SourceVersion`에 전달합니다.
- 지도 상세에는 `자료 버전`을 표시하며 긴 hash가 좁은 화면을 넘지 않도록 줄바꿈합니다.

관광 좌표는 공개 관광지 좌표만 보존하며 주소·연락처·이미지를 snapshot에 저장하지 않습니다. 온라인가격과 KOSIS는 단위가 다른 자료이므로 가격차·절감액·순위를 계산하지 않는 기존 경계를 유지합니다.

## 검증

- `선택공공데이터MapProjectionTests`: 4개 통과
  - 세 source별 snapshot 갱신
  - 관광 source 실패 시 이전 성공본 유지
  - 파일 저장 뒤 새 store instance에서 같은 version과 관광 항목 복원
  - 지도 observation에 source version, freshness, 수집 시각과 단위 분리 유지
- scoped Fast 검증: v3.5 build와 관련 test 통과
- scoped Task 검증: v3.5 build와 관련 test 통과
- 실제 서버 실행은 로컬 DB의 기존 개인정보 암호문과 현재 Data Protection key ring 불일치로 초기 seed 단계에서 중단됐습니다.
- 따라서 `/community/home`의 `자료 버전` 실제 렌더링은 미확인이고 코드 build와 투영 test로 간접 확인했습니다.

## 화면

간접 확인 — 런타임의 Data Protection 키 복구 뒤 실제 화면 캡처가 필요합니다.
