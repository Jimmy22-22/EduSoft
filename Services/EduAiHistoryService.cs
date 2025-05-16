using EduSoft.Data;
using Microsoft.EntityFrameworkCore;

namespace EduSoft.Services
{
    public class EduAiHistoryService
    {
        private readonly AppDbContext _context;

        public EduAiHistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task GuardarMensajeAsync(int usuarioId, string rol, string contenido)
        {
            var conversacion = new ConversacionIA
            {
                UsuarioId = usuarioId,
                Rol = rol,
                Contenido = contenido,
                Fecha = DateTime.Now
            };

            _context.ConversacionesIA.Add(conversacion);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> ObtenerConversacionAsync(int usuarioId)
        {
            return await _context.ConversacionesIA
                .Where(c => c.UsuarioId == usuarioId)
                .OrderBy(c => c.Fecha)
                .Select(c => new ChatMessage
                {
                    Role = c.Rol,
                    Content = c.Contenido
                })
                .ToListAsync();
        }
    }
}
