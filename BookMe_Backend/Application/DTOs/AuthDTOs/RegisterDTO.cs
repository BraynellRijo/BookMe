using Domain.Enums.Users;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AuthDTOs
{
    public class RegisterDTO
    {
        public required string FirstName { get; set; } = string.Empty;
        public required string LastName { get; set; } = string.Empty;
        public required Genders Gender { get; set; }
        public required string PhoneNumber { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
