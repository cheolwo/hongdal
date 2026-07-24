namespace Ssalddel.WebApp.ViewModels;

public enum DriverTransportProofMessageTone
{
    Info,
    Success,
    Warning,
    Error
}

public delegate Task DriverTransportProofCommandRunner(
    string successMessage,
    Func<CancellationToken, Task> action);
