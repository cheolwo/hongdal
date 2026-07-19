using System.Net;
using System.Text;
using System.Text.Json;
using Ssalddel.Services.External.Typecast;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Speech;

public sealed class TypecastClientTests
{
    [Fact]
    public async Task 음성목록조회Async_공식V2응답을_모델과용도로_역직렬화한다()
    {
        string? apiKey = null;
        Uri? requestUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            apiKey = request.Headers.GetValues("X-API-KEY").Single();
            requestUri = request.RequestUri;
            return JsonResponse(
                """
                [
                  {
                    "voice_id": "tc_60e5426de8b95f1d3000d7b5",
                    "voice_name": "Olivia",
                    "models": [
                      { "version": "ssfm-v30", "emotions": ["normal", "happy"] }
                    ],
                    "gender": "female",
                    "age": "young_adult",
                    "use_cases": ["Audiobook", "E-learning"],
                    "voice_type": "original"
                  }
                ]
                """);
        });
        var sut = CreateClient(handler);

        var result = await sut.음성목록조회Async(
            new Typecast음성조회필터("ssfm-v30", "female", "young_adult", "E-learning", "original"),
            CancellationToken.None);

        var voice = Assert.Single(result);
        Assert.Equal("test-key", apiKey);
        Assert.Equal(
            "/v2/voices?model=ssfm-v30&gender=female&age=young_adult&use_cases=E-learning&voice_type=original",
            requestUri!.PathAndQuery);
        Assert.Equal("Olivia", voice.이름);
        Assert.Equal("original", voice.음성유형);
        Assert.Equal(["normal", "happy"], Assert.Single(voice.지원모델).지원감정);
        Assert.Equal(["Audiobook", "E-learning"], voice.용도);
    }

    [Fact]
    public async Task 음성합성Async_공식요청필드와_ApiKey를_보낸다()
    {
        string? body = null;
        string? apiKey = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            apiKey = request.Headers.GetValues("X-API-KEY").Single();
            body = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([1, 2, 3])
                {
                    Headers = { ContentType = new("audio/wav") }
                }
            };
        });
        var sut = CreateClient(handler);

        var result = await sut.음성합성Async(new Typecast음성합성요청
        {
            VoiceId = "tc_voice",
            텍스트 = "안녕하세요",
            모델 = "ssfm-v30",
            언어코드 = "kor",
            오디오형식 = "wav"
        }, CancellationToken.None);

        Assert.Equal("test-key", apiKey);
        Assert.Equal([1, 2, 3], result.오디오);
        Assert.Equal("audio/wav", result.ContentType);
        using var payload = JsonDocument.Parse(body!);
        Assert.Equal("tc_voice", payload.RootElement.GetProperty("voice_id").GetString());
        Assert.Equal("안녕하세요", payload.RootElement.GetProperty("text").GetString());
        Assert.Equal("wav", payload.RootElement.GetProperty("output").GetProperty("audio_format").GetString());
    }

    private static TypecastClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.typecast.ai") };
        return new TypecastClient(httpClient, Options.Create(new TypecastOptions
        {
            Enabled = true,
            ApiKey = "test-key"
        }));
    }

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(_responseFactory(request));
    }
}
