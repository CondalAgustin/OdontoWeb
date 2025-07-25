using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OdontoWebAPI.Data;
using OdontoWebAPI.DTOs;
using OdontoWebAPI.Helpers;

namespace OdontoWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly OdontoContext _context;
        private readonly IConfiguration _config;

        public AuthController(OdontoContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PasswordHash))
                return BadRequest("Email y contraseña son obligatorios.");

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.PasswordHash == request.PasswordHash);

                if (usuario == null)
                    return Unauthorized("Credenciales inválidas.");

                if (!usuario.EstaActivo)
                    return Unauthorized("Su usuario se encuentra dado de baja. Para reactivarlo, contáctese con el siguiente email: condal.agustin@outlook.com");

                var secretKey = _config["Jwt:Key"]!;
                var issuer = _config["Jwt:Issuer"]!;
                var audience = _config["Jwt:Audience"]!;
                var token = JwtHelper.GenerateJwtToken(usuario, secretKey, issuer, audience);


                return Ok(new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Email,
                    Rol = usuario.Rol?.Nombre, // Asegurate de que sea string
                    Token = token
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR EN LOGIN: " + ex.Message);
                return StatusCode(500, "Error interno al procesar el login.");
            }

        }
    }
}
