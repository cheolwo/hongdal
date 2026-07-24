using System.Reflection;

namespace Ssalddel.ApiMetadata;

/// <summary>
/// 제품 버전이 아니라 API가 제공하는 안정적인 업무 능력을 표시합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class SsalddelApiCapabilityAttribute : Attribute
{
    public SsalddelApiCapabilityAttribute(SsalddelCapability capability)
    {
        Capability = capability;
    }

    public SsalddelCapability Capability { get; }

    public string CapabilityLabel => SsalddelCapabilityLabels.GetLabel(Capability);
}

/// <summary>
/// 권한을 대신하지 않고 API를 사용하는 업무 역할을 설명합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class SsalddelApiAudienceAttribute : Attribute
{
    public SsalddelApiAudienceAttribute(SsalddelActor actor)
    {
        Actor = actor;
    }

    public SsalddelActor Actor { get; }

    public string ActorLabel => SsalddelActorLabels.GetLabel(Actor);
}

/// <summary>
/// API가 사용자 관점에서 수행하는 업무 동작을 표시합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class SsalddelApiOperationAttribute : Attribute
{
    public SsalddelApiOperationAttribute(SsalddelOperation operation)
    {
        Operation = operation;
    }

    public SsalddelOperation Operation { get; }

    public string OperationLabel => SsalddelOperationLabels.GetLabel(Operation);
}

/// <summary>
/// 실행 노출을 제어하는 Feature Key를 제품 버전 이력과 분리해 표시합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class SsalddelApiFeatureAttribute : Attribute
{
    public SsalddelApiFeatureAttribute(string featureKey)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
        {
            throw new ArgumentException("Feature Key는 비워 둘 수 없습니다.", nameof(featureKey));
        }

        FeatureKey = featureKey.Trim();
    }

    public string FeatureKey { get; }
}

/// <summary>
/// 현재 책임 분류가 아니라 API가 처음 도입된 제품 시점을 기록합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class SsalddelApiIntroducedInAttribute : SsalddelApiVersionAttribute
{
    public SsalddelApiIntroducedInAttribute(SsalddelProductVersion version)
        : base(version)
    {
    }
}

public enum SsalddelCapability
{
    CommunityInformationDiscovery = 100,
    RelationshipFormation = 200,
    CommunityLedger = 300,
    GroupPurchaseDemand = 400,
    OrderParticipation = 500,
    TransportRequest = 600,
    TradePreparation = 700,
    Dispatch = 800,
    TransportExecution = 900,
    WarehouseInbound = 1000,
    InventoryManagement = 1100,
    WarehouseFulfillment = 1200,
    WorkActivitySignal = 1300
}

public enum SsalddelOperation
{
    Browse = 100,
    Request = 200,
    Decide = 300,
    Execute = 400,
    Record = 500,
    Manage = 600
}

public static class SsalddelCapabilityLabels
{
    public static string GetLabel(SsalddelCapability capability)
    {
        return capability switch
        {
            SsalddelCapability.CommunityInformationDiscovery => "커뮤니티 정보 둘러보기",
            SsalddelCapability.RelationshipFormation => "인연 형성",
            SsalddelCapability.CommunityLedger => "공동 원장",
            SsalddelCapability.GroupPurchaseDemand => "공동구매 수요·모집",
            SsalddelCapability.OrderParticipation => "주문 참여",
            SsalddelCapability.TransportRequest => "운송 의뢰",
            SsalddelCapability.TradePreparation => "무역 준비",
            SsalddelCapability.Dispatch => "배차",
            SsalddelCapability.TransportExecution => "운송 실행",
            SsalddelCapability.WarehouseInbound => "창고 입고",
            SsalddelCapability.InventoryManagement => "재고 관리",
            SsalddelCapability.WarehouseFulfillment => "창고 이행",
            SsalddelCapability.WorkActivitySignal => "업무 활동 신호",
            _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, "알 수 없는 업무 영역입니다.")
        };
    }
}

public static class SsalddelOperationLabels
{
    public static string GetLabel(SsalddelOperation operation)
    {
        return operation switch
        {
            SsalddelOperation.Browse => "둘러보기",
            SsalddelOperation.Request => "요청하기",
            SsalddelOperation.Decide => "판단하기",
            SsalddelOperation.Execute => "실행하기",
            SsalddelOperation.Record => "기록하기",
            SsalddelOperation.Manage => "관리하기",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "알 수 없는 업무 동작입니다.")
        };
    }
}

public sealed record SsalddelApiClassificationDescriptor(
    Type ComponentType,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> Audiences,
    IReadOnlyList<string> Operations,
    string? Workflow,
    string? FeatureKey,
    string? IntroducedIn);

public static class SsalddelApiClassificationReader
{
    public static SsalddelApiClassificationDescriptor Read(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        var capabilities = componentType
            .GetCustomAttributes<SsalddelApiCapabilityAttribute>(inherit: true)
            .Select(attribute => attribute.CapabilityLabel)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var audiences = componentType
            .GetCustomAttributes<SsalddelApiAudienceAttribute>(inherit: true)
            .Select(attribute => attribute.ActorLabel)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var operations = componentType
            .GetCustomAttributes<SsalddelApiOperationAttribute>(inherit: true)
            .Select(attribute => attribute.OperationLabel)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var workflow = componentType
            .GetCustomAttributes<SsalddelApiWorkflowAttribute>(inherit: true)
            .Select(attribute => attribute.WorkflowLabel)
            .FirstOrDefault();
        var featureKey = componentType
            .GetCustomAttribute<SsalddelApiFeatureAttribute>(inherit: true)?
            .FeatureKey;
        var introducedIn = componentType
            .GetCustomAttribute<SsalddelApiIntroducedInAttribute>(inherit: true)?
            .VersionLabel;

        return new SsalddelApiClassificationDescriptor(
            componentType,
            capabilities,
            audiences,
            operations,
            workflow,
            featureKey,
            introducedIn);
    }
}
