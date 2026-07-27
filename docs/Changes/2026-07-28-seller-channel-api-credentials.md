# 판매자 외부 채널 API 자격증명 보안 입력

## 결과

- `SmartStore`, `Coupang`, `Shopify`, `Amazon`의 자격증명 입력 필드를 공유 contract catalog로 정의해 판매자 앱과 서버 adapter가 같은 key를 사용하게 했다.
- 판매자는 채널을 선택한 뒤 상점명과 API별 애플리케이션 ID, API Key, Secret, Token, Marketplace ID 등을 입력할 수 있다.
- 앱은 공통 보호 전송 client로 요청하고, 서버는 판매채널 전용 Data Protection purpose로 JSON 문서를 암호화해 기존 `토큰암호화저장값`에 저장한다.
- 목록·상세 응답에는 비밀값 원문을 포함하지 않고 필드별 설정 여부와 끝 네 자리 마스킹 값만 반환한다.
- 서버 내부 `ISalesChannelCredentialProvider`만 복호화된 값을 읽을 수 있다. 향후 채널별 typed client가 이 경계에서 값을 받아 Header, Query, Body, Endpoint 또는 서명 값으로 조립한다.
- 자격증명을 저장해도 외부 연결 확인, 상품 발행, 주문 수집은 자동 실행하지 않는다.

## 화면 확인

간접 확인이다. Windows MAUI 앱을 실제 실행해 판매자 셸과 홈이 즉시 렌더링되는 것을 접근성 트리로 확인했다. 인증된 판매채널 입력 화면은 운영 서버와 판매자 세션이 없는 로컬 검증 환경에서 직접 제출하지 않았으며, 공통 Razor 빌드와 ViewModel 테스트로 채널별 동적 필드 구성을 확인했다.

## 검증

- `Ssalddel`, `Ssalddel.Ui.Common`, `SellerApp` Windows 빌드
- 채널별 schema, 전용 Data Protection, 암호화 저장, 마스킹 응답, 내부 adapter 조회 경계 테스트
- 기존 판매채널 계정 권한·client·페이지 ViewModel 회귀 테스트
