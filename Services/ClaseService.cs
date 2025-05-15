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
        public async Task<bool> CrearTarea(int claseId, string titulo, string descripcion, DateTime fechaEntrega, int usuarioId, string? link, string? archivoNombre, byte[]? archivoContenido, bool esExamen)
        {
            var tarea = new Tarea
            {
                Titulo = titulo,
                Descripcion = descripcion,
                FechaEntrega = fechaEntrega,
                UsuarioId = usuarioId,
                ClaseId = claseId,
                Link = link,
                ArchivoNombre = archivoNombre,
                ArchivoContenido = archivoContenido,
                EsExamen = esExamen
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

        /// <summary>
        /// Obtiene una tarea por su ID con todos sus datos (título, descripción, enlace y archivo).
        /// </summary>
        /// <param name="tareaId">ID de la tarea.</param>
        /// <returns>La tarea encontrada o null si no existe.</returns>
        public async Task<Tarea?> GetTareaPorIdAsync(int tareaId)
        {
            return await _context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId);
        }

        /// <summary>
        /// Obtiene todas las clases creadas por un profesor específico (por su nombre).
        /// </summary>
        /// <param name="nombreProfesor">Nombre del profesor.</param>
        /// <returns>Lista de clases creadas por ese profesor.</returns>
        public async Task<List<Clase>> GetClasesPorProfesorAsync(string nombreProfesor)
        {
            return await _context.Clases
                .Where(c => c.Profesor == nombreProfesor)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<List<NotaEstudianteDto>> ObtenerNotasPorClaseAsync(int claseId)
        {
            var tareas = await _context.Tareas
                .Where(t => t.ClaseId == claseId)
                .ToListAsync();

            var estudiantes = await _context.UsuarioClases
                .Where(uc => uc.ClaseId == claseId)
                .Select(uc => uc.Usuario)
                .ToListAsync();

            var notas = await _context.EntregasTareasEstudiantes
                .Where(e => tareas.Select(t => t.Id).Contains(e.TareaId))
                .ToListAsync();

            var resultado = new List<NotaEstudianteDto>();

            foreach (var estudiante in estudiantes)
            {
                var notasEstudiante = notas
                    .Where(n => n.UsuarioId == estudiante.Id)
                    .Select(n => new NotaTareaDto
                    {
                        TareaId = n.TareaId,
                        TituloTarea = tareas.First(t => t.Id == n.TareaId).Titulo,
                        Nota = n.Nota
                    })
                    .ToList();

                resultado.Add(new NotaEstudianteDto
                {
                    EstudianteId = estudiante.Id,
                    NombreEstudiante = estudiante.Nombre,
                    Notas = notasEstudiante
                });
            }

            return resultado;
        }

        public async Task<List<Clase>> ObtenerClasesAsync()
        {
            return await _context.Clases
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<List<Tarea>> ObtenerTareasPorClaseAsync(int claseId)
        {
            return await _context.Tareas
                .Where(t => t.ClaseId == claseId)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();
        }

        public async Task ActualizarNotaAsync(EntregaTareaEstudiante entrega)
        {
            var entregaExistente = await _context.EntregasTareasEstudiantes
                .FirstOrDefaultAsync(e => e.Id == entrega.Id);

            if (entregaExistente != null)
            {
                entregaExistente.Nota = entrega.Nota;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<EntregaTareaEstudiante>> ObtenerEntregasPorTareaAsync(int tareaId)
        {
            return await _context.EntregasTareasEstudiantes
                .Where(e => e.TareaId == tareaId)
                .Include(e => e.Usuario)
                .ToListAsync();
        }

        public async Task<List<Tarea>> ObtenerTareasPorClaseSinExamenAsync(int claseId)
        {
            return await _context.Tareas
                .Where(t => t.ClaseId == claseId && !t.EsExamen)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();
        }

        public async Task<List<Tarea>> ObtenerExamenesPorClaseAsync(int claseId)
        {
            return await _context.Tareas
                .Where(t => t.ClaseId == claseId && t.EsExamen)
                .OrderByDescending(t => t.FechaEntrega)
                .ToListAsync();
        }

        public async Task GuardarNotaYRetroalimentacion(int entregaId, decimal? nota, string? retroalimentacion)
        {
            var entrega = await _context.EntregasTareasEstudiantes.FindAsync(entregaId);
            if (entrega != null)
            {
                entrega.Nota = nota;
                entrega.Retroalimentacion = retroalimentacion;
                await _context.SaveChangesAsync();
            }
        }



    }
}
