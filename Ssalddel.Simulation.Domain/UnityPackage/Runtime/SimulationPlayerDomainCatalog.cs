using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class Simulation기본플레이어분야Catalog
    {
        public const string CatalogRevision = "player-domain-catalog.r1";
        public const string RuleRevision = "player-domain-progress.r1";

        public static Simulation플레이어분야CatalogSnapshot Create()
            => new Simulation플레이어분야CatalogSnapshot
            {
                CatalogRevision = CatalogRevision,
                RuleRevision = RuleRevision,
                이해도단계기준들 = new[]
                {
                    Stage(Simulation분야단계Codes.미접촉, "미접촉", 0),
                    Stage(Simulation분야단계Codes.입문, "입문", 1),
                    Stage(Simulation분야단계Codes.이해, "이해", 3),
                    Stage(Simulation분야단계Codes.연결이해, "연결 이해", 7),
                },
                숙련도단계기준들 = new[]
                {
                    Stage(Simulation분야단계Codes.미경험, "미경험", 0),
                    Stage(Simulation분야단계Codes.기초, "기초", 1),
                    Stage(Simulation분야단계Codes.익숙함, "익숙함", 5),
                    Stage(Simulation분야단계Codes.숙련, "숙련", 12),
                },
                분야들 = new[]
                {
                    Domain(Simulation플레이어분야Codes.자연생존, "자연 생존", Simulation분야준비상태Codes.Playable,
                        Skill("risk-response", "위험 대응"), Skill("shelter-rest", "거점 휴식")),
                    Domain(Simulation플레이어분야Codes.탐사공간, "탐사·공간", Simulation분야준비상태Codes.RegisteredWI,
                        Skill("discovery", "지역 발견"), Skill("navigation", "경로 탐색")),
                    Domain(Simulation플레이어분야Codes.채집자원, "채집·자원", Simulation분야준비상태Codes.Playable,
                        Skill("logging", "벌목"), Skill("field-pickup", "현장 획득"), Skill("storage", "자원 보관")),
                    Domain(Simulation플레이어분야Codes.전투사냥, "전투·사냥", Simulation분야준비상태Codes.Playable,
                        Skill("threat-assessment", "위협 판단"), Skill("encounter-combat", "조우 전투")),
                    Domain(Simulation플레이어분야Codes.건설배치, "건설·배치", Simulation분야준비상태Codes.Playable,
                        Skill("site-selection", "터 선정"), Skill("building", "건설"), Skill("repair", "수리")),
                    Domain(Simulation플레이어분야Codes.설비에너지, "설비·에너지", Simulation분야준비상태Codes.AssetSeed,
                        Skill("power-systems", "전력 설비")),
                    Domain(Simulation플레이어분야Codes.제작장비, "제작·장비", Simulation분야준비상태Codes.Playable,
                        Skill("equipment", "장비 사용"), Skill("field-supply", "현장 보급 제작")),
                    Domain(Simulation플레이어분야Codes.농업생산, "농업·생산", Simulation분야준비상태Codes.RegisteredWI,
                        Skill("soil", "토양 작업"), Skill("sowing", "파종"), Skill("growth", "생육"),
                        Skill("harvest", "수확"), Skill("collection", "집하"), Skill("packing", "포장")),
                    Domain(Simulation플레이어분야Codes.창고재고, "창고·재고", Simulation분야준비상태Codes.RegisteredWI,
                        Skill("inspection", "검수"), Skill("putaway", "적재"), Skill("picking", "피킹"),
                        Skill("packing", "창고 포장"), Skill("inventory", "재고 관리")),
                    Domain(Simulation플레이어분야Codes.운송배송, "운송·배송", Simulation분야준비상태Codes.RegisteredWI,
                        Skill("loading", "상차"), Skill("movement", "운송"), Skill("unloading", "하차·인수")),
                    Domain(Simulation플레이어분야Codes.시장생활서비스, "시장·생활 서비스", Simulation분야준비상태Codes.RegisteredWI,
                        Skill("market-stock", "매장 재고"), Skill("order-service", "주문 서비스"),
                        Skill("consumption", "생활 소비"), Skill("city-service", "도심 서비스")),
                    Domain(Simulation플레이어분야Codes.운영조직, "운영·조직", Simulation분야준비상태Codes.RegisteredWI,
                        Skill("planning", "계획"), Skill("delegation", "위임"), Skill("review", "업무 검토"),
                        Skill("turn-closing", "운영 마감"), Skill("cancellation", "업무 취소")),
                    Domain(Simulation플레이어분야Codes.지역발전복구, "지역 발전·복구", Simulation분야준비상태Codes.RegisteredWI,
                        Skill("route-restoration", "경로 복원"), Skill("party-safety", "탐사대 안전"),
                        Skill("regional-repair", "지역 복구")),
                    Domain(Simulation플레이어분야Codes.교역수출, "교역·수출", Simulation분야준비상태Codes.ContractSeed,
                        Skill("export-readiness", "수출 준비")),
                    Domain(Simulation플레이어분야Codes.성찰근거해석, "성찰·근거 해석", Simulation분야준비상태Codes.RegisteredWI,
                        Skill("approved-reflection", "승인 자료 성찰"), Skill("symbol-interpretation", "상징 해석")),
                    Domain(Simulation플레이어분야Codes.기존성찰, "기존 성찰 호환", Simulation분야준비상태Codes.ContractSeed,
                        Skill("legacy-awareness", "기존 알아차림"), Skill("legacy-resolve", "기존 결의")),
                },
                Wi결속들 = Bindings(),
            };

        private static SimulationWI분야결속Definition[] Bindings()
            => new[]
            {
                Bind("WI-ACTOR-01", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.채집자원, "field-pickup"),
                Bind("WI-ACTOR-02", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.제작장비, "equipment"),
                Bind("WI-FARM-01", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.농업생산, "soil"),
                Bind("WI-FARM-02", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.농업생산, "sowing"),
                Bind("WI-FARM-03", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.농업생산, "growth"),
                Bind("WI-FARM-04", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.농업생산, "harvest"),
                Bind("WI-FARM-05", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.농업생산, "collection"),
                Bind("WI-FARM-06", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.농업생산, "packing"),
                Bind("WI-LOG-01", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.운송배송, "loading"),
                Bind("WI-LOG-02", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.운송배송, "movement"),
                Bind("WI-LOG-03", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.운송배송, "movement"),
                Bind("WI-LOG-04", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.운송배송, "unloading"),
                Bind("WI-LOG-05", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.운송배송, "unloading"),
                Bind("WI-001", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.창고재고, "inspection"),
                Bind("WI-002", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.창고재고, "putaway"),
                Bind("WI-HUB-03", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.창고재고, "inventory"),
                Bind("WI-HUB-04", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.창고재고, "picking"),
                Bind("WI-HUB-05", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.창고재고, "packing"),
                Bind("WI-HUB-06", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.운송배송, "loading"),
                Bind("WI-MARKET-01", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.운송배송, "movement"),
                Bind("WI-MARKET-02", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.운송배송, "unloading"),
                Bind("WI-MARKET-03", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.창고재고, "inspection"),
                Bind("WI-MARKET-04", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.창고재고, "putaway"),
                Bind("WI-MARKET-05", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.시장생활서비스, "market-stock"),
                Bind("WI-ORDER-01", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.시장생활서비스, "order-service"),
                Bind("WI-ORDER-02", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.시장생활서비스, "order-service"),
                Bind("WI-ORDER-03", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.시장생활서비스, "order-service"),
                Bind("WI-ORDER-04", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.시장생활서비스, "order-service"),
                Bind("WI-ORDER-05", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.시장생활서비스, "order-service"),
                Bind("WI-ORDER-06", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.시장생활서비스, "order-service"),
                Bind("WI-ORDER-07", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.시장생활서비스, "consumption"),
                Bind("WI-NATURE-01", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.전투사냥, "threat-assessment"),
                Bind("WI-NATURE-02", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.자연생존, "risk-response"),
                Bind("WI-NATURE-03", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.지역발전복구, "route-restoration"),
                Bind("WI-NATURE-04", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.지역발전복구, "party-safety"),
                Bind("WI-NATURE-05", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.제작장비, "equipment"),
                Bind("WI-NATURE-06", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.채집자원, "logging"),
                Bind("WI-NATURE-07", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.건설배치, "site-selection"),
                Bind("WI-NATURE-08", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.건설배치, "building"),
                Bind("WI-NATURE-09", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.자연생존, "shelter-rest"),
                Bind("WI-NATURE-10", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.자연생존, "shelter-rest"),
                Bind("WI-NATURE-11", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.전투사냥, "encounter-combat"),
                Bind("WI-NATURE-12", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.운영조직, "cancellation"),
                Bind("WI-NATURE-13", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.채집자원, "storage"),
                Bind("WI-NATURE-14", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.자연생존, "shelter-rest"),
                Bind("WI-NATURE-15", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.운영조직, "planning"),
                Bind("WI-NATURE-16", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.제작장비, "field-supply"),
                Bind("WI-NATURE-17", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.운영조직, "delegation"),
                Bind("WI-NATURE-18", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.채집자원, "field-pickup"),
                Bind("WI-REFLECT-01", Simulation분야기여방식Codes.LearningOnly, Simulation플레이어분야Codes.성찰근거해석, "approved-reflection"),
                None("WI-CARD-01", "메이저 아르카나 활성화만으로 분야 진척을 지급하지 않는다."),
                Bind("WI-CON-01", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.건설배치, "building"),
                Bind("WI-CITY-01", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.운영조직, "planning"),
                Bind("WI-CITY-02", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.운영조직, "delegation"),
                Bind("WI-CITY-03", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.시장생활서비스, "city-service"),
                Bind("WI-CITY-04", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.시장생활서비스, "city-service"),
                Bind("WI-WORLD-01", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.운영조직, "delegation"),
                Bind("WI-WORLD-02", Simulation분야기여방식Codes.OperationOnly, Simulation플레이어분야Codes.운영조직, "delegation"),
                Bind("WI-WORLD-03", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.운영조직, "cancellation"),
                Bind("WI-WORLD-04", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.지역발전복구, "regional-repair"),
                Bind("WI-WORLD-05", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.탐사공간, "discovery"),
                Bind("WI-WORLD-06", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.제작장비, "equipment"),
                Bind("WI-WORLD-07", Simulation분야기여방식Codes.PlayerOrOperation, Simulation플레이어분야Codes.운영조직, "planning"),
                Bind("WI-WORLD-08", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.운영조직, "turn-closing"),
                Bind("WI-REVIEW-01", Simulation분야기여방식Codes.PlayerDirect, Simulation플레이어분야Codes.운영조직, "review"),
            };

        private static Simulation플레이어분야Definition Domain(string code,
            string name, string readiness, params Simulation세부숙련Definition[] skills)
            => new Simulation플레이어분야Definition
            {
                분야StableId = code, 한국어명 = name, 준비상태Code = readiness,
                세부숙련들 = skills,
            };

        private static Simulation세부숙련Definition Skill(string id, string name)
            => new Simulation세부숙련Definition { StableId = id, 한국어명 = name };

        private static Simulation분야단계기준Definition Stage(string code,
            string name, int minimum)
            => new Simulation분야단계기준Definition
                { 단계Code = code, 한국어명 = name, 최소진척 = minimum };

        private static SimulationWI분야결속Definition Bind(string wi, string mode,
            string domain, string skill)
            => new SimulationWI분야결속Definition
            {
                WorldInteractionId = wi, 기여방식Code = mode,
                결속선들 = new[] { new Simulation분야숙련결속선Definition
                    { 분야StableId = domain, 세부숙련StableId = skill } },
            };

        private static SimulationWI분야결속Definition None(string wi, string reason)
            => new SimulationWI분야결속Definition
            {
                WorldInteractionId = wi,
                기여방식Code = Simulation분야기여방식Codes.None,
                NoPlayerProgressReason = reason,
            };
    }
}
