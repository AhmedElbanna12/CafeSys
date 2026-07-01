using Foodics.Dtos.Paymob;

namespace Foodics.Services.Interfaces
{
    public interface IPaymobService
    {

        Task<CreatePaymentIntentResponseDto> CreatePaymentIntentAsync(
            CreatePaymentIntentRequestDto request);

        Task<bool> VerifyWebhookAsync(
            PaymobWebhookDto request,
            string hmacFromQuery);

        Task UpdatePaymentStatusAsync(
            string merchantOrderId,
            bool isPaid,
            string? transactionId);
    }
}
