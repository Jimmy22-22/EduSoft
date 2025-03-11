using EduSoft.Data;
using Microsoft.EntityFrameworkCore;

namespace EduSoft.Services
{
    public class DashboardEstudianteService
    {
        private readonly AppDbContext _context;

        public DashboardEstudianteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Clase>> GetClasesPorEstudianteAsync(int usuarioId)
        {
            return await _context.UsuarioClases
                .Where(uc => uc.UsuarioId == usuarioId)
                .Select(uc => uc.Clase)
                .OrderBy(c => c.Horario)
                .ToListAsync();
        }

        public async Task<List<Tarea>> GetTareasPorEstudianteAsync(int usuarioId)
        {
            return await _context.Tareas
                .Where(t => t.UsuarioId == usuarioId)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();
        }

        public async Task<bool> UnirseAClaseAsync(int usuarioId, string codigoClase)
        {
            var clase = await _context.Clases.FirstOrDefaultAsync(c => c.CodigoClase == codigoClase);
            if (clase == null) return false;

            bool yaInscrito = await _context.UsuarioClases.AnyAsync(uc => uc.UsuarioId == usuarioId && uc.ClaseId == clase.Id);
            if (yaInscrito) return false;

            _context.UsuarioClases.Add(new UsuarioClase { UsuarioId = usuarioId, ClaseId = clase.Id });
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
