using System.Net;
using System.Net.Mail;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Education;
using Ssalddel.Services.Community;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Education;

public interface I교육기관제출전송Service
{
    Task<교육기관제출전송결과> 전송Async(
        교육기관제출작업 작업,
        커뮤니티원장Dto 원장,
        CancellationToken cancellationToken);
}

public sealed record 교육기관제출전송결과(bool 성공, bool 설정필요, string? 오류)
{
    public static 교육기관제출전송결과 완료() => new(true, false, null);
    public static 교육기관제출전송결과 실패(string 오류) => new(false, false, 오류);
    public static 교육기관제출전송결과 설정대기(string 오류) => new(false, true, 오류);
}

public sealed class 교육기관제출전송Service : I교육기관제출전송Service
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<교육기관제출Options> _options;

    public 교육기관제출전송Service(HttpClient httpClient, IOptionsMonitor<교육기관제출Options> options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public Task<교육기관제출전송결과> 전송Async(
        교육기관제출작업 작업,
        커뮤니티원장Dto 원장,
        CancellationToken cancellationToken)
    {
        var payload = BuildPayload(원장, 작업.제출Id);
        return 작업.전송방식 switch
        {
            교육기관제출방식.이메일 => 이메일전송Async(작업, 원장, payload, cancellationToken),
            교육기관제출방식.Api => Api전송Async(작업, payload, cancellationToken),
            _ => Task.FromResult(교육기관제출전송결과.실패("자동 전송 대상이 아닌 제출 방식입니다."))
        };
    }

    private async Task<교육기관제출전송결과> 이메일전송Async(
        교육기관제출작업 작업,
        커뮤니티원장Dto 원장,
        object payload,
        CancellationToken cancellationToken)
    {
        var smtp = _options.CurrentValue.Smtp;
        if (string.IsNullOrWhiteSpace(smtp.Host) || string.IsNullOrWhiteSpace(smtp.FromAddress))
        {
            return 교육기관제출전송결과.설정대기("EducationSubmissions:Smtp 설정이 필요합니다.");
        }

        if (!TryMailAddress(작업.담당이메일, out var recipient, out var error))
        {
            return 교육기관제출전송결과.실패(error);
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(smtp.FromAddress, smtp.FromDisplayName, Encoding.UTF8),
                Subject = $"[현장 체험 활동 제출] {원장.제목}",
                SubjectEncoding = Encoding.UTF8,
                Body = JsonSerializer.Serialize(payload, JsonOptions),
                BodyEncoding = Encoding.UTF8
            };
            message.To.Add(recipient!);
            message.Headers.Add("X-Ssalddel-Submission-Id", 작업.제출Id);

            using var client = new SmtpClient(smtp.Host, smtp.Port)
            {
                EnableSsl = smtp.EnableSsl
            };
            if (!string.IsNullOrWhiteSpace(smtp.UserName))
            {
                client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);
            }

            await client.SendMailAsync(message, cancellationToken);
            return 교육기관제출전송결과.완료();
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            return 교육기관제출전송결과.실패(ex.Message);
        }
    }

    private async Task<교육기관제출전송결과> Api전송Async(
        교육기관제출작업 작업,
        object payload,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(작업.제출처Key)
            || !_options.CurrentValue.Api제출처.TryGetValue(작업.제출처Key, out var destination))
        {
            return 교육기관제출전송결과.설정대기("등록된 교육기관 API 제출처를 찾을 수 없습니다.");
        }

        if (!Uri.TryCreate(destination.Url, UriKind.Absolute, out var endpoint)
            || (endpoint.Scheme != Uri.UriSchemeHttps && !endpoint.IsLoopback))
        {
            return 교육기관제출전송결과.설정대기("교육기관 API 제출처는 HTTPS 주소여야 합니다.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };
            request.Headers.TryAddWithoutValidation("Idempotency-Key", 작업.제출Id);
            if (!string.IsNullOrWhiteSpace(destination.ApiKeyHeaderName)
                && !string.IsNullOrWhiteSpace(destination.ApiKey))
            {
                request.Headers.TryAddWithoutValidation(destination.ApiKeyHeaderName, destination.ApiKey);
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return 교육기관제출전송결과.완료();
            }

            return 교육기관제출전송결과.실패(
                $"교육기관 API가 {(int)response.StatusCode} 상태를 반환했습니다.");
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            return 교육기관제출전송결과.실패(ex.Message);
        }
    }

    private static object BuildPayload(커뮤니티원장Dto ledger, string submissionId)
    {
        var studentPlan = FindBlock(ledger, 현장체험활동원장상수.학생계획Block)?.Data;
        var activityPlan = FindBlock(ledger, 현장체험활동원장상수.활동계획Block)?.Data;
        var guardianApproval = FindBlock(ledger, 현장체험활동원장상수.보호자승인Block)?.Data;
        var activityRecords = ledger.블록목록
            .Where(x => x.BlockType == 현장체험활동원장상수.활동기록Block)
            .Select(x => x.Data)
            .ToArray();

        return new
        {
            제출Id = submissionId,
            원장Id = ledger.원장Id,
            ledger.제목,
            학생 = studentPlan,
            활동계획 = activityPlan,
            활동기록 = activityRecords,
            보호자승인 = guardianApproval,
            생성시각Utc = DateTime.UtcNow
        };
    }

    private static 커뮤니티원장블록Dto? FindBlock(커뮤니티원장Dto ledger, string blockType)
        => ledger.블록목록.FirstOrDefault(x => x.BlockType == blockType);

    private static bool TryMailAddress(string? value, out MailAddress? address, out string error)
    {
        try
        {
            address = string.IsNullOrWhiteSpace(value) ? null : new MailAddress(value.Trim());
            error = address is null ? "학교 담당 이메일이 필요합니다." : string.Empty;
            return address is not null;
        }
        catch (FormatException)
        {
            address = null;
            error = "학교 담당 이메일 형식이 올바르지 않습니다.";
            return false;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
}
