using Microsoft.EntityFrameworkCore;

namespace Voyago.UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
}
