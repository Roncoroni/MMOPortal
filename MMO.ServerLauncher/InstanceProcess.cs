using System.Diagnostics;

namespace MMO.ServerLauncher;

internal record InstanceProcess
{
    public Process Process { get; set; }
    public ushort Port { get; set; }
    public Guid ServerId { get; set; }
}