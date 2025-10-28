using System;

namespace NextLayer.Models
{
    public class Anexo
    {
        public int Id { get; set; }
        public string NomeArquivo { get; set; } // Ex: "screenshot_erro.png"

        // Onde o arquivo está salvo (ex: "/uploads/chamado_XYZ/screenshot.png")
        public string UrlArquivo { get; set; }
        public string TipoConteudo { get; set; } // Ex: "image/png"
        public DateTime DataUpload { get; set; }

        // Relacionamento: A qual chamado pertence
        public int ChamadoId { get; set; }
        public virtual Chamado Chamado { get; set; }
    }
}