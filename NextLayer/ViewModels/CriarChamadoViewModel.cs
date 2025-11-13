using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NextLayer.ViewModels
{
    public class CriarChamadoViewModel
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [MaxLength(200)]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Descricao { get; set; }

        // O front-end enviará os arquivos de imagem aqui
        public List<IFormFile>? Imagens { get; set; }
    }
}