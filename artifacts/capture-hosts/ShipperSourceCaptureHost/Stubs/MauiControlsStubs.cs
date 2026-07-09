namespace Microsoft.Maui.Controls;

public sealed class Application
{
    public static Application? Current { get; set; }

    public IReadOnlyList<Window> Windows { get; } = [];
}

public sealed class Window
{
    public Page? Page { get; init; }
}

public sealed class Page
{
    public Task DisplayAlertAsync(string title, string message, string cancel)
        => Task.CompletedTask;
}
