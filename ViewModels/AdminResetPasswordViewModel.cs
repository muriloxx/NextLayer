using System.ComponentModel.DataAnnotations;

namespace NextLayer.ViewModels
{
    /// <summary>
    /// ViewModel usado pelo Administrador para forçar a redefinição
    /// da senha de qualquer usuário (Cliente ou Funcionário).
    /// </summary>
    public class AdminResetPasswordViewModel
    {
        [Required(ErrorMessage = "O e-mail do usuário é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Compare("NovaSenha", ErrorMessage = "A nova senha e a confirmação não conferem.")]
        public string? ConfirmarNovaSenha { get; set; }
    }
}