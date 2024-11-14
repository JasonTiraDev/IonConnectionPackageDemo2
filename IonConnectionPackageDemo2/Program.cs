using InforIonClientLibrary; 
using IonConnectionPackageDemo2.Components;
using IonConnectionPackageDemo2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure InforIonClientLibrary services
builder.Services.AddHttpClient();
builder.Services.AddSingleton(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var configPath = "ionapi/IonConnectionPackage.ionapi"; // Path to the configuration file
    return ConnectionOptions.FromIonApiFile(configPath);
});
builder.Services.AddScoped<IdoCollectionService>();

builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddSingleton<ApiHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
