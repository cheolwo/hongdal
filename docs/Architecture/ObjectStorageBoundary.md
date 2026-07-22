# 객체 저장소 경계

게시글 첨부, 업무 증빙, POD, 음성, 생성 이미지는 공급자 SDK가 아니라 `IObjectStorageService`에 의존한다.

```text
업로드 UseCase
└─ IObjectStorageService
   ├─ AzureBlobStorageService          운영
   ├─ DevelopmentLocalStorageService   로컬 개발
   └─ GoogleCloudStorageService        선택 가능한 기존 어댑터
```

## 공개와 비공개

- `ObjectStorageAccess.Public`: 공개 게시글 이미지, 상품·메뉴 이미지, 공개용 생성 이미지
- `ObjectStorageAccess.Private`: 운송 증빙, 파일 POD, 게시글 음성, 기사 인증용 생성 이미지
- 공개 객체는 `community-public`, 비공개 객체는 `platform-private` 컨테이너를 기본값으로 사용한다.
- 비공개 객체는 URL을 알고 있어도 익명 요청으로 읽을 수 없다. 다운로드는 권한과 접근 로그를 적용하는 서버 API를 통한다.
- 기존 RDB의 `BucketName` 열과 응답 필드는 호환을 위해 유지하지만, 애플리케이션 내부 결과에서는 공급자 중립적인 `ContainerName`으로 다룬다.

## Azure 운영 인증

운영 서버는 `DefaultAzureCredential`로 Azure VM의 system-assigned Managed Identity를 사용한다. 연결 문자열, Storage Account Key, SAS는 소스와 VM 환경 변수에 저장하지 않는다. VM Identity에는 두 컨테이너 범위의 `Storage Blob Data Contributor`만 부여한다.

```json
{
  "ObjectStorage": {
    "Provider": "AzureBlob"
  },
  "AzureBlobStorage": {
    "ServiceUri": "https://STORAGE_ACCOUNT.blob.core.windows.net",
    "PublicContainerName": "community-public",
    "PrivateContainerName": "platform-private",
    "ManagedIdentityClientId": ""
  }
}
```

system-assigned identity는 `ManagedIdentityClientId`를 비워 둔다. user-assigned identity를 명시적으로 선택할 때만 client ID를 설정한다.

## 로컬 개발과 Google 호환

Development 환경은 `DevelopmentLocalStorageService`로 교체된다. 공개 파일은 `.local-storage`만 정적 제공하고, 비공개 파일은 `.local-private-storage`에 분리해 정적 경로로 노출하지 않는다.

Google 어댑터는 기존 배포의 점진적 이전을 위해 남긴다. `PublicBucketName`과 `PrivateBucketName`을 각각 지정할 수 있고, 비어 있으면 기존 `BucketName`을 호환 기본값으로 사용한다.

## 운영 비용

Azure Blob Storage는 완전 무료 자원이 아니다. 저장 용량, 읽기·쓰기 요청, 외부 전송량에 따라 비용이 발생한다. 현재처럼 소규모 이미지 중심인 미리보기 단계에서는 보통 소액이지만 Azure Cost Management의 실제 사용량과 예산 알림으로 확인한다.
