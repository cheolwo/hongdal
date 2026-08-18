using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.WorkflowRules
{
    public static class 화물배차추천점수Policy
    {
        public const string RuleRevision = "freight-dispatch-score.v1";

        public static 화물배차추천점수판정 판정(화물배차추천점수요청 request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var schedule = 0m;
            if (request.전체일정완수가능여부.HasValue)
            {
                if (!request.전체일정완수가능여부.Value) schedule -= 50m;
                else if (request.일정삽입가능여부 == true) schedule += 10m;
                if (request.경로변경이점여부) schedule += 15m;
            }

            var profit = request.예상추가순이익.HasValue
                ? Math.Clamp(request.예상추가순이익.Value / 1000m, -20m, 40m)
                : 0m;
            var delay = request.추가지연분.HasValue
                ? request.추가지연분.Value <= 5m ? 18m
                    : request.추가지연분.Value <= 10m ? 10m
                    : request.추가지연분.Value <= 20m ? 2m
                    : -10m
                : 0m;
            var distance = request.경로기준거리Km.HasValue
                ? request.경로기준거리Km.Value <= 2m ? 15m
                    : request.경로기준거리Km.Value <= 5m ? 8m
                    : request.경로기준거리Km.Value <= 8m ? 2m
                    : -8m
                : 0m;
            var recommendationType = string.Equals(
                    request.추천유형, "bundle_insert", StringComparison.OrdinalIgnoreCase)
                ? 12m
                : string.Equals(request.추천유형, "next_after_dropoff", StringComparison.OrdinalIgnoreCase)
                    ? 8m
                    : 0m;
            var sensitivity = request.화물민감여부 ? -6m : 0m;
            var returnBurden = request.복귀지기준사용여부 && request.복귀우회증가거리Km.HasValue
                ? request.복귀우회증가거리Km.Value <= 0m ? 20m
                    : request.복귀우회증가거리Km.Value <= 5m ? 10m
                    : request.복귀우회증가거리Km.Value <= 15m ? 0m
                    : request.복귀우회증가거리Km.Value <= 30m ? -10m
                    : -25m
                : 0m;
            var total = schedule + profit + delay + distance + recommendationType
                + sensitivity + returnBurden;

            return new 화물배차추천점수판정
            {
                일정점수 = schedule,
                수익점수 = profit,
                지연점수 = delay,
                거리점수 = distance,
                추천유형점수 = recommendationType,
                화물민감도점수 = sensitivity,
                복귀부담점수 = returnBurden,
                총점 = total,
                RuleRevision = RuleRevision,
                SourceStableIds = new[]
                {
                    "capability:driver.freight-transport",
                    "contract-revision:driver-transport.v1",
                    "rule-revision:" + RuleRevision,
                },
            };
        }
    }

    public static class 화물배차기사대기점수Policy
    {
        public const decimal 최대점수 = 24m;
        public const decimal 점수단위분 = 30m;
        public const decimal 단위점수 = 3m;

        public static decimal 계산(decimal 기사대기분)
        {
            if (기사대기분 < 점수단위분) return 0m;
            var score = Math.Floor(기사대기분 / 점수단위분) * 단위점수;
            return Math.Clamp(score, 0m, 최대점수);
        }
    }

    public static class 화물배차후보선정Policy
    {
        public const string RuleRevision = "freight-dispatch-candidate.v1";

        public static 화물배차후보선정판정 판정(화물배차후보선정요청 request)
        {
            Validate(request);
            var evaluations = request.후보목록.Select(candidate => Evaluate(request, candidate)).ToList();
            var eligible = evaluations
                .Where(value => value.적격여부)
                .OrderByDescending(value => value.총추천점수)
                .ThenBy(value => value.상차거리Km ?? decimal.MaxValue)
                .ThenBy(value => value.후보StableId, StringComparer.Ordinal)
                .ToArray();
            for (var index = 0; index < eligible.Length; index++) eligible[index].추천순위 = index + 1;

            var ordered = evaluations
                .OrderByDescending(value => value.적격여부)
                .ThenBy(value => value.적격여부 ? value.추천순위 : int.MaxValue)
                .ThenBy(value => value.후보StableId, StringComparer.Ordinal)
                .ToArray();
            return new 화물배차후보선정판정
            {
                추천후보StableId = eligible.FirstOrDefault()?.후보StableId,
                적격후보수 = eligible.Length,
                후보평가목록 = ordered,
                RuleRevision = RuleRevision,
                SourceStableIds = new[]
                {
                    "capability:driver.freight-transport",
                    "contract-revision:driver-transport.v1",
                    "rule-revision:" + RuleRevision,
                    "rule-revision:" + 화물배차추천점수Policy.RuleRevision,
                },
            };
        }

        private static 화물배차후보평가 Evaluate(
            화물배차후보선정요청 request,
            화물배차후보입력 candidate)
        {
            var blocks = new List<string>();
            if (!candidate.화물운송앱여부) blocks.Add(화물배차후보차단사유코드.화물운송앱아님);
            if (!candidate.차량활성여부) blocks.Add(화물배차후보차단사유코드.차량비활성);
            if (!candidate.기사운행중여부) blocks.Add(화물배차후보차단사유코드.기사운행중아님);
            if (!string.IsNullOrWhiteSpace(request.제외후보StableId)
                && string.Equals(candidate.후보StableId, request.제외후보StableId, StringComparison.Ordinal))
                blocks.Add(화물배차후보차단사유코드.명시적제외);
            if (candidate.이전거절여부) blocks.Add(화물배차후보차단사유코드.이전거절);

            if (!candidate.위치경과분.HasValue)
                blocks.Add(화물배차후보차단사유코드.위치정보없음);
            else if (candidate.위치경과분.Value < -1m
                || candidate.위치경과분.Value > request.위치유효시간분)
                blocks.Add(화물배차후보차단사유코드.위치정보오래됨);

            if (!candidate.차량적합여부)
            {
                blocks.Add(화물배차후보차단사유코드.차량부적합);
                blocks.AddRange(candidate.차량부적합사유코드목록
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
            }
            if (!string.Equals(candidate.차량용량단위코드, request.화물단위코드,
                    StringComparison.OrdinalIgnoreCase))
                blocks.Add(화물배차후보차단사유코드.차량용량단위불일치);
            if (candidate.차량용량 < request.화물수량)
                blocks.Add(화물배차후보차단사유코드.차량용량부족);

            EvaluatePickupAccess(request, candidate, blocks);
            var score = 화물배차추천점수Policy.판정(candidate.추천점수요청);
            var aging = 화물배차기사대기점수Policy.계산(candidate.기사대기분);
            var eligible = blocks.Count == 0;
            var reasonParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(candidate.기본추천사유))
                reasonParts.Add(candidate.기본추천사유.Trim());
            if (candidate.상차거리Km.HasValue)
                reasonParts.Add("상차 " + candidate.상차거리Km.Value.ToString("0.0", CultureInfo.InvariantCulture) + "km");
            if (aging > 0m)
                reasonParts.Add("기사대기보정 +" + aging.ToString("0", CultureInfo.InvariantCulture));
            if (eligible)
                reasonParts.Add("추천점수 " + (score.총점 + aging).ToString("0", CultureInfo.InvariantCulture));

            return new 화물배차후보평가
            {
                후보StableId = candidate.후보StableId.Trim(),
                차량StableId = candidate.차량StableId.Trim(),
                적격여부 = eligible,
                기본추천점수 = score.총점,
                기사대기보정점수 = aging,
                총추천점수 = score.총점 + aging,
                상차거리Km = candidate.상차거리Km,
                차량용량 = candidate.차량용량,
                차량용량단위코드 = candidate.차량용량단위코드.Trim(),
                추천사유 = eligible
                    ? string.Join(" · ", reasonParts)
                    : "차단: " + string.Join(",", blocks.Distinct(StringComparer.Ordinal)),
                차단사유코드목록 = blocks.Distinct(StringComparer.Ordinal).ToArray(),
                점수내역 = score,
            };
        }

        private static void EvaluatePickupAccess(
            화물배차후보선정요청 request,
            화물배차후보입력 candidate,
            List<string> blocks)
        {
            if (!candidate.상차거리Km.HasValue)
            {
                blocks.Add(화물배차후보차단사유코드.거리정보없음);
                return;
            }

            var distance = candidate.상차거리Km.Value;
            var baseRadius = request.기본상차접근반경Km;
            var candidateRadius = Math.Max(baseRadius,
                candidate.상차접근허용반경Km ?? baseRadius);
            var allowedRadius = Math.Min(candidateRadius, request.원거리상차접근최대반경Km);
            if (distance > allowedRadius)
            {
                blocks.Add(화물배차후보차단사유코드.상차접근반경초과);
                return;
            }
            if (distance <= baseRadius) return;

            if (!request.상차시간창남은분.HasValue || request.상차시간창남은분.Value <= 0m)
            {
                blocks.Add(화물배차후보차단사유코드.상차시간창종료);
                return;
            }
            var estimatedMinutes = distance / request.원거리상차평균속도KmH * 60m
                + request.원거리상차도착여유분;
            if (request.상차시간창남은분.Value < estimatedMinutes)
                blocks.Add(화물배차후보차단사유코드.상차시간창도착불가);
        }

        private static void Validate(화물배차후보선정요청 request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.화물수량 <= 0m
                || string.IsNullOrWhiteSpace(request.화물단위코드)
                || request.위치유효시간분 <= 0m
                || request.기본상차접근반경Km <= 0m
                || request.원거리상차접근최대반경Km < request.기본상차접근반경Km
                || request.원거리상차평균속도KmH <= 0m
                || request.원거리상차도착여유분 < 0m
                || request.후보목록 == null
                || request.후보목록.Length == 0)
                throw new ArgumentException("화물 배차 후보 판정 입력이 올바르지 않습니다.", nameof(request));

            if (request.후보목록.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.후보StableId)
                    || string.IsNullOrWhiteSpace(value.차량StableId)
                    || value.추천점수요청 == null)
                || request.후보목록.GroupBy(value => value.후보StableId.Trim(), StringComparer.Ordinal)
                    .Any(group => group.Count() > 1))
                throw new ArgumentException("화물 배차 후보 식별자가 올바르지 않습니다.", nameof(request));
        }
    }
}
