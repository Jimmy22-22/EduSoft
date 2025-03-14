using Microsoft.EntityFrameworkCore;

namespace EduSoft.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Clase> Clases { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<UsuarioClase> UsuarioClases { get; set; }
        public DbSet<HorarioClase> HorariosClases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UsuarioClase>()
                .HasKey(uc => new { uc.UsuarioId, uc.ClaseId });

            modelBuilder.Entity<UsuarioClase>()
                .HasOne(uc => uc.Usuario)
                .WithMany(u => u.UsuarioClases)
                .HasForeignKey(uc => uc.UsuarioId);

            modelBuilder.Entity<UsuarioClase>()
                .HasOne(uc => uc.Clase)
                .WithMany(c => c.UsuarioClases)
                .HasForeignKey(uc => uc.ClaseId);

            modelBuilder.Entity<Tarea>()
                .HasOne(t => t.Clase)
                .WithMany(c => c.Tareas)
                .HasForeignKey(t => t.ClaseId);

            modelBuilder.Entity<HorarioClase>()
                .HasOne(h => h.Clase)
                .WithMany(c => c.Horarios)
                .HasForeignKey(h => h.ClaseId);
        }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? SesionToken { get; set; }
        public bool SesionActiva { get; set; }
        public RolUsuario Rol { get; set; }
        public List<UsuarioClase> UsuarioClases { get; set; } = new();
    }

    public enum RolUsuario
    {
        Estudiante = 1,
        Maestro = 2
    }

    public class Clase
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Profesor { get; set; }
        public string CodigoClase { get; set; }
        public List<HorarioClase> Horarios { get; set; } = new();
        public List<UsuarioClase> UsuarioClases { get; set; } = new();
        public List<Tarea> Tareas { get; set; } = new();
    }

    public class Tarea
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaEntrega { get; set; }
        public int UsuarioId { get; set; }
        public int ClaseId { get; set; }
        public Clase Clase { get; set; }
    }

    public class UsuarioClase
    {
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public int ClaseId { get; set; }
        public Clase Clase { get; set; }
    }

    public class HorarioClase
    {
        public int Id { get; set; }
        public int ClaseId { get; set; }
        public Clase Clase { get; set; }
        public string NombreClase => Clase?.Nombre;
        public string Aula { get; set; }
        public string Profesor { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }
}
