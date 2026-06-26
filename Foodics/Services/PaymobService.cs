using Foodics.Dtos.Paymob;
using Foodics.Helpers;
using Foodics.Models;
using Foodics.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using POSSystem.Data;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Foodics.Services
{
    public class PaymobService : IPaymobService
    {
        private readonly ApplicationDbContext _context;
        private readonly PaymobOptions _options;
        private readonly HttpClient _httpClient;

        public PaymobService(
            ApplicationDbContext context,
            IOptions<PaymobOptions> options,
            HttpClient httpClient)
        {
            _context = context;
            _options = options.Value;
            _httpClient = httpClient;
        }

        public async Task<CreatePaymentIntentResponseDto> CreatePaymentIntentAsync(
      CreatePaymentIntentRequestDto request)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
            {
                return new CreatePaymentIntentResponseDto
                {
                    Success = false,
                    Message = "Order not found."
                };
            }

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                return new CreatePaymentIntentResponseDto
                {
                    Success = false,
                    Message = "Order already paid."
                };
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == order.UserId);

            if (user == null)
            {
                return new CreatePaymentIntentResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // ==========================
            // Build Items
            // ==========================

            var items = new List<PaymobItem>();

            // المنتجات
            foreach (var item in order.OrderItems)
            {
                items.Add(new PaymobItem
                {
                    name = item.ProductNameEn,
                    description = item.ProductNameEn,
                    quantity = item.Quantity,

                    // سعر الوحدة فقط
                    amount = (int)(item.UnitPrice * 100)
                });
            }

            // الدليفري
            if (order.DeliveryFee > 0)
            {
                items.Add(new PaymobItem
                {
                    name = "Delivery",
                    description = "Delivery Fee",
                    quantity = 1,
                    amount = (int)(order.DeliveryFee * 100)
                });
            }

            // الخصم (Promo Code)
            if (order.DiscountAmount > 0)
            {
                items.Add(new PaymobItem
                {
                    name = "Discount",
                    description = "Promo Code",
                    quantity = 1,

                    // خصم بالسالب
                    amount = -(int)(order.DiscountAmount * 100)
                });
            }

            // لو Order Reward يبقى المنتج مجاني
            if (order.IsRewardOrder)
            {
                items.Clear();

                items.Add(new PaymobItem
                {
                    name = order.OrderItems.First().ProductNameEn,
                    description = "Reward",
                    quantity = 1,
                    amount = 0
                });

                if (order.DeliveryFee > 0)
                {
                    items.Add(new PaymobItem
                    {
                        name = "Delivery",
                        description = "Delivery Fee",
                        quantity = 1,
                        amount = (int)(order.DeliveryFee * 100)
                    });
                }
            }

            foreach (var i in order.OrderItems)
            {
                Console.WriteLine(
                    $"Name={i.ProductNameEn} | Qty={i.Quantity} | Unit={i.UnitPrice} | Total={i.TotalPrice}");
            }

            Console.WriteLine($"SubTotal = {order.SubTotal}");
            Console.WriteLine($"Discount = {order.DiscountAmount}");
            Console.WriteLine($"Delivery = {order.DeliveryFee}");
            Console.WriteLine($"OrderTotal = {order.TotalAmount}");
            Console.WriteLine($"ItemsTotal = {items.Sum(x => x.amount * x.quantity) / 100m}");

            // ==========================
            // Safety Check
            // ==========================

            var totalItemsAmount = items.Sum(x => x.amount * x.quantity);

            if (totalItemsAmount != (int)(order.TotalAmount * 100))
            {
                return new CreatePaymentIntentResponseDto
                {
                    Success = false,
                    Message =
                        $"Items Total ({totalItemsAmount}) != Order Total ({(int)(order.TotalAmount * 100)})"
                };
            }

            var body = new CreateIntentionRequest
            {
                amount = (int)(order.TotalAmount * 100),

                currency = "EGP",

                payment_methods = new List<int>
        {
            _options.IntegrationId
        },

                special_reference = order.Id.ToString(),

                expiration = 3600,

                notification_url = _options.WebhookUrl,

                redirection_url = _options.RedirectionUrl,

                billing_data = new BillingData
                {
                    first_name = string.IsNullOrWhiteSpace(user.FullName)
                        ? "Customer"
                        : user.FullName,

                    last_name = "Customer",

                    email = user.Email ?? "",

                    phone_number = user.PhoneNumber ?? "",

                    apartment = "NA",
                    building = "NA",
                    floor = "NA",
                    street = "NA",
                    city = "Cairo",
                    state = "Cairo",
                    country = "EG"
                },

                items = items,

                extras = new Dictionary<string, object>
        {
            { "OrderId", order.Id }
        }
            };

            var json = JsonSerializer.Serialize(body);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Token",
                    _options.SecretKey);

            var response = await _httpClient.PostAsync(
                $"{_options.BaseUrl}/v1/intention/",
                content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new CreatePaymentIntentResponseDto
                {
                    Success = false,
                    Message = responseBody
                };
            }

            var result = JsonSerializer.Deserialize<CreateIntentionResponse>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null)
            {
                return new CreatePaymentIntentResponseDto
                {
                    Success = false,
                    Message = "Invalid Paymob response."
                };
            }

            // ==========================
            // Update Order
            // ==========================

            order.PaymobOrderId = result.intention_order_id.ToString();
            order.ClientSecret = result.client_secret;
            order.PaymentStatus = PaymentStatus.Pending;

            // ==========================
            // Create / Update Payment
            // ==========================

            var payment = order.Payment;

            if (payment == null)
            {
                payment = new Payment
                {
                    OrderId = order.Id,
                    CreatedAt = TimeHelper.NowCairo()
                };

                _context.Payments.Add(payment);
            }

            payment.Amount = order.TotalAmount;
            payment.Method = PaymentMethod.Online;
            payment.Status = PaymentStatus.Pending;
            payment.PaymobOrderId = result.intention_order_id.ToString();
            payment.ClientSecret = result.client_secret;

            order.Payment = payment;

            await _context.SaveChangesAsync();

            return new CreatePaymentIntentResponseDto
            {
                Success = true,
                ClientSecret = result.client_secret,
                PaymobOrderId = result.intention_order_id.ToString(),

                CheckoutUrl =
                    $"https://accept.paymob.com/unifiedcheckout/?publicKey={_options.PublicKey}&clientSecret={result.client_secret}"
            };
        }


        private string CalculateHmac(PaymobWebhookDto request)
        {
            var raw =
                $"{request.amount_cents}" +
                $"{request.created_at}" +
                $"{request.currency}" +
                $"{request.error_occured}" +
                $"{request.has_parent_transaction}" +
                $"{request.id}" +
                $"{request.integration_id}" +
                $"{request.is_3d_secure}" +
                $"{request.is_auth}" +
                $"{request.is_capture}" +
                $"{request.is_refunded}" +
                $"{request.is_standalone_payment}" +
                $"{request.is_voided}" +
                $"{request.order.id}" +
                $"{request.owner}" +
                $"{request.pending}" +
                $"{request.source_data.pan}" +
                $"{request.source_data.sub_type}" +
                $"{request.source_data.type}" +
                $"{request.success}";

            using var hmac = new HMACSHA512(
                Encoding.UTF8.GetBytes(_options.HmacSecret));

            var hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(raw));

            return Convert.ToHexString(hash)
                .ToLowerInvariant();
        }

       
            public Task<bool> VerifyWebhookAsync(PaymobWebhookDto request)
        {
            var calculated = CalculateHmac(request);

            return Task.FromResult(
                calculated.Equals(
                    request.hmac,
                    StringComparison.OrdinalIgnoreCase));
        }
        public async Task UpdatePaymentStatusAsync(
     int paymobOrderId,
     bool isPaid,
     string? transactionId)
        {
            var order = await _context.Orders
                .Include(x => x.Payment)
                .FirstOrDefaultAsync(x =>
                    x.PaymobOrderId == paymobOrderId.ToString());

            if (order == null)
                return;

            // لو اتدفع بالفعل متعملش Update تاني
            if (order.PaymentStatus == PaymentStatus.Paid)
                return;

            order.PaymentStatus =
                isPaid
                    ? PaymentStatus.Paid
                    : PaymentStatus.Failed;

            order.PaymobTransactionId = transactionId;
            order.PaymentDate = DateTime.UtcNow;

            if (order.Payment != null)
            {
                order.Payment.Status =
                    isPaid
                        ? PaymentStatus.Paid
                        : PaymentStatus.Failed;

                order.Payment.TransactionId = transactionId;

                order.Payment.PaidAt =
                    isPaid
                        ? DateTime.UtcNow
                        : null;
            }

            await _context.SaveChangesAsync();
        }
    }
}
