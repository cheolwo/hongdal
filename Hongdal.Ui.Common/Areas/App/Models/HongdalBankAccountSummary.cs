namespace Hongdal.Ui.Common.Areas.App.Models;

public sealed record HongdalBankAccountSummary(
    string BankName,
    string AccountHolderName,
    string AccountNo,
    string AccountStatus,
    int MonthlyDispatchCount,
    decimal MonthlyFee);
