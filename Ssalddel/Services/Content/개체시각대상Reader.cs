using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Mart;
using Ssalddel.Application.Warehouse;
using Ssalddel.Application.WorldProjection;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Services.Content;

public sealed record 개체시각대상ReadResult(string Diagnostic, 개체시각대상Dto? Target = null);
public interface I개체시각대상Reader
{
    Task<개체시각대상ReadResult> ReadAsync(개체시각대상Query query, CancellationToken cancellationToken);
}

/// <summary>등록된 기존 조회만 사용한다. EF 형식명/테이블명으로 임의 원장을 조회하지 않는다.</summary>
[SsalddelCodeMetadata(개체시각대응Codes.Feature, SsalddelCodeLayer.Application,
    "기존 권한별 농장·창고·마트·공통 품목 조회를 시각 검토 대상으로 압축한다.",
    StepKey = "source", FlowOrder = 15, ExecutionStage = SsalddelCodeExecutionStage.Query,
    Effects = SsalddelCodeEffect.PersistentRead,
    ReadsFrom = SsalddelCodeDataScope.OperationalState | SsalddelCodeDataScope.SharedPublicData,
    Boundary = "주소·연락처·거래·센서관측 원문을 복사하지 않고 관측행을 객체로 승격하지 않는다.")]
public sealed class 개체시각대상Reader(
    IFarmProducerPerspectiveUseCase farm,
    I창고WorldSnapshot조회UseCase warehouse,
    I마트공개상품조회UseCase mart,
    I공통식품품목Identity조회UseCase food,
    ICurrentUserAccessor user) : I개체시각대상Reader
{
    public static bool IsSupported(string kind) => kind is
        "farm" or "farm.plot" or "farm.cultivation" or "warehouse.inventory" or "mart.product" or "food.product";

    public async Task<개체시각대상ReadResult> ReadAsync(개체시각대상Query q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserId)) return new("Unauthorized");
        if (q is null || !IsSupported(q.Kind)) return new("UnsupportedKind");
        if (string.IsNullOrWhiteSpace(q.StableId) || q.StableId.Length > 160 ||
            q.Purpose is not ("Summary" or "Inventory" or "Growing" or "Harvested" or "Packaged"))
            return new("InvalidTarget");
        var privateScope = "viewer:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(user.UserId)));
        if (q.Kind.StartsWith("farm", StringComparison.Ordinal))
        {
            var result = await farm.QueryAsync(ct);
            if (result.IsFailed) return new("SourceAccessOrQueryFailed");
            var farms = result.Value.Farms;
            if (q.Kind == "farm")
            {
                var items = farms.Where(x => x.StableId == q.StableId).ToArray();
                if (items.Length > 1) return new("SourceConflict");
                var item = items.SingleOrDefault();
                return item is null ? new("NotFoundOrNotAuthorized") : Target("FarmProducer", privateScope,
                    item.Revision.ToString(CultureInfo.InvariantCulture), item.StatusCode, "Building", item.FarmName);
            }
            if (q.Kind == "farm.plot")
            {
                var items = farms.SelectMany(x => x.Plots).Where(x => x.StableId == q.StableId).ToArray();
                if (items.Length > 1) return new("SourceConflict");
                var item = items.SingleOrDefault();
                return item is null ? new("NotFoundOrNotAuthorized") : Target("FarmProducer", privateScope,
                    item.Revision.ToString(CultureInfo.InvariantCulture), "Reference", "Surface", item.PlotName);
            }
            var crops = farms.SelectMany(x => x.Plots).SelectMany(x => x.Cultivations)
                .Where(x => x.StableId == q.StableId).ToArray();
            if (crops.Length > 1) return new("SourceConflict");
            var crop = crops.SingleOrDefault();
            return crop is null ? new("NotFoundOrNotAuthorized") : Target("FarmProducer", privateScope,
                crop.Revision.ToString(CultureInfo.InvariantCulture), crop.GrowthStatusCode, "Crop", crop.CropName);
        }
        if (q.Kind == "warehouse.inventory")
        {
            if (q.WarehouseId is not > 0) return new("WarehouseIdRequired");
            var result = await warehouse.조회Async(q.WarehouseId, ct);
            if (result.IsFailed) return new("SourceAccessOrQueryFailed");
            var items = result.Value.InventoryItems.Where(x => x.StableId == q.StableId).ToArray();
            if (items.Length > 1) return new("SourceConflict");
            var item = items.SingleOrDefault();
            // 기존 조회의 50개 창 밖은 부재/권한 여부를 추정하지 않는다.
            return item is null ? new("NotFoundOrOutsideAuthorizedWindow") : Target("WarehouseInventory",
                privateScope + ":" + q.WarehouseId.Value.ToString(CultureInfo.InvariantCulture), result.Value.Revision,
                item.Status, "Cargo", item.ProductName);
        }
        if (q.Kind == "mart.product")
        {
            if (!long.TryParse(q.StableId, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0 ||
                id.ToString(CultureInfo.InvariantCulture) != q.StableId) return new("InvalidTarget");
            var result = await mart.상세Async(id, ct);
            if (result.IsFailed) return new("SourceAccessOrQueryFailed");
            var item = result.Value;
            return Target("MartPublicProduct", "Public", item.수정일시Utc.Ticks.ToString(CultureInfo.InvariantCulture),
                item.판매가능여부 ? "Available" : "Unavailable", "Product", item.상품명);
        }
        var product = await food.단건조회Async(q.StableId, ct);
        return product is null ? new("NotFound") : Target("CommonFoodIdentity", "Public", product.Revision,
            "Reference", "Product", product.DisplayName);

        개체시각대상ReadResult Target(string source, string scope, string revision, string state, string representation, string name)
            => new("Found", new(q.Kind, q.StableId, source, scope, revision, state, q.Purpose, representation, name));
    }
}
