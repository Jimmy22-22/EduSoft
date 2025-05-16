using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    /// <summary>
    /// Servicio que maneja la lógica del panel principal para los maestros.
    /// Permite obtener las clases creadas por el docente y registrar nuevas clases.
    /// </summary>
    public class DashboardMaestroService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        /// <summary>
        /// Constructor que recibe el factory para crear el contexto de base de datos.
        /// </summary>
        /// <param name="contextFactory">Factory de EF Core.</param>
        public DashboardMaestroService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Obtiene todas las clases creadas por un profesor específico.
        /// Las clases se ordenan por el horario más temprano asignado (si existe).
        /// </summary>
        /// <param name="nombreProfesor">Nombre del profesor.</param>
        /// <returns>Lista de clases ordenadas por hora de inicio.</returns>
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

        /// <summary>
        /// Registra una nueva clase en la base de datos para un profesor.
        /// Se genera automáticamente un código único para la clase.
        /// </summary>
        /// <param name="nombre">Nombre de la clase.</param>
        /// <param name="profesor">Nombre del profesor que la crea.</param>
        /// <returns>True si la clase se creó exitosamente.</returns>
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

        /// <summary>
        /// Genera un código único de 6 caracteres para identificar una clase.
        /// </summary>
        /// <returns>Cadena aleatoria de 6 caracteres.</returns>
        private string GenerarCodigoUnico()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<List<string>> ObtenerNotificacionesMaestroAsync(string nombreMaestro)
        {
            using var context = _contextFactory.CreateDbContext();
            var notificaciones = new List<string>();

            var entregas = await context.EntregasTareasEstudiantes
                .Include(e => e.Usuario)
                .Include(e => e.Tarea)
                .ThenInclude(t => t.Clase)
                .Where(e => e.Tarea.Usuario.Nombre == nombreMaestro)
                .OrderByDescending(e => e.FechaEntrega)
                .ToListAsync();

            foreach (var entrega in entregas)
            {
                if (entrega.Usuario != null && entrega.Tarea != null && entrega.Tarea.Clase != null)
                {
                    var esExamen = entrega.Tarea.EsExamen;
                    var tipo = esExamen ? "examen" : "tarea";
                    var articulo = esExamen ? "el" : "la";

                    notificaciones.Add($"El estudiante {entrega.Usuario.Nombre} entregó {articulo} {tipo} {entrega.Tarea.Titulo} de la clase {entrega.Tarea.Clase.Nombre}.");
                }
            }

            return notificaciones;
        }
    }
}
