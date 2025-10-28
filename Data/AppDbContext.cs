using Microsoft.EntityFrameworkCore;
using NextLayer.Models;

namespace NextLayer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets existentes
        public DbSet<Client> Clients { get; set; } = null!; // '= null!' suprime warning CS8618
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Chamado> Chamados { get; set; } = null!;
        public DbSet<MensagemChat> MensagensChat { get; set; } = null!;
        public DbSet<Anexo> Anexos { get; set; } = null!;

        // --- NOVO DBSET ---
        public DbSet<FaqItem> FaqItems { get; set; } = null!;
        // --- FIM DO NOVO DBSET ---

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações existentes...
            modelBuilder.Entity<Client>(entity => { /*...*/ });
            modelBuilder.Entity<Employee>(entity => { /*...*/ });
            modelBuilder.Entity<Chamado>(entity => { /*...*/ });
            modelBuilder.Entity<MensagemChat>(entity => { /*...*/ });
            modelBuilder.Entity<Anexo>(entity => { /*...*/ });

            // --- NOVA CONFIGURAÇÃO PARA FAQ ---
            modelBuilder.Entity<FaqItem>(entity =>
            {
                entity.ToTable("FaqItens"); // Nome da tabela em português

                // Define nomes das colunas (opcional, mas bom para consistência)
                entity.Property(p => p.Pergunta).HasColumnName("Pergunta");
                entity.Property(p => p.Resposta).HasColumnName("Resposta");
                entity.Property(p => p.DataCriacao).HasColumnName("DataCriacao");
                entity.Property(p => p.DataUltimaAtualizacao).HasColumnName("DataUltimaAtualizacao");

                // Garante que DataCriacao tenha um valor padrão no banco
                entity.Property(p => p.DataCriacao).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            // --- FIM DA NOVA CONFIGURAÇÃO ---
        }
    }
}