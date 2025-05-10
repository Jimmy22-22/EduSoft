using EduSoft.Data;
using Microsoft.EntityFrameworkCore;

namespace EduSoft.Services
{
    /// <summary>
    /// Servicio que maneja las funcionalidades del dashboard del estudiante:
    /// obtener clases, tareas y unirse a una clase.
    /// </summary>
    public class DashboardEstudianteService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        /// <summary>
        /// Constructor que recibe el factory del contexto para acceder a la base de datos.
        /// </summary>
        /// <param name="contextFactory">Factory para crear el contexto de EF.</param>
        public DashboardEstudianteService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Devuelve todas las clases a las que está inscrito un estudiante.
        /// Se ordenan por el horario más temprano de cada clase (si existe).
        /// </summary>
        /// <param name="usuarioId">ID del estudiante.</param>
        /// <returns>Lista de clases.</returns>
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

        /// <summary>
        /// Devuelve todas las tareas asignadas al estudiante,
        /// ordenadas por su fecha de entrega más próxima.
        /// </summary>
        /// <param name="usuarioId">ID del estudiante.</param>
        /// <returns>Lista de tareas pendientes.</returns>
        public async Task<List<Tarea>> GetTareasPorEstudianteAsync(int usuarioId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tareas
                .Where(t => t.UsuarioId == usuarioId)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();
        }

        /// <summary>
        /// Permite que el estudiante se una a una clase mediante el código de clase.
        /// Verifica que el código exista y que no esté ya inscrito.
        /// </summary>
        /// <param name="usuarioId">ID del estudiante.</param>
        /// <param name="codigoClase">Código proporcionado de la clase.</param>
        /// <returns>True si se unió correctamente, false si ya estaba inscrito o no existe la clase.</returns>
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
