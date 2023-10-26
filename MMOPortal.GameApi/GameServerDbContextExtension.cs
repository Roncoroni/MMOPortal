using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MMOPortal.GameApi.Data;

namespace MMOPortal.GameApi;

public static class GameServerDbContextExtension
{
    public static void AddGameServerEntities<TUser, TKey>(this ModelBuilder builder)  where TUser : IdentityUser<TKey> where TKey : IEquatable<TKey>
    {
        builder.Entity<GameServer<TKey>>();
        builder.Entity<Character<TUser, TKey>>();
    }
}