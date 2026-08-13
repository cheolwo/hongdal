using 살뜰.Services.External.PublicData;
using 살뜰.Services.External.PublicData.Korea;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddKoreaAdministrativeRegionPublicDataProviders(
        this IServiceCollection services)
    {
        services.AddKoreaLegalDongProvider();
        services.AddKoreaAdministrativeJurisdictionProvider();
        services.AddKoreaBuildingAndLicensedBusinessLedgers();
        return services;
    }

    private static void AddKoreaLegalDongProvider(this IServiceCollection services)
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
    }

    private static void AddKoreaAdministrativeJurisdictionProvider(
        this IServiceCollection services)
    {
        services.AddOptions<대한민국행정동관할CodeOptions>()
            .BindConfiguration(대한민국행정동관할CodeOptions.SectionName)
            .Validate(options => Uri.TryCreate(options.ArchiveUrl, UriKind.Absolute, out var uri)
                                 && uri.Scheme == Uri.UriSchemeHttps
                                 && uri.Host.EndsWith("mois.go.kr", StringComparison.OrdinalIgnoreCase),
                "대한민국 행정동 관할코드 원본은 행정안전부 HTTPS 주소여야 합니다.")
            .Validate(options => options.MaxArchiveBytes is >= 1024 and <= 50 * 1024 * 1024,
                "대한민국 행정동 관할코드 압축파일 크기 제한이 올바르지 않습니다.")
            .Validate(options => options.MaxExpandedBytes >= options.MaxArchiveBytes
                                 && options.MaxExpandedBytes <= 200 * 1024 * 1024,
                "대한민국 행정동 관할코드 압축 해제 크기 제한이 올바르지 않습니다.")
            .Validate(options => options.MaxRecordCount is >= 10_000 and <= 1_000_000,
                "대한민국 행정동 관할코드 자료 건수 제한이 올바르지 않습니다.");
        services.AddSingleton<IExternalDataSourceRegistration,
            대한민국행정동관할CodeSourceRegistration>();
        services.AddHttpClient<대한민국행정동관할CodeCollector>(client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-Korea-PublicData/1.0");
        });
        services.AddScoped<IExternalDataCollector>(provider =>
            provider.GetRequiredService<대한민국행정동관할CodeCollector>());
        services.AddScoped<IExternalDataNormalizer, 대한민국행정동관할CodeNormalizer>();
    }

    private static void AddKoreaBuildingAndLicensedBusinessLedgers(
        this IServiceCollection services)
    {
        services.AddScoped<건축물주용도분류원장Service>();
        services.AddScoped<건축물형태구성원장Service>();
        services.AddScoped<VWorld건물통합정보ImportService>();
        services.AddScoped<평창군공공공간원본등록Service>();
        services.AddSingleton<IExternalDataSourceRegistration, 지방행정인허가사업장SourceRegistration>();
        services.AddScoped<지방행정인허가사업장ImportService>();
        services.AddScoped<공개사업장건축물연결Service>();
    }
}
