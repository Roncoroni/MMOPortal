namespace MMOPortal.GameApi.Authentication;

/// <summary>
/// The JSON data transfer object for the GameServer token response typically found in "/login" and "/refresh" responses.
/// </summary>
public sealed class GameServerTokenResponse
{
    /// <summary>
    /// The value is always "GameServer" which indicates this response provides a "GameServer" token
    /// in the form of an opaque <see cref="AccessToken"/>.
    /// </summary>
    /// <remarks>
    /// This is serialized as "tokenType": "GameServer" using <see cref="JsonSerializerDefaults.Web"/>.
    /// </remarks>
    public string TokenType { get; } = GameServerTokenDefaults.AuthenticationScheme;

    /// <summary>
    /// The opaque GameServer token to send as part of the Authorization request header.
    /// </summary>
    /// <remarks>
    /// This is serialized as "accessToken": "{AccessToken}" using <see cref="JsonSerializerDefaults.Web"/>.
    /// </remarks>
    public required string AccessToken { get; init; }
}