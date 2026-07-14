using System.Net.Mail;
using System.Text.Json;
using Hongdal.Contracts.Common.Education;
using Hongdal.Domain.Education;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Infrastructure.Security;

namespace Hongdal.Services.Education;

public interface I교육과정참여Service
{
    Task<교육과정신청Dto> 신청Async(교육과정신청요청 요청, string 신청자UserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<교육과정신청Dto>> 내신청목록조회Async(string 신청자UserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<교육과정신청Dto>> 신청목록조회Async(string? 과정코드, string? 상태, int 개수, CancellationToken cancellationToken);
    Task<교육과정신청Dto> 심사Async(long 신청Id, 교육과정신청심사요청 요청, string 심사자UserId, CancellationToken cancellationToken);
    Task 개인정보삭제Async(long 신청Id, string 요청자UserId, bool 관리자여부, CancellationToken cancellationToken);
    Task<교육과정진행현황Dto> 진행현황조회Async(long 등록Id, string 요청자UserId, bool 관리자여부, CancellationToken cancellationToken);
    Task<교육과정과제제출Dto> 과제제출Async(long 등록Id, 교육과정과제제출요청 요청, string 참여자UserId, CancellationToken cancellationToken);
    Task<교육과정과제제출Dto> 과제확인Async(long 제출Id, 교육과정과제확인요청 요청, string 확인자UserId, bool 관리자여부, CancellationToken cancellationToken);
    Task<교육과정진행현황Dto> 참석기록Async(long 등록Id, 교육과정참석기록요청 요청, string 기록자UserId, bool 관리자여부, CancellationToken cancellationToken);
}

public sealed class 교육과정참여Service : I교육과정참여Service
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HongdalContext _db;
    private readonly IPersonalDataEncryptionService _개인정보암호화;

    public 교육과정참여Service(
        HongdalContext db,
        IPersonalDataEncryptionService 개인정보암호화)
    {
        _db = db;
        _개인정보암호화 = 개인정보암호화;
    }

    public async Task<교육과정신청Dto> 신청Async(
        교육과정신청요청 요청,
        string 신청자UserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(요청);
        EnsureUser(신청자UserId);
        ValidateApplication(요청);

        var courseCode = 요청.과정코드.Trim().ToLowerInvariant();
        var course = await _db.교육과정
            .SingleOrDefaultAsync(x => x.과정코드 == courseCode && x.활성화여부, cancellationToken)
            ?? throw new InvalidOperationException("신청할 수 있는 교육과정을 찾을 수 없습니다.");

        var applicationForm = await _db.교육과정양식.SingleOrDefaultAsync(
            x => x.교육과정Id == course.Id &&
                 x.양식코드 == 교육과정양식코드.입교신청 &&
                 x.활성화여부,
            cancellationToken)
            ?? throw new InvalidOperationException("교육과정 입교 신청 양식을 찾을 수 없습니다.");
        var applicationFields = 교육과정양식검증기.필드정의역직렬화(applicationForm.필드정의Json);
        var formErrors = 교육과정양식검증기.답변검증(applicationFields, CreateApplicationAnswers(요청));
        if (formErrors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", formErrors));
        }

        var activeStates = new[]
        {
            교육과정신청상태.검토대기,
            교육과정신청상태.보류,
            교육과정신청상태.승인
        };
        var duplicate = await _db.교육과정신청.AnyAsync(
            x => x.교육과정Id == course.Id &&
                 x.신청자UserId == 신청자UserId &&
                 activeStates.Contains(x.상태),
            cancellationToken);
        if (duplicate)
        {
            throw new InvalidOperationException("이미 진행 중인 교육과정 신청이 있습니다.");
        }

        var now = DateTime.UtcNow;
        var application = new 교육과정신청
        {
            교육과정Id = course.Id,
            교육과정 = course,
            신청자UserId = 신청자UserId,
            이름암호문 = Protect(요청.이름.Trim()),
            별명암호문 = Protect(Clean(요청.별명)),
            이메일암호문 = Protect(요청.이메일.Trim()),
            전화번호암호문 = Protect(요청.전화번호.Trim()),
            성별암호문 = Protect(요청.성별.Trim()),
            출생연도암호문 = Protect(요청.출생연도.ToString()),
            거주국가암호문 = Protect(Clean(요청.거주국가)),
            회원가입확인 = 요청.회원가입확인,
            입교서약동의 = 요청.입교서약동의,
            개인정보수집이용동의 = 요청.개인정보수집이용동의,
            개인정보제3자제공동의 = 요청.개인정보제3자제공동의,
            개인정보동의버전 = 요청.개인정보동의버전.Trim(),
            제3자제공동의버전 = 요청.제3자제공동의버전.Trim(),
            동의일시Utc = now,
            상태 = 교육과정신청상태.검토대기,
            심사메모암호문 = Protect(string.Empty),
            신청일시Utc = now
        };

        _db.교육과정신청.Add(application);
        await _db.SaveChangesAsync(cancellationToken);
        return ToApplicationDto(application);
    }

    public async Task<IReadOnlyList<교육과정신청Dto>> 내신청목록조회Async(
        string 신청자UserId,
        CancellationToken cancellationToken)
    {
        EnsureUser(신청자UserId);
        var applications = await _db.교육과정신청
            .AsNoTracking()
            .Include(x => x.교육과정)
            .Include(x => x.등록)
            .Where(x => x.신청자UserId == 신청자UserId)
            .OrderByDescending(x => x.신청일시Utc)
            .ToListAsync(cancellationToken);
        return applications.Select(ToApplicationDto).ToList();
    }

    public async Task<IReadOnlyList<교육과정신청Dto>> 신청목록조회Async(
        string? 과정코드,
        string? 상태,
        int 개수,
        CancellationToken cancellationToken)
    {
        var take = Math.Clamp(개수, 1, 200);
        var query = _db.교육과정신청
            .AsNoTracking()
            .Include(x => x.교육과정)
            .Include(x => x.등록)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(과정코드))
        {
            var code = 과정코드.Trim().ToLowerInvariant();
            query = query.Where(x => x.교육과정 != null && x.교육과정.과정코드 == code);
        }
        if (!string.IsNullOrWhiteSpace(상태))
        {
            var status = 상태.Trim();
            query = query.Where(x => x.상태 == status);
        }

        var applications = await query
            .OrderByDescending(x => x.신청일시Utc)
            .Take(take)
            .ToListAsync(cancellationToken);
        return applications.Select(ToApplicationDto).ToList();
    }

    public async Task<교육과정신청Dto> 심사Async(
        long 신청Id,
        교육과정신청심사요청 요청,
        string 심사자UserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(요청);
        EnsureUser(심사자UserId);
        if (!교육과정신청상태.심사가능(요청.상태))
        {
            throw new InvalidOperationException("신청 심사 상태는 보류, 승인, 거절 중 하나여야 합니다.");
        }

        var application = await _db.교육과정신청
            .Include(x => x.교육과정)
            .Include(x => x.등록)
            .SingleOrDefaultAsync(x => x.Id == 신청Id, cancellationToken)
            ?? throw new InvalidOperationException("교육과정 신청을 찾을 수 없습니다.");

        if (application.등록 is not null && 요청.상태 != 교육과정신청상태.승인)
        {
            throw new InvalidOperationException("이미 등록된 신청은 승인 이외 상태로 변경할 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        application.상태 = 요청.상태;
        application.심사자UserId = 심사자UserId;
        application.심사메모암호문 = Protect(Clean(요청.심사메모));
        application.심사일시Utc = now;

        if (요청.상태 == 교육과정신청상태.승인 && application.등록 is null)
        {
            application.등록 = new 교육과정등록
            {
                교육과정Id = application.교육과정Id,
                참여자UserId = application.신청자UserId,
                담당멘토UserId = Clean(요청.담당멘토UserId),
                상태 = 교육과정등록상태.진행중,
                시작일시Utc = 요청.시작일시Utc ?? now,
                생성일시Utc = now
            };
        }
        else if (application.등록 is not null && !string.IsNullOrWhiteSpace(요청.담당멘토UserId))
        {
            application.등록.담당멘토UserId = 요청.담당멘토UserId.Trim();
        }

        await _db.SaveChangesAsync(cancellationToken);
        return ToApplicationDto(application);
    }

    public async Task 개인정보삭제Async(
        long 신청Id,
        string 요청자UserId,
        bool 관리자여부,
        CancellationToken cancellationToken)
    {
        EnsureUser(요청자UserId);
        var application = await _db.교육과정신청
            .SingleOrDefaultAsync(x => x.Id == 신청Id, cancellationToken)
            ?? throw new InvalidOperationException("교육과정 신청을 찾을 수 없습니다.");
        if (!관리자여부 && application.신청자UserId != 요청자UserId)
        {
            throw new InvalidOperationException("이 교육과정 신청의 개인정보를 삭제할 권한이 없습니다.");
        }

        var empty = Protect(string.Empty);
        application.이름암호문 = empty;
        application.별명암호문 = empty;
        application.이메일암호문 = empty;
        application.전화번호암호문 = empty;
        application.성별암호문 = empty;
        application.출생연도암호문 = empty;
        application.거주국가암호문 = empty;
        application.심사메모암호문 = empty;
        application.개인정보삭제일시Utc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<교육과정진행현황Dto> 진행현황조회Async(
        long 등록Id,
        string 요청자UserId,
        bool 관리자여부,
        CancellationToken cancellationToken)
    {
        EnsureUser(요청자UserId);
        var enrollment = await LoadEnrollmentAsync(등록Id, cancellationToken);
        if (!관리자여부 && enrollment.참여자UserId != 요청자UserId && enrollment.담당멘토UserId != 요청자UserId)
        {
            throw new InvalidOperationException("이 교육과정 진행현황을 조회할 권한이 없습니다.");
        }
        return ToProgress(enrollment);
    }

    public async Task<교육과정과제제출Dto> 과제제출Async(
        long 등록Id,
        교육과정과제제출요청 요청,
        string 참여자UserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(요청);
        EnsureUser(참여자UserId);
        var enrollment = await _db.교육과정등록
            .Include(x => x.교육과정)
            .SingleOrDefaultAsync(x => x.Id == 등록Id, cancellationToken)
            ?? throw new InvalidOperationException("교육과정 등록을 찾을 수 없습니다.");
        if (enrollment.참여자UserId != 참여자UserId)
        {
            throw new InvalidOperationException("이 교육과정에 과제를 제출할 권한이 없습니다.");
        }
        if (enrollment.상태 != 교육과정등록상태.진행중)
        {
            throw new InvalidOperationException("현재 상태에서는 교육과정 과제를 제출할 수 없습니다.");
        }

        var formCode = 요청.양식코드?.Trim() ?? string.Empty;
        var periodKey = 요청.제출기간Key?.Trim() ?? string.Empty;
        if (periodKey.Length is < 1 or > 100)
        {
            throw new InvalidOperationException("제출기간Key는 1~100자로 입력해야 합니다.");
        }

        var form = await _db.교육과정양식.SingleOrDefaultAsync(
            x => x.교육과정Id == enrollment.교육과정Id &&
                 x.양식코드 == formCode &&
                 x.활성화여부,
            cancellationToken)
            ?? throw new InvalidOperationException("제출할 교육과정 양식을 찾을 수 없습니다.");
        if (form.양식코드 == 교육과정양식코드.입교신청)
        {
            throw new InvalidOperationException("입교 신청서는 교육과정 신청 API로 제출해야 합니다.");
        }

        var fields = 교육과정양식검증기.필드정의역직렬화(form.필드정의Json);
        var errors = 교육과정양식검증기.답변검증(fields, 요청.답변);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", errors));
        }

        var submission = await _db.교육과정과제제출.SingleOrDefaultAsync(
            x => x.교육과정등록Id == 등록Id &&
                 x.교육과정양식Id == form.Id &&
                 x.제출기간Key == periodKey,
            cancellationToken);
        if (submission?.상태 == 교육과정제출상태.확인)
        {
            throw new InvalidOperationException("이미 확인된 과제는 다시 제출할 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        if (submission is null)
        {
            submission = new 교육과정과제제출
            {
                교육과정등록Id = enrollment.Id,
                교육과정양식Id = form.Id,
                제출기간Key = periodKey
            };
            _db.교육과정과제제출.Add(submission);
        }

        submission.교육과정양식 = form;
        submission.답변암호문 = Protect(JsonSerializer.Serialize(요청.답변, JsonOptions));
        submission.상태 = 교육과정제출상태.제출;
        submission.확인자UserId = null;
        submission.확인메모암호문 = Protect(string.Empty);
        submission.제출일시Utc = now;
        submission.확인일시Utc = null;
        await _db.SaveChangesAsync(cancellationToken);
        return ToSubmissionDto(submission);
    }

    public async Task<교육과정과제제출Dto> 과제확인Async(
        long 제출Id,
        교육과정과제확인요청 요청,
        string 확인자UserId,
        bool 관리자여부,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(요청);
        EnsureUser(확인자UserId);
        if (요청.상태 is not 교육과정제출상태.확인 and not 교육과정제출상태.보완요청)
        {
            throw new InvalidOperationException("과제 확인 상태는 확인 또는 보완요청이어야 합니다.");
        }

        var submission = await _db.교육과정과제제출
            .Include(x => x.교육과정양식)
            .Include(x => x.교육과정등록)
            .SingleOrDefaultAsync(x => x.Id == 제출Id, cancellationToken)
            ?? throw new InvalidOperationException("교육과정 과제 제출을 찾을 수 없습니다.");
        EnsureMentor(submission.교육과정등록, 확인자UserId, 관리자여부);

        submission.상태 = 요청.상태;
        submission.확인자UserId = 확인자UserId;
        submission.확인메모암호문 = Protect(Clean(요청.확인메모));
        submission.확인일시Utc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ToSubmissionDto(submission);
    }

    public async Task<교육과정진행현황Dto> 참석기록Async(
        long 등록Id,
        교육과정참석기록요청 요청,
        string 기록자UserId,
        bool 관리자여부,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(요청);
        EnsureUser(기록자UserId);
        var enrollment = await LoadEnrollmentAsync(등록Id, cancellationToken);
        EnsureMentor(enrollment, 기록자UserId, 관리자여부);

        var subjectCode = 요청.과목코드?.Trim() ?? string.Empty;
        var sessionKey = 요청.회차Key?.Trim() ?? string.Empty;
        if (sessionKey.Length is < 1 or > 100)
        {
            throw new InvalidOperationException("회차Key는 1~100자로 입력해야 합니다.");
        }
        if (요청.수업일시Utc == default)
        {
            throw new InvalidOperationException("수업일시Utc가 필요합니다.");
        }

        var subject = enrollment.교육과정?.과목목록.SingleOrDefault(x => x.과목코드 == subjectCode)
            ?? throw new InvalidOperationException("교육과정 과목을 찾을 수 없습니다.");
        var attendance = enrollment.참석목록.SingleOrDefault(
            x => x.교육과정과목Id == subject.Id && x.회차Key == sessionKey);
        if (attendance is null)
        {
            attendance = new 교육과정참석기록
            {
                교육과정등록Id = enrollment.Id,
                교육과정과목Id = subject.Id,
                회차Key = sessionKey
            };
            enrollment.참석목록.Add(attendance);
        }

        attendance.회차명 = string.IsNullOrWhiteSpace(요청.회차명) ? sessionKey : 요청.회차명.Trim();
        attendance.수업일시Utc = 요청.수업일시Utc;
        attendance.참석여부 = 요청.참석여부;
        attendance.기록자UserId = 기록자UserId;
        attendance.기록일시Utc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ToProgress(enrollment);
    }

    private async Task<교육과정등록> LoadEnrollmentAsync(long enrollmentId, CancellationToken cancellationToken)
        => await _db.교육과정등록
            .Include(x => x.교육과정).ThenInclude(x => x!.과목목록)
            .Include(x => x.교육과정).ThenInclude(x => x!.양식목록)
            .Include(x => x.참석목록)
            .Include(x => x.제출목록)
            .SingleOrDefaultAsync(x => x.Id == enrollmentId, cancellationToken)
            ?? throw new InvalidOperationException("교육과정 등록을 찾을 수 없습니다.");

    private 교육과정신청Dto ToApplicationDto(교육과정신청 application)
        => new()
        {
            신청Id = application.Id,
            과정코드 = application.교육과정?.과정코드 ?? string.Empty,
            과정명 = application.교육과정?.과정명 ?? string.Empty,
            신청자UserId = application.신청자UserId,
            이름 = Unprotect(application.이름암호문),
            별명 = Unprotect(application.별명암호문),
            이메일 = Unprotect(application.이메일암호문),
            전화번호 = Unprotect(application.전화번호암호문),
            성별 = Unprotect(application.성별암호문),
            출생연도 = int.TryParse(Unprotect(application.출생연도암호문), out var year) ? year : 0,
            거주국가 = Unprotect(application.거주국가암호문),
            회원가입확인 = application.회원가입확인,
            입교서약동의 = application.입교서약동의,
            개인정보수집이용동의 = application.개인정보수집이용동의,
            개인정보제3자제공동의 = application.개인정보제3자제공동의,
            상태 = application.상태,
            심사자UserId = application.심사자UserId,
            심사메모 = Unprotect(application.심사메모암호문),
            신청일시Utc = application.신청일시Utc,
            심사일시Utc = application.심사일시Utc,
            등록Id = application.등록?.Id
        };

    private 교육과정과제제출Dto ToSubmissionDto(교육과정과제제출 submission)
    {
        var answers = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            Unprotect(submission.답변암호문),
            JsonOptions) ?? new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        return new 교육과정과제제출Dto
        {
            제출Id = submission.Id,
            양식코드 = submission.교육과정양식?.양식코드 ?? string.Empty,
            양식명 = submission.교육과정양식?.양식명 ?? string.Empty,
            제출기간Key = submission.제출기간Key,
            답변 = answers,
            상태 = submission.상태,
            확인자UserId = submission.확인자UserId,
            확인메모 = Unprotect(submission.확인메모암호문),
            제출일시Utc = submission.제출일시Utc,
            확인일시Utc = submission.확인일시Utc
        };
    }

    private static 교육과정진행현황Dto ToProgress(교육과정등록 enrollment)
    {
        var course = enrollment.교육과정
            ?? throw new InvalidOperationException("교육과정 정보를 찾을 수 없습니다.");
        return new 교육과정진행현황Dto
        {
            등록Id = enrollment.Id,
            과정코드 = course.과정코드,
            과정명 = course.과정명,
            상태 = enrollment.상태,
            담당멘토UserId = enrollment.담당멘토UserId,
            시작일시Utc = enrollment.시작일시Utc,
            과목목록 = course.과목목록
                .OrderBy(x => x.표시순서)
                .Select(x =>
                {
                    var count = enrollment.참석목록.Count(a => a.교육과정과목Id == x.Id && a.참석여부);
                    return new 교육과정과목진행Dto
                    {
                        과목코드 = x.과목코드,
                        과목명 = x.과목명,
                        참석횟수 = count,
                        최소참석횟수 = x.최소참석횟수,
                        충족여부 = count >= x.최소참석횟수
                    };
                })
                .ToList(),
            양식목록 = course.양식목록
                .Where(x => x.활성화여부 && x.양식코드 != 교육과정양식코드.입교신청)
                .Select(x =>
                {
                    var count = enrollment.제출목록.Count(s =>
                        s.교육과정양식Id == x.Id && s.상태 != 교육과정제출상태.보완요청);
                    return new 교육과정양식진행Dto
                    {
                        양식코드 = x.양식코드,
                        양식명 = x.양식명,
                        제출횟수 = count,
                        최소제출횟수 = x.최소제출횟수,
                        충족여부 = count >= x.최소제출횟수
                    };
                })
                .ToList()
        };
    }

    private static void ValidateApplication(교육과정신청요청 request)
    {
        if (string.IsNullOrWhiteSpace(request.과정코드))
        {
            throw new InvalidOperationException("과정코드가 필요합니다.");
        }
        if (string.IsNullOrWhiteSpace(request.이름) || request.이름.Length > 100)
        {
            throw new InvalidOperationException("이름은 1~100자로 입력해야 합니다.");
        }
        if (!MailAddress.TryCreate(request.이메일, out _))
        {
            throw new InvalidOperationException("올바른 이메일 주소가 필요합니다.");
        }
        if (string.IsNullOrWhiteSpace(request.전화번호) || request.전화번호.Count(char.IsDigit) < 8)
        {
            throw new InvalidOperationException("올바른 전화번호가 필요합니다.");
        }
        if (string.IsNullOrWhiteSpace(request.성별) || request.성별.Length > 20)
        {
            throw new InvalidOperationException("성별 입력값이 필요합니다.");
        }
        var currentYear = DateTime.UtcNow.Year;
        if (request.출생연도 is < 1900 || request.출생연도 > currentYear)
        {
            throw new InvalidOperationException("출생연도가 올바르지 않습니다.");
        }
        if (string.IsNullOrWhiteSpace(request.개인정보동의버전) || string.IsNullOrWhiteSpace(request.제3자제공동의버전))
        {
            throw new InvalidOperationException("개인정보 동의문 버전이 필요합니다.");
        }
    }

    private static Dictionary<string, JsonElement> CreateApplicationAnswers(교육과정신청요청 request)
        => new(StringComparer.Ordinal)
        {
            ["회원가입확인"] = JsonSerializer.SerializeToElement(request.회원가입확인),
            ["이름"] = JsonSerializer.SerializeToElement(request.이름),
            ["별명"] = JsonSerializer.SerializeToElement(request.별명 ?? string.Empty),
            ["이메일"] = JsonSerializer.SerializeToElement(request.이메일),
            ["전화번호"] = JsonSerializer.SerializeToElement(request.전화번호),
            ["성별"] = JsonSerializer.SerializeToElement(request.성별),
            ["출생연도"] = JsonSerializer.SerializeToElement(request.출생연도),
            ["거주국가"] = JsonSerializer.SerializeToElement(request.거주국가 ?? string.Empty),
            ["입교서약동의"] = JsonSerializer.SerializeToElement(request.입교서약동의),
            ["개인정보수집이용동의"] = JsonSerializer.SerializeToElement(request.개인정보수집이용동의),
            ["개인정보제3자제공동의"] = JsonSerializer.SerializeToElement(request.개인정보제3자제공동의)
        };

    private static void EnsureUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("사용자 인증 정보가 필요합니다.");
        }
    }

    private static void EnsureMentor(교육과정등록? enrollment, string userId, bool administrator)
    {
        if (enrollment is null)
        {
            throw new InvalidOperationException("교육과정 등록을 찾을 수 없습니다.");
        }
        if (!administrator && enrollment.담당멘토UserId != userId)
        {
            throw new InvalidOperationException("이 교육과정을 관리할 권한이 없습니다.");
        }
    }

    private string Protect(string? value)
        => _개인정보암호화.Protect(value ?? string.Empty) ?? string.Empty;

    private string Unprotect(string? value)
        => _개인정보암호화.Unprotect(value ?? string.Empty) ?? string.Empty;

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
