using Microservicio.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Microservicio.Controllers
{
    [Route("usuario/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly string _connectionString;

        public UsuarioController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registrar([FromBody] Usuarios usuario)
        {
            if (usuario == null || string.IsNullOrEmpty(usuario.Password) || string.IsNullOrEmpty(usuario.Correo))
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
                    using (var cmd = new MySqlCommand("INSERT INTO usuarios (nombre, apellido, correo, password, rol) VALUES (@nombre, @apellido, @correo, @password, @rol)", con))
                    {
                        cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                        cmd.Parameters.AddWithValue("@apellido", usuario.Apellido);
                        cmd.Parameters.AddWithValue("@correo", usuario.Correo);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);
                        cmd.Parameters.AddWithValue("@rol", usuario.Rol); // Esto tomará el rol por defecto

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return CreatedAtAction(nameof(ListarUsuarios), new { id = usuario.Correo }, "Usuario registrado exitosamente.");
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
                                    Id = reader.GetInt32("id"),
                                    Nombre = reader.GetString("nombre"),
                                    Apellido = reader.GetString("apellido"),
                                    Correo = reader.GetString("correo"),
                                    Password = reader.GetString("password"), // No exponer esto
                                    Rol = reader.GetString("rol")
                                };
                            }
                        }
                    }
                }

                if (usuario == null || !VerifyPassword(loginRequest.Password, usuario.Password))
                {
                    return Unauthorized("Credenciales incorrectas.");
                }

                return Ok(new { mensaje = $"Bienvenido {usuario.Nombre} {usuario.Apellido}", rol = usuario.Rol }); // Devolver el rol
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, $"Error al iniciar sesión: {ex.Message}");
            }
        }



        // Obtener todos los usuarios
        [HttpGet("listar")]
        public async Task<IActionResult> ListarUsuarios()
        {
            try
            {
                var usuarios = new List<UsuarioDto>(); // Usa un DTO aquí

                using (var con = new MySqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    using (var cmd = new MySqlCommand("SELECT id, nombre, apellido, correo, rol FROM usuarios", con)) // No seleccionar password
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var usuario = new UsuarioDto
                                {
                                    Id = reader.GetInt32("id"),
                                    Nombre = reader.GetString("nombre"),
                                    Apellido = reader.GetString("apellido"),
                                    Correo = reader.GetString("correo"),
                                    Rol = reader.GetString("rol")
                                };
                                usuarios.Add(usuario);
                            }
                        }
                    }
                }

                if (usuarios.Count == 0)
                {
                    return NotFound("No hay usuarios registrados.");
                }

                return Ok(usuarios);
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, $"Error al listar usuarios: {ex.Message}");
            }
        }

        [HttpPut("asignar-rol/{id}")]
        public async Task<IActionResult> AsignarRol(int id, [FromBody] string nuevoRol)
        {
            if (string.IsNullOrEmpty(nuevoRol) || (nuevoRol != "Usuario" && nuevoRol != "Administrador" && nuevoRol != "Superusuario"))
            {
                return BadRequest(new { mensaje = "Rol inválido." });
            }

            try
            {
                using (var con = new MySqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    using (var cmd = new MySqlCommand("UPDATE usuarios SET rol = @rol WHERE id = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@rol", nuevoRol);
                        cmd.Parameters.AddWithValue("@id", id);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected == 0)
                        {
                            return NotFound(new { mensaje = "Usuario no encontrado." });
                        }
                    }
                }

                return Ok(new { mensaje = "Rol asignado exitosamente." }); // Cambiado para devolver un objeto JSON
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, new { mensaje = $"Error al asignar el rol: {ex.Message}" }); // Cambiado para devolver un objeto JSON
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

    // DTO para la respuesta de listar usuarios
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
    }

    // Modelo para la petición de login
    public class LoginRequest
    {
        public required string Correo { get; set; }
        public required string Password { get; set; }
    }
}
