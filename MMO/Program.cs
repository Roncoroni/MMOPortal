using System.Security.Claims;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MMO.Authentication;
using MMO.Client;
using MMO.Components;
using MMO.Components.Account;
using MMO.Data;
using MMO.Game;
using MMO.Game.Services;
using MMO.Hubs;
using MudBlazor.Services;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => { options.DetailedErrors = true; })
    /*.AddInteractiveWebAssemblyComponents()*/;

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.TryAddEnumerable(ServiceDescriptor
    .Singleton<IConfigureOptions<GameServerTokenOptions>, GameServerTokenConfigurationOptions>());

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddScheme<GameServerTokenOptions, GameServerTokenHandler>(GameServerTokenDefaults.AuthenticationScheme, _ => { });

Action<IServiceProvider, DbContextOptionsBuilder> method = (serviceProvider, options) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionStringBuilder = new MySqlConnectionStringBuilder(config.GetConnectionString("localdb") ??
                                                                   throw new InvalidOperationException(
                                                                       "Connection string 'localdb' not found."));
    var server = connectionStringBuilder.Server;
    if (server.Contains(':'))
    {
        var parts = server.Split(":");
        if (parts.Length == 2)
        {
            connectionStringBuilder.Server = parts[0];
            connectionStringBuilder.Port = uint.Parse(parts[1]);
        }
    }

    options.UseMySql(connectionStringBuilder.ConnectionString,
        ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString));
};
//builder.Services.AddDbContext<ApplicationDbContext>(method);

builder.Services.AddDbContextFactory<ApplicationDbContext>(method);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
/*builder.Services
    .AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)*/
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InstanceLauncher", policyBuilder =>
    {
        policyBuilder.AuthenticationSchemes = new List<string> { GameServerTokenDefaults.AuthenticationScheme };
        policyBuilder.RequireClaim(GameServerTokenDefaults.ServerIdClaim);
    });
    options.AddPolicy("Admin", policyBuilder =>
        policyBuilder.RequireRole("Admin")
    );
});

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddMudServices();

builder.Services.AddGame<ApplicationDbContext>();
builder.Services.AddSingleton<IUserIdProvider, ServerIdProvider>();
builder.Services.AddScoped<InstanceManagerConnection>();
builder.Services.AddTransient<Lazy<IInstanceConnection>>(provider =>
    new Lazy<IInstanceConnection>(() => provider.GetRequiredService<InstanceManagerConnection>()));

/*
builder.Services.AddAutoMapper((serviceProvider, cfg) =>
{
    //cfg.AddDataReaderMapping();
    cfg.AddCollectionMappers();
    cfg.UseEntityFrameworkCoreModel<ApplicationDbContext>(serviceProvider);
}, typeof(ApplicationDbContext).Assembly);*/

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = IdentityConstants.BearerScheme
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = IdentityConstants.BearerScheme,
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    options.AddSignalRSwaggerGen();
});

var app = builder.Build();

// Configure the HTTP request pipeline. 
if (!app.Environment.IsProduction())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
    /*using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        if (roleManager?.Roles.Any() == false)
        {
            await roleManager.CreateAsync(new ApplicationRole("Admin"));
        }

        if (userManager?.Users.Count() == 1)
        {
            var firstUser = userManager.Users.First();
            await userManager.AddToRoleAsync(firstUser, "Admin");
        }
    }*/
}
else
{
    app.UseResponseCompression();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    //.AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MMO.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

var group = app.MapGroup("/api/account");
group.MapIdentityApi<ApplicationUser>();
group.MapGet("/game/info",
    async Task<Results<Ok<UserInfo>, ValidationProblem, NotFound>> (ClaimsPrincipal claimsPrincipal,
        UserManager<ApplicationUser> userManager) =>
    {
        if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new UserInfo
        {
            UserId = user.Id.ToString(),
            Email = user.Email ?? "",
            Roles = await userManager.GetRolesAsync(user),
        });
    }).RequireAuthorization();

app.MapControllers();

app.MapHub<InstanceManagerHub>("hubs/instance");
app.MapGet("/ValidateToken/{token}", (
    string token,
    [FromServices] IDataProtectionProvider dp) =>
{
    var serverTokenProtector = new TicketDataFormat(dp.CreateProtector(GameServerTokenConfigurationOptions._primaryPurpose, GameServerTokenDefaults.AuthenticationScheme, "Token"));
    var ticket = serverTokenProtector.Unprotect(token);
    return ticket?.Principal.Claims.Humanize();
});

app.Run();