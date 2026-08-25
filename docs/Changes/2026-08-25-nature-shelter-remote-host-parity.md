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

## 증거 경계

이 결과는 실제 TCP·JSON 경계를 지나는 독립 RemoteHost와 LocalProcess의 결정적 상태·저장 동등성 증거다. canonical Scene의 수동 Game View 조작, 실제 청음, 최종 화면 캡처를 대신하지 않는다. Unity 배치 모드에서 관찰된 기존 FarmPlot·HarvestLot Scene 직렬화 자동 보정 메시지와 라이선스 갱신 오류도 이 검증으로 해소됐다고 보지 않는다. 따라서 현재 `WI-NATURE-05`와 부모 폐루프의 E7 상태는 계속 `Partial`이다.
