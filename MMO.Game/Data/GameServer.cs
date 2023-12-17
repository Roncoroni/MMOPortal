using System.ComponentModel.DataAnnotations.Schema;

namespace MMO.Game.Data;

public class GameServer
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Guid GameServerId { get; set; }
    public virtual GameServerType GameServerType { get; set; }
    public virtual InstanceHost InstanceHost { get; set; }
}