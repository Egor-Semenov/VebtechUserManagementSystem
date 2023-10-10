using Domain.Models.Enums; 

namespace Application.DTOs
{
    public sealed record UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public List<Roles> Roles { get; set; }
    }
}
