using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using 홍달.도메인.기사;

namespace 홍달.Services.Settlement
{
    public interface I기사월정산Service
    {
        Task<기사월정산> 배차확정반영Async(string 기사Id, DateTime? 기준시각Utc = null, CancellationToken cancellationToken = default);
        Task<기사월정산> 월마감처리Async(string 기사Id, int 년도, int 월, DateTime? 기준시각Utc = null, CancellationToken cancellationToken = default);
        Task<기사월정산> 월말청구결제완료처리Async(string 기사Id, int 년도, int 월, DateTime? 기준시각Utc = null, CancellationToken cancellationToken = default);
    }

    public sealed class 기사월정산Service : I기사월정산Service
    {
        private readonly HongdalContext _db;
        private readonly 기사이용료정책Options _policy;
        private readonly ILogger<기사월정산Service> _logger;

        public 기사월정산Service(HongdalContext db, IOptions<기사이용료정책Options> policy, ILogger<기사월정산Service> logger)
        {
            _db = db;
            _policy = policy.Value;
            _logger = logger;
        }

        public async Task<기사월정산> 배차확정반영Async(string 기사Id, DateTime? 기준시각Utc = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(기사Id))
            {
                throw new ArgumentException("기사Id is required", nameof(기사Id));
            }

            var now = 기준시각Utc ?? DateTime.UtcNow;
            var settlement = await GetOrCreateAsync(기사Id, now.Year, now.Month, now, cancellationToken);

            settlement.배차건수 += 1;
            settlement.이용료 = CalculateFee(settlement.배차건수);
            settlement.결제완료 = false;
            settlement.UpdatedAt = now;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Action={Action} DriverId={DriverId} Year={Year} Month={Month} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
                "SettlementApplied",
                기사Id,
                settlement.년도,
                settlement.월,
                "Pending",
                settlement.결제완료 ? "Paid" : "Applied",
                "Success",
                System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
                now);
            return settlement;
        }

        public async Task<기사월정산> 월마감처리Async(string 기사Id, int 년도, int 월, DateTime? 기준시각Utc = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(기사Id))
            {
                throw new ArgumentException("기사Id is required", nameof(기사Id));
            }

            if (년도 < 1 || 월 < 1 || 월 > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(월), "월은 1~12 범위여야 합니다.");
            }

            var now = 기준시각Utc ?? DateTime.UtcNow;
            var settlement = await GetOrCreateAsync(기사Id, 년도, 월, now, cancellationToken);

            settlement.이용료 = CalculateFee(settlement.배차건수);
            settlement.결제완료 = true;
            settlement.UpdatedAt = now;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Action={Action} DriverId={DriverId} Year={Year} Month={Month} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
                "SettlementApplied",
                기사Id,
                년도,
                월,
                "Unpaid",
                "Paid",
                "Success",
                System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
                now);
            return settlement;
        }

        public Task<기사월정산> 월말청구결제완료처리Async(string 기사Id, int 년도, int 월, DateTime? 기준시각Utc = null, CancellationToken cancellationToken = default)
        {
            return 월마감처리Async(기사Id, 년도, 월, 기준시각Utc, cancellationToken);
        }

        private async Task<기사월정산> GetOrCreateAsync(string 기사Id, int 년도, int 월, DateTime now, CancellationToken cancellationToken)
        {
            var settlement = await _db.기사월정산
                .FirstOrDefaultAsync(x => x.기사Id == 기사Id && x.년도 == 년도 && x.월 == 월, cancellationToken);

            if (settlement != null)
            {
                return settlement;
            }

            settlement = new 기사월정산
            {
                기사Id = 기사Id,
                년도 = 년도,
                월 = 월,
                배차건수 = 0,
                이용료 = 0,
                결제완료 = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _db.기사월정산.AddAsync(settlement, cancellationToken);
            return settlement;
        }

        private decimal CalculateFee(int dispatchCount)
        {
            if (_policy.무료배차)
            {
                return 0;
            }

            return Math.Min(dispatchCount * _policy.기본이용료, _policy.추가이용료);
        }
    }
}
