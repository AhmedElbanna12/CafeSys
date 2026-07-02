
using DotNetEnv;
using FirebaseAdmin;
using Foodics.Dtos.Auth;
using Foodics.Filters;
using Foodics.Helpers;
using Foodics.Hub;
using Foodics.Models;
using Foodics.Services;
using Foodics.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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


            // ✅ مرة واحدة بس بكل الـ options
            builder.Services.AddControllers(options =>
            {
                options.Filters.AddService<ActiveUserFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.UnmappedMemberHandling =
                    System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip;
                options.JsonSerializerOptions.DefaultIgnoreCondition =
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.ReferenceHandler =
                    System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
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




            builder.Services.AddSingleton<OfflineOrderService>();


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
                        "https://cafe-app-amber.vercel.app",
                        "https://orderinsights.vercel.app",
                        "http://localhost:3000",
                        "http://localhost:8080",
                        "http://localhost:5173",
                        "http://localhost:4173",
                        "http://192.168.1.96:5173",
                        "https://localhost:7171"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });

                // ✅ أضف policy جديدة للـ webhook
                options.AddPolicy("AllowPaymob", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });


            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<FcmService>();



            var firebaseKeyPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-key.json");

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(firebaseKeyPath)
                });
            }

            //// ✅ وحطّ ده بدله
            //var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");

            //if (FirebaseApp.DefaultInstance == null)
            //{
            //    FirebaseApp.Create(new AppOptions()
            //    {
            //        Credential = GoogleCredential.FromJson(firebaseJson)
            //    });
            //}

            //FirebaseApp.Create(new AppOptions()
            //{
            //    Credential = GoogleCredential.FromFile("firebase-key.json")
            //});



            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Foodics API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter JWT Token like this: Bearer {your token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });


            builder.Services.AddHttpClient<IPaymobService, PaymobService>();
            builder.Services.Configure<PaymobOptions>(options =>
            {
                options.ApiKey = Environment.GetEnvironmentVariable("PAYMOB_API_KEY")!;
                options.SecretKey = Environment.GetEnvironmentVariable("PAYMOB_SECRET_KEY")!;
                options.PublicKey = Environment.GetEnvironmentVariable("PAYMOB_PUBLIC_KEY")!;
                options.IntegrationId = int.Parse(
                    Environment.GetEnvironmentVariable("PAYMOB_INTEGRATION_ID")!);

                var walletIntegrationId = Environment.GetEnvironmentVariable("PAYMOB_WALLET_INTEGRATION_ID");
                if (string.IsNullOrEmpty(walletIntegrationId))
                {
                    throw new Exception("PAYMOB_WALLET_INTEGRATION_ID is missing in .env file");
                }
                options.WalletIntegrationId = int.Parse(walletIntegrationId);

                options.HmacSecret = Environment.GetEnvironmentVariable("PAYMOB_HMAC_SECRET")!;
                options.BaseUrl = "https://accept.paymob.com";
                options.WebhookUrl = Environment.GetEnvironmentVariable("PAYMOB_WEBHOOK_URL")!; 
                options.RedirectionUrl = Environment.GetEnvironmentVariable("PAYMOB_REDIRECTION_URL")!;
            });


            builder.Services.AddScoped<ActiveUserFilter>();

            builder.Services.AddSingleton<InMemoryLogStore>();

            var app = builder.Build();

            // Seed Data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                var admins = new List<(string Email, string Password, string Phone)>
{
    (adminEmail, adminPassword, adminphonenumber)
};

                // ✅ غيّر builder.Configuration لـ Environment.GetEnvironmentVariable
                var admin2Email = Environment.GetEnvironmentVariable("ADMIN2_EMAIL");
                var admin2Password = Environment.GetEnvironmentVariable("ADMIN2_PASSWORD");
                var admin2Phone = Environment.GetEnvironmentVariable("ADMIN2_PHONE");

                if (!string.IsNullOrEmpty(admin2Email) && !string.IsNullOrEmpty(admin2Password))
                    admins.Add((admin2Email, admin2Password, admin2Phone ?? ""));

                await SeedData.InitializeAsync(context, userManager, roleManager, admins);
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

            app.UseRouting();          // ✅ MUST be first

            // ✅ Apply CORS with path-based routing
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value ?? "";
                if (path.StartsWith("/api/paymob/webhook", StringComparison.OrdinalIgnoreCase))
                {
                    // ✅ Allow Paymob webhook - any origin
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                    if (context.Request.Method == "OPTIONS")
                    {
                        context.Response.StatusCode = 204;
                        return;
                    }
                }
                await next();
            });

            app.UseCors("AllowFrontend"); 
           // app.UseRateLimiter();      // ✅ لازم بعد UseRouting
            app.UseAuthentication();   // التاني
            app.UseAuthorization();    // التالت



            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");  // بدل UseEndpoints
                                                              // في آخر الـ endpoints، قبل app.Run()


            //var logMessages = new System.Collections.Concurrent.ConcurrentQueue<string>();

            //app.Use(async (context, next) =>
            //{
            //    if (context.Request.Path.StartsWithSegments("/api/paymob/webhook"))
            //    {
            //        var originalBody = context.Response.Body;
            //        context.Request.EnableBuffering();
            //        await next();
            //        context.Request.Body.Position = 0;
            //        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            //        var hmac = context.Request.Query["hmac"].ToString();
            //        logMessages.Enqueue($"[{DateTime.Now}] HMAC={hmac} | Body={body}");
            //        while (logMessages.Count > 20) logMessages.TryDequeue(out _);
            //        return;
            //    }
            //    await next();
            //});

            //app.MapGet("/api/paymob/logs", () =>
            //{
            //    return string.Join("\n\n---\n\n", logMessages);
            //}).AllowAnonymous();


            //app.Use(async (context, next) =>
            //{
            //    if (context.Request.Path.StartsWithSegments("/api/paymob/webhook"))
            //    {
            //        context.Request.EnableBuffering();
            //        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            //        context.Request.Body.Position = 0;

            //        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            //        logger.LogWarning("🔍 WEBHOOK PAYLOAD: {Payload}", body);
            //    }
            //    await next();
            //});


            app.Run();
        }
    }
}
