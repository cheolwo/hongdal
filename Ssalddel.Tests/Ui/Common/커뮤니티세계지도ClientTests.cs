using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 커뮤니티세계지도ClientTests
{
    [Fact]
    public async Task 질문초안은_선택관측값과목적을_익명초안경로로보낸다()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new 커뮤니티세계지도질문초안Response
            {
                Evidence = new 커뮤니티세계지도EvidenceReferenceDto
                {
                    ObservationStableId = "price:kr:kamis"
                },
                SuggestedPost = new PlatformCommunityPostCreateRequest
                {
                    Title = "공개 근거 질문"
                }
            })
        });
        var client = CreateClient(handler);

        var result = await client.질문초안생성Async(
            "price:kr:kamis",
            new 커뮤니티세계지도질문초안Request
            {
                DatasetCode = "day-work",
                QuestionFocus = "가격 변화를 함께 확인해요"
            });

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal(
            "/api/v1/community/world-map/observations/price%3Akr%3Akamis/question-draft",
            handler.RequestUri?.PathAndQuery);
        Assert.Null(handler.AuthorizationScheme);
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("day-work", body.RootElement.GetProperty("datasetCode").GetString());
        Assert.Equal("가격 변화를 함께 확인해요", body.RootElement.GetProperty("questionFocus").GetString());
        Assert.Equal("공개 근거 질문", result.SuggestedPost.Title);
    }

    [Fact]
    public async Task 질문게시는_출처확인값과_Bearer토큰을_게시경로로보낸다()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new 커뮤니티세계지도질문게시Response
            {
                PostHref = "/community/posts/41",
                ProvisionalLedgerCreated = false
            })
        });
        var client = CreateClient(handler);

        var result = await client.질문게시Async(
            "price:kr:kamis",
            new 커뮤니티세계지도질문게시Request
            {
                DatasetCode = "day-work",
                Title = "왜 가격이 달라졌나요?",
                Body = "출처를 함께 확인하고 싶습니다.",
                Nickname = "자료확인자",
                Password = "1234",
                ConfirmSourceReference = true
            },
            "access-token");

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal(
            "/api/v1/community/world-map/observations/price%3Akr%3Akamis/questions",
            handler.RequestUri?.PathAndQuery);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("access-token", handler.AuthorizationParameter);
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.True(body.RootElement.GetProperty("confirmSourceReference").GetBoolean());
        Assert.Equal("자료확인자", body.RootElement.GetProperty("nickname").GetString());
        Assert.False(result.ProvisionalLedgerCreated);
    }

    private static 커뮤니티세계지도Client CreateClient(HttpMessageHandler handler)
        => new(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost/")
        });

    private sealed class CapturingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            Body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return responseFactory(request);
        }
    }
}
