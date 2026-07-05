using Hongdal.Application.CommandProcessing;
using Hongdal.Contracts.CommandSettings;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Application.Driver.Notification;

public sealed class 기사Command기능설정목록조회QueryHandler : IRequestHandler<기사Command기능설정목록조회Query, Command기능설정목록응답>
{
    private readonly HongdalContext _db;
    private readonly ICommand기능설정Resolver _resolver;
    private readonly ICommand기능CatalogResolver _catalogResolver;

    public 기사Command기능설정목록조회QueryHandler(HongdalContext db, ICommand기능설정Resolver resolver, ICommand기능CatalogResolver catalogResolver)
    {
        _db = db;
        _resolver = resolver;
        _catalogResolver = catalogResolver;
    }

    public async Task<Command기능설정목록응답> Handle(기사Command기능설정목록조회Query request, CancellationToken cancellationToken)
    {
        var overrides = await _db.사용자Command기능설정
            .AsNoTracking()
            .Where(x => x.사용자Id == request.사용자Id)
            .ToListAsync(cancellationToken);

        var items = new List<Command기능설정항목응답>();
        var features = _catalogResolver.GetFeatures();
        foreach (var command in _catalogResolver.GetDriverCommands())
        {
            var defaultRule = _resolver.GetDefaultRule(command.CommandName);
            foreach (var policy in features)
            {
                var featureName = policy.FeatureName;
                var userOverride = overrides.FirstOrDefault(x =>
                    string.Equals(x.CommandName, command.CommandName, StringComparison.Ordinal)
                    && string.Equals(x.FeatureName, featureName, StringComparison.Ordinal));
                var defaultEnabled = Command기능설정Resolver.GetFeatureEnabled(defaultRule, featureName);
                var isEnabled = policy.IsRequired ? true : userOverride?.IsEnabled ?? defaultEnabled;

                items.Add(new Command기능설정항목응답
                {
                    CommandName = command.CommandName,
                    CommandDisplayName = command.DisplayName,
                    FeatureName = featureName,
                    FeatureDisplayName = _catalogResolver.GetFeatureDisplayName(featureName),
                    DefaultEnabled = defaultEnabled,
                    IsEnabled = isEnabled,
                    HasUserOverride = userOverride is not null,
                    IsUserConfigurable = policy.IsUserConfigurable
                });
            }
        }

        return new Command기능설정목록응답 { Items = items };
    }
}
