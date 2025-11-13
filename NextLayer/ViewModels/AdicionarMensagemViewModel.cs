using System.ComponentModel.DataAnnotations;

namespace NextLayer.ViewModels
{
    public class AdicionarMensagemViewModel
    {
        [Required]
        public string Conteudo { get; set; }

        // O front-end vai nos dizer se é "Client" ou "Employee"
        [Required]
        public string TipoRemetente { get; set; }
    }
}