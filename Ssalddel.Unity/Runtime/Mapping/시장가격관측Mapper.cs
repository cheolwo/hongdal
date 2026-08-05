using System;
using System.Collections.Generic;

namespace Ssalddel.Unity.Data
{
    public sealed class MappingResult<T>
        where T : class
    {
        private MappingResult(T? value, string[] errorCodes)
        {
            Value = value;
            ErrorCodes = errorCodes;
        }

        public T? Value { get; }

        public string[] ErrorCodes { get; }

        public bool IsMapped => Value != null && ErrorCodes.Length == 0;

        public static MappingResult<T> Success(T value)
        {
            return new MappingResult<T>(value ?? throw new ArgumentNullException(nameof(value)), Array.Empty<string>());
        }

        public static MappingResult<T> Failure(params string[] errorCodes)
        {
            return new MappingResult<T>(null, errorCodes ?? Array.Empty<string>());
        }
    }

    public sealed class 시장가격관측Mapper
    {
        public const string SupportedSchemaVersion = "1.0.0";

        public MappingResult<시장가격관측Snapshot> Map(
            시장가격관측ApiModel? apiModel,
            ExternalCodeMapping? itemMapping)
        {
            var errors = new List<string>();
            if (apiModel == null)
            {
                return MappingResult<시장가격관측Snapshot>.Failure("ApiModelMissing");
            }

            if (itemMapping == null)
            {
                return MappingResult<시장가격관측Snapshot>.Failure("ItemMappingMissing");
            }

            if (!string.Equals(apiModel.SchemaVersion, SupportedSchemaVersion, StringComparison.Ordinal))
            {
                errors.Add("UnsupportedSchema");
            }

            if (!string.Equals(itemMapping.ExternalCode, apiModel.ExternalItemCode, StringComparison.Ordinal))
            {
                errors.Add("ExternalItemCodeMismatch");
            }

            if (!string.Equals(itemMapping.QualityCode, 데이터품질Codes.Valid, StringComparison.Ordinal))
            {
                errors.Add("ItemMappingNotApproved");
            }

            if (apiModel.Price <= 0)
            {
                errors.Add("PriceInvalid");
            }

            if (!string.Equals(apiModel.CurrencyCode, "KRW", StringComparison.Ordinal)
                || !string.Equals(apiModel.Unit, "KRW/kg", StringComparison.Ordinal))
            {
                errors.Add("IncompatibleUnit");
            }

            if (!string.Equals(apiModel.QualityCode, 데이터품질Codes.Valid, StringComparison.Ordinal)
                && !string.Equals(apiModel.QualityCode, 데이터품질Codes.Fixture, StringComparison.Ordinal))
            {
                errors.Add("EvidenceQualityRejected");
            }

            if (apiModel.ObservedAt == default
                || apiModel.IngestedAt == default
                || string.IsNullOrWhiteSpace(apiModel.LicenseOrTermsReference))
            {
                errors.Add("EvidenceRequiredFieldMissing");
            }

            if (errors.Count > 0)
            {
                return MappingResult<시장가격관측Snapshot>.Failure(errors.ToArray());
            }

            return MappingResult<시장가격관측Snapshot>.Success(new 시장가격관측Snapshot
            {
                ObservationKey = apiModel.ObservationKey,
                CropKey = itemMapping.GameDataKey,
                PriceKrwPerKg = apiModel.Price,
                Evidence = new 데이터근거Envelope
                {
                    EvidenceId = "evidence:market:" + apiModel.ObservationKey.Replace(':', '-'),
                    SourceType = string.Equals(apiModel.QualityCode, 데이터품질Codes.Fixture, StringComparison.Ordinal)
                        ? 데이터SourceTypes.Fixture
                        : 데이터SourceTypes.PublicObservation,
                    SourceKey = apiModel.SourceKey,
                    SourceRecordId = apiModel.SourceRecordId,
                    DatasetKey = apiModel.DatasetKey,
                    DatasetVersion = apiModel.DatasetVersion,
                    ObservedAt = apiModel.ObservedAt,
                    IngestedAt = apiModel.IngestedAt,
                    RegionKey = apiModel.RegionKey,
                    MarketStageKey = apiModel.MarketStageKey,
                    OriginalValue = apiModel.Price,
                    OriginalUnit = apiModel.Unit,
                    CurrencyCode = apiModel.CurrencyCode,
                    NormalizedValue = apiModel.Price,
                    NormalizedUnit = "KRW/kg",
                    QualityCode = apiModel.QualityCode,
                    FreshnessCode = apiModel.FreshnessCode,
                    LicenseOrTermsReference = apiModel.LicenseOrTermsReference,
                    Limitations = apiModel.Limitations,
                    PayloadHash = apiModel.PayloadHash,
                },
            });
        }
    }
}
