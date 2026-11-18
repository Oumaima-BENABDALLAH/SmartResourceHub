namespace Identity.API.Models
{
    public sealed record RefreshTokenRequest(
     string UserId,
     string RefreshToken);
}
