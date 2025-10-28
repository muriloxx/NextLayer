using Microsoft.EntityFrameworkCore;
using NextLayer.Models;

namespace NextLayer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Chamado> Chamados { get; set; }
        public DbSet<MensagemChat> MensagensChat { get; set; }
        public DbSet<Anexo> Anexos { get; set; }

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

            // Configuração da Tabela "Chamados"
            modelBuilder.Entity<Chamado>(entity =>
            {
                entity.ToTable("Chamados");

                // Nomes das colunas em português
                entity.Property(p => p.NumeroChamado).HasColumnName("NumeroChamado");
                entity.Property(p => p.Titulo).HasColumnName("Titulo");
                entity.Property(p => p.Descricao).HasColumnName("Descricao");
                entity.Property(p => p.DataAbertura).HasColumnName("DataAbertura");
                entity.Property(p => p.Status).HasColumnName("Status");
                entity.Property(p => p.Prioridade).HasColumnName("Prioridade");
                entity.Property(p => p.RoleDesignada).HasColumnName("RoleDesignada");
                entity.Property(p => p.ClienteId).HasColumnName("ClienteId");
                entity.Property(p => p.AnalistaId).HasColumnName("AnalistaId");

                // --- MAPEAMENTO DA NOVA COLUNA ---
                entity.Property(p => p.AnalistaInteragiu)
                      .HasColumnName("AnalistaInteragiu")
                      .HasDefaultValue(false); // Define o valor padrão no banco
                // --- FIM DO MAPEAMENTO ---

                // Relacionamentos
                entity.HasOne(d => d.Cliente)
                      .WithMany()
                      .HasForeignKey(d => d.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Analista)
                      .WithMany()
                      .HasForeignKey(d => d.AnalistaId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuração da Tabela "MensagensChat"
            modelBuilder.Entity<MensagemChat>(entity =>
            {
                entity.ToTable("MensagensChat");
                entity.Property(p => p.Conteudo).HasColumnName("Conteudo");
                entity.Property(p => p.DataEnvio).HasColumnName("DataEnvio");
                entity.Property(p => p.ChamadoId).HasColumnName("ChamadoId");
                entity.Property(p => p.ClienteRemetenteId).HasColumnName("ClienteRemetenteId");
                entity.Property(p => p.FuncionarioRemetenteId).HasColumnName("FuncionarioRemetenteId");
                entity.Property(p => p.RemetenteNome).HasColumnName("RemetenteNome");

                entity.HasOne(d => d.Chamado)
                      .WithMany(p => p.Mensagens)
                      .HasForeignKey(d => d.ChamadoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração da Tabela "Anexos"
            modelBuilder.Entity<Anexo>(entity =>
            {
                entity.ToTable("Anexos");
                entity.Property(p => p.NomeArquivo).HasColumnName("NomeArquivo");
                entity.Property(p => p.UrlArquivo).HasColumnName("UrlArquivo");
                entity.Property(p => p.TipoConteudo).HasColumnName("TipoConteudo");
                entity.Property(p => p.DataUpload).HasColumnName("DataUpload");
                entity.Property(p => p.ChamadoId).HasColumnName("ChamadoId");

                entity.HasOne(d => d.Chamado)
                      .WithMany(p => p.Anexos)
                      .HasForeignKey(d => d.ChamadoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}