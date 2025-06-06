using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EduSoft.Services
{
    /// <summary>
    /// Servicio que construye un contexto personalizado para los maestros,
    /// con información detallada sobre sus clases, tareas, entregas, asistencias y horarios.
    /// Utilizado por Edu AI para proporcionar respuestas informadas y personalizadas.
    /// </summary>
    public class EduAiContextMaestroBuilder
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor que recibe el contexto de base de datos.
        /// </summary>
        /// <param name="context">Instancia de <see cref="AppDbContext"/>.</param>
        public EduAiContextMaestroBuilder(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Construye un resumen textual completo con todos los datos relevantes
        /// del maestro: clases que imparte, tareas asignadas, entregas recibidas,
        /// asistencias registradas y horarios programados.
        /// </summary>
        /// <param name="nombreMaestro">Nombre del maestro.</param>
        /// <returns>Texto plano con información completa del contexto académico del docente.</returns>
        public async Task<string> ConstruirContextoMaestroAsync(string nombreMaestro)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Datos del maestro: {nombreMaestro}\n");

            // Clases creadas por el maestro
            var clases = await _context.Clases
                .Where(c => c.Profesor == nombreMaestro)
                .ToListAsync();

            sb.AppendLine("Clases creadas:");
            foreach (var clase in clases)
                sb.AppendLine($"- {clase.Nombre} (Código: {clase.CodigoClase})");

            // Tareas asignadas por el maestro
            var tareas = await _context.Tareas
                .Where(t => t.Usuario.Nombre == nombreMaestro)
                .Include(t => t.Clase)
                .ToListAsync();

            sb.AppendLine("\nTareas asignadas:");
            foreach (var tarea in tareas)
            {
                var tipo = tarea.EsExamen ? "[Examen]" : "[Tarea]";
                sb.AppendLine($"- {tipo} {tarea.Titulo} | Clase: {tarea.Clase.Nombre} | Entrega: {tarea.FechaEntrega:dd/MM/yyyy}");
            }

            // Entregas realizadas por estudiantes en tareas del maestro
            var entregas = await _context.EntregasTareasEstudiantes
                .Include(e => e.Tarea)
                .Include(e => e.Usuario)
                .Where(e => e.Tarea.Usuario.Nombre == nombreMaestro)
                .ToListAsync();

            sb.AppendLine("\nEntregas de estudiantes:");
            foreach (var entrega in entregas)
            {
                sb.AppendLine($"- {entrega.Usuario.Nombre} entregó '{entrega.Tarea.Titulo}' el {entrega.FechaEntrega:dd/MM/yyyy} | Nota: {entrega.Nota?.ToString("0.0") ?? "N/A"}");
            }

            // Asistencias registradas en clases del maestro
            var asistencias = await _context.AsistenciasEstudiantes
                .Include(a => a.Clase)
                .Where(a => a.Clase.Profesor == nombreMaestro)
                .ToListAsync();

            sb.AppendLine("\nResumen de asistencias registradas:");
            foreach (var grupo in asistencias.GroupBy(a => a.Clase.Nombre))
            {
                var total = grupo.Count();
                var presentes = grupo.Count(a => a.Asistio);
                sb.AppendLine($"- {grupo.Key}: {presentes}/{total} asistencias");
            }

            // Horarios asignados al maestro
            var horarios = await _context.HorariosClases
                .Include(h => h.Clase)
                .Where(h => h.Profesor == nombreMaestro)
                .OrderBy(h => h.Fecha)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();

            sb.AppendLine("\nHorarios de clase:");
            foreach (var h in horarios)
            {
                sb.AppendLine($"- {h.Fecha:dd/MM/yyyy} | {h.Clase.Nombre} | {h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm} | Aula: {h.Aula}");
            }

            return sb.ToString();
        }
    }
}
