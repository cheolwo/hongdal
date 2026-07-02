using System.Text;
using System.Data.Common;
using Hongdal.Hubs;
using Hongdal.Application.Behaviors;
using Hongdal.Application.CommandProcessing;
using Hongdal.Security;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StackExchange.Redis;
using Serilog;
using Hongdal.Application.Driver.Transport;
using 홍달.Infrastructure;
using Hongdal.Middleware;
using 홍달.Services.Audit;
using 홍달.Services.Documents;
using 홍달.Services.External.Google;
using 홍달.Services.External.KieAi;
using 홍달.Services.Images;
using 홍달.Services.Options;
using 홍달.Services.Sales;
using 홍달.Services.ViewSettings;
using 홍달.Services.Warehouse;

var builder = WebApplication.CreateBuilder(args);
var isRunningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Hongdal.LogisticsApi")
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/hongdal-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14);
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddDataProtection();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Command후처리Behavior<,>));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
{
    throw new InvalidOperationException("Jwt:SecretKey configuration is required.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<AuthTokenService>();

var tossOptions = builder.Configuration.GetSection(TossPaymentsOptions.SectionName).Get<TossPaymentsOptions>() ?? new TossPaymentsOptions();
if (string.IsNullOrWhiteSpace(tossOptions.SecretKey))
{
    throw new InvalidOperationException("TossPayments:SecretKey configuration is required.");
}

builder.Services.Configure<TossPaymentsOptions>(builder.Configuration.GetSection(TossPaymentsOptions.SectionName));
builder.Services.Configure<GoogleCloudStorageOptions>(builder.Configuration.GetSection(GoogleCloudStorageOptions.SectionName));
builder.Services.Configure<KieAiOptions>(builder.Configuration.GetSection(KieAiOptions.SectionName));
builder.Services.Configure<NaverCloudDirectionsOptions>(builder.Configuration.GetSection(NaverCloudDirectionsOptions.SectionName));
builder.Services.Configure<OpinetOptions>(builder.Configuration.GetSection(OpinetOptions.SectionName));
builder.Services.Configure<NtsBusinessRegistrationOptions>(builder.Configuration.GetSection(NtsBusinessRegistrationOptions.SectionName));
builder.Services.Configure<해외제조업소조회Options>(builder.Configuration.GetSection(해외제조업소조회Options.SectionName));
builder.Services.Configure<수입식품제품조회Options>(builder.Configuration.GetSection(수입식품제품조회Options.SectionName));
builder.Services.Configure<기사이용료정책Options>(builder.Configuration.GetSection(기사이용료정책Options.SectionName));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(RedisOptions.SectionName));
builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.Configure<PushNotificationsOptions>(builder.Configuration.GetSection(PushNotificationsOptions.SectionName));
builder.Services.Configure<CommandProcessingOptions>(builder.Configuration.GetSection(CommandProcessingOptions.SectionName));
builder.Services.Configure<CommandFileStorageOptions>(builder.Configuration.GetSection(CommandFileStorageOptions.SectionName));

builder.Services.AddHongdalInfrastructure();
builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
builder.Services.AddScoped<ICommand기능설정Resolver, Command기능설정Resolver>();
builder.Services.AddScoped<ICommand기능CatalogResolver, Command기능CatalogResolver>();
builder.Services.AddScoped<ICommand후처리Processor, Command감사로그Processor>();
builder.Services.AddScoped<ICommand후처리Processor, Command알림의도Processor>();
builder.Services.AddScoped<I사용자행위로그Service, 사용자행위로그Service>();
builder.Services.AddScoped<ISalesChannelService, SalesChannelService>();
builder.Services.AddScoped<IView가시성Service, View가시성Service>();
builder.Services.AddScoped<IWarehouseOperationService, WarehouseOperationService>();
builder.Services.AddScoped<사용자행위로그Middleware>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection configuration is required.");
}

builder.Services.AddDbContext<HongdalContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 4, 0)),
        mysqlOptions =>
        {
            mysqlOptions.MigrationsAssembly("Hongdal");
            mysqlOptions.EnableRetryOnFailure();
        }));

var redisConnectionString = builder.Configuration.GetSection(RedisOptions.SectionName).GetValue<string>(nameof(RedisOptions.ConnectionString))
                            ?? Environment.GetEnvironmentVariable("Redis__ConnectionString");
if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    throw new InvalidOperationException("Redis:ConnectionString configuration is required.");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

var mongoOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>() ?? new MongoDbOptions();
var mongoConnectionString = string.IsNullOrWhiteSpace(mongoOptions.ConnectionString)
    ? Environment.GetEnvironmentVariable("MongoDb__ConnectionString")
    : mongoOptions.ConnectionString;
if (string.IsNullOrWhiteSpace(mongoConnectionString))
{
    throw new InvalidOperationException("MongoDb:ConnectionString configuration is required.");
}

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<HongdalContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs/dispatch-recommendations"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("서버관리자전용", policy => policy.RequireRole(역할명.서버관리자));
    options.AddPolicy("용달기사전용", policy => policy.RequireRole(역할명.용달기사, 역할명.기사));
    options.AddPolicy("화주또는판매자전용", policy => policy.RequireRole(역할명.화주, 역할명.판매자));
    options.AddPolicy("물류운영사용자전용", policy => policy.RequireRole(역할명.용달기사, 역할명.기사, 역할명.화주, 역할명.창고관리자, 역할명.서버관리자));
    options.AddPolicy("창고관리자전용", policy => policy.RequireRole(역할명.창고관리자, 역할명.서버관리자));
    options.AddPolicy("운영사용자전용", policy => policy.RequireRole(역할명.화주, 역할명.판매자, 역할명.창고관리자, 역할명.서버관리자));
});

builder.Services.AddHttpClient<IGeocodingService, GoogleGeocodingService>();
builder.Services.AddHttpClient<IRouteDistanceService, GoogleRouteDistanceService>();
builder.Services.AddHttpClient<INaverCloudDirectionsService, NaverCloudDirectionsService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NaverCloudDirectionsOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddHttpClient<IOpinetAveragePriceService, OpinetAveragePriceService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpinetOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddHttpClient<INtsBusinessRegistrationService, NtsBusinessRegistrationService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NtsBusinessRegistrationOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddHttpClient<I해외제조업소조회Service, 해외제조업소조회Service>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<해외제조업소조회Options>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddHttpClient<I수입식품제품조회Service, 수입식품제품조회Service>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<수입식품제품조회Options>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddHttpClient<ITossPaymentsService, TossPaymentsService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TossPaymentsOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddHttpClient<IKieAiImageGenerationClient, KieAiImageGenerationClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KieAiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddHttpClient<IDriverRecommendationPushService, FcmDriverRecommendationPushService>();
builder.Services.AddSingleton<IGoogleCloudStorageService, GoogleCloudStorageService>();
builder.Services.AddSingleton<IDriverLocationStore, DriverLocationStore>();
builder.Services.AddSingleton<IDriverWorkQueueStore, RedisDriverWorkQueueStore>();
builder.Services.AddSingleton<IDriverRejectedRequestStore, RedisDriverRejectedRequestStore>();
builder.Services.AddSingleton<IDriverPushTokenStore, RedisDriverPushTokenStore>();
builder.Services.AddSingleton<IDriverRecommendationPushStateStore, RedisDriverRecommendationPushStateStore>();
builder.Services.AddSingleton<IDriverCallScopeStore, RedisDriverCallScopeStore>();
builder.Services.AddSingleton<IDriverNotificationSettingsStore, RedisDriverNotificationSettingsStore>();
builder.Services.AddSingleton<ICommandFileStoragePathResolver, CommandFileStoragePathResolver>();
builder.Services.AddSingleton<IDispatchRecommendationLogStore, DispatchRecommendationLogStore>();
builder.Services.AddSingleton<IDispatchAcceptanceLogStore, DispatchAcceptanceLogStore>();
builder.Services.AddSingleton<I탐색캠페인이벤트저장소, 탐색캠페인이벤트저장소>();
builder.Services.AddSingleton<IAdminFilePodStore, AdminFilePodStore>();
builder.Services.AddSingleton<I문서관리Store, 문서관리Store>();
builder.Services.AddSingleton<I문서관리Service, 문서관리Service>();
builder.Services.AddSingleton<이미지프롬프트생성기Resolver, 기본이미지프롬프트생성기Resolver>();
builder.Services.AddScoped<I샘플이미지대상ResolverResolver, 샘플이미지대상ResolverResolver>();
builder.Services.AddSingleton<I이미지프롬프트생성기, 화주상품사진프롬프트생성기>();
builder.Services.AddSingleton<I이미지프롬프트생성기, 기사상차인증사진프롬프트생성기>();
builder.Services.AddSingleton<I이미지프롬프트생성기, 기사배차완료인증사진프롬프트생성기>();
builder.Services.AddSingleton<I이미지프롬프트생성기, 음식상품썸네일프롬프트생성기>();
builder.Services.AddSingleton<I이미지프롬프트생성기, 주문후기사진프롬프트생성기>();
builder.Services.AddScoped<I샘플이미지대상Resolver, 판매상품샘플이미지대상Resolver>();
builder.Services.AddScoped<I샘플이미지생성Service, 샘플이미지생성Service>();
builder.Services.AddScoped<I배차추천경로Service, 배차추천경로Service>();
builder.Services.AddScoped<I기사운송일정구성Service, 기사운송일정구성Service>();
builder.Services.AddScoped<I운송일정삽입평가Service, 운송일정삽입평가Service>();
builder.Services.AddScoped<I배차추천판정Service, 배차추천판정Service>();
builder.Services.AddScoped<I배차추천평가Service, 배차추천평가Service>();
builder.Services.AddScoped<홍달.Services.Dispatch.Recommendation.I차량화물적합성Service, 홍달.Services.Dispatch.Recommendation.차량화물적합성Service>();
builder.Services.AddScoped<Hongdal.Application.Shipper.Request.I차량추천Service, Hongdal.Application.Shipper.Request.차량추천Service>();
builder.Services.AddScoped<Hongdal.Application.Shipper.Request.I화주운송의뢰추천Service, Hongdal.Application.Shipper.Request.화주운송의뢰추천Service>();
builder.Services.AddScoped<Hongdal.Application.Shipper.Request.I화주운송의뢰일괄등록파서Service, Hongdal.Application.Shipper.Request.화주운송의뢰일괄등록파서Service>();
builder.Services.AddScoped<I판매상품샘플시드Service, 판매상품샘플시드Service>();
builder.Services.AddScoped<I배차추천Service, 화물배차추천Service>();
builder.Services.AddScoped<I음식배차추천Service, 음식배차추천Service>();
builder.Services.AddScoped<I운행중배차추천Service, 운행중배차추천Service>();
builder.Services.AddScoped<I비운행중배차추천Service, 비운행중배차추천Service>();
builder.Services.AddScoped<I기사운송상태전이Service, 기사운송상태전이Service>();
builder.Services.AddScoped<I탐색대상추천Service, 탐색대상추천Service>();
builder.Services.AddScoped<I탐색캠페인상태전이Service, 탐색캠페인상태전이Service>();
builder.Services.AddScoped<INationalDispatchRequestService, NationalDispatchRequestService>();
builder.Services.AddScoped<I기사월정산Service, 기사월정산Service>();
builder.Services.AddHostedService<KieAiTaskPollingWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HongdalContext>();
    await InitializeDatabaseAsync(db, app.Services, app.Logger);
}

if (!isRunningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
        diagnosticContext.Set("UserName", httpContext.User?.Identity?.Name ?? string.Empty);
    };
});

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var errors = validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(e => e.ErrorMessage).ToArray());

            var problem = new ValidationProblemDetails(errors)
            {
                Title = "요청값 검증에 실패했습니다.",
                Status = StatusCodes.Status400BadRequest
            };

            await context.Response.WriteAsJsonAsync(problem);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "서버 오류가 발생했습니다.",
            Status = StatusCodes.Status500InternalServerError
        });
    });
});

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<사용자행위로그Middleware>();
app.MapControllers();
app.MapHub<DispatchRecommendationHub>("/hubs/dispatch-recommendations");

app.Run();

static async Task InitializeDatabaseAsync(HongdalContext db, IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger)
{
    var migrationDelays = new[]
    {
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(20),
        TimeSpan.FromSeconds(30)
    };

    for (var attempt = 0; attempt <= migrationDelays.Length; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            break;
        }
        catch (Exception ex) when (attempt < migrationDelays.Length)
        {
            var delay = migrationDelays[attempt];
            logger.LogWarning(ex, "MySQL migration failed on attempt {Attempt}. Retrying in {Delay}.", attempt + 1, delay);
            await Task.Delay(delay);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "MySQL migration failed after {Attempt} attempts. Application will continue without applying migrations at startup.", attempt + 1);
            return;
        }
    }

    await EnsureIdentityCompatibilityAsync(db, logger);
    await EnsureVehicleRateCompatibilityAsync(db, logger);

    try
    {
        await IdentityDataSeeder.SeedAsync(services);
        var viewVisibilityService = services.GetRequiredService<IView가시성Service>();
        await viewVisibilityService.SeedPoliciesAsync();
        var documentService = services.GetRequiredService<I문서관리Service>();
        await documentService.SeedDefaultsAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Initial data seeding failed after database migration.");
    }
}

static async Task EnsureIdentityCompatibilityAsync(HongdalContext db, Microsoft.Extensions.Logging.ILogger logger)
{
    var connection = db.Database.GetDbConnection();

    try
    {
        await db.Database.OpenConnectionAsync();

        if (!await ColumnExistsAsync(connection, "AspNetUsers", "BusinessRegistrationNumber"))
        {
            await using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = "ALTER TABLE `AspNetUsers` ADD COLUMN `BusinessRegistrationNumber` varchar(256) NULL;";
            await alterCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Added missing column AspNetUsers.BusinessRegistrationNumber.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Identity schema compatibility check failed.");
    }
    finally
    {
        await db.Database.CloseConnectionAsync();
    }
}

static async Task<bool> ColumnExistsAsync(DbConnection connection, string tableName, string columnName)
{
    await using var command = connection.CreateCommand();
    command.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @tableName
  AND COLUMN_NAME = @columnName;";

    var tableParam = command.CreateParameter();
    tableParam.ParameterName = "@tableName";
    tableParam.Value = tableName;
    command.Parameters.Add(tableParam);

    var columnParam = command.CreateParameter();
    columnParam.ParameterName = "@columnName";
    columnParam.Value = columnName;
    command.Parameters.Add(columnParam);

    var result = await command.ExecuteScalarAsync();
    return Convert.ToInt32(result) > 0;
}

static async Task EnsureVehicleRateCompatibilityAsync(HongdalContext db, Microsoft.Extensions.Logging.ILogger logger)
{
    var connection = db.Database.GetDbConnection();

    try
    {
        await db.Database.OpenConnectionAsync();

        if (!await TableExistsAsync(connection, "차량단가"))
        {
            await using var createCommand = connection.CreateCommand();
            createCommand.CommandText = @"
CREATE TABLE `차량단가` (
    `차량종류` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    PRIMARY KEY (`차량종류`)
) CHARACTER SET=utf8mb4;";
            await createCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Created missing table 차량단가.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Vehicle rate schema compatibility check failed.");
    }
    finally
    {
        await db.Database.CloseConnectionAsync();
    }
}

static async Task<bool> TableExistsAsync(DbConnection connection, string tableName)
{
    await using var command = connection.CreateCommand();
    command.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @tableName;";

    var tableParam = command.CreateParameter();
    tableParam.ParameterName = "@tableName";
    tableParam.Value = tableName;
    command.Parameters.Add(tableParam);

    var result = await command.ExecuteScalarAsync();
    return Convert.ToInt32(result) > 0;
}
