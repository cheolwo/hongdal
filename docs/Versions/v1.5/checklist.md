# Ssalddel 1.5 Checklist

- [x] 공급자·기업 자료마다 원출처, 확인 시각과 식별 근거가 있습니다.
- [x] 견적에 통화, 단위, MOQ, 납기, 포장, Incoterms 후보와 유효기간이 있습니다.
- [x] 상품·국제 운송보험·관세·세금·국내 이행 예상비가 분리됩니다.
- [x] HSK·HTSUS 후보에 분류 근거, 신뢰도와 검토 상태가 있습니다.
- [x] 한국 MFDS와 미국 수입 검토 항목을 국가별로 분리합니다.
- [x] 판매자·수입자·관세사·플랫폼 책임이 원장에 구분됩니다.
- [x] 미확인 규제·계약 항목이 완료로 표시되지 않습니다.
- [x] 실제 신고, 계약 서명, 결제와 운송 지시는 실행하지 않습니다.
- [x] 핵심 API·화면 메타데이터와 독립 솔루션이 제품 버전 `1.5`를 사용합니다.

## 검증 근거

- 계약·평가 정책: `Ssalddel.Contracts/Common/Orderer/공동수입준비원장Dtos.cs`
- 승인 인계·멱등·Revision 영속 흐름: `Ssalddel/Services/Orderer/공동수입준비원장Service.cs`
- 원천·대상 양방향 추적: `Ssalddel/Services/Orderer/공동구매수요모집OS.cs`, `Ssalddel/Services/Orderer/공동구매자동집단화저장소.cs`
- 관리자·기능 플래그 경계: `Ssalddel/Controllers/Admin/Orderer/공동수입준비원장AdminController.cs`
- 공식 근거 읽기 화면: `OrdererApp/Components/GroupPurchase/GroupPurchaseTradeReadinessEvidencePanel.razor`
- 관리자 작성·검토 화면: `Ssalddel.Ui.Common/Areas/BackOffice/Components/공동수입준비관리작업대.razor`
- 독립 릴리스 구성: `Ssalddel.v1.5.slnx`, `.github/workflows/release-readiness.yml`
- 자동 검증: 서비스·컨트롤러·기능 플래그·화면 조립·솔루션 구성 테스트

체크 완료는 **기술적 Simulation 계약의 완결**을 뜻합니다. 실제 수입 적격성, 품목분류, 법률·인허가, 계약 또는 운영 승인을 뜻하지 않습니다.
