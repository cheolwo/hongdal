using System.Text;

namespace Hongdal.Contracts.Common.Community;

internal static class 노드스티커샘플이미지UrlFactory
{
    public static string 생성(string 강조색상, string 글자, string 라벨)
    {
        var svg = $$"""
            <svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
              <rect width="512" height="512" rx="128" fill="#ffffff"/>
              <circle cx="256" cy="230" r="148" fill="{{강조색상}}" opacity="0.16"/>
              <circle cx="256" cy="222" r="112" fill="{{강조색상}}"/>
              <text x="256" y="263" text-anchor="middle" font-size="104" font-family="Arial, sans-serif" font-weight="700" fill="#ffffff">{{글자}}</text>
              <rect x="126" y="354" width="260" height="58" rx="29" fill="#111827" opacity="0.9"/>
              <text x="256" y="393" text-anchor="middle" font-size="32" font-family="Arial, sans-serif" font-weight="700" fill="#ffffff">{{라벨}}</text>
            </svg>
            """;

        return $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(svg))}";
    }
}
