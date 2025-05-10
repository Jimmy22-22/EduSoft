using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    /// <summary>
    /// Servicio encargado de gestionar la lógica relacionada con las clases,
    /// como la creación, inscripción, asignación de tareas y horarios.
    /// </summary>
    public class ClaseService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa una nueva instancia del <see cref="ClaseService"/>.
        /// </summary>
        /// <param name="context">El contexto de base de datos inyectado.</param>
        public ClaseService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea una nueva clase con un código único y asigna el nombre del profesor.
        /// </summary>
        /// <param name="nombre">Nombre de la clase.</param>
        /// <param name="profesor">Nombre del profesor.</param>
        /// <returns>True si la clase fue creada correctamente.</returns>
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

        /// <summary>
        /// Permite que un usuario se una a una clase usando un código.
        /// </summary>
        /// <param name="usuarioId">ID del usuario.</param>
        /// <param name="codigoClase">Código único de la clase.</param>
        /// <returns>True si el usuario se unió exitosamente, false si ya estaba inscrito o no existe la clase.</returns>
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

        /// <summary>
        /// Obtiene una clase por su ID.
        /// </summary>
        /// <param name="claseId">ID de la clase.</param>
        /// <returns>La clase encontrada o null si no existe.</returns>
        public async Task<Clase?> GetClasePorIdAsync(int claseId)
        {
            return await _context.Clases.FirstOrDefaultAsync(c => c.Id == claseId);
        }

        /// <summary>
        /// Obtiene la lista de estudiantes inscritos en una clase.
        /// </summary>
        /// <param name="claseId">ID de la clase.</param>
        /// <returns>Lista de estudiantes inscritos.</returns>
        public async Task<List<Usuario>> GetEstudiantesPorClaseAsync(int claseId)
        {
            return await _context.UsuarioClases
                .Where(uc => uc.ClaseId == claseId)
                .Select(uc => uc.Usuario)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene la lista de tareas asociadas a una clase ordenadas por fecha de entrega.
        /// </summary>
        /// <param name="claseId">ID de la clase.</param>
        /// <returns>Lista de tareas.</returns>
        public async Task<List<Tarea>> GetTareasPorClaseAsync(int claseId)
        {
            return await _context.Tareas
                .Where(t => t.ClaseId == claseId)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();
        }

        /// <summary>
        /// Crea una tarea para una clase específica y asigna un usuario responsable.
        /// </summary>
        /// <param name="claseId">ID de la clase.</param>
        /// <param name="descripcion">Descripción de la tarea.</param>
        /// <param name="fechaEntrega">Fecha de entrega de la tarea.</param>
        /// <param name="usuarioId">ID del usuario que la crea.</param>
        /// <returns>True si la tarea fue creada correctamente.</returns>
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

        /// <summary>
        /// Obtiene el primer horario asignado a una clase, ordenado por hora de inicio.
        /// </summary>
        /// <param name="claseId">ID de la clase.</param>
        /// <returns>Un objeto <see cref="HorarioClase"/> o null si no existe.</returns>
        public async Task<HorarioClase?> GetHorarioPorClaseAsync(int claseId)
        {
            return await _context.HorariosClases
                .Where(h => h.ClaseId == claseId)
                .OrderBy(h => h.HoraInicio)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Genera un código alfanumérico único de 6 caracteres para una nueva clase.
        /// </summary>
        /// <returns>Un string que representa el código generado.</returns>
        private string GenerarCodigoUnico()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
