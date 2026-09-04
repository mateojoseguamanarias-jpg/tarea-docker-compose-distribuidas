using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehiculosMicroservicio.Api.Data;
using VehiculosMicroservicio.Api.DTOs;
using VehiculosMicroservicio.Api.Models;

namespace VehiculosMicroservicio.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VehiculoController : ControllerBase
    {
        private readonly VehiculosDbContext _context;
        private readonly ILogger<VehiculoController> _logger;

        public VehiculoController(VehiculosDbContext context, ILogger<VehiculoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehiculo>>> ObtenerTodos()
        {
            var vehiculos = await _context.Vehiculos.AsNoTracking().ToListAsync();
            return Ok(vehiculos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Vehiculo>> ObtenerPorId(int id)
        {
            var vehiculo = await _context.Vehiculos.AsNoTracking().FirstOrDefaultAsync(v => v.IdVehiculo == id);
            if (vehiculo == null)
            {
                return NotFound(new { mensaje = $"Vehículo con ID {id} no encontrado." });
            }
            return Ok(vehiculo);
        }

        [HttpGet("categoria/{idCategoria:int}")]
        public async Task<ActionResult<IEnumerable<Vehiculo>>> ObtenerPorCategoria(int idCategoria)
        {
            var vehiculos = await _context.Vehiculos
                .AsNoTracking()
                .Where(v => v.IdCategoria == idCategoria)
                .ToListAsync();
            return Ok(vehiculos);
        }

        [HttpPost]
        public async Task<ActionResult<Vehiculo>> CrearVehiculo([FromBody] CrearVehiculoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nuevoVehiculo = new Vehiculo
            {
                IdCategoria = dto.IdCategoria,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                Precio = dto.Precio,
                Stock = dto.Stock,
                Estado = dto.Estado
            };

            _context.Vehiculos.Add(nuevoVehiculo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vehículo creado exitosamente con ID: {IdVehiculo}", nuevoVehiculo.IdVehiculo);

            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoVehiculo.IdVehiculo }, nuevoVehiculo);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarVehiculo(int id, [FromBody] ActualizarVehiculoDto dto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound(new { mensaje = $"Vehículo con ID {id} no existe." });
            }

            vehiculo.IdCategoria = dto.IdCategoria;
            vehiculo.Marca = dto.Marca;
            vehiculo.Modelo = dto.Modelo;
            vehiculo.Precio = dto.Precio;
            vehiculo.Stock = dto.Stock;
            vehiculo.Estado = dto.Estado;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarVehiculo(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound(new { mensaje = $"Vehículo con ID {id} no encontrado." });
            }

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
