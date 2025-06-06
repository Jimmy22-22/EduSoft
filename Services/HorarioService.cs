using EduSoft.Data;
using Microsoft.EntityFrameworkCore;

namespace EduSoft.Services
{
    /// <summary>
    /// Servicio responsable de gestionar los horarios de clase.
    /// Incluye funcionalidades para consultar, agregar, editar y eliminar horarios.
    /// </summary>
    public class HorarioService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        /// <summary>
        /// Constructor que recibe el factory para crear instancias del contexto de base de datos.
        /// </summary>
        /// <param name="contextFactory">Factory de EF Core.</param>
        public HorarioService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Obtiene la lista de horarios registrados por un maestro. 
        /// Se puede filtrar por una fecha específica si se desea.
        /// </summary>
        /// <param name="nombreProfesor">Nombre del profesor que creó los horarios.</param>
        /// <param name="fecha">Fecha opcional para filtrar los horarios.</param>
        /// <returns>Lista de horarios ordenados por fecha y hora.</returns>
        public async Task<List<HorarioClase>> GetHorariosPorMaestroAsync(string nombreProfesor, DateTime? fecha = null)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.HorariosClases
                .Include(h => h.Clase)
                .Where(h => h.Profesor == nombreProfesor);

            if (fecha.HasValue)
            {
                query = query.Where(h => h.Fecha.Date == fecha.Value.Date);
            }

            return await query.OrderBy(h => h.Fecha).ThenBy(h => h.HoraInicio).ToListAsync();
        }

        /// <summary>
        /// Devuelve la lista de clases creadas por un profesor específico.
        /// </summary>
        /// <param name="nombreProfesor">Nombre del profesor.</param>
        /// <returns>Lista de clases.</returns>
        public async Task<List<Clase>> GetClasesPorMaestroAsync(string nombreProfesor)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Clases
                .Where(c => c.Profesor == nombreProfesor)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Registra un nuevo horario para una clase existente.
        /// </summary>
        /// <param name="claseId">ID de la clase a la que pertenece el horario.</param>
        /// <param name="fecha">Fecha del horario.</param>
        /// <param name="horaInicio">Hora de inicio.</param>
        /// <param name="horaFin">Hora de finalización.</param>
        /// <param name="aula">Nombre o número del aula.</param>
        /// <param name="profesor">Nombre del profesor asignado.</param>
        /// <returns>True si el horario fue agregado con éxito.</returns>
        public async Task<bool> AgregarHorarioAsync(int claseId, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin, string aula, string profesor)
        {
            using var context = _contextFactory.CreateDbContext();

            var clase = await context.Clases.FindAsync(claseId);
            if (clase == null) return false;

            var nuevoHorario = new HorarioClase
            {
                ClaseId = claseId,
                Fecha = fecha,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                Aula = aula,
                Profesor = profesor
            };

            context.HorariosClases.Add(nuevoHorario);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Permite modificar los datos de un horario ya existente.
        /// </summary>
        /// <param name="horarioId">ID del horario a modificar.</param>
        /// <param name="claseId">Nuevo ID de clase si se desea cambiar.</param>
        /// <param name="fecha">Nueva fecha del horario.</param>
        /// <param name="horaInicio">Nueva hora de inicio.</param>
        /// <param name="horaFin">Nueva hora de finalización.</param>
        /// <param name="aula">Nuevo nombre o número de aula.</param>
        /// <returns>True si la edición fue exitosa.</returns>
        public async Task<bool> EditarHorarioAsync(int horarioId, int claseId, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin, string aula)
        {
            using var context = _contextFactory.CreateDbContext();

            var horario = await context.HorariosClases.FindAsync(horarioId);
            if (horario == null) return false;

            horario.ClaseId = claseId;
            horario.Fecha = fecha;
            horario.HoraInicio = horaInicio;
            horario.HoraFin = horaFin;
            horario.Aula = aula;

            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Elimina un horario específico de la base de datos.
        /// </summary>
        /// <param name="horarioId">ID del horario a eliminar.</param>
        /// <returns>True si el horario fue eliminado con éxito.</returns>
        public async Task<bool> EliminarHorarioAsync(int horarioId)
        {
            using var context = _contextFactory.CreateDbContext();

            var horario = await context.HorariosClases.FindAsync(horarioId);
            if (horario == null) return false;

            context.HorariosClases.Remove(horario);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Devuelve el primer horario asignado a una clase (el más temprano).
        /// </summary>
        /// <param name="claseId">ID de la clase.</param>
        /// <returns>Horario correspondiente, o null si no hay asignado.</returns>
        public async Task<HorarioClase?> GetHorarioPorClaseAsync(int claseId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.HorariosClases
                .Where(h => h.ClaseId == claseId)
                .OrderBy(h => h.HoraInicio)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtiene todos los horarios de las clases en las que el estudiante está inscrito.
        /// </summary>
        /// <param name="estudianteId">ID del estudiante.</param>
        /// <returns>Lista de horarios con detalle de clase.</returns>
        public async Task<List<HorarioClase>> GetHorariosPorEstudianteAsync(int estudianteId)
        {
            using var context = _contextFactory.CreateDbContext();

            var claseIds = await context.UsuarioClases
                .Where(uc => uc.UsuarioId == estudianteId)
                .Select(uc => uc.ClaseId)
                .ToListAsync();

            return await context.HorariosClases
                .Include(h => h.Clase)
                .Where(h => claseIds.Contains(h.ClaseId))
                .OrderBy(h => h.Fecha)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();
        }
    }
}
