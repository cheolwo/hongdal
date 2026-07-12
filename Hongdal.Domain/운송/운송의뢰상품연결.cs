namespace 홍달.도메인.운송;

public class 운송의뢰상품연결
{
    public long Id { get; set; }

    public string 운송의뢰Id { get; set; } = string.Empty;

    public long 입고상품Id { get; set; }

    public int 할당수량 { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
