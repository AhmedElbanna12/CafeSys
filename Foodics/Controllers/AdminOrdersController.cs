using Foodics.Dtos.Admin.Orders;
using Foodics.ExtensionMethod;
using Foodics.Models;
using Foodics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using System.Net.NetworkInformation;

namespace Foodics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Cashier,Admin")] // فقط الكاشير أو الأدمن
    public class AdminOrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly OfflineOrderService _offlineService;
        public AdminOrdersController(ApplicationDbContext context, UserManager<User> userManager, OfflineOrderService offlineService)
        {
            _context = context;
            _userManager = userManager;
            _offlineService = offlineService;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var offlineService = new OfflineOrderService();

            if (!IsInternetAvailable())
            {
                // حفظ Offline
                var offlineOrder = new OfflineOrderDto
                {
                    CustomerCode = dto.CustomerCode,
                    POSDeviceId = dto.POSDeviceId,
                    Items = dto.Items,
                    CreatedAt = DateTime.UtcNow
                };
                offlineService.SaveOfflineOrder(offlineOrder);
                return Ok(new { message = "Order saved offline, will sync when internet is back" });
            }

            try
            {
                var result = await SaveOrderToDb(dto); // تستخدم Private Method
                return Ok(result);
            }
            catch (Exception ex)
            {
                // سجل الخطأ في Log
                return StatusCode(500, new { message = "Error creating order", detail = ex.Message });
            }
        }

        [HttpPost("sync-offline-orders")]
        public async Task<IActionResult> SyncOfflineOrders()
        {
            var offlineOrders = _offlineService.GetOfflineOrders();
            var successList = new List<int>();
            var failedList = new List<string>();

            foreach (var offlineOrder in offlineOrders)
            {
                var createDto = new CreateOrderDto
                {
                    CustomerCode = offlineOrder.CustomerCode,
                    POSDeviceId = offlineOrder.POSDeviceId,
                    Items = offlineOrder.Items
                };

                var (success, orderId, errorMessage) = await CreateOrderInternal(createDto);

                if (success) successList.Add(orderId.Value);
                else failedList.Add($"{offlineOrder.CustomerCode}: {errorMessage}");
            }

            _offlineService.DeleteOfflineOrders(successList); // احذف الأوردرات الناجحة
            return Ok(new { message = "Offline orders sync completed", syncedOrders = successList.Count, failedOrders = failedList });
        }

        [HttpGet("get-orders")]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Modifiers)
                        .ThenInclude(m => m.ModifierOption)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var response = orders.Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                CustomerName = o.User.FullName,
                CustomerPhone = o.User.PhoneNumber,
                TotalAmount = o.TotalAmount,
                PointsEarned = o.OrderItems.Sum(oi => oi.Product.PointsReward * oi.Quantity),
                CreatedAt = o.CreatedAt,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    Modifiers = oi.Modifiers.Select(m => m.ModifierOption.Name).ToList()
                }).ToList()
            }).ToList();

            return Ok(response);
        }



        private async Task<bool> DeductIngredients(int productId, int quantity, int orderId)
        {
            var productIngredients = await _context.ProductIngredients
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            foreach (var pi in productIngredients)
            {
                var ingredient = await _context.Ingredients.FirstOrDefaultAsync(i => i.Id == pi.IngredientId);
                if (ingredient == null) continue;

                var requiredQty = pi.Quantity * quantity;

                if (ingredient.Quantity < requiredQty)
                    return false;

                ingredient.Quantity -= requiredQty;

                _context.StockMovements.Add(new StockMovement
                {
                    IngredientId = ingredient.Id,
                    Quantity = -requiredQty,
                    Type = "Order",
                    ReferenceId = orderId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return true;
        }






        // ===== Private Method مشتركة للحفظ =====
        private async Task<dynamic> SaveOrderToDb(CreateOrderDto dto, int? totalPoints = null, decimal? totalAmount = null)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.CustomerCode == dto.CustomerCode);

            if (user == null)
                throw new Exception("Customer not found");

            decimal orderTotalAmount = totalAmount ?? 0;
            int orderTotalPoints = totalPoints ?? 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var product = await _context.Products
                    .Include(p => p.ModifierGroups).ThenInclude(g => g.Options)
                    .Include(p => p.Sizes)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null) continue;

                var discountAmount = product.DiscountPercentage.HasValue
                    ? product.Price * (product.DiscountPercentage.Value / 100)
                    : 0;

                var unitPrice = product.Price - discountAmount;
                var totalPrice = unitPrice * item.Quantity;
                if (totalPoints == null) orderTotalPoints += product.PointsReward * item.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductSizeId = item.ProductSizeId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    DiscountAmount = discountAmount,
                    TotalPrice = totalPrice,
                    Modifiers = new List<OrderItemModifier>()
                };

                if (item.ModifierOptionIds != null)
                {
                    foreach (var modId in item.ModifierOptionIds)
                    {
                        var modOption = product.ModifierGroups.SelectMany(g => g.Options)
                                            .FirstOrDefault(m => m.Id == modId);
                        if (modOption != null)
                        {
                            orderItem.Modifiers.Add(new OrderItemModifier { ModifierOptionId = modOption.Id });
                            totalPrice += modOption.ExtraPrice * item.Quantity;
                            orderItem.TotalPrice = totalPrice;
                        }
                    }
                }

                if (totalAmount == null) orderTotalAmount += totalPrice;
                orderItems.Add(orderItem);
            }

            var order = new Order
            {
                UserId = user.Id,
                POSDeviceId = dto.POSDeviceId,
                BranchId = 1,
                Status = "Completed",
                PaymentStatus = "Paid",
                CreatedAt = DateTime.UtcNow,
                TotalAmount = orderTotalAmount,
                OrderItems = orderItems,
                Payment = new Payment
                {
                    Amount = orderTotalAmount,
                    Method = "Cash",
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Orders.Add(order);

            var pointsTransaction = new PointsTransaction
            {
                UserId = user.Id,
                Order = order,
                Points = orderTotalPoints,
                Type = "Earn",
                WeekStartDate = DateTime.UtcNow.StartOfWeek(),
                CreatedAt = DateTime.UtcNow
            };
            _context.PointsTransactions.Add(pointsTransaction);

            var userPoints = await _context.UserPoints.FirstOrDefaultAsync(u => u.UserId == user.Id);
            if (userPoints == null)
                _context.UserPoints.Add(new UserPoints { UserId = user.Id, TotalPoints = orderTotalPoints, UsedPoints = 0 });
            else
                userPoints.TotalPoints += orderTotalPoints;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Order created successfully", orderId = order.Id, totalAmount, pointsEarned = totalPoints });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating order", detail = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        private bool IsInternetAvailable()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send("8.8.8.8", 1000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch { return false; }
        }



        private async Task<(bool success, int? orderId, string errorMessage)> CreateOrderInternal(CreateOrderDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.CustomerCode == dto.CustomerCode);
            if (user == null) return (false, null, "Customer not found");

            decimal totalAmount = 0;
            int totalPointsEarned = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var product = await _context.Products
                    .Include(p => p.ModifierGroups).ThenInclude(g => g.Options)
                    .Include(p => p.Sizes)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product == null) continue;

                var discountAmount = product.DiscountPercentage.HasValue ? product.Price * (product.DiscountPercentage.Value / 100) : 0;
                var unitPrice = product.Price - discountAmount;
                var totalPrice = unitPrice * item.Quantity;
                totalPointsEarned += product.PointsReward * item.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductSizeId = item.ProductSizeId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    DiscountAmount = discountAmount,
                    TotalPrice = totalPrice,
                    Modifiers = new List<OrderItemModifier>()
                };

                if (item.ModifierOptionIds != null)
                {
                    foreach (var modId in item.ModifierOptionIds)
                    {
                        var modOption = product.ModifierGroups.SelectMany(g => g.Options).FirstOrDefault(m => m.Id == modId);
                        if (modOption != null)
                        {
                            orderItem.Modifiers.Add(new OrderItemModifier { ModifierOptionId = modOption.Id });
                            totalPrice += modOption.ExtraPrice * item.Quantity;
                            orderItem.TotalPrice = totalPrice;
                        }
                    }
                }

                totalAmount += totalPrice;
                orderItems.Add(orderItem);
            }

            var order = new Order
            {
                UserId = user.Id,
                POSDeviceId = dto.POSDeviceId,
                BranchId = 1,
                Status = "Completed",
                PaymentStatus = "Paid",
                CreatedAt = DateTime.UtcNow,
                TotalAmount = totalAmount,
                OrderItems = orderItems,
                Payment = new Payment
                {
                    Amount = totalAmount,
                    Method = "Cash",
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow
                }
            };

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in orderItems)
                {
                    var success = await DeductIngredients(item.ProductId, item.Quantity, order.Id);
                    if (!success)
                        return (false, null, "Not enough ingredients in stock");
                }

                var pointsTransaction = new PointsTransaction
                {
                    UserId = user.Id,
                    Order = order,
                    Points = totalPointsEarned,
                    Type = "Earn",
                    WeekStartDate = DateTime.UtcNow.StartOfWeek(),
                    CreatedAt = DateTime.UtcNow
                };
                _context.PointsTransactions.Add(pointsTransaction);

                var userPoints = await _context.UserPoints.FirstOrDefaultAsync(u => u.UserId == user.Id);
                if (userPoints == null)
                    _context.UserPoints.Add(new UserPoints { UserId = user.Id, TotalPoints = totalPointsEarned, UsedPoints = 0 });
                else
                    userPoints.TotalPoints += totalPointsEarned;




                await _context.SaveChangesAsync();
                return (true, order.Id, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}