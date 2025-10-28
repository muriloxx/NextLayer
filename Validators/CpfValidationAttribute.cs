using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;

namespace NextLayer.Validators
{
    public class CpfValidationAttribute : ValidationAttribute
    {
        public CpfValidationAttribute()
            : base("O CPF fornecido não é válido.") { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // Deixa o [Required] cuidar da validação de nulo
                return ValidationResult.Success;
            }

            var cpf = value.ToString();

            // Remove caracteres não numéricos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11)
            {
                return new ValidationResult(ErrorMessage);
            }

            // Verifica CPFs inválidos conhecidos (ex: "11111111111")
            if (cpf.All(c => c == cpf[0]))
            {
                return new ValidationResult(ErrorMessage);
            }

            // Validação dos dígitos verificadores
            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            }

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            string digito = resto.ToString();
            tempCpf += digito;

            soma = 0;
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            digito += resto.ToString();

            if (cpf.EndsWith(digito))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}