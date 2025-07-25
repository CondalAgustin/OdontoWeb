using OdontoWebAPI.Models;

public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } // Ej: "admin", "paciente", "gerente"

    public ICollection<Usuario> Usuarios { get; set; }
}