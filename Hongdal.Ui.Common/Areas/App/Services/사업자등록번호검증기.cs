using System.Text;

namespace Hongdal.Ui.Common.Areas.App.Services;

public static class 사업자등록번호검증기
{
    private static readonly int[] Weights = [1, 3, 7, 1, 3, 7, 1, 3, 5];

    public static string 숫자만추출(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(capacity: 10);
        foreach (var ch in input)
        {
            if (char.IsDigit(ch))
            {
                builder.Append(ch);
                if (builder.Length == 10)
                {
                    break;
                }
            }
        }

        return builder.ToString();
    }

    public static string 표시형식(string? digits)
    {
        var value = 숫자만추출(digits);
        if (value.Length <= 3)
        {
            return value;
        }

        if (value.Length <= 5)
        {
            return $"{value[..3]}-{value[3..]}";
        }

        return $"{value[..3]}-{value[3..5]}-{value[5..]}";
    }

    public static bool 형식유효(string? digits)
    {
        var value = 숫자만추출(digits);
        return value.Length == 10;
    }

    public static bool 체크섬유효(string? digits)
    {
        var value = 숫자만추출(digits);
        if (value.Length != 10)
        {
            return false;
        }

        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            sum += (value[i] - '0') * Weights[i];
        }

        sum += ((value[8] - '0') * 5) / 10;
        var checkDigit = (10 - (sum % 10)) % 10;

        return checkDigit == (value[9] - '0');
    }
}
