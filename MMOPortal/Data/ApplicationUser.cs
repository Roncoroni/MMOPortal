using Microsoft.AspNetCore.Identity;

namespace MMOPortal.Data;

public class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser() : base()
    {
        
    }

    public ApplicationUser(string userName) : base(userName)
    {
        
    }
}