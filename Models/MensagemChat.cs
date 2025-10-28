using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NextLayer.Models
{
    public class MensagemChat
    {
        public int Id { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataEnvio { get; set; }

        // Relacionamento: A qual chamado pertence
        public int ChamadoId { get; set; }
        public virtual Chamado Chamado { get; set; }

        // Relacionamento: Quem enviou (pode ser cliente, funcionário ou IA)
        public int? ClienteRemetenteId { get; set; }
        [ForeignKey("ClienteRemetenteId")]
        public virtual Client? ClienteRemetente { get; set; }

        public int? FuncionarioRemetenteId { get; set; }
        [ForeignKey("FuncionarioRemetenteId")]
        public virtual Employee? FuncionarioRemetente { get; set; }

        // Se for a IA, os IDs acima serão nulos e usaremos este campo
        public string? RemetenteNome { get; set; } // Ex: "IA NextLayer"
    }
}