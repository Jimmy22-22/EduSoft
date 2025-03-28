using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    public class HorarioService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public HorarioService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

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

        public async Task<List<Clase>> GetClasesPorMaestroAsync(string nombreProfesor)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Clases
                .Where(c => c.Profesor == nombreProfesor)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

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

        public async Task<bool> EliminarHorarioAsync(int horarioId)
        {
            using var context = _contextFactory.CreateDbContext();

            var horario = await context.HorariosClases.FindAsync(horarioId);
            if (horario == null) return false;

            context.HorariosClases.Remove(horario);
            await context.SaveChangesAsync();

            return true;
        }
        public async Task<HorarioClase?> GetHorarioPorClaseAsync(int claseId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.HorariosClases
                .Where(h => h.ClaseId == claseId)
                .OrderBy(h => h.HoraInicio)
                .FirstOrDefaultAsync();
        }
    }
}
