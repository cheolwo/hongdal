using System.Globalization;
using Ssalddel.Contracts.Common.Inventory;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I출고인계준비페이지Service
{
    Task<출고인계준비목록페이지응답> 목록조회Async(출고인계준비목록조회요청 request, CancellationToken cancellationToken = default);
    Task<출고인계준비상세응답?> 상세조회Async(long inboundItemId, CancellationToken cancellationToken = default);
    Task<출고인계준비결과응답?> 완료Async(long inboundItemId, 출고인계준비완료요청 request, CancellationToken cancellationToken = default);
}

public sealed class 출고인계준비페이지Service(ISsalddelJsonApiClient client) : I출고인계준비페이지Service
{
    private const string BasePath = "api/v1/warehouse-operations/outbound-handoff-tasks";
    public async Task<출고인계준비목록페이지응답> 목록조회Async(출고인계준비목록조회요청 request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request); var page=Math.Max(0,request.Page); var pageSize=Math.Clamp(request.PageSize,1,50);
        var query=new List<string>{$"status={Uri.EscapeDataString(출고인계준비조회상태코드.Normalize(request.Status))}",$"page={page.ToString(CultureInfo.InvariantCulture)}",$"pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}"};
        if(request.WarehouseId is>0)query.Add($"warehouseId={request.WarehouseId.Value.ToString(CultureInfo.InvariantCulture)}");
        if(!string.IsNullOrWhiteSpace(request.Search))query.Add($"search={Uri.EscapeDataString(request.Search.Trim())}");
        return await client.GetAsync<출고인계준비목록페이지응답>($"{BasePath}?{string.Join("&",query)}","출고 인계 준비 목록 조회",allowNotFound:false,cancellationToken:cancellationToken)
            ??new 출고인계준비목록페이지응답{Page=page,PageSize=pageSize};
    }
    public Task<출고인계준비상세응답?> 상세조회Async(long inboundItemId,CancellationToken cancellationToken=default)
        =>client.GetAsync<출고인계준비상세응답>($"{BasePath}/{inboundItemId.ToString(CultureInfo.InvariantCulture)}","출고 인계 준비 상세 조회",cancellationToken:cancellationToken);
    public Task<출고인계준비결과응답?> 완료Async(long inboundItemId,출고인계준비완료요청 request,CancellationToken cancellationToken=default)
        =>client.SendAsync<출고인계준비완료요청,출고인계준비결과응답>(HttpMethod.Post,$"{BasePath}/{inboundItemId.ToString(CultureInfo.InvariantCulture)}/complete",request,"출고 인계 준비 완료",cancellationToken:cancellationToken);
}
