// TurnosController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OdontoWebAPI.Data;
using OdontoWebAPI.DTOs;
using OdontoWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OdontoWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TurnosController : ControllerBase
    {
        private readonly OdontoContext _context;

        public TurnosController(OdontoContext context)
        {
            _context = context;
        }

        // GET: api/Turnos/disponibles?fecha=2025-07-12
        [HttpGet("disponibles")]
        public async Task<IActionResult> GetTurnosDisponibles([FromQuery] DateTime fecha)
        {
            // Traer horarios ocupados y normalizar a minutos
            var horariosOcupados = await _context.Turnos
                .Where(t => t.FechaTurno == fecha.Date)
                .Select(t => t.HoraTurno)
                .ToListAsync();

            var horaInicio = new TimeSpan(8, 0, 0);
            var horaFin = new TimeSpan(17, 0, 0);
            var duracionTurno = TimeSpan.FromMinutes(30);

            var disponibles = new List<TimeSpan>();

            for (var hora = horaInicio; hora < horaFin; hora += duracionTurno)
            {
                // Comparar total de minutos para evitar problemas con ticks o milisegundos
                bool ocupado = horariosOcupados.Any(h => h.TotalMinutes == hora.TotalMinutes);

                if (!ocupado)
                {
                    disponibles.Add(hora);
                }
            }

            return Ok(disponibles);
        }



        [HttpPost]
        public async Task<IActionResult> CrearTurno([FromBody] TurnoCreateDto dto)
        {
            try
            {
                bool turnoExiste = await _context.Turnos.AnyAsync(t =>
                    t.FechaTurno.Date == dto.FechaTurno.Date &&
                    t.HoraTurno == dto.HoraTurno
                );

                if (turnoExiste)
                {
                    return BadRequest(new { mensaje = "Ya hay un turno reservado en ese horario" });
                }

                var nuevoTurno = new Turno
                {
                    UsuarioId = dto.usuarioId,
                    FechaTurno = dto.FechaTurno.Date,
                    HoraTurno = dto.HoraTurno,
                    Estado = dto.Estado,
                    FechaCreacion = DateTime.Now,
                    UsuarioRegistroTurnoId = dto.UsuarioRegistroTurnoId,
                    TurnosServicios = new List<TurnoServicio>
            {
                new TurnoServicio { ServicioId = dto.idEspecialidad }
            }
                };

                _context.Turnos.Add(nuevoTurno);
                await _context.SaveChangesAsync();

                // Obtener datos del usuario para el email
                var usuario = await _context.Usuarios.FindAsync(dto.usuarioId);
                var servicio = await _context.Servicios.FindAsync(dto.idEspecialidad);

                if (usuario != null && servicio != null)
                {
                    var emailService = new EmailService();

                    string asunto = "Confirmación de turno - OdontoWeb";
                    string cuerpo = $@"
                         <h3>Hola {usuario.Nombre},</h3>
                         <p>Tu turno fue reservado correctamente.</p>
                         <p><strong>Fecha:</strong> {dto.FechaTurno:dd/MM/yyyy}</p>
                         <p><strong>Hora:</strong> {dto.HoraTurno.ToString(@"hh\:mm")}</p>
                         <p><strong>Especialidad:</strong> {servicio.Nombre}</p>
                         <br />
                         <p>Gracias por confiar en OdontoWeb.</p>
                          ";


                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await emailService.EnviarEmailAsync(usuario.Email, asunto, cuerpo);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al enviar email: {ex.Message}");
                        }
                    });

                }

                return Ok(new { mensaje = "Turno reservado", idTurno = nuevoTurno.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno al reservar turno", detalle = ex.Message });
            }
        }




        // GET: api/Turnos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var turnos = await _context.Turnos
                .Include(t => t.UsuarioId)
                .ToListAsync();
            return Ok(turnos);
        }

        // PUT: api/Turnos/1
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarTurno(int id, [FromBody] Turno turnoActualizado)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
                return NotFound("Turno no encontrado.");

            turno.FechaTurno = turnoActualizado.FechaTurno;
            turno.HoraTurno = turnoActualizado.HoraTurno;
            turno.Estado = turnoActualizado.Estado;
            turno.UsuarioId = turnoActualizado.UsuarioId;

            await _context.SaveChangesAsync();
            return Ok(turno);
        }

        // DELETE: api/Turnos/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarTurno(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
                return NotFound("Turno no encontrado.");

            _context.Turnos.Remove(turno);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("mis-turnos/{idUsuario}")]
        public async Task<IActionResult> ObtenerTurnosUsuario(int idUsuario)
        {
            try
            {
                var ahora = DateTime.Now;

                var turnos = await _context.Turnos
                    .Include(t => t.TurnosServicios)
                        .ThenInclude(ts => ts.Servicio)
                    .Where(t => t.UsuarioId == idUsuario)
                    .ToListAsync();

                var turnosFuturos = turnos
                    .Where(t => t.FechaTurno.Add(t.HoraTurno) > ahora)
                    .Select(t => new
                    {
                        t.Id,
                        t.FechaTurno,
                        HoraTurno = t.HoraTurno.ToString(@"hh\:mm"),
                        t.Estado,
                        nombreEspecialidad = t.TurnosServicios.Select(ts => ts.Servicio.Nombre).FirstOrDefault()
                    })
                    .ToList();

                return Ok(turnosFuturos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno al obtener los turnos del usuario",
                    error = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("historial-turnos/{idUsuario}")]
        public async Task<IActionResult> ObtenerHistorialTurnos(int idUsuario)
        {
            try
            {
                var ahora = DateTime.Now;

                var turnos = await _context.Turnos
                    .Include(t => t.TurnosServicios)
                        .ThenInclude(ts => ts.Servicio)
                    .Where(t => t.UsuarioId == idUsuario)
                    .ToListAsync();

                var turnosPasados = turnos
                    .Where(t => t.FechaTurno.Add(t.HoraTurno) <= ahora)
                    .Select(t => new
                    {
                        t.Id,
                        t.FechaTurno,
                        HoraTurno = t.HoraTurno.ToString(@"hh\:mm"),
                        t.Estado,
                        nombreEspecialidad = t.TurnosServicios.Select(ts => ts.Servicio.Nombre).FirstOrDefault()
                    })
                    .ToList();

                return Ok(turnosPasados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener el historial de turnos",
                    error = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }



        [HttpPut("cancelar/{idTurno}")]
        public async Task<IActionResult> CancelarTurno(int idTurno)
        {
            var turno = await _context.Turnos.FindAsync(idTurno);
            if (turno == null)
            {
                return NotFound(new { mensaje = "Turno no encontrado" });
            }

            var ahora = DateTime.Now;
            var fechaCompleta = turno.FechaTurno.Date + turno.HoraTurno;

            if (fechaCompleta <= ahora)
            {
                return BadRequest(new { mensaje = "No se puede cancelar un turno pasado." });
            }

            if ((fechaCompleta - ahora).TotalHours < 12)
            {
                return BadRequest(new { mensaje = "No se puede cancelar un turno con menos de 12 horas de anticipación." });
            }

            turno.Estado = "Cancelado";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Turno cancelado correctamente" });
        }

        [HttpGet("por-fecha")]
        public async Task<IActionResult> ObtenerTurnosPorFecha([FromQuery] DateTime fecha)
        {
            try
            {
                var turnos = await _context.Turnos
      .Include(t => t.Usuario) // ← ahora esto sí es válido
      .Include(t => t.TurnosServicios)
          .ThenInclude(ts => ts.Servicio)
      .Where(t => t.FechaTurno.Date == fecha.Date)
      .Select(t => new
      {
          t.Id,
          t.FechaTurno,
          HoraTurno = t.HoraTurno.ToString(@"hh\:mm"),
          t.Estado,
          PacienteDni = t.Usuario.Dni,
          PacienteNombre = t.Usuario.Nombre,
          PacienteEmail = t.Usuario.Email,
          PacienteTelefono = t.Usuario.Telefono,
          Especialidad = t.TurnosServicios.Select(ts => ts.Servicio.Nombre).FirstOrDefault()
      })
      .ToListAsync();


                return Ok(turnos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener turnos", ex.Message });
            }
        }

        [HttpGet("proximos/{idUsuario}")]
        public async Task<IActionResult> ObtenerTurnosProximos(int idUsuario)
        {
            var turnos = await _context.Turnos
                .Where(t => t.UsuarioId == idUsuario && t.FechaTurno >= DateTime.Today)
                .Include(t => t.TurnosServicios)
                    .ThenInclude(ts => ts.Servicio)
                .Select(t => new
                {
                    t.FechaTurno,
                    t.HoraTurno,
                    Especialidad = t.TurnosServicios.FirstOrDefault().Servicio.Nombre
                })
                .ToListAsync();

            return Ok(turnos);
        }



    }
}
