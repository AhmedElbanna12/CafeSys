using Foodics.Dtos.Paymob;
using Foodics.Helpers; // <-- أضف الـ using
using Foodics.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foodics.Controllers
{
    [Route("api/paymob")]
    [ApiController]
    public class PaymobController : ControllerBase
    {
        private readonly IPaymobService _paymobService;
        private readonly ILogger<PaymobController> _logger;
        private readonly InMemoryLogStore _logStore; // <-- أضف ده

        public PaymobController(
            IPaymobService paymobService,
            ILogger<PaymobController> logger,
            InMemoryLogStore logStore) // <-- حقن الخدمة
        {
            _paymobService = paymobService;
            _logger = logger;
            _logStore = logStore;
        }

        // ✅ Endpoint مخصص لعرض الـ Logs في المتصفح
        [AllowAnonymous]
        [HttpGet("logs")]
        public IActionResult GetLogs()
        {
            var logs = _logStore.GetLogs();
            return Ok(string.Join("\n", logs));
        }

        // ✅ Endpoint لمسح الـ Logs (اختياري)
        [AllowAnonymous]
        [HttpDelete("logs")]
        public IActionResult ClearLogs()
        {
            _logStore.ClearLogs();
            return Ok("Logs cleared.");
        }

        [AllowAnonymous]
        [EnableCors("AllowPaymob")]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromQuery] string? hmac)
        {
            _logStore.AddLog("========== WEBHOOK RECEIVED ==========");

            Request.EnableBuffering();

            string body;
            using (var reader = new StreamReader(Request.Body, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
            }

            _logStore.AddLog($"Raw Body: {body}");
            _logStore.AddLog($"HMAC from Query: {hmac ?? "NULL"}");

            if (string.IsNullOrEmpty(body))
            {
                _logStore.AddLog("❌ Empty body received");
                return BadRequest("Empty body");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
            };

            PaymobWebhookRequestDto? request;
            try
            {
                request = JsonSerializer.Deserialize<PaymobWebhookRequestDto>(body, options);
                _logStore.AddLog("✅ Deserialization successful");
            }
            catch (Exception ex)
            {
                _logStore.AddLog($"❌ Deserialization failed: {ex.Message}");
                return BadRequest($"Deserialization failed: {ex.Message}");
            }

            // تجاهل Card Token Callback
            if (request?.obj?.order == null)
            {
                _logStore.AddLog("ℹ️ Ignoring non-transaction callback (card token)");
                return Ok();
            }

            var transaction = request.obj;
            _logStore.AddLog($"📦 Transaction ID: {transaction.id}");
            _logStore.AddLog($"📦 Merchant Order ID: {transaction.order.merchant_order_id ?? "NULL"}");
            _logStore.AddLog($"📦 Success: {transaction.success}");

            var hmacValue = !string.IsNullOrEmpty(hmac) ? hmac : request?.hmac;
            _logStore.AddLog($"🔐 HMAC used for verification: {hmacValue ?? "NULL"}");

            if (string.IsNullOrEmpty(hmacValue))
            {
                _logStore.AddLog("❌ Missing HMAC");
                return Unauthorized("Missing HMAC");
            }

            // ✅ سجل البيانات اللي هتتحسب قبل الـ HMAC
            _logStore.AddLog($"🔢 Amount: {transaction.amount_cents}");
            _logStore.AddLog($"🔢 Created At: {transaction.created_at}");
            _logStore.AddLog($"🔢 Currency: {transaction.currency}");
            _logStore.AddLog($"🔢 Order ID: {transaction.order.id}");
            _logStore.AddLog($"🔢 Owner: {transaction.owner}");
            _logStore.AddLog($"🔢 PAN: {transaction.source_data.pan}");
            _logStore.AddLog($"🔢 Sub Type: {transaction.source_data.sub_type}");
            _logStore.AddLog($"🔢 Type: {transaction.source_data.type}");

            var verified = await _paymobService.VerifyWebhookAsync(transaction, hmacValue);
            _logStore.AddLog($"🔐 HMAC Verified: {verified}");

            if (!verified)
            {
                _logStore.AddLog("❌ HMAC verification FAILED");
                return Unauthorized("HMAC verification failed");
            }

            var orderId = transaction.order.merchant_order_id ?? transaction.order.id.ToString();
            _logStore.AddLog($"🔍 Searching for order with ID: {orderId}");

            await _paymobService.UpdatePaymentStatusAsync(orderId, transaction.success, transaction.id.ToString());
            _logStore.AddLog("✅ Payment status updated successfully");
            _logStore.AddLog("========== WEBHOOK COMPLETED ==========");

            return Ok();
        }
    }
}