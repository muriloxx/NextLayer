using NextLayer.Models;
using NextLayer.ViewModels; 
using System.Threading.Tasks;

namespace NextLayer.Services
{
    public interface IAuthService
    {
        Task<(object user, string userType)> AuthenticateAsync(LoginViewModel model);
        Task<Client> RegisterClientAsync(ClientRegisterViewModel model);
        Task<Employee> RegisterEmployeeAsync(EmployeeRegisterViewModel model);



        /// <summary>
        /// Altera a senha de um usuário logado (Cliente ou Funcionário).
        /// </summary>
        /// <param name="userId">O ID do usuário (lido do token JWT)</param>
        /// <param name="userType">O tipo do usuário (Client ou Employee, lido do token)</param>
        /// <param name="model">Contém a SenhaAntiga e a NovaSenha</param>
        /// <exception cref="InvalidOperationException">Lança uma exceção se a senha antiga estiver incorreta ou a validação falhar.</exception>
        Task MudarSenhaAsync(string userId, string userType, MudarSenhaViewModel model);

        /// <summary>
        /// Força a redefinição da senha de um usuário (Cliente ou Funcionário)
        /// Esta ação é executada por um Administrador.
        /// </summary>
        /// <param name="model">Contém o E-mail do usuário e a Nova Senha</param>
        /// <exception cref="InvalidOperationException">Lança uma exceção se o usuário não for encontrado.</exception>
        Task AdminResetPasswordAsync(AdminResetPasswordViewModel model);
    }
}