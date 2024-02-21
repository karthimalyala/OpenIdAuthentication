using Azure.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

string appConfigConnectionString = builder.Configuration.GetConnectionString("AppConfig")!;
Console.WriteLine("App config connection String" + appConfigConnectionString);
string appConfigPrefix = builder.Configuration["AppConfigPrefix"]!;
Console.WriteLine("App config Prefix" + appConfigPrefix);

builder.Services.AddAzureAppConfiguration();

bool isAppConfigConnected = false;

try
{

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(appConfigConnectionString)
    .ConfigureKeyVault(kv =>
    {
        kv.SetCredential(new DefaultAzureCredential());
    })
    .Select(KeyFilter.Any, appConfigPrefix)
    .ConfigureRefresh(refreshOptions => refreshOptions.Register(appConfigPrefix + ":Sentinel", appConfigPrefix, refreshAll: true));

    isAppConfigConnected = true;
    Console.WriteLine("connected", isAppConfigConnected);
});

}
catch (Exception ex)
{
    Console.WriteLine("Error while connecting to app config");
}


/*builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ILOBService, LOBService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEntityService, EntityService>();*/

if (isAppConfigConnected)
{

    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection(appConfigPrefix + ":AzureAd"));
    Console.WriteLine("connected in if");
}
else
{
    Console.WriteLine("connection failed");
}

//Console.WriteLine("App config Prefix" + clientId);

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddRazorPages();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{

    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
