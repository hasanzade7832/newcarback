using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CarAds.Data;
using CarAds.Hubs;
using CarAds.Models;
using CarAds.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// ✅ سرویس پاک‌سازی پیام‌های تلگرام راس ۱۲ شب
builder.Services.AddHostedService<TelegramCleanupService>();

var jwt = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(jwt["Key"]!);

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
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "https://tanisha-wheaten-nico.ngrok-free.dev",
                "https://k1khodro.liara.run",
                "https://k1khodro-swagger.liara.run",
                "https://k1khodro.com",
                "https://www.k1khodro.com",
                "https://api.k1khodro.com",
                "http://k1khodro.com",
                "http://www.k1khodro.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

// ✅ Migration خودکار
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "❌ Database.Migrate failed.");
}

// ✅ ساخت جدول TelegramMessages اگه وجود نداشته باشه
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TelegramMessages')
        CREATE TABLE TelegramMessages (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            MessageId BIGINT NOT NULL,
            ChatId BIGINT NOT NULL,
            Text NVARCHAR(MAX) NOT NULL DEFAULT '',
            FromUsername NVARCHAR(255) NULL,
            FromFirstName NVARCHAR(255) NULL,
            ReceivedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            TelegramLink NVARCHAR(500) NOT NULL DEFAULT ''
        )
    ");
    app.Logger.LogInformation("✅ TelegramMessages table ready");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "❌ TelegramMessages table creation failed");
}

try
{
    await DbSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "❌ DbSeeder.SeedAsync failed.");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CarAds API v1");
    c.RoutePrefix = "swagger";
});

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<CarAdHub>("/hubs/carAds");

app.Run();