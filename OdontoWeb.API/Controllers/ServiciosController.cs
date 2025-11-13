using Microsoft.AspNetCore.Mvc;
using OdontoWebAPI.Data;
using OdontoWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OdontoWebAPI.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ServiciosController : ControllerBase
{
    private readonly OdontoContext _context;

    public ServiciosController(OdontoContext context)
    {
        _context = context;
    }

    // GET: api/Especialidades
    [HttpGet]
    public IActionResult Get()
    {
        var especialidades = _context.Servicios.ToList();
        return Ok(especialidades);
    }

    // GET: api/Especialidades/3
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var especialidad = _context.Servicios.FirstOrDefault(e => e.Id == id);
        if (especialidad == null)
            return NotFound("Especialidad no encontrada");

        return Ok(especialidad);
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ServiciosDto dto)
    {
        var nueva = new Servicio
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        _context.Servicios.Add(nueva);
        await _context.SaveChangesAsync();

        return Ok(nueva);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] ServiciosDto dto)
    {
        var existente = await _context.Servicios.FindAsync(id);
        if (existente == null) return NotFound();

        existente.Nombre = dto.Nombre;
        existente.Descripcion = dto.Descripcion;

        await _context.SaveChangesAsync();
        return Ok(existente);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existente = await _context.Servicios.FindAsync(id);
        if (existente == null) return NotFound();

        _context.Servicios.Remove(existente);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("obtenerPreguntas")]
    public IActionResult ObtenerPreguntas()
    {
        var faciles = _context.preguntas
            .Where(p => p.dificultad == "Fácil")
            .OrderBy(x => Guid.NewGuid())
            .Take(5)
            .ToList();

        var dificiles = _context.preguntas
            .Where(p => p.dificultad == "Dificil" || p.dificultad == "Difícil")
            .OrderBy(x => Guid.NewGuid())
            .Take(5)
            .ToList();

        var seleccionadas = faciles.Concat(dificiles)
            .OrderBy(x => Guid.NewGuid())
            .ToList();

        // transformar las columnas A/B/C/D a un array de opciones
        var resultado = seleccionadas.Select(p => new
        {
            id = p.id,
            pregunta = p.texto,
            opciones = new string[] { p.opciona, p.opcionb, p.opcionc, p.opciond },
            correcta = p.correcta,
            dificultad = p.dificultad
        });
          
        return Ok(resultado);
    }

}
