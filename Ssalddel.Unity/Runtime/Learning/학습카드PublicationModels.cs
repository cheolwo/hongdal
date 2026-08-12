using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Learning
{
    public static class 학습카드PublicationContract
    {
        public const string SchemaVersion = "hongik-unity-learning-card-publication.v1";
        public const string CatalogSchemaVersion = "hongik-unity-learning-card-catalog.v1";
        public const string HongikAcademyEffectBasis = "HongikAcademyTranscript";
    }

    public sealed class 학습카드SourceProvenanceApiModel
    {
        public string YoutubeVideoId { get; set; } = string.Empty;
        public long StartMilliseconds { get; set; }
        public long EndMilliseconds { get; set; }
        public string CorePassage { get; set; } = string.Empty;
        public string SourceAnalysisId { get; set; } = string.Empty;
        public string[] EvidenceSegmentIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 학습카드GeneralMeaningApiModel
    {
        public string SourceUri { get; set; } = string.Empty;
        public int Revision { get; set; }
        public string Summary { get; set; } = string.Empty;
        public int ReviewStatus { get; set; }
    }

    public sealed class 학습카드ImageBlobApiModel
    {
        public string ContainerName { get; set; } = string.Empty;
        public string ObjectName { get; set; } = string.Empty;
        public string Sha256 { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long ByteLength { get; set; }
        public string SourceUri { get; set; } = string.Empty;
        public string LicenseCode { get; set; } = string.Empty;
    }

    public sealed class 학습카드EffectApiModel
    {
        public string BasisCode { get; set; } = string.Empty;
        public int Revision { get; set; }
        public string TargetStatCode { get; set; } = string.Empty;
        public int StatDelta { get; set; }
        public string GrantedRuleCode { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
    }

    public sealed class 학습카드PublicationApiModel
    {
        public string SchemaVersion { get; set; } = string.Empty;
        public string LearningContentStableId { get; set; } = string.Empty;
        public int ContentRevision { get; set; }
        public string ArcanaStableId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string KeyPhrase { get; set; } = string.Empty;
        public string Interpretation { get; set; } = string.Empty;
        public string ReflectionPrompt { get; set; } = string.Empty;
        public int ReviewStatus { get; set; }
        public int AudioReviewStatus { get; set; }
        public 학습카드SourceProvenanceApiModel HongikAcademySource { get; set; }
            = new 학습카드SourceProvenanceApiModel();
        public 학습카드GeneralMeaningApiModel? GeneralMeaning { get; set; }
        public 학습카드ImageBlobApiModel Image { get; set; } = new 학습카드ImageBlobApiModel();
        public 학습카드EffectApiModel Effect { get; set; } = new 학습카드EffectApiModel();
        public string EditorialReviewStableId { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTimeOffset PublishedAtUtc { get; set; }
        public string PublicationHash { get; set; } = string.Empty;
    }

    public sealed class 학습카드PublicationCatalogApiModel
    {
        public string SchemaVersion { get; set; } = string.Empty;
        public 학습카드PublicationApiModel[] Items { get; set; }
            = Array.Empty<학습카드PublicationApiModel>();
    }

    public sealed class 학습카드PublicationReadModel
    {
        public 저녁학당콘텐츠Snapshot Content { get; set; } = new 저녁학당콘텐츠Snapshot();
        public 학습카드ImageBlobApiModel Image { get; set; } = new 학습카드ImageBlobApiModel();
        public string PublicationHash { get; set; } = string.Empty;
        public string KeyPhrase { get; set; } = string.Empty;
    }

    public sealed class 학습카드PublicationAdapter
    {
        public 학습카드PublicationReadModel Map(학습카드PublicationApiModel source)
        {
            Validate(source);
            return new 학습카드PublicationReadModel
            {
                Content = new 저녁학당콘텐츠Snapshot
                {
                    StableId = source.LearningContentStableId,
                    Revision = source.ContentRevision,
                    KindCode = 학습콘텐츠종류Codes.ArcanaAndVideo,
                    Title = source.Title.Trim(),
                    TeachingSummary = source.Interpretation.Trim(),
                    ReflectionPrompt = source.ReflectionPrompt.Trim(),
                    CardStableId = source.ArcanaStableId,
                    KnowledgeNoteStableId = source.EditorialReviewStableId,
                    SourceVideoId = source.HongikAcademySource.YoutubeVideoId,
                    SourceStartSeconds = checked((int)(source.HongikAcademySource.StartMilliseconds / 1000)),
                    TargetStatCode = source.Effect.TargetStatCode,
                    GrantedRuleCode = source.Effect.GrantedRuleCode,
                    StatDelta = source.Effect.StatDelta,
                    SourceStableIds = new[]
                    {
                        source.ArcanaStableId,
                        source.EditorialReviewStableId,
                        source.HongikAcademySource.SourceAnalysisId,
                        "publication:" + source.PublicationHash,
                    }.Concat(source.HongikAcademySource.EvidenceSegmentIds)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray(),
                },
                Image = Clone(source.Image),
                PublicationHash = source.PublicationHash,
                KeyPhrase = source.KeyPhrase.Trim(),
            };
        }

        public void Validate(학습카드PublicationApiModel source)
        {
            if (source == null
                || source.SchemaVersion != 학습카드PublicationContract.SchemaVersion
                || !StableDataId.IsValid(source.LearningContentStableId)
                || source.ContentRevision <= 0
                || !StableDataId.IsValid(source.ArcanaStableId)
                || string.IsNullOrWhiteSpace(source.Title)
                || string.IsNullOrWhiteSpace(source.KeyPhrase)
                || string.IsNullOrWhiteSpace(source.Interpretation)
                || string.IsNullOrWhiteSpace(source.ReflectionPrompt)
                || source.ReviewStatus != 2 || source.AudioReviewStatus != 1
                || source.HongikAcademySource == null
                || source.Image == null || source.Effect == null
                || !StableDataId.IsValid(source.EditorialReviewStableId)
                || string.IsNullOrWhiteSpace(source.ApprovedBy)
                || source.PublishedAtUtc == default
                || source.PublishedAtUtc.Offset != TimeSpan.Zero)
                throw new InvalidOperationException("LearningCardPublicationInvalid");

            ValidateSource(source.HongikAcademySource);
            ValidateGeneralMeaning(source.GeneralMeaning);
            ValidateImage(source.Image);
            ValidateEffect(source.Effect);
            if (source.PublicationHash.Length != 64
                || source.PublicationHash.Any(value => !IsLowerHex(value))
                || !string.Equals(source.PublicationHash, ComputeHash(source), StringComparison.Ordinal))
                throw new InvalidOperationException("LearningCardPublicationHashMismatch");
        }

        public static string ComputeHash(학습카드PublicationApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var values = new List<string?>
            {
                source.SchemaVersion,
                source.LearningContentStableId,
                source.ContentRevision.ToString(CultureInfo.InvariantCulture),
                source.ArcanaStableId,
                source.Title,
                source.KeyPhrase,
                source.Interpretation,
                source.ReflectionPrompt,
                ReviewStatusName(source.ReviewStatus),
                AudioReviewStatusName(source.AudioReviewStatus),
                source.HongikAcademySource.YoutubeVideoId,
                source.HongikAcademySource.StartMilliseconds.ToString(CultureInfo.InvariantCulture),
                source.HongikAcademySource.EndMilliseconds.ToString(CultureInfo.InvariantCulture),
                source.HongikAcademySource.CorePassage,
                source.HongikAcademySource.SourceAnalysisId,
            };
            values.AddRange(source.HongikAcademySource.EvidenceSegmentIds);
            values.Add(source.GeneralMeaning == null ? null : source.GeneralMeaning.SourceUri);
            values.Add(source.GeneralMeaning == null ? null : source.GeneralMeaning.Revision.ToString(CultureInfo.InvariantCulture));
            values.Add(source.GeneralMeaning == null ? null : source.GeneralMeaning.Summary);
            values.Add(source.GeneralMeaning == null ? null : ReviewStatusName(source.GeneralMeaning.ReviewStatus));
            values.AddRange(new[]
            {
                source.Image.ContainerName,
                source.Image.ObjectName,
                source.Image.Sha256,
                source.Image.ContentType,
                source.Image.ByteLength.ToString(CultureInfo.InvariantCulture),
                source.Image.SourceUri,
                source.Image.LicenseCode,
                source.Effect.BasisCode,
                source.Effect.Revision.ToString(CultureInfo.InvariantCulture),
                source.Effect.TargetStatCode,
                source.Effect.StatDelta.ToString(CultureInfo.InvariantCulture),
                source.Effect.GrantedRuleCode,
                source.Effect.Rationale,
                source.EditorialReviewStableId,
                source.ApprovedBy,
                source.PublishedAtUtc.ToUniversalTime().ToString("O"),
            });
            var canonical = new StringBuilder();
            foreach (var value in values)
            {
                var normalized = value ?? string.Empty;
                canonical.Append(normalized.Length).Append(':').Append(normalized);
            }

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()));
                return string.Concat(hash.Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        private static void ValidateSource(학습카드SourceProvenanceApiModel source)
        {
            if (string.IsNullOrWhiteSpace(source.YoutubeVideoId)
                || source.StartMilliseconds < 0
                || source.EndMilliseconds <= source.StartMilliseconds
                || string.IsNullOrWhiteSpace(source.CorePassage)
                || !StableDataId.IsValid(source.SourceAnalysisId)
                || source.EvidenceSegmentIds == null || source.EvidenceSegmentIds.Length == 0
                || source.EvidenceSegmentIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("LearningCardPublicationSourceInvalid");
        }

        private static void ValidateGeneralMeaning(학습카드GeneralMeaningApiModel? source)
        {
            if (source == null) return;
            if (!IsHttps(source.SourceUri) || source.Revision <= 0
                || string.IsNullOrWhiteSpace(source.Summary) || source.ReviewStatus != 2)
                throw new InvalidOperationException("LearningCardGeneralMeaningInvalid");
        }

        private static void ValidateImage(학습카드ImageBlobApiModel image)
        {
            if (string.IsNullOrWhiteSpace(image.ContainerName)
                || string.IsNullOrWhiteSpace(image.ObjectName)
                || !image.ObjectName.StartsWith("hakdang/tarot/cards/", StringComparison.Ordinal)
                || Uri.TryCreate(image.ObjectName, UriKind.Absolute, out _)
                || image.Sha256.Length != 64 || image.Sha256.Any(value => !IsLowerHex(value))
                || (image.ContentType != "image/jpeg" && image.ContentType != "image/png"
                    && image.ContentType != "image/webp")
                || image.ByteLength <= 0 || !IsHttps(image.SourceUri)
                || string.IsNullOrWhiteSpace(image.LicenseCode))
                throw new InvalidOperationException("LearningCardPublicationImageInvalid");
        }

        private static void ValidateEffect(학습카드EffectApiModel effect)
        {
            var validPair = effect.TargetStatCode == "Awareness"
                    && effect.GrantedRuleCode == "BeginnerMind"
                || effect.TargetStatCode == "Resolve"
                    && effect.GrantedRuleCode == "IntegratedProgress";
            if (effect.BasisCode != 학습카드PublicationContract.HongikAcademyEffectBasis
                || effect.Revision <= 0 || effect.StatDelta != 1 || !validPair
                || string.IsNullOrWhiteSpace(effect.Rationale))
                throw new InvalidOperationException("LearningCardPublicationEffectInvalid");
        }

        private static 학습카드ImageBlobApiModel Clone(학습카드ImageBlobApiModel source)
            => new 학습카드ImageBlobApiModel
            {
                ContainerName = source.ContainerName,
                ObjectName = source.ObjectName,
                Sha256 = source.Sha256,
                ContentType = source.ContentType,
                ByteLength = source.ByteLength,
                SourceUri = source.SourceUri,
                LicenseCode = source.LicenseCode,
            };

        private static string ReviewStatusName(int value)
            => value == 2 ? "ApprovedForRuntime" : value == 1 ? "NeedsTranscriptReview" : "Draft";

        private static string AudioReviewStatusName(int value)
            => value == 1 ? "VerifiedAgainstAudio" : "Pending";

        private static bool IsHttps(string value)
            => Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;

        private static bool IsLowerHex(char value)
            => value >= '0' && value <= '9' || value >= 'a' && value <= 'f';
    }

    public interface I학습카드PublicationApiClient
    {
        Task<학습카드PublicationCatalogApiModel> GetCatalogAsync(
            CancellationToken cancellationToken);
    }

    public interface I학습카드PublicationRepository
    {
        Task<IReadOnlyList<학습카드PublicationReadModel>> GetApprovedAsync(
            CancellationToken cancellationToken);
    }

    public sealed class 학습카드PublicationApiRepository : I학습카드PublicationRepository
    {
        private readonly I학습카드PublicationApiClient apiClient;
        private readonly 학습카드PublicationAdapter adapter;

        public 학습카드PublicationApiRepository(
            I학습카드PublicationApiClient client,
            학습카드PublicationAdapter publicationAdapter)
        {
            apiClient = client ?? throw new ArgumentNullException(nameof(client));
            adapter = publicationAdapter ?? throw new ArgumentNullException(nameof(publicationAdapter));
        }

        public async Task<IReadOnlyList<학습카드PublicationReadModel>> GetApprovedAsync(
            CancellationToken cancellationToken)
        {
            var catalog = await apiClient.GetCatalogAsync(cancellationToken);
            if (catalog == null
                || catalog.SchemaVersion != 학습카드PublicationContract.CatalogSchemaVersion
                || catalog.Items == null)
                throw new InvalidOperationException("LearningCardPublicationCatalogInvalid");

            var revisions = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<학습카드PublicationReadModel>(catalog.Items.Length);
            foreach (var item in catalog.Items)
            {
                var mapped = adapter.Map(item);
                var revisionKey = mapped.Content.StableId + "@" +
                    mapped.Content.Revision.ToString(CultureInfo.InvariantCulture);
                if (!revisions.Add(revisionKey))
                    throw new InvalidOperationException("LearningCardPublicationRevisionDuplicated");
                result.Add(mapped);
            }

            return result
                .OrderBy(item => item.Content.StableId, StringComparer.Ordinal)
                .ThenBy(item => item.Content.Revision)
                .ToArray();
        }
    }

    public sealed class 저녁학당승인카드조회UseCase
    {
        private readonly I학습카드PublicationRepository repository;

        public 저녁학당승인카드조회UseCase(I학습카드PublicationRepository value)
            => repository = value ?? throw new ArgumentNullException(nameof(value));

        public Task<IReadOnlyList<학습카드PublicationReadModel>> ExecuteAsync(
            CancellationToken cancellationToken)
            => repository.GetApprovedAsync(cancellationToken);
    }
}
