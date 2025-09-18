using System.Text.Json;
using Microsoft.JSInterop;

public class SessionStorageService
{
    private readonly IJSRuntime _js;

    public SessionStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _js.InvokeVoidAsync("sessionStorage.setItem", key, json);
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await _js.InvokeAsync<string>("sessionStorage.getItem", key);
        if (string.IsNullOrEmpty(json))
            return default;

        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _js.InvokeVoidAsync("sessionStorage.removeItem", key);
    }

    public async Task ClearAsync()
    {
        await _js.InvokeVoidAsync("sessionStorage.clear");
    }
}
