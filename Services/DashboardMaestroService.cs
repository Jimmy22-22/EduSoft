using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    public class DashboardMaestroService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public DashboardMaestroService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Clase>> GetClasesPorProfesorAsync(string nombreProfesor)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Clases
                .Where(c => c.Profesor == nombreProfesor)
                .OrderBy(c => context.HorariosClases
                    .Where(h => h.ClaseId == c.Id)
                    .OrderBy(h => h.HoraInicio)
                    .Select(h => h.HoraInicio)
                    .FirstOrDefault())
                .ToListAsync();
        }
        
        public async Task<bool> CrearClaseAsync(string nombre, string profesor)
        {
            using var context = _contextFactory.CreateDbContext();
            var clase = new Clase 

            {
                Nombre = nombre,
                Profesor = profesor,
                CodigoClase = GenerarCodigoUnico()
            };


            context.Clases.Add(clase);
            await context.SaveChangesAsync();
            return true;
        }

        private string GenerarCodigoUnico()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

}