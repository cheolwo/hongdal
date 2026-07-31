using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.Content;
using 살뜰.Data;

namespace Ssalddel.Infrastructure.Persistence.SeedData.Content;

public static class 지역문화이미지PromptSeeder
{
    public static async Task<int> SeedAsync(
        SsalddelContext db,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.지역문화이미지Prompts
            .ToDictionaryAsync(item => item.RegionKey, StringComparer.Ordinal, cancellationToken);
        var changed = 0;

        foreach (var seed in 지역문화이미지PromptSeedData.All)
        {
            if (!existing.TryGetValue(seed.RegionKey, out var current))
            {
                db.지역문화이미지Prompts.Add(seed);
                changed++;
                continue;
            }

            if (!string.Equals(
                    current.ReviewStatusCode,
                    지역문화이미지Prompt검토상태Codes.ResearchDraft,
                    StringComparison.Ordinal)
                || current.PromptVersion >= seed.PromptVersion)
            {
                continue;
            }

            ApplySeed(current, seed);
            changed++;
        }

        if (changed > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return changed;
    }

    private static void ApplySeed(
        지역문화이미지Prompt target,
        지역문화이미지Prompt seed)
    {
        target.CountryCode = seed.CountryCode;
        target.SubdivisionCode = seed.SubdivisionCode;
        target.RegionNameKo = seed.RegionNameKo;
        target.RegionNameEn = seed.RegionNameEn;
        target.RegionNameLocal = seed.RegionNameLocal;
        target.RegionTypeCode = seed.RegionTypeCode;
        target.GeographySummaryKo = seed.GeographySummaryKo;
        target.CultureSummaryKo = seed.CultureSummaryKo;
        target.VisualAnchorsJson = seed.VisualAnchorsJson;
        target.AvoidExpressionsJson = seed.AvoidExpressionsJson;
        target.PromptKo = seed.PromptKo;
        target.AspectRatio = seed.AspectRatio;
        target.SafeCrop = seed.SafeCrop;
        target.RequiresEvidenceReview = seed.RequiresEvidenceReview;
        target.EvidenceNotesKo = seed.EvidenceNotesKo;
        target.PromptVersion = seed.PromptVersion;
        target.UpdatedAtUtc = seed.UpdatedAtUtc;
    }
}

internal static class 지역문화이미지PromptSeedData
{
    private static readonly DateTime SeededAtUtc =
        new(2026, 7, 30, 0, 0, 0, DateTimeKind.Utc);

    public static IReadOnlyList<지역문화이미지Prompt> All =>
        KoreaRows
            .Concat(UnitedStatesRows)
            .Concat(ChinaRows)
            .Select(Create)
            .ToArray();

    private static 지역문화이미지Prompt Create(SeedRow row)
    {
        var anchors = Split(row.VisualAnchors);
        var avoid = Split(row.AvoidExpressions)
            .Concat(CommonAvoidExpressions)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new 지역문화이미지Prompt
        {
            RegionKey = row.RegionKey,
            CountryCode = row.CountryCode,
            SubdivisionCode = row.SubdivisionCode,
            RegionNameKo = row.RegionNameKo,
            RegionNameEn = row.RegionNameEn,
            RegionNameLocal = row.RegionNameLocal,
            RegionTypeCode = row.RegionTypeCode,
            GeographySummaryKo = row.GeographySummaryKo,
            CultureSummaryKo = row.CultureSummaryKo,
            VisualAnchorsJson = JsonSerializer.Serialize(anchors),
            AvoidExpressionsJson = JsonSerializer.Serialize(avoid),
            PromptKo = BuildPrompt(row, anchors, avoid),
            AspectRatio = "16:9",
            SafeCrop = "center-4:3",
            ReviewStatusCode = 지역문화이미지Prompt검토상태Codes.ResearchDraft,
            RequiresEvidenceReview = true,
            EvidenceNotesKo = BuildEvidenceNotes(row),
            PromptVersion = 2,
            CreatedAtUtc = SeededAtUtc,
            UpdatedAtUtc = SeededAtUtc
        };
    }

    private static string BuildPrompt(
        SeedRow row,
        IReadOnlyList<string> anchors,
        IReadOnlyList<string> avoid)
        => $"""
            지역 문화 이해를 돕는 따뜻한 스타일라이즈드 3D 애니메이션 장면. {row.RegionNameKo}의 현재 생활을 장편 애니메이션의 한 장면처럼 그린다.
            지형과 환경: {row.GeographySummaryKo}
            생활문화 장면: {row.CultureSummaryKo}
            핵심 시각 요소: {string.Join(", ", anchors)}.
            부드럽고 입체적인 형태, 손으로 다듬은 듯한 재질, 전경·중경·배경이 분리된 깊이감,
            따뜻한 빛과 서늘한 환경색의 균형, 절제된 표정과 자연스러운 동작을 사용한다.
            실사 사진이나 평면 2D 삽화가 아니라 고품질 3D 애니메이션 필름 스틸처럼 표현하되
            특정 제작사·작가·기존 캐릭터의 화풍이나 인물을 복제하지 않는다.
            주민은 관광객을 위한 연출이 아니라 실제 생활·작업·공예·음식 준비를 함께하는 모습으로 표현하고,
            연령과 배경이 다양한 사람을 현대의 일상복으로 자연스럽게 배치한다.
            음식과 특산물은 1~2개만 보조 요소로 두며 지역 전체나 상품 원산지의 증명처럼 표현하지 않는다.
            16:9 가로 구도, 중앙 4:3 안전 영역에 핵심 인물과 문화 요소를 모두 배치하고 카드 크롭을 고려해 가장자리에 중요한 대상을 두지 않는다.
            화면 안에 문자, 가격, 로고, 국기, 행정구역 지도, 정치적 상징을 넣지 않는다.
            우주 구체, 마법 에너지, 판타지 문양처럼 지역 생활과 무관한 장식은 넣지 않는다.
            피해야 할 표현: {string.Join(", ", avoid)}.
            생성 전 해당 지역의 최신 공식 문화·관광 자료와 당사자 공동체 자료로 건축·복식·공예·음식 표현을 재검토한다.
            """;

    private static string BuildEvidenceNotes(SeedRow row)
        => row.CountryCode switch
        {
            "KR" => $"{row.RegionNameKo} 시·도청 문화·관광·국가유산 담당 부서, 국가유산청 국가유산포털, 지역문화진흥원, 지역 문화재단·문화원·공립박물관의 공식 자료로 생성 전 재검토합니다. 시·도 안의 도시·농어촌·섬 생활권을 하나의 이미지로 고정하지 않습니다.",
            "US" => $"{row.RegionNameKo} 주정부 문화·관광·농업 기관, Smithsonian Folklife, 주립 역사·인문 기관, 해당 원주민 공동체의 공식 자료로 생성 전 재검토합니다. 관광 홍보 문구만으로 대표성을 확정하지 않습니다.",
            _ => $"중국 비물질문화유산망, {row.RegionNameKo} 인민정부·문화여유 부서, 성급 박물관과 관련 민족 공동체 자료로 생성 전 재검토합니다. 성급 행정구역 안의 여러 생활권과 민족문화를 하나로 고정하지 않습니다."
        };

    private static string[] Split(string value)
        => value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static readonly string[] CommonAvoidExpressions =
    [
        "단일 민족·외모·복장으로 지역 전체를 고정하는 표현",
        "관광 엽서처럼 랜드마크만 나열하는 콜라주",
        "시대가 다른 건축·복식·도구를 한 장면에 무근거로 혼합",
        "실제 상품의 원산지나 생산량을 증명하는 듯한 표현"
    ];

    private static readonly SeedRow[] KoreaRows =
    [
        Kr("seoul", "KR-11", "서울특별시", "Seoul",
            지역문화행정구역유형Codes.KoreaSpecialCity,
            "한강과 북한산·관악산 사이에 고밀도 도심, 오래된 골목과 신도시 생활권이 겹치는 수도",
            "동네 시장과 골목 공방, 한옥을 돌보는 주민, 현대적인 음식 준비와 생활 기술이 세대 사이에서 이어지는 장면",
            "한강과 산 능선|동네 시장과 골목 공방|한옥과 현대 주거의 공존",
            "궁궐·한복·초고층 야경만으로 서울 전체를 대표"),
        Kr("busan", "KR-26", "부산광역시", "Busan",
            지역문화행정구역유형Codes.KoreaMetropolitanCity,
            "산지가 바다 가까이 내려오고 항만·하천·해안 마을이 이어지는 남동부 해양도시",
            "수산시장 상인과 항만 노동, 산복도로 이웃의 생활, 어묵·수산물 손질과 지역 공예가 만나는 현재의 일상",
            "산과 항만이 맞닿은 지형|수산시장 작업대|산복도로 생활 골목",
            "해수욕장·광안대교·갈매기만으로 부산을 관광 엽서처럼 표현"),
        Kr("daegu", "KR-27", "대구광역시", "Daegu",
            지역문화행정구역유형Codes.KoreaMetropolitanCity,
            "분지와 금호강·낙동강, 팔공산 자락이 둘러싼 내륙 도시",
            "섬유·봉제 공방과 약령시장, 골목 음식점과 근교 농산물 시장이 연결되는 생활문화",
            "분지와 팔공산 능선|섬유·봉제 작업|약령시장과 골목 음식",
            "무더위·사과·근대 골목 하나로 도시 전체를 고정"),
        Kr("incheon", "KR-28", "인천광역시", "Incheon",
            지역문화행정구역유형Codes.KoreaMetropolitanCity,
            "서해 갯벌과 섬, 항만과 간척지, 오래된 도심과 신도시가 함께 있는 관문 도시",
            "항만·어시장 노동과 섬 주민의 해산물 손질, 소금·공예·이주 역사가 동네 시장에서 만나는 장면",
            "서해 섬과 갯벌|항만·어시장 생활|오래된 도심과 신도시의 공존",
            "공항·차이나타운·송도 고층건물만으로 인천을 대표"),
        Kr("gwangju", "KR-29", "광주광역시", "Gwangju",
            지역문화행정구역유형Codes.KoreaMetropolitanCity,
            "무등산 자락과 영산강 수계의 평야가 만나는 호남 내륙 도시",
            "시장 음식과 공예 작업, 현대미술·공연을 준비하는 주민, 도시와 근교 농촌의 재료가 만나는 생활",
            "무등산과 도시 생활권|시장 음식 준비|현대미술·공예 작업",
            "특정 정치적 사건·비엔날레·한 가지 음식만으로 광주를 고정"),
        Kr("daejeon", "KR-30", "대전광역시", "Daejeon",
            지역문화행정구역유형Codes.KoreaMetropolitanCity,
            "갑천·유등천·대전천과 낮은 산지 사이에 형성된 중부 내륙 분지 도시",
            "연구·교육 생활과 오래된 시장, 철도 주변 골목의 음식·수선·제작 기술이 함께 이어지는 장면",
            "하천과 낮은 산지|연구·교육의 일상|중앙시장과 생활 공방",
            "연구소·과학 상징·기차역만으로 대전 전체를 대표"),
        Kr("ulsan", "KR-31", "울산광역시", "Ulsan",
            지역문화행정구역유형Codes.KoreaMetropolitanCity,
            "태화강과 동해안, 산업 항만, 영남알프스와 농어촌 생활권이 공존하는 도시",
            "조선·제조 노동자의 일상과 강변 생태, 어촌·농촌 시장, 목공·생활 공예가 함께 보이는 장면",
            "태화강과 대숲|항만 노동의 일상|농어촌 시장과 생활 공예",
            "공장 굴뚝·대형 선박·고래만으로 울산을 고정"),
        Kr("sejong", "KR-50", "세종특별자치시", "Sejong",
            지역문화행정구역유형Codes.KoreaSpecialSelfGoverningCity,
            "금강과 낮은 산지, 계획도시 생활권과 오래된 읍·면 농촌이 함께 있는 중부 지역",
            "새로운 공동주택 이웃과 전통시장·농가가 로컬푸드, 돌봄, 생활 공예로 연결되는 현재의 생활",
            "금강과 낮은 구릉|계획도시와 읍면 생활권|로컬푸드 시장",
            "정부청사와 새 아파트만으로 세종 전체를 대표"),
        Kr("gyeonggi", "KR-41", "경기도", "Gyeonggi-do",
            지역문화행정구역유형Codes.KoreaProvince,
            "한강 수계와 산지, 서해 갯벌, 대도시·공업도시·농촌이 넓게 연결된 수도권",
            "도자·목공 공방과 성곽 주변 마을, 농장·전통시장·다문화 생활권이 서로 다른 지역성을 유지하며 만나는 장면",
            "한강과 서해 갯벌|도자·생활 공예|도시와 농촌 시장의 연결",
            "서울의 교외·아파트·민속촌 하나로 경기도 전체를 고정"),
        Kr("gangwon", "KR-42", "강원특별자치도", "Gangwon State",
            지역문화행정구역유형Codes.KoreaSpecialSelfGoverningProvince,
            "태백산맥의 높은 산지와 동해안, 깊은 계곡과 고원 농촌이 이어지는 지역",
            "어항의 생선 손질과 산촌의 목공·직조, 메밀·감자 등 농산물을 다루는 시장 생활이 이어지는 장면",
            "태백산맥과 동해안|어항과 산촌 생활|고원 농산물 시장",
            "스키장·설경·바다 관광지만으로 강원도를 대표"),
        Kr("chungbuk", "KR-43", "충청북도", "Chungcheongbuk-do",
            지역문화행정구역유형Codes.KoreaProvince,
            "소백산맥과 차령산맥 사이의 산지·분지, 충주호와 남한강 수계가 있는 내륙 지역",
            "과수·약초·곡물을 다루는 장터와 금속·인쇄·생활 공예, 호수 주변 마을의 일상이 이어지는 장면",
            "내륙 산지와 충주호|과수·약초 시장|인쇄·생활 공예",
            "호수 관광·사찰·농촌 풍경 하나로 충북 전체를 고정"),
        Kr("chungnam", "KR-44", "충청남도", "Chungcheongnam-do",
            지역문화행정구역유형Codes.KoreaProvince,
            "서해 리아스 해안과 갯벌, 금강 유역 평야와 낮은 산지가 연결된 지역",
            "갯벌·어업과 벼농사, 젓갈·곡물 시장, 백제 문화에서 이어진 공예를 현대 주민이 다루는 생활",
            "서해 갯벌과 금강 평야|어업·농업 작업|젓갈·곡물·공예 시장",
            "백제 유적·사찰·서해 낙조만으로 충남 전체를 대표"),
        Kr("jeonbuk", "KR-45", "전북특별자치도", "Jeonbuk State",
            지역문화행정구역유형Codes.KoreaSpecialSelfGoverningProvince,
            "호남평야와 서해안, 노령산맥·지리산 자락이 만나는 농업·산촌 지역",
            "판소리 연습과 한지·목공, 장류·발효 음식과 곡물을 준비하는 시장·마을의 현재 생활",
            "호남평야와 산지|판소리·한지 공방|발효 음식과 곡물 시장",
            "전주 한옥마을·한복·비빔밥만으로 전북 전체를 고정"),
        Kr("jeonnam", "KR-46", "전라남도", "Jeollanam-do",
            지역문화행정구역유형Codes.KoreaProvince,
            "다도해의 긴 해안과 섬·갯벌, 영산강 평야와 지리산 남서 자락이 이어지는 지역",
            "어업·염전·농사 노동과 공동 음식 준비, 대나무·도자·직조 같은 생활 공예가 장터에서 만나는 장면",
            "다도해와 갯벌|어업·염전·농사|공동 음식과 생활 공예",
            "섬·낙조·초가·한 가지 축제만으로 전남을 이국적으로 표현"),
        Kr("gyeongbuk", "KR-47", "경상북도", "Gyeongsangbuk-do",
            지역문화행정구역유형Codes.KoreaProvince,
            "태백·소백 산지와 낙동강 상류, 동해안과 넓은 내륙 농촌이 이어지는 지역",
            "서원·종가 주변의 일상과 목공·한지·도자, 동해 어업과 농산물 시장이 현대 생활 속에서 공존하는 장면",
            "낙동강과 동해안|목공·한지·도자 공방|어업과 농산물 시장",
            "경주 유적·유교 의례·사과만으로 경북 전체를 대표"),
        Kr("gyeongnam", "KR-48", "경상남도", "Gyeongsangnam-do",
            지역문화행정구역유형Codes.KoreaProvince,
            "남해안의 섬과 만, 낙동강 하류·분지, 지리산 동쪽 산지가 공존하는 지역",
            "조선·기계 산업 노동과 어업·농사, 도자·나전·생활 공예가 항구와 내륙 시장에서 만나는 장면",
            "남해안과 낙동강 하류|항구·산업 노동의 일상|도자·나전과 농수산 시장",
            "조선소·벚꽃·한려수도 풍경만으로 경남 전체를 고정"),
        Kr("jeju", "KR-49", "제주특별자치도", "Jeju Special Self-Governing Province",
            지역문화행정구역유형Codes.KoreaSpecialSelfGoverningProvince,
            "화산섬의 오름·곶자왈·용암 해안과 돌담 밭, 바람이 강한 마을 생활권",
            "해녀의 공동 작업과 밭농사, 돌·목공·직조, 해산물·감귤을 다루는 시장 생활이 이어지는 장면",
            "오름과 돌담 밭|해녀 공동 작업|해산물·감귤 시장과 생활 공예",
            "리조트·야자수·전통복식·해녀 한 사람만으로 제주를 이국적 관광지처럼 표현")
    ];

    private static readonly SeedRow[] UnitedStatesRows =
    [
        Us("alabama", "AL", "앨라배마주", "Alabama",
            "애팔래치아 남단과 블랙벨트의 완만한 구릉, 소나무 숲, 멕시코만 연안",
            "소도시 공동체 장터에서 퀼트와 목공을 만들고 지역 농산물과 연안 수산물을 나누는 현재의 생활",
            "붉은 흙과 소나무|블랙벨트 퀼트 공방|멕시코만 수산물 장터",
            "목화농장과 남북전쟁 이미지만으로 축소"),
        Us("alaska", "AK", "알래스카주", "Alaska",
            "북극 툰드라, 침엽수림, 빙하와 긴 해안이 공존하는 광대한 북부 지역",
            "현대의 작은 항구에서 어업과 수리 작업을 하고 지역 공예와 식재료를 나누는 공동체 생활",
            "설산과 해안|현대식 작업 항구|연어와 선박 수리",
            "이글루·모피복·단일 원주민 이미지로 고정"),
        Us("arizona", "AZ", "애리조나주", "Arizona",
            "소노라 사막의 사구아로 지대부터 고원과 협곡, 소나무 숲까지 이어지는 건조한 남서부",
            "사막 도시의 야외 장터에서 지역 도자·직조와 멕시코계 음식 문화가 현대 생활과 만나는 장면",
            "사구아로와 사막 산지|현대 남서부 도시|도자·직조 공예",
            "서부영화·카우보이·부족 의상을 무근거로 혼합"),
        Us("arkansas", "AR", "아칸소주", "Arkansas",
            "오자크·우아치타 산지, 미시시피 충적평야와 습지가 만나는 남중부",
            "강과 숲 가까운 지역 장터에서 목공·현악기 연주와 쌀·과수 농산물을 함께 나누는 생활",
            "오자크 숲과 강|목공과 현악기|쌀·과수 장터",
            "농촌 빈곤이나 산악 은둔 이미지로 축소"),
        Us("california", "CA", "캘리포니아주", "California",
            "태평양 해안, 중앙계곡, 산맥, 사막과 북부 삼림이 한 주 안에 공존",
            "다양한 이주 공동체가 농산물 장터와 해안 도시 생활 속에서 재료와 조리문화를 나누는 장면",
            "태평양 해안|중앙계곡 농산물|다문화 도시 장터",
            "할리우드·해변·고급 생활만으로 축소"),
        Us("colorado", "CO", "콜로라도주", "Colorado",
            "로키산맥과 고산 분지, 동부 대평원이 이어지는 내륙 고지대",
            "산악 소도시 장터에서 목축·농업 생산물과 현대 공예를 나누고 야외 활동을 준비하는 생활",
            "로키산맥|고산 소도시|목축·농산물 장터",
            "스키장과 부유한 휴양지 이미지만 강조"),
        Us("connecticut", "CT", "코네티컷주", "Connecticut",
            "롱아일랜드 해협의 해안, 코네티컷강 계곡과 숲이 가까운 뉴잉글랜드",
            "오래된 산업도시와 해안 마을의 주민이 지역 장터에서 해산물·공예·책과 음악을 나누는 장면",
            "해협과 하구|벽돌 산업건축|마을 장터와 해산물",
            "부유한 교외와 아이비리그만으로 대표"),
        Us("delaware", "DE", "델라웨어주", "Delaware",
            "대서양 해변, 델라웨어만 습지와 완만한 농지가 이어지는 작은 해안 주",
            "만 연안의 어업과 내륙 농업이 만나는 소규모 시장에서 주민이 수산물과 과채를 손질하는 생활",
            "염습지와 만|작은 작업 항구|과채·수산물 시장",
            "기업 본사나 해변 휴양지만으로 축소"),
        Us("florida", "FL", "플로리다주", "Florida",
            "아열대 반도, 대서양·멕시코만 해안, 습지와 산호섬이 이어지는 지역",
            "카리브해·라틴아메리카·미국 남부 문화가 공존하는 동네 시장과 어항의 현재 생활",
            "아열대 습지|다문화 동네 시장|감귤과 연안 어업",
            "테마파크·야자수 해변·은퇴촌만으로 대표"),
        Us("georgia", "GA", "조지아주", "Georgia",
            "애팔래치아 산기슭, 피드몬트 구릉, 대서양 염습지와 농업 평야",
            "과수원과 피칸 농지 곁 지역 장터에서 도시와 농촌 주민이 음식과 음악을 나누는 생활",
            "피드몬트 구릉|복숭아·피칸|공동체 장터와 음악",
            "복숭아 상징이나 남부 대농장만으로 축소"),
        Us("hawaii", "HI", "하와이주", "Hawaii",
            "화산섬, 열대림, 해안과 고지 농업지대가 섬마다 다르게 펼쳐지는 태평양 군도",
            "현대의 지역 시장과 해안 마을에서 어업·타로 재배·꽃과 공예가 이어지는 다섬 공동체 생활",
            "화산 지형과 바다|타로와 지역 농업|현대 섬 공동체 시장",
            "훌라 의상·리조트·서핑만으로 고정하거나 원주민 문화를 장식화"),
        Us("idaho", "ID", "아이다호주", "Idaho",
            "로키산맥, 강 협곡, 고원과 관개 농지가 이어지는 북서 내륙",
            "강과 농지 가까운 소도시에서 주민이 감자 외 다양한 곡물·과채와 목공품을 나누는 생활",
            "산악과 강 협곡|관개 농지|소도시 농산물 장터",
            "감자 하나로만 지역을 대표"),
        Us("illinois", "IL", "일리노이주", "Illinois",
            "미시간호 연안 대도시부터 프레리와 대규모 농지, 미시시피강까지 이어지는 중서부",
            "시카고의 다문화 동네 시장과 내륙 농촌의 곡물·공예가 하나의 유통 장면으로 연결되는 생활",
            "미시간호 도시|프레리 농지|다문화 시장과 철도",
            "시카고 스카이라인이나 옥수수밭 하나로 축소"),
        Us("indiana", "IN", "인디애나주", "Indiana",
            "중앙 저지대 농경지, 북부 호숫가와 남부 구릉이 이어지는 중서부",
            "작은 제조도시와 농촌 장터에서 금속·목공 기술과 옥수수·콩 재료를 나누는 생활",
            "농경 평야|작은 제조도시|공예와 농산물 시장",
            "자동차 경주와 옥수수만으로 대표"),
        Us("iowa", "IA", "아이오와주", "Iowa",
            "완만한 프레리와 비옥한 농경지, 미시시피·미주리강 사이의 내륙",
            "농촌 마을 시장에서 곡물 생산과 지역 베이킹·수공예를 여러 세대가 함께 준비하는 생활",
            "프레리 농지|곡물 저장고|마을 베이킹과 수공예",
            "끝없는 옥수수밭과 단일 농가 이미지"),
        Us("kansas", "KS", "캔자스주", "Kansas",
            "대평원의 초원과 농지, 플린트힐스의 키 큰 풀이 이어지는 중앙 내륙",
            "바람 부는 초원 도시에서 주민이 밀·목축 생산물과 지역 예술을 나누는 공동체 시장",
            "플린트힐스 초원|밀 농지|목축·예술 장터",
            "토네이도·서부 개척·평평한 황무지만 강조"),
        Us("kentucky", "KY", "켄터키주", "Kentucky",
            "애팔래치아 산지, 석회암 구릉과 블루그래스 초지가 이어지는 동부 내륙",
            "산지와 초지 사이 마을에서 현악기 제작·음악, 목공과 지역 식재료를 나누는 생활",
            "블루그래스 초지|현악기와 목공|산지 마을 시장",
            "경마·버번·빈곤한 애팔래치아 이미지만 강조"),
        Us("louisiana", "LA", "루이지애나주", "Louisiana",
            "미시시피강 삼각주, 늪지와 멕시코만 연안이 도시·농촌과 이어지는 지역",
            "크리올·케이준·아프리카계 전통이 공존하는 동네에서 음악과 해산물 요리를 함께 준비하는 생활",
            "삼각주와 습지|동네 음악 연주|해산물 공동 조리",
            "마디그라 의상·늪지·한 가지 인종 이미지로 고정"),
        Us("maine", "ME", "메인주", "Maine",
            "바위 많은 대서양 해안, 침엽수림, 호수와 내륙 산지가 이어지는 북동부",
            "작업 항구에서 어업 장비를 손질하고 숲·농지의 재료와 야생 블루베리를 나누는 공동체 생활",
            "바위 해안과 침엽수림|작업 항구와 어구|야생 블루베리",
            "등대·랍스터·백인 어촌만으로 고정"),
        Us("maryland", "MD", "메릴랜드주", "Maryland",
            "체서피크만과 조수 하천, 애팔래치아 서부 산지와 도시권이 만나는 중대서양",
            "만 연안의 게잡이와 보트 작업, 도시·농촌 장터의 음식문화가 연결되는 생활",
            "체서피크만|작업 보트와 게잡이|도시·농촌 시장",
            "게 요리와 볼티모어 항구만으로 대표"),
        Us("massachusetts", "MA", "매사추세츠주", "Massachusetts",
            "대서양 곶과 섬, 숲·강 계곡, 오래된 항구와 산업도시가 공존",
            "현대 항구도시의 시장에서 어업·교육·이주 공동체의 음식과 공예가 만나는 생활",
            "대서양 항구|벽돌 도시와 마을|해산물·책·공예 시장",
            "식민지 복장·보스턴 명소·엘리트 대학만 강조"),
        Us("michigan", "MI", "미시간주", "Michigan",
            "오대호로 둘러싸인 두 반도, 숲과 과수 지대, 산업도시가 이어지는 북부 중서부",
            "호숫가 작업장과 도시 시장에서 선박·제조 기술, 체리·사과와 담수 어업이 만나는 생활",
            "오대호 해안|산업·선박 작업장|과수와 담수어 시장",
            "자동차 공장이나 디트로이트 쇠퇴만으로 축소"),
        Us("minnesota", "MN", "미네소타주", "Minnesota",
            "수많은 호수, 북부 침엽수림과 남부 프레리 농지가 이어지는 북중부",
            "호숫가 마을과 도시 시장에서 북유럽계·아프리카계·아시아계·원주민 공동체의 현대 생활이 만나는 장면",
            "호수와 북부 숲|겨울 생활 도구|다문화 도시 시장",
            "눈·바이킹 이미지나 단일 백인 농촌으로 고정"),
        Us("mississippi", "MS", "미시시피주", "Mississippi",
            "미시시피강 충적평야, 델타 습지, 소나무 숲과 멕시코만 연안",
            "델타의 음악·퀼트·농업과 연안 수산물이 현재의 공동체 장터에서 만나는 생활",
            "델타 평야와 강|블루스 연주와 퀼트|연안 수산물",
            "가난·목화·인종차별 역사만으로 현재 지역을 고정"),
        Us("missouri", "MO", "미주리주", "Missouri",
            "미시시피·미주리강, 오자크 고원과 농경 평야가 만나는 중앙 지역",
            "강변 도시와 오자크 마을의 주민이 목공·음악·농산물을 시장에서 교류하는 생활",
            "두 큰 강|오자크 구릉|목공·음악·농산물 시장",
            "서부 개척 관문이나 바비큐 하나로 축소"),
        Us("montana", "MT", "몬태나주", "Montana",
            "북부 로키산맥과 넓은 대평원, 강 계곡이 이어지는 내륙",
            "목장과 작은 도시가 만나는 시장에서 주민이 양모·곡물·목공품을 나누는 현대 생활",
            "로키산맥과 초원|현대 목장 작업|양모·곡물 장터",
            "카우보이·황야·백인 개척자만으로 고정"),
        Us("nebraska", "NE", "네브래스카주", "Nebraska",
            "대평원, 샌드힐스 초원과 플랫강 유역의 농목축 지대",
            "강 유역 마을에서 목축·곡물 생산과 지역 공예를 함께 준비하는 공동체 생활",
            "샌드힐스 초원|플랫강|목축·곡물 시장",
            "끝없는 농지와 고속도로 통과 지역으로만 묘사"),
        Us("nevada", "NV", "네바다주", "Nevada",
            "그레이트베이슨 사막, 산맥과 고원, 오아시스 도시가 이어지는 건조 내륙",
            "사막 도시와 작은 광산·목축 마을의 주민이 예술·식재료·수리 기술을 나누는 생활",
            "사막 산맥|작은 내륙 마을|예술과 수리 작업장",
            "라스베이거스 카지노와 네온만으로 대표"),
        Us("new-hampshire", "NH", "뉴햄프셔주", "New Hampshire",
            "화이트산맥, 숲과 호수, 짧은 대서양 해안이 있는 뉴잉글랜드",
            "산림 마을의 목공·단풍철 농산물과 작은 해안의 어업이 지역 시장에서 만나는 생활",
            "화이트산맥과 숲|목공 작업장|단풍철 농산물 시장",
            "단풍 관광과 식민지풍 마을만 강조"),
        Us("new-jersey", "NJ", "뉴저지주", "New Jersey",
            "대서양 해안·습지, 비옥한 내륙 농지와 밀집한 도시권이 가까이 공존",
            "다문화 도시 시장에 해안 수산물과 토마토·블루베리 등 내륙 농산물이 들어오는 생활",
            "해안 습지|다문화 도시 시장|내륙 과채 농업",
            "고속도로·교외·해변 산책로만으로 축소"),
        Us("new-mexico", "NM", "뉴멕시코주", "New Mexico",
            "고지 사막, 리오그란데 계곡과 산악 숲이 이어지는 남서부",
            "푸에블로·히스파노·멕시코계·다양한 이주 공동체의 도자·직조·칠리 음식문화가 현대 시장에서 만나는 장면",
            "고지 사막과 리오그란데|어도비 건축|도자·직조·칠리 시장",
            "부족 의식·카우보이·외계인 관광 이미지를 무근거로 혼합"),
        Us("new-york", "NY", "뉴욕주", "New York",
            "대서양 대도시권부터 허드슨계곡, 오대호, 애디론댁 산지와 농촌까지 이어지는 큰 주",
            "도시의 다문화 식품시장과 북부 농장·공예가 철도와 수로를 통해 연결되는 생활",
            "다문화 도시 거리|허드슨 수로|북부 과수·낙농 시장",
            "맨해튼 스카이라인만으로 뉴욕주 전체를 대표"),
        Us("north-carolina", "NC", "노스캐롤라이나주", "North Carolina",
            "블루리지 산맥, 피드몬트, 대서양 외해사주가 동서로 이어지는 남동부",
            "산악 공예·음악과 피드몬트 농업, 해안 어업이 지역 시장에서 연결되는 생활",
            "블루리지와 외해사주|직조·목공 공예|농산물·수산물 시장",
            "담배·대농장·해변 휴양지만으로 축소"),
        Us("north-dakota", "ND", "노스다코타주", "North Dakota",
            "북부 대평원, 배드랜즈와 미주리강 유역의 농목축 지대",
            "초원 마을에서 곡물·목축 작업과 현대 원주민·이주 공동체의 예술이 함께 보이는 생활",
            "대평원과 배드랜즈|곡물·목축 작업|현대 지역 예술시장",
            "황량한 초원·석유시설·부족 의상을 장식처럼 사용"),
        Us("ohio", "OH", "오하이오주", "Ohio",
            "이리호 연안, 강 계곡과 농지, 오래된 제조도시가 이어지는 중서부",
            "도시의 제작 공방과 농촌 장터에서 금속·도자 기술, 곡물·과채가 교류되는 생활",
            "이리호와 강|벽돌 제조도시|공방·농산물 시장",
            "러스트벨트 쇠퇴나 미식축구만으로 축소"),
        Us("oklahoma", "OK", "오클라호마주", "Oklahoma",
            "대평원, 붉은 흙 구릉, 동부 숲과 서부 건조지대가 만나는 남중부",
            "여러 부족 국가와 목축·도시 공동체가 현대 예술·음식 시장에서 교류하는 생활",
            "붉은 흙과 초원|현대 원주민 예술시장|목축·도시 공동체",
            "서부극·티피·카우보이와 부족 문화를 하나로 혼합"),
        Us("oregon", "OR", "오리건주", "Oregon",
            "태평양 해안, 캐스케이드 산맥, 서부 온대우림과 동부 고원·사막",
            "어항과 산림 마을, 도시 장터에서 연어·배·베리·목공과 현대 공예가 만나는 생활",
            "태평양 해안과 온대우림|작업 어항|목공·과수·베리 시장",
            "포틀랜드 힙스터나 비 오는 숲만으로 대표"),
        Us("pennsylvania", "PA", "펜실베이니아주", "Pennsylvania",
            "애팔래치아 산지, 강 계곡, 농촌과 오래된 산업도시가 이어지는 중대서양",
            "다양한 이주 공동체의 도시 시장과 농촌 제빵·목공·금속 공예가 연결되는 생활",
            "애팔래치아와 강|벽돌 산업도시|농촌 제빵·공예 시장",
            "아미시·식민지·쇠퇴한 제철소만으로 축소"),
        Us("rhode-island", "RI", "로드아일랜드주", "Rhode Island",
            "나라갠싯만, 섬과 짧은 해안, 조밀한 항구도시가 있는 작은 뉴잉글랜드 주",
            "항구의 선박 수리와 이주 공동체의 해산물·공예 시장이 가까이 이어지는 생활",
            "나라갠싯만|선박 수리 항구|해산물·공예 시장",
            "대저택과 요트 휴양지만으로 대표"),
        Us("south-carolina", "SC", "사우스캐롤라이나주", "South Carolina",
            "블루리지 산기슭, 피드몬트와 대서양 로컨트리 염습지가 이어지는 남동부",
            "걸러 문화의 바구니 공예, 연안 어업과 내륙 농산물이 현대 시장에서 만나는 생활",
            "로컨트리 염습지|스위트그래스 바구니|연안 수산물 시장",
            "대농장 저택·해변·남북전쟁 이미지로 축소"),
        Us("south-dakota", "SD", "사우스다코타주", "South Dakota",
            "대평원, 블랙힐스와 배드랜즈가 이어지는 북부 내륙",
            "목축·농업 마을과 라코타·다코타 공동체의 현대 예술·공예가 함께 보이는 생활",
            "대평원과 블랙힐스|현대 지역 예술시장|목축·농산물",
            "러시모어·서부극·의례복으로만 대표"),
        Us("tennessee", "TN", "테네시주", "Tennessee",
            "애팔래치아 산지, 컴벌랜드고원과 미시시피 저지대가 동서로 이어지는 남부",
            "작은 도시의 악기 제작·공연과 농촌 식재료·공예가 일상 시장에서 만나는 생활",
            "스모키산맥과 강|악기 제작과 라이브 음악|농산물 장터",
            "컨트리음악 스타·위스키·산골 이미지로 축소"),
        Us("texas", "TX", "텍사스주", "Texas",
            "걸프 연안, 동부 숲, 중앙 구릉, 대평원과 서부 사막이 모두 있는 광대한 주",
            "멕시코계·흑인·원주민·아시아계 등 다양한 공동체가 목축·에너지 도시·해안 시장에서 교류하는 생활",
            "걸프 연안과 내륙 평원|다문화 도시 시장|목축·해안 식재료",
            "카우보이·석유·바비큐 하나로 거대한 주를 대표"),
        Us("utah", "UT", "유타주", "Utah",
            "콜로라도고원의 붉은 사암, 그레이트솔트레이크와 고산 산맥이 이어지는 내륙",
            "사막·산악 도시의 주민이 야외 활동 장비, 직조·도자와 지역 농산물을 나누는 생활",
            "붉은 사암과 염호|산악 도시|직조·도자·농산물 시장",
            "국립공원 풍경이나 특정 종교 공동체만으로 대표"),
        Us("vermont", "VT", "버몬트주", "Vermont",
            "그린산맥, 숲과 목초지, 작은 강 계곡 마을이 이어지는 뉴잉글랜드",
            "낙농·메이플 생산과 목공·직조를 주민이 협동 장터에서 나누는 생활",
            "그린산맥과 목초지|메이플 작업|낙농·목공 장터",
            "단풍·메이플 시럽·백인 농촌만으로 고정"),
        Us("virginia", "VA", "버지니아주", "Virginia",
            "대서양 연안평야, 피드몬트와 블루리지, 애팔래치아 계곡이 이어지는 중대서양",
            "체서피크 어업, 산지 음악·공예와 다문화 도시 시장이 한 주의 여러 생활권으로 연결되는 장면",
            "체서피크만과 블루리지|산지 악기·공예|도시·연안 시장",
            "식민지 저택·전쟁사·워싱턴 교외만으로 축소"),
        Us("washington", "WA", "워싱턴주", "Washington",
            "태평양·퓨젯사운드, 캐스케이드 산맥, 서부 우림과 동부 건조 농지가 공존",
            "작업 항구와 도시 시장에서 연어·사과·홉, 선박·목공 기술이 다양한 공동체와 만나는 생활",
            "퓨젯사운드와 캐스케이드|작업 항구|사과·연어·목공 시장",
            "시애틀 스카이라인·커피·비만으로 대표"),
        Us("west-virginia", "WV", "웨스트버지니아주", "West Virginia",
            "애팔래치아 산맥의 깊은 계곡, 숲과 강이 이어지는 내륙",
            "산지 마을에서 목공·퀼트·현악기와 지역 농산물을 여러 세대가 함께 만드는 생활",
            "애팔래치아 숲과 강|퀼트·현악기|산지 농산물 장터",
            "석탄·가난·고립된 산골 이미지로만 고정"),
        Us("wisconsin", "WI", "위스콘신주", "Wisconsin",
            "미시간호·슈피리어호 연안, 숲과 빙하호, 낙농·곡물 지대",
            "호숫가 도시와 농촌 시장에서 치즈 제조, 목공·금속 공예와 다양한 이주 음식문화가 만나는 생활",
            "오대호와 숲|치즈·낙농 작업|도시·농촌 공예 시장",
            "치즈·맥주·미식축구만으로 대표"),
        Us("wyoming", "WY", "와이오밍주", "Wyoming",
            "로키산맥, 고원 분지와 넓은 세이지브러시 초원이 이어지는 내륙",
            "목장 마을에서 양모·가죽·목공과 지역 식재료를 나누고 공공 토지 일을 준비하는 생활",
            "산맥과 세이지 초원|현대 목장 작업|양모·목공 시장",
            "카우보이·옐로스톤 야생동물만으로 대표")
    ];

    private static readonly SeedRow[] ChinaRows =
    [
        Cn("beijing", "CN-11", "베이징시", "Beijing", "北京市",
            지역문화행정구역유형Codes.ChinaMunicipality,
            "화북 평원 북단과 산지가 만나는 내륙 대도시",
            "후퉁의 생활 골목, 전통 공예 공방과 현대 동네 시장이 함께 이어지는 수도의 일상",
            "후퉁 골목과 사합원|경극·칠기·연 공방|현대 동네 시장",
            "자금성·만리장성·황제복만으로 대표"),
        Cn("tianjin", "CN-12", "톈진시", "Tianjin", "天津市",
            지역문화행정구역유형Codes.ChinaMunicipality,
            "하이허 하구와 보하이만에 접한 화북의 항구도시",
            "강변과 항구의 상업 생활, 만담·점토 인형·음식 공방이 현대 도시 시장과 만나는 장면",
            "하이허와 항구|점토 인형 공방|상업 골목과 밀 음식",
            "유럽식 조계 건축만으로 도시 전체를 대표"),
        Cn("hebei", "CN-13", "허베이성", "Hebei", "河北省",
            지역문화행정구역유형Codes.ChinaProvince,
            "화북 평원, 옌산·타이항 산맥과 보하이 연안이 수도권을 둘러싼 지역",
            "평원 농촌과 산지 마을의 주민이 종이공예·무술·곡물 음식을 지역 시장에서 나누는 생활",
            "화북 평원과 산지|민간 종이·등 공예|곡물·채소 시장",
            "만리장성이나 베이징 배후지로만 축소"),
        Cn("shanxi", "CN-14", "산시성(山西)", "Shanxi", "山西省",
            지역문화행정구역유형Codes.ChinaProvince,
            "황토고원과 타이항·뤼량 산맥, 펀허 계곡이 이어지는 내륙",
            "회색 벽돌 마을과 면 요리 작업대, 종이오리기·목판 공예가 함께 보이는 생활",
            "황토고원과 산지 마을|면 음식 만들기|종이오리기·목판 공예",
            "석탄산업·고대 상인 저택만으로 대표"),
        Cn("inner-mongolia", "CN-15", "네이멍구 자치구", "Inner Mongolia", "内蒙古自治区",
            지역문화행정구역유형Codes.ChinaAutonomousRegion,
            "동서로 긴 초원·사막·삼림과 농목축 도시가 공존하는 북부",
            "현대 목축 마을과 도시 시장에서 유제품·펠트·가죽 공예를 만들고 나누는 여러 생활권의 장면",
            "초원과 건조 고원|현대 목축 작업|펠트·유제품 시장",
            "유목민·게르·말 한 장면으로 모든 주민을 고정"),
        Cn("liaoning", "CN-21", "랴오닝성", "Liaoning", "辽宁省",
            지역문화행정구역유형Codes.ChinaProvince,
            "요동반도 해안, 랴오허 평야와 동부 산지가 이어지는 동북 지역",
            "항구·조선 도시와 과수·옥수수 농지의 주민이 해산물·사과·공예를 시장에서 나누는 생활",
            "요동반도 항구|랴오허 평야|해산물·과수 시장",
            "요동 역사권과 현재 랴오닝 행정구역을 동일시"),
        Cn("jilin", "CN-22", "지린성", "Jilin", "吉林省",
            지역문화행정구역유형Codes.ChinaProvince,
            "창바이산 산림, 쑹화강과 중부 평야가 이어지는 동북 내륙",
            "산림·농업 마을과 산업도시의 주민이 목공·옥수수·버섯과 여러 민족의 음식을 나누는 시장",
            "창바이산 숲과 강|목공·산림 생산물|옥수수·버섯 시장",
            "설경·조선족 복식·만주 문화 하나로 지역 전체를 고정"),
        Cn("heilongjiang", "CN-23", "헤이룽장성", "Heilongjiang", "黑龙江省",
            지역문화행정구역유형Codes.ChinaProvince,
            "대흥안령 삼림, 넓은 흑토 평야와 큰 강이 있는 최북동부",
            "혹한기 도시와 농촌에서 목재·곡물·어업 생산물을 보관하고 겨울 음식을 준비하는 공동체 생활",
            "침엽수림과 흑토 평야|겨울 작업 시장|곡물·강 어업",
            "얼음축제와 러시아풍 건축만으로 대표"),
        Cn("shanghai", "CN-31", "상하이시", "Shanghai", "上海市",
            지역문화행정구역유형Codes.ChinaMunicipality,
            "장강 하구의 충적평야와 항만·수로가 발달한 초대형 도시",
            "리룽 골목, 하천 시장과 현대 항만도시의 주민이 직물·간식·생활용품을 나누는 일상",
            "장강 하구와 수로|리룽 생활 골목|직물·간식 시장",
            "스카이라인·와이탄·고급 소비만으로 대표"),
        Cn("jiangsu", "CN-32", "장쑤성", "Jiangsu", "江苏省",
            지역문화행정구역유형Codes.ChinaProvince,
            "장강·대운하, 호수와 저지대 수향, 북부 평야가 이어지는 동부",
            "수향 마을과 산업도시의 주민이 비단·자수·차와 담수 식재료를 시장에서 나누는 생활",
            "대운하와 수향|비단·자수 공방|차·담수 식재료 시장",
            "쑤저우 정원 하나로 장쑤 전체를 고정"),
        Cn("zhejiang", "CN-33", "저장성", "Zhejiang", "浙江省",
            지역문화행정구역유형Codes.ChinaProvince,
            "구릉과 차밭, 첸탕강·호수·섬 많은 동중국해 연안",
            "차 산지와 연안 어촌, 소상품 도시의 주민이 대나무·비단·수산물을 나누는 생활",
            "차밭과 구릉|섬·어항|대나무·비단·수산물 시장",
            "항저우 서호·차 한 종류로만 대표"),
        Cn("anhui", "CN-34", "안후이성", "Anhui", "安徽省",
            지역문화행정구역유형Codes.ChinaProvince,
            "회하 평야, 장강 유역과 남부 황산 산지가 이어지는 내륙",
            "남부 후이저우 마을의 목각·벼루 공예와 북부 곡물 생활이 지역 시장에서 만나는 장면",
            "황산과 회하 평야|후이저우 목각·벼루|차·곡물 시장",
            "흰 벽 검은 기와의 후이저우만으로 성 전체를 대표"),
        Cn("fujian", "CN-35", "푸젠성", "Fujian", "福建省",
            지역문화행정구역유형Codes.ChinaProvince,
            "산지가 해안 가까이 이어지고 차밭·하천·섬과 항구가 많은 남동부",
            "산지 차 마을과 민난·하카 생활권의 시장에서 차·도자·해산물을 준비하는 장면",
            "산지 차밭과 해안|토루 생활권의 공동 공간|차·도자·해산물",
            "토루·차·어민 한 이미지로 모든 생활권을 고정"),
        Cn("jiangxi", "CN-36", "장시성", "Jiangxi", "江西省",
            지역문화행정구역유형Codes.ChinaProvince,
            "포양호·간강 유역과 주변 구릉·산지가 이어지는 남동 내륙",
            "호수 어업과 논농사, 징더전 도자 공방이 지역 시장에서 만나는 생활",
            "포양호와 간강|도자 공방|쌀·담수어 시장",
            "징더전 도자 하나로 성 전체를 대표"),
        Cn("shandong", "CN-37", "산둥성", "Shandong", "山东省",
            지역문화행정구역유형Codes.ChinaProvince,
            "황해·보하이 연안의 반도와 화북 평야, 타이산 주변 구릉",
            "연안 장터와 밀 음식 작업대, 웨이팡 연 공방에서 주민이 어업·농업·공예를 함께 이어가는 생활",
            "산둥반도 연안과 평야|웨이팡 연 공방|밀 음식·마늘·수산물 시장",
            "서양식 등대마을·공자·타이산 한 요소로만 대표"),
        Cn("henan", "CN-41", "허난성", "Henan", "河南省",
            지역문화행정구역유형Codes.ChinaProvince,
            "황허 중류와 넓은 화북 평원, 서부 산지가 이어지는 중원",
            "곡물 시장과 면 음식 작업대, 전통 공연·도자·농기구 공방이 현대 생활과 만나는 장면",
            "황허와 중원 평야|면 음식 만들기|공연·도자 공방",
            "소림사 무술·고대 왕조 이미지만으로 대표"),
        Cn("hubei", "CN-42", "후베이성", "Hubei", "湖北省",
            지역문화행정구역유형Codes.ChinaProvince,
            "장강 중류, 한강과 수많은 호수, 서부 산지가 이어지는 중앙 지역",
            "강·호수의 어업과 도시 시장, 칠기·직조·연꽃 식재료가 만나는 생활",
            "장강과 호수|칠기·직조 공방|연근·담수어 시장",
            "우한 대도시나 삼국지 역사만으로 축소"),
        Cn("hunan", "CN-43", "후난성", "Hunan", "湖南省",
            지역문화행정구역유형Codes.ChinaProvince,
            "둥팅호와 샹강 유역, 서부·남부 산지가 이어지는 중남부",
            "호수·논농사와 샹 자수·대나무 공예, 매운 향신 식재료가 시장에서 만나는 생활",
            "둥팅호와 산지|샹 자수·대나무 공예|쌀·고추 시장",
            "매운 음식·마오쩌둥 역사·소수민족 복식만으로 대표"),
        Cn("guangdong", "CN-44", "광둥성", "Guangdong", "广东省",
            지역문화행정구역유형Codes.ChinaProvince,
            "주강 삼각주, 남중국해 연안과 북부 구릉이 이어지는 아열대 남부",
            "광저우·차오산·하카 등 여러 생활권의 주민이 차·도자·수산물·딤섬을 시장과 공방에서 준비하는 장면",
            "주강과 연안|차·도자 공방|수산물·딤섬 시장",
            "광저우 음식이나 초고층 도시 하나로 성 전체를 고정"),
        Cn("guangxi", "CN-45", "광시 좡족 자치구", "Guangxi", "广西壮族自治区",
            지역문화행정구역유형Codes.ChinaAutonomousRegion,
            "카르스트 산지, 강 유역과 베이부만 연안이 이어지는 남서부",
            "좡족을 포함한 여러 공동체의 직조·구리북 전승과 쌀국수·과일 시장이 현대 생활에서 만나는 장면",
            "카르스트 산과 강|직조 공방|쌀국수·열대과일 시장",
            "소수민족 축제복과 계림 풍경만으로 전체 지역을 고정"),
        Cn("hainan", "CN-46", "하이난성", "Hainan", "海南省",
            지역문화행정구역유형Codes.ChinaProvince,
            "남중국해의 열대 섬, 해안·우림·고지 농업이 공존",
            "어촌과 도시 시장에서 어업·코코넛·열대과일, 리족 등 지역 공동체의 직조가 현재 생활과 만나는 장면",
            "열대 해안과 우림|작업 어촌|직조·코코넛·열대과일",
            "리조트 해변·야자수·소수민족 복식만으로 대표"),
        Cn("chongqing", "CN-50", "충칭시", "Chongqing", "重庆市",
            지역문화행정구역유형Codes.ChinaMunicipality,
            "장강·자링강이 깊은 구릉과 협곡을 가르는 산악 대도시와 농촌권",
            "가파른 골목과 강변 부두, 대나무 공예·차·매운 공동 식사가 이어지는 현재 생활",
            "두 강과 산악 도시|계단 골목·부두|차·대나무·공동 식사",
            "야경·훠궈·사이버펑크 도시만으로 대표"),
        Cn("sichuan", "CN-51", "쓰촨성", "Sichuan", "四川省",
            지역문화행정구역유형Codes.ChinaProvince,
            "쓰촨분지, 서부 고산·협곡과 촘촘한 하천·농지가 공존",
            "찻집과 시장, 촉 자수·죽세공·장류 만들기가 여러 민족·생활권의 현재 일상과 만나는 장면",
            "쓰촨분지와 서부 산지|찻집·촉 자수|향신료·장류 시장",
            "판다·매운 음식·티베트 고산문화를 한 장에 장식적으로 혼합"),
        Cn("guizhou", "CN-52", "구이저우성", "Guizhou", "贵州省",
            지역문화행정구역유형Codes.ChinaProvince,
            "카르스트 고원, 깊은 계곡과 다습한 산지가 이어지는 남서부",
            "먀오·둥 등 여러 공동체의 은세공·자수·목조건축 기술과 발효음식이 현대 시장에서 이어지는 장면",
            "카르스트 산지와 계곡|은세공·자수 공방|발효음식·차 시장",
            "축제복·빈곤한 산촌·한 민족 이미지로 성 전체를 고정"),
        Cn("yunnan", "CN-53", "윈난성", "Yunnan", "云南省",
            지역문화행정구역유형Codes.ChinaProvince,
            "고산·협곡, 고원 호수와 아열대 국경지대가 겹치는 남서부",
            "차·꽃·버섯 시장과 직조·도자 공방에서 여러 민족 공동체의 현대 생활이 만나는 장면",
            "고산과 아열대 계곡|차·꽃·버섯 시장|직조·도자 공방",
            "소수민족 축제복과 오래된 마을을 관광 장식으로 나열"),
        Cn("tibet", "CN-54", "시짱(티베트) 자치구", "Tibet", "西藏自治区",
            지역문화행정구역유형Codes.ChinaAutonomousRegion,
            "티베트고원, 히말라야와 큰 강의 발원지, 고산 초원과 계곡 도시",
            "고원 마을과 도시 시장에서 목축·직조·목공·차 생활이 현대 일상으로 이어지는 장면",
            "고원 산맥과 강 계곡|현대 목축·시장 생활|직조·목공·차",
            "승려·사원·전통복식·야크만으로 주민 전체를 고정하거나 정치적 상징 사용"),
        Cn("shaanxi", "CN-61", "산시성(陕西)", "Shaanxi", "陕西省",
            지역문화행정구역유형Codes.ChinaProvince,
            "황토고원, 웨이허 평원과 친링산맥이 남북 생활권을 가르는 내륙",
            "면 음식·종이공예·피영 공방과 현대 도시·농촌 시장이 함께 이어지는 생활",
            "황토고원과 친링|피영·종이공예|면 음식·곡물 시장",
            "병마용·고대 수도·동굴집만으로 대표"),
        Cn("gansu", "CN-62", "간쑤성", "Gansu", "甘肃省",
            지역문화행정구역유형Codes.ChinaProvince,
            "황토고원, 허시회랑의 사막·오아시스와 고산 초원이 길게 이어지는 서북부",
            "오아시스 시장과 국수 작업대, 목각·직조·낙타·목축 생활이 여러 공동체의 현재 일상과 만나는 장면",
            "허시회랑과 오아시스|국수·곡물 시장|목각·직조 공방",
            "실크로드·사막·둔황 벽화만으로 성 전체를 고정"),
        Cn("qinghai", "CN-63", "칭하이성", "Qinghai", "青海省",
            지역문화행정구역유형Codes.ChinaProvince,
            "티베트고원 북동부, 큰 호수·초원·설산과 강 발원지가 이어지는 고지대",
            "고원 도시와 목축 마을에서 양모·직조·유제품·차를 여러 민족 공동체가 나누는 현대 생활",
            "칭하이호와 고원 초원|양모·직조 공방|유제품·차 시장",
            "티베트·후이·몽골 문화를 한 복식과 유목 이미지로 혼합"),
        Cn("ningxia", "CN-64", "닝샤 후이족 자치구", "Ningxia", "宁夏回族自治区",
            지역문화행정구역유형Codes.ChinaAutonomousRegion,
            "황허 관개 평야와 건조 산지·사막 가장자리가 만나는 서북부",
            "관개 농촌과 도시 시장에서 구기자·밀·양고기, 후이족을 포함한 지역 공동체의 음식과 공예가 이어지는 생활",
            "황허 관개 평야|건조 산지와 농지|구기자·밀·직조 시장",
            "사막·모스크·후이족 복식 하나로 전체 지역을 고정"),
        Cn("xinjiang", "CN-65", "신장 위구르 자치구", "Xinjiang", "新疆维吾尔自治区",
            지역문화행정구역유형Codes.ChinaAutonomousRegion,
            "톈산산맥, 타림·준가르 분지와 사막·오아시스 도시가 공존하는 광대한 서북부",
            "오아시스 시장에서 포도·과일·빵, 목공·직조·악기 제작이 여러 민족 공동체의 현재 생활과 만나는 장면",
            "톈산과 오아시스|과일·빵 시장|직조·목공·악기 공방",
            "사막 낙타·바자르·한 민족의 축제복으로 지역 전체를 고정하거나 정치적 상징 사용")
    ];

    private static SeedRow Us(
        string key,
        string postalCode,
        string koreanName,
        string englishName,
        string geography,
        string culture,
        string anchors,
        string avoid)
        => new(
            $"us-{key}",
            "US",
            $"US-{postalCode}",
            koreanName,
            englishName,
            englishName,
            지역문화행정구역유형Codes.UnitedStatesState,
            geography,
            culture,
            anchors,
            avoid);

    private static SeedRow Kr(
        string key,
        string subdivisionCode,
        string koreanName,
        string englishName,
        string regionTypeCode,
        string geography,
        string culture,
        string anchors,
        string avoid)
        => new(
            $"kr-{key}",
            "KR",
            subdivisionCode,
            koreanName,
            englishName,
            koreanName,
            regionTypeCode,
            geography,
            culture,
            anchors,
            avoid);

    private static SeedRow Cn(
        string key,
        string subdivisionCode,
        string koreanName,
        string englishName,
        string localName,
        string regionTypeCode,
        string geography,
        string culture,
        string anchors,
        string avoid)
        => new(
            $"cn-{key}",
            "CN",
            subdivisionCode,
            koreanName,
            englishName,
            localName,
            regionTypeCode,
            geography,
            culture,
            anchors,
            avoid);

    private sealed record SeedRow(
        string RegionKey,
        string CountryCode,
        string SubdivisionCode,
        string RegionNameKo,
        string RegionNameEn,
        string RegionNameLocal,
        string RegionTypeCode,
        string GeographySummaryKo,
        string CultureSummaryKo,
        string VisualAnchors,
        string AvoidExpressions);
}
