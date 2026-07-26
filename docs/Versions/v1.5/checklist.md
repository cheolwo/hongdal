# 문화교통 1.5 Checklist

- [x] 공급자·기업 자료마다 원출처, 확인 시각과 식별 근거가 있습니다.
- [x] 견적에 통화, 단위, MOQ, 납기, 포장, Incoterms 후보와 유효기간이 있습니다.
- [x] 상품·국제 운송보험·관세·세금·국내 이행 예상비가 분리됩니다.
- [x] HSK·HTSUS 후보에 분류 근거, 신뢰도와 검토 상태가 있습니다.
- [x] 한국 MFDS와 미국 수입 검토 항목을 국가별로 분리합니다.
- [x] 판매자·수입자·관세사·플랫폼 책임이 원장에 구분됩니다.
- [x] 미확인 규제·계약 항목이 완료로 표시되지 않습니다.
- [x] 실제 신고, 계약 서명, 결제와 운송 지시는 실행하지 않습니다.
- [x] 핵심 API·화면 메타데이터와 독립 솔루션이 제품 버전 `1.5`를 사용합니다.
- [x] 별도 1.5 준비 원장을 만들지 않고 기존 정식 `group-import` 원장의 준비 블록에 상태와 작업 실행 이력을 영속하며 Revision 충돌을 숨기지 않습니다.
- [x] 하나 이상의 재료·수요 집단을 같은 공동수입 원장에 연결하되 재료별 수량·단위·온도를 유지합니다.
- [x] LCL/FCL은 사용자 또는 플랫폼 선택값이 아니라 합산 물류 조건에 대한 포워더·물류대행업체 회신으로 기록합니다.
- [x] 포워더 전달 자료는 집계 정보가 기본이며 사용자 단위 정보에는 명시적 동의와 근거가 필요합니다.
- [x] Process Manager는 포워더를 자동 선정하거나 자료를 외부로 자동 전송하지 않습니다.
- [x] 공급자·견적·원가·품목분류·규제 근거의 최신성을 정기 worker와 관리자 수동 점검으로 재평가합니다.
- [x] KAMIS·USDA·기업 근거·가격 브리프 배치를 공유 카탈로그로 등록하되 수동 점검이 외부 API를 직접 실행하지 않습니다.
- [x] 작업별 재시도, 멱등 키와 사람이 정한 포워더·전문 검토 수신자·범위·메모를 기록합니다.

## 검증 근거

- 계약·평가 정책: `Ssalddel.Contracts/Common/Orderer/공동수입준비원장Dtos.cs`
- 승인 인계·기존 공동수입 원장 병합·멱등·Revision 영속 흐름: `Ssalddel/Services/Orderer/공동수입준비원장Service.cs`
- 1.5 Process Manager 상태·정기 점검·수동 점검·사람 인계: `Ssalddel/Services/Orderer/공동수입준비ProcessManager.cs`
- 1.5 호환 API 계약: `Ssalddel.Contracts/Common/Orderer/공동수입준비OsDtos.cs`
- 원천·대상 양방향 추적: `Ssalddel/Services/Orderer/공동구매수요모집ProcessManager.cs`, `Ssalddel/Services/Orderer/공동구매자동집단화저장소.cs`
- 관리자·기능 플래그 경계: `Ssalddel/Controllers/Admin/Orderer/공동수입준비원장AdminController.cs`
- 공식 근거 읽기 화면: `OrdererApp/Components/GroupPurchase/GroupPurchaseTradeReadinessEvidencePanel.razor`
- 관리자 작성·검토 화면: `Ssalddel.Ui.Common/Areas/BackOffice/Components/공동수입준비관리작업대.razor`
- 독립 릴리스 구성: `Ssalddel.v1.5.slnx`, `.github/workflows/release-readiness.yml`
- 자동 검증: 서비스·컨트롤러·기능 플래그·화면 조립·솔루션 구성 테스트

체크 완료는 **기술적 Simulation 계약의 완결**을 뜻합니다. 실제 수입 적격성, 품목분류, 법률·인허가, 계약 또는 운영 승인을 뜻하지 않습니다.
