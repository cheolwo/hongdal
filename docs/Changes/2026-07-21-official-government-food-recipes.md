# 각국 정부 공식 음식 레시피 아카이브

## 변경 기록

| 변경 축 | 화면 변경 여부 | 확인 내용 |
| --- | --- | --- |
| 출처·권리 정책 | 화면 없음 | 한국·일본·영국 자동 수집 원천과 미국·캐나다·프랑스 메타데이터 전용 원천을 국가·언어·약관·갱신 주기와 함께 DB seed로 고정 |
| 대표 음식·레시피 변형 | 화면 없음 | 국가+음식명의 대표 후보와 출처+원천 ID의 레시피 변형을 분리하고 수집 당시 권리 snapshot·원문·checksum 저장 |
| 순서별 수집 | 화면 없음 | 식약처 JSON → 농사로 XML → 일본 MAFF HTML → NHS JSON-LD 수집기와 관리자 API·명령행 실행 연결 |
| 운영 경계 | 화면 없음 | 사진 파일 저장, 자동 대표 선정, 커뮤니티 자동 게시, 원장·주문·결제·배달 생성 차단 |

## 검증

- `Ssalddel` 서버 build 경고 0개·오류 0개
- 식약처 JSON, 농사로 XML, MAFF HTML, NHS JSON-LD parser fixture
- 동일 원천 ID 재수집 멱등성, metadata-only 자동 수집 거부, NHS 7일 freshness 제외
- AgriculturalFisheries 전용 Context migration 생성 및 model snapshot 갱신
- 화면 변경이 없어 PNG는 추가하지 않음
