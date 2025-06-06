using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    /// <summary>
    /// Servicio que gestiona la lógica relacionada con el registro y consulta de asistencias de estudiantes.
    /// </summary>
    public class AsistenciaService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor del servicio de asistencia.
        /// </summary>
        /// <param name="context">Instancia del contexto de base de datos.</param>
        public AsistenciaService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene la lista de asistencias para una clase en una fecha específica.
        /// </summary>
        /// <param name="claseId">ID de la clase.</param>
        /// <param name="fecha">Fecha para consultar las asistencias.</param>
        /// <returns>Lista de asistencias con información del usuario.</returns>
        public async Task<List<AsistenciaEstudiante>> ObtenerAsistenciasPorClaseYFechaAsync(int claseId, DateTime fecha)
        {
            return await _context.AsistenciasEstudiantes
                .Include(a => a.Usuario)
                .Where(a => a.ClaseId == claseId && a.Fecha.Date == fecha.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Registra o actualiza la asistencia de los estudiantes en una clase para una fecha determinada.
        /// </summary>
        /// <param name="claseId">ID de la clase.</param>
        /// <param name="fecha">Fecha de la asistencia.</param>
        /// <param name="asistencias">Lista de tuplas con el ID del usuario y si asistió o no.</param>
        public async Task RegistrarAsistencia(int claseId, DateTime fecha, List<(int UsuarioId, bool Asistio)> asistencias)
        {
            foreach (var (usuarioId, asistio) in asistencias)
            {
                var existente = await _context.AsistenciasEstudiantes
                    .FirstOrDefaultAsync(a => a.ClaseId == claseId && a.UsuarioId == usuarioId && a.Fecha.Date == fecha.Date);

                if (existente != null)
                {
                    existente.Asistio = asistio;
                }
                else
                {
                    _context.AsistenciasEstudiantes.Add(new AsistenciaEstudiante
                    {
                        ClaseId = claseId,
                        UsuarioId = usuarioId,
                        Fecha = fecha,
                        Asistio = asistio
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene todas las asistencias registradas para un estudiante específico.
        /// </summary>
        /// <param name="estudianteId">ID del estudiante.</param>
        /// <returns>Lista de asistencias con información de la clase.</returns>
        public async Task<List<AsistenciaEstudiante>> GetAsistenciasPorEstudianteAsync(int estudianteId)
        {
            return await _context.AsistenciasEstudiantes
                .Include(a => a.Clase)
                .Where(a => a.UsuarioId == estudianteId)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }
    }
}
