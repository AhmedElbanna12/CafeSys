using Foodics.Dtos.Paymob;

namespace Foodics.Services.Interfaces
{
    public interface IPaymobService
    {
        Task<CreatePaymentIntentResponseDto> CreatePaymentIntentAsync(
             CreatePaymentIntentRequestDto request);

        Task<bool> VerifyWebhookAsync(
            PaymobWebhookDto request);

        Task UpdatePaymentStatusAsync(
            int orderId,
            bool isPaid,
            string? transactionId);
    }
}
