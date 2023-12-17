using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MMO.Game.Data;

public class GameServerType
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Guid GameServerTypeId { get; set; }
    
    [Required, MaxLength(20)]
    public virtual string Name { get; set; }
    
    [Required, MaxLength(40)]
    public virtual string MapName { get; set; }

    [DefaultValue(GameServerStartType.OnDemand)]
    public virtual GameServerStartType StartType { get; set; }
    public virtual ICollection<GameServer> Instances { get; set; }
}