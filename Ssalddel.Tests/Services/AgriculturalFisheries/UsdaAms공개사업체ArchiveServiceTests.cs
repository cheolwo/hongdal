using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class UsdaAms공개사업체ArchiveServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 29, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Client는_공식Bulk자료에서_허용된사업체필드만읽는다()
    {
        var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www.usdalocalfoodportal.com")
        };
        var client = new UsdaAms공개사업체DirectoryClient(
            httpClient,
            Options.Create(new PublicDataOptions()));

        var rows = await client.GetDirectoryAsync(
            UsdaAms공개사업체Directory유형Codes.FoodHub);

        var row = Assert.Single(rows);
        Assert.Equal("500000", row.ExternalListingId);
        Assert.Equal("Little Rock Local Food Club", row.BusinessName);
        Assert.Equal(2011, row.EstablishedYear);
        Assert.True(row.HasRetailChannel);
        Assert.True(row.HasProcurementService);
        Assert.Contains("Fresh fruits", row.Products);
        Assert.Contains("Seafood", row.Products);
        Assert.Equal(
            new Uri("https://www.usdalocalfoodportal.com/fe/datasharing/"),
            handler.Request!.Headers.Referrer);
        Assert.Equal(
            "XMLHttpRequest",
            Assert.Single(handler.Request.Headers.GetValues(
                "X-Requested-With")));
        Assert.Contains(
            "directory=foodhub",
            handler.Request.RequestUri!.Query);
    }

    [Fact]
    public async Task 수집은_상세주소와연락처를제외하고_업체와품목을멱등저장한다()
    {
        await using var db = CreateDb();
        var client = new SequenceClient(
        [
            [
                Row(
                    "500000",
                    "Little Rock Local Food Club",
                    "509 Scott St, Little Rock, Arkansas 72201",
                    ["Fresh fruits", "Fresh vegetables"]),
                Row(
                    "500001",
                    "Second Food Hub",
                    "Charleston, West Virginia",
                    ["Seafood"])
            ],
            [
                Row(
                    "500000",
                    "Little Rock Local Food Cooperative",
                    "509 Scott St, Little Rock, Arkansas 72201",
                    ["Fresh fruits", "Seafood"],
                    sourceUpdatedAt: new DateTime(
                        2026,
                        7,
                        29,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc))
            ]
        ]);
        var service = CreateService(client, db);
        var request = new UsdaAms공개사업체수집요청
        {
            DirectoryTypes = [UsdaAms공개사업체Directory유형Codes.FoodHub]
        };

        var first = await service.CollectAsync(request);
        var second = await service.CollectAsync(request);

        Assert.Equal(2, first.InsertedCount);
        Assert.Equal(1, second.UpdatedCount);
        Assert.Equal(1, second.NoLongerListedCount);
        Assert.Equal(2, await db.UsdaAmsPublicBusinessProfiles.CountAsync());
        Assert.Equal(2, await db.UsdaAmsPublicBusinessCollectionRuns.CountAsync());

        var current = await db.UsdaAmsPublicBusinessProfiles
            .Include(item => item.Products)
            .SingleAsync(item => item.ExternalListingId == "500000");
        Assert.Equal("Little Rock", current.CityName);
        Assert.Equal("AR", current.StateCode);
        Assert.DoesNotContain("509 Scott", current.CityName);
        Assert.Equal(
            new[] { "Fresh fruits", "Seafood" },
            current.Products
                .OrderBy(item => item.ProductName)
                .Select(item => item.ProductName));
        Assert.True(current.IsCurrentlyListed);

        var removed = await db.UsdaAmsPublicBusinessProfiles
            .SingleAsync(item => item.ExternalListingId == "500001");
        Assert.Equal("Charleston", removed.CityName);
        Assert.Equal("WV", removed.StateCode);
        Assert.False(removed.IsCurrentlyListed);

        var profilePropertyNames = typeof(UsdaAms공개사업체Profile)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        Assert.DoesNotContain(profilePropertyNames, name =>
            name.Contains("Address", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Phone", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Email", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Latitude", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Longitude", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task 조회는_현재등재와주와취급품목을_같이필터링한다()
    {
        await using var db = CreateDb();
        var service = CreateService(
            new SequenceClient(
            [
                [
                    Row(
                        "500000",
                        "Little Rock Local Food Club",
                        "509 Scott St, Little Rock, Arkansas 72201",
                        ["Fresh fruits", "Seafood"]),
                    Row(
                        "500001",
                        "Different Hub",
                        "Denver, Colorado",
                        ["Fresh fruits"])
                ]
            ]),
            db);
        await service.CollectAsync(new UsdaAms공개사업체수집요청
        {
            DirectoryTypes = [UsdaAms공개사업체Directory유형Codes.FoodHub]
        });
        var queryService = new UsdaAms공개사업체QueryService(db);

        var response = await queryService.SearchAsync(
            new UsdaAms공개사업체조회요청
            {
                SearchText = "Little Rock",
                DirectoryTypeCode = "foodhub",
                StateCode = "ar",
                ProductKey = "Seafood"
            });

        Assert.Equal(1, response.TotalCount);
        var item = Assert.Single(response.Items);
        Assert.Equal("AR", item.StateCode);
        Assert.Contains("Seafood", item.Products);
        Assert.True(item.DiscoveryOnly);
        Assert.True(item.RequiresLiveRecheck);
        Assert.False(response.IsCertificationOrPermitRegistry);
    }

    [Theory]
    [InlineData("FoodHub")]
    [InlineData("foodhub")]
    [InlineData("FOODHUB")]
    public void Directory유형은_코드와외부Slug를_같은내부코드로정규화한다(
        string value)
    {
        var normalized = UsdaAms공개사업체DirectoryCatalog.Normalize(value);

        Assert.Equal(
            UsdaAms공개사업체Directory유형Codes.FoodHub,
            normalized);
        Assert.Equal(
            "foodhub",
            UsdaAms공개사업체DirectoryCatalog.GetSlug(normalized));
    }

    [Fact]
    public async Task 원천실패는_실패Run으로기록한다()
    {
        await using var db = CreateDb();
        var service = CreateService(new FailingClient(), db);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.CollectAsync(new UsdaAms공개사업체수집요청
            {
                DirectoryTypes =
                [
                    UsdaAms공개사업체Directory유형Codes.FoodHub
                ]
            }));

        var run = await db.UsdaAmsPublicBusinessCollectionRuns.SingleAsync();
        Assert.Equal(UsdaAms공개사업체Archive상태Codes.실패, run.StatusCode);
        Assert.Contains("official source unavailable", run.ErrorMessage);
    }

    private static UsdaAms공개사업체원본Row Row(
        string id,
        string name,
        string address,
        IReadOnlyList<string> products,
        DateTime? sourceUpdatedAt = null)
        => new(
            UsdaAms공개사업체Directory유형Codes.FoodHub,
            id,
            name,
            address,
            2011,
            "Non-Profit",
            products,
            true,
            true,
            false,
            true,
            sourceUpdatedAt ?? new DateTime(
                2026,
                7,
                28,
                0,
                0,
                0,
                DateTimeKind.Utc));

    private static UsdaAms공개사업체ArchiveService CreateService(
        IUsdaAms공개사업체DirectoryClient client,
        AgriculturalFisheriesDbContext db)
        => new(
            client,
            db,
            new FixedTimeProvider(Now),
            Options.Create(new PublicDataOptions()),
            NullLogger<UsdaAms공개사업체ArchiveService>.Instance);

    private static AgriculturalFisheriesDbContext CreateDb()
    {
        var options =
            new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class SequenceClient(
        IReadOnlyList<IReadOnlyList<UsdaAms공개사업체원본Row>> responses)
        : IUsdaAms공개사업체DirectoryClient
    {
        private int _index;

        public Task<IReadOnlyList<UsdaAms공개사업체원본Row>> GetDirectoryAsync(
            string directoryTypeCode,
            CancellationToken cancellationToken = default)
        {
            var response = responses[Math.Min(_index, responses.Count - 1)];
            _index++;
            return Task.FromResult(response);
        }
    }

    private sealed class FailingClient : IUsdaAms공개사업체DirectoryClient
    {
        public Task<IReadOnlyList<UsdaAms공개사업체원본Row>> GetDirectoryAsync(
            string directoryTypeCode,
            CancellationToken cancellationToken = default)
            => throw new HttpRequestException("official source unavailable");
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            const string json = """
                [
                  {
                    "listing_id": "500000",
                    "listing_name": "Little Rock Local Food Club",
                    "location_address": "509 Scott St, Little Rock, Arkansas 72201",
                    "establish_year": "2011",
                    "legal_status": "Non-Profit;",
                    "products": "Fresh fruits;Fresh vegetables;",
                    "productslocality_seafood": "Exclusively local;",
                    "saleschannel_retail": "Buying clubs;",
                    "saleschannel_wholesale": "",
                    "service_producer": "",
                    "service_procurement": "Take ownership of the product;",
                    "update_time": "2026-07-28 09:30:00",
                    "contact_email": "must-not-be-read@example.com",
                    "contact_phone": "555-0100"
                  }
                ]
                """;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
