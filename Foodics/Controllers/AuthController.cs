using Foodics.Dtos.Auth;
using Foodics.Models;
using Foodics.Services;
using Foodics.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using QRCoder;
using System.Drawing.Imaging;
using System.Net;
using System.Security.Claims;
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
        private readonly ApplicationDbContext _context;



        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            JwtService jwtService,
            IEmailService emailService ,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _context = context;

        }


        // Register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto model)
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

            // رفع صورة البروفايل
            string? imageUrl = null;

            if (model.ProfileImage != null)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "ProfileImages");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }

                imageUrl = $"{Request.Scheme}://{Request.Host}/ProfileImages/{fileName}";
            }

            // إنشاء CustomerCode فريد
            var customerCode = Guid.NewGuid().ToString("N").ToUpper();

            var user = new AppUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                CustomerCode = customerCode,
                ProfileImageUrl = imageUrl
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Generate OTP
            var otp = Random.Shared.Next(100000, 999999).ToString();

            // Delete any previous OTP
            var oldOtps = _context.EmailOtp.Where(x => x.Email == user.Email);
            _context.EmailOtp.RemoveRange(oldOtps);

            // Save new OTP
            _context.EmailOtp.Add(new EmailOtp
            {
                Email = user.Email,
                Code = otp,
                ExpireAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            });

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                user.Email,
                "Foodics Email Verification",
                $@"
<h2>Welcome to Foodics</h2>

<p>Your verification code is:</p>

<h1>{otp}</h1>

<p>This code expires in 5 minutes.</p>");

            // إنشاء QR Code
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(customerCode, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrData);

            using var qrBitmap = qrCode.GetGraphic(20);
            using var ms = new MemoryStream();

            qrBitmap.Save(ms, ImageFormat.Png);

            var qrBase64 = Convert.ToBase64String(ms.ToArray());

            return Ok(new
            {
                message = "User created successfully. Please verify your email.",
                customerCode,
                profileImageUrl = imageUrl,
                qrCodeBase64 = qrBase64
            });
        }

        [Authorize]
        [HttpPut("change-profile-image")]
        public async Task<IActionResult> ChangeProfileImage([FromForm] UpdateProfileImageDto model)
        {
            if (model.ProfileImage == null || model.ProfileImage.Length == 0)
                return BadRequest("Please select an image.");

            var userId = User.FindFirstValue("userId");
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound("User not found.");

            // حذف الصورة القديمة
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                var oldFileName = Path.GetFileName(user.ProfileImageUrl);
                var oldFilePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "ProfileImages",
                    oldFileName);

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // إنشاء فولدر لو مش موجود
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "ProfileImages");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // حفظ الصورة الجديدة
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfileImage.CopyToAsync(stream);
            }

            var imageUrl = $"{Request.Scheme}://{Request.Host}/ProfileImages/{fileName}";

            user.ProfileImageUrl = imageUrl;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                message = "Profile image updated successfully.",
                profileImageUrl = imageUrl
            });
        }

        [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // التحقق من وجود المستخدم
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("User not found.");

            // لو الإيميل متفعل بالفعل
            if (user.EmailVerified)
                return BadRequest("Email is already verified.");

            // البحث عن الـ OTP
            var emailOtp = await _context.EmailOtp
                .FirstOrDefaultAsync(x =>
                    x.Email == model.Email &&
                    x.Code == model.Otp &&
                    !x.IsUsed);

            if (emailOtp == null)
                return BadRequest("Invalid OTP.");

            // التحقق من انتهاء الصلاحية
            if (emailOtp.ExpireAt < DateTime.UtcNow)
                return BadRequest("OTP has expired.");

            // تفعيل الإيميل
            user.EmailVerified = true;

            // تعليم الـ OTP بأنه استُخدم
            emailOtp.IsUsed = true;

            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Email verified successfully. You can now login."
            });
        }


        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp(ResendOtpDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("User not found.");

            if (user.EmailVerified)
                return BadRequest("Email is already verified.");

            // منع إعادة الإرسال قبل مرور دقيقة
            var lastOtp = await _context.EmailOtp
                .Where(x => x.Email == model.Email)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastOtp != null &&
                DateTime.UtcNow < lastOtp.CreatedAt.AddMinutes(1))
            {
                return BadRequest("Please wait one minute before requesting another OTP.");
            }

            // حذف الأكواد القديمة
            var oldOtps = _context.EmailOtp.Where(x => x.Email == model.Email);
            _context.EmailOtp.RemoveRange(oldOtps);

            // إنشاء OTP جديد
            var otp = Random.Shared.Next(100000, 999999).ToString();

            _context.EmailOtp.Add(new EmailOtp
            {
                Email = model.Email,
                Code = otp,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            });

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                model.Email,
                "Foodics Email Verification",
                $@"
        <h2>Welcome to Foodics</h2>
        <p>Your new verification code is:</p>
        <h1>{otp}</h1>
        <p>This code expires in 5 minutes.</p>");

            return Ok(new
            {
                message = "A new verification code has been sent to your email."
            });
        }





        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (user == null)
                return Unauthorized("Invalid Phone Number or Password");

            if (user.IsDeleted)
                return Unauthorized("This account has been deleted.");

            //if (user.IsBlocked)
            //    return Unauthorized("This account has been blocked.");

            if (!user.EmailVerified)
                return Unauthorized("Please verify your email before logging in.");


            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Invalid Phone Number or Password");

            var accessToken = await _jwtService.GenerateAccessToken(user);

            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(5);

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                accessToken,
                refreshToken,
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

            if (user.IsDeleted)
                return Unauthorized("This account has been deleted.");

            if (user.IsBlocked)
                return Unauthorized("This account has been blocked.");

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


        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("User not found");

            if (user.IsDeleted)
                return BadRequest("This account has been deleted.");

            if (user.IsBlocked)
                return BadRequest("This account has been blocked.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebUtility.UrlEncode(token);

            var resetLink = $"https://orderinsights.vercel.app/reset-password?email={model.Email}&token={encodedToken}";

            await _emailService.SendEmailAsync(
                model.Email,
                "Reset Password",
                $"Click here to reset your password: {resetLink}");

            return Ok("Password reset link sent to your email");
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("User not found.");

            if (user.IsDeleted)
                return BadRequest("This account has been deleted.");

            if (user.IsBlocked)
                return BadRequest("This account has been blocked.");

            var decodedToken = WebUtility.UrlDecode(model.Token);

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password reset successfully.");
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User?.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found");

            // 🔥 invalidate refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                message = "Logged out successfully"
            });
        }
    }
}



