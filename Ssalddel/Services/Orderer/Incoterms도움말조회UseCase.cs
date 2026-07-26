using Ssalddel.Contracts.Common.Localization;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Orderer;

public interface IIncoterms도움말조회UseCase
{
    Incoterms도움말응답 조회(string? 선택코드, string? 언어코드);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Application,
    "FOB, CIF, DDP의 비용·위험·보험 책임을 주문자용 그림 구간으로 투영합니다.",
    ContractType = typeof(IIncoterms도움말조회UseCase),
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.None,
    Boundary = "공식 ICC 근거를 교육용으로 요약할 뿐 계약 조건을 추천하거나 선택·저장하지 않습니다.")]
public sealed class Incoterms도움말조회UseCase : IIncoterms도움말조회UseCase
{
    private static readonly IReadOnlySet<string> 지원코드 =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            공동수입준비Incoterms코드.Fob,
            공동수입준비Incoterms코드.Cif,
            공동수입준비Incoterms코드.Ddp
        };

    public Incoterms도움말응답 조회(string? 선택코드, string? 언어코드)
    {
        var code = string.IsNullOrWhiteSpace(선택코드)
            ? 공동수입준비Incoterms코드.Fob
            : 선택코드.Trim().ToUpperInvariant();
        if (!지원코드.Contains(code))
        {
            throw new ArgumentException(
                "도움말은 현재 FOB, CIF, DDP만 지원합니다.",
                nameof(선택코드));
        }

        var language = DisplayLanguageCodes.Normalize(언어코드);
        return new Incoterms도움말응답
        {
            언어코드 = language,
            선택코드 = code,
            화면제목 = 문구(language, "인코텀즈가 무엇인가요?", "What are Incoterms?", "インコタームズとは？"),
            소개 = 문구(
                language,
                "판매자와 구매자가 운송비를 어디까지 부담하고, 운송 중 위험이 어디에서 넘어가는지 정한 국제 거래 규칙입니다.",
                "International trade rules that show how far the seller pays transport costs and where transit risk passes to the buyer.",
                "売主がどこまで輸送費を負担し、輸送中の危険がどこで買主へ移るかを示す国際取引規則です。"),
            장소표기안내 = 문구(
                language,
                "계약에는 코드만 쓰지 말고 지정 항구·장소와 “Incoterms® 2020”을 함께 적어야 합니다.",
                "A contract should state the named port or place and “Incoterms® 2020”, not the three-letter code alone.",
                "契約には3文字コードだけでなく、指定港・指定場所と「Incoterms® 2020」を併記します。"),
            항목목록 =
            [
                Fob(language),
                Cif(language),
                Ddp(language)
            ],
            공식출처목록 =
            [
                new Incoterms도움말출처
                {
                    출처명 = "ICC Incoterms® rules",
                    출처Url = "https://iccwbo.org/business-solutions/incoterms-rules/",
                    확인기준일 = "2026-07-26"
                },
                new Incoterms도움말출처
                {
                    출처명 = "ICC Incoterms® 2020",
                    출처Url = "https://iccwbo.org/business-solutions/incoterms-rules/incoterms-2020/",
                    확인기준일 = "2026-07-26"
                }
            ],
            면책안내 = 문구(
                language,
                "이 도움말은 이해를 돕는 요약입니다. 실제 계약·통관 전에는 계약서의 지정 장소, 보험 범위, 현지 법규와 전문가 검토를 확인하세요.",
                "This is an educational summary. Before contracting or customs clearance, confirm the named place, insurance cover, local law, and professional review.",
                "これは理解を助ける要約です。契約・通関前に、指定場所、保険範囲、現地法令、専門家の確認を行ってください。")
        };
    }

    private static Incoterms도움말항목 Fob(string language)
        => new()
        {
            코드 = 공동수입준비Incoterms코드.Fob,
            영문명 = "Free On Board",
            한줄요약 = 문구(
                language,
                "판매자가 출발항의 배 위에 싣고 나면, 이후 운임과 위험은 구매자가 맡아요.",
                "Once the seller loads the goods on the vessel at origin, the buyer takes the main freight and risk.",
                "売主が仕出港で本船に積み込んだ後は、主運賃と危険を買主が負担します。"),
            적용운송범위 = 문구(language, "해상·내수로 전용", "Sea and inland waterway only", "海上・内陸水路輸送のみ"),
            판매자책임요약 = 문구(
                language,
                "수출 통관을 하고 지정 선적항에서 본선에 적재합니다.",
                "Clears the goods for export and loads them on the vessel at the named port of shipment.",
                "輸出通関を行い、指定船積港で本船に積み込みます。"),
            구매자책임요약 = 문구(
                language,
                "주운송, 선택 보험, 수입 통관·관세와 도착 뒤 운송을 맡습니다.",
                "Arranges main carriage, optional insurance, import clearance and duties, and onward transport.",
                "主運送、任意保険、輸入通関・関税、到着後の輸送を手配します。"),
            비용이전설명 = 문구(
                language,
                "본선 적재 뒤의 주운송 비용은 구매자 부담",
                "Buyer pays the main-carriage costs after loading on board",
                "本船積込後の主運送費は買主負担"),
            위험이전설명 = 문구(
                language,
                "출발항에서 물품이 본선에 실리는 때 구매자에게 이전",
                "Risk passes when the goods are on board at the port of shipment",
                "仕出港で本船に積み込まれた時点で危険が買主へ移転"),
            판매자보험부보여부 = false,
            보험설명 = 문구(
                language,
                "판매자의 보험 의무가 없으므로 구매자가 필요 범위를 판단합니다.",
                "The seller has no insurance obligation; the buyer decides the cover needed.",
                "売主に保険義務はなく、買主が必要な補償を判断します。"),
            그림구간목록 =
            [
                구간(1, Incoterms도움말구간코드.판매자출고, 문구(language, "판매자 출고", "Seller dispatch", "売主出荷"), "Seller", "Seller"),
                구간(2, Incoterms도움말구간코드.수출통관선적항, 문구(language, "수출 통관·선적항", "Export · origin port", "輸出通関・船積港"), "Seller", "Seller"),
                구간(3, Incoterms도움말구간코드.본선적재, 문구(language, "본선 적재", "Loaded on board", "本船積込"), "Seller", "Buyer", 위험이전: true),
                구간(4, Incoterms도움말구간코드.주운송, 문구(language, "해상 운송", "Ocean carriage", "海上輸送"), "Buyer", "Buyer"),
                구간(5, Incoterms도움말구간코드.수입통관, 문구(language, "수입 통관·내륙운송", "Import · onward delivery", "輸入通関・国内輸送"), "Buyer", "Buyer")
            ]
        };

    private static Incoterms도움말항목 Cif(string language)
        => new()
        {
            코드 = 공동수입준비Incoterms코드.Cif,
            영문명 = "Cost, Insurance and Freight",
            한줄요약 = 문구(
                language,
                "판매자가 도착항까지 운임·보험을 내지만, 위험은 출발항에서 먼저 구매자에게 넘어가요.",
                "The seller pays freight and insurance to the destination port, but risk passes earlier at the origin port.",
                "売主は到着港まで運賃・保険を負担しますが、危険は仕出港で先に買主へ移ります。"),
            적용운송범위 = 문구(language, "해상·내수로 전용", "Sea and inland waterway only", "海上・内陸水路輸送のみ"),
            판매자책임요약 = 문구(
                language,
                "수출 통관·본선 적재 후 지정 도착항까지 운임과 최소 수준의 적하보험을 마련합니다.",
                "Clears and loads the goods, then arranges freight and minimum cargo insurance to the named destination port.",
                "輸出通関・本船積込後、指定到着港までの運賃と最低限の貨物保険を手配します。"),
            구매자책임요약 = 문구(
                language,
                "본선 적재 뒤 운송 위험을 부담하고, 수입 통관·관세와 도착 뒤 운송을 맡습니다.",
                "Bears transit risk after loading on board and handles import clearance, duties, and onward transport.",
                "本船積込後の輸送危険を負担し、輸入通関・関税・到着後輸送を担当します。"),
            비용이전설명 = 문구(
                language,
                "판매자가 지정 도착항까지 운임과 보험료를 부담",
                "Seller pays freight and insurance to the named destination port",
                "売主が指定到着港までの運賃・保険料を負担"),
            위험이전설명 = 문구(
                language,
                "비용 구간과 달리, 위험은 출발항 본선 적재 때 구매자에게 이전",
                "Unlike the cost line, risk passes to the buyer when loaded on board at origin",
                "費用区間とは異なり、危険は仕出港で本船積込時に買主へ移転"),
            판매자보험부보여부 = true,
            보험설명 = 문구(
                language,
                "판매자가 최소 수준의 적하보험을 마련합니다. 더 넓은 보장이 필요하면 별도로 합의하세요.",
                "The seller arranges minimum cargo cover. Agree separately if broader insurance is needed.",
                "売主が最低限の貨物保険を手配します。より広い補償は別途合意します。"),
            그림구간목록 =
            [
                구간(1, Incoterms도움말구간코드.판매자출고, 문구(language, "판매자 출고", "Seller dispatch", "売主出荷"), "Seller", "Seller"),
                구간(2, Incoterms도움말구간코드.수출통관선적항, 문구(language, "수출 통관·선적항", "Export · origin port", "輸出通関・船積港"), "Seller", "Seller"),
                구간(3, Incoterms도움말구간코드.본선적재, 문구(language, "본선 적재", "Loaded on board", "本船積込"), "Seller", "Buyer", 위험이전: true, 보험표시: true),
                구간(4, Incoterms도움말구간코드.주운송, 문구(language, "운임·보험은 도착항까지", "Freight · insurance to port", "運賃・保険は到着港まで"), "Seller", "Buyer", 보험표시: true),
                구간(5, Incoterms도움말구간코드.수입통관, 문구(language, "수입 통관·내륙운송", "Import · onward delivery", "輸入通関・国内輸送"), "Buyer", "Buyer")
            ]
        };

    private static Incoterms도움말항목 Ddp(string language)
        => new()
        {
            코드 = 공동수입준비Incoterms코드.Ddp,
            영문명 = "Delivered Duty Paid",
            한줄요약 = 문구(
                language,
                "판매자가 수입 통관·관세까지 맡아 지정 목적지에 하역 준비 상태로 가져와요.",
                "The seller handles import clearance and duties and brings the goods to the named destination ready for unloading.",
                "売主が輸入通関・関税まで負担し、指定目的地へ荷卸し準備ができた状態で届けます。"),
            적용운송범위 = 문구(language, "모든 운송 방식", "Any mode of transport", "すべての輸送手段"),
            판매자책임요약 = 문구(
                language,
                "수출·운송·수입 통관과 관세·세금을 맡고 지정 목적지까지 위험을 부담합니다.",
                "Handles export, carriage, import clearance, duties and taxes, and bears risk to the named destination.",
                "輸出・輸送・輸入通関、関税・税金を負担し、指定目的地まで危険を負担します。"),
            구매자책임요약 = 문구(
                language,
                "지정 목적지에서 물품을 인수하고 하역합니다.",
                "Takes delivery and unloads the goods at the named destination.",
                "指定目的地で物品を受け取り、荷卸しを行います。"),
            비용이전설명 = 문구(
                language,
                "판매자가 지정 목적지까지 통상 비용과 수입 관세·세금을 부담",
                "Seller bears normal costs and import duties and taxes to the named destination",
                "売主が指定目的地までの通常費用と輸入関税・税金を負担"),
            위험이전설명 = 문구(
                language,
                "지정 목적지에서 하역 준비된 상태로 구매자에게 제공할 때 이전",
                "Risk passes when the goods are placed at the buyer's disposal ready for unloading at destination",
                "指定目的地で荷卸し準備済みの状態で買主の処分に委ねた時点で移転"),
            판매자보험부보여부 = false,
            보험설명 = 문구(
                language,
                "보험 의무가 별도로 정해진 것은 아니지만, 목적지까지 위험을 지는 판매자가 필요 범위를 판단합니다.",
                "There is no separate insurance obligation, but the seller bears risk to destination and decides the cover needed.",
                "保険義務は別途定められていませんが、目的地まで危険を負う売主が必要な補償を判断します。"),
            그림구간목록 =
            [
                구간(1, Incoterms도움말구간코드.판매자출고, 문구(language, "판매자 출고", "Seller dispatch", "売主出荷"), "Seller", "Seller"),
                구간(2, Incoterms도움말구간코드.수출통관선적항, 문구(language, "수출 통관", "Export clearance", "輸出通関"), "Seller", "Seller"),
                구간(3, Incoterms도움말구간코드.주운송, 문구(language, "국제 운송", "International carriage", "国際輸送"), "Seller", "Seller"),
                구간(4, Incoterms도움말구간코드.수입통관, 문구(language, "수입 통관·관세", "Import · duties", "輸入通関・関税"), "Seller", "Seller"),
                구간(5, Incoterms도움말구간코드.지정목적지, 문구(language, "지정 목적지", "Named destination", "指定目的地"), "Seller", "Buyer", 위험이전: true),
                구간(6, Incoterms도움말구간코드.하역, 문구(language, "구매자 하역", "Buyer unloads", "買主荷卸し"), "Buyer", "Buyer")
            ]
        };

    private static Incoterms도움말그림구간 구간(
        int 순서,
        string 코드,
        string 표시명,
        string 비용부담역할코드,
        string 위험부담역할코드,
        bool 위험이전 = false,
        bool 보험표시 = false)
        => new()
        {
            순서 = 순서,
            구간코드 = 코드,
            표시명 = 표시명,
            비용부담역할코드 = 비용부담역할코드,
            위험부담역할코드 = 위험부담역할코드,
            위험이전지점여부 = 위험이전,
            보험표시여부 = 보험표시
        };

    private static string 문구(string language, string korean, string english, string japanese)
        => DisplayLanguageCodes.Select(language, korean, english, japanese);
}
