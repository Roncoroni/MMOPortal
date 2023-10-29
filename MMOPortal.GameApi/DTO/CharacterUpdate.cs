using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MMOPortal.GameApi.Data;

namespace MMOPortal.GameApi.DTO;

//[AutoMap(typeof(Character<,>), ReverseMap = true)]
public record CharacterUpdate<TKey>(TKey CharacterId, double PositionX, double PositionY, double PositionZ, double Rotation) where TKey : IEquatable<TKey>;


public record CharacterBatchUpdate<TKey>(IList<CharacterUpdate<TKey>> Batch)  where TKey : IEquatable<TKey>;