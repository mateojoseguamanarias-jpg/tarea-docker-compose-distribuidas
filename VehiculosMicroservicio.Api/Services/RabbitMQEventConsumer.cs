using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using VehiculosMicroservicio.Api.Data;
using VehiculosMicroservicio.Api.Events;
using VehiculosMicroservicio.Api.Models;

namespace VehiculosMicroservicio.Api.Services
{
    /// <summary>
    /// BackgroundService que escucha eventos de nuevas categorías para inicializar vehículos demo o registrar la categoría.
    /// </summary>
    public class RabbitMQEventConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQEventConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQEventConsumer(
            IConfiguration configuration,
            ILogger<RabbitMQEventConsumer> logger,
            IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando Consumidor RabbitMQ en Microservicio de Vehículos...");

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.TryParse(_configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                    UserName = _configuration["RabbitMQ:UserName"] ?? "admin",
                    Password = _configuration["RabbitMQ:Password"] ?? "admin123"
                };

                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                var queueName = _configuration["RabbitMQ:QueueName"] ?? "categoria_creada";

                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken
                );

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (sender, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var evento = JsonSerializer.Deserialize<CategoriaCreadaEvento>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (evento != null && evento.IdCategoria > 0)
                        {
                            _logger.LogInformation("Evento recibido en Vehículos: Categoría ID: {IdCategoria} - {Nombre}",
                                evento.IdCategoria, evento.Nombre);

                            // Opcional: auto-registrar un vehículo de muestra si es una categoría nueva
                            using var scope = _scopeFactory.CreateScope();
                            var dbContext = scope.ServiceProvider.GetRequiredService<VehiculosDbContext>();

                            var tieneVehiculos = await dbContext.Vehiculos.AnyAsync(v => v.IdCategoria == evento.IdCategoria);
                            if (!tieneVehiculos)
                            {
                                var vehiculoDemo = new Vehiculo
                                {
                                    IdCategoria = evento.IdCategoria,
                                    Marca = "Marca Base",
                                    Modelo = $"Modelo {evento.Nombre}",
                                    Precio = 15000.00m,
                                    Stock = 1,
                                    Estado = true
                                };

                                dbContext.Vehiculos.Add(vehiculoDemo);
                                await dbContext.SaveChangesAsync();

                                _logger.LogInformation("Vehículo inicial creado automáticamente para Categoría ID {IdCategoria}", evento.IdCategoria);
                            }
                        }

                        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar mensaje de RabbitMQ en Vehículos");
                        await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                await _channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken
                );

                _logger.LogInformation("Consumidor de Vehículos conectado exitosamente a la cola '{Queue}'", queueName);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumidor de Vehículos detenido.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico de conexión RabbitMQ en Vehículos");
            }
        }
    }
}
