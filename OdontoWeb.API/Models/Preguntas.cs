using System.ComponentModel.DataAnnotations;

namespace OdontoWeb.Models
{
    public class Pregunta
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string texto { get; set; } = string.Empty;

        [Required]
        public string opciona { get; set; } = string.Empty;

        [Required]
        public string opcionb { get; set; } = string.Empty;

        [Required]
        public string opcionc { get; set; } = string.Empty;

        [Required]
        public string opciond { get; set; } = string.Empty;

        [Required]
        public string correcta { get; set; } = string.Empty; // "A", "B", "C" o "D"

        [Required]
        public string dificultad { get; set; } = string.Empty; // "facil" o "dificil"
    }
}
