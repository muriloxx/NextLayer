using Microsoft.EntityFrameworkCore;
using NextLayer.Data;
using NextLayer.Models;
using NextLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net; // Importante para o Hash e Verify

namespace NextLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        // --- Métodos que já existiam (sem alterações) ---
        public async Task<(object user, string userType)> AuthenticateAsync(LoginViewModel model)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == model.Email);
            if (client != null)
            {
                if (BCrypt.Net.BCrypt.Verify(model.Password, client.PasswordHash))
                {
                    return (client, "Client");
                }
            }
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == model.Email);
            if (employee != null)
            {
                if (BCrypt.Net.BCrypt.Verify(model.Password, employee.PasswordHash))
                {
                    return (employee, "Employee");
                }
            }
            return (null, null);
        }

        public async Task<Client> RegisterClientAsync(ClientRegisterViewModel model)
        {
            if (await _context.Clients.AnyAsync(c => c.Email == model.Email))
            {
                throw new InvalidOperationException("Este e-mail já está em uso.");
            }
            if (await _context.Clients.AnyAsync(c => c.Cpf == model.Cpf))
            {
                throw new InvalidOperationException("Este CPF já está em uso.");
            }
            var client = new Client
            {
                Name = model.Name,
                Email = model.Email,
                Cpf = model.Cpf,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<Employee> RegisterEmployeeAsync(EmployeeRegisterViewModel model)
        {
            

            // 1. Converte o e-mail de entrada para minúsculas ANTES de checar
            var emailLower = model.Email.ToLower();

            // 2. Verifica na tabela Employees (ignorando maiúsculas/minúsculas)
            var employeeExists = await _context.Employees.AnyAsync(e => e.Email.ToLower() == emailLower);
            if (employeeExists)
            {
                // Usa o e-mail original (model.Email) na mensagem de erro
                throw new InvalidOperationException($"Email '{model.Email}' já está em uso.");
            }

            // 3. ADIÇÃO CRÍTICA: Verifica também na tabela de Clientes
            var clientExists = await _context.Clients.AnyAsync(c => c.Email.ToLower() == emailLower);
            if (clientExists)
            {
                // Lança o mesmo erro para o usuário não saber que foi em outra tabela
                throw new InvalidOperationException($"Email '{model.Email}' já está em uso.");
            }


            // O resto do seu código estava correto:
            var employee = new Employee
            {
                Name = model.Name,
                Email = model.Email, // Salva o e-mail original no banco
                Role = model.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                IsAdmin = model.IsAdmin
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task MudarSenhaAsync(string userId, string userType, MudarSenhaViewModel model)
        {
            if (model.NovaSenha != model.ConfirmarNovaSenha)
            {
                throw new InvalidOperationException("A nova senha e a confirmação não conferem.");
            }
            if (!int.TryParse(userId, out int id))
            {
                throw new InvalidOperationException("ID de usuário inválido.");
            }

            if (userType == "Client")
            {
                var client = await _context.Clients.FindAsync(id);
                if (client == null) throw new InvalidOperationException("Usuário não encontrado.");
                if (!BCrypt.Net.BCrypt.Verify(model.SenhaAntiga, client.PasswordHash))
                {
                    throw new InvalidOperationException("A senha antiga está incorreta.");
                }
                client.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);
            }
            else if (userType == "Employee")
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null) throw new InvalidOperationException("Usuário não encontrado.");
                if (!BCrypt.Net.BCrypt.Verify(model.SenhaAntiga, employee.PasswordHash))
                {
                    throw new InvalidOperationException("A senha antiga está incorreta.");
                }
                employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);
            }
            else
            {
                throw new InvalidOperationException("Tipo de usuário desconhecido.");
            }
            await _context.SaveChangesAsync();
        }

        // --- FIM DOS MÉTODOS QUE JÁ EXISTIAM ---


        // --- INÍCIO DO NOVO MÉTODO ADICIONADO ---

        /// <summary>
        /// Força a redefinição da senha de um usuário (Cliente ou Funcionário)
        /// Esta ação é executada por um Administrador.
        /// </summary>
        public async Task AdminResetPasswordAsync(AdminResetPasswordViewModel model)
        {
            if (model.NovaSenha != model.ConfirmarNovaSenha)
            {
                throw new InvalidOperationException("A nova senha e a confirmação não conferem.");
            }

            // Procura o usuário primeiro na tabela de Clientes
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == model.Email);
            if (client != null)
            {
                // Encontrou um cliente. Gera o hash e salva.
                client.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);
            }
            else
            {
                // Se não for cliente, procura na tabela de Funcionários
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == model.Email);
                if (employee != null)
                {
                    // Encontrou um funcionário. Gera o hash e salva.
                    employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);
                }
                else
                {
                    // Não encontrou em nenhuma das tabelas.
                    throw new InvalidOperationException("Nenhum usuário (Cliente ou Funcionário) foi encontrado com este e-mail.");
                }
            }

            // Salva as mudanças no banco de dados
            await _context.SaveChangesAsync();
        }

        // --- FIM DO NOVO MÉTODO ADICIONADO ---
    }
}