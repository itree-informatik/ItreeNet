namespace ItreeNet.Services;

public class BodyChangedService
{
    public event Action? OnChange;

    public void NotifyChange()
    {
        OnChange?.Invoke();
    }
}
