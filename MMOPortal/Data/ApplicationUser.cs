using Microsoft.AspNetCore.Identity;
using MMOPortal.GameApi;
using MMOPortal.GameApi.Data;

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