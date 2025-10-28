namespace NextLayer.ViewModels
{
    /// <summary>
    /// Representa a resposta estruturada da IA.
    /// </summary>
    public class IaResposta
    {
        /// <summary>
        /// O texto da resposta para o usuário.
        /// </summary>
        public string TextoResposta { get; set; } = string.Empty;

        /// <summary>
        /// A categoria (Role) que a IA determinou para o chamado (ex: Infraestrutura, Software).
        /// Será nulo se a IA não identificar uma ou não for pedido.
        /// </summary>
        public string? RoleSugerida { get; set; }

        /// <summary>
        /// Indica se a IA decidiu encaminhar para um analista (baseado no prompt).
        /// </summary>
        public bool DeveEncaminhar { get; set; }
    }
}