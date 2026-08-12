# Unity 학습 카드 게시·출처 계약

## 결과

`CARD-BIZ-0`의 기술 게이트와 `CARD-BIZ-1` catalog 소비 경계를 구현했다. VideoSearch가 승인된 홍익학당 revision과 Blob 이미지 계보를 immutable publication snapshot으로 만들고, Unity가 같은 SHA-256을 다시 계산한 뒤 기존 저녁 학당 콘텐츠 read model로 투영한다.

실제 승인 데이터는 만들지 않았다. 현재 타로 노트와 직접 인용은 음성 대조·사람 승인 전이므로 바보·전차의 런타임 게시 건수는 0건이다.

## 계약 경계

- schema: `hongik-unity-learning-card-publication.v1`
- 런타임 승인과 원음 대조가 모두 완료되어야 게시 가능
- Notion은 editorial projection이며 런타임 권위가 아님
- 이미지는 공개 URL이 아니라 Blob container·object name·content hash·mime·원출처·license로 추적
- 일반 타로 의미는 별도 source·revision·approval로 분리
- 게임 효과 근거는 `HongikAcademyTranscript`만 허용
- 동일 stable ID와 revision은 덮어쓰지 않고 수정 시 새 revision 추가
- Unity는 schema·stable ID·해시·Blob 경로·효과 allow-list를 fail-closed 검증

## Unity 투영

게시 snapshot은 기존 `저녁학당콘텐츠Snapshot`으로 변환된다. 카드·Notion 검수 행·자막 분석·segment·publication hash 계보가 `SourceStableIds`에 남고 Blob image reference는 별도 read model로 유지된다.

첫 vertical slice의 허용 효과는 다음 두 쌍뿐이다.

- `Awareness + BeginnerMind +1`
- `Resolve + IntegratedProgress +1`

## CARD-BIZ-1 catalog 경계

- VideoSearch endpoint: `GET /api/integration/v1/unity-learning-cards`
- catalog schema: `hongik-unity-learning-card-catalog.v1`
- 승인 snapshot이 0건이면 빈 catalog가 정상 상태다.
- Unity core는 `I학습카드PublicationApiClient → 학습카드PublicationApiRepository → 저녁학당승인카드조회UseCase`로 소비한다.
- Repository는 개별 adapter 검증에 더해 catalog schema와 동일 stable ID·revision 중복을 거부한다.
- `Samples~/LearningCards`의 operational client는 `UnityWebRequest`로 catalog를 받고 JSON wire model을 core API model로 변환한다.
- Base URL의 path base를 보존하고 network·HTTP·JSON 실패를 서로 다른 오류 코드로 보고한다.
- composition root 등록과 Presentation은 다음 단계로 남긴다.

## 검증

- VideoSearch 전체: 152/152 통과
- Unity 전체: 358/358 통과
- Unity 6000.5.6f1 실제 engine reference로 operational client 별도 컴파일 통과, 경고 0
- 양쪽 동일 fixture의 publication hash 일치
- 임시 HTTP 런타임은 기존 장시간 startup seed 때문에 응답 전 timeout되어 미확인
- 실제 Azure Blob 작업, Unity Scene·Presenter·Game View 검증 없음
- commit·push 없음

## 남은 작업

바보·전차 음성 대조와 사람 승인 후 실제 DB Blob 메타데이터를 사용해 첫 두 snapshot을 게시해야 한다. 그 뒤 operational client를 composition root에 등록하고 Presenter를 연결한다.
