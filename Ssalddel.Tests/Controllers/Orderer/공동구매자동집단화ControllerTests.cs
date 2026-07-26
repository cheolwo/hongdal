using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Tests.Controllers.Orderer;

public sealed class 공동구매자동집단화ControllerTests
{
    [Fact]
    public void Controller는인증사용자만허용한다()
        => Assert.NotNull(typeof(공동구매자동집단화Controller)
            .GetCustomAttribute<AuthorizeAttribute>());

    [Fact]
    public void 공개목록은_비로그인탐색을허용한다()
        => Assert.NotNull(typeof(공동구매자동집단화Controller)
            .GetMethod(nameof(공동구매자동집단화Controller.목록))!
            .GetCustomAttribute<AllowAnonymousAttribute>());

    [Fact]
    public void 공개상세는_비로그인탐색을허용한다()
        => Assert.NotNull(typeof(공동구매자동집단화Controller)
            .GetMethod(nameof(공동구매자동집단화Controller.상세))!
            .GetCustomAttribute<AllowAnonymousAttribute>());

    [Fact]
    public async Task 목록은참여자와결제정보가없는요약만반환한다()
    {
        var useCase = new RecordingUseCase
        {
            Groups =
            [
                Group(
                    Demand("orderer-a", "참여자 A", "address:a", 10_001m),
                    Demand("orderer-b", "참여자 B", "address:b", 20_002m))
            ]
        };
        var controller = Controller(useCase, "viewer");

        var result = await controller.목록(null, null, null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var groups = Assert.IsType<공동구매자동집단요약응답[]>(ok.Value);
        var group = Assert.Single(groups);
        Assert.Equal("auto-group-1", group.자동집단Id);
        Assert.Equal(2, group.수요건수);

        var json = JsonSerializer.Serialize(ok.Value);
        Assert.DoesNotContain(nameof(공동구매자동집단응답.수요목록), json, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(공동구매자동집단응답.공동구매주문집계원장Id), json, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(공동구매자동수요응답.개별원함원장Id), json, StringComparison.Ordinal);
        Assert.DoesNotContain("wish-ledger-orderer-a", json, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(공동구매자동집단응답.예약결제합계), json, StringComparison.Ordinal);
        Assert.DoesNotContain("orderer-a", json, StringComparison.Ordinal);
        Assert.DoesNotContain("참여자 A", json, StringComparison.Ordinal);
        Assert.DoesNotContain("address:a", json, StringComparison.Ordinal);
        Assert.DoesNotContain("10001", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 상세는_배송권집계와참여판단만반환하고_개인정보는반환하지않는다()
    {
        var detail = Group(
            Demand("orderer-a", "참여자 A", "address:a", 10_001m),
            Demand("orderer-b", "참여자 B", "address:b", 20_002m));
        detail.상품키 = "apple-5kg";
        detail.배송권키 = "kr:11:11470:1147051000";
        detail.배송권명 = "서울 양천구 목5동";
        detail.참여자수 = 5;
        detail.목표참여자수 = 8;
        detail.총희망수량 = 7;
        detail.목표수량 = 10;
        var useCase = new RecordingUseCase { DetailResult = detail };
        var controller = Controller(useCase, "viewer");

        var result = await controller.상세("auto-group-1", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<같이주문공개상세응답>(ok.Value);
        Assert.Equal("같이 주문", response.같이주문표시명);
        Assert.Equal(3, response.추가필요참여자수);
        Assert.Equal(3, response.추가필요수량);
        Assert.True(response.참여가능여부);
        Assert.True(response.비구속수요만허용);
        Assert.True(response.자동참여금지);
        Assert.Equal(
            "/group-purchase/delivery-scopes/kr%3A11%3A11470%3A1147051000",
            response.배송권보기경로);
        Assert.Equal(
            "/group-purchase/compare/apple-5kg",
            response.주문방식비교경로);

        var json = JsonSerializer.Serialize(response);
        Assert.DoesNotContain(nameof(공동구매자동집단응답.수요목록), json, StringComparison.Ordinal);
        Assert.DoesNotContain("orderer-a", json, StringComparison.Ordinal);
        Assert.DoesNotContain("참여자 A", json, StringComparison.Ordinal);
        Assert.DoesNotContain("address:a", json, StringComparison.Ordinal);
        Assert.DoesNotContain("10001", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 기존Post수요등록도_로그인사용자의비구속저장으로강제한다()
    {
        var useCase = new RecordingUseCase
        {
            NonBindingSaveResult = Group(
                Demand("authenticated-orderer", "인증 주문자", "address:mine", 30_003m, "source-mine"),
                Demand("other-orderer", "다른 참여자", "address:other", 40_004m, "source-other"))
        };
        var controller = Controller(useCase, "authenticated-orderer", "인증 주문자");
        var command = new 공동구매자동수요등록Command
        {
            수요출처키 = "source-mine",
            주문자키 = "spoofed-orderer",
            주문자표시명 = string.Empty
        };

        var result = await controller.수요등록(command, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Null(useCase.LastCommand);
        Assert.Same(command, useCase.LastNonBindingCommand);
        Assert.Equal("authenticated-orderer", useCase.LastNonBindingCommand!.주문자키);
        Assert.Equal("인증 주문자", useCase.LastNonBindingCommand.주문자표시명);
        Assert.StartsWith("legacy-post:", useCase.LastNonBindingCommand.요청멱등키, StringComparison.Ordinal);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<공동구매자동집단사용자응답>(ok.Value);
        var ownDemand = Assert.Single(response.수요목록);
        Assert.Equal("authenticated-orderer", ownDemand.주문자키);
        Assert.Equal("wish-ledger-authenticated-orderer", ownDemand.개별원함원장Id);
        Assert.Equal(30_003m, ownDemand.예약결제금액);
        Assert.Equal("aggregation-ledger-1", response.공동구매주문집계원장Id);

        var json = JsonSerializer.Serialize(ok.Value);
        Assert.DoesNotContain(nameof(공동구매자동수요응답.주문자표시명), json, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(공동구매자동수요응답.수령지주소참조키), json, StringComparison.Ordinal);
        Assert.DoesNotContain("other-orderer", json, StringComparison.Ordinal);
        Assert.DoesNotContain("다른 참여자", json, StringComparison.Ordinal);
        Assert.DoesNotContain("address:other", json, StringComparison.Ordinal);
        Assert.DoesNotContain("40004", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 배치미리보기는_로그인사용자를적용하고_수요를저장하지않는다()
    {
        var useCase = new RecordingUseCase
        {
            PreviewResult = new 공동구매자동집단배치미리보기응답
            {
                자동집단Id = "auto-group-preview",
                배치유형 = 공동구매자동집단배치유형코드.기존집단,
                예상진행 = new 공동구매자동집단진행응답
                {
                    참여자수 = 2,
                    현재상태 = 공동구매자동집단상태코드.수요수집중
                }
            }
        };
        var controller = Controller(useCase, "authenticated-orderer", "인증 주문자");
        var command = new 공동구매자동수요등록Command
        {
            주문자키 = "spoofed-orderer",
            주문자표시명 = string.Empty
        };

        var result = await controller.배치미리보기(command, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<공동구매자동집단배치미리보기응답>(ok.Value);
        Assert.Equal("auto-group-preview", response.자동집단Id);
        Assert.Same(command, useCase.LastPreviewCommand);
        Assert.Equal("authenticated-orderer", command.주문자키);
        Assert.Equal("인증 주문자", command.주문자표시명);
        Assert.Null(useCase.LastCommand);
    }

    [Fact]
    public async Task 비구속수요저장은_경로와로그인사용자와멱등키를적용한다()
    {
        var useCase = new RecordingUseCase
        {
            NonBindingSaveResult = Group(
                Demand("authenticated-orderer", "인증 주문자", "", 0, "ingredient:garlic:seoul"))
        };
        var controller = Controller(useCase, "authenticated-orderer", "인증 주문자");
        var command = new 공동구매자동수요등록Command
        {
            수요출처키 = "spoofed-source",
            주문자키 = "spoofed-orderer"
        };

        var result = await controller.비구속수요저장(
            "ingredient:garlic:seoul",
            command,
            "save-demand-1",
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<공동구매자동집단사용자응답>(ok.Value);
        Assert.Same(command, useCase.LastNonBindingCommand);
        Assert.Equal("ingredient:garlic:seoul", command.수요출처키);
        Assert.Equal("authenticated-orderer", command.주문자키);
        Assert.Equal("save-demand-1", command.요청멱등키);
    }

    [Fact]
    public async Task 비구속수요철회는_로그인사용자와멱등키와개별원함Revision을전달한다()
    {
        var useCase = new RecordingUseCase
        {
            WithdrawalResult = new 공동구매자동수요철회응답
            {
                철회완료 = true,
                수요출처키 = "ingredient:garlic:seoul"
            }
        };
        var controller = Controller(useCase, "authenticated-orderer");

        var result = await controller.비구속수요철회(
            "ingredient:garlic:seoul",
            "withdraw-demand-1",
            "더 이상 필요하지 않음",
            CancellationToken.None,
            개별원함기대Revision: 7);

        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(useCase.LastWithdrawalCommand);
        Assert.Equal("authenticated-orderer", useCase.LastWithdrawalCommand!.주문자키);
        Assert.Equal("withdraw-demand-1", useCase.LastWithdrawalCommand.요청멱등키);
        Assert.Equal("ingredient:garlic:seoul", useCase.LastWithdrawalCommand.수요출처키);
        Assert.Equal(7, useCase.LastWithdrawalCommand.개별원함기대Revision);
        Assert.Equal("더 이상 필요하지 않음", useCase.LastWithdrawalCommand.철회사유);
    }

    [Fact]
    public void 공개응답은기존클라이언트의핵심필드와역직렬화호환된다()
    {
        var response = new 공동구매자동집단사용자응답
        {
            자동집단Id = "auto-group-1",
            상품키 = "product-1",
            상품명 = "테스트 상품",
            공동구매주문집계원장Id = "aggregation-ledger-1",
            수요목록 =
            [
                new 공동구매자동본인수요응답
                {
                    수요Id = "demand-1",
                    수요출처키 = "source-mine",
                    주문자키 = "authenticated-orderer",
                    개별원함원장Id = "wish-ledger-1",
                    개별주문원장Id = "individual-ledger-1"
                }
            ]
        };

        var legacyResponse = JsonSerializer.Deserialize<공동구매자동집단응답>(
            JsonSerializer.Serialize(response));

        Assert.NotNull(legacyResponse);
        Assert.Equal(response.자동집단Id, legacyResponse.자동집단Id);
        Assert.Equal(response.공동구매주문집계원장Id, legacyResponse.공동구매주문집계원장Id);
        Assert.Equal("wish-ledger-1", Assert.Single(legacyResponse.수요목록).개별원함원장Id);
        Assert.Equal("individual-ledger-1", Assert.Single(legacyResponse.수요목록).개별주문원장Id);
    }

    private static 공동구매자동집단화Controller Controller(
        I공동구매자동집단화UseCase useCase,
        string userId,
        string? userName = null)
        => new(useCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Name, userName ?? userId)
                    ], "test"))
                }
            }
        };

    private static 공동구매자동집단응답 Group(params 공동구매자동수요응답[] demands)
        => new()
        {
            자동집단Id = "auto-group-1",
            공동구매주문집계원장Id = "aggregation-ledger-1",
            상품키 = "product-1",
            상품명 = "테스트 상품",
            배송권키 = "delivery-scope-1",
            배송권명 = "테스트 배송권",
            수요건수 = demands.Length,
            예약결제건수 = demands.Length,
            참여자수 = demands.Select(item => item.주문자키).Distinct(StringComparer.Ordinal).Count(),
            예약결제참여자수 = demands.Select(item => item.주문자키).Distinct(StringComparer.Ordinal).Count(),
            총희망수량 = demands.Sum(item => item.희망수량),
            수량단위 = "개",
            예약결제합계 = demands.Sum(item => item.예약결제금액 ?? 0),
            수요목록 = demands
        };

    private static 공동구매자동수요응답 Demand(
        string ordererId,
        string displayName,
        string addressReference,
        decimal reservationAmount,
        string? sourceKey = null)
        => new()
        {
            수요Id = $"demand-{ordererId}",
            수요출처키 = sourceKey ?? $"source-{ordererId}",
            자동집단Id = "auto-group-1",
            상품키 = "product-1",
            상품명 = "테스트 상품",
            주문자키 = ordererId,
            주문자표시명 = displayName,
            수령지주소참조키 = addressReference,
            개별원함원장Id = $"wish-ledger-{ordererId}",
            공동구매주문집계원장Id = "aggregation-ledger-1",
            개별주문원장Id = $"individual-ledger-{ordererId}",
            희망수량 = 1,
            수량단위 = "개",
            예약결제금액 = reservationAmount
        };

    private sealed class RecordingUseCase : I공동구매자동집단화UseCase
    {
        public 공동구매자동수요등록Command? LastCommand { get; private set; }
        public 공동구매자동수요등록Command? LastPreviewCommand { get; private set; }
        public 공동구매자동수요등록Command? LastNonBindingCommand { get; private set; }
        public 공동구매자동수요철회Command? LastWithdrawalCommand { get; private set; }
        public IReadOnlyList<공동구매자동집단응답> Groups { get; set; } = [];
        public 공동구매자동집단응답? DetailResult { get; set; }
        public 공동구매자동집단응답 RegisterResult { get; set; } = new();
        public 공동구매자동집단응답 NonBindingSaveResult { get; set; } = new();
        public 공동구매자동집단배치미리보기응답 PreviewResult { get; set; } = new();
        public 공동구매자동수요철회응답 WithdrawalResult { get; set; } = new();

        public Task<공동구매처리결과<IReadOnlyList<공동구매자동집단응답>>> 목록조회Async(
            공동구매자동집단조회조건 조건,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                공동구매처리결과<IReadOnlyList<공동구매자동집단응답>>.성공결과(Groups));

        public Task<공동구매처리결과<공동구매자동집단응답>> 상세조회Async(
            string 자동집단Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(DetailResult is null
                ? 공동구매처리결과<공동구매자동집단응답>.찾을수없음("같이 주문을 찾을 수 없습니다.")
                : 공동구매처리결과<공동구매자동집단응답>.성공결과(DetailResult));

        public Task<공동구매처리결과<공동구매자동집단배치미리보기응답>> 배치미리보기Async(
            공동구매자동수요등록Command command,
            CancellationToken cancellationToken = default)
        {
            LastPreviewCommand = command;
            return Task.FromResult(
                공동구매처리결과<공동구매자동집단배치미리보기응답>.성공결과(PreviewResult));
        }

        public Task<공동구매처리결과<공동구매자동집단응답>> 수요등록Async(
            공동구매자동수요등록Command command,
            CancellationToken cancellationToken = default)
        {
            LastCommand = command;
            return Task.FromResult(
                공동구매처리결과<공동구매자동집단응답>.성공결과(RegisterResult));
        }

        public Task<공동구매처리결과<공동구매자동집단응답>> 비구속수요저장Async(
            공동구매자동수요등록Command command,
            CancellationToken cancellationToken = default)
        {
            LastNonBindingCommand = command;
            return Task.FromResult(
                공동구매처리결과<공동구매자동집단응답>.성공결과(NonBindingSaveResult));
        }

        public Task<공동구매처리결과<공동구매자동수요철회응답>> 수요철회Async(
            공동구매자동수요철회Command command,
            CancellationToken cancellationToken = default)
        {
            LastWithdrawalCommand = command;
            return Task.FromResult(
                공동구매처리결과<공동구매자동수요철회응답>.성공결과(WithdrawalResult));
        }
    }
}
