using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EduSoft.Services
{
    /// <summary>
    /// Servicio que construye un contexto textual completo para un estudiante,
    /// utilizado por Edu AI para responder con base en los datos académicos reales del usuario.
    /// </summary>
    public class EduAiContextEstudianteBuilder
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor que inyecta el contexto de base de datos.
        /// </summary>
        /// <param name="context">Instancia de <see cref="AppDbContext"/> para acceder a los datos.</param>
        public EduAiContextEstudianteBuilder(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Construye un contexto personalizado para un estudiante específico, incluyendo
        /// sus clases inscritas, horarios, tareas entregadas, notas y asistencias.
        /// </summary>
        /// <param name="estudianteId">ID del estudiante.</param>
        /// <returns>Un string descriptivo con toda la información académica relevante del estudiante.</returns>
        public async Task<string> ConstruirContextoEstudianteAsync(int estudianteId)
        {
            var usuario = await _context.Usuarios.FindAsync(estudianteId);
            if (usuario == null) return "";

            var clases = await _context.UsuarioClases
                .Where(uc => uc.UsuarioId == estudianteId)
                .Include(uc => uc.Clase)
                .ThenInclude(c => c.Horarios)
                .ToListAsync();

            var tareasEntregadas = await _context.EntregasTareasEstudiantes
                .Where(e => e.UsuarioId == estudianteId)
                .Include(e => e.Tarea)
                .ThenInclude(t => t.Clase)
                .ToListAsync();

            var asistencias = await _context.AsistenciasEstudiantes
                .Where(a => a.UsuarioId == estudianteId)
                .Include(a => a.Clase)
                .ToListAsync();

            var sb = new StringBuilder();

            sb.AppendLine($"Estudiante: {usuario.Nombre} ({usuario.Email})\n");

            sb.AppendLine("📚 Clases inscritas:");
            foreach (var uc in clases)
            {
                var clase = uc.Clase;
                sb.AppendLine($"- {clase.Nombre} (Profesor: {clase.Profesor})");

                if (clase.Horarios.Any())
                {
                    sb.AppendLine("  🕒 Horarios:");
                    foreach (var horario in clase.Horarios.OrderBy(h => h.Fecha).ThenBy(h => h.HoraInicio))
                    {
                        sb.AppendLine($"    - {horario.Fecha:dd/MM/yyyy} {horario.HoraInicio}-{horario.HoraFin} Aula: {horario.Aula}");
                    }
                }
            }

            sb.AppendLine("\n📝 Tareas entregadas:");
            foreach (var entrega in tareasEntregadas.OrderByDescending(e => e.FechaEntrega))
            {
                sb.AppendLine($"- {entrega.Tarea.Titulo} ({entrega.Tarea.Clase.Nombre})");
                sb.AppendLine($"  📅 Entregada: {entrega.FechaEntrega:dd/MM/yyyy} | Nota: {(entrega.Nota.HasValue ? entrega.Nota.Value.ToString("0.0") : "N/A")}");
                if (!string.IsNullOrWhiteSpace(entrega.Retroalimentacion))
                {
                    sb.AppendLine($"  💬 Retroalimentación: {entrega.Retroalimentacion}");
                }
            }

            sb.AppendLine("\n🗓️ Asistencias:");
            foreach (var a in asistencias.OrderByDescending(a => a.Fecha))
            {
                sb.AppendLine($"- {a.Fecha:dd/MM/yyyy} | {a.Clase.Nombre} | {(a.Asistio ? "✅ Asistió" : "❌ Faltó")}");
            }

            return sb.ToString();
        }
    }
}
