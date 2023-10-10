
namespace Domain.Models.Entities
{
    public sealed class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<UserRoles> Roles { get; set; } = new List<UserRoles>();
    }
}
