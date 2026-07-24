namespace Ssalddel.Contracts.Common.Community;

public sealed record CommunityPostIngredientPriceHintRequest(string Body);

public sealed record CommunityPostIngredientPriceHintResponse(
    IReadOnlyList<CommunityPostIngredientPriceHint> Hints,
    string Notice,
    DateTime GeneratedAtUtc);

public sealed record CommunityPostIngredientPriceHint(
    string MatchedText,
    string IngredientName,
    string KamisItemCode,
    bool RequiresConfirmation,
    string InterpretationNote,
    bool HasPrice,
    decimal? AveragePrice,
    decimal? MinimumPrice,
    decimal? MaximumPrice,
    string CurrencyCode,
    string Unit,
    DateOnly? ReferenceDate,
    string MarketStageCode,
    string MarketStageName,
    string VarietySummary,
    int SampleCount,
    string Provider,
    string SourceUrl);
