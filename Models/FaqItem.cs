using System;
using System.ComponentModel.DataAnnotations; // Para atributos como [Key]

namespace NextLayer.Models
{
    /// <summary>
    /// Representa um item (pergunta e resposta) na base de conhecimento (FAQ).
    /// </summary>
    public class FaqItem
    {
        [Key] // Define Id como chave primária
        public int Id { get; set; }

        [Required(ErrorMessage = "A pergunta é obrigatória.")]
        [MaxLength(500, ErrorMessage = "A pergunta não pode exceder 500 caracteres.")]
        public string Pergunta { get; set; } = string.Empty;

        [Required(ErrorMessage = "A resposta é obrigatória.")]
        public string Resposta { get; set; } = string.Empty; // Pode ser texto longo, talvez com markdown

        public DateTime DataCriacao { get; set; }

        public DateTime? DataUltimaAtualizacao { get; set; } // Anulável

        // Opcional: Adicionar Categoria, Tags, Contagem de Visualizações, etc. no futuro
        // public string? Categoria { get; set; }
    }
}