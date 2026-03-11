using Foodics.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace POSSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
    ApplicationDbContext context,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    string adminEmail,
    string adminPassword)  // ← هنا
        {
            context.Database.Migrate();

            // 1️⃣ Roles
            string[] roles = new[] { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2️⃣ Admin user
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, adminPassword);
                await userManager.AddToRoleAsync(admin, "Admin");
            }

       
            // 3️⃣ Branch
            if (!context.Branches.Any())
            {
                var branch = new Branch
                {
                    Name = "Main Branch",
                    Address = "123 POS Street",
                    Phone = "0123456789",
                    IsActive = true
                };
                context.Branches.Add(branch);
                await context.SaveChangesAsync();

                // POSDevice
                var device = new POSDevice
                {
                    Name = "POS Device 1",
                    DeviceCode = "POS001",
                    BranchId = branch.Id,
                    IsActive = true,
                    LastSync = DateTime.UtcNow
                };
                context.POSDevices.Add(device);
                await context.SaveChangesAsync();
            }

            // 4️⃣ Categories
            if (!context.Categories.Any())
            {
                var coffeeCategory = new Category { Name = "Coffee", Description = "Coffee Drinks" };
                var dessertCategory = new Category { Name = "Dessert", Description = "Sweet Items" };
                context.Categories.AddRange(coffeeCategory, dessertCategory);
                await context.SaveChangesAsync();

                // 5️⃣ Products
                var latte = new Product
                {
                    Name = "Latte",
                    Price = 70,
                    PointsReward = 7,
                    CategoryId = coffeeCategory.Id,
                    IsAvailable = true
                };
                var mocha = new Product
                {
                    Name = "Mocha",
                    Price = 80,
                    PointsReward = 8,
                    CategoryId = coffeeCategory.Id,
                    IsAvailable = true
                };
                context.Products.AddRange(latte, mocha);
                await context.SaveChangesAsync();
            }

            // 6️⃣ Ingredients
            if (!context.Ingredients.Any())
            {
                var coffeeBeans = new Ingredient { Name = "Coffee Beans", Unit = "kg", Quantity = 5, MinQuantity = 1 };
                var milk = new Ingredient { Name = "Milk", Unit = "liter", Quantity = 10, MinQuantity = 2 };
                var sugar = new Ingredient { Name = "Sugar", Unit = "kg", Quantity = 8, MinQuantity = 1 };
                context.Ingredients.AddRange(coffeeBeans, milk, sugar);
                await context.SaveChangesAsync();

                // 7️⃣ ProductIngredients (Recipes)
                var latteProduct = await context.Products.FirstOrDefaultAsync(p => p.Name == "Latte");
                var mochaProduct = await context.Products.FirstOrDefaultAsync(p => p.Name == "Mocha");
                if (latteProduct != null)
                {
                    context.ProductIngredients.AddRange(
                        new ProductIngredient { ProductId = latteProduct.Id, IngredientId = coffeeBeans.Id, Quantity = 0.02m },
                        new ProductIngredient { ProductId = latteProduct.Id, IngredientId = milk.Id, Quantity = 0.20m }
                    );
                }
                if (mochaProduct != null)
                {
                    context.ProductIngredients.AddRange(
                        new ProductIngredient { ProductId = mochaProduct.Id, IngredientId = coffeeBeans.Id, Quantity = 0.02m },
                        new ProductIngredient { ProductId = mochaProduct.Id, IngredientId = milk.Id, Quantity = 0.20m },
                        new ProductIngredient { ProductId = mochaProduct.Id, IngredientId = sugar.Id, Quantity = 0.05m }
                    );
                }
                await context.SaveChangesAsync();
            }

            // 8️⃣ Rewards
            if (!context.Rewards.Any())
            {
                var latteProduct = await context.Products.FirstOrDefaultAsync(p => p.Name == "Latte");
                context.Rewards.AddRange(
                    new Reward { Name = "Free Latte", PointsRequired = 50, ProductId = latteProduct?.Id, IsActive = true },
                    new Reward { Name = "Free Mocha", PointsRequired = 80, ProductId = latteProduct?.Id, IsActive = true }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}