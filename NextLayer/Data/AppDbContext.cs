using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // Necessário para o ChangeTracker
using NextLayer.Models;
using System.Security.Claims; // Necessário para pegar o UserId
using System.Text.Json; // Necessário para serializar os logs

namespace NextLayer.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Construtor atualizado para injetar o IHttpContextAccessor
        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //  DbSets existentes
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Chamado> Chamados { get; set; }
        public DbSet<MensagemChat> MensagensChat { get; set; }
        public DbSet<Anexo> Anexos { get; set; }
        public DbSet<FaqItem> FaqItens { get; set; }

        // DbSet para a  tabela de Auditoria
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Identity DbSets (como no seu arquivo original)
        public DbSet<IdentityUser> Users { get; set; }
        public DbSet<IdentityRole> Roles { get; set; }
        public DbSet<IdentityUserRole<string>> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações do Identity (como no seu arquivo original)
            modelBuilder.Entity<IdentityUserRole<string>>().HasKey(p => new { p.UserId, p.RoleId });
            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<Employee>().ToTable("Employees");
        }

        // Sobrescrita do SaveChangesAsync
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = await LogAuditEntriesAsync();
            await AuditLogs.AddRangeAsync(auditEntries, cancellationToken);
            return await base.SaveChangesAsync(cancellationToken);
        }

        // Método auxiliar para criar os logs
        private async Task<List<AuditLog>> LogAuditEntriesAsync()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog &&
                            (e.State == EntityState.Added ||
                             e.State == EntityState.Modified ||
                             e.State == EntityState.Deleted))
                .ToList();

            var auditLogs = new List<AuditLog>();
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var timestamp = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    ActionType = entry.State.ToString(),
                    TableName = entry.Metadata.GetTableName(),
                    Timestamp = timestamp,
                    PrimaryKey = GetPrimaryKey(entry)
                };

                if (entry.State == EntityState.Added)
                {
                    auditLog.NewValues = SerializeChanges(entry.CurrentValues);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditLog.OldValues = SerializeChanges(entry.OriginalValues);
                }
                else if (entry.State == EntityState.Modified)
                {
                    var modifiedProperties = entry.Properties
                        .Where(p => p.IsModified)
                        .ToDictionary(p => p.Metadata.Name, p => new
                        {
                            Old = p.OriginalValue,
                            New = p.CurrentValue
                        });

                    auditLog.OldValues = JsonSerializer.Serialize(modifiedProperties.ToDictionary(p => p.Key, p => p.Value.Old));
                    auditLog.NewValues = JsonSerializer.Serialize(modifiedProperties.ToDictionary(p => p.Key, p => p.Value.New));
                }

                auditLogs.Add(auditLog);
            }

            return auditLogs;
        }

        // Método auxiliar para pegar o ID (Chave Primária)
        private string GetPrimaryKey(EntityEntry entry)
        {
            var key = entry.Metadata.FindPrimaryKey();
            if (key == null) return "N/A";

            var pkValues = key.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString());
            return string.Join(",", pkValues);
        }

        // Método auxiliar para converter os valores em JSON
        private string? SerializeChanges(PropertyValues? values)
        {
            if (values == null) return null;

            var obj = values.ToObject();

            // Medida de segurança (para não logar senhas)
            if (obj is IdentityUser user)
            {
                user.PasswordHash = "[REDACTED]";
            }

            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
        }
    }
}