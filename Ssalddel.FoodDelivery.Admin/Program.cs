using Ssalddel.BusinessPackages.AdminUi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.Configure<BusinessPackageAdminOptions>(builder.Configuration.GetSection(BusinessPackageAdminOptions.SectionName));
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapRazorComponents<PackageAdminApp>();
app.Run();
