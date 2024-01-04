using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MMO.Game.Data;

public enum GameServerType
{
    World,
    Entry
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(GameServerType))]
[JsonDerivedType(typeof(WorldServerDefinition), (byte)GameServerType.World)]
[JsonDerivedType(typeof(EntryServerDefinition), (byte)GameServerType.Entry)]
public abstract class GameServerDefinition
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid GameServerDefinitionId { get; set; }
    
    [JsonIgnore]
    public GameServerType GameServerType { get; set; }
    
    [Required, MaxLength(40)]
    public required string MapName { get; set; }
    
    public virtual ICollection<GameServer> Instances { get; set; } = new List<GameServer>();
}

public class WorldServerDefinition : GameServerDefinition
{
    public WorldServerDefinition()
    {
        GameServerType = GameServerType.World;
    }
}

public class EntryServerDefinition : GameServerDefinition
{
    public EntryServerDefinition()
    {
        GameServerType = GameServerType.Entry;
    }
}
