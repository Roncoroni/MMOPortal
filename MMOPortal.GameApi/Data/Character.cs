using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace MMOPortal.GameApi.Data;

[Table("Character")]
public class Character<TUser, TKey> where TUser : IdentityUser<TKey> where TKey : IEquatable<TKey>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual TKey CharacterId { get; set; }
    
    [Required, JsonIgnore]
    public virtual TUser Account { get; set; }
    
    [Required]
    public virtual string Name { get; set; }
    
    public virtual double PositionX { get; set; }
    public virtual double PositionY { get; set; }
    public virtual double PositionZ { get; set; }
    public virtual double Rotation { get; set; }
}