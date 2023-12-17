namespace MMO.ServerLauncher;

public class InstanceLauncherOptions
{
    public const string InstanceLauncher = "InstanceLauncher";
    
    public string HubUrl { get; set; } = String.Empty;
    public bool IsEditor { get; set; } = false;
    public string PathToExecutable { get; set; } = "";
    public string PathToProject { get; set; } = "";
    public int MinPort { get; set; } = 7777;
    public int MaxPort { get; set; } = 7877;
}