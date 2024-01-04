using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MMO.Game.Data;

namespace MMO.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IGameDbContext
{
    public DbSet<Character> Characters { get; set; }
    public DbSet<InstanceHost> InstanceHosts { get; set; }
    public DbSet<GameServerDefinition> GameServerDefinitions { get; set; }
    public DbSet<GameServer> GameServers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Character>()
            .HasOne(character => (ApplicationUser)character.Account)
            .WithMany(user => user.Characters)
            .HasForeignKey(character => character.AccountId); //.Navigation(character => character.Account).
        //builder.Entity<Character>().Property<IGameUser>(nameof(Character.Account)).*/
        this.OnGameModelCreating(builder);
    }
}