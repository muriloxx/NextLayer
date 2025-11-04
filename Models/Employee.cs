using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NextLayer.Models
{
    [Table("Funcionarios")]
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [StringLength(100)] 
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "O cargo é obrigatório")]
        [StringLength(50)]
        public string Role { get; set; } // Ex: "N1", "N2", "Admin"

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Define se o funcionário é um Administrador (pode criar outros usuários).
        /// O padrão (default) é 'false'.
        /// </summary>
        public bool IsAdmin { get; set; } = false;
        }
}