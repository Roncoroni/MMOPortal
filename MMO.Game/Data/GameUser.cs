namespace MMO.Game.Data;

public interface IGameUser
{
    protected internal ICollection<Character> Characters { get; set; }
}