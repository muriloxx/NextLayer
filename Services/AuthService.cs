using Microsoft.EntityFrameworkCore;
using NextLayer.Data;
using NextLayer.Models;
using NextLayer.ViewModels;
using System.Threading.Tasks;
using BCrypt.Net;

namespace NextLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(object user, string userType)> AuthenticateAsync(LoginViewModel model)
        {
            // 1. Tenta encontrar um funcionário
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == model.Email);

            if (employee != null)
            {
                // 2. Verifica o hash da senha
                if (BCrypt.Net.BCrypt.Verify(model.Password, employee.PasswordHash))
                {
                    return (employee, "Employee");
                }
            }

            // 3. Se não for funcionário, tenta encontrar um cliente
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == model.Email);

            if (client != null)
            {
                // 4. Verifica o hash da senha
                if (BCrypt.Net.BCrypt.Verify(model.Password, client.PasswordHash))
                {
                    return (client, "Client");
                }
            }

            // 5. Se não encontrou ninguém ou a senha estava errada
            return (null, null);
        }

        // Método auxiliar para checar duplicidade de CPF
private async Task<bool> CpfExistsAsync(string cpf)
{
    return await _context.Clients.AnyAsync(c => c.Cpf == cpf);
}

        public async Task<Client> RegisterClientAsync(ClientRegisterViewModel model)
        {
            // Verifica se o e-mail já existe (em qualquer tabela)
            if (await EmailExistsAsync(model.Email))
            {
                throw new System.Exception("Este e-mail já está em uso.");
            }

            var client = new Client
            {
                Name = model.Name,
                Email = model.Email,
                Cpf = model.Cpf,
                // Cria o hash da senha
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            
            return client;
        }

        public async Task<Employee> RegisterEmployeeAsync(EmployeeRegisterViewModel model)
        {
            // Verifica se o e-mail já existe (em qualquer tabela)
            if (await EmailExistsAsync(model.Email))
            {
                throw new System.Exception("Este e-mail já está em uso.");
            }

            var employee = new Employee
            {
                Name = model.Name,
                Email = model.Email,
                Role = model.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return employee;
        }

        // Método auxiliar para checar duplicidade de e-mail
        private async Task<bool> EmailExistsAsync(string email)
        {
            bool clientExists = await _context.Clients.AnyAsync(c => c.Email == email);
            bool employeeExists = await _context.Employees.AnyAsync(e => e.Email == email);
            return clientExists || employeeExists;
        }
    }
}