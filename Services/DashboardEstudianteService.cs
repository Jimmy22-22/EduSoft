using EduSoft.Data;
using Microsoft.EntityFrameworkCore;

namespace EduSoft.Services
{
    public class DashboardEstudianteService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        public DashboardEstudianteService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public async Task<List<Clase>> GetClasesPorEstudianteAsync(int usuarioId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.UsuarioClases
                .Where(uc => uc.UsuarioId == usuarioId)
                .Select(uc => uc.Clase)
                .OrderBy(c => context.HorariosClases
                    .Where(h => h.ClaseId == c.Id)
                    .OrderBy(h => h.HoraInicio)
                    .Select(h => h.HoraInicio)
                    .FirstOrDefault())
                .ToListAsync();
        }
        public async Task<List<Tarea>> GetTareasPorEstudianteAsync(int usuarioId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tareas
                .Where(t => t.UsuarioId == usuarioId)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();
        }
        public async Task<bool> UnirseAClaseAsync(int usuarioId, string codigoClase)
        {
            using var context = _contextFactory.CreateDbContext();
            var clase = await context.Clases.FirstOrDefaultAsync(c => c.CodigoClase == codigoClase);
            if (clase == null) return false;

            bool yaInscrito = await context.UsuarioClases.AnyAsync(uc => uc.UsuarioId == usuarioId && uc.ClaseId == clase.Id);
            if (yaInscrito) return false;

            context.UsuarioClases.Add(new UsuarioClase { UsuarioId = usuarioId, ClaseId = clase.Id });
            await context.SaveChangesAsync();
            return true;
        }


    }
}
