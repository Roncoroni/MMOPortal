using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MMOPortal.GameApi;

namespace MMOPortal.Data;


public class ApplicationDbContext : 
    IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext()
        : base()
    {
    }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.AddGameServerEntities<ApplicationUser, Guid>();
        base.OnModelCreating(builder);
    }
}