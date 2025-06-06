using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EduSoft.Pages.NotaEstudiante;

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
        public async Task<bool> UnirseAClase(int usuarioId, string codigoClase)
        {
            var clase = await _context.Clases.FirstOrDefaultAsync(c => c.CodigoClase == codigoClase);
            if (clase == null)
                return false;

            bool yaInscrito = await _context.UsuarioClases.AnyAsync(uc => uc.UsuarioId == usuarioId && uc.ClaseId == clase.Id);
            if (yaInscrito)
                return false;

            var usuarioClase = new UsuarioClase { UsuarioId = usuarioId, ClaseId = clase.Id };

            _context.UsuarioClases.Add(usuarioClase);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Obtiene una clase por su ID.
        /// </summary>
        public async Task<Clase?> GetClasePorIdAsync(int claseId)
        {
            return await _context.Clases.FirstOrDefaultAsync(c => c.Id == claseId);
        }

        /// <summary>
        /// Obtiene la lista de estudiantes inscritos en una clase.
        /// </summary>
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
        public async Task<List<Tarea>> GetTareasPorClaseAsync(int claseId)
        {
            return await _context.Tareas
                .Where(t => t.ClaseId == claseId)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();
        }

        /// <summary>
        /// Crea una tarea para una clase específica.
        /// </summary>
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
        /// Obtiene el primer horario asignado a una clase.
        /// </summary>
        public async Task<HorarioClase?> GetHorarioPorClaseAsync(int claseId)
        {
            return await _context.HorariosClases
                .Where(h => h.ClaseId == claseId)
                .OrderBy(h => h.HoraInicio)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Genera un código alfanumérico único de 6 caracteres para una clase.
        /// </summary>
        private string GenerarCodigoUnico()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Obtiene una tarea por su ID.
        /// </summary>
        public async Task<Tarea?> GetTareaPorIdAsync(int tareaId)
        {
            return await _context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId);
        }

        /// <summary>
        /// Obtiene todas las clases de un profesor.
        /// </summary>
        public async Task<List<Clase>> GetClasesPorProfesorAsync(string nombreProfesor)
        {
            return await _context.Clases
                .Where(c => c.Profesor == nombreProfesor)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todas las notas de los estudiantes en una clase.
        /// </summary>
        public async Task<List<NotaEstudianteDto>> ObtenerNotasPorClaseAsync(int claseId)
        {
            var tareas = await _context.Tareas.Where(t => t.ClaseId == claseId).ToListAsync();
            var estudiantes = await _context.UsuarioClases.Where(uc => uc.ClaseId == claseId).Select(uc => uc.Usuario).ToListAsync();
            var notas = await _context.EntregasTareasEstudiantes.Where(e => tareas.Select(t => t.Id).Contains(e.TareaId)).ToListAsync();

            var resultado = new List<NotaEstudianteDto>();

            foreach (var estudiante in estudiantes)
            {
                var notasEstudiante = notas.Where(n => n.UsuarioId == estudiante.Id)
                    .Select(n => new NotaTareaDto
                    {
                        TareaId = n.TareaId,
                        TituloTarea = tareas.First(t => t.Id == n.TareaId).Titulo,
                        Nota = n.Nota
                    }).ToList();

                resultado.Add(new NotaEstudianteDto
                {
                    EstudianteId = estudiante.Id,
                    NombreEstudiante = estudiante.Nombre,
                    Notas = notasEstudiante
                });
            }

            return resultado;
        }

        /// <summary>
        /// Obtiene todas las clases disponibles.
        /// </summary>
        public async Task<List<Clase>> ObtenerClasesAsync()
        {
            return await _context.Clases.OrderBy(c => c.Nombre).ToListAsync();
        }

        /// <summary>
        /// Obtiene las tareas de una clase.
        /// </summary>
        public async Task<List<Tarea>> ObtenerTareasPorClaseAsync(int claseId)
        {
            return await _context.Tareas
                .Where(t => t.ClaseId == claseId)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();
        }

        /// <summary>
        /// Actualiza la nota de una entrega.
        /// </summary>
        public async Task ActualizarNotaAsync(EntregaTareaEstudiante entrega)
        {
            var entregaExistente = await _context.EntregasTareasEstudiantes.FirstOrDefaultAsync(e => e.Id == entrega.Id);
            if (entregaExistente != null)
            {
                entregaExistente.Nota = entrega.Nota;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Obtiene todas las entregas asociadas a una tarea.
        /// </summary>
        public async Task<List<EntregaTareaEstudiante>> ObtenerEntregasPorTareaAsync(int tareaId)
        {
            return await _context.EntregasTareasEstudiantes
                .Where(e => e.TareaId == tareaId)
                .Include(e => e.Usuario)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene las tareas de una clase que no son exámenes.
        /// </summary>
        public async Task<List<Tarea>> ObtenerTareasPorClaseSinExamenAsync(int claseId)
        {
            return await _context.Tareas
                .Where(t => t.ClaseId == claseId && !t.EsExamen)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene los exámenes de una clase.
        /// </summary>
        public async Task<List<Tarea>> ObtenerExamenesPorClaseAsync(int claseId)
        {
            return await _context.Tareas
                .Where(t => t.ClaseId == claseId && t.EsExamen)
                .OrderByDescending(t => t.FechaEntrega)
                .ToListAsync();
        }

        /// <summary>
        /// Guarda la nota y retroalimentación de una entrega.
        /// </summary>
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

        /// <summary>
        /// Obtiene las notas agrupadas por clase de un estudiante.
        /// </summary>
        public async Task<List<NotasClaseDto>> ObtenerNotasPorEstudianteAsync(int estudianteId)
        {
            var clases = await _context.UsuarioClases.Where(uc => uc.UsuarioId == estudianteId).Include(uc => uc.Clase).ToListAsync();
            var entregas = await _context.EntregasTareasEstudiantes.Where(e => e.UsuarioId == estudianteId).ToListAsync();
            var tareas = await _context.Tareas.ToListAsync();

            var resultado = new List<NotasClaseDto>();

            foreach (var clase in clases)
            {
                var tareasClase = tareas.Where(t => t.ClaseId == clase.ClaseId).ToList();
                var notas = tareasClase.Select(t => {
                    var entrega = entregas.FirstOrDefault(e => e.TareaId == t.Id);
                    return new NotaTareaDto
                    {
                        TareaId = t.Id,
                        TituloTarea = t.Titulo,
                        Nota = entrega?.Nota
                    };
                }).ToList();

                resultado.Add(new NotasClaseDto
                {
                    NombreClase = clase.Clase.Nombre,
                    Notas = notas
                });
            }

            return resultado;
        }

        /// <summary>
        /// Obtiene las clases en las que está inscrito un estudiante.
        /// </summary>
        public async Task<List<Clase>> GetClasesPorEstudianteAsync(int usuarioId)
        {
            return await _context.UsuarioClases
                .Where(uc => uc.UsuarioId == usuarioId)
                .Select(uc => uc.Clase)
                .ToListAsync();
        }
    }
}