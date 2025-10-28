using System;
using System.Collections.Generic;

namespace NextLayer.ViewModels
{
    // Este é o DTO que o front-end usará para a tela de chat
    public class DetalheChamadoViewModel
    {
        public int Id { get; set; }
        public string NumeroChamado { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataAbertura { get; set; }
        public string Status { get; set; }
        public string NomeCliente { get; set; }

        // Listas "planas" que não causam loops
        public List<MensagemViewModel> Mensagens { get; set; }
        public List<AnexoViewModel> Anexos { get; set; }
    }
}