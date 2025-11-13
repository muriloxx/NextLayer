using System;

namespace NextLayer.ViewModels
{
    // Um DTO "plano" para uma mensagem de chat
    public class MensagemViewModel
    {
        public int Id { get; set; }
        public string Conteudo { get; set; } = string.Empty; // Inicializa para evitar warning CS8618
        public DateTime DataEnvio { get; set; }
        public string RemetenteNome { get; set; } = string.Empty; // Inicializa para evitar warning CS8618

        /// <summary>
        /// Indica o tipo de remetente: "Client", "Employee", ou "IA".
        /// Usado pelo front-end para definir o estilo e alinhamento da mensagem.
        /// </summary>
        public string TipoRemetente { get; set; } = string.Empty; // Inicializa para evitar warning CS8618


    }
}