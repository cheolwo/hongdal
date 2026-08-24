using System;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.WorkflowRules
{
    /// <summary>
    /// 운영 창고 원장의 기존 저장 값을 공통 창고 입고 업무 단계로 읽기 전용 정규화합니다.
    /// 저장 값이나 운영 상태 전이를 변경하지 않습니다.
    /// </summary>
    public static class 창고입고업무상태Adapter
    {
        public static string 정규화(string persistedState)
        {
            var state = persistedState?.Trim() ?? string.Empty;
            if (string.Equals(state, "예정", StringComparison.Ordinal)
                || string.Equals(state, "입고예정", StringComparison.Ordinal)
                || string.Equals(state, "운송중", StringComparison.Ordinal))
                return 창고입고상태코드.입고예정;
            if (string.Equals(state, "입고완료", StringComparison.Ordinal)
                || string.Equals(state, "완료", StringComparison.Ordinal)
                || string.Equals(state, "보관중", StringComparison.Ordinal))
                return 창고입고상태코드.검수대기;
            if (state.StartsWith("검수완료", StringComparison.Ordinal))
                return 창고입고상태코드.적재대기;
            if (string.Equals(state, "적재완료", StringComparison.Ordinal))
                return 창고입고상태코드.적재완료;
            return string.Empty;
        }
    }
}
