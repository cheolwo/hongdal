namespace Ssalddel.Domain.Content;

public sealed class 게임객체WI추출묶음
{
    public string BatchId { get; set; } = "";
    public string RequestHash { get; set; } = "";
    public string SourceHash { get; set; } = "";
    public string SourceRevision { get; set; } = "";
    public string ReviewerId { get; set; } = "";
    public DateTime AtUtc { get; set; }
}
public sealed class 게임객체WI참여
{
    public string SourceHash { get; set; } = "";
    public string UseId { get; set; } = "";
    public string BatchId { get; set; } = "";
    public string WorldInteractionId { get; set; } = "";
    public string? DefinitionId { get; set; }
    public string? DefinitionCompositionId { get; set; }
    public string Role { get; set; } = "";
    public string InputJson { get; set; } = "";
    public string InputHash { get; set; } = "";
}
