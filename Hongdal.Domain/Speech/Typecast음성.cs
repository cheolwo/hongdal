namespace Hongdal.Domain.Speech;

public sealed class Typecast음성
{
    public long Id { get; set; }

    public string VoiceId { get; set; } = string.Empty;

    public string 이름 { get; set; } = string.Empty;

    public string 성별 { get; set; } = string.Empty;

    public string 연령대 { get; set; } = string.Empty;

    public string 음성유형 { get; set; } = string.Empty;

    public bool 활성화여부 { get; set; } = true;

    public DateTime 마지막동기화일시Utc { get; set; } = DateTime.UtcNow;

    public DateTime 생성일시Utc { get; set; } = DateTime.UtcNow;

    public DateTime 수정일시Utc { get; set; } = DateTime.UtcNow;

    public ICollection<Typecast음성모델> 지원모델 { get; set; } = new List<Typecast음성모델>();

    public ICollection<Typecast음성용도> 용도 { get; set; } = new List<Typecast음성용도>();
}

public sealed class Typecast음성모델
{
    public long Id { get; set; }

    public long Typecast음성Id { get; set; }

    public Typecast음성? Typecast음성 { get; set; }

    public string 버전 { get; set; } = string.Empty;

    public string 지원감정Json { get; set; } = "[]";
}

public sealed class Typecast음성용도
{
    public long Id { get; set; }

    public long Typecast음성Id { get; set; }

    public Typecast음성? Typecast음성 { get; set; }

    public string 이름 { get; set; } = string.Empty;
}
