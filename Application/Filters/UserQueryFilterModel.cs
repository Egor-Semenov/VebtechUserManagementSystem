using Domain.Models.Enums;

namespace Application.Filters
{
    public sealed record UserQueryFilterModel
    {
        public string? Name { get; set; } = null;
        public int? Age { get; set; } = null;
        public string? Email { get; set; } = null;
        public Roles? Role { get; set; } = null;
    }
}
