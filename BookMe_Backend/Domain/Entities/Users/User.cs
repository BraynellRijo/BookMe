using Domain.Enums.Users;

namespace Domain.Entities.Users
{
    public class User
    {
        public Guid Id { get; set; }
        public IList<RolesType> Roles { get; set; } = new List<RolesType> { RolesType.Guest };
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Genders Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsVerified { get; set; } = false;
        public bool IsRemoved { get; set; } = false;
    }
}
