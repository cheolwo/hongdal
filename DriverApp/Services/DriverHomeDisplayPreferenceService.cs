using DriverApp.Models.Driver.Home;

namespace DriverApp.Services;

public sealed class DriverHomeDisplayPreferenceService
{
    private 홈추천표시방식 _current = 홈추천표시방식.둘다표시;

    public 홈추천표시방식 Current => _current;
    public event Action? Changed;

    public void Set(홈추천표시방식 value)
    {
        if (_current == value)
        {
            return;
        }

        _current = value;
        Changed?.Invoke();
    }
}
