using MMO.ServerLauncher;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();

builder.Services.Configure<InstanceLauncherOptions>(
    builder.Configuration.GetSection(InstanceLauncherOptions.InstanceLauncher));

builder.Services.AddHostedService<InstanceLauncher>();

var host = builder.Build();
host.Run();
