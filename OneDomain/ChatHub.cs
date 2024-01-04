using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneDomain.Data;
using OneDomain.Models;

namespace OneDomain;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendMessageAsync(string user, string text)
    {
        ApplicationUser sender = await _context.Users.SingleAsync(u => u.NormalizedUserName == user.ToUpperInvariant());

        sender.Messages.Add(new Message
        {
            MessageText = text
        });

        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("Receive", user, text);
    }
}
