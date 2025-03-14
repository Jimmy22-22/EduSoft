using BCrypt.Net;
using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    public class AuthService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public AuthService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

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

        public async Task<Usuario?> VerificarSesion()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.SesionActiva);
        }

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