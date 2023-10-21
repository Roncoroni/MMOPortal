using System.ComponentModel.DataAnnotations.Schema;

namespace MMOPortal.GameApi.Data;

public class GameServer
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual string Id { get; set; }
    public virtual string ServerGuid { get; set; }
}