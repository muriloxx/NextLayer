using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NextLayer.Models
{
    public class Chamado
    {
        // Construtor para inicializar as coleções
        public Chamado()
        {
            Mensagens = new HashSet<MensagemChat>();
            Anexos = new HashSet<Anexo>();
            AnalistaInteragiu = false; // Valor inicial padrão
        }

        public int Id { get; set; }
        public string NumeroChamado { get; set; } // A "identificação do chamado"
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataAbertura { get; set; }

        // Opções da caixa lateral do analista
        public string Status { get; set; } // Ex: "Aberto (IA)", "Aguardando Analista", "Em Andamento (Analista)", "Fechado"
        public string Prioridade { get; set; } // Ex: "Baixa", "Média", "Alta"
        public string? RoleDesignada { get; set; } // Para qual role está (N1, N2, etc)

        // Relacionamento: Quem abriu o chamado
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public virtual Client Cliente { get; set; } = null!; // Evita warning CS8618

        // Relacionamento: Quem está atendendo
        public int? AnalistaId { get; set; } // Anulável, pode não ter analista ainda
        [ForeignKey("AnalistaId")]
        public virtual Employee? Analista { get; set; }

        // --- NOVA PROPRIEDADE ---
        /// <summary>
        /// Indica se um analista (Employee) já enviou alguma mensagem neste chamado.
        /// Usado para desativar a resposta automática da IA.
        /// </summary>
        public bool AnalistaInteragiu { get; set; }
        // --- FIM DA NOVA PROPRIEDADE ---

        // Relacionamento: Um chamado tem muitas mensagens e muitos anexos
        public virtual ICollection<MensagemChat> Mensagens { get; set; }
        public virtual ICollection<Anexo> Anexos { get; set; }
    }
}