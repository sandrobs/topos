using System.Threading.RateLimiting;
using CrmIctm.Api.Configuracao;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Endpoints;
using CrmIctm.Api.Infraestrutura;
using CrmIctm.Api.Servicos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
FontesConvitePdf.Configurar();

// O HTTPS termina no Caddy da VPS. A API fica acessível somente na rede
// interna do Compose e em 127.0.0.1 no host; os cabeçalhos encaminhados
// preservam o protocolo original para redirecionamento e cookies seguros.
builder.Services.Configure<ForwardedHeadersOptions>(opcoes =>
{
    opcoes.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    opcoes.KnownIPNetworks.Clear();
    opcoes.KnownProxies.Clear();
});

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(opcoes => opcoes.TimestampFormat = "yyyy-MM-dd HH:mm:ss ");
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}

builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<AvisoPrivacidadeOpcoes>(
    builder.Configuration.GetSection(AvisoPrivacidadeOpcoes.Secao));
builder.Services.Configure<DadosIniciaisOpcoes>(
    builder.Configuration.GetSection(DadosIniciaisOpcoes.Secao));
builder.Services.Configure<IgrejaInicialOpcoes>(
    builder.Configuration.GetSection(IgrejaInicialOpcoes.Secao));

var diretorioChaves = builder.Configuration["Seguranca:DiretorioChaves"];
if (!string.IsNullOrWhiteSpace(diretorioChaves))
{
    builder.Services
        .AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(diretorioChaves));
}

builder.Services.AddDbContext<BancoContexto>(opcoes =>
    opcoes.UseNpgsql(builder.Configuration.GetConnectionString("Banco")));

builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services
    .AddIdentityCore<Usuario>(opcoes =>
    {
        opcoes.Password.RequiredLength = 10;
        opcoes.Password.RequireDigit = true;
        opcoes.Password.RequireLowercase = true;
        opcoes.Password.RequireUppercase = true;
        opcoes.Password.RequireNonAlphanumeric = true;
        opcoes.Lockout.MaxFailedAccessAttempts = 5;
        opcoes.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        opcoes.User.RequireUniqueEmail = true;
    })
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<BancoContexto>();

builder.Services.Configure<CookieAuthenticationOptions>(
    IdentityConstants.ApplicationScheme,
    opcoes =>
    {
        opcoes.Cookie.Name = builder.Environment.IsDevelopment()
            ? "crm-ictm"
            : "__Host-crm-ictm";
        opcoes.Cookie.HttpOnly = true;
        opcoes.Cookie.SameSite = SameSiteMode.Strict;
        opcoes.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);
        opcoes.SlidingExpiration = true;
        opcoes.Events.OnRedirectToLogin = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        opcoes.Events.OnRedirectToAccessDenied = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(opcoes =>
{
    opcoes.Cookie.Name = builder.Environment.IsDevelopment()
        ? "crm-ictm-csrf"
        : "__Host-crm-ictm-csrf";
    opcoes.Cookie.HttpOnly = true;
    opcoes.Cookie.SameSite = SameSiteMode.Strict;
    opcoes.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    opcoes.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opcoes.AddPolicy("formulario-publico", contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    opcoes.AddPolicy("autenticacao", contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            }));
    opcoes.AddPolicy("convite-pdf", contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddSingleton<IRelogio, RelogioSistema>();
builder.Services.AddScoped<UsuarioAtual>();
builder.Services.AddSingleton<GeradorConviteVisitantesPdf>();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
var opcoesArquivosEstaticos = new StaticFileOptions
{
    OnPrepareResponse = contexto =>
    {
        if (string.Equals(contexto.File.Name, "index.html", StringComparison.OrdinalIgnoreCase))
        {
            contexto.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            contexto.Context.Response.Headers.Pragma = "no-cache";
            contexto.Context.Response.Headers.Expires = "0";
            return;
        }

        if (contexto.Context.Request.Path.StartsWithSegments("/assets"))
        {
            contexto.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }
    }
};
app.UseStaticFiles(opcoesArquivosEstaticos);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/api/saude", () => Results.Ok(new { status = "ok" }));
app.MapearSeguranca();
app.MapearAutenticacao();
app.MapearPublico();
app.MapearIgrejas();
app.MapearUsuarios();
app.MapearAtendimentos();
app.MapFallbackToFile("index.html", opcoesArquivosEstaticos);

await DadosIniciais.PrepararAsync(app);
await app.RunAsync();

public partial class Program;
