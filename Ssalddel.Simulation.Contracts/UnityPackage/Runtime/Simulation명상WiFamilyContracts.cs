using System;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// 명상은 구체 WI를 대신 실행하지 않는 상위 분류다.
    /// 실제 Preview, Confirm, Task, Effect와 Revision은 자식 WI가 소유한다.
    /// </summary>
    public static class Simulation명상WiFamilyCodes
    {
        public const string FamilyStableId = "wi-family:meditation";
        public const string CatalogRevision = "meditation-wi-family-catalog.r1";
        public const string RuleRevision = "meditation-wi-family.r1";
        public const string MetadataOnly = "MetadataOnly";
        public const string AfterActionRecord = "AfterActionRecord";
        public const string Bound = "Bound";
        public const string NotApplicable = "NotApplicable";
        public const string PlayerAction = "PlayerAction";
        public const string NpcOrDelegatedOnly = "NpcOrDelegatedOnly";
        public const string NoMeaningfulPlayerAction =
            "NoMeaningfulPlayerAction";
    }

    public sealed class Simulation명상WiFamilyDefinition
    {
        public string WiFamilyStableId { get; set; }
            = Simulation명상WiFamilyCodes.FamilyStableId;
        public string 한국어명 { get; set; } = "명상(정신 차림)";
        public string ExecutionKindCode { get; set; }
            = Simulation명상WiFamilyCodes.MetadataOnly;
        public string ApplicationPhaseCode { get; set; }
            = Simulation명상WiFamilyCodes.AfterActionRecord;
        public bool IsExecutable { get; set; }
        public bool OwnsPreviewConfirmTaskEffect { get; set; }
        public string RuleRevision { get; set; }
            = Simulation명상WiFamilyCodes.RuleRevision;
    }

    public sealed class Simulation명상WiFamilyBindingDefinition
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string[] 상위WiFamilyStableIds { get; set; }
            = Array.Empty<string>();
        public string 결속상태Code { get; set; }
            = Simulation명상WiFamilyCodes.NotApplicable;
        public string 행위분류Code { get; set; } = string.Empty;
        public string 사유Code { get; set; } = string.Empty;
        public string RuleRevision { get; set; }
            = Simulation명상WiFamilyCodes.RuleRevision;
    }

    public sealed class Simulation명상WiFamilyCatalogSnapshot
    {
        public string CatalogRevision { get; set; }
            = Simulation명상WiFamilyCodes.CatalogRevision;
        public string RuleRevision { get; set; }
            = Simulation명상WiFamilyCodes.RuleRevision;
        public Simulation명상WiFamilyDefinition[] Families { get; set; }
            = Array.Empty<Simulation명상WiFamilyDefinition>();
        public Simulation명상WiFamilyBindingDefinition[] Bindings { get; set; }
            = Array.Empty<Simulation명상WiFamilyBindingDefinition>();
    }
}
