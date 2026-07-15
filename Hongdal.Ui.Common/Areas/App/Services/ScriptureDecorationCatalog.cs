namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed record ScriptureLecturePlaylistSource(
    string PlaylistId,
    string Title)
{
    public string Url => $"https://www.youtube.com/playlist?list={Uri.EscapeDataString(PlaylistId)}";
}

public sealed record ScriptureDecorationSource(
    string ScriptureKey,
    string ScriptureTitle,
    string TraditionLabel,
    string Symbol,
    string ChannelName,
    IReadOnlyList<ScriptureLecturePlaylistSource> Playlists);

public sealed record ScriptureDecorationPalette(
    string PreviewBackground,
    string OuterUpaya,
    string OuterPrajna,
    string InnerCommunity,
    string InnerStore,
    string CenterGen,
    string Frame,
    string Labels,
    string ClosedHandle);

public sealed record ScriptureDecorationDefinition(
    string Key,
    string ScriptureTitle,
    string PackTitle,
    string TraditionLabel,
    string Symbol,
    string Summary,
    IReadOnlyList<string> MatchTerms,
    ScriptureDecorationPalette Palette,
    IReadOnlyList<ScriptureLecturePlaylistSource> Playlists)
{
    public string ProductKey => $"store-scripture-{Key}-v1";

    public string PackKey => $"home-theme-scripture-{Key}-v1";

    public ScriptureDecorationSource Source
        => new(Key, ScriptureTitle, TraditionLabel, Symbol, "홍익학당", Playlists);
}

/// <summary>
/// 홍익학당 공개 재생목록의 경전·고전 제목과 홍달 꾸미기 팩을 연결하는 카탈로그입니다.
/// 강의 영상이나 경전 원문을 상품에 포함하지 않고, 공개 제목에서 영감을 얻은 시각 테마만 제공합니다.
/// </summary>
public static class ScriptureDecorationCatalog
{
    public static IReadOnlyList<ScriptureDecorationDefinition> Definitions { get; } =
    [
        new(
            "prajna-diamond",
            "반야심경·금강경",
            "반야심경·금강경 맑은 지혜",
            "불교",
            "般若",
            "비움과 자비가 함께 흐르는 옥빛·금빛 홈 테마입니다.",
            ["반야심경", "금강경"],
            new("#F4F8F2", "#B45309", "#0F766E", "#173F3A", "#9A6B28", "#F5DFA3", "#D8C7A1", "#FFFDF4", "#0F766E"),
            [new("PLaNHcYq59k3yO3Rmf5m-uVz9ECeIMr3M0", "홍익학당 [불교철학] 반야심경과 금강경")]),
        new(
            "vimalakirti",
            "유마경",
            "유마경 재가의 연꽃",
            "불교",
            "維摩",
            "일상 한가운데의 수행을 연꽃빛과 짙은 청록으로 표현한 홈 테마입니다.",
            ["유마경"],
            new("#F8F2F5", "#BE185D", "#0F766E", "#3F1D31", "#9D6B53", "#F9A8D4", "#E7CBD8", "#FFF7FB", "#BE185D"),
            [new("PLaNHcYq59k3ya80BT2WpzWXh7bjbVwhk4", "윤홍식의 [유마경] 강의")]),
        new(
            "dhammapada",
            "법구경",
            "법구경 길 위의 등불",
            "불교",
            "法句",
            "짧은 가르침이 길을 밝히는 모습을 먹빛과 등불색으로 담은 홈 테마입니다.",
            ["법구경"],
            new("#F7F3E8", "#B45309", "#334155", "#171717", "#8B5E3C", "#F59E0B", "#D6C6A4", "#FFFBEB", "#D97706"),
            [new("PLaNHcYq59k3wFKLb_mRlB7Y7VsH_a1c8g", "윤홍식의 [법구경] 강의")]),
        new(
            "avatamsaka",
            "화엄경",
            "화엄경 인드라망",
            "불교",
            "華嚴",
            "서로 비추는 인드라망의 세계를 남보라와 별빛 금색으로 엮은 홈 테마입니다.",
            ["화엄경"],
            new("#F5F3FF", "#7C3AED", "#1D4ED8", "#25164A", "#A16207", "#FACC15", "#DDD6FE", "#FFFFFF", "#7C3AED"),
            [new("PLaNHcYq59k3xOCCCYMZx1HLpQctJbW8Cl", "윤홍식의 [화엄경] 강의")]),
        new(
            "awakening-faith",
            "대승기신론",
            "대승기신론 한마음",
            "불교",
            "一心",
            "한마음의 두 문을 따뜻한 적갈색과 깊은 남색의 균형으로 나타낸 홈 테마입니다.",
            ["대승기신론"],
            new("#F7F4EF", "#9F3A38", "#1E3A5F", "#241C22", "#8B5E3C", "#D8B36A", "#DACBB8", "#FFF9ED", "#9F3A38"),
            [new("PLaNHcYq59k3z20Pz5aep8psvaJRveONLG", "홍익학당 [불교철학] 대승기신론")]),
        new(
            "yogacara",
            "유식학",
            "유식학 마음의 층",
            "불교철학",
            "唯識",
            "마음의 여러 층과 전환을 청색의 깊이 차이로 구성한 홈 테마입니다.",
            ["유식학"],
            new("#EEF6FA", "#0E7490", "#1D4ED8", "#11233F", "#64748B", "#67E8F9", "#B9DCE7", "#F8FAFC", "#0E7490"),
            [new("PLaNHcYq59k3ymuHX8Mr3ZE1DG9pRkSm88", "윤홍식의 [유식학] 강의")]),
        new(
            "i-ching",
            "주역",
            "주역 변화의 괘",
            "역학",
            "易",
            "음양의 변화와 여덟 괘를 흑백·청동빛 대비로 정돈한 홈 테마입니다.",
            ["주역"],
            new("#F4F1E8", "#A33A2B", "#1E4E79", "#171717", "#8A5A30", "#C89B3C", "#D8CDB5", "#FFFCF2", "#6B4F2A"),
            [
                new("PLaNHcYq59k3yLJ1M0JsZrSvrqvGcAoozE", "홍익학당 [주역] 8괘편"),
                new("PLaNHcYq59k3yhMQKJodQfhDPa12PYb6ve", "홍익학당 [동양철학] 주역 강의")
            ]),
        new(
            "great-learning-mean",
            "대학·중용",
            "대학·중용 곧은 중심",
            "유교",
            "中庸",
            "수양에서 세상으로 이어지는 곧은 중심을 주홍과 옥빛으로 담은 홈 테마입니다.",
            ["대학과 중용", "대학·중용"],
            new("#F6F1E7", "#B33A2E", "#285943", "#1E2B25", "#8C6239", "#D9B44A", "#D8CCB1", "#FFFDF5", "#B33A2E"),
            [new("PLaNHcYq59k3whmh8PASsf1EynZNqE29hJ", "윤홍식의 [유교 철학의 핵심 대학과 중용] 강의")]),
        new(
            "mencius",
            "맹자",
            "맹자 호연지기",
            "유교",
            "浩然",
            "넓고 곧은 기상을 하늘빛과 대지색으로 펼친 홈 테마입니다.",
            ["맹자"],
            new("#EEF5F5", "#B44A3C", "#2F6F73", "#1D3334", "#8A6746", "#8FC7C2", "#C6D7D3", "#FAFFFE", "#2F6F73"),
            [new("PLaNHcYq59k3ymfQ8O1RfDfsge1CWEg7uy", "홍익학당 [동양철학] 맹자 강의")]),
        new(
            "analects",
            "논어",
            "논어 배움의 뜰",
            "유교",
            "仁",
            "배움과 어짐이 자라는 뜰을 솔잎색과 종이색으로 표현한 홈 테마입니다.",
            ["논어"],
            new("#F3F5ED", "#9D3D32", "#35614D", "#20352B", "#876540", "#B7C99B", "#D4D8C4", "#FFFEF7", "#35614D"),
            [new("PLaNHcYq59k3xxKP5aBEj_Ju2NBJvPx2oc", "홍익학당 [동양철학] 논어 강의")]),
        new(
            "zhuangzi",
            "장자",
            "장자 소요유",
            "도가",
            "逍遙",
            "걸림 없이 노니는 바람과 하늘을 옅은 청록·구름빛으로 만든 홈 테마입니다.",
            ["장자"],
            new("#EEF8F5", "#C06B3E", "#158A8A", "#164A4A", "#9B7653", "#9FE0D0", "#C9E4DC", "#FBFFFE", "#158A8A"),
            [new("PLaNHcYq59k3zpnpmSV1sZcca6ucULttgq", "홍익학당 [동양철학] 장자 강의")])
    ];

    public static ScriptureDecorationDefinition? FindByPlaylist(string? playlistId, string? playlistTitle)
    {
        if (!string.IsNullOrWhiteSpace(playlistId))
        {
            var byId = Definitions.FirstOrDefault(definition => definition.Playlists.Any(source =>
                string.Equals(source.PlaylistId, playlistId, StringComparison.OrdinalIgnoreCase)));
            if (byId is not null)
            {
                return byId;
            }
        }

        if (string.IsNullOrWhiteSpace(playlistTitle))
        {
            return null;
        }

        return Definitions.FirstOrDefault(definition => definition.MatchTerms.Any(term =>
            playlistTitle.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    public static IReadOnlyList<CommunityDecorationProduct> CreateProducts()
        => Definitions.Select(CreateProduct).ToArray();

    private static CommunityDecorationProduct CreateProduct(ScriptureDecorationDefinition definition)
    {
        var palette = definition.Palette;
        return new(
            definition.ProductKey,
            definition.PackKey,
            definition.PackTitle,
            "Hongdal Lecture Link",
            $"{definition.Summary} 홍익학당의 ‘{definition.ScriptureTitle}’ 관련 공개 재생목록과 이어집니다.",
            CommunityDecorationTarget.HomeNavigatorTheme,
            0,
            "KRW",
            [],
            HomeTheme: new(
                "1.0.0",
                "neutral-taegeuk-v1",
                "연결 테마",
                "홍달 앱 내 개인 사용 · 강의 콘텐츠 미포함",
                palette.PreviewBackground,
                new("outer-upaya", "바깥 방편", palette.OuterUpaya, AltText: $"{definition.ScriptureTitle} 방편 영역"),
                new("outer-prajna", "바깥 반야", palette.OuterPrajna, AltText: $"{definition.ScriptureTitle} 반야 영역"),
                new("inner-community", "커뮤니티", palette.InnerCommunity, AltText: $"{definition.ScriptureTitle} 커뮤니티 영역"),
                new("inner-store", "상점", palette.InnerStore, AltText: $"{definition.ScriptureTitle} 상점 영역"),
                new("center-gen", "가운데 간괘", palette.CenterGen, AltText: $"{definition.ScriptureTitle} 중심"),
                new("frame", "원형 테두리", palette.Frame, AltText: $"{definition.ScriptureTitle} 테두리"),
                new("labels", "라벨", palette.Labels, AltText: $"{definition.ScriptureTitle} 라벨"),
                new("closed-handle", "접힌 손잡이", palette.ClosedHandle, AltText: $"{definition.ScriptureTitle} 접힌 손잡이")),
            ScriptureSource: definition.Source);
    }
}
