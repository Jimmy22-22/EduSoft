using EduSoft.Data;
using Microsoft.EntityFrameworkCore;

namespace EduSoft.Services
{
    /// <summary>
    /// Servicio que gestiona las acciones del estudiante relacionadas con las tareas:
    /// envío, consulta, retroalimentación y calificación.
    /// </summary>
    public class TareaEstudianteService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor que inyecta el contexto de base de datos.
        /// </summary>
        /// <param name="context">Instancia de AppDbContext.</param>
        public TareaEstudianteService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Envía o actualiza una entrega de tarea del estudiante.
        /// </summary>
        /// <param name="entrega">Objeto con los datos de la entrega.</param>
        /// <returns>True si la entrega fue guardada exitosamente.</returns>
        public async Task<bool> EnviarEntregaAsync(EntregaTareaEstudiante entrega)
        {
            var existente = await _context.EntregasTareasEstudiantes
                .FirstOrDefaultAsync(e => e.TareaId == entrega.TareaId && e.UsuarioId == entrega.UsuarioId);

            if (existente != null)
            {
                existente.Comentario = entrega.Comentario;
                existente.Link = entrega.Link;
                existente.ArchivoNombre = entrega.ArchivoNombre;
                existente.ArchivoContenido = entrega.ArchivoContenido;
                existente.FechaEntrega = DateTime.Now;
            }
            else
            {
                entrega.FechaEntrega = DateTime.Now;
                _context.EntregasTareasEstudiantes.Add(entrega);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Obtiene todas las entregas realizadas para una tarea específica.
        /// </summary>
        /// <param name="tareaId">ID de la tarea.</param>
        /// <returns>Lista de entregas con información del estudiante.</returns>
        public async Task<List<EntregaTareaEstudiante>> ObtenerEntregasPorTareaAsync(int tareaId)
        {
            return await _context.EntregasTareasEstudiantes
                .Where(e => e.TareaId == tareaId)
                .Include(e => e.Usuario)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene la entrega específica de un estudiante para una tarea.
        /// </summary>
        /// <param name="tareaId">ID de la tarea.</param>
        /// <param name="usuarioId">ID del estudiante.</param>
        /// <returns>Entrega del estudiante o null si no existe.</returns>
        public async Task<EntregaTareaEstudiante?> ObtenerEntregaEstudianteAsync(int tareaId, int usuarioId)
        {
            return await _context.EntregasTareasEstudiantes
                .FirstOrDefaultAsync(e => e.TareaId == tareaId && e.UsuarioId == usuarioId);
        }

        /// <summary>
        /// Actualiza la retroalimentación y la nota de una entrega específica.
        /// </summary>
        /// <param name="entrega">Objeto con la retroalimentación y nota actualizadas.</param>
        /// <returns>True si se actualizó correctamente, false si no se encontró la entrega.</returns>
        public async Task<bool> ActualizarRetroalimentacionYNotaAsync(EntregaTareaEstudiante entrega)
        {
            var existente = await _context.EntregasTareasEstudiantes
                .FirstOrDefaultAsync(e => e.Id == entrega.Id);

            if (existente != null)
            {
                existente.Retroalimentacion = entrega.Retroalimentacion;
                existente.Nota = entrega.Nota;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
