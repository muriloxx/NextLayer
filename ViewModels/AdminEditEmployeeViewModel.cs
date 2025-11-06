using System.ComponentModel.DataAnnotations;

namespace NextLayer.ViewModels
{
    /// <summary>
    /// ViewModel usado pelo Admin para ATUALIZAR os dados de um funcionário.
    /// O E-mail e a Senha não são alterados por aqui.
    /// </summary>
    public class AdminEditEmployeeViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "O cargo é obrigatório")]
        [StringLength(50)]
        public string Role { get; set; } // Ex: "N1", "N2", "Admin"

        public bool IsAdmin { get; set; } = false;
    }
}