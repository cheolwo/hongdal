using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class Bls평균소매가격ArchiveServiceTests
{
    [Fact]
    public void Series목록은_2026년관측이확인된_전국식품56개계열을제공한다()
    {
        using var httpClient = CreateHttpClient(new RecordingHandler(SuccessResponse));
        using var db = CreateDb();
        var service = CreateService(httpClient, db);

        var result = service.GetSeriesCatalog();

        Assert.Equal(56, result.Count);
        Assert.Contains(result, item =>
            item.SeriesId == "APU0000701312"
            && item.ProductNameKo == "장립종 백미"
            && item.OriginalUnit == "lb");
        Assert.Contains(result, item => item.SeriesId == "APU0000FS1101");
        Assert.DoesNotContain(result, item => item.ItemCode == "711111");
        Assert.DoesNotContain(result, item => item.ItemCode == "712404");
    }

    [Fact]
    public void Kamis비교Catalog는_56개계열을모두검토하고_직접비교경계를분리한다()
    {
        using var httpClient = CreateHttpClient(new RecordingHandler(SuccessResponse));
        using var db = CreateDb();
        var service = CreateService(httpClient, db);

        var result = service.GetKamisComparisonCatalog();

        Assert.Equal(56, result.BlsSeriesCount);
        Assert.Equal(43, result.SeriesWithCandidateCount);
        Assert.Equal(15, result.DirectComparableCandidateSeriesCount);
        Assert.Equal(16, result.UniqueKamisItemCodeCount);
        Assert.Equal(56, result.Items.Count);
        Assert.Contains(result.Items, item =>
            item.SeriesId == "APU0000701312"
            && item.KamisCandidates.Any(candidate =>
                candidate.KamisCategoryCode == "100"
                && candidate.KamisItemCode == "111"
                && candidate.AllowsDirectPriceComparison));
        Assert.Contains(result.Items, item =>
            item.SeriesId == "APU0000FC1101"
            && item.KamisCandidates.Count == 2
            && item.KamisCandidates.All(candidate =>
                candidate.MatchQualityCode
                    == BlsKamis비교품질Codes.광의품목후보
                && !candidate.AllowsDirectPriceComparison));
        Assert.Contains(result.Items, item =>
            item.SeriesId == "APU0000713111"
            && item.KamisCandidates.Single().MatchQualityCode
                == BlsKamis비교품질Codes.가공연관품목);
        Assert.Contains(result.Items, item =>
            item.SeriesId == "APU0000711411"
            && item.KamisCandidates.Count == 0);
    }

    [Fact]
    public async Task 키없는_v1응답을_월별관측으로저장하고_재실행해도중복하지않는다()
    {
        var handler = new RecordingHandler(SuccessResponse);
        using var httpClient = CreateHttpClient(handler);
        await using var db = CreateDb();
        var service = CreateService(httpClient, db);
        var request = new Bls평균소매가격수집요청
        {
            YearFrom = 2026,
            YearTo = 2026
        };

        var first = await service.CollectAsync(request);
        var second = await service.CollectAsync(request);

        Assert.Equal(3, first.FetchedCount);
        Assert.Equal(3, first.InsertedCount);
        Assert.Equal(0, first.UpdatedCount);
        Assert.Equal(0, second.InsertedCount);
        Assert.Equal(0, second.UpdatedCount);
        Assert.Equal(3, second.ExistingCount);
        Assert.Equal(3, await db.BlsAverageRetailPriceObservations.CountAsync());
        Assert.Equal(2, await db.BlsAverageRetailPriceCollectionRuns.CountAsync());
        Assert.All(
            await db.BlsAverageRetailPriceCollectionRuns.ToArrayAsync(),
            run => Assert.Equal(Bls평균소매가격Archive상태Codes.완료, run.StatusCode));

        var rice = await db.BlsAverageRetailPriceObservations.SingleAsync(item =>
            item.SeriesId == "APU0000701312"
            && item.ReferenceMonth == new DateOnly(2026, 6, 1));
        Assert.Equal(1.089m, rice.PriceUsd);
        Assert.Equal("lb", rice.OriginalUnit);
        Assert.Equal("USD", rice.CurrencyCode);
        Assert.False(rice.IsValueMissing);
        Assert.Equal("bls-ap:APU0000701312:2026:M06", rice.RecordKey);

        Assert.Equal(6, handler.Requests.Count);
        Assert.All(handler.Requests, captured =>
        {
            Assert.Equal(HttpMethod.Post, captured.Method);
            Assert.Equal(
                "/publicAPI/v1/timeseries/data/",
                captured.RequestUri!.AbsolutePath);
            Assert.Contains("\"startyear\":\"2026\"", captured.Body, StringComparison.Ordinal);
            Assert.DoesNotContain("registrationkey", captured.Body, StringComparison.OrdinalIgnoreCase);
            using var document = JsonDocument.Parse(captured.Body);
            Assert.InRange(
                document.RootElement.GetProperty("seriesid").GetArrayLength(),
                1,
                25);
        });
        Assert.Contains(
            handler.Requests,
            captured => captured.Body.Contains(
                "\"APU0000701312\"",
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task 변경된공식값은_같은RecordKey를갱신하고_원문Json차이만으로는갱신하지않는다()
    {
        var handler = new RecordingHandler(
            SuccessResponse,
            SuccessResponse,
            SuccessResponse,
            UpdatedResponse,
            UpdatedResponse,
            UpdatedResponse,
            UpdatedResponseWithEquivalentBusinessValue,
            UpdatedResponseWithEquivalentBusinessValue,
            UpdatedResponseWithEquivalentBusinessValue);
        using var httpClient = CreateHttpClient(handler);
        await using var db = CreateDb();
        var service = CreateService(httpClient, db);
        var request = new Bls평균소매가격수집요청
        {
            YearFrom = 2026,
            YearTo = 2026
        };

        await service.CollectAsync(request);
        var updated = await service.CollectAsync(request);
        var equivalent = await service.CollectAsync(request);

        Assert.Equal(1, updated.UpdatedCount);
        Assert.Equal(0, equivalent.UpdatedCount);
        Assert.Equal(3, equivalent.ExistingCount);
        var rice = await db.BlsAverageRetailPriceObservations.SingleAsync(item =>
            item.RecordKey == "bls-ap:APU0000701312:2026:M06");
        Assert.Equal(1.111m, rice.PriceUsd);
        Assert.Equal("Revised", rice.Footnote);
    }

    [Fact]
    public async Task Archive조회는_원통화와원단위를보존하고_없는계열을자료없음으로표시한다()
    {
        using var httpClient = CreateHttpClient(new RecordingHandler(SuccessResponse));
        await using var db = CreateDb();
        var service = CreateService(httpClient, db);
        await service.CollectAsync(new Bls평균소매가격수집요청
        {
            YearFrom = 2026,
            YearTo = 2026
        });

        var rice = await service.GetArchiveAsync(new Bls평균소매가격ArchiveQuery
        {
            CanonicalProductKey = "rice-white-long-grain",
            YearFrom = 2026,
            YearTo = 2026
        });
        var none = await service.GetArchiveAsync(new Bls평균소매가격ArchiveQuery
        {
            SeriesId = "NOT-REGISTERED"
        });

        Assert.Equal(Bls평균소매가격상태Codes.완료, rice.StatusCode);
        Assert.Equal(2, rice.TotalCount);
        Assert.All(rice.Items, item =>
        {
            Assert.Equal("USD", item.CurrencyCode);
            Assert.Equal("lb", item.OriginalUnit);
        });
        Assert.Equal(Bls평균소매가격상태Codes.자료없음, none.StatusCode);
        Assert.Empty(none.Items);
    }

    [Fact]
    public async Task API실패는_실패Run만남기고_관측값을부분저장하지않는다()
    {
        using var httpClient = CreateHttpClient(new RecordingHandler(
            """
            {
              "status": "REQUEST_FAILED",
              "message": ["Request limit exceeded"],
              "Results": {}
            }
            """));
        await using var db = CreateDb();
        var service = CreateService(httpClient, db);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CollectAsync(new Bls평균소매가격수집요청
            {
                YearFrom = 2026,
                YearTo = 2026
            }));

        Assert.Contains("Request limit exceeded", exception.Message, StringComparison.Ordinal);
        Assert.Empty(await db.BlsAverageRetailPriceObservations.ToArrayAsync());
        var run = await db.BlsAverageRetailPriceCollectionRuns.SingleAsync();
        Assert.Equal(Bls평균소매가격Archive상태Codes.실패, run.StatusCode);
        Assert.Contains("Request limit exceeded", run.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("Request limit exceeded", run.SourceMessagesJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 무등록일일한도도달은_Fred의동일Bls계열Csv로투명하게수집한다()
    {
        var handler = new FredFallbackHandler();
        using var httpClient = CreateHttpClient(handler);
        await using var db = CreateDb();
        var service = CreateService(httpClient, db);

        var result = await service.CollectAsync(new Bls평균소매가격수집요청
        {
            YearFrom = 2026,
            YearTo = 2026
        });

        Assert.Equal(Bls평균소매가격상태Codes.완료, result.StatusCode);
        Assert.Equal(56, result.RequestedSeriesCount);
        Assert.Equal(112, result.FetchedCount);
        Assert.Equal(112, result.InsertedCount);
        Assert.Contains(
            result.SourceMessages,
            message => message.Contains("FRED CSV", StringComparison.Ordinal));
        Assert.Equal(7, handler.Requests.Count);
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.All(handler.Requests.Skip(1), request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("fred.stlouisfed.org", request.RequestUri!.Host);
            var seriesIds = Uri.UnescapeDataString(request.RequestUri.Query)
                .Split("id=", StringSplitOptions.RemoveEmptyEntries)
                .Last()
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            Assert.InRange(seriesIds.Length, 1, 10);
        });

        var rice = await db.BlsAverageRetailPriceObservations.SingleAsync(item =>
            item.RecordKey == "bls-ap:APU0000701312:2026:M01");
        var missingBanana = await db.BlsAverageRetailPriceObservations.SingleAsync(item =>
            item.RecordKey == "bls-ap:APU0000711211:2026:M02");
        Assert.Equal(1.059m, rice.PriceUsd);
        Assert.Contains("fred.stlouisfed.org", rice.SourceUrl, StringComparison.Ordinal);
        Assert.True(missingBanana.IsValueMissing);
        Assert.Null(missingBanana.PriceUsd);
        var run = await db.BlsAverageRetailPriceCollectionRuns.SingleAsync();
        Assert.Equal(Bls평균소매가격Archive상태Codes.완료, run.StatusCode);
        Assert.Contains("FRED CSV", run.QuerySummary, StringComparison.Ordinal);
        Assert.Equal(
            "https://fred.stlouisfed.org/graph/fredgraph.csv",
            run.SourceUrl);
    }

    [Fact]
    public async Task FredCsv가_요청계열을일부만반환하면_실패하고부분저장하지않는다()
    {
        var handler = new RecordingHandler(
            DailyThresholdResponse,
            """
            observation_date,APU0000701111,APU0000702111
            2026-01-01,4.921,5.275
            """);
        using var httpClient = CreateHttpClient(handler);
        await using var db = CreateDb();
        var service = CreateService(httpClient, db);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CollectAsync(new Bls평균소매가격수집요청
            {
                YearFrom = 2026,
                YearTo = 2026
            }));

        Assert.Contains("요청 계열 8개가 누락", exception.Message, StringComparison.Ordinal);
        Assert.Empty(await db.BlsAverageRetailPriceObservations.ToArrayAsync());
        var run = await db.BlsAverageRetailPriceCollectionRuns.SingleAsync();
        Assert.Equal(Bls평균소매가격Archive상태Codes.실패, run.StatusCode);
        Assert.Contains("요청 계열 8개가 누락", run.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(2, handler.Requests.Count);
    }

    private static Bls평균소매가격ArchiveService CreateService(
        HttpClient httpClient,
        AgriculturalFisheriesDbContext db)
        => new(
            httpClient,
            db,
            new FixedTimeProvider(
                new DateTimeOffset(2026, 7, 29, 1, 0, 0, TimeSpan.Zero)),
            NullLogger<Bls평균소매가격ArchiveService>.Instance);

    private static HttpClient CreateHttpClient(HttpMessageHandler handler)
        => new(handler)
        {
            BaseAddress = new Uri("https://api.bls.gov/")
        };

    private static AgriculturalFisheriesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class RecordingHandler(params string[] responses) : HttpMessageHandler
    {
        private readonly Queue<string> _responses = new(responses);

        public List<CapturedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new CapturedRequest(request.Method, request.RequestUri, body));
            var response = _responses.Count > 1
                ? _responses.Dequeue()
                : _responses.Peek();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed record CapturedRequest(
        HttpMethod Method,
        Uri? RequestUri,
        string Body);

    private sealed class FredFallbackHandler : HttpMessageHandler
    {
        public List<CapturedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new CapturedRequest(request.Method, request.RequestUri, body));
            if (request.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        DailyThresholdResponse,
                        Encoding.UTF8,
                        "application/json")
                };
            }

            var query = Uri.UnescapeDataString(request.RequestUri!.Query);
            var seriesIds = query
                .Split("id=", StringSplitOptions.RemoveEmptyEntries)
                .Last()
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            var csv = new StringBuilder();
            csv.Append("observation_date,")
                .AppendJoin(',', seriesIds)
                .AppendLine();
            AppendFredRow(csv, "2026-01-01", seriesIds, isFebruary: false);
            AppendFredRow(csv, "2026-02-01", seriesIds, isFebruary: true);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    csv.ToString(),
                    Encoding.UTF8,
                    "text/csv")
            };
        }

        private static void AppendFredRow(
            StringBuilder csv,
            string observationDate,
            IReadOnlyList<string> seriesIds,
            bool isFebruary)
        {
            csv.Append(observationDate);
            foreach (var seriesId in seriesIds)
            {
                csv.Append(',');
                if (seriesId == "APU0000701312")
                {
                    csv.Append(isFebruary ? "1.074" : "1.059");
                }
                else if (seriesId == "APU0000711211")
                {
                    csv.Append(isFebruary ? string.Empty : "0.620");
                }
                else
                {
                    csv.Append("1.000");
                }
            }

            csv.AppendLine();
        }
    }

    private const string SuccessResponse =
        """
        {
          "status": "REQUEST_SUCCEEDED",
          "responseTime": 24,
          "message": [],
          "Results": {
            "series": [
                {
                  "seriesID": "APU0000701312",
                  "data": [
                    {
                      "year": "2026",
                      "period": "M13",
                      "periodName": "Annual",
                      "value": "1.000",
                      "footnotes": [{}]
                    },
                    {
                      "year": "2026",
                      "period": "M06",
                      "periodName": "June",
                      "value": "1.089",
                      "footnotes": [{}]
                    },
                    {
                      "year": "2026",
                      "period": "M05",
                      "periodName": "May",
                      "value": "1.055",
                      "footnotes": [{}]
                    }
                  ]
                },
                {
                  "seriesID": "APU0000711211",
                  "data": [
                    {
                      "year": "2026",
                      "period": "M06",
                      "periodName": "June",
                      "value": "0.645",
                      "footnotes": [
                        { "code": "P", "text": "Preliminary" }
                      ]
                    }
                  ]
                }
            ]
          }
        }
        """;

    private const string UpdatedResponse =
        """
        {
          "status": "REQUEST_SUCCEEDED",
          "message": [],
          "Results": {
            "series": [
                {
                  "seriesID": "APU0000701312",
                  "data": [
                    {
                      "year": "2026",
                      "period": "M06",
                      "periodName": "June",
                      "value": "1.111",
                      "footnotes": [{ "text": "Revised" }]
                    },
                    {
                      "year": "2026",
                      "period": "M05",
                      "periodName": "May",
                      "value": "1.055",
                      "footnotes": [{}]
                    }
                  ]
                },
                {
                  "seriesID": "APU0000711211",
                  "data": [
                    {
                      "year": "2026",
                      "period": "M06",
                      "periodName": "June",
                      "value": "0.645",
                      "footnotes": [
                        { "code": "P", "text": "Preliminary" }
                      ]
                    }
                  ]
                }
            ]
          }
        }
        """;

    private const string UpdatedResponseWithEquivalentBusinessValue =
        """
        {
          "status": "REQUEST_SUCCEEDED",
          "responseTime": 999,
          "message": [],
          "Results": {
            "series": [
                {
                  "seriesID": "APU0000701312",
                  "data": [
                    {
                      "footnotes": [{ "text": "Revised" }],
                      "value": "1.111",
                      "periodName": "June",
                      "period": "M06",
                      "year": "2026"
                    },
                    {
                      "value": "1.055",
                      "year": "2026",
                      "period": "M05",
                      "periodName": "May",
                      "footnotes": [{}]
                    }
                  ]
                },
                {
                  "seriesID": "APU0000711211",
                  "data": [
                    {
                      "value": "0.645",
                      "year": "2026",
                      "period": "M06",
                      "periodName": "June",
                      "footnotes": [
                        { "text": "Preliminary", "code": "P" }
                      ]
                    }
                  ]
                }
            ]
          }
        }
        """;

    private const string DailyThresholdResponse =
        """
        {
          "status": "REQUEST_NOT_PROCESSED",
          "message": [
            "Request could not be serviced, as the daily threshold for total number of requests allocated to the user with registration key  has been reached."
          ],
          "Results": {}
        }
        """;

}
