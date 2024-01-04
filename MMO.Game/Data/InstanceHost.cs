using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MMO.Game.Data;

public class InstanceHost
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid InstanceHostId { get; set; }

    [Length(25, 25)]
    public required string SharedSecret { get; set; }

    public bool Online { get; set; } = false;
    public DateTime LastHeartbeat { get; set; } = DateTime.Now;
    [MaxLength(45)]
    public string? Address { get; set; } = null;
    
    public IList<GameServer> GameServers { get; set; }
}