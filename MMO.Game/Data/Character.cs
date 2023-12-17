using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MMO.Game.Data;

public class Character
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Guid CharacterId { get; set; }
    [Required, JsonIgnore]
    public virtual Guid AccountId { get; set; }
    [Required, JsonIgnore]
    public virtual IGameUser Account { get; set; }
    [Required, MaxLength(60)]
    public virtual string Name { get; set; }
    public virtual double PositionX { get; set; }
    public virtual double PositionY{ get; set; }
    public virtual double PositionZ{ get; set; }
    public virtual double Orientation { get; set; }
}
