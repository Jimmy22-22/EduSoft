using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace EduSoft.Services
{
    public class AsistenciaService
    {
        private readonly AppDbContext _context;

        public AsistenciaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AsistenciaEstudiante>> ObtenerAsistenciasPorClaseYFechaAsync(int claseId, DateTime fecha)
        {
            return await _context.AsistenciasEstudiantes
                .Include(a => a.Usuario)
                .Where(a => a.ClaseId == claseId && a.Fecha.Date == fecha.Date)
                .ToListAsync();
        }

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
