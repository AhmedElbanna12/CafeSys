using Foodics.Dtos.Cart.Cart;
using Foodics.Dtos.Cart.Order;
using Foodics.Dtos.Cart.Promocode;
using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using System.Security.Claims;
using AppUser = Foodics.Models.User;

namespace Foodics.Controllers       
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string? GetUserId() => User.FindFirstValue("userId");

        // ➕ Add to Cart
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var userId = GetUserId();
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                _context.Carts.Add(cart);
            }

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) return NotFound("Product not found");

            var item = cart.Items.FirstOrDefault(x =>
                x.ProductId == dto.ProductId &&
                x.ProductSizeId == dto.ProductSizeId &&
                x.Modifiers.Select(m => m.ModifierOptionId).OrderBy(id => id)
                    .SequenceEqual(dto.ModifierOptionIds.OrderBy(id => id))
            );

            if (item != null)
            {
                item.Quantity += dto.Quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = dto.ProductId,
                    ProductSizeId = dto.ProductSizeId,
                    Quantity = dto.Quantity,
                    Price = product.Price,
                    Modifiers = new List<CartItemModifier>()
                };

                foreach (var modId in dto.ModifierOptionIds)
                {
                    var option = await _context.ModifierOptions.FindAsync(modId);
                    if (option != null)
                    {
                        cartItem.Modifiers.Add(new CartItemModifier
                        {
                            ModifierOptionId = modId,
                            Price = option.ExtraPrice
                        });
                    }
                }

                cart.Items.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // تحويل البيانات لـ DTO
            var cartDto = MapCartToDto(cart);
            return Ok(cartDto);
        }

        // ✏️ Update Cart Item Quantity
        [HttpPost("update")]
        public async Task<IActionResult> UpdateCartItem(UpdateCartItemDto dto)
        {
            var userId = GetUserId();
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return NotFound("Cart not found");

            var item = cart.Items.FirstOrDefault(i => i.Id == dto.CartItemId);
            if (item == null) return NotFound("Cart item not found");

            if (dto.Quantity <= 0)
                cart.Items.Remove(item);
            else
                item.Quantity = dto.Quantity;

            await _context.SaveChangesAsync();

            var cartDto = MapCartToDto(cart);
            return Ok(cartDto);
        }

        // 🗑 Remove Cart Item
        [HttpPost("remove/{id}")]
        public async Task<IActionResult> RemoveCartItem(int id)
        {
            var userId = GetUserId();
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return NotFound("Cart not found");

            var item = cart.Items.FirstOrDefault(i => i.Id == id);
            if (item == null) return NotFound("Cart item not found");

            cart.Items.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(cart);
        }

        // 🎁 Apply Promo
        [HttpPost("apply-promo")]
        public async Task<IActionResult> ApplyPromo(ApplyPromoDto dto)
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            // 🔹 حساب SubTotal
            var subTotal = cart.Items.Sum(i =>
                (i.Price + i.Modifiers.Sum(m => m.Price)) * i.Quantity
            );

            // 🔹 جلب البروموكود
            var promo = await _context.PromoCodes
                .FirstOrDefaultAsync(p => p.Code == dto.PromoCode && p.IsActive);

            decimal discount = 0;

            if (promo != null)
            {
                discount = subTotal * (promo.DiscountAmount / 100m);
                cart.PromoCode = dto.PromoCode;
                cart.Discount = discount; // لو عندك field في Cart
            }
            else
            {
                return BadRequest("Invalid promo code");
            }

            var total = subTotal - discount;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                cart.Id,
                cart.UserId,
                cart.PromoCode,
                SubTotal = subTotal,
                Discount = discount,
                Total = total,
                Items = cart.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.Quantity,
                    i.Price,
                    Modifiers = i.Modifiers.Select(m => new
                    {
                        m.ModifierOptionId,
                        m.Price
                    })
                })
            });
        }

        // ✅ Checkout / Create Order
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutDto dto)
        {
            var userId = GetUserId();

            // جلب الكارت مع كل التفاصيل
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.Items == null || !cart.Items.Any())
                return BadRequest("Cart is empty");

            // 🔹 حساب SubTotal
            decimal subTotal = 0;
            foreach (var item in cart.Items)
            {
                decimal itemTotal = item.Price * item.Quantity;
                itemTotal += item.Modifiers.Sum(m => m.Price) * item.Quantity;
                subTotal += itemTotal;
            }

            // 🔹 حساب الخصم
            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(dto.PromoCode))
            {
                var promo = await _context.PromoCodes
                    .FirstOrDefaultAsync(p => p.Code == dto.PromoCode && p.IsActive);

                if (promo != null)
                {
                    discountAmount = subTotal * (promo.DiscountAmount / 100m);
                }
            }

            // 🔥 حساب الدليفري
            decimal deliveryFee = 0;

            if (dto.OrderType == OrderType.Delivery)
            {
                var settings = await _context.AppSettings.FirstOrDefaultAsync();
                deliveryFee = settings?.DeliveryFee ?? 50; // fallback
            }

            // 🔥 الحساب النهائي
            decimal totalAmount = subTotal - discountAmount + deliveryFee;

            // 🔹 حساب النقاط
            int pointsEarned = (int)(totalAmount / 10);

            var order = new Order
            {
                UserId = userId,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                DeliveryFee = deliveryFee, // 👈 جديد
                TotalAmount = totalAmount,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = dto.PaymentMethod == PaymentMethod.CashOnDelivery
                    ? PaymentStatus.Unpaid
                    : PaymentStatus.Unpaid,
                PaymentMethod = dto.PaymentMethod,
                OrderType = dto.OrderType,
                ShippingAddress = dto.ShippingAddress,
                PointsEarned = pointsEarned,
                PointsRedeemed = dto.PointsRedeemed,
                OrderItems = new List<OrderItem>() // مهم عشان متحصلش null
            };

            // 🔹 تحويل CartItems إلى OrderItems
            foreach (var cartItem in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.Name,
                    ProductSizeId = cartItem.ProductSizeId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Price,
                    TotalPrice = (cartItem.Price + cartItem.Modifiers.Sum(m => m.Price)) * cartItem.Quantity,
                    Modifiers = cartItem.Modifiers.Select(m => new OrderItemModifier
                    {
                        ModifierOptionId = m.ModifierOptionId,
                        Price = m.Price
                    }).ToList()
                };

                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);

            // 🗑 مسح الكارت
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            // 🔹 Response
            return Ok(new
            {
                order.Id,
                order.UserId,
                order.SubTotal,
                order.DiscountAmount,
                order.DeliveryFee, // 👈 جديد
                order.TotalAmount,
                order.PointsEarned,
                Items = order.OrderItems.Select(oi => new
                {
                    oi.ProductId,
                    oi.ProductName,
                    oi.ProductSizeId,
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.TotalPrice,
                    Modifiers = oi.Modifiers.Select(m => new { m.ModifierOptionId, m.Price })
                })
            });
        }


        private CartDto MapCartToDto(Cart cart)
        {
            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.Items.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    Modifiers = ci.Modifiers.Select(m => new CartItemModifierDto
                    {
                        Id = m.Id,
                        ModifierOptionName = m.ModifierOption.Name,
                        Price = m.Price
                    }).ToList()
                }).ToList(),
                SubTotal = cart.Items.Sum(i => (i.Price + i.Modifiers.Sum(m => m.Price)) * i.Quantity),
                Discount = 0, // حسب منطق الخصم عندك
                Total = cart.Items.Sum(i => (i.Price + i.Modifiers.Sum(m => m.Price)) * i.Quantity),
                PromoCode = null
            };
        }

        [HttpGet("mycart")]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetUserId(); // دالة تجيب ال userId من الـ JWT أو session

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return Ok(new CartResponseDto
            {
                UserId = userId,
                Items = new List<CartItemDto>(),
                SubTotal = 0,
                Discount = 0,
                Total = 0
            });

            var response = new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                SubTotal = cart.Items.Sum(i => i.Price * i.Quantity + i.Modifiers.Sum(m => m.Price) * i.Quantity),
                Discount = cart.Discount,
                Total = cart.Items.Sum(i => i.Price * i.Quantity + i.Modifiers.Sum(m => m.Price) * i.Quantity) - cart.Discount,
                PromoCode = cart.PromoCode,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : "",
                    ProductSizeId = i.ProductSizeId,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Modifiers = i.Modifiers.Select(m => new CartItemModifierDto
                    {
                        Id = m.Id,
                        ModifierOptionId = m.ModifierOptionId,
                        ModifierOptionName = m.ModifierOption != null ? m.ModifierOption.Name : "",
                        Price = m.Price
                    }).ToList()
                }).ToList()
            };

            return Ok(response);
        }
    }
}
