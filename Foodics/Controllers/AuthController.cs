using Foodics.Dtos.Auth;
using Foodics.Models;
using Foodics.Services;
using Foodics.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing.Imaging;
using System.Net;
using AppUser = Foodics.Models.User;

namespace Foodics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService; 



        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            JwtService jwtService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService; 

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

            var user = new  AppUser
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

            // 🔥 Generate Access Token
            var accessToken = await _jwtService.GenerateAccessToken(user);

            // 🔥 Generate Refresh Token
            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(5); // ✅ 5 days

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                accessToken = accessToken,
                refreshToken = refreshToken,

                user = new
                {
                    id = user.Id,
                    phoneNumber = user.PhoneNumber,
                    name = user.FullName
                }
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenRequestDto dto)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (user == null)
                return Unauthorized("Invalid refresh token");

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized("Refresh token expired");


            var newAccessToken = await _jwtService.GenerateAccessToken(user);

            var newRefreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(5);

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("User not found");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebUtility.UrlEncode(token);

            var resetLink = $"https://yourfrontend.com/reset-password?email={model.Email}&token={encodedToken}";

//var resetLink = $"myapp://reset-password?email={{model.Email}}&token={{Uri.EscapeDataString(token)\r\n";


            // ابعت الإيميل هنا
            await _emailService.SendEmailAsync(model.Email, "Reset Password",
                $"Click here to reset your password: {resetLink}");

            return Ok("Password reset link sent to your email");
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("User not found.");

            // ✅ decode الـ token الأول قبل ما تبعته لـ Identity
            var decodedToken = WebUtility.UrlDecode(model.Token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password reset successfully.");
        }
    }
}



