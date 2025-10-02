public class LoaderService
{
    public event Action? OnChange;

    public bool IsVisible { get; private set; }
    public string Message { get; private set; } = string.Empty;

    public void Show(string message)
    {
        Message = message;
        IsVisible = true;
        NotifyStateChanged();
    }

    public void Hide()
    {
        IsVisible = false;
        Message = string.Empty;
        NotifyStateChanged();
    }

    public async Task RunWithLoader(string message, Func<Task> action)
    {
        try
        {
            Show(message);
            await action();
        }
        finally
        {
            Hide();
        }
    }

    // 🔹 Generična verzija za Task<T>
    public async Task<T> RunWithLoader<T>(string message, Func<Task<T>> action)
    {
        try
        {
            Show(message);
            return await action();
        }
        finally
        {
            Hide();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
