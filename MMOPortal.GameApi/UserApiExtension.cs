using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MMOPortal.GameApi.Data;
using MMOPortal.GameApi.DTO;

namespace MMOPortal.GameApi;

public static class UserApiExtension
{
    public static void MapUserApi<TDbContext, TUserManager, TUser, TKey>(this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string path) 
        where TDbContext : DbContext
        where TUserManager : UserManager<TUser>
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
    {
        var usersApi = endpoints.MapGroup(path);
        
        var userApi = usersApi.MapGroup("{id}");
        
        userApi.MapGet("valid",
            async Task<Results<Ok, ValidationProblem, NotFound>> (string id, TUserManager userManager) =>
            {
                if (await userManager.FindByIdAsync(id) is not { } user)
                {
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok();
            });
        userApi.MapGet("character", (TKey id, TDbContext dbContext) => 
            dbContext.Set<Character<TUser, TKey>>().SingleAsync(character => character.Account.Id.Equals(id)));
        userApi.MapPost("character", async Task<Results<Ok, NotFound>> (TKey id, 
            [FromBody] CharacterUpdate<TKey> characterUpdate, 
            TDbContext dbContext, 
            IMapper mapper) =>
        {
            
            var characterSet = dbContext.Set<Character<TUser, TKey>>();
            var updateValid = await characterSet
                .AnyAsync(character => character.Account.Id.Equals(id) &&
                                       character.CharacterId.Equals(characterUpdate.CharacterId));

            if (!updateValid)
            {
                return TypedResults.NotFound();;
            }

            await dbContext.Set<Character<TUser, TKey>>().Persist(mapper)
                .InsertOrUpdateAsync(characterUpdate);
            await dbContext.SaveChangesAsync();
            return TypedResults.Ok();
        });
    }
}