namespace NextLayer.ViewModels
{
    /// <summary>
    /// ViewModel simples para representar um analista na lista de designação.
    /// </summary>
    public class AnalistaViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Funcao { get; set; } = string.Empty; // Usaremos a propriedade 'Role' do Employee
    }
}