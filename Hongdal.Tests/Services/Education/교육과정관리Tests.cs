using System.Reflection;
using System.Text.Json;
using Hongdal.Contracts.Common.Education;
using Hongdal.Controllers.Admin.Education08;
using Hongdal.Controllers.Common;
using Hongdal.Services.Education;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Data;

namespace Hongdal.Tests.Services.Education;

public sealed class 교육과정관리Tests
{
    [Fact]
    public void 홍익학당초안은_공개교육과정과_입력양식의핵심항목을담는다()
    {
        var draft = 교육과정정의Service.CreateHongikAcademyDraft();

        Assert.Equal(3, draft.최소이수개월);
        Assert.Equal(
            ["참나각성", "양심성찰", "호흡수련", "독서스터디"],
            draft.과목목록.OrderBy(x => x.표시순서).Select(x => x.과목코드));
        Assert.All(draft.과목목록, x => Assert.Equal(3, x.최소참석횟수));

        var application = Assert.Single(draft.양식목록, x => x.양식코드 == 교육과정양식코드.입교신청);
        var requiredKeys = application.필드목록.Where(x => x.필수여부).Select(x => x.Key).ToHashSet();
        Assert.Contains("회원가입확인", requiredKeys);
        Assert.Contains("이름", requiredKeys);
        Assert.Contains("이메일", requiredKeys);
        Assert.Contains("전화번호", requiredKeys);
        Assert.Contains("입교서약동의", requiredKeys);
        Assert.Contains("개인정보수집이용동의", requiredKeys);
        Assert.Contains("개인정보제3자제공동의", requiredKeys);

        var training = Assert.Single(draft.양식목록, x => x.양식코드 == 교육과정양식코드.수련체험기);
        Assert.Equal("매월 1회", training.제출주기);
        Assert.Equal(3, training.최소제출횟수);
        Assert.Contains(training.필드목록, x => x.Key == "양심성찰사안");
        Assert.Contains(training.필드목록, x => x.Key == "상대방원하는것");
        Assert.Contains(training.필드목록, x => x.Key == "부당하게피해준부분");
        Assert.Contains(training.필드목록, x => x.Key == "결론");

        var consultation = Assert.Single(draft.양식목록, x => x.양식코드 == 교육과정양식코드.상담과제);
        Assert.Equal(["아공필기", "법공필기", "구공필기"], consultation.필드목록.Select(x => x.Key));
    }

    [Fact]
    public void 양식검증기는_필수누락과_정의되지않은답변을거부한다()
    {
        var fields = new[]
        {
            new 교육과정양식필드Dto
            {
                Key = "결론",
                라벨 = "결론",
                유형 = 교육과정양식필드유형.긴글,
                필수여부 = true,
                최대길이 = 100
            }
        };
        var answers = new Dictionary<string, JsonElement>
        {
            ["정의되지않음"] = JsonSerializer.SerializeToElement("답변")
        };

        var errors = 교육과정양식검증기.답변검증(fields, answers);

        Assert.Contains(errors, x => x.Contains("정의되지 않은 답변", StringComparison.Ordinal));
        Assert.Contains(errors, x => x.Contains("필수", StringComparison.Ordinal));
    }

    [Fact]
    public void 양식검증기는_정의에맞는답변을허용한다()
    {
        var fields = new[]
        {
            new 교육과정양식필드Dto
            {
                Key = "성별",
                라벨 = "성별",
                유형 = 교육과정양식필드유형.단일선택,
                필수여부 = true,
                최대길이 = 20,
                선택목록 = ["남", "여"]
            },
            new 교육과정양식필드Dto
            {
                Key = "동의",
                라벨 = "동의",
                유형 = 교육과정양식필드유형.참거짓,
                필수여부 = true,
                최대길이 = 1
            }
        };
        var answers = new Dictionary<string, JsonElement>
        {
            ["성별"] = JsonSerializer.SerializeToElement("여"),
            ["동의"] = JsonSerializer.SerializeToElement(true)
        };

        Assert.Empty(교육과정양식검증기.답변검증(fields, answers));
    }

    [Fact]
    public void 양식검증기는_필수동의의_false값을거부한다()
    {
        var fields = new[]
        {
            new 교육과정양식필드Dto
            {
                Key = "동의",
                라벨 = "개인정보 동의",
                유형 = 교육과정양식필드유형.참거짓,
                필수여부 = true,
                참값필수여부 = true,
                최대길이 = 1
            }
        };
        var answers = new Dictionary<string, JsonElement>
        {
            ["동의"] = JsonSerializer.SerializeToElement(false)
        };

        var errors = 교육과정양식검증기.답변검증(fields, answers);

        Assert.Contains(errors, x => x.Contains("동의하거나 확인", StringComparison.Ordinal));
    }

    [Fact]
    public void 공개조회와_참여운영관리_API권한이_분리된다()
    {
        var publicAction = typeof(교육과정Controller).GetMethod(nameof(교육과정Controller.목록조회));
        Assert.NotNull(publicAction?.GetCustomAttribute<AllowAnonymousAttribute>());

        var participantAuthorize = typeof(교육과정참여Controller).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(participantAuthorize);

        var adminAuthorize = typeof(교육과정관리Controller).GetCustomAttribute<AuthorizeAttribute>();
        Assert.Equal("서버관리자전용", adminAuthorize?.Policy);

        var operationAuthorize = typeof(교육과정운영Controller).GetCustomAttribute<AuthorizeAttribute>();
        Assert.Contains(역할명.교육과정멘토, operationAuthorize?.Roles ?? string.Empty, StringComparison.Ordinal);
        Assert.Contains(역할명.서버관리자, operationAuthorize?.Roles ?? string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain(역할명.선생님, operationAuthorize?.Roles ?? string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain(역할명.현장체험지도자, operationAuthorize?.Roles ?? string.Empty, StringComparison.Ordinal);
    }
}
