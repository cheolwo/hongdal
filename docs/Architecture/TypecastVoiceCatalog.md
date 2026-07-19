# Typecast 음성 캐릭터 카탈로그

## 목적

Typecast 외부 API 계약을 살뜰의 업무 코드와 분리하고, 음성 캐릭터와 지원 모델 정보를 MySQL 카탈로그로 동기화한다. 화면과 다른 업무 모듈은 Typecast 응답을 직접 사용하지 않고 살뜰 계약 DTO를 조회한다.

## 모듈 경계

- `ITypecastClient`: `GET /v2/voices` 및 `POST /v1/text-to-speech` 호출을 담당한다.
- `ITypecast음성카탈로그Service`: 외부 음성 목록을 정규화하고 저장된 카탈로그를 조회한다.
- `ITypecast음성카탈로그저장소`: EF Core와 MySQL 접근을 감춘다.
- `Typecast음성카탈로그Controller`: 서버관리자에게 동기화와 저장 결과 조회 API를 제공한다.

## 저장 구조

- `typecast_voices`: `voice_id`, 이름, 성별, 연령대, 음성 유형, 활성 상태를 저장한다.
- `typecast_voice_models`: 음성별 지원 모델 버전과 감정 목록을 저장한다.
- `typecast_voice_use_cases`: 검색 가능한 음성 용도를 저장한다.

전체 목록 동기화에서 사라진 항목은 삭제하지 않고 비활성화한다. 외부 응답이 비어 있으면 기존 카탈로그를 보호하기 위해 동기화를 실패 처리한다.

## 운영 설정

API 키는 추적되는 설정 파일에 넣지 않는다. `Ssalddel/appsettings.Local.json` 또는 환경 변수에 다음 값을 설정한다.

```json
{
  "Typecast": {
    "Enabled": true,
    "ApiKey": "LOCAL_SECRET"
  }
}
```

동기화는 `POST /api/v1/admin/speech/typecast/voices/sync`, 저장된 목록 조회는 `GET /api/v1/admin/speech/typecast/voices`를 사용한다.

커뮤니티 게시글 음성 Worker는 설정된 `DefaultVoiceId`와 모델 조합이 이 카탈로그에서 활성 상태인지 확인한 뒤 음성 합성을 시작한다. 전체 처리 흐름은 `CommunityPostAudioPipeline.md`를 참고한다.
