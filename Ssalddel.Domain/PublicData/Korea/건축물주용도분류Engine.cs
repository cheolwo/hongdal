namespace Ssalddel.Domain.PublicData.Korea;

public static class 건축물주용도분류Engine
{
    public const string RuleRevision = "kr-building-main-purpose-v1";

    private static readonly (string CategoryCode, string[] Terms)[] Rules =
    [
        (건축물용도CategoryCodes.Residential, ["단독주택", "공동주택"]),
        (건축물용도CategoryCodes.Agriculture, ["동물및식물관련시설"]),
        (건축물용도CategoryCodes.LogisticsStorage, ["창고시설"]),
        (건축물용도CategoryCodes.Industrial, ["공장"]),
        (건축물용도CategoryCodes.EducationResearch, ["교육연구시설"]),
        (건축물용도CategoryCodes.MedicalWelfare, ["의료시설", "노유자시설"]),
        (건축물용도CategoryCodes.Religious, ["종교시설"]),
        (건축물용도CategoryCodes.Transport, ["운수시설", "자동차관련시설"]),
        (건축물용도CategoryCodes.UtilityInfrastructure, ["발전시설", "방송통신시설", "자원순환관련시설", "위험물저장및처리시설"]),
        (건축물용도CategoryCodes.CultureTourism, ["문화및집회시설", "관광휴게시설", "숙박시설", "운동시설", "위락시설"]),
        (건축물용도CategoryCodes.PublicCommunity, ["교정및군사시설", "묘지관련시설"]),
        (건축물용도CategoryCodes.BusinessOffice, ["업무시설"]),
        (건축물용도CategoryCodes.Commercial, ["근린생활시설", "판매시설"]),
    ];

    public static string Classify(string? officialMainPurposeName)
    {
        if (string.IsNullOrWhiteSpace(officialMainPurposeName))
            return 건축물용도CategoryCodes.Unresolved;

        var normalized = new string(officialMainPurposeName
            .Where(character => !char.IsWhiteSpace(character) && character is not '(' and not ')')
            .ToArray());

        foreach (var (categoryCode, terms) in Rules)
        {
            if (terms.Any(normalized.Contains))
                return categoryCode;
        }

        return 건축물용도CategoryCodes.Other;
    }
}
