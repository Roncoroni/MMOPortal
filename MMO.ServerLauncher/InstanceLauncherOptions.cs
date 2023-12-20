namespace MMO.ServerLauncher;

public class InstanceLauncherOptions
{
    public const string InstanceLauncher = "InstanceLauncher";
    
    public string HubUrl { get; set; } = String.Empty;
    public string ApiToken { get; set; } = String.Empty;
    public string ApiSecret { get; set; } = String.Empty;
    public bool IsEditor { get; set; } = false;
    public string PathToExecutable { get; set; } = "";
    public string PathToProject { get; set; } = "";
    public ushort MinPort { get; set; } = 7777;
    public ushort MaxPort { get; set; } = 7877;
}