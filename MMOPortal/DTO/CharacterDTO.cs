using AutoMapper;
using MMOPortal.Data;
using MMOPortal.GameApi.Data;

namespace MMOPortal.DTO;

[AutoMap(typeof(Character<ApplicationUser, Guid>), ReverseMap = true)]
public record CharacterDTO(Guid CharacterId, string Name, double PositionX, double PositionY, double PositionZ, double Rotation);