namespace Hongdal.WebApp.Models;

public sealed record WebAppPageLink(
    string Title,
    string Description,
    string Href,
    string Icon,
    string AppName,
    string Status = "웹 검증 가능");
