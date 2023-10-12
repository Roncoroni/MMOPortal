using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MMOPortal.Areas.Identity;
using MMOPortal.Chat;
using MMOPortal.Components;
using MMOPortal.Data;

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
    .AddServerComponents()
    .AddWebAssemblyComponents();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();

builder.Services.AddChat();

builder.Services.AddRazorPages();
builder.Services
    .AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

builder.Services.AddSwaggerGen(options => options.AddSignalRSwaggerGen());

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
            roleManager.CreateAsync(new ApplicationRole("Admin"));
        }

        if (userManager?.Users.Count() == 1)
        {
            var firstUser = userManager.Users.First();
            userManager.AddToRoleAsync(firstUser, "Admin");
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
app.UseAuthorization();

app.UseStaticFiles();
app.MapRazorPages();

var api = app.MapGroup("api/");
api.MapGroup("account").MapIdentityApi<ApplicationUser>();

api.UseChat("chat", app);

api.MapGet("test", () => "Test");
api.MapGet("test2", () => "Test2").RequireAuthorization();
api.MapGet("test3", () => "Test3").RequireAuthorization(policyBuilder => policyBuilder.RequireRole("Admin"));

app.MapRazorComponents<App>()
    .AddServerRenderMode()
    .AddWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MMOPortal.Client._Imports).Assembly);

app.Run();