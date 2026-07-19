using System.Text.RegularExpressions;
using Ssalddel.Contracts.Common.PublicData;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace 살뜰.Services.External.PublicData;

public sealed class ApartmentManagementFeeLookupService : IApartmentManagementFeeLookupService
{
    private static readonly HashSet<string> MetadataKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "kaptCode", "kaptName", "searchDate", "date", "yyyymm", "month", "resultCode", "resultMsg",
        "pageNo", "numOfRows", "totalCount", "hoCnt", "kaptdPcnt", "householdCount"
    };

    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;
    private readonly IApartmentComplexLookupService _apartmentComplexLookupService;

    public ApartmentManagementFeeLookupService(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options,
        IApartmentComplexLookupService apartmentComplexLookupService)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _apartmentComplexLookupService = apartmentComplexLookupService;
    }

    public async Task<PublicDataLookupResponse<ApartmentManagementFeeSnapshotItem>> GetSnapshotAsync(
        ApartmentManagementFeeSnapshotRequest request,
        CancellationToken cancellationToken = default)
    {
        var month = NormalizeMonth(request.Month);
        if (string.IsNullOrWhiteSpace(request.ComplexCode))
        {
            return Fail("공동주택 단지 코드가 필요합니다.");
        }

        if (month is null)
        {
            return Fail("관리비 기준월은 yyyyMM 형식이어야 합니다.");
        }

        var serviceKey = ResolveServiceKey();
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            return Fail("PublicData:ApartmentManagementFee:ServiceKey 또는 PublicData:DataGoKrServiceKey 설정이 필요합니다.");
        }

        var basic = await _apartmentComplexLookupService.GetBasicInfoAsync(
            new ApartmentComplexBasicRequest { ComplexCode = request.ComplexCode },
            cancellationToken);
        var householdCount = basic.Items.FirstOrDefault()?.HouseholdCount;

        var publicLines = await ReadFeeLinesAsync(
            "PublicManagementFee",
            _options.ApartmentManagementFee.PublicManagementFeePath,
            request.ComplexCode,
            month,
            serviceKey,
            cancellationToken);
        var individualLines = await ReadFeeLinesAsync(
            "IndividualUsageFee",
            _options.ApartmentManagementFee.IndividualUsageFeePath,
            request.ComplexCode,
            month,
            serviceKey,
            cancellationToken);
        var reserveLines = await ReadFeeLinesAsync(
            "LongTermRepairReserve",
            _options.ApartmentManagementFee.LongTermRepairReservePath,
            request.ComplexCode,
            month,
            serviceKey,
            cancellationToken);

        var allLines = publicLines
            .Concat(individualLines)
            .Concat(reserveLines)
            .ToArray();
        var publicAmount = publicLines.Sum(x => x.Amount);
        var individualAmount = individualLines.Sum(x => x.Amount);
        var reserveAmount = reserveLines.Sum(x => x.Amount);
        var totalAmount = publicAmount + individualAmount + reserveAmount;

        return new PublicDataLookupResponse<ApartmentManagementFeeSnapshotItem>
        {
            Success = true,
            Page = 1,
            PageSize = 1,
            TotalCount = 1,
            Items =
            [
                new ApartmentManagementFeeSnapshotItem
                {
                    ComplexCode = request.ComplexCode.Trim(),
                    Month = month,
                    HouseholdCount = householdCount,
                    PublicManagementFeeAmount = publicAmount,
                    IndividualUsageFeeAmount = individualAmount,
                    LongTermRepairReserveMonthlyAmount = reserveAmount,
                    EstimatedTotalMonthlyFeeAmount = totalAmount,
                    EstimatedFeePerHousehold = householdCount is > 0
                        ? decimal.Round(totalAmount / householdCount.Value, 0)
                        : null,
                    LineItems = allLines,
                    DataSource = "국토교통부 K-apt 공동주택관리비 공공데이터"
                }
            ]
        };
    }

    public async Task<ApartmentGroupCommerceOffsetSimulationResult> SimulateGroupCommerceOffsetAsync(
        ApartmentGroupCommerceOffsetSimulationRequest request,
        CancellationToken cancellationToken = default)
    {
        var snapshotResult = await GetSnapshotAsync(new ApartmentManagementFeeSnapshotRequest
        {
            ComplexCode = request.ComplexCode,
            Month = request.Month
        }, cancellationToken);

        var snapshot = snapshotResult.Items.FirstOrDefault() ?? new ApartmentManagementFeeSnapshotItem
        {
            ComplexCode = request.ComplexCode,
            Month = NormalizeMonth(request.Month) ?? request.Month
        };
        var participantCount = Math.Max(0, request.ParticipantHouseholdCount);
        var totalCost = request.ExpectedPurchaseCost
                        + request.ExpectedLogisticsCost
                        + request.ExpectedPlatformFee
                        + request.ExpectedOtherCost;
        var grossProfit = request.ExpectedSalesAmount - totalCost;
        var sharingRate = Math.Clamp(request.ProfitSharingRate, 0m, 1m);
        var sharedProfit = Math.Max(0m, grossProfit) * sharingRate;
        var perParticipant = participantCount > 0
            ? decimal.Round(sharedProfit / participantCount, 0)
            : (decimal?)null;
        var offsetRate = perParticipant.HasValue && snapshot.EstimatedFeePerHousehold is > 0
            ? decimal.Round(perParticipant.Value / snapshot.EstimatedFeePerHousehold.Value, 4)
            : (decimal?)null;

        return new ApartmentGroupCommerceOffsetSimulationResult
        {
            FeeSnapshot = snapshot,
            ParticipantHouseholdCount = participantCount,
            ExpectedSalesAmount = request.ExpectedSalesAmount,
            ExpectedTotalCost = totalCost,
            ExpectedGrossProfit = grossProfit,
            ProfitSharingRate = sharingRate,
            ExpectedSharedProfit = sharedProfit,
            ExpectedMonthlyOffsetPerParticipant = perParticipant,
            EstimatedManagementFeeOffsetRate = offsetRate,
            Summary = BuildSummary(perParticipant, offsetRate)
        };
    }

    private async Task<IReadOnlyList<ApartmentManagementFeeLineItem>> ReadFeeLinesAsync(
        string category,
        string path,
        string complexCode,
        string month,
        string serviceKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return [];
        }

        var query = new Dictionary<string, string?>
        {
            ["serviceKey"] = serviceKey,
            ["kaptCode"] = complexCode.Trim(),
            ["searchDate"] = month,
            ["date"] = month,
            ["pageNo"] = "1",
            ["numOfRows"] = "100",
            ["_type"] = "json"
        };
        var relative = QueryHelpers.AddQueryString(path.TrimStart('/'), query);

        try
        {
            using var response = await _httpClient.GetAsync(relative, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            return PublicDataParsing.ReadItems(body)
                .SelectMany(item => ToLineItems(category, item))
                .ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<ApartmentManagementFeeLineItem> ToLineItems(
        string category,
        Dictionary<string, string?> source)
    {
        foreach (var value in PublicDataParsing.NumericValues(source))
        {
            if (MetadataKeys.Contains(value.Key) || value.Value <= 0)
            {
                continue;
            }

            yield return new ApartmentManagementFeeLineItem
            {
                Category = category,
                Code = value.Key,
                DisplayName = ToDisplayName(value.Key),
                Amount = value.Value
            };
        }
    }

    private static string? NormalizeMonth(string value)
    {
        var digits = Regex.Replace(value ?? string.Empty, "[^0-9]", string.Empty);
        return digits.Length == 6 ? digits : null;
    }

    private static string ToDisplayName(string code)
        => code switch
        {
            "gnrlMngCost" => "일반관리비",
            "clnCost" => "청소비",
            "scrtyCost" => "경비비",
            "elvtrMntCost" => "승강기유지비",
            "repairCost" => "수선비",
            "electricCost" => "전기료",
            "waterCost" => "수도료",
            "heatingCost" => "난방비",
            "hotWaterCost" => "급탕비",
            "gasCost" => "가스사용료",
            "monthlyAmount" => "월부과액",
            "reserveBalance" => "충당금잔액",
            _ => code
        };

    private string ResolveServiceKey()
    {
        if (!string.IsNullOrWhiteSpace(_options.ApartmentManagementFee.ServiceKey))
        {
            return _options.ApartmentManagementFee.ServiceKey;
        }

        if (!string.IsNullOrWhiteSpace(_options.DataGoKrServiceKey))
        {
            return _options.DataGoKrServiceKey;
        }

        return _options.ServiceKey;
    }

    private static string BuildSummary(decimal? perParticipant, decimal? offsetRate)
    {
        if (!perParticipant.HasValue)
        {
            return "참여 세대 수가 없어 세대별 관리비 상쇄액을 계산할 수 없습니다.";
        }

        if (!offsetRate.HasValue)
        {
            return $"공동판매 예상 배분액은 세대당 약 {perParticipant.Value:N0}원입니다.";
        }

        return $"공동판매 예상 배분액은 세대당 약 {perParticipant.Value:N0}원이며, 월 관리비 추정액의 약 {offsetRate.Value:P1}를 상쇄할 수 있습니다.";
    }

    private static PublicDataLookupResponse<ApartmentManagementFeeSnapshotItem> Fail(string message)
        => new()
        {
            Success = false,
            ErrorMessage = message,
            Page = 1,
            PageSize = 1,
            Items = []
        };
}
