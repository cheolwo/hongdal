using Serilog;
using Ssalddel.FoodApi.Options;
using Ssalddel.Contracts.Food;
using Ssalddel.FoodApi.Application;
using Ssalddel.FoodApi.Application.DeliveryTickets;
using Ssalddel.FoodApi.Application.DeliveryTickets.Commands;
using Ssalddel.FoodApi.Application.DeliveryTickets.Handlers;
using Ssalddel.FoodApi.Application.Orders.Commands;
using Ssalddel.FoodApi.Application.Orders.Events;
using Ssalddel.FoodApi.Application.Orders.Handlers;
using Ssalddel.FoodApi.Application.Pricing;
using Ssalddel.FoodApi.Application.Settlements;
using Ssalddel.FoodApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Ssalddel.FoodApi")
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/ssalddel-foodapi-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<배차연동Options>(builder.Configuration.GetSection(배차연동Options.SectionName));
builder.Services.Configure<KakaoLocalOptions>(builder.Configuration.GetSection(KakaoLocalOptions.SectionName));
builder.Services.Configure<FoodDeliveryPricingOptions>(builder.Configuration.GetSection(FoodDeliveryPricingOptions.SectionName));
builder.Services.AddHttpClient<I음식배차큐연동Service, 음식배차큐연동Service>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<배차연동Options>>().Value;
    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl);
    }
});
builder.Services.AddHttpClient<IKakao좌표변환Service, Kakao좌표변환Service>();
builder.Services.AddSingleton<음식샘플Store>();
builder.Services.AddSingleton<배차주소샘플Store>();
builder.Services.AddSingleton<IFoodDeliveryTicketMemoryIndex, FoodDeliveryTicketMemoryIndex>();
builder.Services.AddSingleton<IFoodDeliverySettlementStore, FoodDeliverySettlementMemoryStore>();
builder.Services.AddSingleton<IFoodDeliveryPricingPolicyStore, FoodDeliveryPricingPolicyMemoryStore>();
builder.Services.AddScoped<IFoodDeliveryPricingService, FoodDeliveryPricingService>();
builder.Services.AddScoped<IFoodDeliveryTicketRecommendationService, FoodDeliveryTicketRecommendationService>();
builder.Services.AddScoped<IFoodEventPublisher, FoodEventPublisher>();
builder.Services.AddScoped<IFoodCommandHandler<음식주문등록Command, 음식주문응답>, 음식주문등록CommandHandler>();
builder.Services.AddScoped<IFoodCommandHandler<음식주문배차대기요청Command, 음식주문응답?>, 음식주문배차대기요청CommandHandler>();
builder.Services.AddScoped<IFoodCommandHandler<음식배달완료정산반영Command, FoodDeliverySettlementApplyResult>, 음식배달완료정산반영CommandHandler>();
builder.Services.AddScoped<IFoodEventHandler<음식주문등록됨Event>, 음식점신규주문알림EventHandler>();
builder.Services.AddScoped<IFoodEventHandler<음식주문배차대기요청됨Event>, 음식배달권인덱싱EventHandler>();
builder.Services.AddScoped<IFoodEventHandler<음식주문배차대기요청됨Event>, 음식배차큐연동EventHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
