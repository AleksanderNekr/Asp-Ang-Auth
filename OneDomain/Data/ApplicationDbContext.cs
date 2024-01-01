using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OneDomain.Models;

namespace OneDomain.Data;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions)
        : base(options, operationalStoreOptions) {}

    public DbSet<Message> MessageEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Message>()
            .ToTable("messages");

        builder.Entity<Message>()
            .Property(e => e.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.Entity<Message>()
            .Property<string>(e => e.UserId)
            .IsRequired()
            .HasColumnType("varchar(255)")
            .HasColumnName("user_id");

        builder.Entity<Message>()
            .Property<string>(e => e.MessageText)
            .IsRequired()
            .HasColumnType("varchar(1000)")
            .HasColumnName("message_text");

        builder.Entity<Message>()
            .HasKey(e => e.Id)
            .HasName("message_id_pk");

        builder.Entity<Message>()
            .HasOne<ApplicationUser>(message => message.User)
            .WithMany(user => user.Messages)
            .HasForeignKey(message => message.UserId)
            .HasConstraintName("user_has_messages_fk")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
