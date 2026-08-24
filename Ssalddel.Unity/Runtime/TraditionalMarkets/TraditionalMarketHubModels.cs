using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.TraditionalMarkets
{
    public static class 전통시장물류거점SourceTypeCodes
    {
        public const string OperationalProjection = "OperationalProjection";
        public const string SimulatedFixture = "SimulatedFixture";
    }

    public static class 전통시장물류거점상태Codes
    {
        public const string Pilot = "Pilot";
        public const string Active = "Active";
    }

    public static class 전통시장위치정밀도Codes
    {
        public const string MarketAddressGeocoded = "MarketAddressGeocoded";
        public const string MarketSiteRepresentative = "TraditionalMarketSiteRepresentative";
    }

    public sealed class 전통시장물류기능ScreenModel
    {
        public bool 대량입고지원 { get; set; }

        public bool 분류지원 { get; set; }

        public bool 주민픽업지원 { get; set; }

        public bool 마지막구간배송지원 { get; set; }

        public bool 냉장보관지원 { get; set; }

        public bool 냉동보관지원 { get; set; }
    }

    public sealed class 전통시장물류거점ScreenModel
    {
        public string StableId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string 시장Code { get; set; } = string.Empty;

        public string 시장명 { get; set; } = string.Empty;

        public string 시도 { get; set; } = string.Empty;

        public string 시군구 { get; set; } = string.Empty;

        public string 상태Code { get; set; } = string.Empty;

        public decimal 서비스반경Km { get; set; }

        public int 일일공동구매처리용량 { get; set; }

        public string 입고시간대 { get; set; } = string.Empty;

        public string 픽업시간대 { get; set; } = string.Empty;

        public 전통시장물류기능ScreenModel 물류기능 { get; set; } = new 전통시장물류기능ScreenModel();

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public string LocationPrecisionCode { get; set; } = string.Empty;

        public string SourceTypeCode { get; set; } = string.Empty;

        public string SourceName { get; set; } = string.Empty;

        public string SourceHref { get; set; } = string.Empty;

        public DateTimeOffset EvidenceAsOf { get; set; }

        public DateTimeOffset GeneratedAt { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public interface I전통시장물류거점조회UseCase
    {
        Task<전통시장물류거점ScreenModel> 조회Async(
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class Simulated전통시장물류거점조회UseCase : I전통시장물류거점조회UseCase
    {
        private static readonly DateTimeOffset FixtureAsOf =
            DateTimeOffset.Parse("2026-08-08T10:00:00+09:00");

        public Task<전통시장물류거점ScreenModel> 조회Async(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new 전통시장물류거점ScreenModel
            {
                StableId = "traditional-market-hub:simulated-central-001",
                Revision = 1,
                시장Code = "SIM-MARKET-001",
                시장명 = "샘플 중앙전통시장",
                시도 = "샘플시",
                시군구 = "중앙구",
                상태Code = 전통시장물류거점상태Codes.Pilot,
                서비스반경Km = 5m,
                일일공동구매처리용량 = 120,
                입고시간대 = "06:00-10:00",
                픽업시간대 = "15:00-19:00",
                물류기능 = new 전통시장물류기능ScreenModel
                {
                    대량입고지원 = true,
                    분류지원 = true,
                    주민픽업지원 = true,
                    마지막구간배송지원 = true,
                    냉장보관지원 = true,
                    냉동보관지원 = false,
                },
                Latitude = 37.5665m,
                Longitude = 126.9780m,
                LocationPrecisionCode = 전통시장위치정밀도Codes.MarketSiteRepresentative,
                SourceTypeCode = 전통시장물류거점SourceTypeCodes.SimulatedFixture,
                SourceName = "SIMULATED traditional-market-hub fixture",
                EvidenceAsOf = FixtureAsOf,
                GeneratedAt = FixtureAsOf,
            });
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 전통시장물류거점ScreenModelValidator
    {
        private static readonly HashSet<string> SourceTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            전통시장물류거점SourceTypeCodes.OperationalProjection,
            전통시장물류거점SourceTypeCodes.SimulatedFixture,
        };

        private static readonly HashSet<string> PublicStatuses = new HashSet<string>(StringComparer.Ordinal)
        {
            전통시장물류거점상태Codes.Pilot,
            전통시장물류거점상태Codes.Active,
        };

        private static readonly HashSet<string> LocationPrecisions = new HashSet<string>(StringComparer.Ordinal)
        {
            전통시장위치정밀도Codes.MarketAddressGeocoded,
            전통시장위치정밀도Codes.MarketSiteRepresentative,
        };

        public string[] Validate(전통시장물류거점ScreenModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var errors = new List<string>();
            if (!StableDataId.IsValid(model.StableId))
            {
                errors.Add("HubStableIdInvalid");
            }

            if (model.Revision < 0)
            {
                errors.Add("HubRevisionInvalid");
            }

            Require(model.시장Code, "MarketCodeMissing", errors);
            Require(model.시장명, "MarketNameMissing", errors);
            Require(model.시도, "ProvinceMissing", errors);
            Require(model.시군구, "CityCountyMissing", errors);
            Require(model.SourceName, "SourceNameMissing", errors);

            if (!PublicStatuses.Contains(model.상태Code))
            {
                errors.Add("HubPublicStatusInvalid");
            }

            if (!SourceTypes.Contains(model.SourceTypeCode))
            {
                errors.Add("HubSourceTypeInvalid");
            }

            if (!LocationPrecisions.Contains(model.LocationPrecisionCode))
            {
                errors.Add("LocationPrecisionInvalid");
            }

            if (model.Latitude < -90m || model.Latitude > 90m)
            {
                errors.Add("LatitudeInvalid");
            }

            if (model.Longitude < -180m || model.Longitude > 180m)
            {
                errors.Add("LongitudeInvalid");
            }

            if (model.서비스반경Km < 0m)
            {
                errors.Add("ServiceRadiusInvalid");
            }

            if (model.일일공동구매처리용량 < 0)
            {
                errors.Add("DailyCapacityInvalid");
            }

            if (model.물류기능 == null)
            {
                errors.Add("LogisticsCapabilitiesMissing");
            }

            if (model.EvidenceAsOf == default)
            {
                errors.Add("EvidenceAsOfMissing");
            }

            if (model.GeneratedAt == default)
            {
                errors.Add("GeneratedAtMissing");
            }

            return errors.ToArray();
        }

        private static void Require(string value, string error, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(error);
            }
        }
    }
}
