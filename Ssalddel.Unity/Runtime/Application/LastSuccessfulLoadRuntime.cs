using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Unity.Application
{
    public sealed class LastSuccessfulLoadResult<TSnapshot, TChange>
        where TSnapshot : class
    {
        public ZoneRuntimeStateCode StateCode { get; set; }
        public TSnapshot? Snapshot { get; set; }
        public TChange? Changes { get; set; }
        public Exception? Error { get; set; }
    }

    /// <summary>
    /// Zone별 표현 모델과 무관하게 최초 조회와 새로고침의 마지막 성공 상태를 보존한다.
    /// 서버 상태를 만들거나 변경하지 않는 Unity 읽기 전용 조율기다.
    /// </summary>
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.UnityResilientWorldLoad,
        SsalddelCodeLayer.ClientAdapter,
        "최초 조회와 새로고침을 구분하고 마지막 성공 Snapshot을 유지한다.",
        StepKey = "client.last-successful-runtime",
        ExecutionStage = SsalddelCodeExecutionStage.Presentation,
        Effects = SsalddelCodeEffect.UiStateMutation,
        WritesTo = SsalddelCodeDataScope.ClientPresentation,
        FlowOrder = 10,
        Boundary = "서버 상태를 만들거나 변경하지 않고 실패 시 마지막 성공 표현만 보존한다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class LastSuccessfulLoadRuntime<TSnapshot, TChange>
        where TSnapshot : class
    {
        private TSnapshot? lastSuccessful;

        public ZoneRuntimeStateCode StateCode { get; private set; }
            = ZoneRuntimeStateCode.Idle;

        public async Task<LastSuccessfulLoadResult<TSnapshot, TChange>> LoadAsync(
            Func<CancellationToken, Task<TSnapshot>> load,
            Func<TSnapshot?, TSnapshot, TChange> reconcile,
            CancellationToken cancellationToken = default)
        {
            if (load == null) throw new ArgumentNullException(nameof(load));
            if (reconcile == null) throw new ArgumentNullException(nameof(reconcile));

            StateCode = lastSuccessful == null
                ? ZoneRuntimeStateCode.InitialLoading
                : ZoneRuntimeStateCode.Refreshing;
            try
            {
                var snapshot = await load(cancellationToken).ConfigureAwait(false);
                var changes = reconcile(lastSuccessful, snapshot);
                lastSuccessful = snapshot;
                StateCode = ZoneRuntimeStateCode.Ready;
                return new LastSuccessfulLoadResult<TSnapshot, TChange>
                {
                    StateCode = StateCode,
                    Snapshot = snapshot,
                    Changes = changes,
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception error)
            {
                StateCode = lastSuccessful == null
                    ? ZoneRuntimeStateCode.InitialError
                    : ZoneRuntimeStateCode.RefreshError;
                return new LastSuccessfulLoadResult<TSnapshot, TChange>
                {
                    StateCode = StateCode,
                    Snapshot = lastSuccessful,
                    Error = error,
                };
            }
        }
    }
}
