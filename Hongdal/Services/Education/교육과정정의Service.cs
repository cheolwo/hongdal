using System.Text.RegularExpressions;
using Hongdal.Contracts.Common.Education;
using Hongdal.Domain.Education;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Services.Education;

public interface I교육과정정의Service
{
    Task<IReadOnlyList<교육과정목록항목Dto>> 목록조회Async(bool 활성과정만, CancellationToken cancellationToken);
    Task<교육과정상세Dto> 상세조회Async(string 과정코드, bool 활성과정만, CancellationToken cancellationToken);
    Task<교육과정상세Dto> 저장Async(string 과정코드, 교육과정관리요청 요청, CancellationToken cancellationToken);
    Task<교육과정상세Dto> 홍익학당온라인신사과정초안등록Async(CancellationToken cancellationToken);
}

public sealed partial class 교육과정정의Service : I교육과정정의Service
{
    public const string 홍익학당온라인신사과정코드 = "hongik-academy-shinsa-online";
    public const string 홍익학당과정Url = "https://hihd.imweb.me/mentor002";
    public const string 홍익학당신청서Url = "https://docs.google.com/forms/d/e/1FAIpQLScip-w28uexEyFyTM2_WuKajREh7J_pNbdvhGFV4yGD0_Om-A/viewform";

    private readonly HongdalContext _db;

    public 교육과정정의Service(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<교육과정목록항목Dto>> 목록조회Async(
        bool 활성과정만,
        CancellationToken cancellationToken)
    {
        var query = _db.교육과정.AsNoTracking();
        if (활성과정만)
        {
            query = query.Where(x => x.활성화여부);
        }

        return await query
            .OrderBy(x => x.과정명)
            .Select(x => new 교육과정목록항목Dto
            {
                과정코드 = x.과정코드,
                과정명 = x.과정명,
                설명 = x.설명,
                운영방식 = x.운영방식,
                최소이수개월 = x.최소이수개월,
                활성화여부 = x.활성화여부,
                출처Url = x.출처Url
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<교육과정상세Dto> 상세조회Async(
        string 과정코드,
        bool 활성과정만,
        CancellationToken cancellationToken)
    {
        var normalizedCode = NormalizeCode(과정코드);
        var query = _db.교육과정
            .AsNoTracking()
            .Include(x => x.과목목록)
            .Include(x => x.양식목록)
            .Where(x => x.과정코드 == normalizedCode);

        if (활성과정만)
        {
            query = query.Where(x => x.활성화여부);
        }

        var course = await query.SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("교육과정을 찾을 수 없습니다.");

        return ToDetail(course, 활성과정만);
    }

    public async Task<교육과정상세Dto> 저장Async(
        string 과정코드,
        교육과정관리요청 요청,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(요청);
        var normalizedCode = NormalizeCode(과정코드);
        ValidateRequest(요청);

        var course = await _db.교육과정
            .Include(x => x.과목목록)
            .Include(x => x.양식목록)
            .SingleOrDefaultAsync(x => x.과정코드 == normalizedCode, cancellationToken);

        var now = DateTime.UtcNow;
        if (course is null)
        {
            course = new 교육과정
            {
                과정코드 = normalizedCode,
                생성일시Utc = now
            };
            _db.교육과정.Add(course);
        }

        course.과정명 = 요청.과정명.Trim();
        course.설명 = 요청.설명.Trim();
        course.운영방식 = 요청.운영방식.Trim();
        course.최소이수개월 = 요청.최소이수개월;
        course.활성화여부 = 요청.활성화여부;
        course.출처Url = Clean(요청.출처Url);
        course.수정일시Utc = now;

        foreach (var subjectRequest in 요청.과목목록)
        {
            var subjectCode = NormalizeChildCode(subjectRequest.과목코드, "과목코드");
            var subject = course.과목목록.SingleOrDefault(x => x.과목코드 == subjectCode);
            if (subject is null)
            {
                subject = new 교육과정과목 { 과목코드 = subjectCode };
                course.과목목록.Add(subject);
            }

            subject.과목명 = subjectRequest.과목명.Trim();
            subject.표시순서 = subjectRequest.표시순서;
            subject.최소참석횟수 = subjectRequest.최소참석횟수;
        }

        foreach (var formRequest in 요청.양식목록)
        {
            var formCode = NormalizeChildCode(formRequest.양식코드, "양식코드");
            var form = course.양식목록.SingleOrDefault(x => x.양식코드 == formCode);
            if (form is null)
            {
                form = new 교육과정양식
                {
                    양식코드 = formCode,
                    생성일시Utc = now
                };
                course.양식목록.Add(form);
            }

            form.양식명 = formRequest.양식명.Trim();
            form.목적 = formRequest.목적.Trim();
            form.버전 = formRequest.버전.Trim();
            form.제출주기 = formRequest.제출주기.Trim();
            form.최소제출횟수 = formRequest.최소제출횟수;
            form.필수여부 = formRequest.필수여부;
            form.활성화여부 = formRequest.활성화여부;
            form.필드정의Json = 교육과정양식검증기.필드정의직렬화(formRequest.필드목록);
            form.출처Url = Clean(formRequest.출처Url);
            form.수정일시Utc = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return ToDetail(course, false);
    }

    public Task<교육과정상세Dto> 홍익학당온라인신사과정초안등록Async(CancellationToken cancellationToken)
        => 저장Async(홍익학당온라인신사과정코드, CreateHongikAcademyDraft(), cancellationToken);

    public static 교육과정관리요청 CreateHongikAcademyDraft()
    {
        return new 교육과정관리요청
        {
            과정명 = "홍익학당 온라인 신사과정",
            설명 = "공개된 홍익학당 온라인 신사과정 안내와 입교 신청서 및 과제 양식을 토대로 구성한 관리 초안입니다.",
            운영방식 = "온라인 ZOOM",
            최소이수개월 = 3,
            활성화여부 = true,
            출처Url = 홍익학당과정Url,
            과목목록 =
            [
                Subject("참나각성", "참나각성", 1, 3),
                Subject("양심성찰", "양심성찰", 2, 3),
                Subject("호흡수련", "호흡수련", 3, 3),
                Subject("독서스터디", "독서스터디", 4, 3)
            ],
            양식목록 =
            [
                CreateEntryApplicationForm(),
                CreateTrainingExperienceForm(),
                CreateConsultationAssignmentForm()
            ]
        };
    }

    private static 교육과정양식관리요청 CreateEntryApplicationForm()
        => new()
        {
            양식코드 = 교육과정양식코드.입교신청,
            양식명 = "교육과정 입교 신청서",
            목적 = "회원 여부와 연락 정보, 입교서약 및 개인정보 동의를 확인합니다.",
            버전 = "2026-07-13",
            제출주기 = "입교 시 1회",
            최소제출횟수 = 1,
            필수여부 = true,
            출처Url = 홍익학당신청서Url,
            필드목록 =
            [
                Field("회원가입확인", "흥여회 가입 여부", 교육과정양식필드유형.참거짓, true, 1, "입교 자격", trueRequired: true),
                Field("이름", "이름", 교육과정양식필드유형.짧은글, true, 2, "신청자 정보", 100),
                Field("별명", "별명", 교육과정양식필드유형.짧은글, false, 3, "신청자 정보", 100),
                Field("이메일", "이메일", 교육과정양식필드유형.이메일, true, 4, "신청자 정보", 320),
                Field("전화번호", "전화번호", 교육과정양식필드유형.전화번호, true, 5, "신청자 정보", 50),
                Choice("성별", "성별", true, 6, ["남", "여"]),
                Field("출생연도", "출생연도", 교육과정양식필드유형.숫자, true, 7, "신청자 정보"),
                Field("거주국가", "현재 거주하는 국가", 교육과정양식필드유형.짧은글, false, 8, "해외 거주자", 100),
                Field("입교서약동의", "교육과정 입교서약서 동의", 교육과정양식필드유형.참거짓, true, 9, "동의", trueRequired: true),
                Field("개인정보수집이용동의", "개인정보 수집 및 이용 동의", 교육과정양식필드유형.참거짓, true, 10, "동의", trueRequired: true),
                Field("개인정보제3자제공동의", "개인정보 제3자 제공 동의", 교육과정양식필드유형.참거짓, true, 11, "동의", trueRequired: true)
            ]
        };

    private static 교육과정양식관리요청 CreateTrainingExperienceForm()
        => new()
        {
            양식코드 = 교육과정양식코드.수련체험기,
            양식명 = "수련체험기",
            목적 = "참나각성, 호흡수련, 양심성찰을 일상에서 어떻게 수련하고 적용했는지 기록합니다.",
            버전 = "2022-07-23",
            제출주기 = "매월 1회",
            최소제출횟수 = 3,
            필수여부 = true,
            출처Url = 홍익학당과정Url,
            필드목록 =
            [
                Field("참나각성하루수련시간", "참나각성 하루 수련시간", 교육과정양식필드유형.짧은글, false, 1, "참나각성", 100),
                Field("참나각성내용", "참나각성 수련 내용", 교육과정양식필드유형.긴글, false, 2, "참나각성"),
                Field("현재호흡초수", "현재 호흡 초수", 교육과정양식필드유형.숫자, false, 3, "호흡수련"),
                Field("호흡하루수련시간", "호흡 하루 수련시간", 교육과정양식필드유형.짧은글, false, 4, "호흡수련", 100),
                Field("호흡수련내용", "호흡수련 내용", 교육과정양식필드유형.긴글, false, 5, "호흡수련"),
                Field("양심성찰사안", "양심성찰을 적용한 사안", 교육과정양식필드유형.긴글, true, 6, "양심성찰"),
                Field("몰입현재", "지금 이 순간 깨어있는가", 교육과정양식필드유형.긴글, false, 7, "몰입"),
                Field("몰입당시", "당시에는 깨어있었는가", 교육과정양식필드유형.긴글, false, 8, "몰입"),
                Field("상대방원하는것", "상대방이 원하는 것", 교육과정양식필드유형.긴글, false, 9, "사랑"),
                Field("상대방두려운것", "상대방이 두려워하고 싫어하는 것", 교육과정양식필드유형.긴글, false, 10, "사랑"),
                Field("나의원하는것", "내가 원하는 것", 교육과정양식필드유형.긴글, false, 11, "사랑"),
                Field("나의두려운것", "내가 두려워하고 싫어하는 것", 교육과정양식필드유형.긴글, false, 12, "사랑"),
                Field("부당하게피해준부분", "상대방에게 부당하게 피해 준 부분", 교육과정양식필드유형.긴글, false, 13, "정의"),
                Field("부당하게피해받은부분", "상대방에게 부당하게 피해 받은 부분", 교육과정양식필드유형.긴글, false, 14, "정의"),
                Field("상황수용", "처한 상황을 있는 그대로 진심으로 수용했는가", 교육과정양식필드유형.긴글, false, 15, "예절"),
                Field("겸손과조화", "생각과 언행이 겸손하며 상황과 조화를 이루었는가", 교육과정양식필드유형.긴글, false, 16, "예절"),
                Field("성실", "양심의 인도를 따르는 데 최선의 노력을 기울였는가", 교육과정양식필드유형.긴글, false, 17, "성실"),
                Field("지혜", "나의 선택과 판단은 찜찜함 없이 자명한가", 교육과정양식필드유형.긴글, false, 18, "지혜"),
                Field("결론", "결론", 교육과정양식필드유형.긴글, true, 19, "정리")
            ]
        };

    private static 교육과정양식관리요청 CreateConsultationAssignmentForm()
        => new()
        {
            양식코드 = 교육과정양식코드.상담과제,
            양식명 = "상담과제",
            목적 = "상담 전에 아공, 법공, 구공의 진리를 필기하여 제출합니다.",
            버전 = "2021-04-29-v0.6",
            제출주기 = "상담 시",
            최소제출횟수 = 0,
            필수여부 = false,
            출처Url = 홍익학당과정Url,
            필드목록 =
            [
                Field("아공필기", "아공 필기", 교육과정양식필드유형.긴글, true, 1, "아공", 10_000),
                Field("법공필기", "법공 필기", 교육과정양식필드유형.긴글, true, 2, "법공", 10_000),
                Field("구공필기", "구공 필기", 교육과정양식필드유형.긴글, true, 3, "구공", 10_000)
            ]
        };

    private static 교육과정과목관리요청 Subject(
        string code,
        string name,
        int order,
        int minimumAttendance)
        => new()
        {
            과목코드 = code,
            과목명 = name,
            표시순서 = order,
            최소참석횟수 = minimumAttendance
        };

    private static 교육과정양식필드Dto Field(
        string key,
        string label,
        string type,
        bool required,
        int order,
        string section,
        int maxLength = 2000,
        bool trueRequired = false)
        => new()
        {
            Key = key,
            라벨 = label,
            유형 = type,
            필수여부 = required,
            참값필수여부 = trueRequired,
            표시순서 = order,
            섹션 = section,
            최대길이 = maxLength
        };

    private static 교육과정양식필드Dto Choice(
        string key,
        string label,
        bool required,
        int order,
        IReadOnlyList<string> choices)
        => new()
        {
            Key = key,
            라벨 = label,
            유형 = 교육과정양식필드유형.단일선택,
            필수여부 = required,
            표시순서 = order,
            섹션 = "신청자 정보",
            최대길이 = 20,
            선택목록 = choices
        };

    private static 교육과정상세Dto ToDetail(교육과정 course, bool activeFormsOnly)
        => new()
        {
            과정코드 = course.과정코드,
            과정명 = course.과정명,
            설명 = course.설명,
            운영방식 = course.운영방식,
            최소이수개월 = course.최소이수개월,
            활성화여부 = course.활성화여부,
            출처Url = course.출처Url,
            과목목록 = course.과목목록
                .OrderBy(x => x.표시순서)
                .Select(x => new 교육과정과목Dto
                {
                    과목코드 = x.과목코드,
                    과목명 = x.과목명,
                    표시순서 = x.표시순서,
                    최소참석횟수 = x.최소참석횟수
                })
                .ToList(),
            양식목록 = course.양식목록
                .Where(x => !activeFormsOnly || x.활성화여부)
                .OrderBy(x => x.Id)
                .Select(x => new 교육과정양식Dto
                {
                    양식코드 = x.양식코드,
                    양식명 = x.양식명,
                    목적 = x.목적,
                    버전 = x.버전,
                    제출주기 = x.제출주기,
                    최소제출횟수 = x.최소제출횟수,
                    필수여부 = x.필수여부,
                    활성화여부 = x.활성화여부,
                    출처Url = x.출처Url,
                    필드목록 = 교육과정양식검증기.필드정의역직렬화(x.필드정의Json)
                })
                .ToList()
        };

    private static void ValidateRequest(교육과정관리요청 request)
    {
        if (string.IsNullOrWhiteSpace(request.과정명) || request.과정명.Length > 200)
        {
            throw new InvalidOperationException("과정명은 1~200자로 입력해야 합니다.");
        }
        if (string.IsNullOrWhiteSpace(request.운영방식) || request.운영방식.Length > 100)
        {
            throw new InvalidOperationException("운영방식은 1~100자로 입력해야 합니다.");
        }
        if (request.최소이수개월 is < 0 or > 120)
        {
            throw new InvalidOperationException("최소이수개월은 0~120 사이여야 합니다.");
        }
        if (request.과목목록.GroupBy(x => x.과목코드?.Trim() ?? string.Empty, StringComparer.Ordinal).Any(x => x.Count() > 1))
        {
            throw new InvalidOperationException("과목코드는 과정 안에서 중복될 수 없습니다.");
        }
        if (request.양식목록.GroupBy(x => x.양식코드?.Trim() ?? string.Empty, StringComparer.Ordinal).Any(x => x.Count() > 1))
        {
            throw new InvalidOperationException("양식코드는 과정 안에서 중복될 수 없습니다.");
        }

        foreach (var subject in request.과목목록)
        {
            NormalizeChildCode(subject.과목코드, "과목코드");
            if (string.IsNullOrWhiteSpace(subject.과목명) || subject.과목명.Length > 200)
            {
                throw new InvalidOperationException("과목명은 1~200자로 입력해야 합니다.");
            }
            if (subject.최소참석횟수 is < 0 or > 1000)
            {
                throw new InvalidOperationException("최소참석횟수는 0~1000 사이여야 합니다.");
            }
        }

        foreach (var form in request.양식목록)
        {
            NormalizeChildCode(form.양식코드, "양식코드");
            if (string.IsNullOrWhiteSpace(form.양식명) || form.양식명.Length > 200)
            {
                throw new InvalidOperationException("양식명은 1~200자로 입력해야 합니다.");
            }
            if (string.IsNullOrWhiteSpace(form.버전) || form.버전.Length > 50)
            {
                throw new InvalidOperationException("양식 버전은 1~50자로 입력해야 합니다.");
            }
            if (form.최소제출횟수 is < 0 or > 1000)
            {
                throw new InvalidOperationException("최소제출횟수는 0~1000 사이여야 합니다.");
            }

            var errors = 교육과정양식검증기.필드정의검증(form.필드목록);
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(string.Join(" ", errors));
            }
        }
    }

    private static string NormalizeCode(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!CourseCodeRegex().IsMatch(normalized))
        {
            throw new InvalidOperationException("과정코드는 영문 소문자, 숫자, 하이픈을 사용해 3~100자로 입력해야 합니다.");
        }
        return normalized;
    }

    private static string NormalizeChildCode(string value, string label)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length is < 1 or > 100)
        {
            throw new InvalidOperationException($"{label}는 1~100자로 입력해야 합니다.");
        }
        return normalized;
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{1,98}[a-z0-9]$", RegexOptions.CultureInvariant)]
    private static partial Regex CourseCodeRegex();
}
