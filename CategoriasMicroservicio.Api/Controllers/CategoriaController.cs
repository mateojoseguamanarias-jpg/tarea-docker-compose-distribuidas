using CategoriasMicroservicio.Api.Data;
using CategoriasMicroservicio.Api.DTOs;
using CategoriasMicroservicio.Api.Models;
using CategoriasMicroservicio.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CategoriasMicroservicio.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaController : ControllerBase
    {
        private readonly CategoriasDbContext _context;
        private readonly RabbitMQEventPublisher _eventPublisher;
        private readonly ILogger<CategoriaController> _logger;

        public CategoriaController(
            CategoriasDbContext context,
            RabbitMQEventPublisher eventPublisher,
            ILogger<CategoriaController> logger)
        {
            _context = context;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> ObtenerTodas()
        {
            var categorias = await _context.Categorias.AsNoTracking().ToListAsync();
            return Ok(categorias);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Categoria>> ObtenerPorId(int id)
        {
            var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.IdCategoria == id);
            if (categoria == null)
            {
                return NotFound(new { mensaje = $"Categoría con ID {id} no encontrada." });
            }
            return Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult<Categoria>> CrearCategoria([FromBody] CrearCategoriaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nuevaCategoria = new Categoria
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado
            };

            _context.Categorias.Add(nuevaCategoria);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Categoría creada con ID: {IdCategoria}", nuevaCategoria.IdCategoria);

            // Publicación del evento en RabbitMQ
            await _eventPublisher.PublicarCategoriaCreadaAsync(nuevaCategoria);

            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaCategoria.IdCategoria }, nuevaCategoria);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] ActualizarCategoriaDto dto)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound(new { mensaje = $"Categoría con ID {id} no existe." });
            }

            categoria.Nombre = dto.Nombre;
            categoria.Descripcion = dto.Descripcion;
            categoria.Estado = dto.Estado;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound(new { mensaje = $"Categoría con ID {id} no encontrada." });
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
