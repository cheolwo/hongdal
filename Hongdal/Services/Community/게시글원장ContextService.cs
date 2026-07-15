using FluentResults;
using Hongdal.Contracts.Common.Community;
using Microsoft.AspNetCore.Http;
using 홍달.Services.Versioning;

namespace Hongdal.Services.Community;

public interface I게시글원장ContextService
{
    Task<IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse>> 연결가능원장목록조회Async(
        string? 사용자UserId,
        string? 업무분류,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse>> 공유원장목록조회Async(
        string? 사용자UserId,
        string? 업무분류,
        CancellationToken cancellationToken);

    Task<Result<커뮤니티원장Dto>> 연결가능원장조회Async(
        string 원장Id,
        string? 사용자UserId,
        string? 업무분류,
        CancellationToken cancellationToken);

    Task<PlatformCommunityPostLedgerContextResponse?> 조회Async(
        string? 원장Id,
        string? 사용자UserId,
        CancellationToken cancellationToken);
}

public sealed class 게시글원장ContextService : I게시글원장ContextService
{
    private const int 최대포함원장깊이 = 4;
    private readonly I커뮤니티원장저장소 _원장저장소;
    private readonly IVersionFeatureFlagService _featureFlagService;
    private readonly I커뮤니티원장공유Service _공유Service;

    public 게시글원장ContextService(
        I커뮤니티원장저장소 원장저장소,
        IVersionFeatureFlagService featureFlagService,
        I커뮤니티원장공유Service 공유Service)
    {
        _원장저장소 = 원장저장소;
        _featureFlagService = featureFlagService;
        _공유Service = 공유Service;
    }

    public async Task<IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse>> 연결가능원장목록조회Async(
        string? 사용자UserId,
        string? 업무분류,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(사용자UserId))
        {
            return [];
        }

        var 요청분류 = string.IsNullOrWhiteSpace(업무분류)
            ? null
            : CommunityWorkClassificationCatalog.FindByWorkflowTag(업무분류);
        var 원장목록 = await _원장저장소.원장목록조회Async(
            new 커뮤니티원장조회조건
            {
                접근UserId = 사용자UserId.Trim(),
                Limit = 50
            },
            cancellationToken);

        return 원장목록
            .Select(원장 => (원장, 분류: CommunityWorkClassificationCatalog.FindByLedgerTemplate(원장.원장템플릿Key)))
            .Where(x => x.분류 is not null
                        && _featureFlagService.IsEnabled(x.분류.FeatureFlagKey)
                        && (요청분류 is null
                            || string.Equals(x.분류.Code, 요청분류.Code, StringComparison.OrdinalIgnoreCase)))
            .Select(x =>
            {
                var 내가만든원장 = string.Equals(x.원장.생성자UserId, 사용자UserId, StringComparison.OrdinalIgnoreCase);
                var 참여역할 = 내가만든원장
                    ? "생성자"
                    : x.원장.참여자목록.FirstOrDefault(participant =>
                        string.Equals(participant.UserId, 사용자UserId, StringComparison.OrdinalIgnoreCase))?.RoleLabel ?? "참여자";
                var template = CommunityLedgerTemplateCatalog.Find(x.원장.원장템플릿Key);

                return new PlatformCommunityPostLedgerChoiceResponse
                {
                    원장Id = x.원장.원장Id,
                    원장템플릿Key = x.원장.원장템플릿Key,
                    원장템플릿명 = template.DisplayName,
                    제목 = x.원장.제목,
                    상태 = x.원장.상태,
                    현재단계 = x.원장.현재단계Key ?? string.Empty,
                    업무분류명 = x.분류!.DisplayName,
                    WorkflowTag = x.분류.WorkflowTag,
                    내가만든원장 = 내가만든원장,
                    내접근원장여부 = true,
                    참여역할 = 참여역할,
                    수정시각Utc = x.원장.수정시각Utc
                };
            })
            .ToArray();
    }

    public Task<IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse>> 공유원장목록조회Async(
        string? 사용자UserId,
        string? 업무분류,
        CancellationToken cancellationToken)
        => _공유Service.공유원장목록조회Async(사용자UserId, 업무분류, cancellationToken);

    public async Task<Result<커뮤니티원장Dto>> 연결가능원장조회Async(
        string 원장Id,
        string? 사용자UserId,
        string? 업무분류,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(사용자UserId))
        {
            return Fail("원장을 게시글에 연결하려면 로그인이 필요합니다.", StatusCodes.Status401Unauthorized);
        }

        var 원장 = await _원장저장소.원장조회Async(원장Id.Trim(), cancellationToken);
        if (원장 is null)
        {
            return Fail("연결할 원장을 찾을 수 없습니다.", StatusCodes.Status404NotFound);
        }

        var 접근판정 = await _공유Service.접근판정Async(원장, 사용자UserId, cancellationToken);
        if (!접근판정.직접접근가능 && !접근판정.재공유가능)
        {
            return Fail("본인이 참여 중이거나 생성자가 재공유를 허용한 원장만 게시글에 연결할 수 있습니다.", StatusCodes.Status403Forbidden);
        }

        var 원장기준분류 = CommunityWorkClassificationCatalog.FindByLedgerTemplate(원장.원장템플릿Key);
        var 요청분류 = CommunityWorkClassificationCatalog.FindByWorkflowTag(업무분류) ?? 원장기준분류;
        if (요청분류 is null || !요청분류.LedgerTemplateKeys.Contains(원장.원장템플릿Key, StringComparer.OrdinalIgnoreCase))
        {
            return Fail("선택한 업무 분류와 연결하려는 원장 종류가 일치하지 않습니다.", StatusCodes.Status400BadRequest);
        }

        if (!_featureFlagService.IsEnabled(요청분류.FeatureFlagKey))
        {
            return Fail("현재 업무 분류의 기능 설정이 꺼져 있어 원장을 연결할 수 없습니다.", StatusCodes.Status409Conflict);
        }

        return Result.Ok(원장);
    }

    public async Task<PlatformCommunityPostLedgerContextResponse?> 조회Async(
        string? 원장Id,
        string? 사용자UserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(원장Id))
        {
            return null;
        }

        var 원장 = await _원장저장소.원장조회Async(원장Id.Trim(), cancellationToken);
        if (원장 is null)
        {
            return null;
        }

        var context = await 원장Context생성Async(원장, 사용자UserId, cancellationToken);
        if (context is null)
        {
            return null;
        }

        context.포함원장목록 = await 포함원장목록생성Async(
            원장,
            사용자UserId,
            context.상세조회가능여부,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { 원장.원장Id },
            깊이: 1,
            cancellationToken);
        return context;
    }

    private async Task<PlatformCommunityPostLedgerContextResponse?> 원장Context생성Async(
        커뮤니티원장Dto 원장,
        string? 사용자UserId,
        CancellationToken cancellationToken)
    {
        var 접근판정 = await _공유Service.접근판정Async(원장, 사용자UserId, cancellationToken);
        if (!접근판정.직접접근가능 && !접근판정.공개조회가능)
        {
            return null;
        }

        var 상세조회가능 = 접근판정.직접접근가능;
        var 공개항목 = 접근판정.정책.공개항목Key목록;
        var 제목공개 = 상세조회가능 || IsPublic(공개항목, 커뮤니티원장공개항목Key.제목);
        var 상태공개 = 상세조회가능 || IsPublic(공개항목, 커뮤니티원장공개항목Key.상태);
        var 현재단계공개 = 상세조회가능 || IsPublic(공개항목, 커뮤니티원장공개항목Key.현재단계);
        var 다이어그램공개 = 상세조회가능 || IsPublic(공개항목, 커뮤니티원장공개항목Key.다이어그램구조);
        var template = CommunityLedgerTemplateCatalog.Find(원장.원장템플릿Key);
        var 업무분류 = CommunityWorkClassificationCatalog.FindByLedgerTemplate(원장.원장템플릿Key);
        var 기능활성화 = 업무분류 is not null && _featureFlagService.IsEnabled(업무분류.FeatureFlagKey);

        return new PlatformCommunityPostLedgerContextResponse
        {
            원장Id = 원장.원장Id,
            Revision = 원장.Revision,
            원장템플릿Key = 원장.원장템플릿Key,
            원장템플릿명 = template.DisplayName,
            제목 = 제목공개 ? 원장.제목 : template.DisplayName,
            상태 = 상태공개 ? 원장.상태 : "공개 원장",
            현재단계 = 현재단계공개 ? 원장.현재단계Key ?? string.Empty : string.Empty,
            처리체계명 = template.TargetOperatingSystemName,
            업무분류Code = 업무분류?.Code ?? string.Empty,
            업무분류명 = 업무분류?.DisplayName ?? template.WorkflowTag,
            기능설정Key = 업무분류?.FeatureFlagKey ?? string.Empty,
            기능활성화여부 = 기능활성화,
            상세조회가능여부 = 상세조회가능,
            참여요청필요여부 = !상세조회가능,
            재사용허용여부 = 접근판정.재사용가능,
            재공유허용여부 = 접근판정.재공유가능,
            다이어그램 = 다이어그램공개 ? SanitizeDiagram(원장.다이어그램스냅샷, 상세조회가능) : null,
            블록목록 = SanitizeBlocks(원장.블록목록, 공개항목, 상세조회가능),
            가능한행동목록 = !기능활성화
                ? ["기능 준비 중"]
                : 상세조회가능
                    ? template.ActionHints
                    : BuildPublicActions(접근판정),
            노드행동목록 = 커뮤니티원장노드행동Policy.Build(
                원장,
                사용자UserId,
                상세조회가능,
                기능활성화)
        };
    }

    private async Task<IReadOnlyList<PlatformCommunityIncludedLedgerResponse>> 포함원장목록생성Async(
        커뮤니티원장Dto 상위원장,
        string? 사용자UserId,
        bool 상위원장상세조회가능,
        HashSet<string> 현재경로,
        int 깊이,
        CancellationToken cancellationToken)
    {
        if (깊이 > 최대포함원장깊이 || 상위원장.포함원장목록.Count == 0)
        {
            return [];
        }

        var result = new List<PlatformCommunityIncludedLedgerResponse>();
        foreach (var 참조 in 상위원장.포함원장목록.OrderBy(item => item.표시순서))
        {
            var template = CommunityLedgerTemplateCatalog.Find(참조.원장템플릿Key);
            if (현재경로.Contains(참조.원장Id))
            {
                if (상위원장상세조회가능)
                {
                    result.Add(제한항목(참조, template.DisplayName, "순환참조"));
                }

                continue;
            }

            var 하위원장 = await _원장저장소.원장조회Async(참조.원장Id, cancellationToken);
            if (하위원장 is null)
            {
                if (상위원장상세조회가능)
                {
                    result.Add(제한항목(참조, template.DisplayName, "원장누락"));
                }

                continue;
            }

            var 하위Context = await 원장Context생성Async(하위원장, 사용자UserId, cancellationToken);
            if (하위Context is null)
            {
                if (상위원장상세조회가능)
                {
                    result.Add(제한항목(참조, template.DisplayName, "접근권한필요"));
                }

                continue;
            }

            현재경로.Add(하위원장.원장Id);
            var 하위포함원장목록 = await 포함원장목록생성Async(
                하위원장,
                사용자UserId,
                하위Context.상세조회가능여부,
                현재경로,
                깊이 + 1,
                cancellationToken);
            현재경로.Remove(하위원장.원장Id);

            result.Add(new PlatformCommunityIncludedLedgerResponse
            {
                원장Id = 하위원장.원장Id,
                원장템플릿Key = 하위원장.원장템플릿Key,
                원장템플릿명 = CommunityLedgerTemplateCatalog.Find(하위원장.원장템플릿Key).DisplayName,
                역할 = 참조.역할,
                필수여부 = 참조.필수여부,
                표시순서 = 참조.표시순서,
                조회상태 = "정상",
                접근가능여부 = true,
                원장 = 하위Context,
                포함원장목록 = 하위포함원장목록
            });
        }

        return result;
    }

    private static PlatformCommunityIncludedLedgerResponse 제한항목(
        커뮤니티포함원장참조Dto 참조,
        string 원장템플릿명,
        string 조회상태)
        => new()
        {
            원장Id = 참조.원장Id,
            원장템플릿Key = 참조.원장템플릿Key,
            원장템플릿명 = 원장템플릿명,
            역할 = 참조.역할,
            필수여부 = 참조.필수여부,
            표시순서 = 참조.표시순서,
            조회상태 = 조회상태,
            접근가능여부 = false
        };

    private static IReadOnlyList<PlatformCommunityLedgerBlockResponse> SanitizeBlocks(
        IReadOnlyList<커뮤니티원장블록Dto> blocks,
        IReadOnlyList<string> publicKeys,
        bool detailed)
        => blocks
            .Select(block =>
            {
                var titleVisible = detailed || IsPublic(publicKeys, 커뮤니티원장공개항목Key.블록제목(block.BlockId));
                var stateVisible = detailed || IsPublic(publicKeys, 커뮤니티원장공개항목Key.블록상태(block.BlockId));
                var items = detailed
                    ? new Dictionary<string, string>(block.Data, StringComparer.OrdinalIgnoreCase)
                    : block.Data
                        .Where(item => IsPublic(publicKeys, 커뮤니티원장공개항목Key.블록Data(block.BlockId, item.Key)))
                        .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);

                return new
                {
                    HasVisibleValue = detailed || titleVisible || stateVisible || items.Count > 0,
                    Block = new PlatformCommunityLedgerBlockResponse
                    {
                        블록Id = block.BlockId,
                        블록유형 = block.BlockType,
                        제목 = titleVisible ? block.Title : "공개 블록",
                        상태 = stateVisible ? block.State : null,
                        항목 = items
                    }
                };
            })
            .Where(item => item.HasVisibleValue)
            .Select(item => item.Block)
            .ToArray();

    private static IReadOnlyList<string> BuildPublicActions(커뮤니티원장공유접근판정 access)
    {
        var actions = new List<string> { "참여 요청" };
        if (access.재사용가능) actions.Add("원장 재사용");
        if (access.재공유가능) actions.Add("게시글에 공유");
        return actions;
    }

    private static DiagramSnapshotDto? SanitizeDiagram(DiagramSnapshotDto? diagram, bool detailed)
    {
        if (diagram is null) return null;
        if (detailed) return diagram;
        return new DiagramSnapshotDto
        {
            DiagramId = diagram.DiagramId,
            DiagramName = diagram.DiagramName,
            LedgerId = diagram.LedgerId,
            LedgerTemplateKey = diagram.LedgerTemplateKey,
            WorkflowModeKey = diagram.WorkflowModeKey,
            Nodes = diagram.Nodes.Select(node => new DiagramNodeDto
            {
                NodeId = node.NodeId,
                Kind = node.Kind,
                Title = node.Title,
                GroupLabel = node.GroupLabel,
                X = node.X,
                Y = node.Y
            }).ToArray(),
            Edges = diagram.Edges.Select(edge => new DiagramEdgeDto
            {
                EdgeId = edge.EdgeId,
                FromNodeId = edge.FromNodeId,
                ToNodeId = edge.ToNodeId,
                Label = edge.Label,
                MeaningCode = edge.MeaningCode
            }).ToArray()
        };
    }

    private static bool IsPublic(IReadOnlyList<string> keys, string key)
        => keys.Contains(key, StringComparer.OrdinalIgnoreCase);

    private static Result<커뮤니티원장Dto> Fail(string message, int statusCode)
        => Result.Fail<커뮤니티원장Dto>(new Error(message).WithMetadata("StatusCode", statusCode));
}
