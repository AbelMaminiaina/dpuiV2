using LignageApp.Components;
using LignageApp.Data;
using LignageApp.Services;
using LignageApp.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Cache en mémoire
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();

// Repository avec cache
builder.Services.AddScoped<LignageRepository>();
builder.Services.AddScoped<ILignageRepository, CachedLignageRepository>();

// Services
builder.Services.AddScoped<NavigationSession>();
builder.Services.AddScoped<INoeudsService, NoeudsService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();

// ViewModels
builder.Services.AddScoped<NoeudsListViewModel>();
builder.Services.AddScoped<ConsultationViewModel>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
