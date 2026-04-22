namespace Application.DTOs.TokensDTOs
{
    public class TokenResponseDTO
    {
        public required string Accesstoken { get; set; }
        public string RefreshToken { get; set; }
    }
}
