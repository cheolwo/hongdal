using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class Simulation영역건물발전Catalog
    {
        public static Simulation영역건물발전CatalogSnapshot CreateDefault()
        {
            var catalog = new Simulation영역건물발전CatalogSnapshot
            {
                Revision = Simulation영역건물발전Codes.CatalogRevision,
                Blueprints = CreateBlueprints(),
                ApprovedTeachingMaterials = CreateTeachingMaterials(),
            };
            catalog.HashSha256 = CalculateHash(catalog);
            return Clone(catalog);
        }

        public static void Validate(Simulation영역건물발전CatalogSnapshot catalog)
        {
            if (catalog == null
                || string.IsNullOrWhiteSpace(catalog.Revision)
                || catalog.Blueprints == null
                || catalog.Blueprints.Length == 0)
                throw new SimulationContractException(
                    Simulation영역건물발전Codes.CatalogInvalid);

            var duplicates = catalog.Blueprints.GroupBy(value =>
                    value.BlueprintStableId, StringComparer.Ordinal)
                .FirstOrDefault(group => string.IsNullOrWhiteSpace(group.Key)
                    || group.Count() > 1);
            if (duplicates != null)
                throw new SimulationContractException(
                    Simulation영역건물발전Codes.CatalogInvalid);

            var blueprints = catalog.Blueprints.ToDictionary(value =>
                value.BlueprintStableId, StringComparer.Ordinal);
            foreach (var blueprint in catalog.Blueprints)
            {
                if (!KnownArea(blueprint.AreaCode)
                    || !KnownStage(blueprint.StageCode)
                    || string.IsNullOrWhiteSpace(blueprint.KoreanName)
                    || string.IsNullOrWhiteSpace(blueprint.H1StableId)
                    || string.IsNullOrWhiteSpace(blueprint.FacilityStableId)
                    || blueprint.RequiredTimberQuantity < 0
                    || blueprint.RequiredRebuildPartQuantity < 0
                    || blueprint.ConstructionSeconds <= 0
                    || blueprint.FootprintWidthCentimeters <= 0
                    || blueprint.FootprintDepthCentimeters <= 0
                    || blueprint.ClearanceCentimeters < 0
                    || blueprint.RequiredOperationalBlueprintStableIds.Any(
                        required => !blueprints.ContainsKey(required)))
                    throw new SimulationContractException(
                        Simulation영역건물발전Codes.CatalogInvalid);
            }

            EnsureAcyclic(blueprints);

            var materialDuplicates = catalog.ApprovedTeachingMaterials
                .GroupBy(value => value.TeachingMaterialStableId,
                    StringComparer.Ordinal)
                .FirstOrDefault(group => string.IsNullOrWhiteSpace(group.Key)
                    || group.Count() > 1);
            if (materialDuplicates != null
                || catalog.ApprovedTeachingMaterials.Any(value =>
                    !value.AdminApproved
                    || string.IsNullOrWhiteSpace(value.Revision)
                    || string.IsNullOrWhiteSpace(value.HashSha256)
                    || string.IsNullOrWhiteSpace(value.KoreanTitle)
                    || string.IsNullOrWhiteSpace(value.TopicCode)))
                throw new SimulationContractException(
                    Simulation영역건물발전Codes.CatalogInvalid);

            if (!string.Equals(catalog.HashSha256, CalculateHash(catalog),
                    StringComparison.OrdinalIgnoreCase))
                throw new SimulationContractException(
                    Simulation영역건물발전Codes.CatalogHashMismatch);
        }

        public static string CalculateHash(
            Simulation영역건물발전CatalogSnapshot catalog)
        {
            var canonical = new StringBuilder();
            Append(canonical, catalog.Revision);
            foreach (var blueprint in (catalog.Blueprints
                         ?? Array.Empty<Simulation건물청사진Definition>())
                     .OrderBy(value => value.BlueprintStableId,
                         StringComparer.Ordinal))
            {
                Append(canonical, blueprint.BlueprintStableId);
                Append(canonical, blueprint.AreaCode);
                Append(canonical, blueprint.StageCode);
                Append(canonical, blueprint.KoreanName);
                Append(canonical, blueprint.H1StableId);
                Append(canonical, blueprint.FacilityStableId);
                foreach (var capability in blueprint.CapabilityCodes
                             .OrderBy(value => value, StringComparer.Ordinal))
                    Append(canonical, capability);
                foreach (var required in blueprint.RequiredOperationalBlueprintStableIds
                             .OrderBy(value => value, StringComparer.Ordinal))
                    Append(canonical, required);
                Append(canonical, blueprint.RequiredTimberQuantity);
                Append(canonical, blueprint.RequiredRebuildPartQuantity);
                Append(canonical, blueprint.ConstructionSeconds);
                Append(canonical, blueprint.FootprintWidthCentimeters);
                Append(canonical, blueprint.FootprintDepthCentimeters);
                Append(canonical, blueprint.ClearanceCentimeters);
                Append(canonical, blueprint.Optional);
            }
            foreach (var material in (catalog.ApprovedTeachingMaterials
                         ?? Array.Empty<Simulation승인가르침자료Snapshot>())
                     .OrderBy(value => value.TeachingMaterialStableId,
                         StringComparer.Ordinal))
            {
                Append(canonical, material.TeachingMaterialStableId);
                Append(canonical, material.Revision);
                Append(canonical, material.HashSha256);
                Append(canonical, material.TopicCode);
                Append(canonical, material.KoreanTitle);
                Append(canonical, material.ShortSummary);
                Append(canonical, material.SourceKindCode);
                Append(canonical, material.SourceReference);
                Append(canonical, material.ViewpointAndLimitations);
                Append(canonical, material.AdminApproved);
            }
            return Sha256(canonical.ToString());
        }

        public static Simulation영역건물발전CatalogSnapshot Clone(
            Simulation영역건물발전CatalogSnapshot source)
            => new Simulation영역건물발전CatalogSnapshot
            {
                Revision = source.Revision,
                HashSha256 = source.HashSha256,
                Blueprints = (source.Blueprints
                        ?? Array.Empty<Simulation건물청사진Definition>())
                    .Select(Clone).ToArray(),
                ApprovedTeachingMaterials = (source.ApprovedTeachingMaterials
                        ?? Array.Empty<Simulation승인가르침자료Snapshot>())
                    .Select(Clone).ToArray(),
            };

        internal static Simulation건물청사진Definition Clone(
            Simulation건물청사진Definition source)
            => new Simulation건물청사진Definition
            {
                BlueprintStableId = source.BlueprintStableId,
                AreaCode = source.AreaCode,
                StageCode = source.StageCode,
                KoreanName = source.KoreanName,
                H1StableId = source.H1StableId,
                FacilityStableId = source.FacilityStableId,
                CapabilityCodes = source.CapabilityCodes.ToArray(),
                RequiredOperationalBlueprintStableIds =
                    source.RequiredOperationalBlueprintStableIds.ToArray(),
                RequiredTimberQuantity = source.RequiredTimberQuantity,
                RequiredRebuildPartQuantity = source.RequiredRebuildPartQuantity,
                ConstructionSeconds = source.ConstructionSeconds,
                FootprintWidthCentimeters = source.FootprintWidthCentimeters,
                FootprintDepthCentimeters = source.FootprintDepthCentimeters,
                ClearanceCentimeters = source.ClearanceCentimeters,
                Optional = source.Optional,
            };

        internal static Simulation승인가르침자료Snapshot Clone(
            Simulation승인가르침자료Snapshot source)
            => new Simulation승인가르침자료Snapshot
            {
                TeachingMaterialStableId = source.TeachingMaterialStableId,
                Revision = source.Revision,
                HashSha256 = source.HashSha256,
                TopicCode = source.TopicCode,
                KoreanTitle = source.KoreanTitle,
                ShortSummary = source.ShortSummary,
                SourceKindCode = source.SourceKindCode,
                SourceReference = source.SourceReference,
                ViewpointAndLimitations = source.ViewpointAndLimitations,
                AdminApproved = source.AdminApproved,
            };

        private static Simulation건물청사진Definition[] CreateBlueprints()
            => new[]
            {
                Blueprint(Simulation영역건물발전Codes.NatureCabinBlueprint,
                    Simulation영역건물발전Codes.Nature,
                    Simulation영역건물발전Codes.Foundation, "오두막",
                    SimulationNatureSurvivalCodes.CabinSiteH1StableId,
                    "facility:nature-cabin", 6, 0, 30, 520, 420, 180,
                    Array.Empty<string>(), "Shelter", "Storage"),
                Blueprint(Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                    Simulation영역건물발전Codes.Nature,
                    Simulation영역건물발전Codes.Operations, "작업대",
                    "h1:nature:workbench", "facility:nature-workbench",
                    4, 1, 20, 220, 140, 100,
                    new[] { Simulation영역건물발전Codes.NatureCabinBlueprint },
                    "Crafting"),
                Blueprint(Simulation영역건물발전Codes.NatureStorageRackBlueprint,
                    Simulation영역건물발전Codes.Nature,
                    Simulation영역건물발전Codes.Operations, "보관대",
                    "h1:nature:storage-rack", "facility:nature-storage-rack",
                    6, 1, 24, 260, 120, 100,
                    new[] { Simulation영역건물발전Codes.NatureCabinBlueprint },
                    "Storage"),
                Blueprint(Simulation영역건물발전Codes.NaturePalisadeBlueprint,
                    Simulation영역건물발전Codes.Nature,
                    Simulation영역건물발전Codes.Operations, "방책",
                    "h1:nature:palisade", "facility:nature-palisade",
                    8, 1, 30, 600, 80, 80,
                    new[] { Simulation영역건물발전Codes.NatureCabinBlueprint },
                    "Defense"),
                Blueprint(Simulation영역건물발전Codes.NatureLearningLodgeBlueprint,
                    Simulation영역건물발전Codes.Nature,
                    Simulation영역건물발전Codes.Specialization, "자연 배움터",
                    Simulation영역건물발전Codes.NatureLearningLodgeH1,
                    Simulation영역건물발전Codes.NatureLearningLodgeFacility,
                    10, 1, 60, 480, 360, 180,
                    new[] { Simulation영역건물발전Codes.NatureWorkbenchBlueprint },
                    true, "Learning", "Reflection"),
                Blueprint("blueprint:nature-watch-post.v1", "Nature", "Resilience",
                    "위협 감시소", "h1-stock:nature-threat-watch",
                    "facility:nature-watch-post", 8, 1, 45, 300, 300, 120,
                    new[] { Simulation영역건물발전Codes.NaturePalisadeBlueprint },
                    "ThreatObservation"),
                Blueprint("blueprint:nature-restoration-workshop.v1", "Nature",
                    "Resilience", "복원 작업장", "h1-stock:nature-restoration-work",
                    "facility:nature-restoration-workshop", 10, 1, 50, 500, 400, 150,
                    new[] { Simulation영역건물발전Codes.NatureWorkbenchBlueprint },
                    "Restoration"),
                Blueprint("blueprint:nature-reflection-grove.v1", "Nature", "Landmark",
                    "공동 사색원", "h1-stock:nature-safe-recovery",
                    "facility:nature-reflection-grove", 12, 2, 75, 700, 600, 180,
                    new[]
                    {
                        Simulation영역건물발전Codes.NatureLearningLodgeBlueprint,
                        "blueprint:nature-restoration-workshop.v1",
                    }, true, "Learning", "SafeRecovery"),

                Blueprint("blueprint:farm-small-storage.v1", "Farm",
                    "Foundation", "소형 농장 창고", "h1-stock:farm-tool-storage",
                    "facility:farm-small-storage", 0, 0, 2, 400, 400, 100,
                    Array.Empty<string>(), "Storage"),
                Blueprint("blueprint:farm-seed-tools.v1", "Farm", "Operations",
                    "종자·도구 준비소", "h1-stock:farm-seed-preparation",
                    "facility:farm-seed-tools", 0, 0, 2, 400, 300, 100,
                    new[] { "blueprint:farm-small-storage.v1" }, "SeedPreparation"),
                Blueprint("blueprint:farm-wash-sort-pack.v1", "Farm",
                    "Specialization", "세척·선별·포장소",
                    "h1-stock:farm-washing", "facility:farm-wash-sort-pack",
                    0, 0, 3, 700, 500, 150,
                    new[] { "blueprint:farm-seed-tools.v1" }, "Sorting", "Packaging"),
                Blueprint(SimulationRegionalDevelopmentCodes.FarmExposureInspectionH1,
                    "Farm", "Resilience", "외부 노출 점검소",
                    SimulationRegionalDevelopmentCodes.FarmExposureInspectionH1,
                    "facility:farm-exposure-inspection", 0, 0, 2, 300, 300, 50,
                    new[] { "blueprint:farm-small-storage.v1" }, "IncidentInspection"),
                Blueprint(SimulationRegionalDevelopmentCodes.FarmIncidentQuarantineH1,
                    "Farm", "Resilience", "사건 격리소",
                    SimulationRegionalDevelopmentCodes.FarmIncidentQuarantineH1,
                    "facility:farm-incident-quarantine", 0, 0, 2, 400, 300, 50,
                    new[] { "blueprint:farm-small-storage.v1" }, "IncidentQuarantine"),
                Blueprint(SimulationRegionalDevelopmentCodes.FarmWeatherProtectionH1,
                    "Farm", "Resilience", "기상 보호소",
                    SimulationRegionalDevelopmentCodes.FarmWeatherProtectionH1,
                    "facility:farm-weather-protection", 0, 0, 2, 300, 400, 50,
                    new[] { "blueprint:farm-small-storage.v1" }, "WeatherProtection"),
                Blueprint("blueprint:farm-recovery-campus.v1", "Farm", "Landmark",
                    "농장 회복 복합구역",
                    "h1-stock:farm-restoration-supply",
                    "facility:farm-recovery-campus", 0, 0, 4, 1000, 800, 200,
                    new[]
                    {
                        SimulationRegionalDevelopmentCodes.FarmExposureInspectionH1,
                        SimulationRegionalDevelopmentCodes.FarmIncidentQuarantineH1,
                        SimulationRegionalDevelopmentCodes.FarmWeatherProtectionH1,
                    }, "FarmRecovery"),

                Blueprint("blueprint:town-community-store.v1", "Town", "Foundation",
                    "공동상점", "h1-stock:town-market-display",
                    "facility:town-community-store", 0, 0, 2, 600, 500, 120,
                    Array.Empty<string>(), "MarketDisplay"),
                Blueprint("blueprint:town-order-fulfillment.v1", "Town", "Operations",
                    "주문 포장·수령소", "h1-stock:town-order-packing",
                    "facility:town-order-fulfillment", 0, 0, 2, 600, 450, 120,
                    new[] { "blueprint:town-community-store.v1" }, "OrderPacking", "Pickup"),
                Blueprint("blueprint:town-neighborhood-service.v1", "Town",
                    "Specialization", "근린 생활서비스소",
                    "h1-stock:town-neighborhood-service",
                    "facility:town-neighborhood-service", 0, 0, 2, 600, 500, 120,
                    new[] { "blueprint:town-order-fulfillment.v1" }, "ResidentService"),
                Blueprint("blueprint:town-recall-relief.v1", "Town", "Resilience",
                    "회수·구호 안내소", "h1-stock:town-recall-service",
                    "facility:town-recall-relief", 0, 0, 3, 700, 500, 150,
                    new[] { "blueprint:town-neighborhood-service.v1" }, "Recall", "Relief"),
                Blueprint("blueprint:town-life-service-campus.v1", "Town", "Landmark",
                    "생활서비스 복합구역", "h1-stock:town-nature-relief",
                    "facility:town-life-service-campus", 0, 0, 4, 1000, 800, 200,
                    new[] { "blueprint:town-recall-relief.v1" }, "ResidentService", "Relief"),

                Blueprint("blueprint:hub-inbound-staging.v1", "Hub", "Foundation",
                    "반입·임시 적치소", "h1-stock:hub-temporary-staging",
                    "facility:hub-inbound-staging", 0, 0, 2, 800, 700, 150,
                    Array.Empty<string>(), "Receiving", "Staging"),
                Blueprint("blueprint:hub-inspection-quarantine.v1", "Hub", "Operations",
                    "검사·격리소", "h1-stock:hub-quarantine",
                    "facility:hub-inspection-quarantine", 0, 0, 2, 700, 600, 150,
                    new[] { "blueprint:hub-inbound-staging.v1" }, "Inspection", "Quarantine"),
                Blueprint("blueprint:hub-cold-picking.v1", "Hub", "Specialization",
                    "냉장·피킹 창고", "h1-stock:hub-cold-storage",
                    "facility:hub-cold-picking", 0, 0, 3, 1000, 800, 200,
                    new[] { "blueprint:hub-inspection-quarantine.v1" }, "ColdStorage", "Picking"),
                Blueprint("blueprint:hub-maintenance-power.v1", "Hub", "Resilience",
                    "정비·비상전력소", "h1-stock:hub-service-maintenance",
                    "facility:hub-maintenance-power", 0, 0, 3, 900, 700, 180,
                    new[] { "blueprint:hub-cold-picking.v1" }, "Maintenance", "EmergencyPower"),
                Blueprint("blueprint:hub-resilient-logistics.v1", "Hub", "Landmark",
                    "회복력 물류거점", "h1-stock:hub-outbound-staging",
                    "facility:hub-resilient-logistics", 0, 0, 4, 1200, 1000, 250,
                    new[] { "blueprint:hub-maintenance-power.v1" }, "ResilientLogistics"),

                Blueprint("blueprint:city-community-pickup.v1", "City", "Foundation",
                    "공동수령소", "h1-stock:city-community-pickup",
                    "facility:city-community-pickup", 0, 0, 2, 600, 500, 120,
                    Array.Empty<string>(), "Pickup"),
                Blueprint("blueprint:city-life-market.v1", "City", "Operations",
                    "생활시장", "h1-stock:city-life-market",
                    "facility:city-life-market", 0, 0, 2, 800, 600, 150,
                    new[] { "blueprint:city-community-pickup.v1" }, "Market"),
                Blueprint("blueprint:city-information-service.v1", "City",
                    "Specialization", "도시 정보·서비스관",
                    "h1-stock:city-information-service",
                    "facility:city-information-service", 0, 0, 3, 900, 700, 180,
                    new[] { "blueprint:city-life-market.v1" }, "Information", "ResidentService"),
                Blueprint("blueprint:city-utility-recovery.v1", "City", "Resilience",
                    "기반시설 복구소", "h1-stock:city-utility-recovery",
                    "facility:city-utility-recovery", 0, 0, 3, 1000, 800, 200,
                    new[] { "blueprint:city-information-service.v1" }, "UtilityRecovery"),
                Blueprint("blueprint:city-life-service-campus.v1", "City", "Landmark",
                    "도시 생활서비스 복합구역", "h1-stock:city-life-service-campus",
                    "facility:city-life-service-campus", 0, 0, 4, 1200, 900, 220,
                    new[] { "blueprint:city-utility-recovery.v1" }, "UrbanLifeService"),
            };

        private static Simulation승인가르침자료Snapshot[] CreateTeachingMaterials()
            => new[]
            {
                Teaching("teaching:nature:season-cycle.v1", "NatureCycle",
                    "계절의 순환", "계절 변화와 생존 준비의 관계를 관찰한다."),
                Teaching("teaching:nature:resource-renewal.v1", "ResourceRenewal",
                    "자원의 회복", "채취와 재생 사이의 시간과 한계를 살핀다."),
                Teaching("teaching:nature:universal-reflection.v1", "Reflection",
                    "보편 가르침과 성찰", "서로 다른 철학·종교적 관점을 우열 없이 비교하고 삶의 원칙을 성찰한다.",
                    "특정 교리의 진실성·우열이나 NPC 자격을 판정하지 않는다."),
            };

        private static Simulation건물청사진Definition Blueprint(
            string id, string area, string stage, string name, string h1,
            string facility, int timber, int parts, int seconds, int width,
            int depth, int clearance, string[] required,
            params string[] capabilities)
            => Blueprint(id, area, stage, name, h1, facility, timber, parts,
                seconds, width, depth, clearance, required, false, capabilities);

        private static Simulation건물청사진Definition Blueprint(
            string id, string area, string stage, string name, string h1,
            string facility, int timber, int parts, int seconds, int width,
            int depth, int clearance, string[] required, bool optional,
            params string[] capabilities)
            => new Simulation건물청사진Definition
            {
                BlueprintStableId = id,
                AreaCode = area,
                StageCode = stage,
                KoreanName = name,
                H1StableId = h1,
                FacilityStableId = facility,
                RequiredTimberQuantity = timber,
                RequiredRebuildPartQuantity = parts,
                ConstructionSeconds = seconds,
                FootprintWidthCentimeters = width,
                FootprintDepthCentimeters = depth,
                ClearanceCentimeters = clearance,
                RequiredOperationalBlueprintStableIds = required,
                Optional = optional,
                CapabilityCodes = capabilities,
            };

        private static Simulation승인가르침자료Snapshot Teaching(
            string id, string topic, string title, string summary,
            string limitations = "승인된 Simulation Fixture이며 외부 콘텐츠 추천이나 운영 사실이 아니다.")
        {
            var source = "fixture:" + id;
            return new Simulation승인가르침자료Snapshot
            {
                TeachingMaterialStableId = id,
                Revision = "approved-teaching.r1",
                HashSha256 = Sha256(source + "|" + title + "|" + summary),
                TopicCode = topic,
                KoreanTitle = title,
                ShortSummary = summary,
                SourceKindCode = "ApprovedFixture",
                SourceReference = source,
                ViewpointAndLimitations = limitations,
                AdminApproved = true,
            };
        }

        private static void EnsureAcyclic(
            IReadOnlyDictionary<string, Simulation건물청사진Definition> blueprints)
        {
            var visiting = new HashSet<string>(StringComparer.Ordinal);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in blueprints.Keys)
                Visit(id, blueprints, visiting, visited);
        }

        private static void Visit(string id,
            IReadOnlyDictionary<string, Simulation건물청사진Definition> blueprints,
            ISet<string> visiting, ISet<string> visited)
        {
            if (visited.Contains(id)) return;
            if (!visiting.Add(id))
                throw new SimulationContractException(
                    Simulation영역건물발전Codes.CatalogInvalid);
            foreach (var required in blueprints[id].RequiredOperationalBlueprintStableIds)
                Visit(required, blueprints, visiting, visited);
            visiting.Remove(id);
            visited.Add(id);
        }

        private static bool KnownArea(string value)
            => value is "Nature" or "Farm" or "Town" or "Hub" or "City";

        private static bool KnownStage(string value)
            => value is "Foundation" or "Operations" or "Specialization"
                or "Resilience" or "Landmark";

        private static void Append(StringBuilder builder, object? value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture)
                ?? string.Empty;
            builder.Append(text.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(text).Append('|');
        }

        private static string Sha256(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty);
        }
    }
}
