using System.Security.Claims;
using AutoMapper;
using AutoMapper.Data;
using AutoMapper.EntityFrameworkCore;
using AutoMapper.EquivalencyExpression;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MMOPortal.Areas.Identity;
using MMOPortal.Chat;
using MMOPortal.Components;
using MMOPortal.Components.Pages.Admin;
using MMOPortal.Data;
using MMOPortal.DTO;
using MMOPortal.GameApi;
using MMOPortal.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();


builder.Services.AddDbContext<ApplicationDbContext>(optionsBuilder =>
{
    /*if (builder.Environment.IsDevelopment())
    {*/
    optionsBuilder.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    /*}
    else
    {
        var connectionString = builder.Configuration.GetSection("MYSQLCONNSTR")["localdb"];
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }*/
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();

builder.Services.AddChat();
builder.Services.AddGameApi<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services
    .AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.UseEntityFrameworkCoreModel<ApplicationDbContext>();
    cfg.AddDataReaderMapping();
    cfg.AddCollectionMappers();
    cfg.SetGeneratePropertyMaps<GenerateEntityFrameworkCorePrimaryKeyPropertyMaps>();
});

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
    app.UseSwagger();
    app.UseSwaggerUI();
    using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
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
    }
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorPages();

var api = app.MapGroup("api");
var accountApi = api.MapGroup("account");
accountApi.MapIdentityApi<ApplicationUser>();

accountApi.MapGet("token", async (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
{
    var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
    var userManager = signInManager.UserManager;
    if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
    {
        return Results.NotFound();
    }

    return Results.SignIn(claimsPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
}).RequireAuthorization();
accountApi.MapGet("game/info", async Task<Results<Ok<UserInfo>, ValidationProblem, NotFound>>(ClaimsPrincipal claimsPrincipal, UserManager<ApplicationUser> userManager) =>
{
    if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
    {
        return TypedResults.NotFound();
    }

    return TypedResults.Ok(new UserInfo(user.Id));
}).RequireAuthorization();

api.UseChat("chat");
api.UseGameApi<DbContext, UserManager<ApplicationUser>, ApplicationUser>("server");

api.MapGet("test", () => "Test");
api.MapGet("test2", () => "Test2").RequireAuthorization();
api.MapGet("test3", () => "Test3").RequireAuthorization(policyBuilder => policyBuilder.RequireRole("Admin"));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MMOPortal.Client._Imports).Assembly);

app.Run();