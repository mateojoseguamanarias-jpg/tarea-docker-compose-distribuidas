using System.Text;
using System.Text.Json;
using CategoriasMicroservicio.Api.Models;
using RabbitMQ.Client;

namespace CategoriasMicroservicio.Api.Services
{
    public class RabbitMQEventPublisher
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQEventPublisher> _logger;

        public RabbitMQEventPublisher(IConfiguration configuration, ILogger<RabbitMQEventPublisher> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task PublicarCategoriaCreadaAsync(Categoria categoria)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.TryParse(_configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                    UserName = _configuration["RabbitMQ:UserName"] ?? "admin",
                    Password = _configuration["RabbitMQ:Password"] ?? "admin123"
                };

                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                var queueName = _configuration["RabbitMQ:QueueName"] ?? "categoria_creada";

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var payload = new
                {
                    IdCategoria = categoria.IdCategoria,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    Estado = categoria.Estado,
                    FechaEvento = DateTime.UtcNow
                };

                var mensaje = JsonSerializer.Serialize(payload);
                var body = Encoding.UTF8.GetBytes(mensaje);

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    body: body
                );

                _logger.LogInformation("Evento publicado en RabbitMQ [Cola: {Queue}]: Categoria ID {IdCategoria} - {Nombre}",
                    queueName, categoria.IdCategoria, categoria.Nombre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al publicar evento RabbitMQ para Categoria ID: {IdCategoria}", categoria.IdCategoria);
            }
        }
    }
}
