using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;

namespace NextLayer.Validators
{
    public class CpfValidationAttribute : ValidationAttribute
    {
        public CpfValidationAttribute() : base("O CPF fornecido não é válido.") { }

        // Assinatura corrigida para 'object?' e 'ValidationResult?'
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // [Required] cuida disso
            }
            var cpf = value.ToString();
            if (cpf == null) return new ValidationResult(ErrorMessage ?? "CPF inválido.");
            cpf = new string(cpf.Where(char.IsDigit).ToArray());
            if (cpf.Length != 11) { return new ValidationResult(ErrorMessage ?? "CPF inválido."); }
            if (cpf.All(c => c == cpf[0])) { return new ValidationResult(ErrorMessage ?? "CPF inválido."); }
            int[] m1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 }; int[] m2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string temp = cpf.Substring(0, 9); int soma = 0;
            for (int i = 0; i < 9; i++) soma += int.Parse(temp[i].ToString()) * m1[i];
            int r = soma % 11; r = r < 2 ? 0 : 11 - r; string d = r.ToString(); temp += d;
            soma = 0; for (int i = 0; i < 10; i++) soma += int.Parse(temp[i].ToString()) * m2[i];
            r = soma % 11; r = r < 2 ? 0 : 11 - r; d += r.ToString();
            if (cpf.EndsWith(d)) { return ValidationResult.Success; }
            return new ValidationResult(ErrorMessage ?? "CPF inválido.");
        }
    }
}