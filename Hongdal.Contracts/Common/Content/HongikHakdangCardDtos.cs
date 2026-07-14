namespace Hongdal.Contracts.Common.Content;

public sealed record HongikHakdangCardDto(
    long Id,
    string SourceKey,
    string? Title,
    string? Description,
    string OriginalImageUrl,
    string? ThumbnailImageUrl,
    string? RelatedUrl,
    string? LocalImagePath,
    string ImageDownloadStatus,
    long? ImageSizeBytes,
    string? ImageSha256,
    bool IsActive,
    DateTime LastSeenAtUtc,
    int SortOrder);

public sealed record HongikHakdangCardCollectionDto(
    long Id,
    string SourceKey,
    string Name,
    int SortOrder,
    bool IsActive,
    DateTime LastSeenAtUtc,
    IReadOnlyList<HongikHakdangCardDto> Cards);

public sealed record HongikHakdangCardSyncResultDto(
    bool Executed,
    int CollectionCount,
    int CardOccurrenceCount,
    int UniqueCardCount,
    int AddedCardCount,
    int UpdatedCardCount,
    int DownloadedImageCount,
    int FailedImageCount,
    DateTime? SyncedAtUtc,
    string Message);

public sealed record HongikHakdangMobileCardDto(
    long Id,
    string? Title,
    string? Description,
    string? RelatedUrl,
    string NotificationImageUrl,
    string LockScreenImageUrl,
    string ImageSha256,
    IReadOnlyList<string> CollectionKeys);

public sealed record HongikHakdangCardCatalogDto(
    string CatalogVersion,
    DateTime GeneratedAtUtc,
    IReadOnlyList<HongikHakdangMobileCardDto> Cards);

public sealed record HongikHakdangTodayCardDto(
    DateOnly SelectionDate,
    string TimeZoneId,
    HongikHakdangMobileCardDto Card);

public sealed record HongikHakdangCardDeliveryPreferenceDto(
    bool Enabled,
    string DeliveryMode,
    bool PushEnabled,
    int LocalDeliveryMinute,
    string TimeZoneId,
    bool ShuffleWithoutRepeats,
    string? PreferredCollectionKey,
    DateTime? UpdatedAtUtc);

public sealed record HongikHakdangCardDeliveryPreferenceUpdateRequest(
    bool Enabled,
    string DeliveryMode,
    bool PushEnabled,
    int LocalDeliveryMinute,
    string TimeZoneId,
    bool ShuffleWithoutRepeats,
    string? PreferredCollectionKey);

public sealed record HongikHakdangCardVariantPreparationResultDto(
    int CandidateCardCount,
    int GeneratedVariantCount,
    int ReusedVariantCount,
    int FailedVariantCount);
