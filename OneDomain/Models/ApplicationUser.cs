using Microsoft.AspNetCore.Identity;

namespace OneDomain.Models;

public class ApplicationUser : IdentityUser
{
    public IEnumerable<Message> Messages { get; set; } = null!;
}