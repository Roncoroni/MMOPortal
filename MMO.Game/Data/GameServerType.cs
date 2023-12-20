using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MMO.Game.Data;

[Index(nameof(Name), IsUnique = true)]
public class GameServerType
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid GameServerTypeId { get; set; }
    
    [Required, MaxLength(20)]
    public required string Name { get; set; }
    
    [Required, MaxLength(40)]
    public required string MapName { get; set; }

    [DefaultValue(GameServerStartType.OnDemand)]
    public GameServerStartType StartType { get; set; }

    public virtual ICollection<GameServer> Instances { get; set; } = new List<GameServer>();
}