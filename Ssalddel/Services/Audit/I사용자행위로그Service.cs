namespace 살뜰.Services.Audit;

public interface I사용자행위로그Service
{
    Task 기록Async(사용자행위로그기록 entry, CancellationToken cancellationToken = default);
}
