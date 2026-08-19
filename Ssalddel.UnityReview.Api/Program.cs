using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Ssalddel.Services.Storage;
using Ssalddel.Services.WorldProjection;
using Ssalddel.UnityReview.Api.Configuration;
using Ssalddel.UnityReview.Api.Controllers;
using Ssalddel.UnityReview.Api.Persistence;
using 살뜰.Services.Options;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = Synty공간조립검토촬영업로드Service.MaximumPngBytes + 256_000);

var accessSection = builder.Configuration.GetSection(UnityReviewAccessOptions.SectionName);
var access = accessSection.Get<UnityReviewAccessOptions>() ?? new UnityReviewAccessOptions();
builder.Services.AddOptions<UnityReviewAccessOptions>()
    .Bind(accessSection)
    .Validate(options => !string.IsNullOrWhiteSpace(options.AdminUserName), "AdminUserName is required.")
    .Validate(options => options.AdminPasswordPbkdf2.Split('.').Length == 3,
        "AdminPasswordPbkdf2 is invalid.")
    .Validate(options =>
    {
        try
        {
            return Convert.FromBase64String(options.JwtSigningKeyBase64).Length >= 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }, "JwtSigningKeyBase64 must contain at least 32 random bytes.")
    .Validate(options => options.TokenLifetimeHours is >= 1 and <= 24,
        "TokenLifetimeHours must be between 1 and 24.")
    .ValidateOnStart();

builder.Services.AddOptions<UnityReviewDatabaseOptions>()
    .Configure(options =>
        options.ConnectionString = builder.Configuration.GetConnectionString("UnityReview") ?? string.Empty)
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString),
        "ConnectionStrings:UnityReview is required.")
    .ValidateOnStart();

byte[] signingKey;
try
{
    signingKey = Convert.FromBase64String(access.JwtSigningKeyBase64);
}
catch (FormatException)
{
    signingKey = [];
}
if (signingKey.Length < 32)
{
    // Options validation reports the actionable startup error without exposing a secret.
    signingKey = new byte[32];
}
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKey),
            ValidateIssuer = true,
            ValidIssuer = access.JwtIssuer,
            ValidateAudience = true,
            ValidAudience = access.JwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});
var controllers = builder.Services.AddControllers();
controllers.PartManager.ApplicationParts.Clear();
controllers.AddApplicationPart(typeof(UnityReviewAuthController).Assembly);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.Configure<ObjectStorageOptions>(options =>
    options.Provider = ObjectStorageProviderNames.Local);
builder.Services.AddSingleton<IObjectStorageService, DevelopmentLocalStorageService>();
builder.Services.AddSingleton<UnityReviewMySqlSchema>();
builder.Services.AddSingleton<ISynty공간조립검토원장Store, MySqlSynty공간조립검토원장Store>();
builder.Services.AddSingleton<ISynty공간조립검토촬영업로드Store,
    MySqlSynty공간조립검토촬영업로드Store>();
builder.Services.AddSingleton<ISynty공간조립모바일검토Service, Synty공간조립모바일검토Service>();
builder.Services.AddSingleton<ISynty공간조립검토촬영업로드Service,
    Synty공간조립검토촬영업로드Service>();

var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
                       | ForwardedHeaders.XForwardedHost
                       | ForwardedHeaders.XForwardedProto
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

var publicImageRoot = Path.Combine(
    app.Environment.ContentRootPath,
    DevelopmentLocalStorageService.PublicStorageDirectoryName);
Directory.CreateDirectory(publicImageRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(publicImageRoot),
    RequestPath = DevelopmentLocalStorageService.PublicRequestPath,
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
        context.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    }
});

app.MapGet("/healthz", async (UnityReviewMySqlSchema schema, CancellationToken cancellationToken) =>
{
    await schema.EnsureInitializedAsync(cancellationToken);
    await using var connection = await schema.OpenConnectionAsync(cancellationToken);
    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT 1;";
    await command.ExecuteScalarAsync(cancellationToken);
    return Results.Ok(new
    {
        status = "Healthy",
        component = "Ssalddel.UnityReview.Api",
        storage = "MySqlAndImmutableLocalVolume"
    });
}).AllowAnonymous();
app.MapControllers();
app.Run();

public partial class Program
{
}
