using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Web.UnityReviewApp.Models;

public static class Synty공간조립검토Presentation
{
    public static readonly IReadOnlyList<Synty공간조립검토문제선택지> IssueChoices =
    [
        new(Synty공간조립검토문제Codes.RouteUnclear, "동선 불명확"),
        new(Synty공간조립검토문제Codes.TooDense, "너무 복잡함"),
        new(Synty공간조립검토문제Codes.PackBlendAwkward, "팩 혼합 어색"),
        new(Synty공간조립검토문제Codes.PsychologicalReadabilityWeak, "회복·위협감 부족"),
        new(Synty공간조립검토문제Codes.PerformanceConcern, "성능 우려"),
        new(Synty공간조립검토문제Codes.EntranceExitUnclear, "출입구 불명확")
    ];

    public static string StateLabel(string stateCode)
        => stateCode switch
        {
            Synty공간조립검토상태Codes.WaitingForCapture => "촬영 대기",
            Synty공간조립검토상태Codes.ReadyForReview => "검토 대기",
            Synty공간조립검토상태Codes.ReviewedCandidate => "좋음 후보",
            Synty공간조립검토상태Codes.NeedsRevision => "Unity 재촬영 대기",
            Synty공간조립검토상태Codes.OnHold => "보류",
            Synty공간조립검토상태Codes.CompareCandidate => "비교 후보",
            Synty공간조립검토상태Codes.Stale => "입력 변경 · 재검토",
            _ => stateCode
        };

    public static string HistoryLabel(Synty공간조립검토결정이력Dto history)
        => history.EventCode switch
        {
            Synty공간조립검토EventCodes.RecaptureSubmitted => "새 재촬영 등록",
            Synty공간조립검토EventCodes.SourceUpdated => "조립 입력 변경",
            _ => history.DecisionCode switch
            {
                Synty공간조립검토결정Codes.Good => "좋음 후보",
                Synty공간조립검토결정Codes.NeedsRevision => "수정 필요",
                Synty공간조립검토결정Codes.OnHold => "보류",
                Synty공간조립검토결정Codes.CompareCandidate => "비교 후보",
                _ => history.EventCode
            }
        };

    public static string StateCss(string stateCode)
        => stateCode switch
        {
            Synty공간조립검토상태Codes.ReadyForReview => "ready",
            Synty공간조립검토상태Codes.Stale => "stale",
            Synty공간조립검토상태Codes.ReviewedCandidate => "reviewed",
            Synty공간조립검토상태Codes.NeedsRevision => "revision",
            _ => "neutral"
        };
}

public sealed record Synty공간조립검토문제선택지(string Code, string Label);
