using admin.Components;
using admin.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.FluentUI.AspNetCore.Components;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents(options =>
{
    options.ValidateClassNames = false;
});

builder.Services.AddSignalR();
builder.Services.AddControllers();

var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();

builder.Services.AddDataProtection()
    .SetApplicationName("admin")
    .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisOptions.ConnectionString), "DataProtection-Keys");
    
builder.Services.AddSingleton<IKvpStore, KvpStore>();
builder.Services.AddSingleton<ITrackStore, TrackStore>();
builder.Services.AddSingleton<IMusicStore, MusicStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<TrackHub>("/trackhub");

app.MapControllers();

app.Run();
