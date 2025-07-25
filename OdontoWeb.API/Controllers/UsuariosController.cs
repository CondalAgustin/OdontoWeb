using Microsoft.AspNetCore.Mvc;
using OdontoWebAPI.Data;
using OdontoWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OdontoWebAPI.DTOs;

namespace OdontoWeb.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly OdontoContext _context;

        public UsuariosController(OdontoContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios([FromQuery] bool incluirInactivos = false)
        {
            var query = _context.Usuarios
                .Include(u => u.Rol) // importante para que traiga el nombre del rol
                .AsQueryable();

            if (!incluirInactivos)
                query = query.Where(u => u.EstaActivo);

            var usuarios = await query
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Sexo = u.Sexo,
                    Dni = u.Dni,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    Rol = u.Rol.Nombre, // accede al nombre del rol
                    FechaRegistro = u.FechaRegistro,
                    EstaActivo = u.EstaActivo
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult> CrearUsuario(CreateUsuarioDto dto)
        {
            var rol = await _context.Roles.FindAsync(dto.RolId);
            if (rol == null) return BadRequest("Rol no válido.");

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Telefono = dto.Telefono,
                PasswordHash = dto.PasswordHash,
                Dni = dto.Dni,
                Sexo = dto.Sexo,
                Rol = rol,
                EstaActivo = true,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Usuarios/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> EditarUsuario(int id, UpdateUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null || !usuario.EstaActivo) return NotFound();

            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == dto.Rol);
            if (rol == null) return BadRequest("Rol no válido.");

            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.Email = dto.Email;
            usuario.Telefono = dto.Telefono;
            usuario.Dni = dto.Dni;
            usuario.Sexo = dto.Sexo;
            usuario.Rol = rol;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/Usuarios/reactivar/{id}
        [HttpPut("reactivar/{id}")]
        public async Task<ActionResult> ReactivarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null || usuario.EstaActivo) return NotFound();

            usuario.EstaActivo = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Usuarios/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null || !usuario.EstaActivo) return NotFound();

            usuario.EstaActivo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("perfil")]
        public IActionResult ObtenerPerfil()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            Console.WriteLine("Authorization Header: " + authHeader);  // <-- Agregá esto
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id.ToString() == userId);

            if (usuario == null)
                return NotFound();

            return Ok(new
            {
                usuario.Nombre,
                usuario.Apellido,
                usuario.Email,
                usuario.Telefono,
                usuario.Sexo,
                usuario.Dni
            });
        }

        [Authorize]
        [HttpPut("perfilActualizar")]
        public IActionResult ActualizarPerfil([FromBody] UsuarioDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id.ToString() == userId);

            if (usuario == null)
                return NotFound();

            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.Email = dto.Email;
            usuario.Telefono = dto.Telefono;
            usuario.Sexo = dto.Sexo;

            _context.SaveChanges();

            return Ok(new { mensaje = "Perfil actualizado correctamente" });
        }

        [Authorize]
        [HttpPut("cambiarContrasenia")]
        public IActionResult CambiarContrasenia([FromBody] CambiarContraseniaDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id.ToString() == userId);

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            if (usuario.PasswordHash != dto.ContraseniaActual)
                return BadRequest("La contraseña actual es incorrecta");

            if (dto.NuevaContrasenia != dto.ConfirmarContrasenia)
                return BadRequest("La nueva contraseña y la confirmación no coinciden");

            usuario.PasswordHash = dto.NuevaContrasenia;
            _context.SaveChanges();

            return Ok(new { mensaje = "Contraseña actualizada correctamente" });
        }

        [HttpGet("buscarPorDni/{dni}")]
        public async Task<IActionResult> BuscarPorDni(string dni)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Dni == dni);

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            return Ok(new
            {
                id = usuario.Id,
                nombre = $"{usuario.Nombre} {usuario.Apellido}"
            });
        }


    }
}
