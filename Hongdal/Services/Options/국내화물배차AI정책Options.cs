namespace 홍달.Services.Options;

public sealed class 국내화물배차AI정책Options
{
    public const string SectionName = "DomesticCargoDispatchAI";

    public decimal 목표기사건당지급액 { get; set; } = 0m;

    public decimal 기사목표지급액미달패널티배수 { get; set; } = 3m;

    public decimal 기사목표지급액초과패널티배수 { get; set; } = 0.2m;

    public int 수익묶음최대묶음크기 { get; set; } = 3;

    public int 수익묶음최대조합탐색크기 { get; set; } = 4;

    public int 수익묶음최대묶음수최소값 { get; set; } = 50;

    public decimal 수익묶음거리원가기준Km당 { get; set; } = 900m;

    public decimal 수익묶음최소예상순이익 { get; set; } = 0m;

    public decimal 목표건당플랫폼순이익 { get; set; } = 500m;

    public bool 목표건당플랫폼순이익미달차단 { get; set; } = true;

    public decimal 목표수익미달패널티배수 { get; set; } = 3m;

    public decimal 목표수익회귀보너스배수 { get; set; } = 10m;

    public decimal 목표수익초과보너스배수 { get; set; } = 0.1m;

    public decimal 목표수익초과보너스상한 { get; set; } = 20_000m;

    public decimal 멀티묶음기본보너스 { get; set; } = 15_000m;

    public decimal 추가묶음건당보너스 { get; set; } = 3_000m;

    public decimal 멀티묶음원가보정비율 { get; set; } = 0.9m;

    public decimal 묶음추가건당원가보정감소폭 { get; set; } = 0.05m;

    public decimal 멀티묶음최소원가보정비율 { get; set; } = 0.75m;

    public decimal 같은배달권보너스 { get; set; } = 20_000m;

    public decimal 인접배달권보너스 { get; set; } = 8_000m;

    public decimal 외부배달권패널티 { get; set; } = 50_000m;

    public decimal 상차지근접권장Km { get; set; } = 5m;

    public decimal 상차지근접보너스 { get; set; } = 10_000m;

    public decimal 상차지분산패널티Km당 { get; set; } = 2_500m;

    public decimal 하차지근접권장Km { get; set; } = 8m;

    public decimal 하차지근접보너스 { get; set; } = 8_000m;

    public decimal 하차지분산패널티Km당 { get; set; } = 1_500m;

    public decimal 상차시간창권장차이분 { get; set; } = 60m;

    public decimal 상차시간창근접보너스 { get; set; } = 5_000m;

    public decimal 상차시간창차이패널티분당 { get; set; } = 50m;
}
