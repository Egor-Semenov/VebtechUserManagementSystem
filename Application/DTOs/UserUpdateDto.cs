
using Domain.Models.Enums;

namespace Application.DTOs
{
    public sealed record UserUpdateDto
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<Roles> Roles { get; set; }
    }
}
