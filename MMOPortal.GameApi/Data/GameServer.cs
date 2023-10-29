using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MMOPortal.GameApi.Data;

[Table("GameServer")]
public class GameServer<TKey> where TKey : IEquatable<TKey>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual TKey GameServerId { get; set; }
    
    public virtual string Name { get; set; }
    
    [JsonIgnore]
    public virtual string SharedSecret { get; set; }
}