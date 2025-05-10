using BCrypt.Net;
using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    /// <summary>
    /// Servicio de autenticación que gestiona el registro, inicio de sesión, verificación y cierre de sesión de usuarios.
    /// </summary>
    public class AuthService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        /// <summary>
        /// Constructor del servicio de autenticación.
        /// </summary>
        /// <param name="contextFactory">Fábrica para crear instancias del contexto de base de datos.</param>
        public AuthService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Registra un nuevo usuario si el correo electrónico no está en uso.
        /// </summary>
        /// <param name="nombre">Nombre del usuario.</param>
        /// <param name="email">Correo electrónico del usuario.</param>
        /// <param name="password">Contraseña del usuario (en texto plano).</param>
        /// <param name="rol">Rol del usuario (estudiante o maestro).</param>
        /// <returns>True si el usuario fue registrado exitosamente, false si el correo ya existe.</returns>
        public async Task<bool> RegisterUser(string nombre, string email, string password, RolUsuario rol)
        {
            using var context = _contextFactory.CreateDbContext();
            var existingUser = await context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
                return false;

            var usuario = new Usuario
            {
                Nombre = nombre,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Rol = rol,
                SesionActiva = false,
                SesionToken = null
            };

            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Realiza el proceso de inicio de sesión y desactiva otras sesiones activas.
        /// </summary>
        /// <param name="email">Correo electrónico del usuario.</param>
        /// <param name="password">Contraseña ingresada por el usuario.</param>
        /// <returns>El usuario autenticado si las credenciales son válidas; de lo contrario, null.</returns>
        public async Task<Usuario?> Login(string email, string password)
        {
            using var context = _contextFactory.CreateDbContext();
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
                return null;

            var usuariosConSesionActiva = await context.Usuarios.Where(u => u.SesionActiva).ToListAsync();
            foreach (var user in usuariosConSesionActiva)
            {
                user.SesionActiva = false;
                user.SesionToken = null;
            }

            usuario.SesionActiva = true;
            usuario.SesionToken = Guid.NewGuid().ToString();

            context.Usuarios.Update(usuario);
            await context.SaveChangesAsync();

            return usuario;
        }

        /// <summary>
        /// Verifica si existe una sesión activa y devuelve el usuario correspondiente.
        /// </summary>
        /// <returns>El usuario con sesión activa, o null si no hay sesión activa.</returns>
        public async Task<Usuario?> VerificarSesion()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.SesionActiva);
        }

        /// <summary>
        /// Elimina la sesión activa del usuario especificado.
        /// </summary>
        /// <param name="usuarioId">ID del usuario que cerrará sesión.</param>
        public async Task EliminarSesion(int usuarioId)
        {
            using var context = _contextFactory.CreateDbContext();
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
            if (usuario != null)
            {
                usuario.SesionActiva = false;
                usuario.SesionToken = null;
                context.Usuarios.Update(usuario);
                await context.SaveChangesAsync();
            }
        }
    }
}
