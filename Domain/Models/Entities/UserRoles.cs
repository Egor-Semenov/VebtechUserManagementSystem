using Domain.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Entities
{
    public sealed class UserRoles
    {
        public Roles Role { get; set; }
        [NotMapped]
        public Role Roles { get; set; }
        public int UserId { get; set; }
        [NotMapped]
        public User User { get; set; }
    }
}
