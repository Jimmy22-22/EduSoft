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

        /// <summary>
        /// Obtiene notificaciones relacionadas a tareas próximas, calificaciones, asistencias y nuevos horarios.
        /// </summary>
        public async Task<List<string>> ObtenerNotificacionesAsync(int estudianteId)
        {
            using var context = _contextFactory.CreateDbContext();
            var notificaciones = new List<string>();

            var entregas = await context.EntregasTareasEstudiantes
                .Include(e => e.Tarea)
                .ThenInclude(t => t.Clase)
                .Where(e => e.UsuarioId == estudianteId)
                .ToListAsync();

            foreach (var entrega in entregas)
            {
                if (entrega.Nota != null)
                {
                    notificaciones.Add($"Recibiste una nota de {entrega.Nota}/100 en la tarea '{entrega.Tarea.Titulo}' de {entrega.Tarea.Clase.Nombre}.");
                }
                else if ((entrega.Tarea.FechaEntrega - DateTime.Now).TotalDays <= 1 && entrega.FechaEntrega == default)
                {
                    notificaciones.Add($"¡La tarea '{entrega.Tarea.Titulo}' de {entrega.Tarea.Clase.Nombre} vence mañana!");
                }
            }

            var asistencias = await context.AsistenciasEstudiantes
                .Include(a => a.Clase)
                .Where(a => a.UsuarioId == estudianteId && !a.Asistio)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            foreach (var falta in asistencias)
            {
                notificaciones.Add($"Faltaste el {falta.Fecha:dd/MM/yyyy} a la clase de {falta.Clase.Nombre}.");
            }

            var fechaLimite = DateTime.Now.AddDays(-2);

            var clasesDelEstudiante = await context.UsuarioClases
                .Where(uc => uc.UsuarioId == estudianteId)
                .Select(uc => uc.ClaseId)
                .ToListAsync();

            var nuevosHorarios = await context.HorariosClases
                .Include(h => h.Clase)
                .Where(h => clasesDelEstudiante.Contains(h.ClaseId) && h.Fecha >= fechaLimite)
                .ToListAsync();

            foreach (var horario in nuevosHorarios)
            {
                notificaciones.Add($"Nuevo horario publicado para {horario.Clase.Nombre}: {horario.Fecha:dd/MM/yyyy} de {horario.HoraInicio} a {horario.HoraFin} en Aula {horario.Aula}.");
            }

            return notificaciones;
        }
    }
}
