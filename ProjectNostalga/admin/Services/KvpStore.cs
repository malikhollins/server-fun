
using StackExchange.Redis;

public class KvpStore : IKvpStore
{
    private readonly ConnectionMultiplexer _redis;

    public KvpStore()
    {
        _redis = ConnectionMultiplexer.Connect("redis:6379");
    }

    public void SetValue(string key, string value)
    {
        var db = _redis.GetDatabase();
        db.StringSet(key, value);
    }

    public string? GetValue(string key)
    {
        var db = _redis.GetDatabase();
        return db.StringGet(key);
    }
}