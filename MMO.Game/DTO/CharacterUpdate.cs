namespace MMO.Game.DTO;

public record CharacterUpdate(Guid CharacterId, double PositionX, double PositionY, double PositionZ, double Rotation);

public record CharacterBatchUpdate(IList<CharacterUpdate> Batch);
