
using StackExchange.Redis;

public class KvpStore : IKvpStore
{
    private readonly ConnectionMultiplexer _redis;

    public KvpStore(IConfiguration configuration)
    {
        var connectionString = configuration["Redis:ConnectionString"] ?? "redis:6379";
        _redis = ConnectionMultiplexer.Connect(connectionString);
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