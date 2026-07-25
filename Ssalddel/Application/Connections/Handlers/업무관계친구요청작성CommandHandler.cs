using System.Text.Json;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Connections.Commands;
using Ssalddel.Domain.HumanResources;
using 살뜰.도메인.사용자;
using 살뜰.도메인.설정;

namespace Ssalddel.Application.Connections.Handlers;

public sealed class 업무관계친구요청작성CommandHandler
    : IRequestHandler<업무관계친구요청작성Command, Result<long>>
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 업무관계친구요청작성CommandHandler(
        SsalddelContext db,
        ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<long>> Handle(
        업무관계친구요청작성Command request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserAccessor.UserId;
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Result.Fail<long>("로그인 사용자 정보를 확인할 수 없습니다.");
        }

        var purpose = request.요청목적?.Trim() ?? string.Empty;
        var message = request.요청메시지?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(purpose))
        {
            return Result.Fail<long>("친구 요청을 보내는 목적을 입력해 주세요.");
        }

        if (purpose.Length > 300 || message.Length > 1000)
        {
            return Result.Fail<long>("요청 목적은 300자, 메시지는 1000자 이내로 입력해 주세요.");
        }

        var snapshot = await _db.WorkRelationshipSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.업무관계스냅샷Id, cancellationToken);
        var currentUserIsActor = string.Equals(
            snapshot?.ActorUserId,
            currentUserId,
            StringComparison.Ordinal);
        var currentUserIsCounterparty = string.Equals(
            snapshot?.CounterpartyUserId,
            currentUserId,
            StringComparison.Ordinal);
        if (snapshot is null || (!currentUserIsActor && !currentUserIsCounterparty))
        {
            return Result.Fail<long>("현재 사용자의 업무 관계 기록을 찾을 수 없습니다.");
        }

        if (!string.Equals(
                snapshot.PrivacyLevel,
                WorkRelationshipPrivacyLevels.ConnectionRequestEligible,
                StringComparison.Ordinal))
        {
            return Result.Fail<long>("이 업무 관계 기록은 친구 요청에 사용할 수 없습니다.");
        }

        var targetUserId = currentUserIsActor
            ? snapshot.CounterpartyUserId
            : snapshot.ActorUserId;
        var requesterRoleCode = currentUserIsActor
            ? snapshot.ActorRoleCode
            : snapshot.CounterpartyRoleCode;
        var targetRoleCode = currentUserIsActor
            ? snapshot.CounterpartyRoleCode
            : snapshot.ActorRoleCode;
        if (string.IsNullOrWhiteSpace(targetUserId)
            || string.Equals(targetUserId, currentUserId, StringComparison.Ordinal))
        {
            return Result.Fail<long>("연결 요청을 받을 업무 상대를 확인할 수 없습니다.");
        }

        if (!TryMapRole(
                string.IsNullOrWhiteSpace(requesterRoleCode)
                    ? _currentUserAccessor.Role
                    : requesterRoleCode,
                out var requesterRole)
            || !TryMapRole(targetRoleCode, out var counterpartyRole))
        {
            return Result.Fail<long>("업무 관계에 기록된 역할을 친구 요청 역할로 변환할 수 없습니다.");
        }

        var duplicated = await _db.친구요청.AnyAsync(
            x => x.요청자참여자Id == currentUserId
                 && x.대상자참여자Id == targetUserId
                 && x.상태 == 친구요청상태.대기,
            cancellationToken);
        if (duplicated)
        {
            return Result.Fail<long>("이미 이 업무 상대에게 보낸 대기 중 친구 요청이 있습니다.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new 친구요청
        {
            요청자참여자Id = currentUserId,
            요청자역할 = requesterRole,
            대상자참여자Id = targetUserId,
            대상자역할 = counterpartyRole,
            요청목적 = purpose,
            요청메시지 = message,
            상태 = 친구요청상태.대기,
            요청일시 = now
        };

        _db.친구요청.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        _db.Command알림Outbox.Add(new Command알림Outbox
        {
            CommandName = nameof(업무관계친구요청작성CommandHandler),
            // 기존 Outbox consumer 호환을 위해 EventName과 payload key는 유지한다.
            EventName = "업무인연연결요청생성됨",
            FeatureName = "Connection",
            Target = "Participant",
            PayloadJson = JsonSerializer.Serialize(new
            {
                friendRequestId = entity.Id,
                workRelationshipSnapshotId = snapshot.Id,
                인연연결요청Id = entity.Id,
                업무인연스냅샷Id = snapshot.Id,
                entity.요청자참여자Id,
                entity.요청자역할,
                entity.대상자참여자Id,
                entity.대상자역할,
                entity.요청목적,
                entity.요청메시지,
                entity.요청일시
            }),
            Status = "Pending",
            TraceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok(entity.Id);
    }

    private static bool TryMapRole(string? roleCode, out 살뜰역할유형 role)
    {
        var normalized = roleCode?.Trim();
        if (Enum.TryParse(normalized, ignoreCase: true, out role))
        {
            return true;
        }

        role = normalized?.ToLowerInvariant() switch
        {
            "orderer" or "buyer" => 살뜰역할유형.주문자,
            "shipper" or "seller" => 살뜰역할유형.판매자,
            "driver" or "transportoperator" => 살뜰역할유형.기사,
            "warehousemanager" => 살뜰역할유형.창고관리자,
            "customsbroker" => 살뜰역할유형.관세사,
            _ => default
        };

        return role != default;
    }
}
