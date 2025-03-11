using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EduSoft.Services
{
    public class DashboardMaestroService
    {
        private readonly AppDbContext _context;

        public DashboardMaestroService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Clase>> GetClasesPorProfesorAsync(string nombreProfesor)
        {
            return await _context.Clases
                .Where(c => c.Profesor == nombreProfesor)
                .OrderBy(c => c.Horario)
                .ToListAsync();
        }

        public async Task<bool> CrearClaseAsync(string nombre, string profesor)
        {
            var clase = new Clase
            {
                Nombre = nombre,
                Profesor = profesor,
                Horario = DateTime.Now,
                CodigoClase = GenerarCodigoUnico()
            };

            _context.Clases.Add(clase);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerarCodigoUnico()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
