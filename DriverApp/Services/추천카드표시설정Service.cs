using DriverApp.Models.Driver;

namespace DriverApp.Services;

public sealed class 추천카드표시설정Service
{
    private 추천카드표시설정 _settings = new();

    public event Action? Changed;

    public 추천카드표시설정 GetSettings() => _settings;

    public void SetMode(추천카드표시모드 mode)
    {
        if (_settings.표시모드 == mode) return;
        _settings.표시모드 = mode;
        Changed?.Invoke();
    }

    public void UpdateOverride(Action<추천카드표시설정> updater)
    {
        updater(_settings);
        Changed?.Invoke();
    }

    public 추천카드표시정책 CalculatePolicy()
    {
        // base defaults per mode
        var mode = _settings.표시모드;
        bool 운송방식 = true;
        bool 시간조건 = true;
        bool 거리 = mode != 추천카드표시모드.간단히;
        bool 차량조건 = mode != 추천카드표시모드.간단히;
        bool 인수증 = mode != 추천카드표시모드.간단히;
        bool 복귀거리 = mode == 추천카드표시모드.자세히;
        bool 공차거리 = mode == 추천카드표시모드.자세히;
        bool 추천사유 = mode != 추천카드표시모드.간단히;

        // apply overrides if present
        if (_settings.운송방식표시Override.HasValue) 운송방식 = _settings.운송방식표시Override.Value;
        if (_settings.시간조건표시Override.HasValue) 시간조건 = _settings.시간조건표시Override.Value;
        if (_settings.거리표시Override.HasValue) 거리 = _settings.거리표시Override.Value;
        if (_settings.차량조건표시Override.HasValue) 차량조건 = _settings.차량조건표시Override.Value;
        if (_settings.인수증표시Override.HasValue) 인수증 = _settings.인수증표시Override.Value;
        if (_settings.복귀거리표시Override.HasValue) 복귀거리 = _settings.복귀거리표시Override.Value;
        if (_settings.공차거리표시Override.HasValue) 공차거리 = _settings.공차거리표시Override.Value;
        if (_settings.추천사유표시Override.HasValue) 추천사유 = _settings.추천사유표시Override.Value;

        return new 추천카드표시정책
        {
            운송방식표시 = 운송방식,
            시간조건표시 = 시간조건,
            거리표시 = 거리,
            차량조건표시 = 차량조건,
            인수증표시 = 인수증,
            복귀거리표시 = 복귀거리,
            공차거리표시 = 공차거리,
            추천사유표시 = 추천사유
        };
    }
}
