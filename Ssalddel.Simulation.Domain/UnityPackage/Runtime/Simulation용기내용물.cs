using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "동종 내용물의 양·가중 온도·제조 출처를 보존하는 순수 전송 계산이다.",
        Boundary = "미래 섭취·제작 소비자 지원만 수행한다. Command·행위·성장·시간·Save·WI 실행은 없다.",
        WorldInteractionIds = new[] { "WI-ACTOR-CONSUME", "WI-CRAFT-BREW" })]
    public static class Simulation용기내용물Calculator
    {
        public static Simulation내용물전송Result 전송(Simulation용기내용물Snapshot 원천,
            Simulation용기내용물Snapshot 대상, long 요청량Ml)
        {
            검증(원천); 검증(대상);
            if (원천.용기StableId == 대상.용기StableId) 거부("ContentsSameContainer");
            if (!원천.물보관가능 || !대상.물보관가능) 거부("ContentsStorageNotSupported");
            if (요청량Ml <= 0) 거부("ContentsTransferQuantityInvalid");
            if (요청량Ml > 원천.현재량Ml) 거부("ContentsSourceInsufficient");
            var 빈용량 = 대상.최대량Ml - 대상.현재량Ml;
            if (빈용량 == 0) 거부("ContentsTargetFull");
            if (대상.현재량Ml > 0 && (원천.종류Code != 대상.종류Code
                || 원천.처방StableId != 대상.처방StableId || 원천.효과Revision != 대상.효과Revision))
                거부("ContentsProfileMismatch");
            var 이동량 = Math.Min(요청량Ml, 빈용량);
            var 원천출처 = 원천.제조출처.OrderBy(x => x.제조출처StableId, StringComparer.Ordinal).ToArray();
            var 배분 = 원천출처.Select(x => (long)((BigInteger)x.양Ml * 이동량 / 원천.현재량Ml)).ToArray();
            var 미배분 = 이동량 - 배분.Sum();
            // 최대나머지 방식. 동률은 고유 식별자 Ordinal 순서다. 출처별 총량은 정확히 보존한다.
            foreach (var i in Enumerable.Range(0, 원천출처.Length)
                .OrderByDescending(i => (BigInteger)원천출처[i].양Ml * 이동량 % 원천.현재량Ml)
                .ThenBy(i => 원천출처[i].제조출처StableId, StringComparer.Ordinal).Take((int)미배분))
                배분[i]++;
            var 남은출처 = new List<Simulation내용물출처Snapshot>();
            var 합산출처 = 대상.제조출처.ToDictionary(x => x.제조출처StableId, x => x.양Ml, StringComparer.Ordinal);
            for (var i = 0; i < 원천출처.Length; i++)
            {
                var 항목 = 원천출처[i];
                if (항목.양Ml > 배분[i]) 남은출처.Add(new Simulation내용물출처Snapshot(항목.제조출처StableId, 항목.양Ml - 배분[i]));
                if (배분[i] == 0) continue;
                합산출처.TryGetValue(항목.제조출처StableId, out var 기존량);
                합산출처[항목.제조출처StableId] = checked(기존량 + 배분[i]);
            }
            var 대상량 = checked(대상.현재량Ml + 이동량);
            // 곱셈은 임의 정밀도로 계산한다. 정수 milli°C 미만은 0 방향 버림이며 효과 판본은 바꾸지 않는다.
            var 대상온도 = (long)(((BigInteger)원천.온도MilliCelsius * 이동량
                + (BigInteger)대상.온도MilliCelsius * 대상.현재량Ml) / 대상량);
            var 남은양 = 원천.현재량Ml - 이동량;
            return new Simulation내용물전송Result(
                사본(원천, 남은양, 남은양 == 0 ? 0 : 원천.온도MilliCelsius, 원천, 남은출처.ToArray()),
                사본(대상, 대상량, 대상온도, 원천, 합산출처.OrderBy(x => x.Key, StringComparer.Ordinal)
                    .Select(x => new Simulation내용물출처Snapshot(x.Key, x.Value)).ToArray()), 이동량);
        }

        public static void 검증(Simulation용기내용물Snapshot 상태)
        {
            if (상태 == null) 거부("ContentsStateRequired");
            if (!식별자유효(상태!.용기StableId)) 거부("ContentsContainerIdInvalid");
            if (상태.최대량Ml <= 0 || 상태.현재량Ml < 0 || 상태.현재량Ml > 상태.최대량Ml)
                거부("ContentsCapacityInvalid");
            var 출처 = 상태.제조출처;
            if (상태.현재량Ml == 0)
            {
                if (상태.종류Code != "" || 상태.처방StableId != "" || 상태.효과Revision != ""
                    || 상태.온도MilliCelsius != 0 || 출처.Length != 0) 거부("ContentsEmptyStateInvalid");
                return;
            }
            if (!상태.물보관가능) 거부("ContentsStorageNotSupported");
            if (상태.종류Code != Simulation용기내용물Codes.물 && 상태.종류Code != Simulation용기내용물Codes.차)
                거부("ContentsKindInvalid");
            if (상태.종류Code == Simulation용기내용물Codes.물)
            {
                if (상태.처방StableId != "" || 상태.효과Revision != "") 거부("ContentsWaterProfileInvalid");
            }
            else if (!식별자유효(상태.처방StableId) || !식별자유효(상태.효과Revision)) 거부("ContentsTeaProfileInvalid");
            var 식별자 = new HashSet<string>(StringComparer.Ordinal);
            BigInteger 총량 = 0;
            foreach (var 항목 in 출처)
            {
                if (항목 == null || !식별자유효(항목.제조출처StableId) || 항목.양Ml <= 0
                    || !식별자.Add(항목.제조출처StableId)) 거부("ContentsProvenanceInvalid");
                총량 += 항목!.양Ml;
            }
            if (총량 != 상태.현재량Ml) 거부("ContentsProvenanceQuantityMismatch");
        }

        private static Simulation용기내용물Snapshot 사본(Simulation용기내용물Snapshot 용기, long 양,
            long 온도, Simulation용기내용물Snapshot 종류, Simulation내용물출처Snapshot[] 출처)
            => new Simulation용기내용물Snapshot(용기.용기StableId, 용기.물보관가능, 용기.최대량Ml, 양,
                양 == 0 ? "" : 종류.종류Code, 양 == 0 ? "" : 종류.처방StableId,
                양 == 0 ? "" : 종류.효과Revision, 온도, 출처);
        private static bool 식별자유효(string 값) => !string.IsNullOrWhiteSpace(값) && 값 == 값.Trim();
        private static void 거부(string 코드) => throw new SimulationContractException(코드);
    }
}
