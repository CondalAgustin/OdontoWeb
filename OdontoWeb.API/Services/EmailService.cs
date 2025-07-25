using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService
{
    private readonly string remitente = "odontoweb.noreply@gmail.com";
    private readonly string claveApp = "ytvx qiyt qdar ucej";

    public async Task EnviarEmailAsync(string destinatario, string asunto, string cuerpo)
    {
        var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(remitente, claveApp),
            EnableSsl = true
        };

        var mensaje = new MailMessage(remitente, destinatario, asunto, cuerpo)
        {
            IsBodyHtml = true
        };

        await smtp.SendMailAsync(mensaje);
    }
}
