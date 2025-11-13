using System.ComponentModel.DataAnnotations;

namespace NextLayer.Validators
{
    public class InstitutionalEmailAttribute : ValidationAttribute
    {
        private const string InstitutionalDomain = "nextlayer.com";
        public InstitutionalEmailAttribute() : base($"O e-mail deve pertencer ao domínio @{InstitutionalDomain}.") { }

        // Assinatura corrigida para 'object?' e 'ValidationResult?'
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // [Required] cuida disso
            }
            string email = value.ToString() ?? "";
            if (email.EndsWith($"@{InstitutionalDomain}", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage ?? $"E-mail deve ser @{InstitutionalDomain}.");
        }
    }
}