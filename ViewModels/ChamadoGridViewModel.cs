using System;

namespace NextLayer.ViewModels //Para o Grid do Analista
{
    public class ChamadoGridViewModel
    {
        public int Id { get; set; }
        public string NumeroChamado { get; set; }
        public string Titulo { get; set; }
        public string NomeCliente { get; set; }
        public DateTime DataAbertura { get; set; }
        public string Status { get; set; }
    }
}