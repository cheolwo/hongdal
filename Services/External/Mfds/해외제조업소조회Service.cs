using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Options;

namespace 홍달.Services.External.Mfds
{
    public interface I해외제조업소조회Service
    {
        Task<해외제조업소조회응답> 조회Async(해외제조업소조회요청 요청, CancellationToken 취소토큰 = default);
    }

    public sealed class 해외제조업소조회Service : I해외제조업소조회Service
    {
        private readonly HttpClient _httpClient;
        private readonly 해외제조업소조회Options _옵션;

        public 해외제조업소조회Service(HttpClient httpClient, IOptions<해외제조업소조회Options> 옵션)
        {
            _httpClient = httpClient;
            _옵션 = 옵션.Value;

            if (string.IsNullOrWhiteSpace(_옵션.ServiceKey))
            {
                throw new InvalidOperationException("해외제조업소조회:ServiceKey 설정이 필요합니다.");
            }
        }

        public async Task<해외제조업소조회응답> 조회Async(해외제조업소조회요청 요청, CancellationToken 취소토큰 = default)
        {
            ArgumentNullException.ThrowIfNull(요청);

            var 페이지번호 = 요청.페이지번호 <= 0 ? 1 : 요청.페이지번호;
            var 한페이지결과수 = 요청.한페이지결과수 <= 0 ? 10 : 요청.한페이지결과수;
            var 데이터형식 = 문자열형식정리(string.IsNullOrWhiteSpace(요청.데이터형식) ? _옵션.DefaultType : 요청.데이터형식);

            var 요청주소 = 요청주소생성(new Dictionary<string, string?>
            {
                ["serviceKey"] = _옵션.ServiceKey,
                ["pageNo"] = 페이지번호.ToString(CultureInfo.InvariantCulture),
                ["numOfRows"] = 한페이지결과수.ToString(CultureInfo.InvariantCulture),
                ["type"] = 데이터형식,
                ["OCTR_MNFT_BSSH_NM"] = 요청.해외제조업소명,
                ["FOOD_SE_NM"] = 요청.식품구분명,
                ["NATN_NM"] = 요청.국가명
            });

            using var 응답 = await _httpClient.GetAsync(요청주소, 취소토큰);
            응답.EnsureSuccessStatusCode();

            var 본문텍스트 = await 응답.Content.ReadAsStringAsync(취소토큰);
            return string.Equals(데이터형식, "json", StringComparison.OrdinalIgnoreCase)
                ? JSON응답파싱(본문텍스트)
                : XML응답파싱(본문텍스트);
        }

        private string 요청주소생성(IReadOnlyDictionary<string, string?> 매개변수목록)
        {
            var 빌더 = new StringBuilder(_옵션.Endpoint.TrimStart('/'));
            var 첫매개변수인지여부 = true;

            foreach (var 항목 in 매개변수목록)
            {
                if (string.IsNullOrWhiteSpace(항목.Value))
                {
                    continue;
                }

                빌더.Append(첫매개변수인지여부 ? '?' : '&');
                빌더.Append(항목.Key);
                빌더.Append('=');
                빌더.Append(Uri.EscapeDataString(항목.Value!));
                첫매개변수인지여부 = false;
            }

            return 빌더.ToString();
        }

        private static 해외제조업소조회응답 XML응답파싱(string 본문텍스트)
        {
            var 문서 = XDocument.Parse(본문텍스트);
            var 루트 = 문서.Root;

            var 헤더 = 첫자식찾기(루트, "header");
            var 본문 = 첫자식찾기(루트, "body");
            var 항목목록 = 모든자손찾기(본문 ?? 루트, "item").Select(XML항목변환).ToList();

            return new 해외제조업소조회응답
            {
                헤더 = new 해외제조업소조회헤더
                {
                    결과코드 = 문자열찾기(헤더, "resultCode") ?? 문자열찾기(루트, "resultCode"),
                    결과메시지 = 문자열찾기(헤더, "resultMsg") ?? 문자열찾기(루트, "resultMsg")
                },
                본문 = new 해외제조업소조회본문
                {
                    한페이지결과수 = 정수찾기(본문 ?? 루트, "numOfRows") ?? 0,
                    페이지번호 = 정수찾기(본문 ?? 루트, "pageNo") ?? 0,
                    전체결과수 = 정수찾기(본문 ?? 루트, "totalCount") ?? 0,
                    아이템 = new 해외제조업소조회아이템목록
                    {
                        항목 = 항목목록
                    }
                }
            };
        }

        private static 해외제조업소조회응답 JSON응답파싱(string 본문텍스트)
        {
            using var 문서 = JsonDocument.Parse(본문텍스트);
            var 루트 = 문서.RootElement;
            var 응답영역 = 속성찾기(루트, "response") ?? 루트;
            var 헤더영역 = 속성찾기(응답영역, "header");
            var 본문영역 = 속성찾기(응답영역, "body");
            var 항목목록 = JSON항목찾기(본문영역 ?? 응답영역).Select(JSON항목변환).ToList();

            return new 해외제조업소조회응답
            {
                헤더 = new 해외제조업소조회헤더
                {
                    결과코드 = JSON문자열찾기(헤더영역, "resultCode") ?? JSON문자열찾기(응답영역, "resultCode"),
                    결과메시지 = JSON문자열찾기(헤더영역, "resultMsg") ?? JSON문자열찾기(응답영역, "resultMsg")
                },
                본문 = new 해외제조업소조회본문
                {
                    한페이지결과수 = JSON정수찾기(본문영역 ?? 응답영역, "numOfRows") ?? 0,
                    페이지번호 = JSON정수찾기(본문영역 ?? 응답영역, "pageNo") ?? 0,
                    전체결과수 = JSON정수찾기(본문영역 ?? 응답영역, "totalCount") ?? 0,
                    아이템 = new 해외제조업소조회아이템목록
                    {
                        항목 = 항목목록
                    }
                }
            };
        }

        private static 해외제조업소조회항목 XML항목변환(XElement 항목)
        {
            var 결과 = new 해외제조업소조회항목
            {
                해외제조업소코드 = 문자열찾기(항목, "OCTR_MNFT_BSSH_CD"),
                해외제조업소명 = 문자열찾기(항목, "OCTR_MNFT_BSSH_NM"),
                해외제조업소주소 = 문자열찾기(항목, "OCTR_MNFT_BSSH_ADDR"),
                영업구분코드 = 문자열찾기(항목, "OCTR_MNFT_ENTP_BSN_DIVS_CD"),
                영업구분명 = 문자열찾기(항목, "OCTR_MNFT_ENTP_BSN_DIVS_NM"),
                식품구분코드 = 문자열찾기(항목, "FOOD_SE_CD"),
                식품구분명 = 문자열찾기(항목, "FOOD_SE_NM"),
                시설취소철회일 = 문자열찾기(항목, "FCLT_RTRCN_DT"),
                국가코드 = 문자열찾기(항목, "NATN_CD"),
                국가명 = 문자열찾기(항목, "NATN_NM"),
                지역코드 = 문자열찾기(항목, "AREA_CD"),
                지역명 = 문자열찾기(항목, "AREA_NM"),
                식품안전관리시스템인증여부 = 문자열찾기(항목, "FOOD_SAFE_MNG_SYS_CERT_YN"),
                인증명 = 문자열찾기(항목, "CERT_NM"),
                인증기관명 = 문자열찾기(항목, "CERT_INST_NM"),
                인증기관인증일 = 문자열찾기(항목, "CERT_INST_CERT_DT"),
                인증기관만료일 = 문자열찾기(항목, "CERT_INST_EXPRN_DT"),
                단종여부 = 문자열찾기(항목, "DSCTN_YN"),
                단종일 = 문자열찾기(항목, "DSCTN_DT"),
                취소중단코드 = 문자열찾기(항목, "RTRCN_SUSP_CD"),
                취소중단명 = 문자열찾기(항목, "RTRCN_SUSP_NM"),
                수동등록구분코드 = 문자열찾기(항목, "PASV_REG_DIVS_CD"),
                식품유통시작일 = 문자열찾기(항목, "FOOD_SLDT_BGNG_DT"),
                식품유통종료일 = 문자열찾기(항목, "FOOD_SLDT_END_DT"),
                수산시작일 = 문자열찾기(항목, "MARN_BGNG_DT"),
                수입중단번호 = 문자열찾기(항목, "IPRT_SUSP_NO")
            };

            결과.주의필요여부 = 결과.주의필요조건충족();
            결과.주의사유 = 결과.주의필요여부 ? 결과.주의사유생성() : null;
            return 결과;
        }

        private static 해외제조업소조회항목 JSON항목변환(JsonElement 항목)
        {
            var 결과 = new 해외제조업소조회항목
            {
                해외제조업소코드 = JSON문자열찾기(항목, "OCTR_MNFT_BSSH_CD"),
                해외제조업소명 = JSON문자열찾기(항목, "OCTR_MNFT_BSSH_NM"),
                해외제조업소주소 = JSON문자열찾기(항목, "OCTR_MNFT_BSSH_ADDR"),
                영업구분코드 = JSON문자열찾기(항목, "OCTR_MNFT_ENTP_BSN_DIVS_CD"),
                영업구분명 = JSON문자열찾기(항목, "OCTR_MNFT_ENTP_BSN_DIVS_NM"),
                식품구분코드 = JSON문자열찾기(항목, "FOOD_SE_CD"),
                식품구분명 = JSON문자열찾기(항목, "FOOD_SE_NM"),
                시설취소철회일 = JSON문자열찾기(항목, "FCLT_RTRCN_DT"),
                국가코드 = JSON문자열찾기(항목, "NATN_CD"),
                국가명 = JSON문자열찾기(항목, "NATN_NM"),
                지역코드 = JSON문자열찾기(항목, "AREA_CD"),
                지역명 = JSON문자열찾기(항목, "AREA_NM"),
                식품안전관리시스템인증여부 = JSON문자열찾기(항목, "FOOD_SAFE_MNG_SYS_CERT_YN"),
                인증명 = JSON문자열찾기(항목, "CERT_NM"),
                인증기관명 = JSON문자열찾기(항목, "CERT_INST_NM"),
                인증기관인증일 = JSON문자열찾기(항목, "CERT_INST_CERT_DT"),
                인증기관만료일 = JSON문자열찾기(항목, "CERT_INST_EXPRN_DT"),
                단종여부 = JSON문자열찾기(항목, "DSCTN_YN"),
                단종일 = JSON문자열찾기(항목, "DSCTN_DT"),
                취소중단코드 = JSON문자열찾기(항목, "RTRCN_SUSP_CD"),
                취소중단명 = JSON문자열찾기(항목, "RTRCN_SUSP_NM"),
                수동등록구분코드 = JSON문자열찾기(항목, "PASV_REG_DIVS_CD"),
                식품유통시작일 = JSON문자열찾기(항목, "FOOD_SLDT_BGNG_DT"),
                식품유통종료일 = JSON문자열찾기(항목, "FOOD_SLDT_END_DT"),
                수산시작일 = JSON문자열찾기(항목, "MARN_BGNG_DT"),
                수입중단번호 = JSON문자열찾기(항목, "IPRT_SUSP_NO")
            };

            결과.주의필요여부 = 결과.주의필요조건충족();
            결과.주의사유 = 결과.주의필요여부 ? 결과.주의사유생성() : null;
            return 결과;
        }

        private static XElement? 첫자식찾기(XElement? 요소, string 이름)
        {
            return 요소?.Elements().FirstOrDefault(x => string.Equals(x.Name.LocalName, 이름, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<XElement> 모든자손찾기(XElement? 요소, string 이름)
        {
            return 요소?.Descendants().Where(x => string.Equals(x.Name.LocalName, 이름, StringComparison.OrdinalIgnoreCase))
                ?? Enumerable.Empty<XElement>();
        }

        private static string? 문자열찾기(XElement? 요소, string 이름)
        {
            return 요소?.Elements().FirstOrDefault(x => string.Equals(x.Name.LocalName, 이름, StringComparison.OrdinalIgnoreCase))?.Value
                ?? 요소?.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, 이름, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        private static int? 정수찾기(XElement? 요소, string 이름)
        {
            var 값 = 문자열찾기(요소, 이름);
            return int.TryParse(값, NumberStyles.Any, CultureInfo.InvariantCulture, out var 결과) ? 결과 : null;
        }

        private static JsonElement? 속성찾기(JsonElement 요소, string 이름)
        {
            if (요소.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var 속성 in 요소.EnumerateObject())
            {
                if (string.Equals(속성.Name, 이름, StringComparison.OrdinalIgnoreCase))
                {
                    return 속성.Value;
                }
            }

            return null;
        }

        private static IEnumerable<JsonElement> JSON항목찾기(JsonElement 요소)
        {
            var 목록 = new List<JsonElement>();
            var 항목영역 = 속성찾기(요소, "items");
            if (항목영역.HasValue)
            {
                var 값 = 항목영역.Value;
                if (값.ValueKind == JsonValueKind.Array)
                {
                    목록.AddRange(값.EnumerateArray());
                    return 목록;
                }

                var 내부항목 = 속성찾기(값, "item");
                if (내부항목.HasValue && 내부항목.Value.ValueKind == JsonValueKind.Array)
                {
                    목록.AddRange(내부항목.Value.EnumerateArray());
                    return 목록;
                }
            }

            var 직접항목 = 속성찾기(요소, "item");
            if (직접항목.HasValue)
            {
                if (직접항목.Value.ValueKind == JsonValueKind.Array)
                {
                    목록.AddRange(직접항목.Value.EnumerateArray());
                }
                else if (직접항목.Value.ValueKind == JsonValueKind.Object)
                {
                    목록.Add(직접항목.Value);
                }
            }

            return 목록;
        }

        private static string? JSON문자열찾기(JsonElement? 요소, string 이름)
        {
            if (!요소.HasValue)
            {
                return null;
            }

            var 속성 = 속성찾기(요소.Value, 이름);
            return 속성.HasValue ? 속성.Value.ToString() : null;
        }

        private static int? JSON정수찾기(JsonElement? 요소, string 이름)
        {
            if (!요소.HasValue)
            {
                return null;
            }

            var 속성 = 속성찾기(요소.Value, 이름);
            if (!속성.HasValue)
            {
                return null;
            }

            return 속성.Value.ValueKind switch
            {
                JsonValueKind.Number when 속성.Value.TryGetInt32(out var 결과) => 결과,
                JsonValueKind.String when int.TryParse(속성.Value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var 결과) => 결과,
                _ => null
            };
        }

        private static string 문자열형식정리(string 값)
        {
            var 정리된값 = 값.Trim().ToLowerInvariant();
            return 정리된값 == "json" ? "json" : "xml";
        }
    }

    public sealed class 해외제조업소조회요청
    {
        public int 페이지번호 { get; set; } = 1;
        public int 한페이지결과수 { get; set; } = 10;
        public string 데이터형식 { get; set; } = "xml";
        public string? 해외제조업소명 { get; set; }
        public string? 식품구분명 { get; set; }
        public string? 국가명 { get; set; }
    }

    public sealed class 해외제조업소조회응답
    {
        public 해외제조업소조회헤더? 헤더 { get; set; }
        public 해외제조업소조회본문? 본문 { get; set; }
    }

    public sealed class 해외제조업소조회헤더
    {
        public string? 결과코드 { get; set; }
        public string? 결과메시지 { get; set; }
    }

    public sealed class 해외제조업소조회본문
    {
        public int 한페이지결과수 { get; set; }
        public int 페이지번호 { get; set; }
        public int 전체결과수 { get; set; }
        public 해외제조업소조회아이템목록? 아이템 { get; set; }
    }

    public sealed class 해외제조업소조회아이템목록
    {
        public List<해외제조업소조회항목> 항목 { get; set; } = [];
    }

    public sealed class 해외제조업소조회항목
    {
        public string? 해외제조업소코드 { get; set; }
        public string? 해외제조업소명 { get; set; }
        public string? 해외제조업소주소 { get; set; }
        public string? 영업구분코드 { get; set; }
        public string? 영업구분명 { get; set; }
        public string? 식품구분코드 { get; set; }
        public string? 식품구분명 { get; set; }
        public string? 시설취소철회일 { get; set; }
        public string? 국가코드 { get; set; }
        public string? 국가명 { get; set; }
        public string? 지역코드 { get; set; }
        public string? 지역명 { get; set; }
        public string? 식품안전관리시스템인증여부 { get; set; }
        public string? 인증명 { get; set; }
        public string? 인증기관명 { get; set; }
        public string? 인증기관인증일 { get; set; }
        public string? 인증기관만료일 { get; set; }
        public string? 단종여부 { get; set; }
        public string? 단종일 { get; set; }
        public string? 취소중단코드 { get; set; }
        public string? 취소중단명 { get; set; }
        public string? 수동등록구분코드 { get; set; }
        public string? 식품유통시작일 { get; set; }
        public string? 식품유통종료일 { get; set; }
        public string? 수산시작일 { get; set; }
        public string? 수입중단번호 { get; set; }
        public bool 주의필요여부 { get; set; }
        public string? 주의사유 { get; set; }

        public bool 주의필요조건충족()
        {
            return !string.IsNullOrWhiteSpace(시설취소철회일)
                || !string.IsNullOrWhiteSpace(단종여부) && 단종여부.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(단종일)
                || !string.IsNullOrWhiteSpace(취소중단코드)
                || !string.IsNullOrWhiteSpace(취소중단명)
                || !string.IsNullOrWhiteSpace(수입중단번호);
        }

        public string 주의사유생성()
        {
            var 사유목록 = new List<string>();

            if (!string.IsNullOrWhiteSpace(시설취소철회일)) 사유목록.Add($"시설 취소/철회일: {시설취소철회일}");
            if (!string.IsNullOrWhiteSpace(단종여부)) 사유목록.Add($"단종 여부: {단종여부}");
            if (!string.IsNullOrWhiteSpace(단종일)) 사유목록.Add($"단종일: {단종일}");
            if (!string.IsNullOrWhiteSpace(취소중단명)) 사유목록.Add($"취소·중단: {취소중단명}");
            if (!string.IsNullOrWhiteSpace(수입중단번호)) 사유목록.Add($"수입중단번호: {수입중단번호}");

            return 사유목록.Count == 0 ? string.Empty : string.Join("; ", 사유목록);
        }
    }
}
