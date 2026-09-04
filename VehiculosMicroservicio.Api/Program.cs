using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VehiculosMicroservicio.Api.Data;
using VehiculosMicroservicio.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForDistributedSystemsVehiculos2026!#Key";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "OAuthJwtService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SistemaDistribuidosVehiculos";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Base de datos SQL Server
builder.Services.AddDbContext<VehiculosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VehiculosConnection")));

// Consumidor en segundo plano de RabbitMQ
builder.Services.AddHostedService<RabbitMQEventConsumer>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Microservicio de Vehículos - API", Version = "v1", Description = "Gestión de vehículos y stock con eventos RabbitMQ y seguridad JWT" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vehículos API v1"));

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
