using System.Text.Json;
using System.Text.Json.Serialization;
using Hongdal.Contracts.Admin.Dispatch;
using Microsoft.AspNetCore.Hosting;

namespace 홍달.Services.Dispatch.Coordination;

public interface I배차AI판단사례LedgerStore
{
    Task<DispatchAIJudgmentCaseCatalogDto> GetCatalogAsync(CancellationToken cancellationToken = default);

    Task<DispatchAIJudgmentCaseDto> CreateAsync(
        DispatchAIJudgmentCaseCreateRequest request,
        string? createdBy,
        CancellationToken cancellationToken = default);

    Task<DispatchAIJudgmentCaseDto> PromoteSuggestionAsync(
        string suggestionKey,
        DispatchAIJudgmentCasePromoteSuggestionRequest request,
        string? createdBy,
        CancellationToken cancellationToken = default);
}

public sealed class File배차AI판단사례LedgerStore : I배차AI판단사례LedgerStore, I배차AI판단근거Source
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly IReadOnlyList<DispatchAIJudgmentCaseSuggestionDto> SuggestedCases =
    [
        new()
        {
            SuggestionKey = "DCT-BUNDLE-SAME-SCOPE",
            Title = "같은 배달권 2건 묶음과 목표 수익 회귀",
            RelatedOS = "국내 화물 운송 OS",
            Keywords = ["묶음", "같은배달권", "목표수익", "플랫폼", "한 명의 기사에게 묶음 동시 배정"],
            SituationSummary = "상차지와 하차지가 같은 배달권 안에 있는 2건 의뢰가 동시에 대기 중이고, 단건보다 묶음의 건당 플랫폼 순이익이 목표값에 더 가깝다.",
            SuggestedJudgmentSummary = "필수 조건 충돌이 없고 상차 시간창이 크게 벌어지지 않으면 플랫폼 수익 묶음 후보로 승인한다.",
            DefaultUserDecision = "묶음 승인",
            DefaultBalancedDecision = "운영자 승인",
            Source = "admin-suggestion:DCT-BUNDLE-SAME-SCOPE"
        },
        new()
        {
            SuggestionKey = "DCT-BUNDLE-EXTERNAL-SCOPE",
            Title = "외부 배달권 묶음과 운행 부담",
            RelatedOS = "국내 화물 운송 OS",
            Keywords = ["묶음", "외부배달권", "운행부담", "수익", "보류"],
            SituationSummary = "플랫폼 순이익은 높지만 상차지 또는 하차지가 외부 배달권으로 크게 벌어져 기사 운행 부담이 커지는 상황이다.",
            SuggestedJudgmentSummary = "건당 수익이 목표값을 넘더라도 외부권 이동 부담이 크면 묶음 승인을 보류하고 단건 또는 다른 조합을 우선한다.",
            DefaultUserDecision = "보류",
            DefaultBalancedDecision = "운행 부담 우선",
            Source = "admin-suggestion:DCT-BUNDLE-EXTERNAL-SCOPE"
        },
        new()
        {
            SuggestionKey = "DCT-PICKUP-DEADLINE-AGING",
            Title = "상차 마감 임박과 오래 기다린 기사",
            RelatedOS = "국내 화물 운송 OS",
            Keywords = ["상차마감", "기사대기", "Aging", "거리", "시간창"],
            SituationSummary = "상차 마감까지 시간이 얼마 남지 않았고, 가까운 기사와 오래 대기한 기사가 함께 후보로 들어온 상황이다.",
            SuggestedJudgmentSummary = "상차 실패 위험이 커지는 경우에는 가까운 기사를 우선하고, 시간 여유가 있으면 오래 대기한 기사에게 보정점을 준다.",
            DefaultUserDecision = "조건부 승인",
            DefaultBalancedDecision = "상차 실패 위험 우선",
            Source = "admin-suggestion:DCT-PICKUP-DEADLINE-AGING"
        },
        new()
        {
            SuggestionKey = "DCT-NO-CANDIDATE-PUBLIC-CARGO",
            Title = "조건 맞는 기사 없음과 공개 화물 전환",
            RelatedOS = "국내 화물 운송 OS",
            Keywords = ["후보없음", "공개화물", "공개배차", "차량조건", "권역확장"],
            SituationSummary = "냉동, 위험물, 큰 적재함처럼 필수 조건이 있는 의뢰인데 현재 추천 가능한 기사가 없는 상황이다.",
            SuggestedJudgmentSummary = "조건이 맞지 않는 기사에게 억지로 추천하지 않고 정해진 대기시간 이후 공개 화물 또는 권역 확장으로 전환한다.",
            DefaultUserDecision = "공개 전환",
            DefaultBalancedDecision = "필수 조건 우선",
            Source = "admin-suggestion:DCT-NO-CANDIDATE-PUBLIC-CARGO"
        },
        new()
        {
            SuggestionKey = "FOOD-MULTI-DELIVERY-LIMIT",
            Title = "음식 멀티배차와 배달 완료 시간 제한",
            RelatedOS = "음식 배달 OS",
            Keywords = ["음식", "멀티배차", "조리완료", "배달시간", "6km", "42분"],
            SituationSummary = "두 음식 주문을 묶으면 기사 수익성은 좋아지지만 한 주문의 조리 완료 후 배달 완료 시간이 안내된 최대 시간을 넘을 수 있다.",
            SuggestedJudgmentSummary = "피크타임 허용 완충을 반영하더라도 주문별 최대 배달 시간을 넘는 묶음은 제외한다.",
            DefaultUserDecision = "조건부 승인",
            DefaultBalancedDecision = "주문자 안내 시간 우선",
            Source = "admin-suggestion:FOOD-MULTI-DELIVERY-LIMIT"
        }
    ];

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly 정적배차AI판단근거Source _staticSource = new();
    private readonly string _ledgerPath;

    public File배차AI판단사례LedgerStore(IWebHostEnvironment environment)
    {
        _ledgerPath = Path.Combine(environment.ContentRootPath, "App_Data", "dispatch-ai-judgment-cases.json");
    }

    public IReadOnlyList<배차AI정책근거Seed> 정책근거목록 => _staticSource.정책근거목록;

    public IReadOnlyList<배차AI판단사례Seed> 사례목록
    {
        get
        {
            _gate.Wait();
            try
            {
                return _staticSource.사례목록
                    .Concat(LoadLedger().Cases.Where(x => x.Active).Select(ToSeed))
                    .ToArray();
            }
            finally
            {
                _gate.Release();
            }
        }
    }

    public async Task<DispatchAIJudgmentCaseCatalogDto> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var ledger = await LoadLedgerAsync(cancellationToken);
            return new DispatchAIJudgmentCaseCatalogDto
            {
                Cases = ledger.Cases
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenBy(x => x.CaseId, StringComparer.Ordinal)
                    .Select(Clone)
                    .ToList(),
                Suggestions = SuggestedCases.Select(Clone).ToList()
            };
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<DispatchAIJudgmentCaseDto> CreateAsync(
        DispatchAIJudgmentCaseCreateRequest request,
        string? createdBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var ledger = await LoadLedgerAsync(cancellationToken);
            var item = CreateCase(
                request.Title,
                request.RelatedOS,
                request.Keywords,
                request.SituationSummary,
                request.JudgmentSummary,
                request.UserDecision,
                request.BalancedDecision,
                string.IsNullOrWhiteSpace(request.Source) ? "admin-manual" : request.Source,
                request.Active,
                createdBy);

            ledger.Cases.Add(item);
            await SaveLedgerAsync(ledger, cancellationToken);
            return Clone(item);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<DispatchAIJudgmentCaseDto> PromoteSuggestionAsync(
        string suggestionKey,
        DispatchAIJudgmentCasePromoteSuggestionRequest request,
        string? createdBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(suggestionKey))
        {
            throw new ArgumentException("판단 사례 예시 키가 필요합니다.", nameof(suggestionKey));
        }

        var suggestion = SuggestedCases.FirstOrDefault(x => string.Equals(x.SuggestionKey, suggestionKey, StringComparison.Ordinal));
        if (suggestion is null)
        {
            throw new ArgumentException("존재하지 않는 판단 사례 예시입니다.", nameof(suggestionKey));
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var ledger = await LoadLedgerAsync(cancellationToken);
            var item = CreateCase(
                suggestion.Title,
                suggestion.RelatedOS,
                suggestion.Keywords,
                suggestion.SituationSummary,
                string.IsNullOrWhiteSpace(request.JudgmentSummary) ? suggestion.SuggestedJudgmentSummary : request.JudgmentSummary,
                string.IsNullOrWhiteSpace(request.UserDecision) ? suggestion.DefaultUserDecision : request.UserDecision,
                string.IsNullOrWhiteSpace(request.BalancedDecision) ? suggestion.DefaultBalancedDecision : request.BalancedDecision,
                suggestion.Source,
                request.Active,
                createdBy);

            ledger.Cases.Add(item);
            await SaveLedgerAsync(ledger, cancellationToken);
            return Clone(item);
        }
        finally
        {
            _gate.Release();
        }
    }

    private DispatchAIJudgmentCaseLedger LoadLedger()
    {
        if (!File.Exists(_ledgerPath))
        {
            return new DispatchAIJudgmentCaseLedger();
        }

        var json = File.ReadAllText(_ledgerPath);
        return JsonSerializer.Deserialize<DispatchAIJudgmentCaseLedger>(json, JsonOptions)
               ?? new DispatchAIJudgmentCaseLedger();
    }

    private async Task<DispatchAIJudgmentCaseLedger> LoadLedgerAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_ledgerPath))
        {
            return new DispatchAIJudgmentCaseLedger();
        }

        await using var stream = File.OpenRead(_ledgerPath);
        return await JsonSerializer.DeserializeAsync<DispatchAIJudgmentCaseLedger>(stream, JsonOptions, cancellationToken)
               ?? new DispatchAIJudgmentCaseLedger();
    }

    private async Task SaveLedgerAsync(DispatchAIJudgmentCaseLedger ledger, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_ledgerPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_ledgerPath);
        await JsonSerializer.SerializeAsync(stream, ledger, JsonOptions, cancellationToken);
    }

    private static DispatchAIJudgmentCaseDto CreateCase(
        string title,
        string relatedOS,
        IReadOnlyList<string> keywords,
        string situationSummary,
        string judgmentSummary,
        string userDecision,
        string balancedDecision,
        string? source,
        bool active,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("판단 사례 제목이 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(relatedOS))
        {
            throw new ArgumentException("관련 OS가 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(situationSummary))
        {
            throw new ArgumentException("상황 요약이 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(judgmentSummary))
        {
            throw new ArgumentException("판단 요약이 필요합니다.");
        }

        var normalizedKeywords = keywords
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (normalizedKeywords.Count == 0)
        {
            throw new ArgumentException("검색 키워드를 하나 이상 입력해야 합니다.");
        }

        var now = DateTimeOffset.UtcNow;
        var id = $"ADMIN-DCT-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..31];
        return new DispatchAIJudgmentCaseDto
        {
            CaseId = id,
            Title = title.Trim(),
            RelatedOS = relatedOS.Trim(),
            Keywords = normalizedKeywords,
            SituationSummary = situationSummary.Trim(),
            JudgmentSummary = judgmentSummary.Trim(),
            UserDecision = string.IsNullOrWhiteSpace(userDecision) ? "보류" : userDecision.Trim(),
            BalancedDecision = string.IsNullOrWhiteSpace(balancedDecision) ? "운영자 판정" : balancedDecision.Trim(),
            Source = string.IsNullOrWhiteSpace(source) ? $"admin-ledger:{id}" : source.Trim(),
            Active = active,
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "admin" : createdBy,
            CreatedAt = now
        };
    }

    private static 배차AI판단사례Seed ToSeed(DispatchAIJudgmentCaseDto item)
        => new(
            item.CaseId,
            item.Title,
            item.RelatedOS,
            item.Keywords,
            item.SituationSummary,
            item.JudgmentSummary,
            item.UserDecision,
            item.BalancedDecision,
            item.Source);

    private static DispatchAIJudgmentCaseDto Clone(DispatchAIJudgmentCaseDto source)
        => new()
        {
            CaseId = source.CaseId,
            Title = source.Title,
            RelatedOS = source.RelatedOS,
            Keywords = [.. source.Keywords],
            SituationSummary = source.SituationSummary,
            JudgmentSummary = source.JudgmentSummary,
            UserDecision = source.UserDecision,
            BalancedDecision = source.BalancedDecision,
            Source = source.Source,
            Active = source.Active,
            CreatedBy = source.CreatedBy,
            CreatedAt = source.CreatedAt,
            UpdatedBy = source.UpdatedBy,
            UpdatedAt = source.UpdatedAt
        };

    private static DispatchAIJudgmentCaseSuggestionDto Clone(DispatchAIJudgmentCaseSuggestionDto source)
        => new()
        {
            SuggestionKey = source.SuggestionKey,
            Title = source.Title,
            RelatedOS = source.RelatedOS,
            Keywords = [.. source.Keywords],
            SituationSummary = source.SituationSummary,
            SuggestedJudgmentSummary = source.SuggestedJudgmentSummary,
            DefaultUserDecision = source.DefaultUserDecision,
            DefaultBalancedDecision = source.DefaultBalancedDecision,
            Source = source.Source
        };
}

internal sealed class DispatchAIJudgmentCaseLedger
{
    public List<DispatchAIJudgmentCaseDto> Cases { get; set; } = [];
}
