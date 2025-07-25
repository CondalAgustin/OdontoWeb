
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoWebAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellido { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime? FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; } = DateTime.Now;

      //  public ICollection<Turno>? Turnos { get; set; }
        public bool EstaActivo { get; set; } = true;
        public string Dni { get; set; }
        public string Sexo { get; set; }
        public int RolId { get; set; }
        public Rol Rol { get; set; }

        [InverseProperty("Usuario")]
        public ICollection<Turno> TurnosComoPaciente { get; set; }

        [InverseProperty("UsuarioRegistroTurno")]
        public ICollection<Turno> TurnosReservados { get; set; }

    }



    public class Turno
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        [InverseProperty("TurnosComoPaciente")]
        public Usuario Usuario { get; set; }

        [Required]
        public int UsuarioRegistroTurnoId { get; set; }

        [ForeignKey("UsuarioRegistroTurnoId")]
        [InverseProperty("TurnosReservados")]
        public Usuario UsuarioRegistroTurno { get; set; }

        public DateTime FechaTurno { get; set; }
        public TimeSpan HoraTurno { get; set; }
        public DateTime FechaCreacion { get; set; }

        public string Estado { get; set; }

        public ICollection<TurnoServicio>? TurnosServicios { get; set; }
    }

    public class TurnoServicio
    {
        public int TurnoId { get; set; }
        public Turno Turno { get; set; } = null!;

        public int ServicioId { get; set; }
        public Servicio Servicio { get; set; } = null!;
    }
}