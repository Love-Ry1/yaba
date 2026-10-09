using Microsoft.EntityFrameworkCore;

namespace event_service.Data;

public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable("events", t => t.HasCheckConstraint("ck_events_capacity", "capacity > 0"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            e.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
            e.Property(x => x.Location).HasColumnName("location").HasMaxLength(200);
            e.Property(x => x.StartsAt).HasColumnName("starts_at").HasColumnType("timestamptz");
            e.Property(x => x.Capacity).HasColumnName("capacity");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz").HasDefaultValueSql("now()");
            e.HasIndex(x => x.StartsAt);
        });
    }
}
