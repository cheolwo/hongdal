using 살뜰.Services.External.PublicData;
using 살뜰.Services.External.PublicData.Korea;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddKoreaAdministrativeRegionPublicDataProviders(
        this IServiceCollection services)
    {
        services.AddOptions<대한민국법정동CodeOptions>()
            .BindConfiguration(대한민국법정동CodeOptions.SectionName)
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
                                 && uri.Scheme == Uri.UriSchemeHttps,
                "대한민국 법정동코드 기준 주소는 HTTPS 절대 주소여야 합니다.")
            .Validate(options => options.MaxArchiveBytes is >= 1024 and <= 50 * 1024 * 1024,
                "대한민국 법정동코드 압축파일 크기 제한이 올바르지 않습니다.")
            .Validate(options => options.MaxExpandedBytes >= options.MaxArchiveBytes
                                 && options.MaxExpandedBytes <= 100 * 1024 * 1024,
                "대한민국 법정동코드 압축 해제 크기 제한이 올바르지 않습니다.")
            .Validate(options => options.MaxRecordCount is >= 1_000 and <= 1_000_000,
                "대한민국 법정동코드 자료 건수 제한이 올바르지 않습니다.");

        services.AddSingleton<IExternalDataSourceRegistration, 대한민국법정동CodeSourceRegistration>();
        services.AddHttpClient<대한민국법정동CodeCollector>(client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-Korea-PublicData/1.0");
        });
        services.AddScoped<IExternalDataCollector>(provider =>
            provider.GetRequiredService<대한민국법정동CodeCollector>());
        services.AddScoped<IExternalDataNormalizer, 대한민국법정동CodeNormalizer>();
        services.AddScoped<대한민국법정동행정구역원장승격Service>();
        return services;
    }
}
