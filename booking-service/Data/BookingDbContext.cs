using Microsoft.EntityFrameworkCore;

namespace booking_service.Data;

public class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<EventCapacity> EventCapacities => Set<EventCapacity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(e =>
        {
            e.ToTable("bookings", t => t.HasCheckConstraint("ck_bookings_seats", "seats >= 1 AND seats <= 10"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            e.Property(x => x.EventId).HasColumnName("event_id").IsRequired();
            e.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(100).IsRequired();
            e.Property(x => x.Seats).HasColumnName("seats").HasDefaultValue(1);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz").HasDefaultValueSql("now()");
            e.HasIndex(x => x.EventId);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => new { x.EventId, x.UserId }).IsUnique();
        });

        modelBuilder.Entity<EventCapacity>(e =>
        {
            e.ToTable("event_capacity", t => t.HasCheckConstraint("ck_event_capacity_reserved", "reserved <= capacity"));
            e.HasKey(x => x.EventId);
            e.Property(x => x.EventId).HasColumnName("event_id").ValueGeneratedNever();
            e.Property(x => x.Capacity).HasColumnName("capacity");
            e.Property(x => x.Reserved).HasColumnName("reserved").HasDefaultValue(0);
        });
    }
}
