namespace MMOPortal.DTO;

public record UserListDto(Guid Id, string? UserName, string? Email, IEnumerable<string?> Roles)
{
    //public bool Administrator { get { return Roles.Contains()}}
}