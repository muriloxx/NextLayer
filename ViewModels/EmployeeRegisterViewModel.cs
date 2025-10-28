using NextLayer.Validators; // Importa nosso validador
using System.ComponentModel.DataAnnotations;

namespace NextLayer.ViewModels
{
    public class EmployeeRegisterViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [InstitutionalEmail] // <-- NOSSA VALIDAÇÃO CUSTOMIZADA
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "As senhas não conferem.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "O cargo é obrigatório.")]
        public string Role { get; set; }
    }
}