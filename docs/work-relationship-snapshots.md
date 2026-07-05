# Work Relationship Snapshots

업무 중 접점이 생긴 사람과 업무 행위를 익명화된 형태로 남기는 부가 기능이다. 예를 들어 기사 배차 수락, 창고 입고/검수/포장, 운송 완료 같은 행위에서 “내가 어떤 업무 맥락에서 누구와 연결되었는지”를 나중에 확인할 수 있게 만드는 기록이다.

## 필수 기능과 부가 기능

업무 상태를 전환하는 핵심 처리는 필수 기능으로 둔다. 서비스가 정상적으로 돌아가기 위해 반드시 수행되어야 하므로 관리자 설정으로 끄지 않는다.

- 상차 완료, 하차 완료 같은 상태 전환
- 완료 처리에 필요한 사진 촬영과 파일 업로드
- 입고 완료, 검수, 적재, 포장처럼 재고/운송 상태를 바꾸는 처리
- 감사 로그처럼 운영 추적에 필요한 필수 기록

업무를 더 부드럽게 만들거나 추가적인 편의를 제공하는 처리는 부가 기능으로 둔다. 부가 기능은 관리자 화면에서 전역 또는 사용자별로 켜고 끌 수 있다.

- 인연 스냅샷 기록
- SMS/SNS/Push 알림
- 외부 연동 알림
- 추천, 보조 안내, 후속 제안 같은 업무 보조 처리

## 현재 적용 방식

- `IWorkRelationshipSnapshotCommand`를 구현한 command만 스냅샷 후처리 대상이 된다.
- command handler는 실제 업무 처리가 성공한 뒤 `IWorkRelationshipSnapshotCollector`에 후보 기록을 담는다.
- `Command후처리Behavior`가 명령별 기능 설정을 확인한다.
- `WorkRelationshipSnapshotEnabled`가 켜져 있을 때만 `Command인연스냅샷Processor`가 후보 기록을 저장한다.
- 컨트롤러에서 `IWorkRelationshipSnapshotService`를 직접 호출하지 않는다. 이렇게 해야 관리자 설정을 우회하지 않는다.

현재 예시 적용 command:

- 기사 배차 수락 command

창고 입고/검수/적재/포장 API는 업무 자체가 필수 흐름이므로 계속 처리된다. 다만 인연 스냅샷 기록은 command 후처리 정책으로 옮겨야 하며, 직접 서비스 호출 방식은 사용하지 않는다.

## 설정 우선순위

부가 기능 설정은 다음 순서로 적용된다.

1. `appsettings`의 `CommandProcessing.Defaults`
2. `appsettings`의 command별 기본값
3. 관리자 전역 override (`user_id = "__global__"`)
4. 관리자 사용자별 override

`WorkRelationshipSnapshots.Enabled`는 배포 환경에서 쓰는 마스터 스위치다. 기본적으로는 `true`로 두고, 실제 사용 여부는 command별/사용자별 기능 설정에서 제어한다. 긴급 중단이 필요하면 이 값을 `false`로 내려 전체 저장을 막을 수 있다.

```json
{
  "WorkRelationshipSnapshots": {
    "Enabled": true
  },
  "CommandProcessing": {
    "Defaults": {
      "AuditLogEnabled": true,
      "WorkRelationshipSnapshotEnabled": false
    },
    "Commands": {
      "배차수락Command": {
        "WorkRelationshipSnapshotEnabled": false
      }
    }
  }
}
```

## 관리자 화면

관리자 앱의 `부가 기능 설정` 화면에서 지원 command의 부가 기능을 조정한다.

- 전역 설정: 모든 사용자에게 적용되는 기본 override
- 사용자 설정: 특정 사용자에게만 적용되는 override
- 필수 기능: 화면에 표시되더라도 변경 불가
- 부가 기능: 사용자 설정 가능 항목만 변경 가능

현재 화면은 command 기능 설정부터 다룬다. event/service 단위의 부가 기능도 같은 모델로 확장할 수 있도록 `TargetType` 구조를 열어두었다.

## 조회 API

- `GET /api/v1/work-relationship-snapshots/me`

로그인한 사용자의 스냅샷만 조회한다. 응답에는 익명 라벨, 업무 도메인, 업무 공정, 작업 코드, 연결 엔티티, 메모가 포함된다. 연락처나 실제 사용자 식별자는 공개하지 않는다.

## 확장 방향

- 창고 업무를 MediatR command로 감싸서 입고/검수/적재/포장 스냅샷도 같은 후처리 정책을 타게 한다.
- 상차 완료와 하차 완료 사진 업로드는 필수 완료 증빙으로 유지하고, 그와 별개로 인연 스냅샷 기록 여부만 부가 설정으로 제어한다.
- event/service 단위 부가 기능 카탈로그를 추가해 관리자 화면에서 command 외 기능도 같은 방식으로 조정한다.
- 사용자 본인이 고객센터나 설정 화면을 통해 일부 부가 기능을 끌 수 있는 흐름을 추가한다.
