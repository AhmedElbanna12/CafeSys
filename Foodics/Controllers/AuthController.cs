//using Foodics.Dtos.Auth;
//using Foodics.Models;
//using Foodics.Services;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using POSSystem.Data;
//using QRCoder;
//using System.Drawing.Imaging;

//namespace Foodics.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AuthController : ControllerBase
//    {
//        private readonly UserManager<User> _userManager;
//        private readonly SignInManager<User> _signInManager;
//        private readonly JwtService _jwtService;
//        private readonly ApplicationDbContext _context;
//        private readonly SmsService _smsService;

//        public AuthController(
//            UserManager<User> userManager,
//            SignInManager<User> signInManager,
//            JwtService jwtService,
//            ApplicationDbContext context,
//            SmsService smsService)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _jwtService = jwtService;
//            _context = context;
//            _smsService = smsService;
//        }

//        // =========================
//        // 1. Send OTP (Register)
//        // =========================
//        [HttpPost("send-otp")]
//        public async Task<IActionResult> SendOtp(string phoneNumber)
//        {
//            var exists = await _userManager.Users
//                .AnyAsync(u => u.PhoneNumber == phoneNumber);

//            if (exists)
//                return BadRequest("Phone already registered");

//            var otp = new Random().Next(100000, 999999).ToString();

//            var otpEntity = new OtpCode
//            {
//                PhoneNumber = phoneNumber,
//                Code = otp,
//                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
//                IsUsed = false
//            };

//            _context.OtpCode.Add(otpEntity);
//            await _context.SaveChangesAsync();

//            await _smsService.SendSms(phoneNumber, $"Your OTP is: {otp}");

//            return Ok("OTP sent");
//        }

//        // =========================
//        // 2. Register with OTP
//        // =========================
//        [HttpPost("register")]
//        public async Task<IActionResult> Register(RegisterDto model)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var otpRecord = await _context.OtpCode
//                .Where(o => o.PhoneNumber == model.PhoneNumber && o.Code == model.Otp)
//                .OrderByDescending(o => o.ExpiryTime)
//                .FirstOrDefaultAsync();

//            if (otpRecord == null || otpRecord.IsUsed || otpRecord.ExpiryTime < DateTime.UtcNow)
//                return BadRequest("Invalid or expired OTP");

//            otpRecord.IsUsed = true;

//            var existingPhone = await _userManager.Users
//                .AnyAsync(u => u.PhoneNumber == model.PhoneNumber);
//            if (existingPhone)
//                return BadRequest("Phone number already exists");

//            var existingUser = await _userManager.FindByEmailAsync(model.Email);
//            if (existingUser != null)
//                return BadRequest("Email already exists");

//            var customerCode = Guid.NewGuid().ToString("N").ToUpper();

//            var user = new User
//            {
//                FullName = model.FullName,
//                UserName = model.Email,
//                Email = model.Email,
//                PhoneNumber = model.PhoneNumber,
//                CustomerCode = customerCode
//            };

//            var result = await _userManager.CreateAsync(user, model.Password);
//            if (!result.Succeeded)
//                return BadRequest(result.Errors);

//            await _context.SaveChangesAsync();

//            // QR Code
//            var qrGenerator = new QRCodeGenerator();
//            var qrData = qrGenerator.CreateQrCode(customerCode, QRCodeGenerator.ECCLevel.Q);
//            var qrCode = new QRCode(qrData);

//            using var qrBitmap = qrCode.GetGraphic(20);
//            using var ms = new MemoryStream();
//            qrBitmap.Save(ms, ImageFormat.Png);

//            var qrBase64 = Convert.ToBase64String(ms.ToArray());

//            return Ok(new
//            {
//                message = "User Created Successfully",
//                customerCode,
//                qrCodeBase64 = qrBase64
//            });
//        }

//        // =========================
//        // 3. Login
//        // =========================
//        [HttpPost("login")]
//        public async Task<IActionResult> Login(LoginDto model)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var user = await _userManager.Users
//                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

//            if (user == null)
//                return Unauthorized("Invalid Phone Number or Password");

//            var result = await _signInManager
//                .CheckPasswordSignInAsync(user, model.Password, false);

//            if (!result.Succeeded)
//                return Unauthorized("Invalid Phone Number or Password");

//            var token = await _jwtService.GenerateToken(user);

//            return Ok(new
//            {
//                token,
//                phoneNumber = user.PhoneNumber,
//                name = user.FullName
//            });
//        }

//        // =========================
//        // 4. Forgot Password (Send OTP)
//        // =========================
//        [HttpPost("forgot-password")]
//        public async Task<IActionResult> ForgotPassword(string phoneNumber)
//        {
//            var user = await _userManager.Users
//                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

//            if (user == null)
//                return NotFound("User not found");

//            var otp = new Random().Next(100000, 999999).ToString();

//            var otpEntity = new OtpCode
//            {
//                PhoneNumber = phoneNumber,
//                Code = otp,
//                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
//                IsUsed = false
//            };

//            _context.OtpCode.Add(otpEntity);
//            await _context.SaveChangesAsync();

//            await _smsService.SendSms(phoneNumber, $"Reset OTP: {otp}");

//            return Ok("OTP sent for password reset");
//        }

//        // =========================
//        // 5. Reset Password
//        // =========================
//        [HttpPost("reset-password")]
//        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
//        {
//            var otpRecord = await _context.OtpCode
//                .Where(o => o.PhoneNumber == model.PhoneNumber && o.Code == model.Otp)
//                .OrderByDescending(o => o.ExpiryTime)
//                .FirstOrDefaultAsync();

//            if (otpRecord == null || otpRecord.IsUsed || otpRecord.ExpiryTime < DateTime.UtcNow)
//                return BadRequest("Invalid or expired OTP");

//            var user = await _userManager.Users
//                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

//            if (user == null)
//                return NotFound("User not found");

//            otpRecord.IsUsed = true;

//            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

//            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

//            if (!result.Succeeded)
//                return BadRequest(result.Errors);

//            await _context.SaveChangesAsync();

//            return Ok("Password reset successfully");
//        }
//    }
//}


using Foodics.Dtos.Auth;
using Foodics.Models;
using Foodics.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing.Imaging;

namespace Foodics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtService _jwtService;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            JwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        // Register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingPhone = await _userManager.Users
                .AnyAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (existingPhone)
                return BadRequest("Phone number already exists");

            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
                return BadRequest("Email already exists");

            // إنشاء CustomerCode فريد
            var customerCode = Guid.NewGuid().ToString("N").ToUpper();

            var user = new User
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                CustomerCode = customerCode
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // إنشاء QR Code يحتوي على CustomerCode
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(customerCode, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrData);

            using var qrBitmap = qrCode.GetGraphic(20);
            using var ms = new MemoryStream();

            qrBitmap.Save(ms, ImageFormat.Png);

            var qrBase64 = Convert.ToBase64String(ms.ToArray());

            // إرجاع البيانات
            return Ok(new
            {
                message = "User Created Successfully",
                customerCode = customerCode,
                qrCodeBase64 = qrBase64
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (user == null)
                return Unauthorized("Invalid Phone Number or Password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Invalid Phone Number or Password");

            var token = await _jwtService.GenerateToken(user);

            return Ok(new
            {
                token = token,
                phoneNumber = user.PhoneNumber,
                name = user.FullName
            });
        }
    }
}



