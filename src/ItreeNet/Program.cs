using ItreeNet.Data.Extensions;
using MudBlazor.Services;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using ItreeNet.Middleware;
using ItreeNet.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews();

builder.Services.AddMudServices();
builder.Services.AddAuthorization(config =>
{
    config.AddPolicy("internPolicy", policy => policy.RequireClaim("IsIntern", true.ToString()));
});

var bosses = builder.Configuration.GetSection("Bosses").Get<List<string>>();
if (bosses == null)
{
    throw new InvalidDataException("No Bosses in configuration found!");
}

Globals.BossList = bosses;
Globals.FileStorePath = builder.Configuration.GetValue<string>("File:Store");
var autoMapperLicenseKey = builder.Configuration.GetValue<string>("LicenseKeys:AutoMapper");

var connectionString = builder.Configuration.GetConnectionString("APP");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidDataException("ConnectionString:APP is empty");
}

if (string.IsNullOrEmpty(autoMapperLicenseKey))
    throw new InvalidOperationException("Missing license key for AutoMapper");

// database
builder.Services.AddDbContextFactory<ZeiterfassungContext>(opt =>

    opt.UseNpgsql(connectionString)
);

// services
SerilogConfig.AddSerilogServices(builder.Services, builder.Configuration);

builder.Services.AddAutoMapper(cfg => {
        cfg.LicenseKey = autoMapperLicenseKey;
        cfg.AddMaps(typeof(Program));
    });

builder.Services.AddScoped<IAnwesenheitService, AnwesenheitService>();
builder.Services.AddScoped<IArbeitszeitService, ArbeitszeitService>();
builder.Services.AddScoped<IArbeitszeitReduktionService, ArbeitszeitReduktionService>();
builder.Services.AddScoped<BodyChangedService>();
builder.Services.AddScoped<IVorgangService, VorgangService>();
builder.Services.AddScoped<IKundenService, KundenService>();
builder.Services.AddScoped<IProjektService, ProjektService>();
builder.Services.AddScoped<IMitarbeiterService, MitarbeiterService>();
builder.Services.AddScoped<IBuchungsService, BuchungsService>();
builder.Services.AddScoped<IPensumService, PensumService>();
builder.Services.AddScoped<ISpesenService, SpesenService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IFerienArbeitspensumService, FerienArbeitspensumService>();
builder.Services.AddScoped<IMitarbeiterSaldoService, MitarbeiterSaldoService>();
builder.Services.AddScoped<IMitarbeiterSaldoKorrekturService, MitarbeiterSaldoKorrekturService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IDokumentService, DokumentService>();
builder.Services.AddSingleton<MetadataProvider>();
builder.Services.AddScoped<MetadataTransferService>();
builder.Services.AddScoped<IProfilService, ProfilService>();
builder.Services.AddScoped<IClaimsTransformation, UserInfoClaims>();

// Singelton
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

// Hosted Service
builder.Services.AddHostedService<BackgroundQueueHostedService>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(option => { option.DetailedErrors = builder.Environment.IsDevelopment(); });

builder.Services.AddHsts(opt =>
{
    opt.MaxAge = TimeSpan.FromDays(30);
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRewriter(
    new RewriteOptions().Add(
        context =>
        {
            if (context.HttpContext.Request.Path == "/MicrosoftIdentity/Account/SignedOut")
            {
                context.HttpContext.Response.Redirect("/intern");
            }
        }));

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

Culture.SetCulture();

app.Run();