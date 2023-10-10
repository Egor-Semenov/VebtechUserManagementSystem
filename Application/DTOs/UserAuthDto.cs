
namespace Application.DTOs
{
    public sealed record UserAuthDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
