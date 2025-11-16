using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using MisFinanzas.Components;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;
using MisFinanzas.Infrastructure.Services;
using MisFinanzas.Services;
using System.Globalization;

// Configurar PostgreSQL para usar timestamps sin zona horaria (compatibilidad con SQLite)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);


// CONFIGURACIÓN DE MONEDA DOMINICANA (PESO DOMINICANO - DOP)
var dominicanCulture = new CultureInfo("es-DO");
dominicanCulture.NumberFormat.CurrencySymbol = "RD$";
dominicanCulture.NumberFormat.CurrencyDecimalDigits = 2;
dominicanCulture.NumberFormat.CurrencyDecimalSeparator = ".";
dominicanCulture.NumberFormat.CurrencyGroupSeparator = ",";

CultureInfo.DefaultThreadCurrentCulture = dominicanCulture;
CultureInfo.DefaultThreadCurrentUICulture = dominicanCulture;

// Leer configuración de encriptación
var useEncryption = builder.Configuration.GetValue<bool>("Security:UsePasswordEncryption");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();



// Agregar Autenticación con Identity y Google
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

authBuilder.AddIdentityCookies();

authBuilder.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
        ?? throw new InvalidOperationException("Google ClientId not configured");
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
        ?? throw new InvalidOperationException("Google ClientSecret not configured");
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;

    // Solicitar permisos de perfil y email
    options.Scope.Add("profile");
    options.Scope.Add("email");

    Console.WriteLine("Google Authentication configured");
});

authBuilder.AddMicrosoftAccount(options =>
{
    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]
        ?? throw new InvalidOperationException("Microsoft ClientId not configured");
    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]
        ?? throw new InvalidOperationException("Microsoft ClientSecret not configured");
    options.CallbackPath = "/signin-microsoft";
    options.SaveTokens = true;

    // Solicitar permisos de perfil y email
    options.Scope.Add("User.Read");

    Console.WriteLine("✅ Microsoft Authentication configured");
});

// CONFIGURAR PostgreSQL
// En producción (Render), usar variable de entorno DATABASE_URL
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Render usa formato DATABASE_URL de Heroku, convertir a formato Npgsql si es necesario
if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(connectionString);
    var dbPort = uri.Port > 0 ? uri.Port : 5432; // Usar puerto por defecto si no está especificado
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={dbPort};Database={uri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    Console.WriteLine($"✅ Connection string convertido correctamente (Host: {uri.Host}, Port: {dbPort})");
}
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrar también DbContext para servicios que lo necesiten directamente
builder.Services.AddScoped(p =>
    p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Registrar el custom password hasher
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>>(
    sp => new MisFinanzas.Infrastructure.Security.PlainTextPasswordHasher(useEncryption));

//  CONFIGURAR IDENTITY CON ApplicationUser
builder.Services.AddIdentityCore<MisFinanzas.Domain.Entities.ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;  //Debo ponerlo en true cuando implemente el envio de email
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

})
    .AddRoles<IdentityRole>()  // Soporte para roles
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// REGISTRAR NUESTROS SERVICIOS (Dependency Injection)
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IExpenseIncomeService, ExpenseIncomeService>();
builder.Services.AddScoped<IFinancialGoalService, FinancialGoalService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILoanService, LoanService>();
// AGREGAR AuthService como Scoped
builder.Services.AddScoped<AuthService>();
// Registrar UserService
builder.Services.AddScoped<UserService>();
// Registrar servicios para el Dashboard y otros componentes
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ExpenseIncomeService>();
builder.Services.AddScoped<FinancialGoalService>();
builder.Services.AddScoped<BudgetService>();

// Registrar servicio de fondo para notificaciones automáticas
// TEMPORALMENTE DESHABILITADO para configuración de PostgreSQL/Render
// builder.Services.AddHostedService<NotificationBackgroundService>();

// Registrar servicios de reportes
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<PdfReportGenerator>();
builder.Services.AddScoped<ExcelReportGenerator>();

// Registrar caché temporal de archivos como Singleton

builder.Services.AddSingleton<MisFinanzas.Infrastructure.Services.TemporaryFileCache>();

// Agregar soporte para controladores API

builder.Services.AddControllers();

// Configurar SignalR para archivos grandes
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
});


// Configurar puerto para Render (usa variable de entorno PORT)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});


var app = builder.Build();

// APLICAR MIGRACIONES AUTOMÁTICAMENTE EN PRODUCCIÓN
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Aplicar migraciones pendientes
        if (context.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("🔄 Aplicando migraciones pendientes...");
            context.Database.Migrate();
            Console.WriteLine("✅ Migraciones aplicadas exitosamente");
        }
        else
        {
            Console.WriteLine("✅ Base de datos ya está actualizada");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al aplicar migraciones: {ex.Message}");
        // En producción, podrías querer que falle si no puede migrar
        // throw;
    }
}

// Configurar headers para detectar HTTPS cuando está detrás de un proxy (Render)
if (!app.Environment.IsDevelopment())
{
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next();
    });
}

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Mapear controladores API

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
