using AmxBookstore.Domain.Validation;
using AmxBookstore.Domain.Entities.Users.Enum;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Users
{
    public class User : IdentityUser<Guid>
    {
        public string Name { get; set; }
        public UserRoles Role { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
        public bool Deleted { get; set; }

        public User() { }
        public User(string name, string email, string password, UserRoles role, Guid id = default)
        {
            Id = id == default ? Guid.NewGuid() : id;
            ValidateDomain(name, email, password, role);
        }

        private void ValidateDomain(string name, string email, string? password, UserRoles role)
        {
            DomainExceptionValidation.When(string.IsNullOrEmpty(name), "Invalid name. Name is required");
            DomainExceptionValidation.When(string.IsNullOrEmpty(email), "Invalid email. Email is required");
            DomainExceptionValidation.When(name.Length < 3, "Invalid name, too short, minimum 3 characters");
            Name = name;
            UserName = email;
            Email = email;
            Role = role;
            Deleted = false;
        }

        public void Update(string name, string email, UserRoles role, string password)
        {
            ValidateDomain(name, email, password, role);
        }

    }
}
