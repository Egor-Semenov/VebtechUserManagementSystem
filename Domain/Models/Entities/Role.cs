using Domain.Models.Enums;

namespace Domain.Models.Entities
{
    public sealed class Role
    {
        public Roles UserRole { get; set; }
        public List<User> Users { get; set; }
    }
}
