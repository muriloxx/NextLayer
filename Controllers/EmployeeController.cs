// --- ARQUIVO: Controllers/EmployeeController.cs (COMPLETO E ATUALIZADO) ---

using Microsoft.AspNetCore.Mvc;
using NextLayer.Data; // 1. ADICIONADO: Para usar o AppDbContext
using NextLayer.Models; // 2. ADICIONADO: Para usar o Employee
using NextLayer.Services;
using NextLayer.ViewModels;
using System.Linq;
using System.Security.Claims; // 3. ADICIONADO: Para ler o Token
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // 4. ADICIONADO: Para [Authorize]
using Microsoft.EntityFrameworkCore; // 5. ADICIONADO: Para ToListAsync
using System.Collections.Generic; // 6. ADICIONADO: Para List

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employee")] // Apenas funcionários logados podem acessar este controller
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context; // 7. ADICIONADO: Injeção do DbContext

        // 8. ATUALIZADO: Construtor agora recebe o AppDbContext
        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// (Público para Analistas) Retorna uma lista de analistas para preencher dropdowns.
        /// (Este método já existia e foi mantido)
        /// </summary>
        [HttpGet("analistas")]
        public async Task<IActionResult> GetAnalistasParaDropdown()
        {
            try
            {
                var analistas = await _context.Employees
                    .OrderBy(e => e.Name)
                    .Select(e => new AnalistaViewModel
                    {
                        Id = e.Id,
                        Nome = e.Name,
                        Funcao = e.Role
                    })
                    .ToListAsync();

                return Ok(analistas);
            }
            catch (Exception ex)
            {
                // Em um app real, logar o ex
                return StatusCode(500, "Erro interno ao buscar analistas.");
            }
        }


        // --- INÍCIO: NOVOS MÉTODOS DE ADMIN ADICIONADOS ---

        /// <summary>
        /// (Admin) Retorna a lista de TODOS os funcionários (Analistas e Admins).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetAllEmployees()
        {
            // Verifica se o usuário logado é Admin
            var isAdminClaim = User.FindFirstValue("isAdmin");
            if (isAdminClaim != "True")
            {
                return StatusCode(403, new { message = "Acesso negado. Apenas administradores." });
            }

            // Retorna todos os funcionários, menos o Hash da senha
            var employees = await _context.Employees
                .OrderBy(e => e.Name)
                .Select(e => new Employee // Retorna o modelo Employee, mas sem o PasswordHash
                {
                    Id = e.Id,
                    Name = e.Name,
                    Email = e.Email,
                    Role = e.Role,
                    IsAdmin = e.IsAdmin
                })
                .ToListAsync();

            return Ok(employees);
        }

        /// <summary>
        /// (Admin) Atualiza os dados de um funcionário (Nome, Cargo, IsAdmin).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] AdminEditEmployeeViewModel model)
        {
            // Verifica se o usuário logado é Admin
            var isAdminClaim = User.FindFirstValue("isAdmin");
            if (isAdminClaim != "True")
            {
                return StatusCode(403, new { message = "Acesso negado. Apenas administradores." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = "Funcionário não encontrado." });
            }

            // Atualiza os campos (Não permite alterar E-mail ou Senha)
            employee.Name = model.Name;
            employee.Role = model.Role;
            employee.IsAdmin = model.IsAdmin;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Funcionário atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao atualizar: {ex.Message}" });
            }
        }

        /// <summary>
        /// (Admin) Exclui um funcionário.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            // Verifica se o usuário logado é Admin
            var isAdminClaim = User.FindFirstValue("isAdmin");
            if (isAdminClaim != "True")
            {
                return StatusCode(403, new { message = "Acesso negado. Apenas administradores." });
            }

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = "Funcionário não encontrado." });
            }

            try
            {
                // Verifica se o admin não está tentando se excluir
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (employee.Id.ToString() == currentUserId)
                {
                    return BadRequest(new { message = "Não é possível excluir a si mesmo." });
                }

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Funcionário excluído com sucesso!" });
            }
            catch (DbUpdateException) // Captura erro se o funcionário tiver chamados associados
            {
                return StatusCode(400, new { message = "Não foi possível excluir este funcionário. Provavelmente ele está associado a chamados existentes." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao excluir: {ex.Message}" });
            }
        }

        // --- FIM: NOVOS MÉTODOS DE ADMIN ADICIONADOS ---
    }
}