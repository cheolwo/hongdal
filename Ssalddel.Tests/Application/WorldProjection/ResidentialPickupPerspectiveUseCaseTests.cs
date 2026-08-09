using FluentResults;
using Ssalddel.Application.WorldProjection;
using Ssalddel.Contracts.Common.VehicleLoading;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Services.LogisticsProcessing.VehicleLoading;

namespace Ssalddel.Tests.Application.WorldProjection;

public sealed class ResidentialPickupPerspectiveUseCaseTests
{
    [Fact]
    public async Task 주문자관점은_본인하차항목을_개인정보없는_수령Object로_투영한다()
    {
        var reader = new Reader(Response());
        var useCase = new ResidentialPickupPerspectiveUseCase(reader);

        var result = await useCase.QueryOrdererAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(하차업무관점코드.주문자, reader.LastPerspective);
        Assert.Equal(ResidentialPickupRoleCodes.Orderer, result.Value.AuthorizedRoleCode);
        var point = Assert.Single(result.Value.PickupPoints);
        Assert.Equal("residential-pickup:91", point.StableId);
        Assert.Equal("unloading-task:71.91", point.CanonicalTaskStableId);
        Assert.Equal("공동 수령지", point.PickupPointLabel);
        Assert.Equal("내 수령 상품", point.RoleLabel);
        Assert.Equal(ResidentialPickupStatusCodes.Arrived, point.StatusCode);
    }

    [Fact]
    public async Task 운송자관점은_같은Object를_하차대상으로_표현한다()
    {
        var reader = new Reader(Response());
        var useCase = new ResidentialPickupPerspectiveUseCase(reader);

        var result = await useCase.QueryTransporterAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(하차업무관점코드.운송담당자, reader.LastPerspective);
        Assert.Equal(ResidentialPickupRoleCodes.Transporter, result.Value.AuthorizedRoleCode);
        Assert.Equal("내 하차 대상", Assert.Single(result.Value.PickupPoints).RoleLabel);
    }

    [Fact]
    public void Unity공동수령Contract에는_민감한_원본필드가_없다()
    {
        var propertyNames = typeof(ResidentialPickupPointResponse)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("주문자UserId", propertyNames);
        Assert.DoesNotContain("확정기사UserId", propertyNames);
        Assert.DoesNotContain("하차주소", propertyNames);
        Assert.DoesNotContain("하차상세주소", propertyNames);
        Assert.DoesNotContain("주문참조번호", propertyNames);
        Assert.DoesNotContain("연락처", propertyNames);
    }

    [Fact]
    public async Task 하위권한조회실패를_sample로_대체하지않고_전파한다()
    {
        var error = new Error("로그인 사용자를 확인할 수 없습니다.")
            .WithMetadata("StatusCode", 401);
        var useCase = new ResidentialPickupPerspectiveUseCase(
            new Reader(Result.Fail<하차관점페이지응답>(error)));

        var result = await useCase.QueryOrdererAsync();

        Assert.True(result.IsFailed);
        Assert.Equal(401, result.Errors[0].Metadata["StatusCode"]);
    }

    private static Result<하차관점페이지응답> Response()
    {
        return Result.Ok(new 하차관점페이지응답
        {
            Items =
            [
                new 하차관점항목응답
                {
                    출고예정Id = 91,
                    운송원장Id = 71,
                    창고입고연결여부 = true,
                    상품명 = "감자 20kg",
                    수량 = 3,
                    하차상태 = 하차작업상태코드.도착,
                    수정시각Utc = new DateTime(2026, 8, 8, 1, 0, 0, DateTimeKind.Utc),
                    주문자UserId = "private-orderer",
                    확정기사UserId = "private-driver",
                    하차주소 = "private-address",
                    하차상세주소 = "private-detail",
                    주문참조번호 = "private-order-reference",
                },
            ],
            TotalCount = 1,
            PageSize = 50,
        });
    }

    private sealed class Reader(Result<하차관점페이지응답> response)
        : IUnloadingPerspectiveReadService
    {
        public string? LastPerspective { get; private set; }

        public Task<Result<하차관점페이지응답>> QueryAsync(
            string perspectiveCode,
            string? communityLedgerId,
            하차관점목록조회요청 request,
            CancellationToken cancellationToken = default)
        {
            LastPerspective = perspectiveCode;
            return Task.FromResult(response);
        }
    }
}
