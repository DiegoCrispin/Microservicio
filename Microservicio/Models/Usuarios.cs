using System.ComponentModel.DataAnnotations;

namespace Microservicio.Models
{
    public class Usuarios
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Password { get; set; }

        // Asigna el rol por defecto como 'Usuario'
        public string Rol { get; set; } = "Usuario"; // Rol por defecto
    }

}
