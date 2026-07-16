using System.Text;
using System.Data.Common;
using System.Globalization;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Hongdal.Infrastructure.Persistence.AgriculturalFisheries;
using Hongdal.Services.AgriculturalFisheries.Information;

var builder = WebApplication.CreateBuilder(args);
const string CustomsWebCorsPolicy = "HongdalWebCustoms";
var isRunningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);

if (!isRunningInContainer)
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
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
    options.AddPolicy(CustomsWebCorsPolicy, policy =>
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

builder.Services.AddScoped<AuthTokenService>();

var executionOptions = builder.Configuration
    .GetSection(HongdalExecutionOptions.SectionName)
    .Get<HongdalExecutionOptions>() ?? new HongdalExecutionOptions();
if (!Enum.IsDefined(executionOptions.Mode))
{
    throw new InvalidOperationException("HongdalExecution:Mode must be Simulation or Operational.");
}

var tossOptions = builder.Configuration.GetSection(TossPaymentsOptions.SectionName).Get<TossPaymentsOptions>() ?? new TossPaymentsOptions();
if (executionOptions.Mode == HongdalExecutionMode.Operational && string.IsNullOrWhiteSpace(tossOptions.SecretKey))
{
    throw new InvalidOperationException("TossPayments:SecretKey configuration is required in Operational mode.");
}

builder.Services.AddHongdalOptions(builder.Configuration);
builder.Services.AddHongdalOperatingMarketServices(builder.Configuration);
builder.Services.AddScoped<I가입온보딩인연후보Service, 가입온보딩인연후보Service>();

var dispatchQueueJobOptions = builder.Configuration.GetSection(배차큐배치작업Options.SectionName).Get<배차큐배치작업Options>() ?? new 배차큐배치작업Options();
var salesOrderSyncOptions = builder.Configuration.GetSection(SalesChannelOrderSyncOptions.SectionName).Get<SalesChannelOrderSyncOptions>() ?? new SalesChannelOrderSyncOptions();
var youTubeOptions = builder.Configuration.GetSection(YouTubeOptions.SectionName).Get<YouTubeOptions>() ?? new YouTubeOptions();
var hongikHakdangCardOptions = builder.Configuration.GetSection(HongikHakdangCardOptions.SectionName).Get<HongikHakdangCardOptions>() ?? new HongikHakdangCardOptions();

builder.Services.AddHongdalBackgroundJobs(dispatchQueueJobOptions, salesOrderSyncOptions, youTubeOptions, hongikHakdangCardOptions, executionOptions);
builder.Services.AddHongdalPersistence(builder.Configuration);

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

                if (!string.IsNullOrWhiteSpace(accessToken) &&
                    (path.StartsWithSegments("/hubs/dispatch-recommendations") ||
                     path.StartsWithSegments(DiagramCollaborationHub.HubPath)))
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

builder.Services.AddAgriculturalFisheriesInformationModule();
builder.Services.AddHongdalHttpClients();
builder.Services.AddHongdalDomainServices();
if (builder.Environment.IsDevelopment())
{
    builder.Services.Replace(ServiceDescriptor.Singleton<IGoogleCloudStorageService, DevelopmentLocalCloudStorageService>());
}
builder.Services.AddSingleton<Hongdal.Services.Orderer.IRestaurantSearchPolicyStore, Hongdal.Services.Orderer.InMemoryRestaurantSearchPolicyStore>();
builder.Services.AddSingleton<I기사개발스냅샷Provider, InMemory기사개발스냅샷Provider>();

var app = builder.Build();
app.Logger.LogInformation("Hongdal execution mode: {ExecutionMode}", executionOptions.Mode);

if (args.Any(argument =>
        string.Equals(argument, "--collect-usda-nass-prices", StringComparison.OrdinalIgnoreCase)))
{
    var yearFromArgument = args.FirstOrDefault(argument =>
        argument.StartsWith("--year-from=", StringComparison.OrdinalIgnoreCase));
    var yearFrom = int.TryParse(
        yearFromArgument?["--year-from=".Length..],
        out var parsedYearFrom)
        ? parsedYearFrom
        : DateTime.UtcNow.Year - 1;

    await using var scope = app.Services.CreateAsyncScope();
    var archiveDb = scope.ServiceProvider.GetRequiredService<AgriculturalFisheriesDbContext>();
    await archiveDb.Database.MigrateAsync();
    var archiveService = scope.ServiceProvider.GetRequiredService<IUsdaNassPriceArchiveService>();
    var archiveResult = await archiveService.CollectRecentMonthlyPricesAsync(yearFrom);
    app.Logger.LogInformation(
        "USDA NASS DB 저장 완료. RunId={RunId}, Fetched={Fetched}, Inserted={Inserted}, Existing={Existing}, Mappings={Mappings}, LatestSourceLoad={LatestSourceLoad}",
        archiveResult.CollectionRunId,
        archiveResult.FetchedCount,
        archiveResult.InsertedCount,
        archiveResult.ExistingCount,
        archiveResult.MappingCount,
        archiveResult.LatestSourceLoadTimeUtc);
    return;
}

if (args.Any(argument =>
        string.Equals(argument, "--collect-kamis-prices", StringComparison.OrdinalIgnoreCase)))
{
    var targetDateArgument = args.FirstOrDefault(argument =>
        argument.StartsWith("--target-date=", StringComparison.OrdinalIgnoreCase));
    var defaultTargetDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    var targetDate = DateOnly.TryParseExact(
        targetDateArgument?["--target-date=".Length..],
        "yyyy-MM-dd",
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out var parsedTargetDate)
        ? parsedTargetDate
        : defaultTargetDate;

    await using var scope = app.Services.CreateAsyncScope();
    var archiveDb = scope.ServiceProvider.GetRequiredService<AgriculturalFisheriesDbContext>();
    await archiveDb.Database.MigrateAsync();
    var archiveService = scope.ServiceProvider.GetRequiredService<IKamisPriceArchiveService>();
    var archiveResult = await archiveService.CollectDailyPricesAsync(targetDate);
    app.Logger.LogInformation(
        "KAMIS 국내 가격 DB 저장 완료. RunId={RunId}, Fetched={Fetched}, Inserted={Inserted}, Updated={Updated}, Existing={Existing}, LatestSurveyDate={LatestSurveyDate}",
        archiveResult.CollectionRunId,
        archiveResult.FetchedCount,
        archiveResult.InsertedCount,
        archiveResult.UpdatedCount,
        archiveResult.ExistingCount,
        archiveResult.LatestSurveyDate);
    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    var localStorageRoot = Path.Combine(
        app.Environment.ContentRootPath,
        DevelopmentLocalCloudStorageService.StorageDirectoryName);
    Directory.CreateDirectory(localStorageRoot);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(localStorageRoot),
        RequestPath = DevelopmentLocalCloudStorageService.RequestPath
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HongdalContext>();
    await InitializeDatabaseAsync(db, scope.ServiceProvider, app.Environment, app.Logger);
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
app.UseCors(CustomsWebCorsPolicy);
app.UseAuthorization();

app.UseMiddleware<IsmsPEncryptedTransportMiddleware>();
app.UseMiddleware<HrRoleAccessMiddleware>();
app.UseMiddleware<사용자행위로그Middleware>();
app.MapControllers();
app.MapHub<DispatchRecommendationHub>("/hubs/dispatch-recommendations");
app.MapHub<RestaurantOrderHub>("/hubs/restaurant-orders");
app.MapHub<DiagramCollaborationHub>(DiagramCollaborationHub.HubPath);

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
            await CommunityLedgerDevelopmentDataSeeder.SeedAsync(services, logger);
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
