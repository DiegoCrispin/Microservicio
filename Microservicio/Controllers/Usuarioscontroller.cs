using Microservicio.Models; 
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;



namespace Microservicio.Controllers
{
    [Route("Usuario/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=localhost;Database=Microservicios;User ID=Diego;Password=12345";

        // Registro de usuario
        [HttpPost("registro")]
        public async Task<IActionResult> Registrar([FromBody] Usuarios usuario)
        {
            if (usuario == null || string.IsNullOrEmpty(usuario.Password))
            {
                return BadRequest("Datos inválidos.");
            }

            // Encriptar la contraseña antes de almacenarla
            string hashedPassword = HashPassword(usuario.Password);

            try
            {
                using (var con = new MySqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    using (var cmd = new MySqlCommand("INSERT INTO usuarios (nombre, apellido, correo, password) VALUES (@nombre, @apellido, @correo, @password)", con))
                    {
                        cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                        cmd.Parameters.AddWithValue("@apellido", usuario.Apellido);
                        cmd.Parameters.AddWithValue("@correo", usuario.Correo);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return Ok("Usuario registrado exitosamente.");
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, $"Error al registrar el usuario: {ex.Message}");
            }
        }

        // Login de usuario
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Correo) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("Datos inválidos.");
            }

            Usuarios usuario = null;

            try
            {
                using (var con = new MySqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    using (var cmd = new MySqlCommand("SELECT * FROM usuarios WHERE correo = @correo", con))
                    {
                        cmd.Parameters.AddWithValue("@correo", loginRequest.Correo);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                usuario = new Usuarios
                                {
                                    Id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                                    Nombre = reader.GetString("nombre"),
                                    Apellido = reader.GetString("apellido"),
                                    Correo = reader.GetString("correo"),
                                    Password = reader.GetString("password")
                                };
                            }
                        }
                    }
                }

                if (usuario == null || !VerifyPassword(loginRequest.Password, usuario.Password))
                {
                    return Unauthorized("Credenciales incorrectas.");
                }

                return Ok($"Bienvenido {usuario.Nombre} {usuario.Apellido}");
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, $"Error al iniciar sesión: {ex.Message}");
            }
        }

        // Método para encriptar la contraseña
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Método para verificar la contraseña
        private bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInputPassword = HashPassword(password);
            return hashedInputPassword == hashedPassword;
        }
    }

    // Modelo para la petición de login
    public class LoginRequest
    {
        public required string Correo { get; set; }
        public required string Password { get; set; }
    }
}
