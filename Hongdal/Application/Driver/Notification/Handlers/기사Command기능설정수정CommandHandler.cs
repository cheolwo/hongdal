using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Hongdal.Application.CommandProcessing;
using 홍달.도메인.설정;

namespace Hongdal.Application.Driver.Notification;

public sealed class 기사Command기능설정수정CommandHandler : IRequestHandler<기사Command기능설정수정Command, Result<Unit>>
{
    private readonly HongdalContext _db;
    private readonly ICommand기능설정Resolver _resolver;
    private readonly ICommand기능CatalogResolver _catalogResolver;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I참여자실행권한검사 _권한검사;

    public 기사Command기능설정수정CommandHandler(
        HongdalContext db,
        ICommand기능설정Resolver resolver,
        ICommand기능CatalogResolver catalogResolver,
        ICurrentUserAccessor currentUserAccessor,
        I참여자실행권한검사 권한검사)
    {
        _db = db;
        _resolver = resolver;
        _catalogResolver = catalogResolver;
        _currentUserAccessor = currentUserAccessor;
        _권한검사 = 권한검사;
    }

    public async Task<Result<Unit>> Handle(기사Command기능설정수정Command request, CancellationToken cancellationToken)
    {
        if (!_권한검사.Try검증(_currentUserAccessor.UserId, _currentUserAccessor.Role, request.참여자Id, request.실행역할, out var 권한오류))
        {
            return Result.Fail<Unit>(권한오류);
        }

        if (!_catalogResolver.IsSupportedDriverCommand(request.CommandName) || !_catalogResolver.IsSupportedFeature(request.FeatureName))
        {
            return Result.Fail<Unit>("지원하지 않는 Command 또는 기능입니다.");
        }

        var policy = _catalogResolver.GetFeatures().First(x => string.Equals(x.FeatureName, request.FeatureName, StringComparison.Ordinal));
        if (!policy.IsUserConfigurable)
        {
            return Result.Fail<Unit>("사용자가 변경할 수 없는 Command 기능입니다.");
        }

        var now = DateTime.UtcNow;
        var entity = await _db.사용자Command기능설정.FirstOrDefaultAsync(x =>
            x.사용자Id == request.사용자Id
            && x.CommandName == request.CommandName
            && x.FeatureName == request.FeatureName,
            cancellationToken);

        if (entity is null)
        {
            entity = new 사용자Command기능설정
            {
                사용자Id = request.사용자Id,
                CommandName = request.CommandName,
                FeatureName = request.FeatureName,
                IsEnabled = request.IsEnabled,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.사용자Command기능설정.Add(entity);
        }
        else
        {
            entity.IsEnabled = request.IsEnabled;
            entity.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _resolver.Invalidate(request.사용자Id, request.CommandName);
        return Result.Ok(Unit.Value);
    }
}
