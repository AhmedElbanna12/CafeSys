using Foodics.Dtos.Paymob;
using Foodics.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foodics.Controllers
{
    [Route("api/paymob")]
    [ApiController]
    public class PaymobController : ControllerBase
    {
        private readonly IPaymobService _paymobService;

        public PaymobController(IPaymobService paymobService)
        {
            _paymobService = paymobService;
        }



        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook(
            [FromBody] PaymobWebhookDto request)
        {
            var verified =
                await _paymobService.VerifyWebhookAsync(request);

            if (!verified)
                return Unauthorized();

            await _paymobService.UpdatePaymentStatusAsync(
               (int)request.order.id,
                request.success,
                request.id.ToString());

            return Ok();
        }


    }
}
