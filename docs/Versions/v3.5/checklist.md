# Ssalddel 3.5 Checklist

- [ ] 알뜰살뜰 마트 상품 주문 흐름 확인
- [ ] 도심 재고 보충/가용 재고 확인
- [ ] 피킹 작업 생성 확인
- [ ] 포장 완료 상태 확인
- [x] `SsalddelMartPackedOrder` 배차 대기 생성 확인
- [x] 포장 완료 전 음식 배달 배차 보류 확인
- [ ] 음식 배달 기사 픽업 정보 표시 확인
- [ ] 묶음 배달 가능성 판단 확인
- [ ] 3.0 음식점 일반 배달과 알뜰살뜰 마트 즉시배송의 정산/배차 정책 분리 확인
- [x] 내부 3.5 Compose 검증에서 `VersionFeatureFlags__SsalddelMartWorkflow=true` 확인
- [x] `Ssalddel.v3.5.slnx`가 01~05 역할과 음식점·배달·운영·서버·테스트 project를 함께 조립
- [x] Release CI가 `mart-v35` Compose와 rollback 가능한 배포 bundle을 생성하도록 구성
- [x] 기본 실행 모드와 Azure 3.5 조립을 `Operational`로 전환하고 비밀키 누락 시 시작 실패 경계 유지
- [x] 주문자 상품 Catalog, 화주 판매채널 주문, 기사 알림함, 창고 입고·피킹·작업 진입을 서버 API 기본 구현으로 연결
- [ ] [공통 릴리즈 게이트](../release-gates.md) 통과 확인
