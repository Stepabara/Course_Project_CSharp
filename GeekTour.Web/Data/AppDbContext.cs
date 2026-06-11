using GeekTour.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Fandom> Fandoms => Set<Fandom>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<TourRoute> Routes => Set<TourRoute>();
    public DbSet<RoutePoint> RoutePoints => Set<RoutePoint>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<EventItem> Events => Set<EventItem>();
    public DbSet<LocationImage> LocationImages => Set<LocationImage>();
    public DbSet<MenuCatalogItem> MenuCatalogItems => Set<MenuCatalogItem>();
    public DbSet<ViewStatistics> ViewStatistics => Set<ViewStatistics>();
    public DbSet<LocationFandom> LocationFandoms => Set<LocationFandom>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<int>();
        });

        // Location
        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(l => l.Type).HasConversion<int>();
            entity.HasOne(l => l.Category).WithMany(c => c.Locations).HasForeignKey(l => l.CategoryId);
            entity.HasOne(l => l.Owner).WithMany(u => u.OwnedLocations).HasForeignKey(l => l.OwnerId).IsRequired(false);
            entity.Ignore(l => l.AverageRating);
            entity.Ignore(l => l.ReviewCount);
        });

        // Fandom
        modelBuilder.Entity<Fandom>(entity =>
        {
            entity.Property(f => f.Category).HasConversion<int>();
        });

        // LocationFandom (many-to-many)
        modelBuilder.Entity<LocationFandom>(entity =>
        {
            entity.HasKey(lf => new { lf.LocationId, lf.FandomId });
            entity.HasOne(lf => lf.Location).WithMany(l => l.LocationFandoms).HasForeignKey(lf => lf.LocationId);
            entity.HasOne(lf => lf.Fandom).WithMany(f => f.LocationFandoms).HasForeignKey(lf => lf.FandomId);
        });

        // Ignore shadow navigation properties
        modelBuilder.Entity<Location>().Ignore(l => l.Fandoms);
        modelBuilder.Entity<Fandom>().Ignore(f => f.Locations);

        // Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId);
            entity.HasOne(r => r.Location).WithMany(l => l.Reviews).HasForeignKey(r => r.LocationId);
        });

        // TourRoute
        modelBuilder.Entity<TourRoute>(entity =>
        {
            entity.HasOne(r => r.User).WithMany(u => u.Routes).HasForeignKey(r => r.UserId);
        });

        // RoutePoint
        modelBuilder.Entity<RoutePoint>(entity =>
        {
            entity.HasOne(rp => rp.Route).WithMany(r => r.Points).HasForeignKey(rp => rp.RouteId);
            entity.HasOne(rp => rp.Location).WithMany().HasForeignKey(rp => rp.LocationId);
        });

        // Promotion
        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasOne(p => p.Location).WithMany(l => l.Promotions).HasForeignKey(p => p.LocationId);
            entity.Ignore(p => p.IsActive);
        });

        // Event
        modelBuilder.Entity<EventItem>(entity =>
        {
            entity.HasOne(e => e.Location).WithMany(l => l.Events).HasForeignKey(e => e.LocationId);
        });

        // LocationImage
        modelBuilder.Entity<LocationImage>(entity =>
        {
            entity.HasOne(li => li.Location).WithMany(l => l.Images).HasForeignKey(li => li.LocationId);
        });

        // MenuCatalogItem
        modelBuilder.Entity<MenuCatalogItem>(entity =>
        {
            entity.HasOne(m => m.Location).WithMany(l => l.MenuCatalog).HasForeignKey(m => m.LocationId);
        });

        // ViewStatistics
        modelBuilder.Entity<ViewStatistics>(entity =>
        {
            entity.HasOne(vs => vs.Location).WithMany(l => l.ViewStats).HasForeignKey(vs => vs.LocationId);
            entity.HasOne(vs => vs.User).WithMany().HasForeignKey(vs => vs.UserId).IsRequired(false);
        });
    }
}
