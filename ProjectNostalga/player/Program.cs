using Microsoft.AspNetCore.DataProtection;
using Microsoft.FluentUI.AspNetCore.Components;
using player.Components;
using player.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RadioOptions>(builder.Configuration.GetSection("Radio"));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents(options =>
{
    options.ValidateClassNames = false;
});

var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();

builder.Services.AddDataProtection()
    .SetApplicationName("admin")
    .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisOptions.ConnectionString), "DataProtection-Keys");

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRouting();

app.UseAntiforgery();
        
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
