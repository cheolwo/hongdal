namespace Ssalddel.Ui.Common.Areas.App.Models;

public sealed record SsalddelBankAccountSummary(
    string BankName,
    string AccountHolderName,
    string AccountNo,
    string AccountStatus,
    int MonthlyDispatchCount,
    decimal MonthlyFee);
