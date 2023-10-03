using MMOPortal.Chat;
using MMOPortal.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddServerComponents()
    .AddWebAssemblyComponents();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddChat();

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

app.UseStaticFiles();

var api = app.MapGroup("api/");
api.UseChat("chat", app);

app.MapRazorComponents<App>()
    .AddServerRenderMode()
    .AddWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MMOPortal.Client._Imports).Assembly);

app.Run();