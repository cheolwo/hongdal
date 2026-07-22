# 커뮤니티 게시글 음성 변환 파이프라인

## 처리 흐름

1. `커뮤니티게시글UseCase.생성Async`가 게시글과 음성 생성 작업을 같은 MySQL 저장 단위로 기록한다.
2. 저장 성공 후 `커뮤니티게시글등록됨Event`를 발행한다.
3. Event Handler는 외부 API를 직접 호출하지 않고 `커뮤니티게시글음성Worker`에 즉시 처리 신호를 보낸다.
4. Worker는 DB 작업을 임대한 뒤 Typecast TTS를 호출한다.
5. 생성된 음성을 공급자 중립 객체 저장소의 비공개 영역에 올리고 구간별 객체 정보를 DB에 저장한다.
6. 사용자가 음성 정보 또는 파일을 요청하면 접근 로그를 남긴다.

이벤트 신호가 유실되더라도 DB 작업이 남으므로 Worker의 주기 조회에서 복구된다. 게시글 등록 요청은 Typecast와 객체 저장소의 응답을 기다리지 않는다.

## 비용 제한

정규화된 `제목 + 본문`이 100자 이상 500자 미만일 때만 Typecast TTS를 호출한다. 100자 미만 또는 500자 이상이면 작업을 `길이제외`로 종료하므로 API 호출, 재시도, 객체 저장소 업로드 비용이 발생하지 않는다. 경계값은 `CommunityPostAudio:MinCharacters`와 `MaxCharactersExclusive`로 관리한다.

본문 분할기는 Typecast의 요청당 최대 길이를 넘지 않도록 유지하지만, 현재 비용 정책에서는 500자 미만만 허용하므로 일반적으로 음성 파일 한 구간만 생성된다.

## 조회 API

- `GET /api/v1/community/posts/{postId}/audio`: 생성 상태와 재생 가능한 구간 목록
- `GET /api/v1/community/posts/{postId}/audio/segments/{sequence}/download`: 서버를 통한 음성 다운로드

비공개 객체의 실제 주소는 공개 응답에 포함하지 않는다. 다운로드가 서버를 통과하므로 로그인 사용자는 사용자 ID, 익명 사용자는 요청 Trace ID를 기준으로 접근 기록을 남긴다.

## 운영 설정

기본 설정은 비활성화되어 있어 게시글 등록만으로 Typecast 비용이 발생하지 않는다. 운영 전 다음 항목을 로컬 비밀 설정 또는 환경 변수에 지정한다.

```json
{
  "Typecast": {
    "Enabled": true,
    "ApiKey": "LOCAL_SECRET"
  },
  "CommunityPostAudio": {
    "Enabled": true,
    "DefaultVoiceId": "tc_voice_id",
    "ModelVersion": "ssfm-v30",
    "MinCharacters": 100,
    "MaxCharactersExclusive": 500
  },
  "ObjectStorage": {
    "Provider": "AzureBlob"
  },
  "AzureBlobStorage": {
    "ServiceUri": "https://STORAGE_ACCOUNT.blob.core.windows.net",
    "PrivateContainerName": "platform-private"
  }
}
```

`DefaultVoiceId`와 모델 조합은 먼저 Typecast 카탈로그 동기화 API로 저장된 활성 카탈로그에 있어야 한다. 설정이나 카탈로그가 준비되지 않은 작업은 실패시키지 않고 `설정대기` 상태로 유지한다.

## 현재 범위

최초 게시글 등록 시 음성을 생성한다. 게시글 수정에 따른 음성 재생성, 오래된 객체 정리, 관리자 수동 재시도 화면은 후속 범위다.
