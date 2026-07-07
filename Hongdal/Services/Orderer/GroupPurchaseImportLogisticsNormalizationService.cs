using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Services.Orderer;

public interface IGroupPurchaseImportLogisticsNormalizationService
{
    IReadOnlyList<ImportLogisticsReferenceItem> SearchReferences(ImportLogisticsReferenceLookupRequest request);

    ImportLogisticsNormalizationSimulationResult Simulate(
        ImportLogisticsNormalizationSimulationRequest request);
}

public sealed class GroupPurchaseImportLogisticsNormalizationService : IGroupPurchaseImportLogisticsNormalizationService
{
    private static readonly IReadOnlyList<ImportLogisticsReferenceItem> ReferenceCatalog =
    [
        CreatePort("KRPUS", "Busan Port", "Busan", "Busan Customs", "Korea Customs Service / MOF public data"),
        CreatePort("KRINC", "Incheon Port", "Incheon", "Incheon Customs", "Korea Customs Service / MOF public data"),
        CreatePort("KRPTK", "Pyeongtaek-Dangjin Port", "Gyeonggi/Chungnam", "Pyeongtaek Customs", "Korea Customs Service / MOF public data"),
        CreatePort("KRKUV", "Gunsan Port", "Jeonbuk", "Gunsan Customs", "Korea Customs Service / MOF public data"),
        CreateAirport("ICN", "Incheon International Airport", "Incheon", "Incheon Airport Customs", "Korea Customs Service / MOLIT public data"),
        CreateAirport("GMP", "Gimpo International Airport", "Seoul", "Gimpo Airport Customs", "Korea Customs Service / MOLIT public data"),
        CreateCustomsOffice("INCHEON", "Incheon Customs", "Incheon", "KRINC"),
        CreateCustomsOffice("PYEONGTAEK", "Pyeongtaek Customs", "Gyeonggi", "KRPTK"),
        CreateCustomsOffice("BUSAN", "Busan Customs", "Busan", "KRPUS"),
        CreateCustomsOffice("INCHEON_AIRPORT", "Incheon Airport Customs", "Incheon", "ICN")
    ];

    private static readonly IReadOnlyList<ImportLogisticsSourceDto> Sources =
    [
        new()
        {
            Name = "Korea Customs Service Cargo Clearance Progress OpenAPI",
            Url = "https://www.data.go.kr/data/15126268/openapi.do",
            Usage = "B/L, H B/L, or cargo management number based cargo status and bonded location lookup"
        },
        new()
        {
            Name = "Korea Customs Service port/airport trade statistics",
            Url = "https://www.data.go.kr/data/15101636/openapi.do",
            Usage = "Port and airport import/export flow statistics"
        },
        new()
        {
            Name = "Ministry of Oceans and Fisheries port facility public data",
            Url = "https://www.data.go.kr/data/3082243/openapi.do",
            Usage = "Port facility and handling capacity reference"
        },
        new()
        {
            Name = "UNIPASS bonded area and registered business references",
            Url = "https://unipass.customs.go.kr/per/index.do",
            Usage = "Official bonded area code, bonded warehouse, and customs logistics reference verification"
        }
    ];

    public IReadOnlyList<ImportLogisticsReferenceItem> SearchReferences(ImportLogisticsReferenceLookupRequest request)
    {
        var query = ReferenceCatalog.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.TransportMode))
        {
            var transportMode = Normalize(request.TransportMode);
            query = query.Where(x => string.IsNullOrWhiteSpace(x.TransportMode)
                                     || Normalize(x.TransportMode) == transportMode);
        }

        if (!string.IsNullOrWhiteSpace(request.CodeType))
        {
            var codeType = Normalize(request.CodeType);
            query = query.Where(x => Normalize(x.CodeType) == codeType);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = Normalize(request.Keyword);
            query = query.Where(x =>
                Normalize(x.Code).Contains(keyword, StringComparison.Ordinal)
                || Normalize(x.Name).Contains(keyword, StringComparison.Ordinal)
                || Normalize(x.RegionName).Contains(keyword, StringComparison.Ordinal)
                || Normalize(x.RelatedCustomsOfficeCode).Contains(keyword, StringComparison.Ordinal));
        }

        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);
        return query
            .OrderBy(x => x.CodeType)
            .ThenBy(x => x.Code)
            .Take(pageSize)
            .ToArray();
    }

    public ImportLogisticsNormalizationSimulationResult Simulate(
        ImportLogisticsNormalizationSimulationRequest request)
    {
        var warnings = new List<string>();
        var references = ResolveReferences(request, warnings);
        var flow = BuildFlow(request, references);
        var simulation = BuildSimulation(request, references, warnings);

        return new ImportLogisticsNormalizationSimulationResult
        {
            Success = warnings.Count == 0 || references.Count > 0,
            DocumentManagementNumber = request.DocumentManagementNumber.Trim(),
            NormalizedReferences = references,
            SuggestedFlow = flow,
            Simulation = simulation,
            Warnings = warnings,
            Sources = Sources
        };
    }

    private static List<ImportLogisticsReferenceItem> ResolveReferences(
        ImportLogisticsNormalizationSimulationRequest request,
        List<string> warnings)
    {
        var references = new List<ImportLogisticsReferenceItem>();
        var destinationCode = Normalize(request.DestinationPortCode);
        var destinationName = Normalize(request.DestinationPortOrAirportName);
        var location = Normalize(request.CurrentLocationSummary);

        var destination = ReferenceCatalog.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(destinationCode) && Normalize(x.Code) == destinationCode);
        destination ??= ReferenceCatalog.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(destinationName) && Normalize(x.Name).Contains(destinationName, StringComparison.Ordinal));
        destination ??= ReferenceCatalog.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(location) && location.Contains(Normalize(x.RegionName), StringComparison.Ordinal));

        if (destination is not null)
        {
            references.Add(destination);
            var relatedCustoms = ReferenceCatalog.FirstOrDefault(x =>
                x.CodeType == ImportLogisticsReferenceCodeType.CustomsOffice
                && Normalize(x.RelatedPortOrAirportCode) == Normalize(destination.Code));
            if (relatedCustoms is not null)
            {
                references.Add(relatedCustoms);
            }
        }
        else
        {
            warnings.Add("Destination port or airport could not be normalized. Keep the raw B/L response and verify the official code.");
        }

        if (!string.IsNullOrWhiteSpace(request.CustomsOfficeCode) || !string.IsNullOrWhiteSpace(request.CustomsOfficeName))
        {
            var customs = ReferenceCatalog.FirstOrDefault(x =>
                x.CodeType == ImportLogisticsReferenceCodeType.CustomsOffice
                && (Normalize(x.Code) == Normalize(request.CustomsOfficeCode)
                    || Normalize(x.Name).Contains(Normalize(request.CustomsOfficeName), StringComparison.Ordinal)));
            if (customs is not null && references.All(x => x.Code != customs.Code))
            {
                references.Add(customs);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.BondedAreaCode) || !string.IsNullOrWhiteSpace(request.BondedAreaName))
        {
            references.Add(new ImportLogisticsReferenceItem
            {
                Code = request.BondedAreaCode.Trim(),
                CodeType = ImportLogisticsReferenceCodeType.BondedArea,
                Name = string.IsNullOrWhiteSpace(request.BondedAreaName) ? request.CurrentLocationSummary.Trim() : request.BondedAreaName.Trim(),
                RegionName = destination?.RegionName ?? string.Empty,
                TransportMode = request.TransportMode.Trim(),
                RelatedPortOrAirportCode = destination?.Code ?? string.Empty,
                RelatedCustomsOfficeCode = references.FirstOrDefault(x => x.CodeType == ImportLogisticsReferenceCodeType.CustomsOffice)?.Code ?? string.Empty,
                SourceName = "UNIPASS cargo tracking response",
                SourceUrl = "https://www.data.go.kr/data/15126268/openapi.do",
                RequiresOfficialVerification = string.IsNullOrWhiteSpace(request.BondedAreaCode)
            });
        }
        else
        {
            warnings.Add("Bonded area code is not present. Cargo can be tracked by location name, but bonded warehouse settlement should wait for the official bonded area code.");
        }

        return references
            .GroupBy(x => $"{x.CodeType}:{Normalize(x.Code)}:{Normalize(x.Name)}")
            .Select(x => x.First())
            .ToList();
    }

    private static IReadOnlyList<ImportLogisticsFlowStepDto> BuildFlow(
        ImportLogisticsNormalizationSimulationRequest request,
        IReadOnlyList<ImportLogisticsReferenceItem> references)
    {
        var portOrAirport = references.FirstOrDefault(x =>
            x.CodeType is ImportLogisticsReferenceCodeType.Port or ImportLogisticsReferenceCodeType.Airport);
        var customsOffice = references.FirstOrDefault(x => x.CodeType == ImportLogisticsReferenceCodeType.CustomsOffice);
        var bondedArea = references.FirstOrDefault(x => x.CodeType == ImportLogisticsReferenceCodeType.BondedArea);

        return
        [
            new()
            {
                Sequence = 1,
                StepCode = "TransportDocumentRegistered",
                DisplayName = request.TransportDocumentType == GroupPurchaseShipmentDocumentTypeCode.AirWaybill
                    ? "AWB registered"
                    : "B/L registered",
                ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.OverseasSeller,
                ReferenceCode = request.TransportDocumentNumber.Trim(),
                ReferenceName = request.TransportDocumentType,
                IsConfirmedByOfficialCode = !string.IsNullOrWhiteSpace(request.TransportDocumentNumber)
            },
            new()
            {
                Sequence = 2,
                StepCode = "ArrivalPortOrAirport",
                DisplayName = request.TransportMode == GroupPurchaseShipmentTransportModeCode.Air
                    ? "Arrive at Korean airport"
                    : "Arrive at Korean port",
                ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Carrier,
                ReferenceCode = portOrAirport?.Code ?? request.DestinationPortCode.Trim(),
                ReferenceName = portOrAirport?.Name ?? request.DestinationPortOrAirportName.Trim(),
                IsConfirmedByOfficialCode = portOrAirport is not null
            },
            new()
            {
                Sequence = 3,
                StepCode = "CustomsJurisdiction",
                DisplayName = "Customs jurisdiction resolved",
                ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.CustomsBroker,
                ReferenceCode = customsOffice?.Code ?? request.CustomsOfficeCode.Trim(),
                ReferenceName = customsOffice?.Name ?? request.CustomsOfficeName.Trim(),
                IsConfirmedByOfficialCode = customsOffice is not null
            },
            new()
            {
                Sequence = 4,
                StepCode = "BondedAreaStorage",
                DisplayName = "Bonded area storage or bonded transport",
                ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.CustomsBroker,
                ReferenceCode = bondedArea?.Code ?? request.BondedAreaCode.Trim(),
                ReferenceName = bondedArea?.Name ?? request.BondedAreaName.Trim(),
                IsConfirmedByOfficialCode = bondedArea is not null && !bondedArea.RequiresOfficialVerification
            },
            new()
            {
                Sequence = 5,
                StepCode = "DomesticLogisticsProxyInbound",
                DisplayName = "Inbound to domestic logistics proxy",
                ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.DomesticLogisticsProxy,
                ReferenceCode = request.DocumentManagementNumber.Trim(),
                ReferenceName = "Group purchase import ledger",
                IsConfirmedByOfficialCode = !string.IsNullOrWhiteSpace(request.DocumentManagementNumber)
            }
        ];
    }

    private static ImportLogisticsCostAndRiskSimulationDto BuildSimulation(
        ImportLogisticsNormalizationSimulationRequest request,
        IReadOnlyList<ImportLogisticsReferenceItem> references,
        List<string> warnings)
    {
        var hasDestination = references.Any(x =>
            x.CodeType is ImportLogisticsReferenceCodeType.Port or ImportLogisticsReferenceCodeType.Airport);
        var hasBondedAreaCode = references.Any(x =>
            x.CodeType == ImportLogisticsReferenceCodeType.BondedArea && !string.IsNullOrWhiteSpace(x.Code));
        var invoiceUnitValue = CalculateUnitValue(request.CargoInvoiceUsd, request.CargoWeightKg);
        var domesticInboundCost = CalculateUnitValue(request.ExpectedDomesticInboundCostKrw, request.CargoWeightKg);

        if (request.CargoWeightKg is <= 0)
        {
            warnings.Add("Cargo weight is required to calculate unit value and domestic inbound cost per kg.");
        }

        var risk = hasDestination && hasBondedAreaCode
            ? ImportLogisticsSimulationRiskCode.Low
            : hasDestination
                ? ImportLogisticsSimulationRiskCode.Medium
                : ImportLogisticsSimulationRiskCode.NeedsReview;

        return new ImportLogisticsCostAndRiskSimulationDto
        {
            InvoiceUnitValueUsdPerKg = invoiceUnitValue,
            ExpectedDomesticInboundCostKrwPerKg = domesticInboundCost,
            ClearanceRouteRiskCode = risk,
            ConfidenceCode = hasDestination && hasBondedAreaCode
                ? ImportLogisticsSimulationRiskCode.Low
                : ImportLogisticsSimulationRiskCode.NeedsReview,
            Summary = hasDestination && hasBondedAreaCode
                ? "Arrival point, customs jurisdiction, and bonded area are normalized enough for operational tracking."
                : "Keep the B/L response, then confirm official bonded area and customs codes before settlement or responsibility assignment."
        };
    }

    private static decimal? CalculateUnitValue(decimal? amount, decimal? weight)
        => amount.HasValue && weight is > 0
            ? decimal.Round(amount.Value / weight.Value, 4, MidpointRounding.AwayFromZero)
            : null;

    private static ImportLogisticsReferenceItem CreatePort(
        string code,
        string name,
        string regionName,
        string customsOfficeName,
        string sourceName)
        => new()
        {
            Code = code,
            CodeType = ImportLogisticsReferenceCodeType.Port,
            Name = name,
            RegionName = regionName,
            TransportMode = GroupPurchaseShipmentTransportModeCode.Ocean,
            RelatedCustomsOfficeCode = customsOfficeName,
            SourceName = sourceName,
            SourceUrl = "https://www.data.go.kr/data/3082243/openapi.do"
        };

    private static ImportLogisticsReferenceItem CreateAirport(
        string code,
        string name,
        string regionName,
        string customsOfficeName,
        string sourceName)
        => new()
        {
            Code = code,
            CodeType = ImportLogisticsReferenceCodeType.Airport,
            Name = name,
            RegionName = regionName,
            TransportMode = GroupPurchaseShipmentTransportModeCode.Air,
            RelatedCustomsOfficeCode = customsOfficeName,
            SourceName = sourceName,
            SourceUrl = "https://www.data.go.kr/data/15101636/openapi.do"
        };

    private static ImportLogisticsReferenceItem CreateCustomsOffice(
        string code,
        string name,
        string regionName,
        string relatedPortOrAirportCode)
        => new()
        {
            Code = code,
            CodeType = ImportLogisticsReferenceCodeType.CustomsOffice,
            Name = name,
            RegionName = regionName,
            RelatedPortOrAirportCode = relatedPortOrAirportCode,
            SourceName = "Korea Customs Service",
            SourceUrl = "https://www.customs.go.kr/"
        };

    private static string Normalize(string? value)
        => (value ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
}
