using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AuthDTOs
{
    public class LoginDTO
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
