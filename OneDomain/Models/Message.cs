namespace OneDomain.Models;

public class Message
{
    public int Id { get; init; }
    public string UserId { get; init; }
    public string MessageText { get; init; }
    public ApplicationUser User { get; init; }
}
