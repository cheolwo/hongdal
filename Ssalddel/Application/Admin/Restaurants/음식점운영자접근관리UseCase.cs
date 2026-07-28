using System.Security.Claims;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Admin.Restaurants;
using Ssalddel.Contracts.Food;
using 살뜰.Data;

namespace Ssalddel.Application.Admin.Restaurants;

public interface I음식점운영자접근관리UseCase
{
    Task<Result<음식점운영자접근응답>> 조회Async(
        string userId,
        CancellationToken cancellationToken);

    Task<Result<음식점운영자접근응답>> 배정Async(
        음식점운영자접근배정요청 request,
        CancellationToken cancellationToken);

    Task<Result<음식점운영자접근응답>> 해제Async(
        음식점운영자접근배정요청 request,
        CancellationToken cancellationToken);
}

public sealed class 음식점운영자접근관리UseCase(
    SsalddelContext db,
    UserManager<ApplicationUser> userManager) : I음식점운영자접근관리UseCase
{
    public async Task<Result<음식점운영자접근응답>> 조회Async(
        string userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedUserId = Clean(userId);
        if (normalizedUserId is null)
        {
            return BadRequest("사용자 ID를 확인해 주세요.");
        }

        var user = await userManager.FindByIdAsync(normalizedUserId);
        return user is null
            ? NotFound("음식점 접근을 조회할 사용자를 찾지 못했습니다.")
            : Result.Ok(await ToResponseAsync(user));
    }

    public async Task<Result<음식점운영자접근응답>> 배정Async(
        음식점운영자접근배정요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var userId = Clean(request.UserId);
        if (userId is null || request.음식점Id <= 0)
        {
            return BadRequest("사용자 ID와 음식점 ID를 확인해 주세요.");
        }

        if (!await db.음식점공개프로필
                .AsNoTracking()
                .AnyAsync(restaurant => restaurant.Id == request.음식점Id, cancellationToken))
        {
            return NotFound("접근 범위를 배정할 음식점을 찾지 못했습니다.");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound("음식점 접근을 배정할 사용자를 찾지 못했습니다.");
        }

        if (!await userManager.IsInRoleAsync(user, 역할명.음식점))
        {
            var roleResult = await userManager.AddToRoleAsync(user, 역할명.음식점);
            if (!roleResult.Succeeded)
            {
                return IdentityFailure("음식점 역할을 부여하지 못했습니다.", roleResult);
            }
        }

        var claims = await userManager.GetClaimsAsync(user);
        var managedClaims = claims
            .Where(claim => claim.Type == 음식점접근ClaimTypes.음식점Id)
            .ToArray();
        if (managedClaims.Length > 0)
        {
            var removeResult = await userManager.RemoveClaimsAsync(user, managedClaims);
            if (!removeResult.Succeeded)
            {
                return IdentityFailure("기존 음식점 접근 범위를 정리하지 못했습니다.", removeResult);
            }
        }

        var addResult = await userManager.AddClaimAsync(
            user,
            new Claim(
                음식점접근ClaimTypes.음식점Id,
                request.음식점Id.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        if (!addResult.Succeeded)
        {
            return IdentityFailure("음식점 접근 범위를 저장하지 못했습니다.", addResult);
        }

        return Result.Ok(await ToResponseAsync(user));
    }

    public async Task<Result<음식점운영자접근응답>> 해제Async(
        음식점운영자접근배정요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var userId = Clean(request.UserId);
        if (userId is null || request.음식점Id <= 0)
        {
            return BadRequest("사용자 ID와 음식점 ID를 확인해 주세요.");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound("음식점 접근을 해제할 사용자를 찾지 못했습니다.");
        }

        var restaurantId = request.음식점Id.ToString(
            System.Globalization.CultureInfo.InvariantCulture);
        var claims = await userManager.GetClaimsAsync(user);
        var targetClaims = claims
            .Where(claim => claim.Type == 음식점접근ClaimTypes.음식점Id
                            && claim.Value == restaurantId)
            .ToArray();
        if (targetClaims.Length == 0)
        {
            return NotFound("사용자에게 해당 음식점 접근 범위가 배정되어 있지 않습니다.");
        }

        var removeClaimsResult = await userManager.RemoveClaimsAsync(user, targetClaims);
        if (!removeClaimsResult.Succeeded)
        {
            return IdentityFailure("음식점 접근 범위를 해제하지 못했습니다.", removeClaimsResult);
        }

        if (await userManager.IsInRoleAsync(user, 역할명.음식점))
        {
            var removeRoleResult = await userManager.RemoveFromRoleAsync(user, 역할명.음식점);
            if (!removeRoleResult.Succeeded)
            {
                return IdentityFailure("음식점 역할을 해제하지 못했습니다.", removeRoleResult);
            }
        }

        return Result.Ok(await ToResponseAsync(user));
    }

    private async Task<음식점운영자접근응답> ToResponseAsync(ApplicationUser user)
    {
        var claims = await userManager.GetClaimsAsync(user);
        var restaurantId = claims
            .Where(claim => claim.Type == 음식점접근ClaimTypes.음식점Id)
            .Select(claim => long.TryParse(claim.Value, out var value) ? value : (long?)null)
            .FirstOrDefault(value => value > 0);
        var hasRole = await userManager.IsInRoleAsync(user, 역할명.음식점);

        return new 음식점운영자접근응답
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            음식점Id = restaurantId,
            음식점역할보유 = hasRole,
            접근가능 = hasRole && restaurantId.HasValue
        };
    }

    private static Result<음식점운영자접근응답> BadRequest(string message)
        => Result.Fail<음식점운영자접근응답>(
            new Error(message).WithMetadata("StatusCode", 400));

    private static Result<음식점운영자접근응답> NotFound(string message)
        => Result.Fail<음식점운영자접근응답>(
            new Error(message).WithMetadata("StatusCode", 404));

    private static Result<음식점운영자접근응답> IdentityFailure(
        string message,
        IdentityResult result)
        => Result.Fail<음식점운영자접근응답>(
            new Error(message)
                .CausedBy(result.Errors.Select(error => error.Description))
                .WithMetadata("StatusCode", 409));

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
