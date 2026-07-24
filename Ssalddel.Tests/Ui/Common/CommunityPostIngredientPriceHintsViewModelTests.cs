using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class CommunityPostIngredientPriceHintsViewModelTests
{
    [Fact]
    public async Task 본문을확인하면_가격힌트와경계안내를표시한다()
    {
        var client = new RecordingClient();
        var viewModel = new CommunityPostIngredientPriceHintsViewModel(client);

        await viewModel.RefreshNowAsync("사과 배 복숭아");

        Assert.True(viewModel.HasAnalyzedText);
        Assert.False(viewModel.IsLoading);
        Assert.Single(viewModel.Hints);
        Assert.Contains("실시간 판매가", viewModel.Notice, StringComparison.Ordinal);
        Assert.Equal("사과 배 복숭아", client.LastBody);
    }

    [Fact]
    public async Task 가격조회실패가_본문작성을막지않는다()
    {
        var viewModel = new CommunityPostIngredientPriceHintsViewModel(
            new ThrowingClient());

        await viewModel.RefreshNowAsync("사과");

        Assert.Empty(viewModel.Hints);
        Assert.True(viewModel.HasAnalyzedText);
        Assert.Contains("본문 작성은 계속", viewModel.StatusMessage, StringComparison.Ordinal);
    }

    private sealed class RecordingClient : ICommunityPostIngredientPriceHintClient
    {
        public string LastBody { get; private set; } = string.Empty;

        public Task<CommunityPostIngredientPriceHintResponse> GetHintsAsync(
            string body,
            CancellationToken cancellationToken = default)
        {
            LastBody = body;
            return Task.FromResult(new CommunityPostIngredientPriceHintResponse(
                [
                    new CommunityPostIngredientPriceHint(
                        "사과",
                        "사과",
                        "411",
                        false,
                        "사과로 연결했습니다.",
                        true,
                        30_000m,
                        29_000m,
                        31_000m,
                        "KRW",
                        "10개",
                        new DateOnly(2026, 7, 22),
                        "Retail",
                        "전국 소매 조사가격",
                        "후지",
                        1,
                        "KAMIS",
                        "https://www.kamis.or.kr")
                ],
                "저장된 조사값이며 실시간 판매가가 아닙니다.",
                new DateTime(2026, 7, 23, 0, 0, 0, DateTimeKind.Utc)));
        }
    }

    private sealed class ThrowingClient : ICommunityPostIngredientPriceHintClient
    {
        public Task<CommunityPostIngredientPriceHintResponse> GetHintsAsync(
            string body,
            CancellationToken cancellationToken = default)
            => throw new HttpRequestException("가격 조회 실패");
    }
}
