using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Unity.Exhibition
{
    public static class MarketProductSeedbedConnectionDepthCodes
    {
        public const string ProductionToMarket = "ProductionToMarket";
        public const string MarketBusinessOnly = "MarketBusinessOnly";
    }

    public static class MarketProductSeedbedCapabilityCodes
    {
        public const string PublicProductRead = "PublicProductRead";
        public const string ProductDetailRead = "ProductDetailRead";
        public const string NonBindingOrderIntent = "NonBindingOrderIntent";
        public const string NonBindingOrderIntentRead = "NonBindingOrderIntentRead";
    }

    public static class MarketProductBusinessApiImplementationCodes
    {
        public const string ExistingUnityAdapter = "ExistingUnityAdapter";
        public const string ContractPrepared = "ContractPrepared";
    }

    public static class MarketProductBusinessApiRoutes
    {
        public const string PublicProducts =
            "api/v1/orderer/mart/products?판매가능만=false&page=1&pageSize=50";
        public const string ProductDetail = "api/v1/orderer/mart/products/{productId}";
        public const string OrderRequests = "api/v1/orderer/mart/order-requests";
        public const string OrderRequestDetail =
            "api/v1/orderer/mart/order-requests/{orderRequestId}";
        public const string OrderRequestQuantity =
            "api/v1/orderer/mart/order-requests/{orderRequestId}/quantity";
        public const string OrderRequestWithdrawal =
            "api/v1/orderer/mart/order-requests/{orderRequestId}/withdrawal";
    }

    public sealed class MarketProductSeedbedLinkDescriptor
    {
        public string ProductStableId { get; set; } = string.Empty;
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string ConnectionDepthCode { get; set; } = string.Empty;
        public string[] SeedbedObjectStableIds { get; set; } = Array.Empty<string>();
        public string[] RuleStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class MarketProductSeedbedItemSnapshot
    {
        public string ProductStableId { get; set; } = string.Empty;
        public long? OperationalProductId { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string SaleUnit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public int ProjectedSaleAvailability { get; set; }
        public string ProjectedQuantityUnit { get; set; } = string.Empty;
        public bool IsServerSaleAvailable { get; set; }
        public string RuntimeModeCode { get; set; } = string.Empty;
        public string ConnectionDepthCode { get; set; } = string.Empty;
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public string[] SeedbedObjectStableIds { get; set; } = Array.Empty<string>();
        public string[] RuleStableIds { get; set; } = Array.Empty<string>();
        public string SourceName { get; set; } = string.Empty;
        public string SourceHref { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public DateTimeOffset EvidenceAsOf { get; set; }
        public string QuantityDisclosure { get; set; } = string.Empty;
        public string[] Limitations { get; set; } = Array.Empty<string>();

        public bool HasProductionRuleConnection
            => ConnectionDepthCode == MarketProductSeedbedConnectionDepthCodes.ProductionToMarket;

        public bool CanRequestOperationalOrderIntent
            => RuntimeModeCode == DataRuntimeMode.Operational.ToString()
                && OperationalProductId.HasValue
                && IsServerSaleAvailable;
    }

    public sealed class MarketProductSeedbedSnapshot
    {
        public string MarketStableId { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public string RuntimeModeCode { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public string QuantityDisclosure { get; set; } = string.Empty;
        public MarketProductSeedbedItemSnapshot[] Products { get; set; }
            = Array.Empty<MarketProductSeedbedItemSnapshot>();
    }

    public sealed class MarketProductSeedbedProjector
    {
        private readonly 도심마트공개상품DataSnapshotValidator validator;
        private readonly IReadOnlyDictionary<string, MarketProductSeedbedLinkDescriptor> links;

        public MarketProductSeedbedProjector()
            : this(
                new 도심마트공개상품DataSnapshotValidator(),
                MarketProductSeedbedLinkCatalog.Create())
        {
        }

        public MarketProductSeedbedProjector(
            도심마트공개상품DataSnapshotValidator snapshotValidator,
            IEnumerable<MarketProductSeedbedLinkDescriptor> linkCatalog)
        {
            validator = snapshotValidator ?? throw new ArgumentNullException(nameof(snapshotValidator));
            if (linkCatalog == null) throw new ArgumentNullException(nameof(linkCatalog));
            links = linkCatalog.ToDictionary(value => value.ProductStableId, StringComparer.Ordinal);
        }

        public MarketProductSeedbedSnapshot Project(도심마트공개상품DataSnapshot source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var errors = validator.Validate(source);
            if (errors.Length > 0)
            {
                throw new InvalidOperationException(errors[0]);
            }

            return new MarketProductSeedbedSnapshot
            {
                MarketStableId = source.StableId,
                DataRevision = source.DataRevision,
                MarketName = source.마트명,
                RuntimeModeCode = source.Mode.ToString(),
                GeneratedAt = source.GeneratedAt,
                QuantityDisclosure = source.QuantityDisclosure,
                Products = source.상품목록.Select(product => ProjectProduct(source, product)).ToArray(),
            };
        }

        private MarketProductSeedbedItemSnapshot ProjectProduct(
            도심마트공개상품DataSnapshot source,
            도심마트공개상품Data product)
        {
            links.TryGetValue(product.StableId, out var link);
            var operationalId = ParseOperationalProductId(product.StableId);
            var capabilities = new List<string> { MarketProductSeedbedCapabilityCodes.PublicProductRead };
            if (operationalId.HasValue)
            {
                capabilities.Add(MarketProductSeedbedCapabilityCodes.ProductDetailRead);
                capabilities.Add(MarketProductSeedbedCapabilityCodes.NonBindingOrderIntent);
            }

            return new MarketProductSeedbedItemSnapshot
            {
                ProductStableId = product.StableId,
                OperationalProductId = operationalId,
                CanonicalProductStableId = link?.CanonicalProductStableId ?? string.Empty,
                DisplayName = product.상품명,
                SaleUnit = product.판매단위,
                Price = product.판매가,
                CurrencyCode = product.통화Code,
                ProjectedSaleAvailability = product.투영판매가능수량,
                ProjectedQuantityUnit = product.투영수량단위,
                IsServerSaleAvailable = product.서버판매가능여부,
                RuntimeModeCode = source.Mode.ToString(),
                ConnectionDepthCode = link?.ConnectionDepthCode
                    ?? MarketProductSeedbedConnectionDepthCodes.MarketBusinessOnly,
                CapabilityCodes = capabilities.ToArray(),
                SeedbedObjectStableIds = link?.SeedbedObjectStableIds
                    ?? MarketProductSeedbedLinkCatalog.MarketObjectStableIds,
                RuleStableIds = link?.RuleStableIds ?? Array.Empty<string>(),
                SourceName = product.SourceName,
                SourceHref = product.SourceHref,
                SourceRevision = product.SourceRevision,
                EvidenceAsOf = product.EvidenceAsOf,
                QuantityDisclosure = source.QuantityDisclosure,
                Limitations = link?.Limitations ?? new[]
                {
                    "공개 상품과 생산 품목의 기준 고유 식별자 연결이 없어 마트 업무만 표현합니다.",
                },
            };
        }

        private static long? ParseOperationalProductId(string stableId)
        {
            const string prefix = "mart-product:";
            if (!stableId.StartsWith(prefix, StringComparison.Ordinal)) return null;
            return long.TryParse(stableId.Substring(prefix.Length), out var value) && value > 0
                ? value
                : (long?)null;
        }
    }

    public static class MarketProductSeedbedLinkCatalog
    {
        public static readonly string[] MarketObjectStableIds =
        {
            "seedbed-object:city.urban-market-building.a",
            "seedbed-object:city.operator-inventory-shelf.a",
            "seedbed-object:town.grouping-cart-table.a",
        };

        public static MarketProductSeedbedLinkDescriptor[] Create()
            => new[]
            {
                new MarketProductSeedbedLinkDescriptor
                {
                    ProductStableId = "product:potato-20kg",
                    CanonicalProductStableId = "product:potato",
                    ConnectionDepthCode = MarketProductSeedbedConnectionDepthCodes.ProductionToMarket,
                    SeedbedObjectStableIds = new[]
                    {
                        "seedbed-object:farm.potato-plant-visual.a",
                        "seedbed-object:farm.potato-harvest-box.a",
                        "seedbed-object:city.urban-market-building.a",
                        "seedbed-object:city.operator-inventory-shelf.a",
                    },
                    RuleStableIds = new[] { "rule:potato-production.fixture.v1" },
                    Limitations = new[]
                    {
                        "생산 규칙은 감자 Fixture이며 마트 공개 수량과 같은 원장이라는 뜻은 아닙니다.",
                    },
                },
                MarketOnly(
                    "product:rice-10kg",
                    "쌀은 공개 상품·가격·판매 가능 수량만 연결됐고 생산 규칙은 아직 없습니다."),
                MarketOnly(
                    "product:onion-10kg",
                    "양파는 공개 상품·가격·판매 가능 수량만 연결됐고 생산 규칙은 아직 없습니다."),
            };

        private static MarketProductSeedbedLinkDescriptor MarketOnly(
            string productStableId,
            string limitation)
            => new MarketProductSeedbedLinkDescriptor
            {
                ProductStableId = productStableId,
                ConnectionDepthCode = MarketProductSeedbedConnectionDepthCodes.MarketBusinessOnly,
                SeedbedObjectStableIds = MarketObjectStableIds.ToArray(),
                Limitations = new[] { limitation },
            };
    }

    public sealed class MarketProductBusinessApiDescriptor
    {
        public string CapabilityCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string RouteTemplate { get; set; } = string.Empty;
        public bool RequiresAuthentication { get; set; }
        public bool RequiresPrivacyConsentEvidence { get; set; }
        public bool MutatesInventory { get; set; }
        public bool CreatesPayment { get; set; }
        public string ServerResultMeaning { get; set; } = string.Empty;
        public string UnityImplementationCode { get; set; } = string.Empty;
    }

    public static class MarketProductBusinessApiCatalog
    {
        public static MarketProductBusinessApiDescriptor[] Create()
            => new[]
            {
                Read(MarketProductSeedbedCapabilityCodes.PublicProductRead,
                    "상품 목록 조회", "GET", MarketProductBusinessApiRoutes.PublicProducts,
                    false, MarketProductBusinessApiImplementationCodes.ExistingUnityAdapter),
                Read(MarketProductSeedbedCapabilityCodes.ProductDetailRead,
                    "상품 상세 조회", "GET", MarketProductBusinessApiRoutes.ProductDetail,
                    false, MarketProductBusinessApiImplementationCodes.ContractPrepared),
                Read(MarketProductSeedbedCapabilityCodes.NonBindingOrderIntentRead,
                    "내 주문 의향 목록 조회", "GET", MarketProductBusinessApiRoutes.OrderRequests,
                    true, MarketProductBusinessApiImplementationCodes.ContractPrepared),
                Read(MarketProductSeedbedCapabilityCodes.NonBindingOrderIntentRead,
                    "내 주문 의향 상세 재조회", "GET", MarketProductBusinessApiRoutes.OrderRequestDetail,
                    true, MarketProductBusinessApiImplementationCodes.ContractPrepared),
                Command(
                    "비구속 주문 의향 등록",
                    "POST",
                    MarketProductBusinessApiRoutes.OrderRequests,
                    true,
                    "비구속 구매 의향을 기록하지만 재고 예약·결제·주문 확정은 만들지 않습니다."),
                Command(
                    "비구속 주문 의향 수량 변경",
                    "PUT",
                    MarketProductBusinessApiRoutes.OrderRequestQuantity,
                    false,
                    "제출 상태의 본인 주문 의향 수량만 변경하며 재고를 예약하지 않습니다."),
                Command(
                    "비구속 주문 의향 철회",
                    "POST",
                    MarketProductBusinessApiRoutes.OrderRequestWithdrawal,
                    false,
                    "본인 주문 의향을 철회 상태로 변경하며 결제·출고 취소를 대신하지 않습니다."),
            };

        private static MarketProductBusinessApiDescriptor Read(
            string capabilityCode,
            string displayName,
            string method,
            string route,
            bool auth,
            string implementation)
            => new MarketProductBusinessApiDescriptor
            {
                CapabilityCode = capabilityCode,
                DisplayName = displayName,
                HttpMethod = method,
                RouteTemplate = route,
                RequiresAuthentication = auth,
                ServerResultMeaning = "서버가 권한과 공개 범위에 맞게 만든 관점별 조회 결과입니다.",
                UnityImplementationCode = implementation,
            };

        private static MarketProductBusinessApiDescriptor Command(
            string displayName,
            string method,
            string route,
            bool consent,
            string meaning)
            => new MarketProductBusinessApiDescriptor
            {
                CapabilityCode = MarketProductSeedbedCapabilityCodes.NonBindingOrderIntent,
                DisplayName = displayName,
                HttpMethod = method,
                RouteTemplate = route,
                RequiresAuthentication = true,
                RequiresPrivacyConsentEvidence = consent,
                MutatesInventory = false,
                CreatesPayment = false,
                ServerResultMeaning = meaning,
                UnityImplementationCode = MarketProductBusinessApiImplementationCodes.ContractPrepared,
            };
    }

    public static class MarketOrderIntentPhaseCodes
    {
        public const string PreviewReady = "PreviewReady";
        public const string AwaitingServerRecord = "AwaitingServerRecord";
        public const string AwaitingServerRefresh = "AwaitingServerRefresh";
        public const string Reconciled = "Reconciled";
    }

    public sealed class MarketOrderIntentDraft
    {
        public bool IsAuthenticated { get; set; }
        public Guid PrivacyConsentEvidenceId { get; set; }
        public string ApplicationSourceCode { get; set; } = string.Empty;
        public Guid ClientRequestId { get; set; }
        public int Quantity { get; set; }
        public bool NonBindingOrderRequestConfirmed { get; set; }
        public string NoticeVersion { get; set; } = string.Empty;
    }

    public sealed class MarketOrderIntentCommandApiModel
    {
        public Guid 신청개인정보동의증적Id { get; set; }
        public string 신청출처Code { get; set; } = string.Empty;
        public Guid 클라이언트요청Id { get; set; }
        public long 공개상품Id { get; set; }
        public int 수량 { get; set; }
        public bool 비구속주문요청확인 { get; set; }
        public string 안내버전 { get; set; } = string.Empty;
    }

    public sealed class MarketOrderIntentResponseApiModel
    {
        public Guid 주문요청Id { get; set; }
        public long 공개상품Id { get; set; }
        public string 상품명 { get; set; } = string.Empty;
        public string 판매단위 { get; set; } = string.Empty;
        public decimal 단가 { get; set; }
        public int 수량 { get; set; }
        public decimal 합계 { get; set; }
        public string 통화 { get; set; } = string.Empty;
        public int 제출시판매가능수량 { get; set; }
        public DateTimeOffset 재고기준시각Utc { get; set; }
        public string 상태코드 { get; set; } = string.Empty;
        public string 안내버전 { get; set; } = string.Empty;
        public DateTimeOffset 제출일시Utc { get; set; }
        public bool 재고예약됨 { get; set; }
        public bool 결제됨 { get; set; }
    }

    public sealed class MarketOrderIntentPreviewSnapshot
    {
        public string ProductStableId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ExpectedTotalPrice { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public int ProjectedSaleAvailability { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool CreatesInventoryReservation => false;
        public bool CreatesPayment => false;
        public bool CreatesConfirmedOrder => false;
    }

    public sealed class MarketOrderIntentSessionSnapshot
    {
        public string PhaseCode { get; set; } = string.Empty;
        public MarketProductSeedbedItemSnapshot Product { get; set; }
            = new MarketProductSeedbedItemSnapshot();
        public MarketOrderIntentPreviewSnapshot Preview { get; set; }
            = new MarketOrderIntentPreviewSnapshot();
        public MarketOrderIntentCommandApiModel Command { get; set; }
            = new MarketOrderIntentCommandApiModel();
        public MarketOrderIntentResponseApiModel? RecordedResponse { get; set; }
        public MarketOrderIntentResponseApiModel? RefreshedResponse { get; set; }
        public bool CanonicalStateMutatedByPresentation => false;
    }

    public sealed class MarketOrderIntentCoordinator
    {
        public const string CurrentNoticeVersion = "2026-07-20";

        public MarketOrderIntentSessionSnapshot CreatePreview(
            MarketProductSeedbedItemSnapshot product,
            MarketOrderIntentDraft draft)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (draft == null) throw new ArgumentNullException(nameof(draft));

            var blockers = new List<string>();
            if (!product.CanRequestOperationalOrderIntent)
                blockers.Add("OperationalProductRequired");
            if (!draft.IsAuthenticated)
                blockers.Add("AuthenticationRequired");
            if (draft.PrivacyConsentEvidenceId == Guid.Empty)
                blockers.Add("PrivacyConsentEvidenceRequired");
            if (string.IsNullOrWhiteSpace(draft.ApplicationSourceCode))
                blockers.Add("ApplicationSourceRequired");
            if (draft.ClientRequestId == Guid.Empty)
                blockers.Add("ClientRequestIdRequired");
            if (draft.Quantity < 1 || draft.Quantity > 100)
                blockers.Add("QuantityOutOfRange");
            else if (draft.Quantity > product.ProjectedSaleAvailability)
                blockers.Add("QuantityExceedsProjectedAvailability");
            if (!draft.NonBindingOrderRequestConfirmed
                || draft.NoticeVersion != CurrentNoticeVersion)
                blockers.Add("CurrentNonBindingNoticeConfirmationRequired");

            return new MarketOrderIntentSessionSnapshot
            {
                PhaseCode = MarketOrderIntentPhaseCodes.PreviewReady,
                Product = product,
                Preview = new MarketOrderIntentPreviewSnapshot
                {
                    ProductStableId = product.ProductStableId,
                    ProductName = product.DisplayName,
                    Quantity = draft.Quantity,
                    UnitPrice = product.Price,
                    ExpectedTotalPrice = product.Price * draft.Quantity,
                    CurrencyCode = product.CurrencyCode,
                    ProjectedSaleAvailability = product.ProjectedSaleAvailability,
                    BlockingReasonCodes = blockers.ToArray(),
                },
                Command = new MarketOrderIntentCommandApiModel
                {
                    신청개인정보동의증적Id = draft.PrivacyConsentEvidenceId,
                    신청출처Code = draft.ApplicationSourceCode,
                    클라이언트요청Id = draft.ClientRequestId,
                    공개상품Id = product.OperationalProductId ?? 0,
                    수량 = draft.Quantity,
                    비구속주문요청확인 = draft.NonBindingOrderRequestConfirmed,
                    안내버전 = draft.NoticeVersion,
                },
            };
        }

        public MarketOrderIntentSessionSnapshot RequestServerRecord(
            MarketOrderIntentSessionSnapshot session)
        {
            RequirePhase(session, MarketOrderIntentPhaseCodes.PreviewReady);
            if (session.Preview.BlockingReasonCodes.Length > 0)
                throw new InvalidOperationException("MarketOrderIntentPreviewBlocked");

            session.PhaseCode = MarketOrderIntentPhaseCodes.AwaitingServerRecord;
            return session;
        }

        public MarketOrderIntentSessionSnapshot AcceptServerRecord(
            MarketOrderIntentSessionSnapshot session,
            MarketOrderIntentResponseApiModel response)
        {
            RequirePhase(session, MarketOrderIntentPhaseCodes.AwaitingServerRecord);
            ValidateResponse(session, response);
            session.RecordedResponse = response;
            session.PhaseCode = MarketOrderIntentPhaseCodes.AwaitingServerRefresh;
            return session;
        }

        public MarketOrderIntentSessionSnapshot ApplyServerRefresh(
            MarketOrderIntentSessionSnapshot session,
            MarketOrderIntentResponseApiModel refreshed)
        {
            RequirePhase(session, MarketOrderIntentPhaseCodes.AwaitingServerRefresh);
            ValidateResponse(session, refreshed);
            if (session.RecordedResponse == null
                || refreshed.주문요청Id != session.RecordedResponse.주문요청Id
                || refreshed.상태코드 != session.RecordedResponse.상태코드
                || refreshed.제출일시Utc != session.RecordedResponse.제출일시Utc)
            {
                throw new InvalidOperationException("MarketOrderIntentRefreshMismatch");
            }

            session.RefreshedResponse = refreshed;
            session.PhaseCode = MarketOrderIntentPhaseCodes.Reconciled;
            return session;
        }

        private static void ValidateResponse(
            MarketOrderIntentSessionSnapshot session,
            MarketOrderIntentResponseApiModel response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (response.주문요청Id == Guid.Empty
                || response.공개상품Id != session.Command.공개상품Id
                || response.수량 != session.Command.수량
                || response.상품명 != session.Product.DisplayName
                || response.판매단위 != session.Product.SaleUnit
                || response.단가 != session.Product.Price
                || response.합계 != session.Preview.ExpectedTotalPrice
                || response.통화 != session.Product.CurrencyCode
                || response.상태코드 != "Submitted"
                || response.안내버전 != CurrentNoticeVersion
                || response.재고기준시각Utc == default
                || response.제출일시Utc == default)
            {
                throw new InvalidOperationException("MarketOrderIntentServerResponseInvalid");
            }

            if (response.재고예약됨 || response.결제됨)
            {
                throw new InvalidOperationException("MarketOrderIntentUnexpectedOperationalEffect");
            }
        }

        private static void RequirePhase(MarketOrderIntentSessionSnapshot session, string expected)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (session.PhaseCode != expected)
                throw new InvalidOperationException("MarketOrderIntentPhaseInvalid");
        }
    }

    public interface IMarketOrderIntentApiClient
    {
        Task<MarketOrderIntentResponseApiModel> 등록Async(
            MarketOrderIntentCommandApiModel command,
            CancellationToken cancellationToken = default);

        Task<MarketOrderIntentResponseApiModel> 상세조회Async(
            Guid orderRequestId,
            CancellationToken cancellationToken = default);
    }

    public sealed class MarketOrderIntentServerUseCase
    {
        private readonly IMarketOrderIntentApiClient apiClient;
        private readonly MarketOrderIntentCoordinator coordinator;

        public MarketOrderIntentServerUseCase(
            IMarketOrderIntentApiClient client,
            MarketOrderIntentCoordinator flowCoordinator)
        {
            apiClient = client ?? throw new ArgumentNullException(nameof(client));
            coordinator = flowCoordinator ?? throw new ArgumentNullException(nameof(flowCoordinator));
        }

        public async Task<MarketOrderIntentSessionSnapshot> 기록후재조회Async(
            MarketOrderIntentSessionSnapshot session,
            CancellationToken cancellationToken = default)
        {
            coordinator.RequestServerRecord(session);
            var recorded = await apiClient.등록Async(session.Command, cancellationToken)
                .ConfigureAwait(false);
            coordinator.AcceptServerRecord(session, recorded);

            var refreshed = await apiClient.상세조회Async(recorded.주문요청Id, cancellationToken)
                .ConfigureAwait(false);
            return coordinator.ApplyServerRefresh(session, refreshed);
        }
    }
}
