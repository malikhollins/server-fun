using admin.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

public class KvpStore : IKvpStore
{
    private readonly ConnectionMultiplexer _redis;

    public KvpStore(IOptions<RedisOptions> options)
    {
        _redis = ConnectionMultiplexer.Connect(options.Value.ConnectionString);
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
