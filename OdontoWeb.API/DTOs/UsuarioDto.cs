public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string? Rol { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public bool EstaActivo { get; set; }
    public string Dni { get; set; }
    public string Sexo { get; set; }

}
