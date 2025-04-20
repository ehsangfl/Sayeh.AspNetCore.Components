namespace Sayeh.AspNetCore.Essentials.WebAssembly;

public interface ILocalStorage
{

    ValueTask Clear();

    ValueTask<Dictionary<string, string?>> ReadAll();

    ValueTask<TValue?> ReadItem<TValue>(string Key);

    ValueTask WriteItem<TValue>(string Key, TValue Data);

    ValueTask WriteAll<TValue>(Dictionary<string, TValue> Data);

    ValueTask Remove(string Key);

}
