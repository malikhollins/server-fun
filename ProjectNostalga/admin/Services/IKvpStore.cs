public interface IKvpStore
{
    string? GetValue(string key);
    void SetValue(string key, string value);
}