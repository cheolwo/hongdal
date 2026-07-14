using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.ContractManagement;
using Hongdal.Services.Community;

namespace Hongdal.Tests.Services.Community;

public sealed class CommunityVoteServiceTests
{
    [Fact]
    public async Task CastVote_SameVoter_ReplacesPreviousVote()
    {
        var service = new InMemoryCommunityVoteService();
        var vote = await service.CreateAsync(CreateVoteRequest(), CancellationToken.None);

        await service.CastVoteAsync(vote.Id, new CommunityVoteCastRequest
        {
            VoterDisplayName = "참여자 A",
            VoterKey = "user-a",
            OptionIds = ["option-1"]
        }, CancellationToken.None);

        var updated = await service.CastVoteAsync(vote.Id, new CommunityVoteCastRequest
        {
            VoterDisplayName = "참여자 A",
            VoterKey = "user-a",
            OptionIds = ["option-2"]
        }, CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(1, updated.TotalVoteCount);
        Assert.Equal(0, updated.Options.Single(x => x.OptionId == "option-1").VoteCount);
        Assert.Equal(1, updated.Options.Single(x => x.OptionId == "option-2").VoteCount);
    }

    [Fact]
    public async Task CreateResolutionDraft_WithLegalReview_RequiresReadyToSignBeforeSigning()
    {
        var service = new InMemoryCommunityVoteService();
        var vote = await service.CreateAsync(CreateVoteRequest(), CancellationToken.None);
        await service.CastVoteAsync(vote.Id, new CommunityVoteCastRequest
        {
            VoterDisplayName = "참여자 A",
            VoterKey = "user-a",
            OptionIds = ["option-1"]
        }, CancellationToken.None);
        await service.CloseAsync(vote.Id, new CommunityVoteCloseRequest { ClosedByDisplayName = "운영자" }, CancellationToken.None);

        var document = await service.CreateResolutionDraftAsync(vote.Id, new CommunityVoteResolutionDraftRequest
        {
            DocumentTitle = "공동 구매 결의문",
            ResolutionText = "공동 구매를 진행하기로 결의합니다.",
            LegalReviewRequested = true,
            RequiredSigners =
            [
                new CommunityVoteResolutionSignerRequest
                {
                    PartyId = "party-a",
                    RoleCode = "Participant",
                    SignerDisplayName = "참여자 A"
                }
            ]
        }, CancellationToken.None);

        Assert.NotNull(document);
        Assert.Equal(CommunityVoteResolutionStatusCodes.LegalReviewRequired, document.Status);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SignResolutionAsync(vote.Id, CreateSignRequest(), CancellationToken.None));

        var ready = await service.MarkResolutionReadyToSignAsync(vote.Id, new CommunityVoteResolutionReadyToSignRequest
        {
            ReviewedByDisplayName = "운영자"
        }, CancellationToken.None);
        Assert.Equal(CommunityVoteResolutionStatusCodes.ReadyToSign, ready?.Status);

        var signed = await service.SignResolutionAsync(vote.Id, CreateSignRequest(), CancellationToken.None);

        Assert.NotNull(signed);
        Assert.Equal(CommunityVoteResolutionStatusCodes.Signed, signed.Status);
        Assert.Equal(ContractSignatureStatusCode.Signed, signed.SignaturePlan?.StatusCode);
        Assert.True(signed.SignaturePlan?.IsFullySigned);
        Assert.False(string.IsNullOrWhiteSpace(signed.DocumentHash));
    }

    [Fact]
    public async Task GroupPurchaseDemand_PickupParticipantOutsideArea_IsAggregatedAtSelectedPickupPoint()
    {
        var handoff = new CapturingGroupPurchaseDemandHandoff();
        var service = new InMemoryCommunityVoteService(handoff);
        var vote = await service.CreateAsync(CreateGroupPurchaseVoteRequest(), CancellationToken.None);

        var updated = await service.CastVoteAsync(vote.Id, new CommunityVoteCastRequest
        {
            VoterDisplayName = "원거리 참여자",
            VoterKey = "remote-user-a",
            OptionIds = ["option-1"],
            RequestedQuantity = 3,
            ParticipationMethodCode = CommunityVoteParticipationMethodCodes.PickupPoint,
            PickupPointId = "seongsu-hub",
            AllowNearbyPickupPointFallback = true
        }, CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(CommunityVoteKindCodes.GroupPurchaseDemand, updated.VoteKind);
        Assert.Equal(3, updated.GroupPurchase?.TotalRequestedQuantity);
        Assert.False(updated.GroupPurchase?.IsMinimumReached);
        Assert.Equal(3, updated.Options.Single(x => x.OptionId == "option-1").RequestedQuantity);

        var pickupPoint = Assert.Single(updated.GroupPurchase!.PickupPoints);
        Assert.Equal(1, pickupPoint.ParticipantCount);
        Assert.Equal(3, pickupPoint.RequestedQuantity);
        Assert.True(pickupPoint.IsMinimumReached);
        Assert.NotNull(handoff.LastRequest);
        Assert.Equal("pickup-point:seongsu-hub", handoff.LastRequest.DeliveryScopeKey);
        Assert.Equal("성수 공동수령소", handoff.LastRequest.DeliveryScopeName);
        Assert.Equal("catalog:detergent-set", handoff.LastRequest.ProductKey);
        Assert.Equal("세제 세트", handoff.LastRequest.ProductName);
        Assert.Equal(101, handoff.LastRequest.SourcePostId);
        Assert.Equal("ledger-group-purchase-101", handoff.LastRequest.CommunityLedgerId);
        Assert.Equal(3, handoff.LastRequest.RequestedQuantity);
        Assert.Equal(1, handoff.LastRequest.MinimumParticipantCount);
        Assert.Equal(3, handoff.LastRequest.MinimumTotalQuantity);
    }

    [Fact]
    public async Task GroupPurchaseDemand_HandoffFailure_IsSavedAndRetriedWithoutLosingVote()
    {
        var handoff = new FailOnceGroupPurchaseDemandHandoff();
        var service = new InMemoryCommunityVoteService(handoff);
        var vote = await service.CreateAsync(CreateGroupPurchaseVoteRequest(), CancellationToken.None);

        var updated = await service.CastVoteAsync(
            vote.Id,
            CreatePickupVote("retry-user", 3),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(1, updated.TotalVoteCount);
        Assert.Equal(3, updated.GroupPurchase?.TotalRequestedQuantity);
        Assert.Equal(1, updated.GroupPurchase?.DemandHandoffPendingCount);
        Assert.Equal(0, updated.GroupPurchase?.DemandHandoffFailedCount);
        Assert.Equal(1, handoff.CallCount);

        Assert.True(await service.ProcessPendingDemandHandoffAsync(CancellationToken.None));

        var retried = await service.GetAsync(vote.Id, CancellationToken.None);
        Assert.NotNull(retried);
        Assert.Equal(1, retried.TotalVoteCount);
        Assert.Equal(3, retried.GroupPurchase?.TotalRequestedQuantity);
        Assert.Equal(0, retried.GroupPurchase?.DemandHandoffPendingCount);
        Assert.Equal(0, retried.GroupPurchase?.DemandHandoffFailedCount);
        Assert.Equal(2, handoff.CallCount);
    }

    [Fact]
    public async Task GroupPurchaseResolution_WithoutExplicitSigners_RequiresEveryCurrentParticipant()
    {
        var service = new InMemoryCommunityVoteService();
        var createRequest = CreateGroupPurchaseVoteRequest();
        createRequest.ResolutionDocumentEnabled = true;
        createRequest.SignatureRequired = true;
        var vote = await service.CreateAsync(createRequest, CancellationToken.None);
        await service.CastVoteAsync(vote.Id, CreatePickupVote("participant-a", 2), CancellationToken.None);
        await service.CastVoteAsync(vote.Id, CreatePickupVote("participant-b", 3), CancellationToken.None);
        await service.CloseAsync(vote.Id, new CommunityVoteCloseRequest
        {
            ClosedByDisplayName = "공동구매 운영자"
        }, CancellationToken.None);

        var resolution = await service.CreateResolutionDraftAsync(vote.Id, new CommunityVoteResolutionDraftRequest
        {
            DocumentTitle = "공동구매 확정안",
            ResolutionText = "수요 결과에 따라 공동구매를 진행합니다.",
            RequiredSigners = [],
            LegalReviewRequested = true
        }, CancellationToken.None);

        Assert.NotNull(resolution);
        Assert.NotNull(resolution.SignaturePlan);
        Assert.Equal(2, resolution.SignaturePlan.RequiredSignerCount);
        Assert.Equal(2, resolution.SignaturePlan.MissingRequiredPartyIds.Count);
        Assert.All(
            resolution.SignaturePlan.Bundle.SignatureRequests,
            signer => Assert.Equal("GroupPurchaseParticipant", signer.RoleCode));
    }

    [Fact]
    public async Task GroupPurchaseDemand_Recast_ReplacesPreviousPickupQuantity()
    {
        var service = new InMemoryCommunityVoteService();
        var vote = await service.CreateAsync(CreateGroupPurchaseVoteRequest(), CancellationToken.None);
        var request = new CommunityVoteCastRequest
        {
            VoterDisplayName = "참여자 A",
            VoterKey = "user-a",
            OptionIds = ["option-1"],
            RequestedQuantity = 2,
            ParticipationMethodCode = CommunityVoteParticipationMethodCodes.PickupPoint,
            PickupPointId = "seongsu-hub"
        };
        await service.CastVoteAsync(vote.Id, request, CancellationToken.None);

        request.OptionIds = ["option-2"];
        request.RequestedQuantity = 4;
        var updated = await service.CastVoteAsync(vote.Id, request, CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(1, updated.TotalVoteCount);
        Assert.Equal(4, updated.GroupPurchase?.TotalRequestedQuantity);
        Assert.Equal(0, updated.Options.Single(x => x.OptionId == "option-1").RequestedQuantity);
        Assert.Equal(4, updated.Options.Single(x => x.OptionId == "option-2").RequestedQuantity);
    }

    [Fact]
    public async Task GroupPurchaseDemand_PickupCapacityExceeded_IsRejected()
    {
        var service = new InMemoryCommunityVoteService();
        var request = CreateGroupPurchaseVoteRequest();
        request.GroupPurchase!.PickupPoints[0].CapacityQuantity = 5;
        var vote = await service.CreateAsync(request, CancellationToken.None);
        await service.CastVoteAsync(vote.Id, CreatePickupVote("user-a", 4), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CastVoteAsync(vote.Id, CreatePickupVote("user-b", 2), CancellationToken.None));

        Assert.Contains("보관 가능 수량", exception.Message);
    }

    [Fact]
    public async Task GroupPurchaseDemand_CommunityOnly_RejectsPickupOnlyParticipation()
    {
        var service = new InMemoryCommunityVoteService();
        var request = CreateGroupPurchaseVoteRequest();
        request.GroupPurchase!.ParticipationPolicyCode = CommunityVoteParticipationPolicyCodes.CommunityOnly;
        request.GroupPurchase.ServiceAreaKey = string.Empty;
        var vote = await service.CreateAsync(request, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CastVoteAsync(vote.Id, CreatePickupVote("user-a", 1), CancellationToken.None));

        Assert.Contains("허용하지 않는 참여 방법", exception.Message);
    }

    private static CommunityVoteCreateRequest CreateVoteRequest()
    {
        return new CommunityVoteCreateRequest
        {
            AppKey = "OrdererApp",
            CommunityScope = "orderer-group:apt-1",
            Title = "공동 구매 진행 여부",
            Description = "다음 달 공동 구매를 진행할지 결정합니다.",
            Options = ["진행", "보류"],
            ResolutionDocumentEnabled = true,
            SignatureRequired = true,
            CreatedByDisplayName = "운영자"
        };
    }

    private static CommunityVoteResolutionSignRequest CreateSignRequest()
    {
        return new CommunityVoteResolutionSignRequest
        {
            PartyId = "party-a",
            SignerDisplayName = "참여자 A",
            ConsentText = "본인은 결의문 내용과 전자서명에 동의합니다.",
            SignatureEvidencePayload = "signature-capture-or-provider-evidence"
        };
    }

    private static CommunityVoteCreateRequest CreateGroupPurchaseVoteRequest()
    {
        return new CommunityVoteCreateRequest
        {
            AppKey = "OrdererApp",
            CommunityScope = "orderer-group:seongsu",
            VoteKind = CommunityVoteKindCodes.GroupPurchaseDemand,
            SourcePostId = 101,
            CommunityLedgerId = "ledger-group-purchase-101",
            Title = "생활용품 공동구매 수요 조사",
            Description = "성수 생활권 또는 공동수령 거점 픽업 가능자를 모집합니다.",
            StructuredOptions =
            [
                new CommunityVoteOptionCreateRequest
                {
                    Text = "세제 세트",
                    ProductKey = "catalog:detergent-set"
                },
                new CommunityVoteOptionCreateRequest
                {
                    Text = "휴지 30롤",
                    ProductKey = "catalog:tissue-30"
                }
            ],
            CreatedByDisplayName = "공동구매 관리자",
            ClosesAtUtc = DateTime.UtcNow.AddDays(3),
            GroupPurchase = new CommunityGroupPurchaseVoteSettingsRequest
            {
                ParticipationPolicyCode = CommunityVoteParticipationPolicyCodes.Hybrid,
                HsCode = "3402.50",
                TemperatureCode = "상온",
                LogisticsMode = "DomesticBulk",
                QuantityUnit = "세트",
                ServiceAreaKey = "area:seongsu",
                ServiceAreaLabel = "성수동 생활권",
                RadiusMeters = 2_000,
                MinimumParticipantCount = 2,
                MinimumTotalQuantity = 5,
                PickupPoints =
                [
                    new CommunityVotePickupPointRequest
                    {
                        PickupPointId = "seongsu-hub",
                        Name = "성수 공동수령소",
                        AddressSummary = "성수역 2번 출구 인근",
                        StorageTypeCode = CommunityVotePickupStorageTypeCodes.Ambient,
                        CapacityQuantity = 100,
                        MinimumParticipantCount = 1,
                        MinimumTotalQuantity = 3,
                        PickupStartsAtUtc = DateTime.UtcNow.AddDays(4),
                        PickupEndsAtUtc = DateTime.UtcNow.AddDays(5)
                    }
                ]
            }
        };
    }

    private static CommunityVoteCastRequest CreatePickupVote(string voterKey, int quantity)
    {
        return new CommunityVoteCastRequest
        {
            VoterDisplayName = voterKey,
            VoterKey = voterKey,
            OptionIds = ["option-1"],
            RequestedQuantity = quantity,
            ParticipationMethodCode = CommunityVoteParticipationMethodCodes.PickupPoint,
            PickupPointId = "seongsu-hub"
        };
    }

    private sealed class CapturingGroupPurchaseDemandHandoff : ICommunityGroupPurchaseDemandHandoff
    {
        public CommunityGroupPurchaseDemandHandoffRequest? LastRequest { get; private set; }

        public Task<string> SyncAsync(
            CommunityGroupPurchaseDemandHandoffRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRequest = request;
            return Task.FromResult("auto-group-test");
        }
    }

    private sealed class FailOnceGroupPurchaseDemandHandoff : ICommunityGroupPurchaseDemandHandoff
    {
        public int CallCount { get; private set; }

        public Task<string> SyncAsync(
            CommunityGroupPurchaseDemandHandoffRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            if (CallCount == 1)
            {
                throw new InvalidOperationException("temporary group-purchase handoff failure");
            }

            return Task.FromResult("auto-group-retried");
        }
    }
}
