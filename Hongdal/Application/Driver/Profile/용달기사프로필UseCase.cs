using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Driver.Profile;
using Microsoft.AspNetCore.Http;
using 홍달.Services;

namespace Hongdal.Application.Driver.Profile;

public interface I용달기사프로필UseCase
{
    Task<Result<용달기사등록응답>> 등록Async(string? 기사Id, 용달기사등록요청? request);
    Task<Result<용달기사등록응답>> 내프로필조회Async(string? 기사Id);
}

[HongdalApiWorkflow(HongdalWorkflow.DomesticTransport)]
[HongdalUseCase("기사 프로필 등록/조회", Summary = "기사가 운송 추천과 진행 업무에 참여할 수 있도록 기사 프로필과 역할 상태를 준비합니다.")]
[HongdalUseCaseActor(HongdalActor.Driver)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "기사알림UseCase",
    Condition = "기사 프로필 등록 후 추천, 배차, 운송 진행 알림을 받을 수 있게 하는 경우",
    Summary = "기사 프로필 준비를 푸시 토큰과 알림 설정 흐름으로 확장합니다.")]
public sealed class 용달기사프로필UseCase : I용달기사프로필UseCase
{
    private readonly HongdalContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IDriverPushTokenStore _pushTokenStore;

    public 용달기사프로필UseCase(
        HongdalContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IDriverPushTokenStore pushTokenStore)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _pushTokenStore = pushTokenStore;
    }

    public async Task<Result<용달기사등록응답>> 등록Async(string? 기사Id, 용달기사등록요청? request)
    {
        if (request == null) return Result.Fail<용달기사등록응답>("request body is required");
        if (string.IsNullOrWhiteSpace(request.기사명)) return Result.Fail<용달기사등록응답>("기사명 is required");
        if (string.IsNullOrWhiteSpace(request.연락처)) return Result.Fail<용달기사등록응답>("연락처 is required");
        if (string.IsNullOrWhiteSpace(request.차량)) return Result.Fail<용달기사등록응답>("차량 is required");
        if (string.IsNullOrWhiteSpace(request.주_활동지역)) return Result.Fail<용달기사등록응답>("주_활동지역 is required");
        if (string.IsNullOrWhiteSpace(기사Id)) return 인증실패("기사 인증 정보가 없습니다.");

        var existing = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == 기사Id);
        if (existing != null)
        {
            return 상태실패("이미 등록된 용달기사입니다.", StatusCodes.Status409Conflict);
        }

        var user = await _userManager.FindByIdAsync(기사Id);
        if (user == null)
        {
            return 인증실패("기사 인증 정보가 없습니다.");
        }

        if (!await _roleManager.RoleExistsAsync(역할명.기사))
        {
            var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole(역할명.기사));
            if (!roleCreateResult.Succeeded)
            {
                return Result.Fail<용달기사등록응답>(["기사 역할 생성에 실패했습니다.", .. roleCreateResult.Errors.Select(x => x.Description)]);
            }
        }

        if (!await _userManager.IsInRoleAsync(user, 역할명.기사))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, 역할명.기사);
            if (!addRoleResult.Succeeded)
            {
                return Result.Fail<용달기사등록응답>(["기사 역할 부여에 실패했습니다.", .. addRoleResult.Errors.Select(x => x.Description)]);
            }
        }

        var now = DateTime.UtcNow;
        var driver = new 용달기사
        {
            기사명 = request.기사명.Trim(),
            기사Id = 기사Id,
            상태 = string.IsNullOrWhiteSpace(request.상태) ? "활동중" : request.상태.Trim(),
            연락처 = request.연락처.Trim(),
            차량 = request.차량.Trim(),
            운행상태 = 상태값.기사운행상태.대기,
            주_활동지역 = request.주_활동지역.Trim(),
            메모 = request.메모?.Trim() ?? string.Empty,
            기본복귀지주소 = request.기본복귀지주소?.Trim(),
            기본복귀지위도 = request.기본복귀지위도,
            기본복귀지경도 = request.기본복귀지경도,
            집주소를복귀지로사용허용 = request.집주소를복귀지로사용허용,
            등록일 = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.용달기사.Add(driver);
        await _db.SaveChangesAsync();

        return await 응답생성Async(driver);
    }

    public async Task<Result<용달기사등록응답>> 내프로필조회Async(string? 기사Id)
    {
        if (string.IsNullOrWhiteSpace(기사Id))
        {
            return 인증실패("기사 인증 정보가 없습니다.");
        }

        var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == 기사Id);
        if (driver == null)
        {
            return 상태실패("용달기사 정보를 찾을 수 없습니다.", StatusCodes.Status404NotFound);
        }

        return await 응답생성Async(driver);
    }

    private async Task<용달기사등록응답> 응답생성Async(용달기사 driver)
    {
        var vehicle = await _db.차량제원.AsNoTracking()
            .FirstOrDefaultAsync(x => x.차량코드 == driver.차량 || x.차량명 == driver.차량);
        var pushToken = await _pushTokenStore.GetAsync(driver.기사Id);

        return new 용달기사등록응답
        {
            기사Id = driver.기사Id,
            기사명 = driver.기사명,
            연락처 = driver.연락처,
            차량 = driver.차량,
            차량코드 = vehicle?.차량코드,
            차량명 = vehicle?.차량명,
            주_활동지역 = driver.주_활동지역,
            상태 = driver.상태,
            운행상태 = driver.운행상태,
            등록일 = driver.등록일,
            메모 = driver.메모,
            기본복귀지주소 = driver.기본복귀지주소,
            기본복귀지위도 = driver.기본복귀지위도,
            기본복귀지경도 = driver.기본복귀지경도,
            집주소를복귀지로사용허용 = driver.집주소를복귀지로사용허용,
            푸시토큰등록됨 = !string.IsNullOrWhiteSpace(pushToken)
        };
    }

    private static Result<용달기사등록응답> 인증실패(string message)
        => 상태실패(message, StatusCodes.Status401Unauthorized);

    private static Result<용달기사등록응답> 상태실패(string message, int statusCode)
        => Result.Fail<용달기사등록응답>(new Error(message).WithMetadata("StatusCode", statusCode));
}
