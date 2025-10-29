namespace NextLayer.ViewModels
{
    /// <summary>
    /// ViewModel para o relatório de contagem de chamados por status.
    /// </summary>
    public class StatusReportViewModel
    {
        /// <summary>
        /// O nome do status (ex: "Aberto (IA)", "Concluído").
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// A quantidade de chamados nesse status.
        /// </summary>
        public int Contagem { get; set; }
    }
}