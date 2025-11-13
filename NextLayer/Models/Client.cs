using System.ComponentModel.DataAnnotations;

namespace NextLayer.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        // NUNCA armazene senhas em texto puro. Este campo guardará o hash.
        public string PasswordHash { get; set; }

        // Outros campos específicos do cliente
        public string Cpf { get; set; }
    }
}   