using EduSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSoft.Services
{
    public class TestSuiteService
    {
        private readonly AuthService _authService;
        private readonly ClaseService _claseService;
        private readonly AsistenciaService _asistenciaService;
        private readonly DashboardEstudianteService _dashboardEstudianteService;
        private readonly DashboardMaestroService _dashboardMaestroService;
        private readonly EduAiContextEstudianteBuilder _aiEstudianteBuilder;
        private readonly EduAiContextMaestroBuilder _aiMaestroBuilder;
        private readonly EduAiHistoryService _aiHistoryService;
        private readonly HorarioService _horarioService;
        private readonly TareaEstudianteService _tareaEstudianteService;

        public TestSuiteService(
            AuthService authService,
            ClaseService claseService,
            AsistenciaService asistenciaService,
            DashboardEstudianteService dashboardEstudianteService,
            DashboardMaestroService dashboardMaestroService,
            EduAiContextEstudianteBuilder aiEstudianteBuilder,
            EduAiContextMaestroBuilder aiMaestroBuilder,
            EduAiHistoryService aiHistoryService,
            HorarioService horarioService,
            TareaEstudianteService tareaEstudianteService)
        {
            _authService = authService;
            _claseService = claseService;
            _asistenciaService = asistenciaService;
            _dashboardEstudianteService = dashboardEstudianteService;
            _dashboardMaestroService = dashboardMaestroService;
            _aiEstudianteBuilder = aiEstudianteBuilder;
            _aiMaestroBuilder = aiMaestroBuilder;
            _aiHistoryService = aiHistoryService;
            _horarioService = horarioService;
            _tareaEstudianteService = tareaEstudianteService;
        }

        public async Task<List<string>> EjecutarPruebasAsync()
        {
            var resultados = new List<string>();
            string testEmail = $"test_{Guid.NewGuid()}@mail.com";
            string testProfesor = "ProfesorTest";
            int testUsuarioId = 1;

            try
            {
                // AUTENTICACIÓN
                resultados.Add("AUTENTICACION");
                var registrado = await _authService.RegisterUser("TestUser", testEmail, "123456", RolUsuario.Estudiante);
                resultados.Add($"Registro: {(registrado ? "OK" : "Fallo")}");

                var login = await _authService.Login(testEmail, "123456");
                resultados.Add($"Login: {(login != null ? "OK" : "Fallo")}");

                var sesion = await _authService.VerificarSesion();
                resultados.Add($"Verificar sesion activa: {(sesion != null ? "OK" : "No activa")}");

                if (login != null)
                {
                    await _authService.EliminarSesion(login.Id);
                    resultados.Add("Cierre de sesion: OK");
                }

                // CLASES
                resultados.Add("CLASES");
                var claseCreada = await _claseService.CrearClase("Clase Test", testProfesor);
                resultados.Add($"Crear clase: {(claseCreada ? "OK" : "Fallo")}");

                var clases = await _claseService.ObtenerClasesAsync();
                resultados.Add($"Obtener clases: {clases.Count} encontradas");

                var claseId = clases.FirstOrDefault(c => c.Profesor == testProfesor)?.Id ?? 0;
                var unirse = await _claseService.UnirseAClase(testUsuarioId, clases[0].CodigoClase);
                resultados.Add($"Unirse a clase: {(unirse ? "OK" : "Ya inscrito - OK")}");

                var estudiantes = await _claseService.GetEstudiantesPorClaseAsync(claseId);
                resultados.Add($"Estudiantes en clase: {estudiantes.Count} encontrados");

                // TAREAS
                resultados.Add("TAREAS");
                var tareaCreada = await _claseService.CrearTarea(claseId, "Tarea Test", "Descripcion", DateTime.Now.AddDays(2), testUsuarioId, null, null, null, false);
                resultados.Add($"Crear tarea: {(tareaCreada ? "OK" : "Fallo")}");

                var tareas = await _claseService.ObtenerTareasPorClaseAsync(claseId);
                resultados.Add($"Tareas por clase: {tareas.Count} encontradas");

                var tarea = tareas.FirstOrDefault();
                if (tarea != null)
                {
                    var entrega = new EntregaTareaEstudiante
                    {
                        UsuarioId = testUsuarioId,
                        TareaId = tarea.Id,
                        Comentario = "Comentario de prueba",
                        FechaEntrega = DateTime.Now
                    };
                    await _tareaEstudianteService.EnviarEntregaAsync(entrega);
                    resultados.Add("Entrega tarea: OK");

                    entrega.Nota = 95;
                    entrega.Retroalimentacion = "Buen trabajo";
                    await _tareaEstudianteService.ActualizarRetroalimentacionYNotaAsync(entrega);
                    resultados.Add("Actualizar nota y feedback: OK");
                }

                // HORARIOS
                resultados.Add("HORARIOS");
                var horarioAgregado = await _horarioService.AgregarHorarioAsync(claseId, DateTime.Today, new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0), "Aula 5", testProfesor);
                resultados.Add($"Agregar horario: {(horarioAgregado ? "OK" : "Fallo")}");

                var horarios = await _horarioService.GetHorariosPorMaestroAsync(testProfesor);
                resultados.Add($"Horarios por maestro: {horarios.Count} encontrados");

                // ASISTENCIA
                resultados.Add("ASISTENCIA");
                await _asistenciaService.RegistrarAsistencia(claseId, DateTime.Today, new List<(int, bool)> { (testUsuarioId, true) });
                resultados.Add("Registrar asistencia: OK");

                var asistencias = await _asistenciaService.GetAsistenciasPorEstudianteAsync(testUsuarioId);
                resultados.Add($"Asistencias por estudiante: {asistencias.Count} encontradas");

                // IA CONTEXTO Y HISTORIAL
                resultados.Add("IA CONTEXTO E HISTORIAL");
                var contextoEst = await _aiEstudianteBuilder.ConstruirContextoEstudianteAsync(testUsuarioId);
                resultados.Add($"Contexto estudiante generado: {(string.IsNullOrWhiteSpace(contextoEst) ? "Vacio" : "OK")}");

                var contextoMaestro = await _aiMaestroBuilder.ConstruirContextoMaestroAsync(testProfesor);
                resultados.Add($"Contexto maestro generado: {(string.IsNullOrWhiteSpace(contextoMaestro) ? "Vacio" : "OK")}");

                await _aiHistoryService.GuardarMensajeAsync(testUsuarioId, "user", "Hola IA");
                var historial = await _aiHistoryService.ObtenerConversacionAsync(testUsuarioId);
                resultados.Add($"Historial IA guardado: {historial.Count} mensajes");

                // NOTIFICACIONES Y DASHBOARD
                resultados.Add("NOTIFICACIONES Y DASHBOARD");
                var notifsEst = await _dashboardEstudianteService.ObtenerNotificacionesAsync(testUsuarioId);
                resultados.Add($"Notificaciones estudiante: {notifsEst.Count} encontradas");

                var notifsMaestro = await _dashboardMaestroService.ObtenerNotificacionesMaestroAsync(testProfesor);
                resultados.Add($"Notificaciones maestro: {notifsMaestro.Count} encontradas");
            }
            catch (Exception ex)
            {
                resultados.Add($"ERROR CRITICO: {ex.Message}");
            }

            return resultados;
        }
    }
}
