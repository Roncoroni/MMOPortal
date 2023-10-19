using Microsoft.AspNetCore.Identity;
using MMOPortal.GameApi;

namespace MMOPortal.Data;

public class ApplicationUser : IdentityUser<Guid>, IGameServerAccount
{
    public ApplicationUser() : base()
    {
        
    }

    public ApplicationUser(string userName) : base(userName)
    {
        
    }
}