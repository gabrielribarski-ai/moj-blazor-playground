
    using IzracunInvalidnostiBlazor;
    using IzracunInvalidnostiBlazor.Components;
using IzracunInvalidnostiBlazor.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container. 
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.MaximumReceiveMessageSize = 1 * 1024 * 1024; // 1MB
});

/*
if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddScoped< PogojService>();
    builder.Services.AddScoped<SegmentService>();
    //builder.Services.AddScoped<IPodatkiService, MockPodatkiService>(); 
}
    else
    {
    builder.Services.AddScoped<PogojService>();
    builder.Services.AddScoped<SegmentService>();
    builder.Services.AddScoped<PogojAtributNaborLoader>();
}*/

builder.Services.AddScoped<OcenjevalniModelLoader>();
builder.Services.AddScoped<OcenjevalniModelSessionService>();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();
    // Configure the HTTP request pipeline. 
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts. 
        app.UseHsts();
    }
    app.UseHttpsRedirection();
    app.UseAntiforgery(); 
app.MapStaticAssets();
    app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
