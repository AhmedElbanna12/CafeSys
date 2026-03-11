using Foodics.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace POSSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches { get; set; }
        public DbSet<POSDevice> POSDevices { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PointsTransaction> PointsTransactions { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<RedeemedReward> RedeemedRewards { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
          
            // Orders → POSDevice
            builder.Entity<Order>()
                .HasOne(o => o.POSDevice)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.POSDeviceId)
                .OnDelete(DeleteBehavior.Restrict); // NO ACTION

            // Orders → Branch
            builder.Entity<Order>()
                .HasOne(o => o.Branch)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BranchId)
                .OnDelete(DeleteBehavior.Restrict); // NO ACTION

            // Orders → Payment (one-to-one)
            builder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // ده آمن لأنه مسار واحد
        }
    }
}