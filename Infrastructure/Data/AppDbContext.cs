using Microsoft.EntityFrameworkCore;
using CourseProject.Models;
using System.Text.Json;

namespace CourseProject.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<TrainType> TrainTypes => Set<TrainType>();
    public DbSet<Train> Trains => Set<Train>();
    public DbSet<TimeTableEntry> TimeTableEntries => Set<TimeTableEntry>();
    public DbSet<PerkGroup> PerkGroups => Set<PerkGroup>();
    public DbSet<PricePolicy> PricePolicies => Set<PricePolicy>();
    public DbSet<PricePolicyPerkGroup> PricePolicyPerkGroups => Set<PricePolicyPerkGroup>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketBooking> TicketBookings => Set<TicketBooking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(eb =>
        {
            eb.HasKey(u => u.Id);
            eb.Property(u => u.Password).HasMaxLength(64).IsRequired();
            eb.Property(u => u.Salt).HasMaxLength(29).IsRequired();
            eb.Property(u => u.Phone).HasMaxLength(36).IsRequired();
            eb.Property(u => u.Passport).HasMaxLength(9);
            eb.Property(u => u.Name).HasMaxLength(64);
            eb.Property(u => u.Surname).HasMaxLength(64);
        });

        modelBuilder.Entity<Station>(eb =>
        {
            eb.HasKey(s => s.Id);
            eb.Property(s => s.Name).HasMaxLength(128).IsRequired();
            eb.Property(s => s.CityName).HasMaxLength(128).IsRequired();
            eb.Property(s => s.Description).HasMaxLength(2048);
        });

        modelBuilder.Entity<TrainType>(eb =>
        {
            eb.HasKey(t => t.Id);
            eb.Property(t => t.Name).HasMaxLength(128).IsRequired();
            eb.Property(t => t.Description).HasMaxLength(2048);
        });

        modelBuilder.Entity<Train>(eb =>
        {
            eb.HasKey(t => t.Id);
            eb.HasOne(t => t.Source).WithMany(s => s.SourceTrains).HasForeignKey(t => t.SourceId).OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(t => t.Destination).WithMany(s => s.DestinationTrains).HasForeignKey(t => t.DestinationId).OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(t => t.Type).WithMany(tt => tt.Trains).HasForeignKey(t => t.TypeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TimeTableEntry>(eb =>
        {
            eb.HasKey(e => e.Id);
            eb.Property(e => e.DepartureTime).IsRequired();
            eb.Property(e => e.ArrivalTime).IsRequired();
            eb.HasOne(e => e.Train).WithMany(t => t.TimeTableEntries).HasForeignKey(e => e.TrainId).OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(e => e.PricePolicy).WithMany(p => p.TimeTableEntries).HasForeignKey(e => e.PricePolicyId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PerkGroup>(eb =>
        {
            eb.HasKey(p => p.Id);
            eb.Property(p => p.Name).HasMaxLength(64).IsRequired();
            eb.Property(p => p.Description).HasMaxLength(2048);
            eb.Property(p => p.FixedPrice).HasColumnType("numeric");
            eb.Property(p => p.Discount).HasColumnType("numeric");
        });

        modelBuilder.Entity<PricePolicy>(eb =>
        {
            eb.HasKey(p => p.Id);
            eb.Property(p => p.PricePerKm);
            eb.Property(p => p.FixedPrice).HasColumnType("numeric");
        });

        modelBuilder.Entity<PricePolicyPerkGroup>(eb =>
        {
            eb.ToTable("PricePolicyPerkGroups");

            eb.HasKey(pp => new 
            { 
                pp.PricePolicyId, 
                pp.PerkGroupId 
            });

            eb.HasOne(pp => pp.PricePolicy)
            .WithMany(p => p.PerkGroups)
            .HasForeignKey(pp => pp.PricePolicyId)
            .OnDelete(DeleteBehavior.Cascade);

            eb.HasOne(pp => pp.PerkGroup)
            .WithMany(pg => pg.PricePolicies)
            .HasForeignKey(pp => pp.PerkGroupId)
            .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Ticket>(eb =>
        {
            eb.HasKey(t => t.Id);
            eb.HasOne(t => t.Entry).WithMany().HasForeignKey(t => t.EntryId).OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(t => t.User).WithMany(u => u.Tickets).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            eb.Property(t => t.Used).HasDefaultValue(false);
        });

        modelBuilder.Entity<TicketBooking>(eb =>
        {
            eb.HasKey(l => l.Id);
            eb.HasOne(l => l.Entry).WithMany().HasForeignKey(l => l.EntryId).OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(l => l.User).WithMany(u => u.TicketBookings).HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
            eb.Property(l => l.InvoiceId).HasMaxLength(64);
            eb.Property(l => l.Paid).HasDefaultValue(false);
            eb.Property(l => l.Sum).HasColumnType("numeric");
        });

        modelBuilder.Entity<Payment>(eb =>
        {
            eb.HasKey(p => p.Id);
            eb.HasOne(p => p.Lock).WithMany().HasForeignKey(p => p.BookingId).OnDelete(DeleteBehavior.Cascade);
            eb.Property(p => p.InvoiceId).HasMaxLength(64);
            eb.Property(p => p.Successful).HasDefaultValue(false);
            eb.Property(p => p.DateTime).IsRequired();
            eb.Property(p => p.Sum).HasColumnType("numeric");
        });
    }
}
