using System.ComponentModel.DataAnnotations;
using NextLayer.Validators;

namespace NextLayer.ViewModels
{
    public class EmployeeRegisterViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [StringLength(100)]
        [InstitutionalEmail]
        public string Email { get; set; }

        [Required(ErrorMessage = "O cargo é obrigatório")]
        [StringLength(50)]
        public string Role { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "A senha e a confirmação não conferem.")]
        public string ConfirmPassword { get; set; }

        // Isso permite que o JSON do front-end (com 'isAdmin: true/false')
        // seja mapeado para este modelo quando o Admin cadastrar um novo usuário.
        public bool IsAdmin { get; set; } = false;
    }
}