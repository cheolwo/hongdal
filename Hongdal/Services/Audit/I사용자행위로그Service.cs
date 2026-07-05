namespace 홍달.Services.Audit;

public interface I사용자행위로그Service
{
    Task 기록Async(사용자행위로그기록 entry, CancellationToken cancellationToken = default);
}
