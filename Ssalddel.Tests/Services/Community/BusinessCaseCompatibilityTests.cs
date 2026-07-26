using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Extensions;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class BusinessCaseCompatibilityTests
{
    [Fact]
    public void 기존원장Dto는_BusinessCase기술계약으로같은내용을읽는다()
    {
        var 원장 = new 커뮤니티원장Dto
        {
            원장Id = "case-1",
            Revision = 3,
            커뮤니티Id = "platform",
            원장템플릿Key = "group-purchase",
            제목 = "동네 공동구매",
            원함 = "배송비를 함께 줄이고 싶어요.",
            상태 = 커뮤니티원장상태.진행중,
            현재단계Key = "collecting",
            블록목록 =
            [
                new 커뮤니티원장블록Dto
                {
                    BlockId = "demand",
                    BlockType = "demand",
                    Title = "수요",
                    State = "확인중",
                    담당자목록 =
                    [
                        new 커뮤니티원장블록담당자Dto
                        {
                            UserId = "user-1",
                            DisplayName = "참여자",
                            RoleLabel = "요청자"
                        }
                    ]
                }
            ],
            참여자목록 =
            [
                new 커뮤니티원장참여자Dto
                {
                    UserId = "user-1",
                    DisplayName = "참여자",
                    RoleLabel = "요청자",
                    ParticipationState = "참여중"
                }
            ],
            상태이력 =
            [
                new 커뮤니티원장상태이력Dto
                {
                    EventId = "event-3",
                    상태 = 커뮤니티원장상태.진행중,
                    이전상태 = 커뮤니티원장상태.초안,
                    변경자 = "user-1"
                }
            ]
        };

        var businessCase = Assert.IsAssignableFrom<IBusinessCaseRecord>(원장);
        var section = Assert.Single(businessCase.Sections);
        var participant = Assert.Single(businessCase.Participants);
        var history = Assert.Single(businessCase.History);

        Assert.Equal("case-1", businessCase.CaseId);
        Assert.Equal("group-purchase", businessCase.CaseTemplateKey);
        Assert.Equal("배송비를 함께 줄이고 싶어요.", businessCase.Intent);
        Assert.Equal(BusinessCaseStatus.InProgress, businessCase.Status);
        Assert.Equal("demand", section.SectionId);
        Assert.Equal("user-1", Assert.Single(section.Assignees).UserId);
        Assert.Equal("참여중", participant.ParticipationStatus);
        Assert.Equal(커뮤니티원장상태.초안, history.PreviousStatus);
    }

    [Fact]
    public void BusinessCase호환속성은_기존Json계약에추가로노출되지않는다()
    {
        var json = JsonSerializer.Serialize(new 커뮤니티원장Dto
        {
            원장Id = "case-1",
            제목 = "호환성 확인"
        });
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("case-1", root.GetProperty("원장Id").GetString());
        Assert.False(root.TryGetProperty("CaseId", out _));
        Assert.False(root.TryGetProperty("CaseTemplateKey", out _));
        Assert.False(root.TryGetProperty("Sections", out _));
    }

    [Fact]
    public async Task BusinessCaseStoreAdapter는_기존원장저장경로를그대로사용한다()
    {
        var 원장 = new 커뮤니티원장Dto
        {
            원장Id = "case-1",
            제목 = "저장 경로 확인"
        };
        var legacyStore = new 원장저장소Stub(원장);
        IBusinessCaseStore store = new BusinessCaseStoreAdapter(legacyStore);

        var saved = await store.SaveAsync(new 커뮤니티원장저장요청 { 원장Id = "case-1" }, "user-1");
        var found = await store.GetAsync("case-1");
        var listed = await store.ListAsync(new 커뮤니티원장조회조건());

        Assert.Same(원장, saved);
        Assert.Same(원장, found);
        Assert.Same(원장, Assert.Single(listed));
        Assert.Equal(1, legacyStore.저장호출수);
        Assert.Equal(1, legacyStore.조회호출수);
        Assert.Equal(1, legacyStore.목록호출수);
    }

    [Fact]
    public void 서버Di는_BusinessCaseStore호환Adapter를제공한다()
    {
        var services = new ServiceCollection();

        services.AddSsalddelDomainServices();

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IBusinessCaseStore)
            && descriptor.ImplementationType == typeof(BusinessCaseStoreAdapter)
            && descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    private sealed class 원장저장소Stub(커뮤니티원장Dto 원장) : I커뮤니티원장저장소
    {
        public int 저장호출수 { get; private set; }
        public int 조회호출수 { get; private set; }
        public int 목록호출수 { get; private set; }

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            저장호출수++;
            return Task.FromResult(원장);
        }

        public Task<커뮤니티원장Dto?> 원장조회Async(
            string 원장Id,
            CancellationToken cancellationToken = default)
        {
            조회호출수++;
            return Task.FromResult<커뮤니티원장Dto?>(원장);
        }

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
        {
            목록호출수++;
            return Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>([원장]);
        }

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => Task.FromResult<커뮤니티원장Dto?>(원장);
    }
}
