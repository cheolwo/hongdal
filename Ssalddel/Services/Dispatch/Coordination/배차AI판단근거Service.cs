namespace 살뜰.Services.Dispatch.Coordination;

public interface I배차AI판단근거조회Service
{
    배차AI판단근거 조회(배차AI판단근거요청 request);
}

public sealed class 규칙기반배차AI판단근거조회Service : I배차AI판단근거조회Service
{
    private readonly I배차AI판단근거Source _source;

    public 규칙기반배차AI판단근거조회Service()
        : this(new 정적배차AI판단근거Source())
    {
    }

    public 규칙기반배차AI판단근거조회Service(I배차AI판단근거Source source)
    {
        _source = source;
    }

    public 배차AI판단근거 조회(배차AI판단근거요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var queryTerms = BuildQueryTerms(request);
        var policies = _source.정책근거목록
            .Select(x => new ScoredSeed<배차AI정책근거Seed>(x, Score(x.키워드, x.제목, x.요약, request, queryTerms)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Seed.근거Id, StringComparer.Ordinal)
            .Take(3)
            .Select(x => new 배차AI정책근거(x.Seed.근거Id, x.Seed.제목, x.Seed.요약, x.Seed.출처))
            .ToArray();
        var cases = _source.사례목록
            .Select(x => new ScoredSeed<배차AI판단사례Seed>(x, Score(x.키워드, x.제목, x.상황요약 + " " + x.판단요약, request, queryTerms)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Seed.사례Id, StringComparer.Ordinal)
            .Take(3)
            .Select(x => new 배차AI판단사례(
                x.Seed.사례Id,
                x.Seed.제목,
                x.Seed.관련OS,
                x.Seed.키워드,
                x.Seed.상황요약,
                x.Seed.판단요약,
                x.Seed.사용자판정,
                x.Seed.중용판정,
                x.Seed.출처))
            .ToArray();

        return new 배차AI판단근거(policies, cases);
    }

    private static HashSet<string> BuildQueryTerms(배차AI판단근거요청 request)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddTerms(terms, request.업무유형);
        foreach (var id in request.의뢰Ids)
        {
            AddTerms(terms, id);
        }

        foreach (var id in request.기사Ids)
        {
            AddTerms(terms, id);
        }

        foreach (var scopeKey in request.배달권키목록)
        {
            AddTerms(terms, scopeKey);
        }

        foreach (var keyword in request.키워드 ?? [])
        {
            AddTerms(terms, keyword);
        }

        if (request.묶음크기 is > 1)
        {
            AddTerms(terms, "묶음 멀티배차 플랫폼 수익 배달권 한 명의 기사에게 묶음 동시배정");
        }

        if (request.업무유형.Contains("음식", StringComparison.OrdinalIgnoreCase)
            || request.업무유형.Contains("Food", StringComparison.OrdinalIgnoreCase))
        {
            AddTerms(terms, "음식 조리완료 포장완료 픽업 고객전달 배달완료시간 멀티배차 같은배달권 인접배달권");
            if (request.묶음크기 is > 1)
            {
                AddTerms(terms, "음식 멀티배차 2건 6km 42분 피크타임 허용초과");
            }
        }

        if (request.목표건당플랫폼순이익.HasValue)
        {
            AddTerms(terms, "목표수익 순이익 플랫폼 회귀");
        }

        if (request.목표기사건당지급액.HasValue)
        {
            AddTerms(terms, "기사 지급액 단가 수익 회귀");
        }

        return terms;
    }

    private static void AddTerms(HashSet<string> terms, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        terms.Add(text.Trim());
        foreach (var term in text.Split([' ', ',', '/', ':', ';', '|', '-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            terms.Add(term);
        }
    }

    private static int Score(
        IReadOnlyList<string> keywords,
        string title,
        string body,
        배차AI판단근거요청 request,
        IReadOnlySet<string> queryTerms)
    {
        var score = 0;
        foreach (var term in queryTerms)
        {
            if (keywords.Any(x => string.Equals(x, term, StringComparison.OrdinalIgnoreCase)))
            {
                score += 5;
                continue;
            }

            if (keywords.Any(x => x.Contains(term, StringComparison.OrdinalIgnoreCase) || term.Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                score += 3;
                continue;
            }

            if (title.Contains(term, StringComparison.OrdinalIgnoreCase) || body.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 1;
            }
        }

        if (request.묶음크기 is > 1 && keywords.Contains("묶음"))
        {
            score += 4;
        }

        if (request.목표건당플랫폼순이익.HasValue && keywords.Contains("플랫폼"))
        {
            score += 4;
        }

        if (request.목표기사건당지급액.HasValue && keywords.Contains("기사"))
        {
            score += 4;
        }

        return score;
    }

    private sealed record ScoredSeed<TSeed>(TSeed Seed, int Score);
}

public sealed record 배차AI판단근거요청(
    string 업무유형,
    IReadOnlyList<string> 의뢰Ids,
    IReadOnlyList<string> 기사Ids,
    IReadOnlyList<string> 배달권키목록,
    int? 묶음크기 = null,
    decimal? 목표건당플랫폼순이익 = null,
    decimal? 목표기사건당지급액 = null,
    IReadOnlyList<string>? 키워드 = null);

public sealed record 배차AI판단근거(
    IReadOnlyList<배차AI정책근거> 정책근거목록,
    IReadOnlyList<배차AI판단사례> 사례목록);

public sealed record 배차AI정책근거(
    string 근거Id,
    string 제목,
    string 요약,
    string 출처);

public sealed record 배차AI판단사례(
    string 사례Id,
    string 제목,
    string 관련OS,
    IReadOnlyList<string> 키워드,
    string 상황요약,
    string 판단요약,
    string 사용자판정,
    string 중용판정,
    string 출처);
