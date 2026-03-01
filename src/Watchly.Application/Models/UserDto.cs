using Watchly.Domain.Entities;

namespace Watchly.Application.Models;

public class UserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string UserName { get; set; }

    public static UserDto MapUser(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName
        };
    }
}
