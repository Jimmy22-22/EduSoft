using BCrypt.Net;
using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterUser(string nombre, string email, string password, RolUsuario rol)
        {
            var existingUser = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
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

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Usuario?> Login(string email, string password)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
                return null;

            var usuariosConSesionActiva = await _context.Usuarios.Where(u => u.SesionActiva).ToListAsync();
            foreach (var user in usuariosConSesionActiva)
            {
                user.SesionActiva = false;
                user.SesionToken = null;
            }

            usuario.SesionActiva = true;
            usuario.SesionToken = Guid.NewGuid().ToString();

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<Usuario?> VerificarSesion()
        {
            return await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.SesionActiva);
        }

        public async Task EliminarSesion(int usuarioId)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
            if (usuario != null)
            {
                usuario.SesionActiva = false;
                usuario.SesionToken = null;
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();
            }
        }
    }
}
