using AutoMapper;
using MMOPortal.GameApi.Data;

namespace MMOPortal.DTO;

[AutoMap(typeof(GameServer<Guid>), ReverseMap = true)]
[AutoMap(typeof(GameServerDTO), ReverseMap = true)]
public class GameServerDTO
{
    public virtual Guid GameServerId { get; set; }
    public virtual string Name { get; set; }
    public virtual string SharedSecret { get; set; }
    public virtual bool SharedSecretVisible { get; set; } = false;

    public virtual string Token { get; set; }
    public virtual bool TokenVisible { get; set; } = false;
}