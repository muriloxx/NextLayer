using Microsoft.EntityFrameworkCore;
using NextLayer.Models;

namespace NextLayer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets (Mapeamento das classes para tabelas)
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Chamado> Chamados { get; set; } = null!;
        public DbSet<MensagemChat> MensagensChat { get; set; } = null!;
        public DbSet<Anexo> Anexos { get; set; } = null!;
        public DbSet<FaqItem> FaqItems { get; set; } = null!; // Inclui FAQ


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da Tabela "Clientes"
            modelBuilder.Entity<Client>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasIndex(c => c.Email).IsUnique();
                entity.HasIndex(c => c.Cpf).IsUnique();
                entity.Property(c => c.PasswordHash).HasColumnType("varchar(100)");
            });

            // Configuração da Tabela "Funcionarios"
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Funcionarios");
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).HasColumnType("varchar(100)");
            });

            // Configuração da Tabela "Chamados" (Atualizada)
            modelBuilder.Entity<Chamado>(entity =>
            {
                entity.ToTable("Chamados");

                // Nomes das colunas
                entity.Property(p => p.NumeroChamado).HasColumnName("NumeroChamado");
                entity.Property(p => p.Titulo).HasColumnName("Titulo");
                entity.Property(p => p.Descricao).HasColumnName("Descricao");
                entity.Property(p => p.DataAbertura).HasColumnName("DataAbertura");
                entity.Property(p => p.Status).HasColumnName("Status");
                entity.Property(p => p.Prioridade).HasColumnName("Prioridade");
                entity.Property(p => p.RoleDesignada).HasColumnName("RoleDesignada");
                entity.Property(p => p.ClienteId).HasColumnName("ClienteId");
                entity.Property(p => p.AnalistaId).HasColumnName("AnalistaId");
                entity.Property(p => p.AnalistaInteragiu).HasColumnName("AnalistaInteragiu").HasDefaultValue(false);
                // --- MAPEAMENTO DA NOVA COLUNA ---
                entity.Property(p => p.DataConclusao).HasColumnName("DataConclusao"); // Coluna anulável DateTime?
                // --- FIM DO MAPEAMENTO ---

                // Relacionamentos
                entity.HasOne(d => d.Cliente).WithMany().HasForeignKey(d => d.ClienteId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Analista).WithMany().HasForeignKey(d => d.AnalistaId).OnDelete(DeleteBehavior.SetNull);
            });

            // Configuração da Tabela "MensagensChat"
            modelBuilder.Entity<MensagemChat>(entity =>
            {
                entity.ToTable("MensagensChat");
                // ... (mapeamento das colunas) ...
                entity.HasOne(d => d.Chamado).WithMany(p => p.Mensagens).HasForeignKey(d => d.ChamadoId).OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração da Tabela "Anexos"
            modelBuilder.Entity<Anexo>(entity =>
            {
                entity.ToTable("Anexos");
                // ... (mapeamento das colunas) ...
                entity.HasOne(d => d.Chamado).WithMany(p => p.Anexos).HasForeignKey(d => d.ChamadoId).OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração da Tabela "FaqItens"
            modelBuilder.Entity<FaqItem>(entity =>
            {
                entity.ToTable("FaqItens");
                entity.Property(p => p.Pergunta).HasColumnName("Pergunta");
                entity.Property(p => p.Resposta).HasColumnName("Resposta");
                entity.Property(p => p.DataCriacao).HasColumnName("DataCriacao").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(p => p.DataUltimaAtualizacao).HasColumnName("DataUltimaAtualizacao");
            });
        }
    }
}