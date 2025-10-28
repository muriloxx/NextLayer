using NextLayer.Models;
using NextLayer.ViewModels;

namespace NextLayer.Services
{
    public interface IAuthService
    {
        // Retorna o usuário (Client ou Employee) e o tipo (string)
        Task<(object user, string userType)> AuthenticateAsync(LoginViewModel model);
        Task<Client> RegisterClientAsync(ClientRegisterViewModel model);
        Task<Employee> RegisterEmployeeAsync(EmployeeRegisterViewModel model);
    }
}