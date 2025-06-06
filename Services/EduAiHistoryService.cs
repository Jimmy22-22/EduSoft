using EduSoft.Data;
using Microsoft.EntityFrameworkCore;

namespace EduSoft.Services
{
    /// <summary>
    /// Servicio encargado de guardar y recuperar el historial de conversaciones
    /// entre el usuario y la IA en la plataforma EduSoft.
    /// </summary>
    public class EduAiHistoryService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor que recibe el contexto de base de datos.
        /// </summary>
        /// <param name="context">Instancia del <see cref="AppDbContext"/> para acceder a la base de datos.</param>
        public EduAiHistoryService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Guarda un nuevo mensaje en el historial de conversación de la IA.
        /// </summary>
        /// <param name="usuarioId">ID del usuario que participa en la conversación.</param>
        /// <param name="rol">Rol del mensaje ("user" o "assistant").</param>
        /// <param name="contenido">Contenido textual del mensaje.</param>
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

        /// <summary>
        /// Recupera el historial completo de conversación para un usuario específico,
        /// ordenado cronológicamente.
        /// </summary>
        /// <param name="usuarioId">ID del usuario del cual se recuperará el historial.</param>
        /// <returns>Lista de mensajes en formato <see cref="ChatMessage"/>.</returns>
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
