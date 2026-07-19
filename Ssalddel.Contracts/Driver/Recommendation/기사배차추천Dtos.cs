namespace Ssalddel.Contracts.Driver.Recommendation;

public sealed class 기사배차추천요약응답
{
    public int 전체추천수 { get; set; }
    public int 적합추천수 { get; set; }
    public int 운행중추천수 { get; set; }
    public int 비운행중추천수 { get; set; }
    public int 전국콜수 { get; set; }
}
