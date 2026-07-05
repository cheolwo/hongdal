using Microsoft.Extensions.Logging;
using System.Text.Json;
using 홍달.Data;
using 홍달.도메인.설정;
using 홍달.Services.Options;

namespace Hongdal.Application.CommandProcessing;

public sealed class Command알림의도Processor : ICommand후처리Processor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HongdalContext _db;
    private readonly ILogger<Command알림의도Processor> _logger;

    public Command알림의도Processor(HongdalContext db, ILogger<Command알림의도Processor> logger)
    {
        _db = db;
        _logger = logger;
    }

    public string Name => "Notification";

    public bool CanProcess(CommandProcessingRule rule) => rule.SmsEnabled.GetValueOrDefault() || rule.SnsEnabled.GetValueOrDefault() || rule.PushEnabled.GetValueOrDefault();

    public async Task ProcessAsync(Command후처리Context context, CancellationToken cancellationToken)
    {
        var payloadJson = JsonSerializer.Serialize(new
        {
            context.CommandName,
            context.Rule.EventName,
            context.Rule.Target,
            context.TraceId,
            context.OccurredAt,
            Request = context.Request,
            Response = context.Response
        }, JsonOptions);

        if (context.Rule.SmsEnabled.GetValueOrDefault())
        {
            AddOutbox(context, Command기능명.Sms, payloadJson);
            _logger.LogInformation(
                "Command 후처리 SMS Intent CommandName={CommandName} EventName={EventName} Target={Target} TraceId={TraceId} OccurredAt={OccurredAt}",
                context.CommandName,
                context.Rule.EventName,
                context.Rule.Target,
                context.TraceId,
                context.OccurredAt);
        }

        if (context.Rule.SnsEnabled.GetValueOrDefault())
        {
            AddOutbox(context, Command기능명.Sns, payloadJson);
            _logger.LogInformation(
                "Command 후처리 SNS Intent CommandName={CommandName} EventName={EventName} Target={Target} TraceId={TraceId} OccurredAt={OccurredAt}",
                context.CommandName,
                context.Rule.EventName,
                context.Rule.Target,
                context.TraceId,
                context.OccurredAt);
        }

        if (context.Rule.PushEnabled.GetValueOrDefault())
        {
            AddOutbox(context, Command기능명.Push, payloadJson);
            _logger.LogInformation(
                "Command 후처리 Push Intent CommandName={CommandName} EventName={EventName} Target={Target} TraceId={TraceId} OccurredAt={OccurredAt}",
                context.CommandName,
                context.Rule.EventName,
                context.Rule.Target,
                context.TraceId,
                context.OccurredAt);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private void AddOutbox(Command후처리Context context, string featureName, string payloadJson)
    {
        _db.Command알림Outbox.Add(new Command알림Outbox
        {
            CommandName = context.CommandName,
            EventName = context.Rule.EventName,
            FeatureName = featureName,
            Target = context.Rule.Target,
            PayloadJson = payloadJson,
            Status = "Pending",
            TraceId = context.TraceId,
            CreatedAt = context.OccurredAt,
            UpdatedAt = context.OccurredAt
        });
    }
}
