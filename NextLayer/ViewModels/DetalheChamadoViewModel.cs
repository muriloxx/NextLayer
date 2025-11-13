using System;
using System.Collections.Generic;

namespace NextLayer.ViewModels
{
    /// <summary>
    /// ViewModel (DTO) usado para retornar os detalhes completos de um chamado
    /// para a tela de chat do front-end. É "plano" para evitar loops de serialização.
    /// </summary>
    public class DetalheChamadoViewModel
    {
        // Construtor para inicializar listas e strings e evitar warnings CS8618
        public DetalheChamadoViewModel()
        {
            NumeroChamado = string.Empty;
            Titulo = string.Empty;
            Descricao = string.Empty;
            Status = string.Empty;
            NomeCliente = string.Empty;
            Prioridade = string.Empty; // Valor padrão
            Mensagens = new List<MensagemViewModel>();
            Anexos = new List<AnexoViewModel>();
        }

        public int Id { get; set; }
        public string NumeroChamado { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; } // Descrição original
        public DateTime DataAbertura { get; set; }
        public string Status { get; set; }
        public string NomeCliente { get; set; }

        // --- NOVAS PROPRIEDADES PARA O FRONT-END ---
        /// <summary>
        /// Prioridade atual do chamado (Baixa, Média, Alta).
        /// </summary>
        public string Prioridade { get; set; }

        /// <summary>
        /// Role/Time para o qual o chamado está designado (N1, N2, etc.).
        /// </summary>
        public string? RoleDesignada { get; set; }

        /// <summary>
        /// ID do analista atualmente designado para o chamado.
        /// </summary>
        public int? AnalistaId { get; set; }

        /// <summary>
        /// Data/Hora (UTC) em que o chamado foi marcado como "Concluído".
        /// Usado pelo front-end para verificar o bloqueio de 72 horas.
        /// </summary>
        public DateTime? DataConclusao { get; set; }
        // --- FIM DAS NOVAS PROPRIEDADES ---

        // Listas "planas" dos ViewModels relacionados
        public List<MensagemViewModel> Mensagens { get; set; }
        public List<AnexoViewModel> Anexos { get; set; }
    }
}