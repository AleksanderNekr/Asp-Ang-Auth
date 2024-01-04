using Microsoft.AspNetCore.Identity;

namespace OneDomain.Models;

public class ApplicationUser : IdentityUser
{
    public IList<Message> Messages { get; set; } = new List<Message>();
}