using OdontoWebAPI.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Servicios")]
public class Servicio
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public ICollection<TurnoServicio>? TurnosServicios { get; set; }
}
