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
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ModifierGroup> ModifierGroups { get; set; }
        public DbSet<ModifierOption> ModifierOptions { get; set; }
        public DbSet<OrderItemModifier> OrderItemModifiers { get; set; }

        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<PointsTransaction> PointsTransactions { get; set; }

        public DbSet<Reward> Rewards { get; set; }
        public DbSet<RedeemedReward> RedeemedRewards { get; set; }

        public DbSet<UserPoints> UserPoints { get; set; }
        public DbSet<OtpCode> OtpCode { get; set; }

        public DbSet<Advertisement> Advertisements { get; set; }

        public DbSet<Notification> Notifications { get; set; }





        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);



            builder.Entity<OrderItem>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.DiscountPercentage)
                .HasPrecision(5, 2); // مثلا 99.99%

            // تأكد من uniqueness على PhoneNumber
            builder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            // Precision Configurations
            builder.Entity<Ingredient>()
                .Property(i => i.MinQuantity)
                .HasPrecision(18, 2);

            builder.Entity<Ingredient>()
                .Property(i => i.Quantity)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Entity<ProductSize>()
                .Property(ps => ps.Price)
                .HasPrecision(18, 2);

            builder.Entity<ModifierOption>()
                .Property(mo => mo.ExtraPrice)
                .HasPrecision(18, 2);

            builder.Entity<ProductIngredient>()
                .Property(pi => pi.Quantity)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.Entity<StockMovement>()
                .Property(sm => sm.Quantity)
                .HasPrecision(18, 2);

            // Product → Sizes
            builder.Entity<Product>()
                .HasMany(p => p.Sizes)
                .WithOne(ps => ps.Product)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product → ModifierGroups
            builder.Entity<Product>()
                .HasMany(p => p.ModifierGroups)
                .WithOne(mg => mg.Product)
                .HasForeignKey(mg => mg.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ModifierGroup → ModifierOptions
            builder.Entity<ModifierGroup>()
                .HasMany(mg => mg.Options)
                .WithOne(o => o.ModifierGroup)
                .HasForeignKey(o => o.ModifierGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem → Modifiers
            builder.Entity<OrderItem>()
     .HasMany(oi => oi.Modifiers)
     .WithOne(oim => oim.OrderItem)
     .HasForeignKey(oim => oim.OrderItemId)
     .OnDelete(DeleteBehavior.Restrict);

            // Orders → POSDevice
            builder.Entity<Order>()
                .HasOne(o => o.POSDevice)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.POSDeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Orders → Branch
            builder.Entity<Order>()
                .HasOne(o => o.Branch)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Orders → Payment (One to One)
            builder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
