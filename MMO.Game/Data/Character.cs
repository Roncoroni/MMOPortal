using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MMO.Game.Data;

public class Character
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid CharacterId { get; set; }
    [JsonIgnore]
    public Guid AccountId { get; set; }
    [JsonIgnore]
    private IGameUser? _account;
    [JsonIgnore]
    public IGameUser Account
    {
        set => _account = value;
        get => _account
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Account));
    }
    
    [Required, MaxLength(60)]
    public required string Name { get; set; }

    public double PositionX { get; set; } = 0;
    public double PositionY { get; set; } = 0;
    public double PositionZ { get; set; } = 0;
    public double Orientation { get; set; } = 0;
}
