namespace Ssalddel.Ui.Common.Areas.App.Models;

public sealed record RolePurposeJourneyStep(
    string StateLabel,
    string Title,
    string Description,
    string Href,
    bool IsPrimary = false);
