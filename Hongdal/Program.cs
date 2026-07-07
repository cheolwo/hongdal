using System.Text;
using System.Data.Common;
using Hongdal.Hubs;
using Hongdal.Application.Behaviors;
using Hongdal.Application.CommandProcessing;
using Hongdal.Controllers;
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
using Hongdal.Extensions;
using Quartz;
using 홍달.Infrastructure;
using 홍달.Infrastructure.BackgroundJobs.DispatchQueue;
using 홍달.Infrastructure.BackgroundJobs.Payments;
using Hongdal.Middleware;
using 홍달.Services.Audit;
using Hongdal.Services.Auth;
using 홍달.Services.Documents;
using 홍달.Services.External.Google;
using 홍달.Services.External.KieAi;
using 홍달.Services.Images;
using 홍달.Services.Options;
using 홍달.Services.Sales;
using 홍달.Services.ViewSettings;
using Hongdal.Services.LogisticsProcessing.Warehouse;
using Hongdal.Services.LogisticsProcessing.SalesOrders;
using 홍달.Services.Dispatch.Queue;
using 홍달.Services.Dispatch.Notification;
using 홍달.Services.Notifications;
using 홍달.Services.Payments;
using Hongdal.Services.Driver.Development;
using Hongdal.Services.Development;
using Hongdal.Services.Security;

var builder = WebApplication.CreateBuilder(args);
const string CustomsBrokerCorsPolicy = "CustomsBrokerApp";
var isRunningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);

if (!isRunningInContainer)
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

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

builder.Services.AddHongdalPresentation();
builder.Services.AddHongdalApplicationCore();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CustomsBrokerCorsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("CustomsBrokerCors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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
builder.Services.Configure<CommunityPostStorageOptions>(builder.Configuration.GetSection(CommunityPostStorageOptions.SectionName));
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
builder.Services.Configure<WorkRelationshipSnapshotOptions>(builder.Configuration.GetSection(WorkRelationshipSnapshotOptions.SectionName));
builder.Services.Configure<CommandFileStorageOptions>(builder.Configuration.GetSection(CommandFileStorageOptions.SectionName));
builder.Services.Configure<CustomsOptions>(builder.Configuration.GetSection(CustomsOptions.SectionName));
builder.Services.Configure<PublicDataOptions>(builder.Configuration.GetSection(PublicDataOptions.SectionName));
builder.Services.Configure<VersionFeatureFlagsOptions>(builder.Configuration.GetSection(VersionFeatureFlagsOptions.SectionName));
builder.Services.Configure<SalesChannelOrderSyncOptions>(builder.Configuration.GetSection(SalesChannelOrderSyncOptions.SectionName));
builder.Services.Configure<배차큐정책Options>(builder.Configuration.GetSection("DispatchQueue"));
builder.Services.Configure<배차큐배치작업Options>(builder.Configuration.GetSection(배차큐배치작업Options.SectionName));
builder.Services.AddScoped<I가입온보딩인연후보Service, 가입온보딩인연후보Service>();

var dispatchQueueJobOptions = builder.Configuration.GetSection(배차큐배치작업Options.SectionName).Get<배차큐배치작업Options>() ?? new 배차큐배치작업Options();
var salesOrderSyncOptions = builder.Configuration.GetSection(SalesChannelOrderSyncOptions.SectionName).Get<SalesChannelOrderSyncOptions>() ?? new SalesChannelOrderSyncOptions();

builder.Services.AddHongdalBackgroundJobs(dispatchQueueJobOptions, salesOrderSyncOptions);

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
builder.Services.AddSingleton<IIsmsPTransportKeyStatusStore, RedisIsmsPTransportKeyStatusStore>();

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
    options.AddPolicy("HsCode운영자전용", policy => policy.RequireRole(역할명.서버관리자, 역할명.관세사));
    options.AddPolicy("용달기사전용", policy => policy.RequireRole(역할명.용달기사, 역할명.기사));
    options.AddPolicy("화주또는판매자전용", policy => policy.RequireRole(역할명.화주, 역할명.판매자));
    options.AddPolicy("물류운영사용자전용", policy => policy.RequireRole(역할명.용달기사, 역할명.기사, 역할명.화주, 역할명.창고관리자, 역할명.서버관리자));
    options.AddPolicy("창고관리자전용", policy => policy.RequireRole(역할명.창고관리자, 역할명.서버관리자));
    options.AddPolicy("운영사용자전용", policy => policy.RequireRole(역할명.화주, 역할명.판매자, 역할명.창고관리자, 역할명.서버관리자));
});

builder.Services.AddHongdalHttpClients();
builder.Services.AddHongdalDomainServices();
builder.Services.AddSingleton<Hongdal.Services.Orderer.IRestaurantSearchPolicyStore, Hongdal.Services.Orderer.InMemoryRestaurantSearchPolicyStore>();
builder.Services.AddSingleton<I기사개발스냅샷Provider, InMemory기사개발스냅샷Provider>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HongdalContext>();
    await InitializeDatabaseAsync(db, app.Services, app.Environment, app.Logger);
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
            problem.Extensions["errorCode"] = "ValidationFailed";
            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problem);
            return;
        }

        if (exception is InvalidOperationException invalidOperationException)
        {
            var failure = Result응답확장.실패분류(invalidOperationException.Message);
            context.Response.StatusCode = failure.StatusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Title = invalidOperationException.Message,
                Status = failure.StatusCode,
                Type = failure.Type,
                Detail = failure.Detail,
                Instance = context.Request.Path.Value
            };
            problem.Extensions["errors"] = new[] { invalidOperationException.Message };
            problem.Extensions["errorCode"] = failure.Code;
            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problem);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        var serverProblem = new ProblemDetails
        {
            Title = "서버 오류가 발생했습니다.",
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://httpstatuses.com/500",
            Detail = "예상하지 못한 서버 오류입니다. traceId로 서버 로그를 확인해야 합니다.",
            Instance = context.Request.Path.Value
        };
        serverProblem.Extensions["errorCode"] = "ServerError";
        serverProblem.Extensions["traceId"] = context.TraceIdentifier;
        await context.Response.WriteAsJsonAsync(serverProblem);
    });
});

app.UseAuthentication();
app.UseCors(CustomsBrokerCorsPolicy);
app.UseAuthorization();

app.UseMiddleware<IsmsPEncryptedTransportMiddleware>();
app.UseMiddleware<HrRoleAccessMiddleware>();
app.UseMiddleware<사용자행위로그Middleware>();
app.MapControllers();
app.MapHub<DispatchRecommendationHub>("/hubs/dispatch-recommendations");

app.Run();

static async Task InitializeDatabaseAsync(HongdalContext db, IServiceProvider services, IWebHostEnvironment environment, Microsoft.Extensions.Logging.ILogger logger)
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
    await EnsureHrRoleAssignmentCompatibilityAsync(db, logger);
    await EnsureHrEmploymentContractCompatibilityAsync(db, logger);
    await EnsurePlatformProfitReturnCompatibilityAsync(db, logger);

    try
    {
        await IdentityDataSeeder.SeedAsync(services);
        var viewVisibilityService = services.GetRequiredService<IView가시성Service>();
        await viewVisibilityService.SeedPoliciesAsync();
        var documentService = services.GetRequiredService<I문서관리Service>();
        await documentService.SeedDefaultsAsync();
        if (environment.IsDevelopment())
        {
            await HongdalV1DevelopmentDataSeeder.SeedAsync(services, logger);
        }
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

static async Task EnsureHrRoleAssignmentCompatibilityAsync(HongdalContext db, Microsoft.Extensions.Logging.ILogger logger)
{
    var connection = db.Database.GetDbConnection();

    try
    {
        await db.Database.OpenConnectionAsync();

        if (!await TableExistsAsync(connection, "hr_role_assignments"))
        {
            await using var createCommand = connection.CreateCommand();
            createCommand.CommandText = @"
CREATE TABLE `hr_role_assignments` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `user_id` varchar(450) NOT NULL,
    `scope_type` varchar(100) NOT NULL,
    `scope_id` varchar(200) NOT NULL,
    `participant_category` varchar(100) NOT NULL,
    `role_code` varchar(100) NOT NULL,
    `role_name` varchar(200) NOT NULL,
    `is_active` tinyint(1) NOT NULL,
    `assigned_at_utc` datetime(6) NOT NULL,
    `assigned_by_user_id` varchar(450) NOT NULL,
    `work_schedule_enabled` tinyint(1) NOT NULL,
    `time_zone_id` varchar(100) NOT NULL,
    `allowed_days_of_week` varchar(100) NOT NULL,
    `work_start_local_time` varchar(16) NULL,
    `work_end_local_time` varchar(16) NULL,
    `worksite_ip_restriction_enabled` tinyint(1) NOT NULL,
    `allowed_worksite_ip_ranges` varchar(2000) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;";
            await createCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Created missing table hr_role_assignments.");
        }

        if (!await IndexExistsAsync(connection, "hr_role_assignments", "IX_hr_role_assignments_user_scope_role_active"))
        {
            await using var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = @"
CREATE INDEX `IX_hr_role_assignments_user_scope_role_active`
ON `hr_role_assignments` (`user_id`, `scope_type`, `role_code`, `is_active`);";
            await indexCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Created missing index IX_hr_role_assignments_user_scope_role_active.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "HR role assignment schema compatibility check failed.");
    }
    finally
    {
        await db.Database.CloseConnectionAsync();
    }
}

static async Task EnsureHrEmploymentContractCompatibilityAsync(HongdalContext db, Microsoft.Extensions.Logging.ILogger logger)
{
    var connection = db.Database.GetDbConnection();

    try
    {
        await db.Database.OpenConnectionAsync();

        if (!await TableExistsAsync(connection, "hr_employment_contracts"))
        {
            await using var createContractsCommand = connection.CreateCommand();
            createContractsCommand.CommandText = @"
CREATE TABLE `hr_employment_contracts` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `worker_user_id` varchar(450) NOT NULL,
    `worker_name` varchar(200) NOT NULL,
    `employer_scope_type` varchar(100) NOT NULL,
    `employer_scope_id` varchar(200) NOT NULL,
    `employer_name` varchar(200) NOT NULL,
    `contract_type` varchar(100) NOT NULL,
    `contract_status` varchar(100) NOT NULL,
    `contract_start_date` date NOT NULL,
    `contract_end_date` date NULL,
    `work_description` varchar(1000) NOT NULL,
    `wage_type` varchar(100) NOT NULL,
    `wage_amount` decimal(18,2) NOT NULL,
    `minimum_wage_amount` decimal(18,2) NULL,
    `minimum_wage_check_passed` tinyint(1) NOT NULL,
    `minimum_wage_check_message` varchar(1000) NOT NULL,
    `payment_cycle` varchar(100) NOT NULL,
    `payment_day_of_month` int NOT NULL,
    `payment_method` varchar(100) NOT NULL,
    `bank_name` varchar(100) NOT NULL,
    `account_number` varchar(200) NOT NULL,
    `account_holder_name` varchar(100) NOT NULL,
    `signed_at_utc` datetime(6) NULL,
    `signed_by_user_id` varchar(450) NOT NULL,
    `memo` varchar(2000) NOT NULL,
    `created_at_utc` datetime(6) NOT NULL,
    `updated_at_utc` datetime(6) NOT NULL,
    PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;";
            await createContractsCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Created missing table hr_employment_contracts.");
        }

        if (!await TableExistsAsync(connection, "hr_payroll_schedules"))
        {
            await using var createSchedulesCommand = connection.CreateCommand();
            createSchedulesCommand.CommandText = @"
CREATE TABLE `hr_payroll_schedules` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `contract_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `worker_user_id` varchar(450) NOT NULL,
    `employer_scope_type` varchar(100) NOT NULL,
    `employer_scope_id` varchar(200) NOT NULL,
    `work_period_start_date` date NOT NULL,
    `work_period_end_date` date NOT NULL,
    `scheduled_payment_date` date NOT NULL,
    `planned_amount` decimal(18,2) NOT NULL,
    `currency_code` varchar(10) NOT NULL,
    `payment_method` varchar(100) NOT NULL,
    `status` varchar(100) NOT NULL,
    `memo` varchar(1000) NOT NULL,
    `created_at_utc` datetime(6) NOT NULL,
    `updated_at_utc` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `FK_hr_payroll_schedules_hr_employment_contracts_contract_id`
        FOREIGN KEY (`contract_id`) REFERENCES `hr_employment_contracts` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;";
            await createSchedulesCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Created missing table hr_payroll_schedules.");
        }

        if (!await IndexExistsAsync(connection, "hr_employment_contracts", "IX_hr_employment_contracts_worker_scope_status"))
        {
            await using var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = @"
CREATE INDEX `IX_hr_employment_contracts_worker_scope_status`
ON `hr_employment_contracts` (`worker_user_id`, `employer_scope_type`, `contract_status`);";
            await indexCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Created missing index IX_hr_employment_contracts_worker_scope_status.");
        }

        if (!await IndexExistsAsync(connection, "hr_payroll_schedules", "IX_hr_payroll_schedules_worker_payment_status"))
        {
            await using var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = @"
CREATE INDEX `IX_hr_payroll_schedules_worker_payment_status`
ON `hr_payroll_schedules` (`worker_user_id`, `scheduled_payment_date`, `status`);";
            await indexCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Created missing index IX_hr_payroll_schedules_worker_payment_status.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "HR employment contract schema compatibility check failed.");
    }
    finally
    {
        await db.Database.CloseConnectionAsync();
    }
}

static async Task EnsurePlatformProfitReturnCompatibilityAsync(HongdalContext db, Microsoft.Extensions.Logging.ILogger logger)
{
    var connection = db.Database.GetDbConnection();

    try
    {
        await db.Database.OpenConnectionAsync();

        if (!await TableExistsAsync(connection, "platform_revenue_entries"))
        {
            await using var createCommand = connection.CreateCommand();
            createCommand.CommandText = @"
CREATE TABLE `platform_revenue_entries` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `revenue_source` varchar(100) NOT NULL,
    `source_reference_type` varchar(100) NOT NULL,
    `source_reference_id` varchar(200) NOT NULL,
    `payer_user_id` varchar(450) NOT NULL,
    `related_participant_user_id` varchar(450) NOT NULL,
    `gross_amount` decimal(18,2) NOT NULL,
    `platform_revenue_amount` decimal(18,2) NOT NULL,
    `currency_code` varchar(10) NOT NULL,
    `occurred_at_utc` datetime(6) NOT NULL,
    `memo` varchar(1000) NOT NULL,
    `created_at_utc` datetime(6) NOT NULL,
    PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;";
            await createCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Created missing table platform_revenue_entries.");
        }

        if (!await TableExistsAsync(connection, "platform_profit_return_policies"))
        {
            await using var createCommand = connection.CreateCommand();
            createCommand.CommandText = @"
CREATE TABLE `platform_profit_return_policies` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `policy_name` varchar(200) NOT NULL,
    `target_participant_category` varchar(100) NOT NULL,
    `return_rate_percent` decimal(9,4) NOT NULL,
    `company_reserve_amount` decimal(18,2) NOT NULL,
    `minimum_profit_threshold` decimal(18,2) NOT NULL,
    `effective_start_date` date NOT NULL,
    `effective_end_date` date NULL,
    `is_active` tinyint(1) NOT NULL,
    `memo` varchar(1000) NOT NULL,
    `created_at_utc` datetime(6) NOT NULL,
    `updated_at_utc` datetime(6) NOT NULL,
    PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;";
            await createCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Created missing table platform_profit_return_policies.");
        }

        if (!await TableExistsAsync(connection, "platform_profit_return_schedules"))
        {
            await using var createCommand = connection.CreateCommand();
            createCommand.CommandText = @"
CREATE TABLE `platform_profit_return_schedules` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `policy_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `participant_user_id` varchar(450) NOT NULL,
    `participant_name` varchar(200) NOT NULL,
    `participant_category` varchar(100) NOT NULL,
    `period_start_date` date NOT NULL,
    `period_end_date` date NOT NULL,
    `scheduled_payment_date` date NOT NULL,
    `total_platform_revenue_amount` decimal(18,2) NOT NULL,
    `operating_cost_amount` decimal(18,2) NOT NULL,
    `estimated_profit_amount` decimal(18,2) NOT NULL,
    `return_pool_amount` decimal(18,2) NOT NULL,
    `participant_weight` decimal(18,4) NOT NULL,
    `planned_return_amount` decimal(18,2) NOT NULL,
    `status` varchar(100) NOT NULL,
    `memo` varchar(1000) NOT NULL,
    `created_at_utc` datetime(6) NOT NULL,
    `updated_at_utc` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `FK_platform_profit_return_schedules_policies_policy_id`
        FOREIGN KEY (`policy_id`) REFERENCES `platform_profit_return_policies` (`id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;";
            await createCommand.ExecuteNonQueryAsync();
            logger.LogWarning("Created missing table platform_profit_return_schedules.");
        }

        if (!await IndexExistsAsync(connection, "platform_revenue_entries", "IX_platform_revenue_entries_source_occurred"))
        {
            await using var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = @"
CREATE INDEX `IX_platform_revenue_entries_source_occurred`
ON `platform_revenue_entries` (`revenue_source`, `occurred_at_utc`);";
            await indexCommand.ExecuteNonQueryAsync();
        }

        if (!await IndexExistsAsync(connection, "platform_profit_return_schedules", "IX_platform_profit_return_schedules_participant_payment_status"))
        {
            await using var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = @"
CREATE INDEX `IX_platform_profit_return_schedules_participant_payment_status`
ON `platform_profit_return_schedules` (`participant_user_id`, `scheduled_payment_date`, `status`);";
            await indexCommand.ExecuteNonQueryAsync();
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Platform profit return schema compatibility check failed.");
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

static async Task<bool> IndexExistsAsync(DbConnection connection, string tableName, string indexName)
{
    await using var command = connection.CreateCommand();
    command.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @tableName
  AND INDEX_NAME = @indexName;";

    var tableParam = command.CreateParameter();
    tableParam.ParameterName = "@tableName";
    tableParam.Value = tableName;
    command.Parameters.Add(tableParam);

    var indexParam = command.CreateParameter();
    indexParam.ParameterName = "@indexName";
    indexParam.Value = indexName;
    command.Parameters.Add(indexParam);

    var result = await command.ExecuteScalarAsync();
    return Convert.ToInt32(result) > 0;
}
