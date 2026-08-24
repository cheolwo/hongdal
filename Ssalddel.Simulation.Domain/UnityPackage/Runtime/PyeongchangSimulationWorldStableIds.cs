namespace Ssalddel.Simulation.Domain
{
public static class PyeongchangSimulationWorldStableIds
{
    public const string 대관령Farm시설 = "facility:sim:pyeongchang:daegwallyeong-farm";
    public const string 진부Hub시설 = "facility:sim:pyeongchang:jinbu-hub";
    public const string 평창읍Mart시설 = "facility:sim:pyeongchang:pyeongchang-town-mart";
    public const string 평창읍음식점시설 = "facility:sim:pyeongchang:pyeongchang-town-restaurant";

    public const string 대관령Farm영역 = "area:sim:pyeongchang:daegwallyeong-farm";
    public const string 진부Hub영역 = "area:sim:pyeongchang:jinbu-hub";
    public const string 평창읍Town영역 = "area:sim:pyeongchang:pyeongchang-town";
    public const string 대관령Farm경관완결영역 =
        "completion-area:sim:pyeongchang:daegwallyeong-farm.v1";

    public static readonly string[] 대관령Farm경관완결L2타일키 =
    new[]
    {
        "kr5186:l2:700:1144",
        "kr5186:l2:701:1144",
        "kr5186:l2:700:1145",
        "kr5186:l2:701:1145",
    };

    public static readonly string[] 대관령Farm경관완결상위타일키 =
    new[]
    {
        "kr5186:l0:43:71",
        "kr5186:l1:175:286",
    };

    public const string 수확판로배분규칙 = "rule:simulation:farm:harvest-allocation";
    public const string Farm출하화물규칙 = "rule:simulation:farm:outbound-cargo";
    public const string 창고용량예약규칙 = "rule:simulation:warehouse:capacity-reservation";
    public const string 창고입고검수규칙 = "rule:simulation:warehouse:inbound-inspection";
    public const string 창고적재규칙 = "rule:simulation:warehouse:put-away";
    public const string 물류이동규칙 = "rule:simulation:logistics:movement";
    public const string 화물배차규칙 = "rule:simulation:freight:dispatch";
    public const string 화물운송규칙 = "rule:simulation:freight:transport";
    public const string 개별주문규칙 = "rule:simulation:order:individual";
    public const string Mart재고진열규칙 = "rule:simulation:mart:stock-display";
    public const string 음식점식자재주문규칙 = "rule:simulation:restaurant:ingredient-order";
    public const string 팀역할Card장착규칙 = "rule:simulation:team-role-card:equip";
    public const string 팀활동시작규칙 = "rule:simulation:team-role-card:activity-start";
    public const string 팀활동종료규칙 = "rule:simulation:team-role-card:activity-end";
    public const string L2타일발견보상규칙 =
        "rule:simulation:collectible-card:l2-discovery-reward";
    public const string 농사완료보상규칙 =
        "rule:simulation:collectible-card:farm-completion-reward";
    public const string 수집Card뽑기규칙 =
        "rule:simulation:collectible-card:draw";
    public const string 수집Card양도규칙 =
        "rule:simulation:collectible-card:transfer";
    public const string 전투시점확정규칙 =
        "rule:simulation:farm-combat:perspective-confirm";
    public const string 전투박자시작규칙 =
        "rule:simulation:farm-combat:beat-start";
    public const string 전투반응판정규칙 =
        "rule:simulation:farm-combat:reaction-confirm";
    public const string 전술기회생성규칙 =
        "rule:simulation:farm-combat:tactical-opportunity";
    public const string 전술명령확정규칙 =
        "rule:simulation:farm-combat:tactical-order-confirm";
}
}
