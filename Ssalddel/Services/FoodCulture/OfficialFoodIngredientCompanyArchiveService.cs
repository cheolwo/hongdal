using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.FoodCulture;

public interface IOfficialFoodIngredientCompanyArchiveService
{
    Task<OfficialFoodIngredientCompanyResearchResponse> ResearchAndArchiveAsync(
        OfficialFoodIngredientCompanyQuery query,
        string triggerCode = OfficialFoodIngredientCompanyResearchTriggerCodes.Manual,
        CancellationToken cancellationToken = default);

    Task<OfficialFoodIngredientCompanyCollectionResponse> CollectCatalogAsync(
        OfficialFoodIngredientCompanyCollectionRequest request,
        string triggerCode = OfficialFoodIngredientCompanyResearchTriggerCodes.Batch,
        CancellationToken cancellationToken = default);

    Task<OfficialFoodIngredientCompanyArchiveResponse?> GetArchiveAsync(
        string? ingredientKey,
        string? ingredientName,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<OfficialFoodIngredientCompanyCoverageResponse> GetCoverageAsync(
        int staleAfterDays = 30,
        CancellationToken cancellationToken = default);
}

public sealed class OfficialFoodIngredientCompanyArchiveService(
    AgriculturalFisheriesDbContext db,
    IOfficialFoodIngredientCompanyResearchService researchService,
    TimeProvider timeProvider,
    ILogger<OfficialFoodIngredientCompanyArchiveService> logger)
    : IOfficialFoodIngredientCompanyArchiveService
{
    private static readonly IReadOnlyList<string> Notices =
    [
        "표시된 업체는 공식 제품·표시 이력에서 재료 관계가 확인된 조사 후보이며 현재 재고, 공급능력, 판매 의사 또는 계약 권한을 보증하지 않습니다.",
        "대표자명, 전화번호, 상세 주소 등 개인 또는 직접 연락 정보는 공개 화면에 복제하지 않습니다.",
        "플랫폼은 업체를 자동 추천·선정·초대하지 않으며 실제 거래 전 당사자 동의와 최신 인허가·인증·수입중단 상태를 다시 확인해야 합니다.",
        "음식의 문화적 국가, 제품 제조국, 상품 원산지와 실제 선적 출발국은 서로 다른 정보로 관리합니다."
    ];

    public async Task<OfficialFoodIngredientCompanyResearchResponse> ResearchAndArchiveAsync(
        OfficialFoodIngredientCompanyQuery query,
        string triggerCode = OfficialFoodIngredientCompanyResearchTriggerCodes.Manual,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var ingredient = await FindIngredientAsync(
            query.IngredientKey,
            query.IngredientName,
            cancellationToken)
            ?? throw new KeyNotFoundException("전산화된 공식 음식 재료를 찾지 못했습니다.");
        var startedAtUtc = UtcNow();
        var run = await StartRunAsync(triggerCode, 1, startedAtUtc, cancellationToken);

        try
        {
            var response = await researchService.ResearchAsync(
                new OfficialFoodIngredientCompanyQuery
                {
                    IngredientKey = ingredient.IngredientKey,
                    IngredientName = ingredient.CanonicalName,
                    Take = Math.Clamp(query.Take, 1, 100)
                },
                cancellationToken);
            var archiveStats = await PersistResearchAsync(
                ingredient,
                run.Id,
                response,
                cancellationToken);
            await CompleteRunAsync(
                run.Id,
                CollectionCounters.From(response, response.Candidates.Count),
                skippedIngredientCount: 0,
                cancellationToken);

            return response with
            {
                Archived = true,
                ArchiveRunKey = run.RunKey,
                ArchivedOrganizationCount = archiveStats.OrganizationCount,
                ArchivedEvidenceCount = archiveStats.EvidenceCount
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await FailRunAsync(run.Id, "조사가 취소되었습니다.", CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            await FailRunAsync(run.Id, exception.Message, cancellationToken);
            throw;
        }
    }

    public async Task<OfficialFoodIngredientCompanyCollectionResponse> CollectCatalogAsync(
        OfficialFoodIngredientCompanyCollectionRequest request,
        string triggerCode = OfficialFoodIngredientCompanyResearchTriggerCodes.Batch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var maxIngredients = Math.Clamp(request.MaxIngredients, 1, 5000);
        var candidatesPerIngredient = Math.Clamp(request.CandidatesPerIngredient, 1, 100);
        var refreshAfterDays = Math.Clamp(request.RefreshAfterDays, 1, 3650);
        var requestDelayMilliseconds = Math.Clamp(request.RequestDelayMilliseconds, 0, 5000);
        var startedAtUtc = UtcNow();
        var refreshCutoffUtc = startedAtUtc.AddDays(-refreshAfterDays);
        var skippedIngredientCount = request.Force
            ? 0
            : await db.OfficialFoodIngredientCompanyProfiles
                .AsNoTracking()
                .CountAsync(
                    profile => profile.LastResearchedAtUtc >= refreshCutoffUtc,
                    cancellationToken);
        var eligibleQuery = db.OfficialFoodIngredients
            .AsNoTracking()
            .Where(ingredient => request.Force
                                 || ingredient.CompanyResearchProfile == null
                                 || ingredient.CompanyResearchProfile.LastResearchedAtUtc
                                 < refreshCutoffUtc);
        var requestedIngredientCount = Math.Min(
            maxIngredients,
            await eligibleQuery.CountAsync(cancellationToken));
        var run = await StartRunAsync(
            triggerCode,
            requestedIngredientCount,
            startedAtUtc,
            cancellationToken);
        var counters = new CollectionCounters();
        long lastIngredientId = 0;

        try
        {
            while (counters.AttemptedIngredientCount < requestedIngredientCount)
            {
                var take = Math.Min(
                    25,
                    requestedIngredientCount - counters.AttemptedIngredientCount);
                var ingredients = await db.OfficialFoodIngredients
                    .AsNoTracking()
                    .Where(ingredient => ingredient.Id > lastIngredientId
                                         && (request.Force
                                             || ingredient.CompanyResearchProfile == null
                                             || ingredient.CompanyResearchProfile.LastResearchedAtUtc
                                             < refreshCutoffUtc))
                    .OrderBy(ingredient => ingredient.Id)
                    .Take(take)
                    .ToArrayAsync(cancellationToken);
                if (ingredients.Length == 0)
                {
                    break;
                }

                foreach (var ingredient in ingredients)
                {
                    counters.AttemptedIngredientCount++;
                    try
                    {
                        var response = await researchService.ResearchAsync(
                            new OfficialFoodIngredientCompanyQuery
                            {
                                IngredientKey = ingredient.IngredientKey,
                                IngredientName = ingredient.CanonicalName,
                                Take = candidatesPerIngredient
                            },
                            cancellationToken);
                        await PersistResearchAsync(
                            ingredient,
                            run.Id,
                            response,
                            cancellationToken);
                        counters.Add(response);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        counters.FailedIngredientCount++;
                        db.ChangeTracker.Clear();
                        await MarkIngredientFailedAsync(
                            ingredient,
                            run.Id,
                            exception.Message,
                            cancellationToken);
                        logger.LogWarning(
                            exception,
                            "Ingredient company batch research failed. IngredientKey={IngredientKey} IngredientName={IngredientName}",
                            ingredient.IngredientKey,
                            ingredient.CanonicalName);
                    }

                    db.ChangeTracker.Clear();
                    if (requestDelayMilliseconds > 0
                        && counters.AttemptedIngredientCount < requestedIngredientCount)
                    {
                        await Task.Delay(requestDelayMilliseconds, cancellationToken);
                    }
                }

                lastIngredientId = ingredients[^1].Id;
            }

            await CompleteRunAsync(
                run.Id,
                counters,
                skippedIngredientCount,
                cancellationToken);
            var completedAtUtc = UtcNow();
            return counters.ToResponse(
                run.RunKey,
                triggerCode,
                DetermineRunStatus(counters),
                requestedIngredientCount,
                skippedIngredientCount,
                startedAtUtc,
                completedAtUtc);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await FailRunAsync(run.Id, "배치 조사가 취소되었습니다.", CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            await FailRunAsync(run.Id, exception.Message, cancellationToken);
            throw;
        }
    }

    public async Task<OfficialFoodIngredientCompanyArchiveResponse?> GetArchiveAsync(
        string? ingredientKey,
        string? ingredientName,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var ingredient = await FindIngredientAsync(
            ingredientKey,
            ingredientName,
            cancellationToken);
        if (ingredient is null)
        {
            return null;
        }

        var profile = await db.OfficialFoodIngredientCompanyProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.IngredientId == ingredient.Id,
                cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var run = await db.OfficialFoodIngredientCompanyResearchRuns
            .AsNoTracking()
            .SingleAsync(item => item.Id == profile.LastResearchRunId, cancellationToken);
        var sources = await db.OfficialFoodIngredientCompanySourceObservations
            .AsNoTracking()
            .Where(item => item.IngredientId == ingredient.Id
                           && item.ResearchRunId == profile.LastResearchRunId)
            .OrderBy(item => item.SourceKey)
            .Select(item => new OfficialFoodIngredientCompanySourceDto(
                item.SourceKey,
                item.Provider,
                item.DisplayName,
                item.CountryScope,
                item.OfficialUrl,
                item.StatusCode,
                item.StatusMessage,
                item.ProvidesDirectIngredientEvidence,
                item.CanVerifyCurrentOrganizationStatus,
                item.RequiresLiveRecheck))
            .ToArrayAsync(cancellationToken);
        var evidenceQuery = db.OfficialFoodIngredientCompanyEvidence
            .AsNoTracking()
            .Where(item => item.IngredientId == ingredient.Id);
        if (!includeInactive)
        {
            evidenceQuery = evidenceQuery.Where(item => item.IsCurrent);
        }

        var evidence = await evidenceQuery
            .OrderBy(item => item.OrganizationName)
            .ThenBy(item => item.RelatedProductName)
            .ThenBy(item => item.CandidateKey)
            .ToArrayAsync(cancellationToken);
        var organizations = evidence
            .GroupBy(item => item.OrganizationKey, StringComparer.Ordinal)
            .Select(group => ToOrganization(group.ToArray()))
            .OrderBy(item => item.RelationCode, StringComparer.Ordinal)
            .ThenBy(item => item.CountryName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.OrganizationName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new OfficialFoodIngredientCompanyArchiveResponse(
            profile.StatusCode,
            ingredient.IngredientKey,
            ingredient.CanonicalName,
            ingredient.LanguageCode,
            ingredient.CategoryCode,
            profile.ResearchQueryTerm,
            run.RunKey,
            AsUtcOffset(profile.LastResearchedAtUtc),
            organizations.Length,
            evidence.Length,
            organizations.Count(item =>
                item.RelationCode == OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer),
            organizations.Count(item =>
                item.RelationCode == OfficialFoodIngredientCompanyRelationCodes.DomesticImporter),
            organizations.Count(item =>
                item.RelationCode == OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer),
            sources,
            organizations,
            Notices);
    }

    public async Task<OfficialFoodIngredientCompanyCoverageResponse> GetCoverageAsync(
        int staleAfterDays = 30,
        CancellationToken cancellationToken = default)
    {
        var cutoffUtc = UtcNow().AddDays(-Math.Clamp(staleAfterDays, 1, 3650));
        var catalogIngredientCount = await db.OfficialFoodIngredients
            .AsNoTracking()
            .CountAsync(cancellationToken);
        var profiles = await db.OfficialFoodIngredientCompanyProfiles
            .AsNoTracking()
            .Select(profile => new { profile.StatusCode, profile.LastResearchedAtUtc })
            .ToArrayAsync(cancellationToken);
        var currentEvidence = await db.OfficialFoodIngredientCompanyEvidence
            .AsNoTracking()
            .Where(item => item.IsCurrent)
            .Select(item => new { item.OrganizationKey, item.RelationCode })
            .ToArrayAsync(cancellationToken);
        var lastCompletedAtUtc = await db.OfficialFoodIngredientCompanyResearchRuns
            .AsNoTracking()
            .Where(run => run.CompletedAtUtc.HasValue)
            .MaxAsync(run => run.CompletedAtUtc, cancellationToken);

        return new OfficialFoodIngredientCompanyCoverageResponse(
            catalogIngredientCount,
            profiles.Length,
            Math.Max(0, catalogIngredientCount - profiles.Length),
            profiles.Count(profile => profile.LastResearchedAtUtc < cutoffUtc),
            profiles.Count(profile =>
                profile.StatusCode == OfficialFoodIngredientCompanyResearchStatusCodes.Available),
            profiles.Count(profile =>
                profile.StatusCode == OfficialFoodIngredientCompanyResearchStatusCodes.Partial),
            profiles.Count(profile =>
                profile.StatusCode == OfficialFoodIngredientCompanyResearchStatusCodes.NoResults),
            profiles.Count(profile =>
                profile.StatusCode == OfficialFoodIngredientCompanyResearchStatusCodes.NotConfigured),
            profiles.Count(profile =>
                profile.StatusCode == OfficialFoodIngredientCompanyResearchStatusCodes.Failed),
            currentEvidence.Select(item => item.OrganizationKey).Distinct(StringComparer.Ordinal).Count(),
            currentEvidence.Length,
            DistinctOrganizationCount(
                currentEvidence.Select(item =>
                    new OrganizationRelation(item.OrganizationKey, item.RelationCode)),
                OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer),
            DistinctOrganizationCount(
                currentEvidence.Select(item =>
                    new OrganizationRelation(item.OrganizationKey, item.RelationCode)),
                OfficialFoodIngredientCompanyRelationCodes.DomesticImporter),
            DistinctOrganizationCount(
                currentEvidence.Select(item =>
                    new OrganizationRelation(item.OrganizationKey, item.RelationCode)),
                OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer),
            lastCompletedAtUtc.HasValue ? AsUtcOffset(lastCompletedAtUtc.Value) : null);
    }

    private async Task<ArchiveStats> PersistResearchAsync(
        OfficialFoodIngredient ingredient,
        long runId,
        OfficialFoodIngredientCompanyResearchResponse response,
        CancellationToken cancellationToken)
    {
        var executionStrategy = db.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(
            () => PersistResearchCoreAsync(
                ingredient,
                runId,
                response,
                cancellationToken));
    }

    private async Task<ArchiveStats> PersistResearchCoreAsync(
        OfficialFoodIngredient ingredient,
        long runId,
        OfficialFoodIngredientCompanyResearchResponse response,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var observedAtUtc = response.ResearchedAtUtc.UtcDateTime;
        foreach (var source in response.Sources)
        {
            db.OfficialFoodIngredientCompanySourceObservations.Add(
                new OfficialFoodIngredientCompanySourceObservation
                {
                    ResearchRunId = runId,
                    IngredientId = ingredient.Id,
                    SourceKey = Truncate(source.SourceKey, 100),
                    Provider = Truncate(source.Provider, 300),
                    DisplayName = Truncate(source.DisplayName, 300),
                    CountryScope = Truncate(source.CountryScope, 300),
                    OfficialUrl = Truncate(source.OfficialUrl, 1000),
                    StatusCode = Truncate(source.StatusCode, 30),
                    StatusMessage = Truncate(source.StatusMessage, 2000),
                    ProvidesDirectIngredientEvidence = source.ProvidesDirectIngredientEvidence,
                    CanVerifyCurrentOrganizationStatus = source.CanVerifyCurrentOrganizationStatus,
                    RequiresLiveRecheck = source.RequiresLiveRecheck,
                    ObservedAtUtc = observedAtUtc
                });
        }

        var refreshedSourceKeys = response.Sources
            .Where(source => source.ProvidesDirectIngredientEvidence
                             && source.StatusCode
                             == OfficialFoodIngredientCompanySourceStatusCodes.Available)
            .Select(source => source.SourceKey)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (refreshedSourceKeys.Length > 0)
        {
            var previousEvidence = await db.OfficialFoodIngredientCompanyEvidence
                .Where(item => item.IngredientId == ingredient.Id
                               && item.IsCurrent
                               && Enumerable.Contains(refreshedSourceKeys, item.SourceKey))
                .ToArrayAsync(cancellationToken);
            foreach (var item in previousEvidence)
            {
                item.IsCurrent = false;
            }
        }

        var candidateKeys = response.Candidates
            .Select(candidate => candidate.CandidateKey)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var existingEvidence = candidateKeys.Length == 0
            ? new Dictionary<string, OfficialFoodIngredientCompanyEvidence>(StringComparer.Ordinal)
            : await db.OfficialFoodIngredientCompanyEvidence
                .Where(item => item.IngredientId == ingredient.Id
                               && Enumerable.Contains(candidateKeys, item.CandidateKey))
                .ToDictionaryAsync(item => item.CandidateKey, StringComparer.Ordinal, cancellationToken);
        foreach (var candidate in response.Candidates)
        {
            if (!existingEvidence.TryGetValue(candidate.CandidateKey, out var entity))
            {
                entity = new OfficialFoodIngredientCompanyEvidence
                {
                    IngredientId = ingredient.Id,
                    CandidateKey = Truncate(candidate.CandidateKey, 64),
                    FirstObservedAtUtc = observedAtUtc,
                    ObservationCount = 0
                };
                db.OfficialFoodIngredientCompanyEvidence.Add(entity);
                existingEvidence[candidate.CandidateKey] = entity;
            }

            entity.LastResearchRunId = runId;
            entity.OrganizationKey = OrganizationKey(candidate);
            entity.OrganizationName = Truncate(candidate.OrganizationName, 500);
            entity.NormalizedOrganizationName = Truncate(
                Normalize(candidate.OrganizationName),
                500);
            entity.CountryCode = Truncate(candidate.CountryCode, 8);
            entity.CountryName = Truncate(candidate.CountryName, 160);
            entity.RelationCode = Truncate(candidate.RelationCode, 40);
            entity.EvidenceCode = Truncate(candidate.EvidenceCode, 50);
            entity.EvidenceSummary = Truncate(candidate.EvidenceSummary, 2000);
            entity.RelatedProductName = Truncate(candidate.RelatedProductName, 500);
            entity.ProductCategory = Truncate(candidate.ProductCategory, 300);
            entity.OfficialIdentifier = Truncate(candidate.OfficialIdentifier, 200);
            entity.EvidenceRecordIdentifier = Truncate(
                candidate.EvidenceRecordIdentifier,
                200);
            entity.VerificationStatusCode = Truncate(candidate.VerificationStatusCode, 50);
            entity.RawIngredientText = Truncate(candidate.RawIngredientText, 8000);
            entity.EvidenceDate = Truncate(candidate.EvidenceDate, 40);
            entity.EvidenceLastChangedDate = Truncate(candidate.EvidenceLastChangedDate, 40);
            entity.EvidenceSequence = Truncate(candidate.EvidenceSequence, 80);
            entity.RequiresAttention = candidate.RequiresAttention;
            entity.AttentionReason = Truncate(candidate.AttentionReason, 2000);
            entity.SourceKey = Truncate(candidate.SourceKey, 100);
            entity.SourceName = Truncate(candidate.SourceName, 300);
            entity.SourceUrl = Truncate(candidate.SourceUrl, 1000);
            entity.ResearchQueryTerm = Truncate(response.IngredientName, 300);
            entity.LastObservedAtUtc = observedAtUtc;
            entity.ObservationCount++;
            entity.IsCurrent = true;
            entity.RequiresLiveRecheck = true;
            entity.CanAutoSelect = false;
            entity.CanAutoContact = false;
        }

        await db.SaveChangesAsync(cancellationToken);
        var currentEvidence = await db.OfficialFoodIngredientCompanyEvidence
            .AsNoTracking()
            .Where(item => item.IngredientId == ingredient.Id && item.IsCurrent)
            .Select(item => new { item.OrganizationKey, item.RelationCode })
            .ToArrayAsync(cancellationToken);
        var profile = await db.OfficialFoodIngredientCompanyProfiles
            .SingleOrDefaultAsync(item => item.IngredientId == ingredient.Id, cancellationToken);
        if (profile is null)
        {
            profile = new OfficialFoodIngredientCompanyProfile
            {
                IngredientId = ingredient.Id,
                CreatedAtUtc = observedAtUtc
            };
            db.OfficialFoodIngredientCompanyProfiles.Add(profile);
        }

        profile.LastResearchRunId = runId;
        profile.StatusCode = Truncate(response.StatusCode, 30);
        profile.ResearchQueryTerm = Truncate(response.IngredientName, 300);
        profile.LastResearchedAtUtc = observedAtUtc;
        profile.OrganizationCount = currentEvidence
            .Select(item => item.OrganizationKey)
            .Distinct(StringComparer.Ordinal)
            .Count();
        profile.EvidenceCount = currentEvidence.Length;
        profile.DomesticManufacturerCount = DistinctOrganizationCount(
            currentEvidence.Select(item =>
                new OrganizationRelation(item.OrganizationKey, item.RelationCode)),
            OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer);
        profile.DomesticImporterCount = DistinctOrganizationCount(
            currentEvidence.Select(item =>
                new OrganizationRelation(item.OrganizationKey, item.RelationCode)),
            OfficialFoodIngredientCompanyRelationCodes.DomesticImporter);
        profile.ForeignManufacturerCount = DistinctOrganizationCount(
            currentEvidence.Select(item =>
                new OrganizationRelation(item.OrganizationKey, item.RelationCode)),
            OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer);
        profile.AvailableSourceCount = response.Sources.Count(source =>
            source.StatusCode == OfficialFoodIngredientCompanySourceStatusCodes.Available);
        profile.FailedSourceCount = response.Sources.Count(source =>
            source.StatusCode == OfficialFoodIngredientCompanySourceStatusCodes.Failed);
        profile.NotConfiguredSourceCount = response.Sources.Count(source =>
            source.StatusCode == OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured);
        var directSources = response.Sources
            .Where(source => source.ProvidesDirectIngredientEvidence)
            .ToArray();
        profile.ConsecutiveFailureCount = directSources.Length > 0
                                          && directSources.All(source =>
                                              source.StatusCode
                                              == OfficialFoodIngredientCompanySourceStatusCodes.Failed)
            ? profile.ConsecutiveFailureCount + 1
            : 0;
        profile.UpdatedAtUtc = observedAtUtc;
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new ArchiveStats(profile.OrganizationCount, profile.EvidenceCount);
    }

    private async Task MarkIngredientFailedAsync(
        OfficialFoodIngredient ingredient,
        long runId,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var nowUtc = UtcNow();
        var profile = await db.OfficialFoodIngredientCompanyProfiles
            .SingleOrDefaultAsync(item => item.IngredientId == ingredient.Id, cancellationToken);
        if (profile is null)
        {
            profile = new OfficialFoodIngredientCompanyProfile
            {
                IngredientId = ingredient.Id,
                CreatedAtUtc = nowUtc
            };
            db.OfficialFoodIngredientCompanyProfiles.Add(profile);
        }

        profile.LastResearchRunId = runId;
        profile.StatusCode = OfficialFoodIngredientCompanyResearchStatusCodes.Failed;
        profile.ResearchQueryTerm = Truncate(ingredient.CanonicalName, 300);
        profile.LastResearchedAtUtc = nowUtc;
        profile.FailedSourceCount = Math.Max(1, profile.FailedSourceCount);
        profile.ConsecutiveFailureCount++;
        profile.UpdatedAtUtc = nowUtc;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogDebug(
            "Archived ingredient company research failure. IngredientKey={IngredientKey} Error={Error}",
            ingredient.IngredientKey,
            Truncate(errorMessage, 500));
    }

    private async Task<OfficialFoodIngredientCompanyResearchRun> StartRunAsync(
        string triggerCode,
        int requestedIngredientCount,
        DateTime startedAtUtc,
        CancellationToken cancellationToken)
    {
        var run = new OfficialFoodIngredientCompanyResearchRun
        {
            RunKey = Guid.NewGuid().ToString("N"),
            TriggerCode = Truncate(triggerCode, 30),
            StatusCode = OfficialFoodIngredientCompanyResearchRunStatusCodes.Running,
            RequestedIngredientCount = requestedIngredientCount,
            StartedAtUtc = startedAtUtc
        };
        db.OfficialFoodIngredientCompanyResearchRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
        return run;
    }

    private async Task CompleteRunAsync(
        long runId,
        CollectionCounters counters,
        int skippedIngredientCount,
        CancellationToken cancellationToken)
    {
        var run = await db.OfficialFoodIngredientCompanyResearchRuns
            .SingleAsync(item => item.Id == runId, cancellationToken);
        run.StatusCode = DetermineRunStatus(counters);
        run.ProcessedIngredientCount = counters.ProcessedIngredientCount;
        run.SkippedIngredientCount = skippedIngredientCount;
        run.AvailableIngredientCount = counters.AvailableIngredientCount;
        run.PartialIngredientCount = counters.PartialIngredientCount;
        run.NoResultIngredientCount = counters.NoResultIngredientCount;
        run.NotConfiguredIngredientCount = counters.NotConfiguredIngredientCount;
        run.FailedIngredientCount = counters.FailedIngredientCount;
        run.ObservedEvidenceCount = counters.ObservedEvidenceCount;
        run.CompletedAtUtc = UtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task FailRunAsync(
        long runId,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        db.ChangeTracker.Clear();
        var run = await db.OfficialFoodIngredientCompanyResearchRuns
            .SingleOrDefaultAsync(item => item.Id == runId, cancellationToken);
        if (run is null)
        {
            return;
        }

        run.StatusCode = OfficialFoodIngredientCompanyResearchRunStatusCodes.Failed;
        run.ErrorMessage = Truncate(errorMessage, 4000);
        run.CompletedAtUtc = UtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<OfficialFoodIngredient?> FindIngredientAsync(
        string? ingredientKey,
        string? ingredientName,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(ingredientKey))
        {
            var key = ingredientKey.Trim();
            return await db.OfficialFoodIngredients
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.IngredientKey == key, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(ingredientName))
        {
            return null;
        }

        var name = ingredientName.Trim();
        var normalizedName = OfficialFoodRecipeIngredientParser.NormalizeName(name);
        return await db.OfficialFoodIngredients
            .AsNoTracking()
            .Where(item => item.CanonicalName == name || item.NormalizedName == normalizedName)
            .OrderByDescending(item => item.LanguageCode == "ko")
            .ThenByDescending(item => item.RecipeIngredients.Count)
            .ThenBy(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static OfficialFoodIngredientCompanyArchivedOrganizationDto ToOrganization(
        IReadOnlyList<OfficialFoodIngredientCompanyEvidence> evidence)
    {
        var first = evidence[0];
        var orderedEvidence = evidence
            .OrderByDescending(item => item.IsCurrent)
            .ThenByDescending(item => item.LastObservedAtUtc)
            .ThenBy(item => item.RelatedProductName, StringComparer.OrdinalIgnoreCase)
            .Select(item => new OfficialFoodIngredientCompanyArchivedEvidenceDto(
                item.CandidateKey,
                item.EvidenceCode,
                item.EvidenceSummary,
                item.RelatedProductName,
                item.ProductCategory,
                item.OfficialIdentifier,
                item.EvidenceRecordIdentifier,
                item.VerificationStatusCode,
                item.RawIngredientText,
                item.EvidenceDate,
                item.EvidenceLastChangedDate,
                item.EvidenceSequence,
                item.SourceKey,
                item.SourceName,
                item.SourceUrl,
                item.RequiresAttention,
                item.AttentionReason,
                AsUtcOffset(item.FirstObservedAtUtc),
                AsUtcOffset(item.LastObservedAtUtc),
                item.ObservationCount,
                item.IsCurrent,
                true,
                false,
                false))
            .ToArray();
        var attention = evidence.FirstOrDefault(item => item.RequiresAttention);
        return new OfficialFoodIngredientCompanyArchivedOrganizationDto(
            first.OrganizationKey,
            first.OrganizationName,
            first.CountryCode,
            first.CountryName,
            first.RelationCode,
            evidence
                .OrderByDescending(item => VerificationRank(item.VerificationStatusCode))
                .Select(item => item.VerificationStatusCode)
                .FirstOrDefault() ?? string.Empty,
            attention is not null,
            attention?.AttentionReason ?? string.Empty,
            AsUtcOffset(evidence.Min(item => item.FirstObservedAtUtc)),
            AsUtcOffset(evidence.Max(item => item.LastObservedAtUtc)),
            orderedEvidence.Length,
            orderedEvidence);
    }

    private static int VerificationRank(string statusCode)
        => statusCode switch
        {
            OfficialFoodIngredientCompanyVerificationStatusCodes.OverseasFacilityMatched => 3,
            OfficialFoodIngredientCompanyVerificationStatusCodes.OfficialProductReport => 2,
            OfficialFoodIngredientCompanyVerificationStatusCodes.ImportedLabelEvidenceOnly => 1,
            _ => 0
        };

    private static string OrganizationKey(OfficialFoodIngredientCompanyCandidateDto candidate)
    {
        var value = string.Join(
            '|',
            candidate.RelationCode.Trim(),
            candidate.CountryCode.Trim().ToUpperInvariant(),
            Normalize(candidate.OrganizationName));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();
    }

    private static string Normalize(string? value)
        => string.Concat((value ?? string.Empty)
            .Where(char.IsLetterOrDigit))
            .ToUpperInvariant();

    private static int DistinctOrganizationCount(
        IEnumerable<OrganizationRelation> items,
        string relationCode)
        => items
            .Where(item => item.RelationCode == relationCode)
            .Select(item => item.OrganizationKey)
            .Distinct(StringComparer.Ordinal)
            .Count();

    private static string DetermineRunStatus(CollectionCounters counters)
        => counters.FailedIngredientCount > 0 && counters.ProcessedIngredientCount == 0
            ? OfficialFoodIngredientCompanyResearchRunStatusCodes.Failed
            : counters.FailedIngredientCount > 0
              || counters.PartialIngredientCount > 0
              || counters.NotConfiguredIngredientCount > 0
                ? OfficialFoodIngredientCompanyResearchRunStatusCodes.Partial
                : OfficialFoodIngredientCompanyResearchRunStatusCodes.Completed;

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private static DateTimeOffset AsUtcOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static string Truncate(string? value, int maxLength)
    {
        var text = value?.Trim() ?? string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength];
    }

    private sealed record ArchiveStats(int OrganizationCount, int EvidenceCount);

    private sealed record OrganizationRelation(string OrganizationKey, string RelationCode);

    private sealed class CollectionCounters
    {
        public int AttemptedIngredientCount { get; set; }

        public int ProcessedIngredientCount { get; private set; }

        public int AvailableIngredientCount { get; private set; }

        public int PartialIngredientCount { get; private set; }

        public int NoResultIngredientCount { get; private set; }

        public int NotConfiguredIngredientCount { get; private set; }

        public int FailedIngredientCount { get; set; }

        public int ObservedEvidenceCount { get; private set; }

        public void Add(OfficialFoodIngredientCompanyResearchResponse response)
        {
            ProcessedIngredientCount++;
            ObservedEvidenceCount += response.Candidates.Count;
            switch (response.StatusCode)
            {
                case OfficialFoodIngredientCompanyResearchStatusCodes.Available:
                    AvailableIngredientCount++;
                    break;
                case OfficialFoodIngredientCompanyResearchStatusCodes.Partial:
                    PartialIngredientCount++;
                    break;
                case OfficialFoodIngredientCompanyResearchStatusCodes.NotConfigured:
                    NotConfiguredIngredientCount++;
                    break;
                case OfficialFoodIngredientCompanyResearchStatusCodes.NoResults:
                    NoResultIngredientCount++;
                    break;
                default:
                    FailedIngredientCount++;
                    break;
            }
        }

        public static CollectionCounters From(
            OfficialFoodIngredientCompanyResearchResponse response,
            int observedEvidenceCount)
        {
            var counters = new CollectionCounters();
            counters.Add(response);
            counters.ObservedEvidenceCount = observedEvidenceCount;
            counters.AttemptedIngredientCount = 1;
            return counters;
        }

        public OfficialFoodIngredientCompanyCollectionResponse ToResponse(
            string runKey,
            string triggerCode,
            string statusCode,
            int requestedIngredientCount,
            int skippedIngredientCount,
            DateTime startedAtUtc,
            DateTime completedAtUtc)
            => new(
                runKey,
                triggerCode,
                statusCode,
                requestedIngredientCount,
                ProcessedIngredientCount,
                skippedIngredientCount,
                AvailableIngredientCount,
                PartialIngredientCount,
                NoResultIngredientCount,
                NotConfiguredIngredientCount,
                FailedIngredientCount,
                ObservedEvidenceCount,
                AsUtcOffset(startedAtUtc),
                AsUtcOffset(completedAtUtc));
    }
}
