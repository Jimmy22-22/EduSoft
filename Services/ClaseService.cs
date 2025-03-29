using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    public class ClaseService
    {
        private readonly AppDbContext _context;
        public ClaseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CrearClase(string nombre, string profesor)
        {
            string codigoClase = GenerarCodigoUnico();

            var clase = new Clase
            {
                Nombre = nombre,
                Profesor = profesor,
                CodigoClase = codigoClase
            };

            _context.Clases.Add(clase);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnirseAClase(int usuarioId, string codigoClase)
        {
            var clase = await _context.Clases.FirstOrDefaultAsync(c => c.CodigoClase == codigoClase);
            if (clase == null)
                return false;

            bool yaInscrito = await _context.UsuarioClases
                .AnyAsync(uc => uc.UsuarioId == usuarioId && uc.ClaseId == clase.Id);

            if (yaInscrito)
                return false;

            var usuarioClase = new UsuarioClase
            {
                UsuarioId = usuarioId,
                ClaseId = clase.Id
            };

            _context.UsuarioClases.Add(usuarioClase);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Clase?> GetClasePorIdAsync(int claseId)
        {
            return await _context.Clases.FirstOrDefaultAsync(c => c.Id == claseId);
        }

        public async Task<List<Usuario>> GetEstudiantesPorClaseAsync(int claseId)
        {
            return await _context.UsuarioClases
                .Where(uc => uc.ClaseId == claseId)
                .Select(uc => uc.Usuario)
                .ToListAsync();
        }

        public async Task<List<Tarea>> GetTareasPorClaseAsync(int claseId)
        {
            return await _context.Tareas
                .Where(t => t.ClaseId == claseId)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();
        }

        public async Task<bool> CrearTarea(int claseId, string descripcion, DateTime fechaEntrega, int usuarioId)
        {
            var tarea = new Tarea
            {
                Descripcion = descripcion,
                FechaEntrega = fechaEntrega,
                ClaseId = claseId,
                UsuarioId = usuarioId
            };

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<HorarioClase?> GetHorarioPorClaseAsync(int claseId)
        {
            return await _context.HorariosClases
                .Where(h => h.ClaseId == claseId)
                .OrderBy(h => h.HoraInicio)
                .FirstOrDefaultAsync();
        }

        private string GenerarCodigoUnico()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
