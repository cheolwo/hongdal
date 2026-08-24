using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Crops
{
    public sealed class CropReferenceCategoryItemApiModel
    {
        public string StableId { get; set; } = string.Empty;

        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;
    }

    public sealed class CropReferenceCategoryListApiModel
    {
        public string SourceTypeCode { get; set; } = string.Empty;

        public string SourceKey { get; set; } = string.Empty;

        public string SourceName { get; set; } = string.Empty;

        public string SourceHref { get; set; } = string.Empty;

        public DateTimeOffset RetrievedAt { get; set; }

        public string Boundary { get; set; } = string.Empty;

        public CropReferenceCategoryItemApiModel[] Items { get; set; } =
            Array.Empty<CropReferenceCategoryItemApiModel>();
    }

    public sealed class 작물기준정보분류
    {
        public string StableId { get; set; } = string.Empty;

        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;
    }

    public sealed class 작물기준정보분류Snapshot
    {
        public string SourceKey { get; set; } = string.Empty;

        public string SourceName { get; set; } = string.Empty;

        public string SourceHref { get; set; } = string.Empty;

        public DateTimeOffset RetrievedAt { get; set; }

        public string Boundary { get; set; } = string.Empty;

        public 작물기준정보분류[] Items { get; set; } = Array.Empty<작물기준정보분류>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class CropReferenceCategoryMapper
    {
        public const string PublicReferenceSourceType = "PublicReference";

        public MappingResult<작물기준정보분류Snapshot> Map(
            CropReferenceCategoryListApiModel? apiModel)
        {
            if (apiModel == null)
            {
                return MappingResult<작물기준정보분류Snapshot>.Failure("ApiModelMissing");
            }

            var errors = new List<string>();
            if (!string.Equals(
                    apiModel.SourceTypeCode,
                    PublicReferenceSourceType,
                    StringComparison.Ordinal))
            {
                errors.Add("SourceTypeInvalid");
            }

            Require(apiModel.SourceKey, "SourceKeyMissing", errors);
            Require(apiModel.SourceName, "SourceNameMissing", errors);
            Require(apiModel.SourceHref, "SourceHrefMissing", errors);
            Require(apiModel.Boundary, "BoundaryMissing", errors);
            if (apiModel.RetrievedAt == default)
            {
                errors.Add("RetrievedAtMissing");
            }

            if (apiModel.Items == null)
            {
                errors.Add("ItemsMissing");
                return MappingResult<작물기준정보분류Snapshot>.Failure(errors.ToArray());
            }

            var mapped = new List<작물기준정보분류>(apiModel.Items.Length);
            var stableIds = new HashSet<string>(StringComparer.Ordinal);
            var categoryCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < apiModel.Items.Length; index++)
            {
                var item = apiModel.Items[index];
                if (item == null)
                {
                    errors.Add("ItemMissing:" + index);
                    continue;
                }

                if (!StableDataId.IsValid(item.StableId))
                {
                    errors.Add("StableIdInvalid:" + index);
                }
                else if (!stableIds.Add(item.StableId))
                {
                    errors.Add("DuplicateStableId:" + item.StableId);
                }

                if (string.IsNullOrWhiteSpace(item.CategoryCode))
                {
                    errors.Add("CategoryCodeMissing:" + index);
                }
                else if (!categoryCodes.Add(item.CategoryCode))
                {
                    errors.Add("DuplicateCategoryCode:" + item.CategoryCode);
                }

                if (string.IsNullOrWhiteSpace(item.CategoryName))
                {
                    errors.Add("CategoryNameMissing:" + index);
                }

                mapped.Add(new 작물기준정보분류
                {
                    StableId = item.StableId,
                    CategoryCode = item.CategoryCode,
                    CategoryName = item.CategoryName,
                });
            }

            if (errors.Count > 0)
            {
                return MappingResult<작물기준정보분류Snapshot>.Failure(errors.ToArray());
            }

            return MappingResult<작물기준정보분류Snapshot>.Success(
                new 작물기준정보분류Snapshot
                {
                    SourceKey = apiModel.SourceKey,
                    SourceName = apiModel.SourceName,
                    SourceHref = apiModel.SourceHref,
                    RetrievedAt = apiModel.RetrievedAt,
                    Boundary = apiModel.Boundary,
                    Items = mapped.ToArray(),
                });
        }

        private static void Require(string value, string error, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(error);
            }
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public interface ICropReferenceCategoryApiClient
    {
        Task<CropReferenceCategoryListApiModel> GetAsync(
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public interface I작물기준정보Repository
    {
        Task<MappingResult<작물기준정보분류Snapshot>> 조회Async(
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public sealed class CropReferenceApiRepository : I작물기준정보Repository
    {
        private readonly ICropReferenceCategoryApiClient apiClient;
        private readonly CropReferenceCategoryMapper mapper;

        public CropReferenceApiRepository(
            ICropReferenceCategoryApiClient apiClient,
            CropReferenceCategoryMapper mapper)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<MappingResult<작물기준정보분류Snapshot>> 조회Async(
            CancellationToken cancellationToken = default)
        {
            var apiModel = await apiClient.GetAsync(cancellationToken).ConfigureAwait(false);
            return mapper.Map(apiModel);
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 작물기준정보분류조회UseCase
    {
        private readonly I작물기준정보Repository repository;

        public 작물기준정보분류조회UseCase(I작물기준정보Repository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<MappingResult<작물기준정보분류Snapshot>> 실행Async(
            CancellationToken cancellationToken = default)
        {
            return repository.조회Async(cancellationToken);
        }
    }
}
