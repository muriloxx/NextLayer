using System.ComponentModel.DataAnnotations;

namespace NextLayer.ViewModels
{
    public class MudarSenhaViewModel
    {
        [Required(ErrorMessage = "A senha antiga é obrigatória")]
        public string? SenhaAntiga { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Compare("NovaSenha", ErrorMessage = "A nova senha e a confirmação não conferem.")]
        public string? ConfirmarNovaSenha { get; set; }
    }
}