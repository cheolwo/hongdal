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
}
