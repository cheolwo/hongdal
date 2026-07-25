using System.Reflection;
using Ssalddel.Controllers.Admin.Inflow02;
using Ssalddel.Controllers.Admin.Progress03;
using Ssalddel.Controllers.Common;
using Ssalddel.Controllers.Driver.Action03;
using Ssalddel.Controllers.Driver.Progress05;
using Ssalddel.Controllers.Driver.Recommendation02;
using Ssalddel.Controllers.Platform;
using Ssalddel.Controllers.Shipper.Payment02;
using Ssalddel.Controllers.Shipper.Request01;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Ssalddel.Tests.V1Smoke;

public sealed class SsalddelV1E2ESmokeContractTests
{
    [Fact]
    public void Core_transport_smoke_endpoints_remain_available()
    {
        AssertRoute<화주운송의뢰Controller>("api/v1/shipper/requests");
        AssertHttpMethod<화주운송의뢰Controller>(nameof(화주운송의뢰Controller.의뢰생성), "POST", null);
        AssertHttpMethod<화주운송의뢰Controller>(nameof(화주운송의뢰Controller.의뢰단건조회), "GET", "{requestId}");
        AssertHttpMethod<화주운송의뢰Controller>(nameof(화주운송의뢰Controller.후불승인), "POST", "{requestId}/settlement/postpay/approve");

        AssertRoute<화주결제Controller>("api/v1/payments");
        AssertHttpMethod<화주결제Controller>(nameof(화주결제Controller.페이크결제승인), "POST", "fake/confirm");

        AssertRoute<배차대기Controller>("api/v1/dispatch/wait");
        AssertHttpMethod<배차대기Controller>(nameof(배차대기Controller.목록조회), "GET", null);

        AssertRoute<기사배차추천Controller>("api/v1/driver/recommendations");
        AssertHttpMethod<기사배차추천Controller>(nameof(기사배차추천Controller.조회), "GET", null);

        AssertRoute<기사배차액션Controller>("api/v1/driver/dispatch-actions");
        AssertHttpMethod<기사배차액션Controller>(nameof(기사배차액션Controller.수락), "POST", "{requestId}/accept");
        AssertHttpMethod<기사배차액션Controller>(nameof(기사배차액션Controller.거절), "POST", "{requestId}/reject");

        AssertRoute<기사운송진행Controller>("api/v1/driver/transports");
        AssertHttpMethod<기사운송진행Controller>(nameof(기사운송진행Controller.현재조회), "GET", "current");
        AssertHttpMethod<기사운송진행Controller>(nameof(기사운송진행Controller.상차완료), "POST", "{id:long}/pickup-complete");
        AssertHttpMethod<기사운송진행Controller>(nameof(기사운송진행Controller.완료), "POST", "{id:long}/complete");

        AssertRoute<파일업로드Controller>("api/v1/files");
        AssertHttpMethod<파일업로드Controller>(nameof(파일업로드Controller.업로드), "POST", "upload");

        AssertRoute<운송원장Controller>("api/v1/transport-request-ledgers");
        AssertHttpMethod<운송원장Controller>(nameof(운송원장Controller.이벤트조회), "GET", "{requestId}/events");

        AssertRoute<노드스티커상점Controller>("api/v1/community/node-sticker-store");
        AssertHttpMethod<노드스티커상점Controller>(nameof(노드스티커상점Controller.목록조회), "GET", "items");
        AssertHttpMethod<노드스티커상점Controller>(nameof(노드스티커상점Controller.상세조회), "GET", "items/{itemKey}");
        AssertHttpMethod<노드스티커상점Controller>(nameof(노드스티커상점Controller.내사용권조회), "GET", "entitlements/me");
        AssertHttpMethod<노드스티커상점Controller>(nameof(노드스티커상점Controller.모의결제확인), "POST", "fake-pg/confirm");
    }

    [Fact]
    public void Admin_ledger_mutation_endpoints_require_admin_policy()
    {
        AssertAuthorizePolicy<배차대기Controller>("서버관리자전용");
        AssertAuthorizePolicy<운송이벤트Controller>("서버관리자전용");
        AssertAuthorizePolicy<운송진행관리Controller>("서버관리자전용");
    }

    [Fact]
    public void Server_ledger_event_endpoint_requires_logistics_operator_policy()
    {
        AssertAuthorizePolicy<운송원장Controller>("물류운영사용자전용");
    }

    private static void AssertRoute<TController>(string expectedTemplate)
    {
        var attribute = typeof(TController).GetCustomAttribute<RouteAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal(expectedTemplate, attribute!.Template);
    }

    private static void AssertHttpMethod<TController>(string methodName, string verb, string? expectedTemplate)
    {
        var method = typeof(TController).GetMethod(methodName);
        Assert.NotNull(method);

        var attribute = method!
            .GetCustomAttributes<HttpMethodAttribute>()
            .SingleOrDefault(x => x.HttpMethods.Contains(verb, StringComparer.OrdinalIgnoreCase));

        Assert.NotNull(attribute);
        Assert.Equal(expectedTemplate, attribute!.Template);
    }

    private static void AssertAuthorizePolicy<TController>(string expectedPolicy)
    {
        var attribute = typeof(TController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal(expectedPolicy, attribute!.Policy);
    }
}
