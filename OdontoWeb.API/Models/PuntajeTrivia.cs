using OdontoWebAPI.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoWeb.Models
{
    public class PuntajeTrivia
    {
        public int id { get; set; }
        public int usuarioid { get; set; }
        public int puntaje { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime fecha { get; set; } = DateTime.UtcNow;

        // 👇 relación con usuario
        [ForeignKey("usuarioid")]
        public Usuario Usuario { get; set; }
    }
}
