using System.ComponentModel.DataAnnotations;

namespace Microservicio.Models
{
    public class Usuarios
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Nombre { get; set; }

        [Required]
        public required string Apellido { get; set; }

        [Required]
        [EmailAddress]
        public  required string Correo { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
