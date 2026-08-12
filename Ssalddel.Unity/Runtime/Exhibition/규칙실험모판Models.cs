using System;
using System.Collections.Generic;
using System.Linq;

namespace Ssalddel.Unity.Exhibition
{
    public static class RuleSeedbedDomainCodes
    {
        public const string Production = "Production";
        public const string Consumption = "Consumption";
        public const string Transport = "Transport";
        public const string Warehouse = "Warehouse";
        public const string Presentation = "Presentation";
    }

    public static class RuleSeedbedModeCodes
    {
        public const string SimulationFixture = "SimulationFixture";
        public const string PresentationPreview = "PresentationPreview";
    }

    public static class RuleSeedbedPhaseCodes
    {
        public const string Ready = "Ready";
        public const string PreviewLoaded = "PreviewLoaded";
        public const string AwaitingCanonicalRefresh = "AwaitingCanonicalRefresh";
        public const string Reconciled = "Reconciled";
        public const string Failed = "Failed";
    }

    public sealed class RuleSeedbedScenarioDescriptor
    {
        public string ScenarioStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string RuleDomainCode { get; set; } = string.Empty;
        public string RuleStableId { get; set; } = string.Empty;
        public string ModeCode { get; set; } = string.Empty;
        public string[] SeedbedObjectStableIds { get; set; } = Array.Empty<string>();
        public string[] StepCodes { get; set; } = Array.Empty<string>();
        public bool CanConfirmSimulation { get; set; }
        public bool RequiresCanonicalRefresh { get; set; }
        public bool DoesNotCallOperationalApi { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class RuleSeedbedResourceValueSnapshot
    {
        public string LedgerStableId { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public sealed class RuleSeedbedCanonicalStateSnapshot
    {
        public string SnapshotStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public long WorldTick { get; set; }
        public bool IsServerAuthoritative { get; set; }
        public RuleSeedbedResourceValueSnapshot[] Values { get; set; } = Array.Empty<RuleSeedbedResourceValueSnapshot>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class RuleSeedbedEffectSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string StepCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public decimal BeforeValue { get; set; }
        public decimal DeltaValue { get; set; }
        public decimal AfterValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsCanonicalResourceEffect { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class RuleSeedbedPreviewSnapshot
    {
        public string PreviewStableId { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public long BasedOnRevision { get; set; }
        public string RuleStableId { get; set; } = string.Empty;
        public RuleSeedbedEffectSnapshot[] Effects { get; set; } = Array.Empty<RuleSeedbedEffectSnapshot>();
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class RuleSeedbedComparisonSnapshot
    {
        public string TargetStableId { get; set; } = string.Empty;
        public decimal BeforeValue { get; set; }
        public decimal PreviewAfterValue { get; set; }
        public decimal RefreshedValue { get; set; }
        public decimal ActualDeltaValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool MatchesPreview { get; set; }
    }

    public sealed class RuleSeedbedSessionSnapshot
    {
        public string PhaseCode { get; set; } = RuleSeedbedPhaseCodes.Ready;
        public RuleSeedbedScenarioDescriptor Scenario { get; set; } = new RuleSeedbedScenarioDescriptor();
        public RuleSeedbedCanonicalStateSnapshot Baseline { get; set; } = new RuleSeedbedCanonicalStateSnapshot();
        public RuleSeedbedPreviewSnapshot? Preview { get; set; }
        public RuleSeedbedCanonicalStateSnapshot? Refreshed { get; set; }
        public RuleSeedbedComparisonSnapshot[] Comparisons { get; set; } = Array.Empty<RuleSeedbedComparisonSnapshot>();
        public string SimulationCommandStableId { get; set; } = string.Empty;
        public string FailureReasonCode { get; set; } = string.Empty;

        // Unity 표현 계층이 기준 원장을 직접 변경하지 않았음을 명시하는 검증 표식이다.
        public bool CanonicalStateMutatedByPresentation => false;
    }

    public sealed class RuleSeedbedCoordinator
    {
        public RuleSeedbedSessionSnapshot Begin(
            RuleSeedbedScenarioDescriptor scenario,
            RuleSeedbedCanonicalStateSnapshot baseline)
        {
            ValidateScenario(scenario);
            ValidateCanonicalState(baseline, "기준 상태 사본");

            return new RuleSeedbedSessionSnapshot
            {
                PhaseCode = RuleSeedbedPhaseCodes.Ready,
                Scenario = scenario,
                Baseline = baseline,
            };
        }

        public RuleSeedbedSessionSnapshot LoadPreview(
            RuleSeedbedSessionSnapshot session,
            RuleSeedbedPreviewSnapshot preview)
        {
            EnsurePhase(session, RuleSeedbedPhaseCodes.Ready);
            ValidatePreview(session, preview);

            session.Preview = preview;
            session.PhaseCode = RuleSeedbedPhaseCodes.PreviewLoaded;
            return session;
        }

        public RuleSeedbedSessionSnapshot RequestSimulationConfirm(
            RuleSeedbedSessionSnapshot session,
            string simulationCommandStableId)
        {
            EnsurePhase(session, RuleSeedbedPhaseCodes.PreviewLoaded);

            if (!session.Scenario.CanConfirmSimulation
                || session.Scenario.ModeCode != RuleSeedbedModeCodes.SimulationFixture)
            {
                throw new InvalidOperationException("표현 미리보기 시나리오는 Simulation 확정을 요청할 수 없습니다.");
            }

            if ((session.Preview!.BlockingReasonCodes ?? Array.Empty<string>()).Length > 0)
            {
                throw new InvalidOperationException("차단 사유가 남은 미리보기는 확정할 수 없습니다.");
            }

            RequireText(simulationCommandStableId, nameof(simulationCommandStableId));
            session.SimulationCommandStableId = simulationCommandStableId;
            session.PhaseCode = RuleSeedbedPhaseCodes.AwaitingCanonicalRefresh;
            return session;
        }

        public RuleSeedbedSessionSnapshot ApplyCanonicalRefresh(
            RuleSeedbedSessionSnapshot session,
            RuleSeedbedCanonicalStateSnapshot refreshed)
        {
            EnsurePhase(session, RuleSeedbedPhaseCodes.AwaitingCanonicalRefresh);
            ValidateCanonicalState(refreshed, "갱신 상태 사본");

            if (!session.Scenario.RequiresCanonicalRefresh)
            {
                throw new InvalidOperationException("이 시나리오는 기준 원장 재조회를 요구하지 않습니다.");
            }

            if (refreshed.Revision <= session.Baseline.Revision)
            {
                throw new InvalidOperationException("갱신 상태 사본의 개정 번호가 기준 상태보다 커야 합니다.");
            }

            if (refreshed.WorldTick < session.Baseline.WorldTick)
            {
                throw new InvalidOperationException("갱신 상태 사본의 World Tick은 과거로 돌아갈 수 없습니다.");
            }

            var refreshedByTarget = refreshed.Values.ToDictionary(value => value.LedgerStableId, StringComparer.Ordinal);
            var comparisons = new List<RuleSeedbedComparisonSnapshot>();

            foreach (var effect in session.Preview!.Effects.Where(value => value.IsCanonicalResourceEffect))
            {
                if (!refreshedByTarget.TryGetValue(effect.TargetStableId, out var actual))
                {
                    throw new InvalidOperationException("갱신 상태 사본에 미리보기 대상 원장이 없습니다: " + effect.TargetStableId);
                }

                if (!string.Equals(actual.Unit, effect.Unit, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("미리보기와 갱신 상태 사본의 단위가 다릅니다: " + effect.TargetStableId);
                }

                var comparison = new RuleSeedbedComparisonSnapshot
                {
                    TargetStableId = effect.TargetStableId,
                    BeforeValue = effect.BeforeValue,
                    PreviewAfterValue = effect.AfterValue,
                    RefreshedValue = actual.Value,
                    ActualDeltaValue = actual.Value - effect.BeforeValue,
                    Unit = effect.Unit,
                    MatchesPreview = actual.Value == effect.AfterValue,
                };

                if (!comparison.MatchesPreview)
                {
                    throw new InvalidOperationException("서버 재조회 값이 미리보기 예상값과 다릅니다: " + effect.TargetStableId);
                }

                comparisons.Add(comparison);
            }

            session.Refreshed = refreshed;
            session.Comparisons = comparisons.ToArray();
            session.PhaseCode = RuleSeedbedPhaseCodes.Reconciled;
            return session;
        }

        public RuleSeedbedSessionSnapshot Reset(RuleSeedbedSessionSnapshot session)
        {
            RequireSession(session);
            session.PhaseCode = RuleSeedbedPhaseCodes.Ready;
            session.Preview = null;
            session.Refreshed = null;
            session.Comparisons = Array.Empty<RuleSeedbedComparisonSnapshot>();
            session.SimulationCommandStableId = string.Empty;
            session.FailureReasonCode = string.Empty;
            return session;
        }

        public RuleSeedbedSessionSnapshot Fail(RuleSeedbedSessionSnapshot session, string failureReasonCode)
        {
            RequireSession(session);
            RequireText(failureReasonCode, nameof(failureReasonCode));
            session.FailureReasonCode = failureReasonCode;
            session.PhaseCode = RuleSeedbedPhaseCodes.Failed;
            return session;
        }

        private static void ValidateScenario(RuleSeedbedScenarioDescriptor scenario)
        {
            if (scenario == null)
            {
                throw new ArgumentNullException(nameof(scenario));
            }

            RequireText(scenario.ScenarioStableId, nameof(scenario.ScenarioStableId));
            RequireText(scenario.DisplayName, nameof(scenario.DisplayName));
            RequireText(scenario.RuleDomainCode, nameof(scenario.RuleDomainCode));
            RequireText(scenario.RuleStableId, nameof(scenario.RuleStableId));
            RequireText(scenario.ModeCode, nameof(scenario.ModeCode));
            RequireIds(scenario.SeedbedObjectStableIds, nameof(scenario.SeedbedObjectStableIds));
            RequireIds(scenario.StepCodes, nameof(scenario.StepCodes));
            RequireIds(scenario.SourceStableIds, nameof(scenario.SourceStableIds));

            if (!scenario.DoesNotCallOperationalApi)
            {
                throw new InvalidOperationException("규칙 실험 모판은 실운영 API를 호출하지 않아야 합니다.");
            }

            var presentationOnly = scenario.ModeCode == RuleSeedbedModeCodes.PresentationPreview;
            if (presentationOnly && (scenario.CanConfirmSimulation || scenario.RequiresCanonicalRefresh))
            {
                throw new InvalidOperationException("표현 미리보기는 Simulation 확정이나 기준 원장 갱신을 요구할 수 없습니다.");
            }

            if (!presentationOnly && (!scenario.CanConfirmSimulation || !scenario.RequiresCanonicalRefresh))
            {
                throw new InvalidOperationException("Simulation 시나리오는 확정 후 기준 원장을 다시 조회해야 합니다.");
            }
        }

        private static void ValidateCanonicalState(RuleSeedbedCanonicalStateSnapshot state, string label)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            RequireText(state.SnapshotStableId, label + ".SnapshotStableId");
            RequireIds(state.SourceStableIds, label + ".SourceStableIds");

            if (!state.IsServerAuthoritative)
            {
                throw new InvalidOperationException(label + "는 서버가 제공한 상태여야 합니다.");
            }

            foreach (var value in state.Values ?? Array.Empty<RuleSeedbedResourceValueSnapshot>())
            {
                RequireText(value.LedgerStableId, label + ".LedgerStableId");
                RequireText(value.Unit, label + ".Unit");
            }
        }

        private static void ValidatePreview(RuleSeedbedSessionSnapshot session, RuleSeedbedPreviewSnapshot preview)
        {
            if (preview == null)
            {
                throw new ArgumentNullException(nameof(preview));
            }

            RequireText(preview.PreviewStableId, nameof(preview.PreviewStableId));
            RequireIds(preview.SourceStableIds, nameof(preview.SourceStableIds));

            if (preview.ScenarioStableId != session.Scenario.ScenarioStableId
                || preview.RuleStableId != session.Scenario.RuleStableId
                || preview.BasedOnRevision != session.Baseline.Revision)
            {
                throw new InvalidOperationException("미리보기의 시나리오, 규칙 또는 기준 개정 번호가 현재 세션과 다릅니다.");
            }

            var effects = preview.Effects ?? Array.Empty<RuleSeedbedEffectSnapshot>();
            if (effects.Length == 0)
            {
                throw new InvalidOperationException("규칙 미리보기에는 하나 이상의 효과가 필요합니다.");
            }

            var presentationOnly = session.Scenario.ModeCode == RuleSeedbedModeCodes.PresentationPreview;
            if (presentationOnly && effects.Any(value => value.IsCanonicalResourceEffect))
            {
                throw new InvalidOperationException("표현 규칙은 기준 원장 자원 효과를 만들 수 없습니다.");
            }

            if (!presentationOnly && effects.Any(value => !value.IsCanonicalResourceEffect))
            {
                throw new InvalidOperationException("업무 규칙 실험 효과는 서버 기준 원장과 대조할 수 있어야 합니다.");
            }

            var baselineByTarget = session.Baseline.Values
                .ToDictionary(value => value.LedgerStableId, StringComparer.Ordinal);

            foreach (var effect in effects)
            {
                RequireText(effect.EffectStableId, nameof(effect.EffectStableId));
                RequireText(effect.StepCode, nameof(effect.StepCode));
                RequireText(effect.TargetStableId, nameof(effect.TargetStableId));
                RequireText(effect.Unit, nameof(effect.Unit));
                RequireIds(effect.SourceStableIds, nameof(effect.SourceStableIds));

                if (!session.Scenario.StepCodes.Contains(effect.StepCode, StringComparer.Ordinal))
                {
                    throw new InvalidOperationException("시나리오에 없는 단계의 효과입니다: " + effect.StepCode);
                }

                if (effect.BeforeValue + effect.DeltaValue != effect.AfterValue)
                {
                    throw new InvalidOperationException("효과의 이전 값, 증감값, 이후 값이 일치하지 않습니다: " + effect.EffectStableId);
                }

                if (!presentationOnly)
                {
                    if (!baselineByTarget.TryGetValue(effect.TargetStableId, out var baselineValue))
                    {
                        throw new InvalidOperationException("기준 상태 사본에 미리보기 대상 원장이 없습니다: " + effect.TargetStableId);
                    }

                    if (baselineValue.Value != effect.BeforeValue
                        || !string.Equals(baselineValue.Unit, effect.Unit, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException("미리보기 이전 값 또는 단위가 기준 상태 사본과 다릅니다: " + effect.TargetStableId);
                    }
                }
            }
        }

        private static void EnsurePhase(RuleSeedbedSessionSnapshot session, string expected)
        {
            RequireSession(session);
            if (session.PhaseCode != expected)
            {
                throw new InvalidOperationException("현재 단계에서는 이 작업을 수행할 수 없습니다: " + session.PhaseCode);
            }
        }

        private static void RequireSession(RuleSeedbedSessionSnapshot session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
        }

        private static void RequireIds(string[] values, string name)
        {
            if (values == null || values.Length == 0 || values.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("비어 있지 않은 식별자 목록이 필요합니다.", name);
            }
        }

        private static void RequireText(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("값이 필요합니다.", name);
            }
        }
    }

    public static class IntegratedRuleSeedbedCatalog
    {
        public static RuleSeedbedScenarioDescriptor[] Create()
            => new[]
            {
                SimulationScenario(
                    "production.potato",
                    "감자 생산 규칙 실험대",
                    RuleSeedbedDomainCodes.Production,
                    "rule:potato-production.fixture.v1",
                    new[]
                    {
                        "seedbed-object:farm.potato-plant-visual.a",
                        "seedbed-object:farm.potato-harvest-box.a",
                    },
                    "Area", "Environment", "HarvestTask", "HarvestLot"),
                SimulationScenario(
                    "consumption.market-resident",
                    "주민과 도심마트 소비 규칙 실험대",
                    RuleSeedbedDomainCodes.Consumption,
                    "rule:market-resident-consumption.resource.v1",
                    new[]
                    {
                        "seedbed-object:town.resident-visual.a",
                        "seedbed-object:city.urban-market-building.a",
                    },
                    "Demand", "Reservation", "Fulfillment", "Consumption"),
                SimulationScenario(
                    "transport.freight",
                    "화물 운송 규칙 실험대",
                    RuleSeedbedDomainCodes.Transport,
                    "rule:freight-transport.resource.v1",
                    new[]
                    {
                        "seedbed-object:shared.cargo-pallet.a",
                        "seedbed-object:town.delivery-truck.a",
                        "seedbed-object:town.hub-inbound-gate.a",
                    },
                    "Loading", "Travel", "Arrival", "Unloading", "Receipt"),
                SimulationScenario(
                    "warehouse.resource-flow",
                    "창고 자원 흐름 규칙 실험대",
                    RuleSeedbedDomainCodes.Warehouse,
                    "rule:warehouse-resource-flow.v1",
                    new[]
                    {
                        "seedbed-object:town.hub-inbound-gate.a",
                        "seedbed-object:shared.cargo-pallet.a",
                        "seedbed-object:city.operator-inventory-shelf.a",
                    },
                    "Intake", "Inspection", "Putaway", "StorageLoss", "Picking", "Outbound"),
                new RuleSeedbedScenarioDescriptor
                {
                    ScenarioStableId = "rule-seedbed:presentation.integrated",
                    DisplayName = "표현 규칙 비교 실험대",
                    RuleDomainCode = RuleSeedbedDomainCodes.Presentation,
                    RuleStableId = "presentation-rule-catalog:integrated-seedbed.v1",
                    ModeCode = RuleSeedbedModeCodes.PresentationPreview,
                    SeedbedObjectStableIds = new[]
                    {
                        "seedbed-object:farm.potato-plant-visual.a",
                        "seedbed-object:town.delivery-truck.a",
                        "seedbed-object:city.operator-inventory-shelf.a",
                    },
                    StepCodes = new[] { "Graphics", "Camera", "Animation", "Lighting", "Audio", "UI" },
                    CanConfirmSimulation = false,
                    RequiresCanonicalRefresh = false,
                    DoesNotCallOperationalApi = true,
                    SourceStableIds = new[] { "source:presentation-rule-catalog" },
                    Limitations = new[] { "표현 변화는 기준 원장과 업무 완료를 변경하지 않습니다." },
                },
            };

        private static RuleSeedbedScenarioDescriptor SimulationScenario(
            string code,
            string displayName,
            string domain,
            string ruleId,
            string[] objects,
            params string[] steps)
            => new RuleSeedbedScenarioDescriptor
            {
                ScenarioStableId = "rule-seedbed:" + code,
                DisplayName = displayName,
                RuleDomainCode = domain,
                RuleStableId = ruleId,
                ModeCode = RuleSeedbedModeCodes.SimulationFixture,
                SeedbedObjectStableIds = objects,
                StepCodes = steps,
                CanConfirmSimulation = true,
                RequiresCanonicalRefresh = true,
                DoesNotCallOperationalApi = true,
                SourceStableIds = new[] { "source:simulation-fixture-rule" },
                Limitations = new[] { "Simulation Fixture이며 실운영 업무를 만들지 않습니다." },
            };
    }
}
