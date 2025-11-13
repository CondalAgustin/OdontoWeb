using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoWeb.API.DTOs
{
    public class RegistraPuntajeDto
    {
        public int id { get; set; }
        public int usuarioid { get; set; }
        public int puntaje { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime fecha { get; set; } = DateTime.UtcNow;
    }
}
