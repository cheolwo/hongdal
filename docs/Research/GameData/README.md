# 게임 자료 조사 목록

- [D403 한국24절기·농수산물 소수 조사](D403/README.md) / [개발 제한 인수](D403/development-intake.r1.md): 명칭24·농산물3/수산물3 참고자료, 정확 절기 시각·품목 절기 대응 미확보. 아래 최초 묶음 상태/hash는 당시 이력이다.

- 묶음: `game-data-research.first-farm.r1`, 2026-08-30, D366.
- 상태: 공식 3자료 내용·권리 표시 확인, 공개 소표본 1개 확보, 개발 인수 대기. 실행·E 승격·권위 규칙 승인이 아니다.
- [첫 Farm 보고서와 인계](first-farm.r1.md)
- [기존 체계 재사용 재고](reuse-inventory.r1.md)
- [구조화 자료 목록](sources.r1.json)
- [D367 후속 묶음: 거래기회 안내 재사용 사전 검토](trade-opportunity-next.r1.md)
- [읽기 전용 기준선과 참조 파일 SHA-256](baseline.r1.json)
- [산출물 SHA-256 목록](manifest.r1.json)

현행 참조 저장소는 `C:/Users/user/source/repos/Hongdal`이며 쓰지 않았다. 배정 worktree HEAD는 `b0c1c8469664ae6cce9e272f93650d6eba796804`, 시작 시 공유 저장소 HEAD는 `712c4a08349bcda0b7c4b0489bfb8ad2a1e7087a`다. 공유 HEAD까지 617개 커밋 차이이며 worktree에는 현행 AGENTS·관련 코드가 없다. 따라서 링크·코드 검증의 기준은 공유 저장소의 실제 파일 바이트와 baseline hash다. 미커밋 문서도 HEAD만으로 재현되지 않으므로 hash를 함께 사용한다.

이 목록은 연구 메타데이터이며 새 권위 DB나 실행용 Source Catalog가 아니다. 원자료는 배정 worktree의 `artifacts/local/game-data-research/`에만 있으며 commit/push 대상이 아니다. 샘플 없이 열람만 한 자료의 원문 hash는 `null`로 둔다. `manifest.r1.json`은 자신을 제외한 문서·목록을 검증한다.

인계 순서는 전문 → 개발 `01a02198-8b2a-7491-ac93-366b30ff474c` 검토·통합 → 기획 `01a025cf-0772-7251-b842-156a20e7483e`다. 다른 승인 개발을 막지 않는다. 후속 대량 수집·키 조회·계정·약관 동의·유료 호출·운영 DB 접근·Scene/Packages/Editor 조작은 하지 않았다.
