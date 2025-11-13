using System.ComponentModel.DataAnnotations;

namespace NextLayer.Models
{
    // Esta classe representa uma única entrada de log de auditoria.
    // Cada vez que algo for criado, editado ou excluído,
    // um objeto desta classe será salvo no banco.
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        // Quem fez a alteração? (Pode ser nulo se a ação for do sistema)
        public string? UserId { get; set; }

        // Que tipo de ação foi? (ex: "Added", "Modified", "Deleted")
        public string ActionType { get; set; }

        // Qual tabela foi afetada? (ex: "Chamados", "Clients")
        public string TableName { get; set; }

        // Quando a alteração ocorreu?
        public DateTime Timestamp { get; set; }

        // Qual foi o ID da linha afetada?
        public string PrimaryKey { get; set; }

        // Armazena um JSON dos valores antigos (apenas para Update/Delete)
        public string? OldValues { get; set; }

        // Armazena um JSON dos valores novos (apenas para Create/Update)
        public string? NewValues { get; set; }
    }
}