using Foodics.Dtos.Cart.Cart;
using Foodics.Dtos.Cart.Order;
using Foodics.Dtos.Cart.Promocode;
using Foodics.Dtos.Paymob;
using Foodics.ExtensionMethod;
using Foodics.Models;
using Foodics.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IPaymobService _paymobService;


        public CartController(ApplicationDbContext context, UserManager<AppUser> userManager ,  IPaymobService paymobService)
        {
            _context = context;
            _userManager = userManager;
            _paymobService = paymobService;
        }

        // =========================
        // 🌍 Language Helper
        // =========================
        private string GetLang()
        {
            var langHeader = Request.Headers["Accept-Language"].ToString();

            return langHeader
                .Split(',')[0]
                .Trim()
                .ToLower()
                .StartsWith("ar")
                ? "ar"
                : "en";
        }


        private async Task RecalculatePromoAsync(Cart cart)
        {
            if (string.IsNullOrWhiteSpace(cart.PromoCode))
            {
                cart.Discount = 0;
                return;
            }

            var now = NowLocal();

            var promo = await _context.PromoCodes.FirstOrDefaultAsync(x =>
                x.Code == cart.PromoCode &&
                x.IsActive &&
                x.StartDate <= now &&
                x.EndDate >= now);

            if (promo == null)
            {
                cart.PromoCode = null;
                cart.Discount = 0;
                return;
            }

            var subTotal = cart.Items.Sum(i =>
                (i.Price + i.Modifiers.Sum(m => m.Price * m.Quantity)) * i.Quantity);

            cart.Discount = subTotal * promo.DiscountAmount / 100m;
        }


        // =========================
        // ⏱ Timezone
        // =========================
        private static readonly TimeZoneInfo CairoZone =
            TimeZoneInfo.GetSystemTimeZones()
                .FirstOrDefault(tz => tz.Id == "Africa/Cairo" || tz.Id == "Egypt Standard Time")
            ?? TimeZoneInfo.Utc;

        private DateTime NowLocal() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CairoZone);

        // =========================
        // 💰 Discount Logic
        // =========================
        private decimal CalculateDiscountedPrice(Product product)
        {
            if (!product.DiscountPercentage.HasValue ||
                !product.DiscountStart.HasValue ||
                !product.DiscountEnd.HasValue)
                return product.Price;

            var now = NowLocal();

            if (now >= product.DiscountStart.Value &&
                now <= product.DiscountEnd.Value)
            {
                return product.Price -
                       (product.Price * (product.DiscountPercentage.Value / 100m));
            }

            return product.Price;
        }

        private decimal CalculateDiscountedPriceFromSize(Product product, decimal basePrice)
        {
            if (!product.DiscountPercentage.HasValue ||
                !product.DiscountStart.HasValue ||
                !product.DiscountEnd.HasValue)
                return basePrice;

            var now = NowLocal();

            if (now >= product.DiscountStart.Value &&
                now <= product.DiscountEnd.Value)
            {
                return basePrice -
                       (basePrice * (product.DiscountPercentage.Value / 100m));
            }

            return basePrice;
        }

        [HttpPost("modifier/increase")]
        public async Task<IActionResult> IncreaseModifier(int cartItemModifierId)
        {
            var userId = GetUserId();

            var modifier = await _context.CartItemModifiers
                .Include(x => x.CartItem)
                .FirstOrDefaultAsync(x => x.Id == cartItemModifierId);

            if (modifier == null)
                return NotFound();

            // تحقق إن الـ CartItem تابع للمستخدم الحالي
            var isOwner = await _context.Carts
                .AnyAsync(c =>
                    c.UserId == userId &&
                    c.Items.Any(i => i.Id == modifier.CartItemId));

            if (!isOwner)
                return Unauthorized();

            modifier.Quantity++;


            var cart = await _context.Carts
    .Include(c => c.Items)
        .ThenInclude(i => i.Modifiers)
    .FirstOrDefaultAsync(c => c.UserId == userId);

            await RecalculatePromoAsync(cart);
            await _context.SaveChangesAsync();

            var refreshedCart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return Ok(MapCartToDto(refreshedCart, GetLang()));
        }

        [HttpPost("modifier/decrease")]
        public async Task<IActionResult> DecreaseModifier(int cartItemModifierId)
        {
            var userId = GetUserId();

            var modifier = await _context.CartItemModifiers
                .Include(x => x.CartItem)
                .FirstOrDefaultAsync(x => x.Id == cartItemModifierId);

            if (modifier == null)
                return NotFound();

            var isOwner = await _context.Carts
                .AnyAsync(c =>
                    c.UserId == userId &&
                    c.Items.Any(i => i.Id == modifier.CartItemId));

            if (!isOwner)
                return Unauthorized();

            modifier.Quantity--;

            if (modifier.Quantity <= 0)
                _context.CartItemModifiers.Remove(modifier);


            var cart = await _context.Carts
    .Include(c => c.Items)
        .ThenInclude(i => i.Modifiers)
    .FirstOrDefaultAsync(c => c.UserId == userId);


            await RecalculatePromoAsync(cart);
            await _context.SaveChangesAsync();

            var refreshedCart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return Ok(MapCartToDto(refreshedCart, GetLang()));
        }



        [HttpGet("modifiers")]
        public async Task<IActionResult> GetCartItemModifiers()
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                return Ok(new List<object>());

            var result = cart.Items.SelectMany(item => item.Modifiers.Select(m => new
            {
                CartItemModifierId = m.Id,
                CartItemId = item.Id,

                ProductId = item.ProductId,

                ModifierOptionId = m.ModifierOptionId,

                NameAr = m.ModifierOption.NameAr,
                NameEn = m.ModifierOption.NameEn,

                Quantity = m.Quantity,
                Price = m.Price
            }));

            return Ok(result);
        }
        private string? GetUserId() =>
            User.FindFirstValue("userId");



        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var lang = GetLang();
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem>()
                };

                _context.Carts.Add(cart);
            }

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound("Product not found");

            var productSize = await _context.ProductSizes.FindAsync(dto.ProductSizeId);
            if (productSize == null)
                return BadRequest("Product size is required");

          
            var sizePrice = CalculateDiscountedPriceFromSize(product, productSize.Price);

            var sortedMods = dto.Modifiers
      .OrderBy(x => x.ModifierOptionId)
      .Select(x => new
      {
          x.ModifierOptionId,
          x.Quantity
      })
      .ToList();

            var item = cart.Items.FirstOrDefault(x =>
     x.ProductId == dto.ProductId &&
     x.ProductSizeId == dto.ProductSizeId &&
     x.Modifiers
         .OrderBy(m => m.ModifierOptionId)
         .Select(m => new
         {
             m.ModifierOptionId,
             m.Quantity
         })
         .SequenceEqual(sortedMods)
 );
            if (item != null)
            {
                item.Quantity += dto.Quantity;
                item.Price = sizePrice;
                item.Comment = dto.Comment;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = dto.ProductId,
                    ProductSizeId = dto.ProductSizeId,
                    Quantity = dto.Quantity,
                    Price = sizePrice,
                    Comment = dto.Comment,
                    Modifiers = new List<CartItemModifier>()
              
                };
                if (dto.Modifiers.Any())
                {
                    var optionIds = dto.Modifiers
                        .Select(x => x.ModifierOptionId)
                        .ToList();

                    var options = await _context.ModifierOptions
                        .Where(x => optionIds.Contains(x.Id))
                        .ToListAsync();

                    foreach (var modifier in dto.Modifiers)
                    {
                        var option = options.First(x => x.Id == modifier.ModifierOptionId);

                        cartItem.Modifiers.Add(new CartItemModifier
                        {
                            ModifierOptionId = option.Id,
                            Price = option.ExtraPrice,
                            Quantity = modifier.Quantity
                        });
                    }
                }
                cart.Items.Add(cartItem);
            }


            await RecalculatePromoAsync(cart);
            await _context.SaveChangesAsync();

            var refreshedCart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductSize)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return Ok(MapCartToDto(refreshedCart, lang));
        }

        // =========================
        // ✏️ Update Cart Item
        // =========================
        [HttpPost("update")]
        public async Task<IActionResult> UpdateCartItem(UpdateCartItemDto dto)
        {
            var lang = GetLang();
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            var item = cart.Items.FirstOrDefault(i => i.Id == dto.CartItemId);

            if (item == null)
                return NotFound("Cart item not found");

            if (dto.Quantity <= 0)
                cart.Items.Remove(item);
            else
                item.Quantity = dto.Quantity;
            item.Comment = dto.Comment;


            await RecalculatePromoAsync(cart);
            await _context.SaveChangesAsync();

            var refreshedCart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return Ok(MapCartToDto(refreshedCart, lang));
        }

        [HttpPost("remove/{id}")]
        public async Task<IActionResult> RemoveCartItem(int id)
        {
            var lang = GetLang();
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            var item = cart.Items.FirstOrDefault(i => i.Id == id);

            if (item == null)
                return NotFound("Cart item not found");

            _context.CartItemModifiers.RemoveRange(item.Modifiers);
            cart.Items.Remove(item);


            await RecalculatePromoAsync(cart);
            await _context.SaveChangesAsync();

            var refreshedCart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return Ok(MapCartToDto(refreshedCart, lang));
        }
        // =========================
        // 🎁 Apply Promo
        // =========================
        [HttpPost("apply-promo")]
        public async Task<IActionResult> ApplyPromo(ApplyPromoDto dto)
        {
            var userId = GetUserId();
            var now = NowLocal();

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            var subTotal = cart.Items.Sum(i =>
(i.Price + i.Modifiers.Sum(m => m.Price * m.Quantity)) * i.Quantity);

            var promo = await _context.PromoCodes.FirstOrDefaultAsync(p =>
                p.Code == dto.PromoCode &&
                p.IsActive &&
                p.StartDate <= now &&
                p.EndDate >= now);

            if (promo == null)
                return BadRequest("Promo code invalid");

            var discount = subTotal * (promo.DiscountAmount / 100m);

            cart.PromoCode = dto.PromoCode;
            cart.Discount = discount;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                cart.Id,
                cart.UserId,
                PromoCode = cart.PromoCode,
                SubTotal = subTotal,
                Discount = cart.Discount,
                Total = subTotal - cart.Discount
            });
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutDto dto)
        {
            var userId = GetUserId();
            var now = NowLocal();

            bool isRewardOrder = dto.RedeemedRewardId.HasValue;

            Cart? cart = null;
            RedeemedReward? redeemedReward = null;

            // =========================
            // 🎁 REWARD ORDER
            // =========================
            if (isRewardOrder)
            {
                redeemedReward = await _context.RedeemedRewards
                    .Include(r => r.Reward)
                        .ThenInclude(r => r.Product)
                    .FirstOrDefaultAsync(r =>
                        r.Id == dto.RedeemedRewardId &&
                        r.UserId == userId);

                if (redeemedReward == null)
                    return BadRequest("Invalid reward");

                if (redeemedReward.IsUsed)
                    return BadRequest("Reward already used");
            }
            else
            {
                cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Modifiers)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.ProductSize)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                    return BadRequest("Cart is empty");
            }

            // =========================
            // 💰 SUBTOTAL
            // =========================
            decimal subTotal = 0;

            if (!isRewardOrder && cart != null)
            {
                subTotal = cart.Items.Sum(i =>
                   (i.Price + i.Modifiers.Sum(m => m.Price * m.Quantity)) * i.Quantity);
            }

            // =========================
            // 🎟 PROMO CODE
            // =========================


            var promoCode = !string.IsNullOrWhiteSpace(dto.PromoCode)
    ? dto.PromoCode
    : cart?.PromoCode;

            decimal discountAmount = 0;

            if (!string.IsNullOrWhiteSpace(promoCode) && !isRewardOrder)
            {
                var promo = await _context.PromoCodes.FirstOrDefaultAsync(p =>
    p.Code == promoCode &&
    p.IsActive &&
    p.StartDate <= now &&
    p.EndDate >= now);

                if (promo == null)
                    return BadRequest("Promo code is invalid.");

                discountAmount = subTotal * (promo.DiscountAmount / 100m);
            }
            // =========================
            // 🚚 ORDER TYPE SETTINGS
            // =========================
            decimal deliveryFee = 0;

            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings != null)
            {
                if (dto.OrderType == OrderType.Delivery)
                {
                    if (!settings.IsDeliveryEnabled)
                        return BadRequest("Delivery is currently unavailable.");

                    deliveryFee = settings.DeliveryFee;
                }
                else if (dto.OrderType == OrderType.Pickup)
                {
                    if (!settings.IsPickupEnabled)
                        return BadRequest("Pickup is currently unavailable.");
                }
            }

            // =========================
            // 🔥 TOTAL
            // =========================
            decimal totalAmount = isRewardOrder
                ? (dto.OrderType == OrderType.Delivery
                    ? deliveryFee
                    : 0)
                : (subTotal - discountAmount + deliveryFee);

            // =========================
            // ⭐ POINTS
            // =========================
            int pointsEarned = isRewardOrder
                ? 0
                : (int)(totalAmount / 20);




            UserLocation? location = null;

            if (dto.OrderType == OrderType.Delivery)
            {
                if (dto.LocationId == null)
                    return BadRequest("Location is required for delivery");


                location = await _context.UserLocations
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.LocationId &&
                        x.UserId == userId);


                if (location == null)
                    return BadRequest("Invalid location");
            }

            // =========================
            // 📦 ORDER
            // =========================
            var order = new Order
            {
                UserId = userId,

                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                DeliveryFee = deliveryFee,
                TotalAmount = totalAmount,

                OrderStatus = OrderStatus.Pending,
                PaymentStatus = dto.PaymentMethod == PaymentMethod.Online
            ? PaymentStatus.Pending
            : PaymentStatus.Unpaid,

                PaymentMethod = dto.PaymentMethod,
                OrderType = dto.OrderType,
                City = location?.City,
                Street = location?.Street,
                BuildingNumber = location?.BuildingNumber,
                FloorNumber = location?.FloorNumber,
                ApartmentNumber = location?.ApartmentNumber,
                Landmark = location?.Landmark,
                PhoneNumber = location?.PhoneNumber,

                Latitude = location?.Latitude ?? 0,
                Longitude = location?.Longitude ?? 0,
                PointsEarned = pointsEarned,
                PointsRedeemed = dto.PointsRedeemed,

                IsRewardOrder = isRewardOrder,

                OrderItems = new List<OrderItem>()
            };

            // =========================
            // 🛒 CART ITEMS
            // =========================
            if (!isRewardOrder && cart != null)
            {
                order.OrderItems = cart.Items
                    .Select(i => new OrderItem
                    {
                        ProductId = i.ProductId,

                        ProductNameAr = i.Product.NameAr,
                        ProductNameEn = i.Product.NameEn,

                        Comment = i.Comment, 
                        ProductSizeId = i.ProductSizeId,

                        Quantity = i.Quantity,
                        UnitPrice = i.Price +
            i.Modifiers.Sum(m => m.Price * m.Quantity),
                        TotalPrice =
(i.Price + i.Modifiers.Sum(m => m.Price * m.Quantity))
* i.Quantity,

                        Modifiers = i.Modifiers
    .Select(m => new OrderItemModifier
    {
        ModifierOptionId = m.ModifierOptionId,
        Price = m.Price,
        Quantity = m.Quantity
    })
                            .ToList()
                    })
                    .ToList();
            }

            // =========================
            // 🎁 REWARD ITEM
            // =========================
            if (isRewardOrder &&
                redeemedReward?.Reward?.Product != null)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = redeemedReward.Reward.ProductId.Value,
                    ProductNameAr = redeemedReward.Reward.Product.NameAr,
                    ProductNameEn = redeemedReward.Reward.Product.NameEn,
                    Quantity = 1,
                    UnitPrice = 0,
                    TotalPrice = 0
                });
            }

            // =========================
            // 💾 SAVE
            // =========================
            _context.Orders.Add(order);

            if (redeemedReward != null)
            {
                redeemedReward.IsUsed = true;
                redeemedReward.UsedAt = now;
            }

            if (dto.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                if (!isRewardOrder && cart != null)
                {
                    _context.Carts.Remove(cart);
                }
            }

            await _context.SaveChangesAsync();

            // =========================
            // 💳 ONLINE PAYMENT
            // =========================
            if (dto.PaymentMethod == PaymentMethod.Online)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == userId);

                if (user == null)
                    return BadRequest("User not found");

                var paymentResult =
                    await _paymobService.CreatePaymentIntentAsync(
                        new CreatePaymentIntentRequestDto
                        {
                            OrderId = order.Id,
                            Amount = order.TotalAmount,
                            CustomerName = user.FullName,
                            Email = user.Email ?? string.Empty,
                            PhoneNumber = user.PhoneNumber ?? string.Empty
                        });

                if (!paymentResult.Success)
                {
                    order.PaymentStatus = PaymentStatus.Failed;

                    await _context.SaveChangesAsync();

                    return BadRequest(paymentResult.Message);
                }

                return Ok(new
                {
                    orderId = order.Id,
                    totalAmount = order.TotalAmount,
                    payment = paymentResult
                });
            }

            // =========================
            // 💵 CASH RESPONSE
            // =========================
            return Ok(new
            {
                order.Id,
                order.TotalAmount,
                order.SubTotal,
                order.DiscountAmount,
                order.DeliveryFee,
                order.PointsEarned,
                order.IsRewardOrder
            });
        }
        // =========================
        // 🛒 Mapper
        // =========================
        private CartDto MapCartToDto(Cart cart, string lang)
        {

            var subTotalBeforeDiscount = cart.Items.Sum(i =>
    ((i.ProductSize?.Price ?? i.Price) +
     i.Modifiers.Sum(m => m.Price * m.Quantity)) * i.Quantity);

            var subTotalAfterProductDiscount = cart.Items.Sum(i =>
                (i.Price +
                 i.Modifiers.Sum(m => m.Price * m.Quantity)) * i.Quantity);

            var productDiscount =
                subTotalBeforeDiscount - subTotalAfterProductDiscount;

            var promoDiscount = cart.Discount;

            var totalDiscount = productDiscount + promoDiscount;

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,

                Items = cart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,

                    Comment = i.Comment,

                    ProductName = LocalizationExtensions.Localize(
                        i.Product.NameAr,
                        i.Product.NameEn,
                        lang),
                    Price = i.Price,
                    Quantity = i.Quantity,

                    ProductSizeId = i.ProductSizeId,
                    ProductSizeName = i.ProductSize == null
    ? null
    : LocalizationExtensions.Localize(
        i.ProductSize.NameAr,
        i.ProductSize.NameEn,
        lang),

                    SizePrice = i.ProductSize?.Price ?? 0,


                    Modifiers = i.Modifiers.Select(m => new CartItemModifierDto
                    {
                        Id = m.Id,
                        ModifierOptionNameAr = m.ModifierOption.NameAr,
                        ModifierOptionNameEn = m.ModifierOption.NameEn,
                        ModifierOptionId = m.ModifierOptionId,
                        Price = m.Price * m.Quantity , 
                        Quantity = m.Quantity
                    }).ToList()
                }).ToList(),

                // قبل أي خصومات
                SubTotal = subTotalBeforeDiscount,

                // خصم المنتجات
                ProductDiscount = productDiscount,

                // خصم البروموكود
                PromoDiscount = promoDiscount,

                // إجمالي الخصومات
                Discount = totalDiscount,

                // النهائي بعد كل الخصومات
                Total = subTotalAfterProductDiscount - promoDiscount,

                PromoCode = cart.PromoCode
            };
        }

        // =========================
        // 🛒 Get My Cart
        // =========================
        [HttpGet("mycart")]
        public async Task<IActionResult> GetMyCart()
        {
            var lang = GetLang();
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .Include(c => c.Items)
    .ThenInclude(i => i.ProductSize)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            

            if (cart == null)
                return Ok(new CartResponseDto
                {
                    UserId = userId,
                    Items = new List<CartItemDto>(),
                    SubTotal = 0,
                    ProductDiscount = 0,
                    PromoDiscount = 0,
                    Discount = 0,
                    Total = 0,
                    PromoCode = null
                });

            return Ok(MapCartToDto(cart, lang));
        }

        // =========================
        // 🧹 Clear Cart
        // =========================
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Modifiers)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cart cleared successfully" });
        }
    }
}
