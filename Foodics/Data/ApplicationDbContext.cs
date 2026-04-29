using Foodics.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<CartItemModifier> CartItemModifiers { get; set; }

        public DbSet<PromoCode> PromoCodes { get; set; }

        public DbSet<AppSettings> AppSettings { get; set; }

        public DbSet<UserDevice> UserDevices { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // =========================
            // 🔴 CART RELATIONS
            // =========================

            builder.Entity<CartItem>()
      .HasOne(ci => ci.Product)
      .WithMany()
      .HasForeignKey(ci => ci.ProductId)
      .OnDelete(DeleteBehavior.NoAction); // ✅ بدل Restrict أو Cascade

            builder.Entity<CartItem>()
                .HasOne(ci => ci.ProductSize)
                .WithMany()
                .HasForeignKey(ci => ci.ProductSizeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<CartItemModifier>()
                .HasOne(c => c.ModifierOption)
                .WithMany()
                .HasForeignKey(c => c.ModifierOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CartItemModifier>()
                .HasOne(c => c.CartItem)
                .WithMany(c => c.Modifiers)
                .HasForeignKey(c => c.CartItemId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // 🔴 ORDER RELATIONS
            // =========================

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductSize)
                .WithMany()
                .HasForeignKey(oi => oi.ProductSizeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<OrderItem>()
                .HasMany(oi => oi.Modifiers)
                .WithOne(m => m.OrderItem)
                .HasForeignKey(m => m.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 🔴 PRODUCT RELATIONS
            // =========================

            builder.Entity<Product>()
                .HasMany(p => p.Sizes)
                .WithOne(s => s.Product)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Product>()
                .HasMany(p => p.ModifierGroups)
                .WithOne(m => m.Product)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ModifierGroup>()
                .HasMany(mg => mg.Options)
                .WithOne(o => o.ModifierGroup)
                .HasForeignKey(o => o.ModifierGroupId)
                .OnDelete(DeleteBehavior.Cascade);



            builder.Entity<PromoCode>()
    .Property(p => p.DiscountAmount)
    .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.DiscountPercentage)
                .HasPrecision(5, 2);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.Entity<AppSettings>()
                .Property(p => p.DeliveryFee)
                .HasPrecision(18, 2);

            // =========================
            // 🔴 PRECISION FIXES
            // =========================

            builder.Entity<Product>().Property(x => x.Price).HasPrecision(18, 2);
            builder.Entity<ProductSize>().Property(x => x.Price).HasPrecision(18, 2);

            builder.Entity<Order>().Property(x => x.SubTotal).HasPrecision(18, 2);
            builder.Entity<Order>().Property(x => x.TotalAmount).HasPrecision(18, 2);
            builder.Entity<Order>().Property(x => x.DiscountAmount).HasPrecision(18, 2);
            builder.Entity<Order>().Property(x => x.DeliveryFee).HasPrecision(18, 2);

            builder.Entity<Cart>().Property(x => x.Total).HasPrecision(18, 2);
            builder.Entity<Cart>().Property(x => x.SubTotal).HasPrecision(18, 2);
            builder.Entity<Cart>().Property(x => x.Discount).HasPrecision(18, 2);

            builder.Entity<CartItem>().Property(x => x.Price).HasPrecision(18, 2);
            builder.Entity<CartItemModifier>().Property(x => x.Price).HasPrecision(18, 2);

            builder.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(18, 2);
            builder.Entity<OrderItem>().Property(x => x.TotalPrice).HasPrecision(18, 2);
            builder.Entity<OrderItem>().Property(x => x.DiscountAmount).HasPrecision(18, 2);

            builder.Entity<OrderItemModifier>().Property(x => x.Price).HasPrecision(18, 2);

            builder.Entity<ModifierOption>().Property(x => x.ExtraPrice).HasPrecision(18, 2);

            builder.Entity<ProductIngredient>().Property(x => x.Quantity).HasPrecision(18, 2);
            builder.Entity<Ingredient>().Property(x => x.Quantity).HasPrecision(18, 2);
            builder.Entity<Ingredient>().Property(x => x.MinQuantity).HasPrecision(18, 2);

            builder.Entity<StockMovement>().Property(x => x.Quantity).HasPrecision(18, 2);

            // =========================
            // 🔴 INDEX / RULES
            // =========================

            builder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();
        }
    }
}
