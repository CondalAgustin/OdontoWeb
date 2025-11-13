using Microsoft.EntityFrameworkCore;
using OdontoWeb.Models;
using OdontoWebAPI.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace OdontoWebAPI.Data
{
    public class OdontoContext : DbContext
    {
        public OdontoContext(DbContextOptions<OdontoContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<TurnoServicio> TurnosServicios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Servicio> Servicios { get; set; }

        public DbSet<Pregunta> preguntas { get; set; }
        public DbSet<PuntajeTrivia> puntajestrivia { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relaciones para Turno con Usuario (como paciente)
            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Usuario)
                .WithMany(u => u.TurnosComoPaciente)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones para Turno con UsuarioRegistroTurno (quien creó el turno)
            modelBuilder.Entity<Turno>()
                .HasOne(t => t.UsuarioRegistroTurno)
                .WithMany(u => u.TurnosReservados)
                .HasForeignKey(t => t.UsuarioRegistroTurnoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación muchos a muchos Turno <-> Servicio
            modelBuilder.Entity<TurnoServicio>()
                .HasKey(ts => new { ts.TurnoId, ts.ServicioId });

            modelBuilder.Entity<TurnoServicio>()
                .HasOne(ts => ts.Turno)
                .WithMany(t => t.TurnosServicios)
                .HasForeignKey(ts => ts.TurnoId);

            modelBuilder.Entity<TurnoServicio>()
                .HasOne(ts => ts.Servicio)
                .WithMany(s => s.TurnosServicios)
                .HasForeignKey(ts => ts.ServicioId);

            // Datos semilla para roles
            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "paciente" },
                new Rol { Id = 2, Nombre = "admin" },
                new Rol { Id = 3, Nombre = "gerente" }
            );
        }

    }
}
