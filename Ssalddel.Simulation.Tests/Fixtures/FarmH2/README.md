# Farm H2 동결 후보 시험 자료

출처는 공간 담당의 `spatial-support/farm-h2/results`이며 [인계 보고서](../../../docs/Reports/FarmH2-E5호환변환-인계-2026-08-30.md)의 원후보 16파일 중 결과 JSON 4개다. 개발 통합에서 개인 worktree 경로 의존성을 제거하려 복사했다. 원본은 변경하지 않았다.

이 사본은 UTF-8/LF와 마지막 개행으로 정규화했다. 따라서 **파일 byte hash는 원본과 다르며**, 줄 끝을 정규화한 텍스트 전수 비교로 내용 일치를 확인했다. JSON 내부의 원후보 입력/표면/결과 정규형 hash와 `UnapprovedCandidate` 의미는 그대로이며 Adapter 시험이 그 hash를 검증한다. 실제 Unity 측정 또는 승인 패턴으로 사용하지 않는다.

| 파일 | 원본 SHA256 | 저장소 사본 SHA256 |
| --- | --- | --- |
| 01-flat.plan.json | `665E89E09C8DDE5B7AEF491081FB126BA2EF6D97BC98F4D6C148EFD39EE47031` | `C768A65800C9044EAC66322BBAFA5C369C233E111CB62373D989EC25D74698EC` |
| 02-noise.plan.json | `9C116769B18507BA44755C7BD98312C14E2C31E8D8FE722DAE50792C71D39191` | `6C0258272CA449D801AF6B6267C08D32E14C07D2FFD492CC2C1128899E7DBB17` |
| 03-terrace.plan.json | `06B11F7868679501AE823FEF92FCA434CA4E140D573AB5289767C383A0EDB299` | `2CF660CB69F48D23681D3C44593F49468B9ACBB40FA78BFDCBF9732D0DCE9B9E` |
| 04-rejected-slope.plan.json | `257E4A848A7B310A8CA4507DBCDDA8B28EAB8D2AEDDD7EB9B94012FE0A5A2E43` | `37D83D25FAB79C967BFD4C1D9E5BDDB24044ADE2EEE2A60A489D303F0859EA0B` |

프로젝트가 JSON을 시험 출력의 `Fixtures/FarmH2`로 복사한다. 선택적으로 `SSALDDEL_FARM_H2_FIXTURES`를 지정할 수 있으며, 파일이 없거나 내용이 손상됐을 때 새 후보를 생성하는 fallback은 없다.
