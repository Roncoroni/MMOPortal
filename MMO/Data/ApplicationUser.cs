using Microsoft.AspNetCore.Identity;
using MMO.Game.Data;

namespace MMO.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser<Guid>, IGameUser
{
    public virtual ICollection<Character> Characters { get; set; } = new List<Character>();
}