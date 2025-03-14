using EduSoft.Data;
using EduSoft.Services;
using ElectronNET.API;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DashboardEstudianteService>();
builder.Services.AddScoped<DashboardMaestroService>();
builder.Services.AddScoped<ClaseService>();
builder.Services.AddScoped<HorarioService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

builder.WebHost.UseElectron(args);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

if (HybridSupport.IsElectronActive)
{
    await ElectronBootstrapAsync();
}

app.Run();

async Task ElectronBootstrapAsync()
{
    var window = await Electron.WindowManager.CreateWindowAsync(new ElectronNET.API.Entities.BrowserWindowOptions
    {
        Width = 1200,
        Height = 800,
        Show = true,
        Frame = true,
        Title = "EduSoft - Plataforma Educativa",
        AutoHideMenuBar = true
    });

    window.SetMenuBarVisibility(false);
    window.OnClosed += () => Electron.App.Exit();
}
