using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MMOPortal.GameApi.Data;

[Table("Character")]
public class Character<TUser, TKey> where TUser : IdentityUser<TKey> where TKey : IEquatable<TKey>
{
    public virtual TKey CharacterId { get; set; }
    
    [Required]
    public virtual TUser Account { get; set; }
}