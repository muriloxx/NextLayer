using System.ComponentModel.DataAnnotations;

namespace NextLayer.ViewModels
{
    public class AtualizarChamadoViewModel //Para a caixa lateral
    {
        [Required]
        public string Status { get; set; }
        [Required]
        public string Prioridade { get; set; }
        public string? RoleDesignada { get; set; }
        public int? AnalistaId { get; set; } // Para designar o chamado a um analista
    }
}