using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Learning
{
    public static class 하루단계Codes
    {
        public const string Day = "Day";
        public const string EveningStudy = "EveningStudy";
    }

    public static class 학습콘텐츠종류Codes
    {
        public const string ArcanaAndVideo = "ArcanaAndVideo";
    }

    public static class 내면규칙Codes
    {
        public const string BeginnerMind = "BeginnerMind";
        public const string IntegratedProgress = "IntegratedProgress";
    }

    public static class 내면StatCodes
    {
        public const string Awareness = "Awareness";
        public const string Resolve = "Resolve";
    }

    public sealed class 플레이어내면상태Snapshot
    {
        public int 알아차림 { get; set; }
        public int 명료함 { get; set; }
        public int 양심 { get; set; }
        public int 조화 { get; set; }
        public int 의지 { get; set; }
        public int 통찰 { get; set; }
        public string[] ActiveRuleCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class 저녁학당콘텐츠Snapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string KindCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string TeachingSummary { get; set; } = string.Empty;
        public string ReflectionPrompt { get; set; } = string.Empty;
        public string CardStableId { get; set; } = string.Empty;
        public string KnowledgeNoteStableId { get; set; } = string.Empty;
        public string SourceVideoId { get; set; } = string.Empty;
        public int SourceStartSeconds { get; set; }
        public string TargetStatCode { get; set; } = string.Empty;
        public string GrantedRuleCode { get; set; } = string.Empty;
        public int StatDelta { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 저녁학당학습기록Data
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public DateTimeOffset StudiedOn { get; set; }
        public string ContentStableId { get; set; } = string.Empty;
        public string ReflectionText { get; set; } = string.Empty;
        public string GrantedRuleCode { get; set; } = string.Empty;
        public string StatCode { get; set; } = string.Empty;
        public int StatDelta { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 저녁학당SimulationSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public DateTimeOffset SimulationDate { get; set; }
        public string DayPhaseCode { get; set; } = string.Empty;
        public 플레이어내면상태Snapshot InnerState { get; set; } = new 플레이어내면상태Snapshot();
        public 저녁학당콘텐츠Snapshot[] AvailableContents { get; set; }
            = Array.Empty<저녁학당콘텐츠Snapshot>();
        public 저녁학당학습기록Data[] StudyLedger { get; set; }
            = Array.Empty<저녁학당학습기록Data>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 저녁학당학습Preview
    {
        public string StableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public string ContentStableId { get; set; } = string.Empty;
        public string ReflectionPrompt { get; set; } = string.Empty;
        public string TargetStatCode { get; set; } = string.Empty;
        public int StatBefore { get; set; }
        public int StatAfter { get; set; }
        public string GrantedRuleCode { get; set; } = string.Empty;
        public DateTimeOffset EffectiveSimulationDate { get; set; }
        public bool RevealsUnknownsInNextDayPreviews { get; set; }
        public bool RequiresExplicitConfirmation { get; set; }
    }

    public sealed class 저녁학당학습Command
    {
        public string StableId { get; set; } = string.Empty;
        public string PreviewStableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public string ContentStableId { get; set; } = string.Empty;
        public string ReflectionText { get; set; } = string.Empty;
        public long SimulationTick { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 저녁학당SimulationValidator
    {
        public void Validate(저녁학당SimulationSnapshot snapshot)
        {
            if (snapshot == null || !StableDataId.IsValid(snapshot.StableId)
                || snapshot.DataRevision <= 0 || snapshot.ModeCode != "Simulation"
                || !StableDataId.IsValid(snapshot.ScenarioStableId)
                || snapshot.SimulationDate == default
                || (snapshot.DayPhaseCode != 하루단계Codes.Day
                    && snapshot.DayPhaseCode != 하루단계Codes.EveningStudy)
                || snapshot.InnerState == null
                || snapshot.AvailableContents == null || snapshot.AvailableContents.Length == 0
                || snapshot.StudyLedger == null
                || snapshot.SourceStableIds == null || snapshot.SourceStableIds.Length == 0
                || snapshot.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("EveningStudySnapshotInvalid");

            ValidateInnerState(snapshot.InnerState);
            foreach (var content in snapshot.AvailableContents) ValidateContent(content);
            if (snapshot.AvailableContents.Select(value => value.StableId)
                .Distinct(StringComparer.Ordinal).Count() != snapshot.AvailableContents.Length)
                throw new InvalidOperationException("EveningStudyContentDuplicate");
            foreach (var record in snapshot.StudyLedger) ValidateRecord(record);
        }

        private static void ValidateInnerState(플레이어내면상태Snapshot state)
        {
            if (state.알아차림 < 0 || state.명료함 < 0 || state.양심 < 0 || state.조화 < 0
                || state.의지 < 0 || state.통찰 < 0 || state.ActiveRuleCodes == null
                || state.ActiveRuleCodes.Any(string.IsNullOrWhiteSpace)
                || state.ActiveRuleCodes.Distinct(StringComparer.Ordinal).Count()
                    != state.ActiveRuleCodes.Length)
                throw new InvalidOperationException("PlayerInnerStateInvalid");
        }

        private static void ValidateContent(저녁학당콘텐츠Snapshot content)
        {
            if (content == null || !StableDataId.IsValid(content.StableId) || content.Revision <= 0
                || content.KindCode != 학습콘텐츠종류Codes.ArcanaAndVideo
                || string.IsNullOrWhiteSpace(content.Title)
                || string.IsNullOrWhiteSpace(content.TeachingSummary)
                || string.IsNullOrWhiteSpace(content.ReflectionPrompt)
                || !StableDataId.IsValid(content.CardStableId)
                || !StableDataId.IsValid(content.KnowledgeNoteStableId)
                || string.IsNullOrWhiteSpace(content.SourceVideoId)
                || content.SourceStartSeconds < 0
                || (content.TargetStatCode != 내면StatCodes.Awareness
                    && content.TargetStatCode != 내면StatCodes.Resolve)
                || (content.GrantedRuleCode != 내면규칙Codes.BeginnerMind
                    && content.GrantedRuleCode != 내면규칙Codes.IntegratedProgress)
                || content.StatDelta != 1
                || content.SourceStableIds == null || content.SourceStableIds.Length == 0
                || content.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("EveningStudyContentInvalid");
        }

        private static void ValidateRecord(저녁학당학습기록Data record)
        {
            if (record == null || !StableDataId.IsValid(record.StableId) || record.Revision <= 0
                || record.StudiedOn == default || !StableDataId.IsValid(record.ContentStableId)
                || string.IsNullOrWhiteSpace(record.ReflectionText)
                || (record.GrantedRuleCode != 내면규칙Codes.BeginnerMind
                    && record.GrantedRuleCode != 내면규칙Codes.IntegratedProgress)
                || (record.StatCode != 내면StatCodes.Awareness
                    && record.StatCode != 내면StatCodes.Resolve)
                || record.StatDelta != 1
                || record.SourceStableIds == null || record.SourceStableIds.Length == 0
                || record.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("EveningStudyRecordInvalid");
        }
    }

    public sealed class 저녁학당SimulationEngine
    {
        private readonly 저녁학당SimulationValidator validator;

        public 저녁학당SimulationEngine(저녁학당SimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public 저녁학당학습Preview Preview(
            저녁학당SimulationSnapshot snapshot,
            string contentStableId)
        {
            validator.Validate(snapshot);
            if (snapshot.DayPhaseCode != 하루단계Codes.EveningStudy)
                throw new InvalidOperationException("EveningStudyNotAvailable");
            if (snapshot.StudyLedger.Any(value => SameDay(value.StudiedOn, snapshot.SimulationDate)))
                throw new InvalidOperationException("EveningStudyAlreadyCompletedToday");
            var content = snapshot.AvailableContents.SingleOrDefault(value => value.StableId == contentStableId)
                ?? throw new InvalidOperationException("EveningStudyContentUnknown:" + contentStableId);
            return new 저녁학당학습Preview
            {
                StableId = "evening-study-preview:" + StableSuffix(content.StableId) + ".r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                ContentStableId = content.StableId,
                ReflectionPrompt = content.ReflectionPrompt,
                TargetStatCode = content.TargetStatCode,
                StatBefore = ReadStat(snapshot.InnerState, content.TargetStatCode),
                StatAfter = ReadStat(snapshot.InnerState, content.TargetStatCode) + content.StatDelta,
                GrantedRuleCode = content.GrantedRuleCode,
                EffectiveSimulationDate = snapshot.SimulationDate.AddDays(1),
                RevealsUnknownsInNextDayPreviews = true,
                RequiresExplicitConfirmation = true,
            };
        }

        public 저녁학당학습Command Confirm(
            저녁학당SimulationSnapshot snapshot,
            저녁학당학습Preview preview,
            string reflectionText)
        {
            validator.Validate(snapshot);
            if (string.IsNullOrWhiteSpace(reflectionText))
                throw new InvalidOperationException("EveningStudyReflectionRequired");
            var expected = preview == null ? null : Preview(snapshot, preview.ContentStableId);
            if (preview == null || expected == null || preview.StableId != expected.StableId
                || preview.SnapshotStableId != expected.SnapshotStableId
                || preview.ExpectedDataRevision != expected.ExpectedDataRevision
                || preview.TargetStatCode != expected.TargetStatCode
                || preview.StatBefore != expected.StatBefore
                || preview.StatAfter != expected.StatAfter
                || preview.GrantedRuleCode != expected.GrantedRuleCode
                || preview.EffectiveSimulationDate != expected.EffectiveSimulationDate
                || !preview.RevealsUnknownsInNextDayPreviews
                || !preview.RequiresExplicitConfirmation)
                throw new InvalidOperationException("EveningStudyPreviewStaleOrInvalid");
            return new 저녁학당학습Command
            {
                StableId = "evening-study-command:" + StableSuffix(preview.ContentStableId) + ".r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                PreviewStableId = preview.StableId,
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                ContentStableId = preview.ContentStableId,
                ReflectionText = reflectionText.Trim(),
                SimulationTick = snapshot.DataRevision + 1,
            };
        }

        public 저녁학당SimulationSnapshot Tick(
            저녁학당SimulationSnapshot snapshot,
            저녁학당학습Command command)
        {
            validator.Validate(snapshot);
            if (command == null || command.SnapshotStableId != snapshot.StableId
                || command.ExpectedDataRevision != snapshot.DataRevision
                || command.SimulationTick != snapshot.DataRevision + 1)
                throw new InvalidOperationException("EveningStudyCommandStaleOrInvalid");
            var expected = Preview(snapshot, command.ContentStableId);
            if (command.PreviewStableId != expected.StableId
                || string.IsNullOrWhiteSpace(command.ReflectionText))
                throw new InvalidOperationException("EveningStudyCommandPreviewMismatch");

            var next = Clone(snapshot);
            next.DataRevision++;
            next.SimulationDate = expected.EffectiveSimulationDate;
            next.DayPhaseCode = 하루단계Codes.Day;
            WriteStat(next.InnerState, expected.TargetStatCode, expected.StatAfter);
            next.InnerState.ActiveRuleCodes = next.InnerState.ActiveRuleCodes
                .Concat(new[] { expected.GrantedRuleCode })
                .Distinct(StringComparer.Ordinal).ToArray();
            next.StudyLedger = next.StudyLedger.Concat(new[]
            {
                new 저녁학당학습기록Data
                {
                    StableId = "evening-study-record:" + StableSuffix(command.ContentStableId) + "."
                        + snapshot.SimulationDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    Revision = 1,
                    StudiedOn = snapshot.SimulationDate,
                    ContentStableId = command.ContentStableId,
                    ReflectionText = command.ReflectionText.Trim(),
                    GrantedRuleCode = expected.GrantedRuleCode,
                    StatCode = expected.TargetStatCode,
                    StatDelta = expected.StatAfter - expected.StatBefore,
                    SourceStableIds = new[] { command.ContentStableId, command.StableId },
                },
            }).ToArray();
            next.SourceStableIds = next.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            validator.Validate(next);
            return next;
        }

        private static bool SameDay(DateTimeOffset left, DateTimeOffset right)
            => left.Year == right.Year && left.DayOfYear == right.DayOfYear;

        private static string StableSuffix(string stableId)
            => stableId.Substring(stableId.IndexOf(':') + 1);

        private static int ReadStat(플레이어내면상태Snapshot state, string statCode)
            => statCode == 내면StatCodes.Awareness ? state.알아차림
                : statCode == 내면StatCodes.Resolve ? state.의지
                : throw new InvalidOperationException("EveningStudyTargetStatUnknown:" + statCode);

        private static void WriteStat(플레이어내면상태Snapshot state, string statCode, int value)
        {
            if (statCode == 내면StatCodes.Awareness) state.알아차림 = value;
            else if (statCode == 내면StatCodes.Resolve) state.의지 = value;
            else throw new InvalidOperationException("EveningStudyTargetStatUnknown:" + statCode);
        }

        private static 저녁학당SimulationSnapshot Clone(저녁학당SimulationSnapshot source)
            => new 저녁학당SimulationSnapshot
            {
                StableId = source.StableId,
                DataRevision = source.DataRevision,
                ModeCode = source.ModeCode,
                ScenarioStableId = source.ScenarioStableId,
                SimulationDate = source.SimulationDate,
                DayPhaseCode = source.DayPhaseCode,
                InnerState = new 플레이어내면상태Snapshot
                {
                    알아차림 = source.InnerState.알아차림,
                    명료함 = source.InnerState.명료함,
                    양심 = source.InnerState.양심,
                    조화 = source.InnerState.조화,
                    의지 = source.InnerState.의지,
                    통찰 = source.InnerState.통찰,
                    ActiveRuleCodes = source.InnerState.ActiveRuleCodes.ToArray(),
                },
                AvailableContents = source.AvailableContents.ToArray(),
                StudyLedger = source.StudyLedger.ToArray(),
                SourceStableIds = source.SourceStableIds.ToArray(),
            };
    }

    public static class 저녁학당SimulationFixture
    {
        public const string FoolContentStableId = "learning:hongik.fool.beginner-mind";

        public static 저녁학당SimulationSnapshot CreateFoolEvening()
            => new 저녁학당SimulationSnapshot
            {
                StableId = "evening-study-session:sim.fool.20260407",
                DataRevision = 1,
                ModeCode = "Simulation",
                ScenarioStableId = "scenario:evening-study.fool.v1",
                SimulationDate = new DateTimeOffset(2026, 4, 7, 21, 0, 0, TimeSpan.FromHours(9)),
                DayPhaseCode = 하루단계Codes.EveningStudy,
                InnerState = new 플레이어내면상태Snapshot(),
                AvailableContents = new[]
                {
                    new 저녁학당콘텐츠Snapshot
                    {
                        StableId = FoolContentStableId,
                        Revision = 4,
                        KindCode = 학습콘텐츠종류Codes.ArcanaAndVideo,
                        Title = "0. 바보 · 모를 뿐",
                        TeachingSummary = "바보의 핵심은 무분별한 마음, 곧 아는 체하지 않는 '모를 뿐'의 자세다.",
                        ReflectionPrompt = "지금 나는 무엇을 모르는가?",
                        CardStableId = "tarot-card:major-00",
                        KnowledgeNoteStableId = "knowledge-note:tarot-card-major-00",
                        SourceVideoId = "qo1tNkwSBVs",
                        SourceStartSeconds = 5339,
                        TargetStatCode = 내면StatCodes.Awareness,
                        GrantedRuleCode = 내면규칙Codes.BeginnerMind,
                        StatDelta = 1,
                        SourceStableIds = new[]
                        {
                            "youtube-video:qo1tnkwsbvs",
                            "knowledge-note:tarot-card-major-00",
                        },
                    },
                    new 저녁학당콘텐츠Snapshot
                    {
                        StableId = "learning:hongik.chariot.integrated-progress",
                        Revision = 1,
                        KindCode = 학습콘텐츠종류Codes.ArcanaAndVideo,
                        Title = "7. 전차 · 통합된 정진",
                        TeachingSummary = "전차는 지성·감성·음양, 사람의 지혜와 힘을 통합해 바른 방향으로 정진하는 긍정적 카드다.",
                        ReflectionPrompt = "오늘의 힘과 지혜를 어느 방향으로 통합할 것인가?",
                        CardStableId = "tarot-card:major-07",
                        KnowledgeNoteStableId = "knowledge-note:tarot-card-major-07",
                        SourceVideoId = "qo1tNkwSBVs",
                        SourceStartSeconds = 5900,
                        TargetStatCode = 내면StatCodes.Resolve,
                        GrantedRuleCode = 내면규칙Codes.IntegratedProgress,
                        StatDelta = 1,
                        SourceStableIds = new[]
                        {
                            "youtube-video:qo1tnkwsbvs",
                            "knowledge-note:tarot-card-major-07",
                        },
                    },
                },
                StudyLedger = Array.Empty<저녁학당학습기록Data>(),
                SourceStableIds = new[]
                {
                    "scenario-source:hongik-academy-tarot",
                    "knowledge-note:tarot-card-major-00",
                },
            };
    }
}
