namespace Hongdal.WebApp.Models;

public sealed record WebAppPageLink(
    string Title,
    string Description,
    string Href,
    string Icon,
    string AreaLabel,
    string Status = "웹 검증 가능");
