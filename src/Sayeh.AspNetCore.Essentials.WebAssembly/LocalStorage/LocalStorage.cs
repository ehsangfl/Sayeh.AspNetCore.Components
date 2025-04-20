namespace Sayeh.AspNetCore.Essentials.WebAssembly;

public class LocalStorage(Blazored.LocalStorage.ILocalStorageService ls) : ILocalStorage
{
    
    public ValueTask Remove(string Key)
            => ls.RemoveItemAsync(Key);

    public ValueTask Clear()
            => ls.ClearAsync();

    public async ValueTask<Dictionary<string, string?>> ReadAll()
    {
        var result = new Dictionary<string, string?>();
        var keys =await ls.KeysAsync();
        foreach (var key in keys)
        {
            result .Add(key,await ls.GetItemAsStringAsync(key));
        }
        return result;
    }

    public async ValueTask<TValue?> ReadItem<TValue>(string Key)
    {
        if (await ls.ContainKeyAsync(Key))
            return await ls.GetItemAsync<TValue>(Key);
        else return default;
    }

    public async ValueTask WriteAll<TValue>(Dictionary<string, TValue?> Data)
    {
        foreach (var item in Data)
        {
           await ls.SetItemAsync(item.Key, item.Value!);
        }
    }

    public async ValueTask WriteItem<TValue>(string Key, TValue Value)
    => await ls.SetItemAsync(Key, Value);
}