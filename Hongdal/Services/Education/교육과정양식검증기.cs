using System.Net.Mail;
using System.Text.Json;
using Hongdal.Contracts.Common.Education;

namespace Hongdal.Services.Education;

public static class 교육과정양식검증기
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string 필드정의직렬화(IReadOnlyList<교육과정양식필드Dto> 필드목록)
        => JsonSerializer.Serialize(필드목록, JsonOptions);

    public static IReadOnlyList<교육과정양식필드Dto> 필드정의역직렬화(string json)
        => JsonSerializer.Deserialize<IReadOnlyList<교육과정양식필드Dto>>(json, JsonOptions) ?? [];

    public static IReadOnlyList<string> 필드정의검증(IReadOnlyList<교육과정양식필드Dto>? 필드목록)
    {
        if (필드목록 is null)
        {
            return ["양식 필드목록이 필요합니다."];
        }

        var errors = new List<string>();
        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var field in 필드목록)
        {
            if (string.IsNullOrWhiteSpace(field.Key) || field.Key.Length > 100)
            {
                errors.Add("양식 필드 Key는 1~100자로 입력해야 합니다.");
            }
            else if (!keys.Add(field.Key))
            {
                errors.Add($"양식 필드 Key '{field.Key}'가 중복되었습니다.");
            }

            if (string.IsNullOrWhiteSpace(field.라벨) || field.라벨.Length > 200)
            {
                errors.Add($"양식 필드 '{field.Key}'의 라벨은 1~200자로 입력해야 합니다.");
            }

            if (!교육과정양식필드유형.지원여부(field.유형))
            {
                errors.Add($"양식 필드 '{field.Key}'의 유형 '{field.유형}'은 지원하지 않습니다.");
            }

            if (field.최대길이 is < 1 or > 20_000)
            {
                errors.Add($"양식 필드 '{field.Key}'의 최대길이는 1~20000이어야 합니다.");
            }

            if (field.유형 == 교육과정양식필드유형.단일선택 && field.선택목록.Count == 0)
            {
                errors.Add($"단일선택 필드 '{field.Key}'에는 선택목록이 필요합니다.");
            }
            if (field.참값필수여부 && field.유형 != 교육과정양식필드유형.참거짓)
            {
                errors.Add($"양식 필드 '{field.Key}'의 참값필수여부는 참거짓 유형에만 사용할 수 있습니다.");
            }
        }

        return errors;
    }

    public static IReadOnlyList<string> 답변검증(
        IReadOnlyList<교육과정양식필드Dto> 필드목록,
        IReadOnlyDictionary<string, JsonElement>? 답변)
    {
        var errors = new List<string>();
        if (답변 is null)
        {
            return ["양식 답변이 필요합니다."];
        }

        var fieldsByKey = 필드목록.ToDictionary(x => x.Key, StringComparer.Ordinal);
        foreach (var key in 답변.Keys.Where(x => !fieldsByKey.ContainsKey(x)))
        {
            errors.Add($"양식에 정의되지 않은 답변 '{key}'가 포함되어 있습니다.");
        }

        foreach (var field in 필드목록)
        {
            var hasValue = 답변.TryGetValue(field.Key, out var value) && !IsEmpty(value);
            if (field.필수여부 && !hasValue)
            {
                errors.Add($"'{field.라벨}' 항목은 필수입니다.");
                continue;
            }

            if (!hasValue)
            {
                continue;
            }

            ValidateValue(field, value, errors);
        }

        var serializedLength = JsonSerializer.Serialize(답변, JsonOptions).Length;
        if (serializedLength > 100_000)
        {
            errors.Add("양식 답변 전체 크기는 100000자를 넘을 수 없습니다.");
        }

        return errors;
    }

    private static void ValidateValue(
        교육과정양식필드Dto field,
        JsonElement value,
        ICollection<string> errors)
    {
        if (field.유형 == 교육과정양식필드유형.숫자)
        {
            if (value.ValueKind != JsonValueKind.Number)
            {
                errors.Add($"'{field.라벨}' 항목은 숫자여야 합니다.");
            }
            return;
        }

        if (field.유형 == 교육과정양식필드유형.참거짓)
        {
            if (value.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
            {
                errors.Add($"'{field.라벨}' 항목은 참 또는 거짓이어야 합니다.");
            }
            else if (field.참값필수여부 && value.ValueKind != JsonValueKind.True)
            {
                errors.Add($"'{field.라벨}' 항목에 동의하거나 확인해야 합니다.");
            }
            return;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            errors.Add($"'{field.라벨}' 항목은 문자열이어야 합니다.");
            return;
        }

        var text = value.GetString() ?? string.Empty;
        if (text.Length > field.최대길이)
        {
            errors.Add($"'{field.라벨}' 항목은 {field.최대길이}자를 넘을 수 없습니다.");
        }

        if (field.유형 == 교육과정양식필드유형.단일선택 &&
            !field.선택목록.Contains(text, StringComparer.Ordinal))
        {
            errors.Add($"'{field.라벨}' 항목의 선택값이 올바르지 않습니다.");
        }
        else if (field.유형 == 교육과정양식필드유형.날짜 && !DateTime.TryParse(text, out _))
        {
            errors.Add($"'{field.라벨}' 항목은 올바른 날짜여야 합니다.");
        }
        else if (field.유형 == 교육과정양식필드유형.이메일 && !MailAddress.TryCreate(text, out _))
        {
            errors.Add($"'{field.라벨}' 항목은 올바른 이메일 주소여야 합니다.");
        }
        else if (field.유형 == 교육과정양식필드유형.전화번호 && text.Count(char.IsDigit) < 8)
        {
            errors.Add($"'{field.라벨}' 항목은 올바른 전화번호여야 합니다.");
        }
    }

    private static bool IsEmpty(JsonElement value)
        => value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ||
           value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString());
}
