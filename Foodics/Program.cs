
using DotNetEnv;
using FirebaseAdmin;
using Foodics.Dtos.Auth;
using Foodics.Hub;
using Foodics.Models;
using Foodics.Services;
using Foodics.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POSSystem.Data;
using System.Text;

namespace Foodics
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Configuration.AddEnvironmentVariables();
            DotNetEnv.Env.Load();

            builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
            // Access variables
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            var adminphonenumber = Environment.GetEnvironmentVariable("ADMIN_PHONE");


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));


            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<JwtService>();
            //builder.Services.AddScoped<SmsService>();

            //builder.Services.Configure<TwilioSettings>(options =>
            //{
            //    options.AccountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            //    options.AuthToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            //    options.FromNumber = Environment.GetEnvironmentVariable("TWILIO_FROM_NUMBER");
            //});

            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("JWT_KEY is missing in .env file");
            }

            var key = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                    ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });


            builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });


            builder.Services.AddSingleton<OfflineOrderService>();


            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            // ✅ وحطّ ده بدله
            builder.Services.Configure<EmailSettings>(options =>
            {
                options.Email = Environment.GetEnvironmentVariable("EMAIL_SETTINGS_EMAIL") ?? string.Empty;
                options.Password = Environment.GetEnvironmentVariable("EMAIL_SETTINGS_PASSWORD") ?? string.Empty;
                options.Host = Environment.GetEnvironmentVariable("EMAIL_SETTINGS_HOST") ?? "smtp.gmail.com";
                options.Port = int.Parse(Environment.GetEnvironmentVariable("EMAIL_SETTINGS_PORT") ?? "587");
            });




            // 7. الـ CORS (تعديلات زمايلك المهمة)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                            
                             "https://cafe-app-amber.vercel.app" ,
                              "http://localhost:3000",
                              "http://localhost:5173",
                              "http://localhost:4173",
                              "http://192.168.1.96:5173",
                              "https://localhost:7171"
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });


            builder.Services.AddScoped<IEmailService, EmailService>();

            var app = builder.Build();

            // Seed Data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await SeedData.InitializeAsync(context, userManager, roleManager, adminEmail, adminPassword, adminphonenumber);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseDeveloperExceptionPage(); //temp 


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("AllowFrontend"); 
            app.UseRouting();          // الأول
           // app.UseRateLimiter();      // ✅ لازم بعد UseRouting
            app.UseAuthentication();   // التاني
            app.UseAuthorization();    // التالت



            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");  // بدل UseEndpoints

            app.Run();
        }
    }
}
