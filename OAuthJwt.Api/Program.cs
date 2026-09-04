var builder = WebApplication.CreateBuilder(args);

// Configuración de Controladores
builder.Services.AddControllers();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Servicio Independiente OAuth/JWT - API",
        Version = "v1",
        Description = "Microservicio dedicado a la autenticación, generación y validación de tokens JWT en arquitectura distribuida."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OAuth/JWT API v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
});

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
