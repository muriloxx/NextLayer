using System.ComponentModel.DataAnnotations;

namespace NextLayer.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Outros campos específicos do funcionário
        public string Role { get; set; } // Ex: "Gerente", "Técnico"
    }
}