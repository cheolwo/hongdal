using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Hongdal.Application.CommandProcessing;

namespace Hongdal.Application.Driver.Notification;

public sealed class 기사Command기능설정기본값복원CommandHandler : IRequestHandler<기사Command기능설정기본값복원Command, Result<Unit>>
{
    private readonly HongdalContext _db;
    private readonly ICommand기능설정Resolver _resolver;
    private readonly ICommand기능CatalogResolver _catalogResolver;

    public 기사Command기능설정기본값복원CommandHandler(HongdalContext db, ICommand기능설정Resolver resolver, ICommand기능CatalogResolver catalogResolver)
    {
        _db = db;
        _resolver = resolver;
        _catalogResolver = catalogResolver;
    }

    public async Task<Result<Unit>> Handle(기사Command기능설정기본값복원Command request, CancellationToken cancellationToken)
    {
        if (!_catalogResolver.IsSupportedDriverCommand(request.CommandName) || !_catalogResolver.IsSupportedFeature(request.FeatureName))
        {
            return Result.Fail<Unit>("지원하지 않는 Command 또는 기능입니다.");
        }

        var entity = await _db.사용자Command기능설정.FirstOrDefaultAsync(x =>
            x.사용자Id == request.사용자Id
            && x.CommandName == request.CommandName
            && x.FeatureName == request.FeatureName,
            cancellationToken);

        if (entity is not null)
        {
            _db.사용자Command기능설정.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        _resolver.Invalidate(request.사용자Id, request.CommandName);
        return Result.Ok(Unit.Value);
    }
}
