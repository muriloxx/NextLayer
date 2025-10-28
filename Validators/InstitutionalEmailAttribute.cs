using System.ComponentModel.DataAnnotations;

namespace NextLayer.Validators
{
    public class InstitutionalEmailAttribute : ValidationAttribute
    {
        private const string InstitutionalDomain = "nextlayer.com";

        // Mensagem de erro padrão
        public InstitutionalEmailAttribute()
            : base($"O e-mail deve pertencer ao domínio @{InstitutionalDomain}.")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // Deixe o [Required] cuidar da validação de nulo/vazio
                return ValidationResult.Success;
            }

            string email = value.ToString();

            // Verifica se o e-mail termina com o domínio (ignorando maiúsculas/minúsculas)
            if (email.EndsWith($"@{InstitutionalDomain}", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Success;
            }

            // Retorna a mensagem de erro definida no construtor
            return new ValidationResult(ErrorMessage);
        }
    }
}