using Watchly.Domain.Entities;

namespace Watchly.Api.Dto.Auth;

public class UserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string Role { get; set; }

    public static UserDto MapUser(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
        };
    }
}
