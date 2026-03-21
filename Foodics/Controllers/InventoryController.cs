using Foodics.Dtos.Admin.Ingrediants;
using Foodics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;

namespace Foodics.Controllers
{
    [Route("api/admin/inventory")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1️⃣ Add Ingredient
        [HttpPost("ingredients")]
        public async Task<IActionResult> AddIngredient(CreateIngredientDto dto)
        {
            var ingredient = new Ingredient
            {
                Name = dto.Name,
                Unit = dto.Unit,
                Quantity = dto.Quantity,
                MinQuantity = dto.MinQuantity
            };

            _context.Ingredients.Add(ingredient);

            await _context.SaveChangesAsync();

            return Ok(ingredient);
        }

        // 2️⃣ Get Ingredients
        [HttpGet("ingredients")]
        public async Task<IActionResult> GetIngredients()
        {
            var ingredients = await _context.Ingredients.ToListAsync();

            return Ok(ingredients);
        }

        // 3️⃣ Add Ingredient to Product
        [HttpPost("products/{productId}/ingredients")]
        public async Task<IActionResult> AddIngredientToProduct(int productId, AddProductIngredientDto dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Product not found");

            var ingredient = await _context.Ingredients.FindAsync(dto.IngredientId);
            if (ingredient == null)
                return NotFound("Ingredient not found");

            var productIngredient = new ProductIngredient
            {
                ProductId = productId,
                IngredientId = dto.IngredientId,
                Quantity = dto.Quantity
            };

            _context.ProductIngredients.Add(productIngredient);

            await _context.SaveChangesAsync();

            return Ok(productIngredient);
        }

        // 4️⃣ Get Inventory
        [HttpGet]
        public async Task<IActionResult> GetInventory()
        {
            var inventory = await _context.Ingredients
                .Select(i => new
                {
                    i.Id,
                    i.Name,
                    i.Unit,
                    i.Quantity,
                    i.MinQuantity,
                    LowStock = i.Quantity <= i.MinQuantity
                })
                .ToListAsync();

            return Ok(inventory);
        }

        // 5️⃣ Add Stock (Purchase)
        [HttpPost("purchase")]
        public async Task<IActionResult> AddStock(StockPurchaseDto dto)
        {
            var ingredient = await _context.Ingredients.FindAsync(dto.IngredientId);

            if (ingredient == null)
                return NotFound("Ingredient not found");

            ingredient.Quantity += dto.Quantity;

            var movement = new StockMovement
            {
                IngredientId = ingredient.Id,
                Quantity = dto.Quantity,
                Type = "Purchase"
            };

            _context.StockMovements.Add(movement);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Stock added successfully",
                ingredient.Name,
                ingredient.Quantity
            });
        }

        // 6️⃣ Get Stock Movements
        [HttpGet("stock-movements")]
        public async Task<IActionResult> GetStockMovements()
        {
            var movements = await _context.StockMovements
                .Include(s => s.Ingredient)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    Ingredient = s.Ingredient.Name,
                    s.Quantity,
                    s.Type,
                    s.CreatedAt
                })
                .ToListAsync();

            return Ok(movements);
        }
    }
}