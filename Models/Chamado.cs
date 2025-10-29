using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NextLayer.Models
{
    /// <summary>
    /// Representa um chamado de suporte técnico no sistema.
    /// </summary>
    public class Chamado
    {
        // Construtor para inicializar as coleções e valores padrão
        public Chamado()
        {
            Mensagens = new HashSet<MensagemChat>();
            Anexos = new HashSet<Anexo>();
            AnalistaInteragiu = false; // Valor inicial padrão
            // Inicializa propriedades string para evitar warnings CS8618
            NumeroChamado = string.Empty;
            Titulo = string.Empty;
            Descricao = string.Empty;
            Status = string.Empty;
            Prioridade = string.Empty;
        }

        public int Id { get; set; }
        public string NumeroChamado { get; set; } // Identificação única do chamado (ex: HD-XYZ)
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataAbertura { get; set; } // Data/Hora (UTC) da criação

        // Propriedades controladas pelo analista
        public string Status { get; set; } // Ex: "Aberto (IA)", "Aguardando Analista", "Em Andamento (Analista)", "Concluído", "Encerrado"
        public string Prioridade { get; set; } // Ex: "Baixa", "Média", "Alta"
        public string? RoleDesignada { get; set; } // Para qual time/nível foi encaminhado (anulável)

        // Relacionamento com o Cliente que abriu o chamado
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public virtual Client Cliente { get; set; } = null!; // Garante que Cliente não é nulo após carregamento

        // Relacionamento com o Funcionário (Analista) que está atendendo
        public int? AnalistaId { get; set; } // Anulável, pode não ter analista designado
        [ForeignKey("AnalistaId")]
        public virtual Employee? Analista { get; set; } // Analista pode ser nulo

        /// <summary>
        /// Flag para indicar se um analista já enviou alguma mensagem neste chamado.
        /// Usado para controlar se a IA deve continuar respondendo.
        /// </summary>
        public bool AnalistaInteragiu { get; set; }

        /// <summary>
        /// Armazena a data e hora (UTC) em que o chamado foi movido para o status "Concluído".
        /// Usado para calcular o prazo de 72 horas para encerramento automático (bloqueio de novas mensagens do cliente).
        /// É anulável porque o chamado pode não ter sido concluído ainda.
        /// </summary>
        public DateTime? DataConclusao { get; set; }

        // Coleções para relacionamentos "muitos"
        public virtual ICollection<MensagemChat> Mensagens { get; set; }
        public virtual ICollection<Anexo> Anexos { get; set; }
    }
}