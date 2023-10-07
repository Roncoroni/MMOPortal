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
    optionsBuilder.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
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
api.MapGroup("account").MapIdentityApi<IdentityUser>();

api.UseChat("chat", app);

api.MapGet("test", () => "Test");
api.MapGet("test2", () => "Test2").RequireAuthorization();

app.MapRazorComponents<App>()
    .AddServerRenderMode()
    .AddWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MMOPortal.Client._Imports).Assembly);

app.Run();
