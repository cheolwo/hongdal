using Microsoft.AspNetCore.Components;
using MudBlazor;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public partial class CommunityGroupPurchaseWorkspace
{
    [Inject]
    private I공동구매업무Service GroupPurchaseService { get; set; } = default!;

    [Inject]
    private 공동구매공개목록ViewModel 목록ViewModel { get; set; } = default!;

    [Inject]
    private 공동구매공개상세ViewModel 상세ViewModel { get; set; } = default!;

    [Inject]
    private ISsalddel현재사용자Context CurrentUserContext { get; set; } = default!;

    [Parameter]
    public Guid? CampaignId { get; set; }

    [Parameter]
    public EventCallback<Guid> CampaignSelected { get; set; }

    [Parameter]
    public EventCallback<Guid> CampaignCreated { get; set; }

    [Parameter]
    public EventCallback CreateCancelled { get; set; }

    [Parameter]
    public CommunityGroupPurchaseIngredientSeed? IngredientSeed { get; set; }

    [Parameter]
    public CommunityGroupPurchaseSurfaceKind Surface { get; set; }
        = CommunityGroupPurchaseSurfaceKind.List;

    [Parameter]
    public string? ObjectionStageCode { get; set; }

    private CommunityGroupPurchaseWorkspaceState State { get; } = new();

    private IReadOnlyList<CommunityVoteResponse> Campaigns => 목록ViewModel.모집목록;

    private IReadOnlyList<PlatformCommunityPostCommentResponse> Comments => 상세ViewModel.의견목록;

    private CommunityVoteResponse? SelectedCampaign => 상세ViewModel.공동구매;

    private bool IsAuthenticated => CurrentUserContext.현재사용자.인증됨;

    private bool IsBusy => _isCommandBusy || 목록ViewModel.처리중 || 상세ViewModel.처리중;

    private bool _isCommandBusy;
    private bool _initialized;
    private CommunityGroupPurchaseIngredientSeed? ActiveIngredientSeed { get; set; }
    private string? _appliedIngredientSeedFingerprint;

    private string BackHref
        => Surface == CommunityGroupPurchaseSurfaceKind.List
            ? CommunityPageRoutes.CollectiveActions
            : CommunityPageRoutes.GroupPurchase;

    private string BackLabel
        => Surface == CommunityGroupPurchaseSurfaceKind.List
            ? "함께 하는 일"
            : "공동구매 목록";

    private bool RequiresAuthenticatedCommand
        => Surface is CommunityGroupPurchaseSurfaceKind.Participation
            or CommunityGroupPurchaseSurfaceKind.Resolution
            or CommunityGroupPurchaseSurfaceKind.Signature;

    private string ActiveStageCode
        => Surface switch
        {
            CommunityGroupPurchaseSurfaceKind.Participation => CommunityGroupPurchasePresentation.StageRecruitment,
            CommunityGroupPurchaseSurfaceKind.Objections => CommunityGroupPurchasePresentation.StageObjection,
            CommunityGroupPurchaseSurfaceKind.Resolution => CommunityGroupPurchasePresentation.StageResolution,
            CommunityGroupPurchaseSurfaceKind.Signature => CommunityGroupPurchasePresentation.StageSignature,
            CommunityGroupPurchaseSurfaceKind.DeliveryOptions or CommunityGroupPurchaseSurfaceKind.FulfillmentDraft =>
                CommunityGroupPurchasePresentation.StageExecution,
            _ => CommunityGroupPurchasePresentation.StageProposal
        };

    private string EffectiveObjectionStageCode
        => CommunityGroupPurchasePresentation.Stages.Any(stage =>
            string.Equals(stage.Code, ObjectionStageCode, StringComparison.OrdinalIgnoreCase))
                ? ObjectionStageCode!
                : CommunityGroupPurchasePresentation.StageObjection;

    private string SurfaceHeading
        => Surface switch
        {
            CommunityGroupPurchaseSurfaceKind.List => "진행 중인 공동구매",
            CommunityGroupPurchaseSurfaceKind.Create => "새 공동구매 제안",
            CommunityGroupPurchaseSurfaceKind.Overview => "공동구매 조건과 진행 현황",
            CommunityGroupPurchaseSurfaceKind.Participation => "공동구매 수요 참여",
            CommunityGroupPurchaseSurfaceKind.Suppliers => "생산자·공급자 직접 찾기",
            CommunityGroupPurchaseSurfaceKind.Negotiation => "공급 조건 공개 협의",
            CommunityGroupPurchaseSurfaceKind.Objections => $"{CommunityGroupPurchasePresentation.StageTitle(EffectiveObjectionStageCode)} 단계 이의제기",
            CommunityGroupPurchaseSurfaceKind.Resolution => "모집 마감과 확정안",
            CommunityGroupPurchaseSurfaceKind.Signature => "공동구매 결의 전자서명",
            CommunityGroupPurchaseSurfaceKind.DeliveryOptions => "배송 가능 정보 확인",
            CommunityGroupPurchaseSurfaceKind.FulfillmentDraft => "이행·발주 원장 초안",
            _ => "공동구매"
        };

    private string SurfaceDescription
        => Surface switch
        {
            CommunityGroupPurchaseSurfaceKind.List => "공개 모집을 훑어보고 선택한 campaign ID의 상세 화면으로 이동합니다.",
            CommunityGroupPurchaseSurfaceKind.Create => "상품, 최소 수량과 수령 조건을 공개 제안으로 저장합니다. 제안은 결제나 계약 확정이 아닙니다.",
            CommunityGroupPurchaseSurfaceKind.Overview => "한 모집의 공개 조건과 서버 원본 절차 상태를 읽습니다.",
            CommunityGroupPurchaseSurfaceKind.Participation => "참여자가 수량과 수령 방법을 직접 정해 비구속 수요 의향을 남깁니다.",
            CommunityGroupPurchaseSurfaceKind.Suppliers => "플랫폼이 상대를 대신 정하지 않으며, 당사자가 공개 정보를 보고 직접 선택합니다.",
            CommunityGroupPurchaseSurfaceKind.Negotiation => "가격·규격·수량·이행 조건의 변경 기록을 덮어쓰지 않고 공개합니다.",
            CommunityGroupPurchaseSurfaceKind.Objections => "선택한 단계의 문제와 대안을 별도 기록으로 남겨 함께 검토합니다.",
            CommunityGroupPurchaseSurfaceKind.Resolution => "접수된 이의를 확인한 뒤 모집 마감과 결의문 초안을 차례로 처리합니다.",
            CommunityGroupPurchaseSurfaceKind.Signature => "결의문 당사자가 자신의 서명 요청을 직접 선택하고 동의합니다.",
            CommunityGroupPurchaseSurfaceKind.DeliveryOptions => "기사의 공개 가능 정보를 확인하되 추천·자동 배차·계약 확정은 수행하지 않습니다.",
            CommunityGroupPurchaseSurfaceKind.FulfillmentDraft => "상호 확인된 조건으로 Simulation 발주와 후속 원장 골격만 준비합니다.",
            _ => string.Empty
        };

    protected override async Task OnInitializedAsync()
    {
        ApplyIngredientSeed();
        if (Surface == CommunityGroupPurchaseSurfaceKind.List)
        {
            await LoadCampaignsAsync();
        }
        else if (Surface != CommunityGroupPurchaseSurfaceKind.Create
                 && CampaignId is Guid campaignId)
        {
            await SelectCampaignAsync(campaignId);
        }

        _initialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        ApplyIngredientSeed();

        if (!_initialized
            || Surface is CommunityGroupPurchaseSurfaceKind.List or CommunityGroupPurchaseSurfaceKind.Create
            || CampaignId is not Guid campaignId
            || 상세ViewModel.요청CampaignId == campaignId)
        {
            return;
        }

        await SelectCampaignAsync(campaignId);
    }

    private void ApplyIngredientSeed()
    {
        if (IngredientSeed is null)
        {
            return;
        }

        if (Surface != CommunityGroupPurchaseSurfaceKind.Create
            || string.Equals(
                _appliedIngredientSeedFingerprint,
                IngredientSeed.Fingerprint,
                StringComparison.Ordinal))
        {
            return;
        }

        ActiveIngredientSeed = IngredientSeed;
        _appliedIngredientSeedFingerprint = IngredientSeed.Fingerprint;
        State.Proposal.Title = IngredientSeed.SuggestedTitle;
        State.Proposal.ProductName = IngredientSeed.IngredientName;
        State.Proposal.ProductKey = IngredientSeed.SuggestedProductKey;
        State.Proposal.QuantityUnit = IngredientSeed.PurchaseUnit;
        State.Proposal.Description = IngredientSeed.BuildSuggestedDescription();
        State.StatusSeverity = Severity.Info;
        State.StatusMessage = IngredientSeed.IsGroupImportReview
            ? "같이 수입 검토 의향을 가져왔습니다. 음식의 국가와 실제 상품 출발국은 다를 수 있으므로 거래경로 정보를 별도로 확인해 주세요."
            : "공공 가격과 공식 레시피의 출처를 공동구매 제안 초안으로 가져왔습니다. 수량·포장·지역 조건을 확인해 주세요.";
    }

    private async Task LoadCampaignsAsync()
    {
        await 목록ViewModel.조회Async();
    }

    private Task RefreshCurrentSurfaceAsync()
        => Surface == CommunityGroupPurchaseSurfaceKind.List
            ? LoadCampaignsAsync()
            : CampaignId is Guid campaignId
                ? SelectCampaignAsync(campaignId)
                : Task.CompletedTask;

    private Task CancelCreateAsync()
        => CreateCancelled.HasDelegate
            ? CreateCancelled.InvokeAsync()
            : Task.CompletedTask;

    private Task OpenCampaignAsync(Guid campaignId)
        => CampaignSelected.HasDelegate
            ? CampaignSelected.InvokeAsync(campaignId)
            : Task.CompletedTask;

    private Task RetryCampaignAsync(Guid campaignId)
        => SelectCampaignAsync(campaignId);

    private async Task SelectCampaignAsync(Guid campaignId)
    {
        if (!await 상세ViewModel.조회Async(campaignId)
            || SelectedCampaign is null)
        {
            return;
        }

        목록ViewModel.공동구매갱신(SelectedCampaign);
        ApplySelectedCampaignDefaults(SelectedCampaign);
    }

    private void ApplySelectedCampaignDefaults(CommunityVoteResponse campaign)
    {
        State.Participation.OptionId = campaign.Options.FirstOrDefault()?.OptionId ?? string.Empty;
        State.Participation.PickupPointId = campaign.GroupPurchase?.PickupPoints.FirstOrDefault()?.PickupPointId;
        var allowedTransactionTypeCodes = NormalizeAllowedTransactionTypeCodes(
            campaign.GroupPurchase?.AllowedTransactionTypeCodes);
        State.Participation.PurchasingOrganizationReference = string.Empty;
        State.Participation.PurchasingOrganizationName = string.Empty;
        State.Participation.TransactionTypeCode = allowedTransactionTypeCodes.Contains(
            공동구매거래유형코드.B2C,
            StringComparer.Ordinal)
                ? 공동구매거래유형코드.B2C
                : allowedTransactionTypeCodes[0];
        State.Participation.PriceBasisCode = State.Participation.IsBusinessPurchase
            ? 공동구매가격표시기준코드.부가세별도
            : 공동구매가격표시기준코드.부가세포함;
        State.Participation.TaxInvoiceRequired = State.Participation.IsBusinessPurchase;
        State.ResolutionTitle = $"{campaign.Title} 공동구매 확정안";
        State.ResolutionText = CommunityGroupPurchasePresentation.DefaultResolutionText(campaign);
        State.ResetSignatureSelection();
    }

    private async Task CreateCampaignAsync()
    {
        if (!EnsureAuthenticatedCommand("공동구매 제안"))
        {
            return;
        }

        var draft = State.Proposal;
        if (string.IsNullOrWhiteSpace(draft.Title)
            || string.IsNullOrWhiteSpace(draft.ProductName)
            || string.IsNullOrWhiteSpace(draft.Nickname)
            || string.IsNullOrWhiteSpace(draft.Password))
        {
            ShowError("제안 제목, 상품명, 제안자 표시명과 게시글 비밀번호를 입력해 주세요.");
            return;
        }

        if (!draft.AllowConsumerPurchases && !draft.AllowBusinessPurchases)
        {
            ShowError("B2C 개인 소비 구매 또는 B2B 사업 목적 구매를 하나 이상 허용해 주세요.");
            return;
        }

        _isCommandBusy = true;
        PlatformCommunityPostResponse? post = null;
        try
        {
            post = await GroupPurchaseService.제안글생성Async(new PlatformCommunityPostCreateRequest
            {
                AppKey = "shipper",
                Category = CommunityBoardCatalog.Participation.DisplayName,
                WorkflowTag = "공동 구매",
                RoleTag = "구매 참여자",
                Title = draft.Title.Trim(),
                Body = BuildProposalPostBody(draft),
                Nickname = draft.Nickname.Trim(),
                Password = draft.Password
            });
            if (post is null)
            {
                throw new InvalidOperationException("제안 글 생성 응답이 비어 있습니다.");
            }

            var pickupPoints = string.IsNullOrWhiteSpace(draft.PickupPointName)
                ? Array.Empty<CommunityVotePickupPointRequest>()
                :
                [
                    new CommunityVotePickupPointRequest
                    {
                        PickupPointId = $"pickup-{Guid.NewGuid():N}"[..20],
                        Name = draft.PickupPointName.Trim(),
                        AddressSummary = draft.PickupPointAddress.Trim(),
                        StorageTypeCode = CommunityVotePickupStorageTypeCodes.Ambient,
                        MinimumParticipantCount = draft.MinimumParticipantCount,
                        MinimumTotalQuantity = draft.MinimumTotalQuantity
                    }
                ];
            var vote = await GroupPurchaseService.공동구매생성Async(new CommunityVoteCreateRequest
            {
                CommunityScope = string.IsNullOrWhiteSpace(draft.CommunityScope)
                    ? "platform"
                    : draft.CommunityScope.Trim(),
                Title = draft.Title.Trim(),
                Description = draft.Description.Trim(),
                SourcePostId = post.Id,
                StructuredOptions =
                [
                    new CommunityVoteOptionCreateRequest
                    {
                        Text = draft.ProductName.Trim(),
                        ProductKey = string.IsNullOrWhiteSpace(draft.ProductKey)
                            ? $"community-product:{post.Id}"
                            : draft.ProductKey.Trim(),
                        QuantityUnit = draft.QuantityUnit.Trim(),
                        TemperatureCode = "상온",
                        LogisticsMode = CommunityGroupImportInternationalTransportModeCodes.ReviewRequired
                    }
                ],
                ResolutionDocumentEnabled = true,
                SignatureRequired = true,
                ClosesAtUtc = DateTime.UtcNow.AddDays(7),
                CreatedByDisplayName = draft.Nickname.Trim(),
                GroupPurchase = new CommunityGroupPurchaseVoteSettingsRequest
                {
                    ParticipationPolicyCode = draft.ParticipationPolicyCode,
                    QuantityUnit = draft.QuantityUnit.Trim(),
                    AllowedTransactionTypeCodes = BuildAllowedTransactionTypeCodes(draft),
                    ServiceAreaKey = draft.CommunityScope.Trim(),
                    ServiceAreaLabel = draft.CommunityScope.Trim(),
                    RadiusMeters = draft.RadiusMeters,
                    MinimumParticipantCount = draft.MinimumParticipantCount,
                    MinimumTotalQuantity = draft.MinimumTotalQuantity,
                    PickupPoints = pickupPoints
                }
            });
            if (vote is null)
            {
                throw new InvalidOperationException("공동구매 수요 투표 생성 응답이 비어 있습니다.");
            }

            목록ViewModel.공동구매갱신(vote);
            await SelectCampaignAsync(vote.Id);
            ShowSuccess("제안 글과 공동구매 수요 투표를 만들었습니다.");
            if (CampaignCreated.HasDelegate)
            {
                await CampaignCreated.InvokeAsync(vote.Id);
            }
        }
        catch (Exception ex)
        {
            ShowError(post is null
                ? $"공동구매 제안을 만들지 못했습니다. {ex.Message}"
                : $"제안 글은 저장됐지만 수요 투표 연결에 실패했습니다. 게시글 번호 {post.Id}를 확인해 주세요. {ex.Message}");
        }
        finally
        {
            _isCommandBusy = false;
        }
    }

    private async Task CastDemandAsync()
    {
        if (!EnsureAuthenticatedCommand("수요 참여"))
        {
            return;
        }

        var participation = State.Participation;
        if (SelectedCampaign is null || string.IsNullOrWhiteSpace(participation.OptionId))
        {
            ShowError("참여할 상품을 선택해 주세요.");
            return;
        }

        if (participation.MethodCode == CommunityVoteParticipationMethodCodes.PickupPoint
            && string.IsNullOrWhiteSpace(participation.PickupPointId))
        {
            ShowError("공동 수령소를 선택해 주세요.");
            return;
        }

        var transactionTypeCode = 공동구매거래유형코드.정규화(
            participation.TransactionTypeCode);
        var allowedTransactionTypeCodes = NormalizeAllowedTransactionTypeCodes(
            SelectedCampaign.GroupPurchase?.AllowedTransactionTypeCodes);
        if (!allowedTransactionTypeCodes.Contains(transactionTypeCode, StringComparer.Ordinal))
        {
            ShowError("이 공동구매에서 허용하는 구매 목적을 선택해 주세요.");
            return;
        }

        if (transactionTypeCode == 공동구매거래유형코드.B2B
            && string.IsNullOrWhiteSpace(participation.PurchasingOrganizationName)
            && string.IsNullOrWhiteSpace(participation.PurchasingOrganizationReference))
        {
            ShowError("B2B 구매에는 구매 조직명을 입력해 주세요.");
            return;
        }

        await RunBusyAsync(async () =>
        {
            var updated = await GroupPurchaseService.수요참여Async(
                SelectedCampaign.Id,
                new CommunityVoteCastRequest
                {
                    VoterDisplayName = participation.DisplayName,
                    OptionIds = [participation.OptionId],
                    RequestedQuantity = participation.Quantity,
                    TransactionTypeCode = transactionTypeCode,
                    PriceBasisCode = 공동구매가격표시기준코드.정규화(
                        participation.PriceBasisCode,
                        transactionTypeCode),
                    PurchasingOrganizationReference = transactionTypeCode == 공동구매거래유형코드.B2B
                        ? participation.PurchasingOrganizationReference
                        : null,
                    PurchasingOrganizationName = transactionTypeCode == 공동구매거래유형코드.B2B
                        ? participation.PurchasingOrganizationName
                        : null,
                    TaxInvoiceRequired = transactionTypeCode == 공동구매거래유형코드.B2B
                        && participation.TaxInvoiceRequired,
                    ParticipationMethodCode = participation.MethodCode,
                    PickupPointId = participation.MethodCode == CommunityVoteParticipationMethodCodes.PickupPoint
                        ? participation.PickupPointId
                        : null,
                    AllowNearbyPickupPointFallback = true
                });
            if (updated is null)
            {
                throw new InvalidOperationException("공동구매 수요 참여 응답이 비어 있습니다.");
            }

            await RefreshSelectedCampaignAsync();
            ShowSuccess("공동구매 수요 참여가 반영됐습니다.");
        });
    }

    private async Task SubmitObjectionAsync()
    {
        var objection = State.Objection;
        if (SelectedCampaign?.SourcePostId is not long postId
            || string.IsNullOrWhiteSpace(objection.Nickname)
            || string.IsNullOrWhiteSpace(objection.Password)
            || string.IsNullOrWhiteSpace(objection.Body))
        {
            ShowError("표시명, 게시글 비밀번호와 이의 내용을 입력해 주세요.");
            return;
        }

        await RunBusyAsync(async () =>
        {
            var created = await GroupPurchaseService.이의등록Async(
                postId,
                new PlatformCommunityPostCommentCreateRequest
                {
                    Nickname = objection.Nickname.Trim(),
                    Password = objection.Password,
                    Body = $"[이의제기:{EffectiveObjectionStageCode}] {objection.Body.Trim()}"
                });
            if (created is not null)
            {
                상세ViewModel.의견추가(created);
            }

            objection.Body = string.Empty;
            ShowSuccess($"{CommunityGroupPurchasePresentation.StageTitle(EffectiveObjectionStageCode)} 단계에 이의제기를 등록했습니다.");
        });
    }

    private async Task CloseCampaignAsync()
    {
        if (!EnsureAuthenticatedCommand("모집 마감") || SelectedCampaign is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            var updated = await GroupPurchaseService.모집마감Async(
                SelectedCampaign.Id,
                new CommunityVoteCloseRequest
                {
                    ClosedByDisplayName = State.OperatorDisplayName
                });
            if (updated is null)
            {
                throw new InvalidOperationException("공동구매 모집 마감 응답이 비어 있습니다.");
            }

            await RefreshSelectedCampaignAsync();
            State.ObjectionReviewConfirmed = false;
            ShowSuccess("수요 모집을 마감했습니다. 이제 확정안 결의문을 만들 수 있습니다.");
        });
    }

    private async Task CreateResolutionAsync()
    {
        if (!EnsureAuthenticatedCommand("결의문 작성") || SelectedCampaign is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await GroupPurchaseService.결의문생성Async(
                SelectedCampaign.Id,
                new CommunityVoteResolutionDraftRequest
                {
                    DocumentTitle = State.ResolutionTitle,
                    ResolutionText = State.ResolutionText,
                    RequiredSigners = [],
                    LegalReviewRequested = true
                });
            await RefreshSelectedCampaignAsync();
            ShowSuccess("현재 참여자 전원을 서명 대상으로 포함한 결의문 초안을 만들었습니다.");
        });
    }

    private async Task MarkReadyToSignAsync()
    {
        if (!EnsureAuthenticatedCommand("서명 준비") || SelectedCampaign is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await GroupPurchaseService.서명준비Async(
                SelectedCampaign.Id,
                new CommunityVoteResolutionReadyToSignRequest
                {
                    ReviewedByDisplayName = State.OperatorDisplayName,
                    ReviewMemo = State.ReviewMemo
                });
            await RefreshSelectedCampaignAsync();
            ShowSuccess("검토를 완료하고 전자서명을 받을 수 있는 상태로 전환했습니다.");
        });
    }

    private async Task SubmitSignatureAsync(SsalddelSignatureCaptureResult signature)
    {
        if (!EnsureAuthenticatedCommand("전자서명"))
        {
            return;
        }

        if (SelectedCampaign is null
            || string.IsNullOrWhiteSpace(State.SelectedSignerPartyId)
            || !State.SignatureConsent)
        {
            ShowError("서명 요청을 선택하고 전자서명 동의 여부를 확인해 주세요.");
            return;
        }

        await RunBusyAsync(async () =>
        {
            await GroupPurchaseService.전자서명Async(
                SelectedCampaign.Id,
                new CommunityVoteResolutionSignRequest
                {
                    PartyId = State.SelectedSignerPartyId,
                    SignerDisplayName = signature.SignerName,
                    ConsentText = "공동구매 결의문을 확인했으며 확정안에 동의합니다.",
                    SignatureEvidencePayload = signature.SignatureDataUrl
                });
            await RefreshSelectedCampaignAsync();
            State.SignatureConsent = false;
            State.ResetSignatureSelection();
            ShowSuccess(SelectedCampaign?.ResolutionDocument?.SignaturePlan?.IsFullySigned == true
                ? "필수 구성원의 전자서명이 모두 완료됐습니다. 실행 단계로 이동할 수 있습니다."
                : "전자서명이 기록됐습니다. 남은 구성원의 서명을 기다립니다.");
        });
    }

    private async Task RefreshSelectedCampaignAsync()
    {
        if (SelectedCampaign is null)
        {
            return;
        }

        var campaignId = SelectedCampaign.Id;
        if (!await 상세ViewModel.조회Async(campaignId))
        {
            throw new InvalidOperationException(
                상세ViewModel.오류메시지 ?? "저장된 공동구매를 다시 조회하지 못했습니다.");
        }

        var refreshed = SelectedCampaign;
        if (refreshed is null)
        {
            throw new InvalidOperationException("저장된 공동구매를 공개 상세에서 다시 찾지 못했습니다.");
        }

        목록ViewModel.공동구매갱신(refreshed);
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        _isCommandBusy = true;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            _isCommandBusy = false;
        }
    }

    private bool EnsureAuthenticatedCommand(string actionName)
    {
        if (IsAuthenticated)
        {
            return true;
        }

        ShowError($"{actionName} 저장은 로그인이 필요합니다. 공개 모집 조회와 익명 이의제기는 로그인 없이 이용할 수 있습니다.");
        return false;
    }

    private static string BuildProposalPostBody(CommunityGroupPurchaseCampaignDraft draft)
        => string.Join(
            Environment.NewLine,
            draft.Description.Trim(),
            string.Empty,
            $"상품: {draft.ProductName.Trim()}",
            $"최소 참여: {draft.MinimumParticipantCount}명",
            $"최소 수량: {draft.MinimumTotalQuantity}{draft.QuantityUnit.Trim()}",
            $"구매 목적: {string.Join(", ", BuildAllowedTransactionTypeCodes(draft).Select(공동구매거래유형코드.표시명))}",
            $"참여 범위: {draft.CommunityScope.Trim()}",
            string.IsNullOrWhiteSpace(draft.PickupPointName)
                ? "공동 수령소: 지정하지 않음"
                : $"공동 수령소: {draft.PickupPointName.Trim()} · {draft.PickupPointAddress.Trim()}");

    private static IReadOnlyList<string> BuildAllowedTransactionTypeCodes(
        CommunityGroupPurchaseCampaignDraft draft)
    {
        var result = new List<string>(2);
        if (draft.AllowConsumerPurchases)
        {
            result.Add(공동구매거래유형코드.B2C);
        }

        if (draft.AllowBusinessPurchases)
        {
            result.Add(공동구매거래유형코드.B2B);
        }

        return result;
    }

    private static IReadOnlyList<string> NormalizeAllowedTransactionTypeCodes(
        IReadOnlyList<string>? transactionTypeCodes)
    {
        var normalized = (transactionTypeCodes ?? [])
            .Where(공동구매거래유형코드.지원여부)
            .Select(공동구매거래유형코드.정규화)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return normalized.Length == 0
            ? [공동구매거래유형코드.B2C]
            : normalized;
    }

    private void ShowSuccess(string message)
    {
        State.StatusSeverity = Severity.Success;
        State.StatusMessage = message;
    }

    private void ShowError(string message)
    {
        State.StatusSeverity = Severity.Error;
        State.StatusMessage = message;
    }
}
