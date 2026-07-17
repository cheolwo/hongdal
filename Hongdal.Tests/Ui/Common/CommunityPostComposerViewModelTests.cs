using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace Hongdal.Tests.Ui.Common;

public sealed class CommunityPostComposerViewModelTests
{
    [Fact]
    public void 글초안은_필수값과활동국가를검증한다()
    {
        var draft = new CommunityPostComposerDraftViewModel();

        Assert.NotNull(draft.Validate());

        draft.Nickname = "테스터";
        draft.Password = "secret";
        draft.Category = "자유";
        draft.WorkflowTag = "커뮤니티 신뢰";
        draft.RoleTag = "플랫폼 구성원";
        draft.Title = "공동수입 참여자를 찾습니다";
        draft.Body = "조건을 먼저 함께 확인하고 싶습니다.";
        draft.IsAuthorDisplayCountryPublic = true;
        draft.AuthorDisplayCountryCode = "K";

        Assert.Contains("ISO 알파-2", draft.Validate());

        draft.AuthorDisplayCountryCode = "KR";
        draft.AuthorDisplayCountryName = "대한민국";
        Assert.Null(draft.Validate());
    }

    [Fact]
    public async Task 임시저장은_비밀번호를제외하고_다시열때복원한다()
    {
        var store = new InMemoryDraftStore();
        using var first = CreateComposer(store);
        first.Configure("shipper", "화주");
        first.Draft.Nickname = "테스터";
        first.Draft.Password = "저장하면안됨";
        first.Draft.Title = "입고 예정 확인";
        first.Draft.Body = "업체별 입고 예정품을 확인합니다.";

        await first.SaveLocalDraftAsync();

        Assert.NotNull(store.Snapshot);
        Assert.DoesNotContain("저장하면안됨", System.Text.Json.JsonSerializer.Serialize(store.Snapshot));

        using var restored = CreateComposer(store);
        restored.Configure("shipper", "화주");
        await restored.LoadLocalDraftAsync();
        restored.Open();

        Assert.Equal("입고 예정 확인", restored.Draft.Title);
        Assert.Equal(string.Empty, restored.Draft.Password);
        Assert.Equal(CommunityComposerMessageKind.Info, restored.StatusKind);
    }

    [Fact]
    public void 수정시작은_게시글을초안에복사하고_설정을연다()
    {
        using var composer = CreateComposer(new InMemoryDraftStore());
        composer.Configure("shipper", "화주");

        composer.BeginEdit(new PlatformCommunityPostResponse
        {
            Id = 17,
            Nickname = "작성자",
            Category = "업무 질문",
            WorkflowTag = "창고·커머스 이행",
            RoleTag = "창고 관리자",
            Title = "입고 질문",
            Body = "검수 순서를 알고 싶습니다."
        });

        Assert.Equal(17, composer.EditingPostId);
        Assert.True(composer.IsOpen);
        Assert.True(composer.IsSettingsOpen);
        Assert.Equal("입고 질문", composer.Draft.Title);
        Assert.Equal(string.Empty, composer.Draft.Password);
    }

    private static CommunityPostComposerViewModel CreateComposer(
        ICommunityPostComposerDraftStore store)
    {
        var service = new PlatformCommunityService(new HttpClient(), null!);
        return new CommunityPostComposerViewModel(service, store);
    }

    private sealed class InMemoryDraftStore : ICommunityPostComposerDraftStore
    {
        public CommunityPostComposerSnapshot? Snapshot { get; private set; }

        public Task<CommunityPostComposerSnapshot?> LoadAsync(
            string appKey,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Snapshot);

        public Task SaveAsync(
            string appKey,
            CommunityPostComposerSnapshot snapshot,
            CancellationToken cancellationToken = default)
        {
            Snapshot = snapshot;
            return Task.CompletedTask;
        }

        public Task ClearAsync(
            string appKey,
            CancellationToken cancellationToken = default)
        {
            Snapshot = null;
            return Task.CompletedTask;
        }
    }
}
