using System.Text;
using CategoriasMicroservicio.Api.Data;
using CategoriasMicroservicio.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

// Inyección de dependencias
builder.Services.AddScoped<RabbitMQEventPublisher>();

// Base de datos SQL Server
builder.Services.AddDbContext<CategoriasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CategoriasConnection")));

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Microservicio de Categorías - API", Version = "v1", Description = "Gestión de categorías y eventos RabbitMQ con seguridad JWT" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Categorías API v1"));

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
