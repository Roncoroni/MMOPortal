namespace MMO.Game.DTO;

public record CharacterCreate(string CharacterName);

public record CharacterUpdate(double PositionX, double PositionY, double PositionZ, double Rotation);

public record CharacterBatchUpdate(IDictionary<Guid, CharacterUpdate> Batch);
