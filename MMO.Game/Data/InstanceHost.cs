using System.ComponentModel.DataAnnotations.Schema;

namespace MMO.Game.Data;

public class InstanceHost
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Guid InstanceHostId { get; set; }

    public virtual string ConnectionId { get; set; } 
}