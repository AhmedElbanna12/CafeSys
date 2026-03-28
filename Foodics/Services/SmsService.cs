//using Foodics.Dtos.Auth;
//using Microsoft.Extensions.Options;
//using Twilio;
//using Twilio.Rest.Api.V2010.Account;

//namespace Foodics.Services
//{
//    public class SmsService
//    {
//        private readonly TwilioSettings _settings;

//        public SmsService(IOptions<TwilioSettings> settings)
//        {
//            _settings = settings.Value;
//        }

//        public async Task SendSms(string to, string message)
//        {
//            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
//            Console.WriteLine($"Twilio SID: {_settings.AccountSid}, Token: {_settings.AuthToken}, From: {_settings.FromNumber}");
//            await MessageResource.CreateAsync(
//                body: message,
//                from: new Twilio.Types.PhoneNumber(_settings.FromNumber),
//                to: new Twilio.Types.PhoneNumber(to)
//            );
//        }
//    }
//}
