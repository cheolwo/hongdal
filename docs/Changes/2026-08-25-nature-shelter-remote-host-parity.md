# Nature 생활거점 외부 RemoteHost 동등성 검증

## 범위

- 대상 폐루프: `playable-loop:nature-shelter-foundation.v1`
- 실행 위치: `LocalProcess`와 별도 `dotnet` 프로세스의 `RemoteHost`
- 서버 주소: 검증 중에만 연 `http://127.0.0.1:5279`
- 실행 모드: `Simulation`
- 외부 자료·운영 DB·Provider 호출: 비활성

## 실행한 흐름

동일한 세션 생성 요청과 명령 식별자를 두 실행 위치에 적용했다.

```text
도끼 획득
→ 벌목 1초 진행
→ 취소
→ 같은 나무 재시도
→ 나무 3개 벌목 완료
→ 오두막 배치
→ 건설 완료
→ 입장
→ 퇴장
→ 저장
→ Replay verification
```

각 Preview의 확정 가능 여부와 주요 결과, 각 Confirm·실시간 진행 뒤 revision, 최종 Nature 상태, 저장 schema·revision·replay hash를 비교했다.

## 결과

| 항목 | 결과 |
| --- | --- |
| 검증 상태 | 통과 |
| 최종 revision | `15` |
| Replay 명령 수 | `15` |
| 저장 schema | `simulation-save.v23` |
| Replay hash | `83a5582724e76d95f8a3344bf86ee7ea56703edbe708aaaf1fa920a131ede0e0` |
| 도끼 | 보유 |
| 통나무 | 오두막 건설에 6개 사용 후 `0` |
| 그루터기 | `3` |
| 오두막 | `Completed` |
| 최종 위치 상태 | 오두막 밖 |
| 진행 중 작업 | 없음 |

서버의 세션 생성, Preview, Confirm, 상태 조회, 저장, Replay verification 요청은 모두 성공했고 검증 뒤 전용 포트를 닫았다. 상세 자동 산출물은 로컬 전용 `artifacts/local/validation/nature-remote-host-parity/`에 있으며 저장소에는 포함하지 않는다.

2026-08-26에는 현재 작업 트리의 `LocalSimulationRuntime`·Nature 계약·Save/Replay 변경을 반영해 이 경로를 전용 회귀 `SimulationNature생활거점동등성Tests`로 고정했다. 취소 전 2초 진행을 포함한 15 revision 전체 경로, Local slot 복원과 RemoteHost `replay-verifications`가 `1/1` 통과했고 Nature·Local Runtime 집중 회귀도 `44/44` 통과했다. 최신 원시 결과는 `artifacts/local/validation/nature-e7/nature-shelter-parity-current.trx`에 둔다.

## 증거 경계

이 결과는 RemoteHost HTTP·JSON 경계와 LocalProcess의 결정적 상태·저장 동등성 증거다. canonical Scene의 사람 수동 조작과 Game View는 별도 UnityPlayMode EvidencePackage가 소유한다. 두 증거를 함께 적용해 `WI-NATURE-05`와 대상 폐루프를 E7로 닫으며, 연결된 재생 종단점이 없어 수행하지 못한 실제 청음은 선택형 감각 수용 항목으로 분리한다. Unity 배치 모드의 범위 밖 경고나 운영 Provider·운영 DB 효과를 이 증거로 해소했다고 보지 않는다.
