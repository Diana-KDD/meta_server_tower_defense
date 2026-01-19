using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TowerDefense.Common.Models.Access;
using TowerDefense.Common.Models.Player;
using TowerDefense.Server.Data;
using TowerDefense.Server.Services;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

//-- Registering the DB context
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

var connectionString = $"Host={builder.Configuration["Database:Host"]};" +
                      $"Port={builder.Configuration["Database:Port"]};" +
                      $"Database={builder.Configuration["Database:Name"]};" +
                      $"Username={builder.Configuration["Database:Username"]};" +
                      $"Password={dbPassword}";

builder.Services.AddDbContext<GameDBContext>(options =>
    options.UseNpgsql(connectionString));

//-- Add Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthorizationSeedService>();

//-- Add controllers
builder.Services.AddControllers();

//-- Add swagger (for the test)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //-- Defining the security scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        //Type = SecuritySchemeType.ApiKey,  // ИЛИ SecuritySchemeType.Http
        Scheme = "Bearer"
    });
    //-- Adding a security requirement for all operations
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

//-- Registration of authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true; //ПОМЕНЯТЬ НА true!!
    options.SaveToken = true;
    var key = Environment.GetEnvironmentVariable("JWT_SECRET");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
        ValidAudience = builder.Configuration["JwtConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(5)

    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

//-- Checking the connection DB and Migration
await ApplyMigrationsWithRetry(app);

//-- Starting Seeds
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<AuthorizationSeedService>();
    await seedService.SeedAsync();
    await seedService.SeedAdmin();
}

app.Use(async (context, next) =>
{
    Console.WriteLine($"Запрос: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Ответ: {context.Response.StatusCode}");
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/", () => "GameTowerDefense API работает!");

app.Run();

async Task ApplyMigrationsWithRetry(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<GameDBContext>();

    logger.LogInformation("Проверка подключения к БД и миграций...");

    int maxRetries = 5;
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            //-- Checking the connection
            var canConnect = await dbContext.Database.CanConnectAsync();

            if (!canConnect)
            {
                throw new Exception("Не удалось подключиться к БД");
            }

            logger.LogInformation("Подключение к БД установлено");

            //-- Checking migrations
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Найдено {pendingMigrations.Count()} непримененных миграций:");
                foreach (var migration in pendingMigrations)
                {
                    logger.LogInformation($"\t- {migration}");
                }

                //-- Applying migrations
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Все миграции успешно применены");
            }
            else
            {
                logger.LogInformation("Все миграции уже применены");
            }

            //-- Checking the current migration
            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
            var currentMigration = appliedMigrations.LastOrDefault();

            if (!string.IsNullOrEmpty(currentMigration))
            {
                logger.LogInformation($"Текущая миграция БД: {currentMigration}");
            }

            return;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning(ex,
                $"Попытка {attempt}/{maxRetries} не удалась. Повтор через {attempt * 2} секунд...");

            await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Не удалось применить миграции после всех попыток");
            throw;
        }
    }

}