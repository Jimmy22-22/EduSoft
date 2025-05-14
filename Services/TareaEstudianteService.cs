using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    public class TareaEstudianteService
    {
        private readonly AppDbContext _context;

        public TareaEstudianteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> EnviarEntregaAsync(EntregaTareaEstudiante entrega)
        {
            var existente = await _context.EntregasTareasEstudiantes
                .FirstOrDefaultAsync(e => e.TareaId == entrega.TareaId && e.UsuarioId == entrega.UsuarioId);

            if (existente != null)
            {
                existente.Comentario = entrega.Comentario;
                existente.Link = entrega.Link;
                existente.ArchivoNombre = entrega.ArchivoNombre;
                existente.ArchivoContenido = entrega.ArchivoContenido;
                existente.FechaEntrega = DateTime.Now;
            }
            else
            {
                entrega.FechaEntrega = DateTime.Now;
                _context.EntregasTareasEstudiantes.Add(entrega);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<EntregaTareaEstudiante>> ObtenerEntregasPorTareaAsync(int tareaId)
        {
            return await _context.EntregasTareasEstudiantes
                .Where(e => e.TareaId == tareaId)
                .Include(e => e.Usuario)
                .ToListAsync();
        }

        public async Task<EntregaTareaEstudiante?> ObtenerEntregaEstudianteAsync(int tareaId, int usuarioId)
        {
            return await _context.EntregasTareasEstudiantes
                .FirstOrDefaultAsync(e => e.TareaId == tareaId && e.UsuarioId == usuarioId);
        }

        public async Task<bool> ActualizarRetroalimentacionYNotaAsync(EntregaTareaEstudiante entrega)
        {
            var existente = await _context.EntregasTareasEstudiantes
                .FirstOrDefaultAsync(e => e.Id == entrega.Id);

            if (existente != null)
            {
                existente.Retroalimentacion = entrega.Retroalimentacion;
                existente.Nota = entrega.Nota;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
