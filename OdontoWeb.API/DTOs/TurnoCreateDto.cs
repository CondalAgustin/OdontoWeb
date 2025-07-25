namespace OdontoWebAPI.DTOs
{
    public class TurnoCreateDto
    {
        public int usuarioId { get; set; }
        public DateTime FechaTurno { get; set; }
        public TimeSpan HoraTurno { get; set; }
        public string Estado { get; set; }
        public  int idEspecialidad { get; set; }
        public int UsuarioRegistroTurnoId { get; set; }
    }
}
