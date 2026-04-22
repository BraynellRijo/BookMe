namespace Application.DTOs.TokensDTOs
{
    public class RequestRefreshTokenDTO
    {
        public Guid UserId { get; set; }
        public required string? RefreshToken { get; set; }
    }
}
