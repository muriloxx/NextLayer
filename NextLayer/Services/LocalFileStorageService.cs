using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SalvarArquivo(IFormFile arquivo, string diretorioDestino)
        {
            if (arquivo == null || arquivo.Length == 0)
                throw new ArgumentException("Arquivo não fornecido.");

            // Gera um nome de arquivo único para evitar sobreposição
            var nomeArquivoUnico = $"{Guid.NewGuid()}_{arquivo.FileName}";

            // Caminho físico onde o arquivo será salvo (ex: C:/.../NextLayer/wwwroot/uploads/chamados/arquivo.png)
            var pastaFisica = Path.Combine(_env.WebRootPath, diretorioDestino);

            // Cria o diretório se ele não existir
            Directory.CreateDirectory(pastaFisica);

            var caminhoCompletoArquivo = Path.Combine(pastaFisica, nomeArquivoUnico);

            // Salva o arquivo no disco
            using (var stream = new FileStream(caminhoCompletoArquivo, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            // Retorna a URL pública para acessar o arquivo (ex: https://localhost:7121/uploads/chamados/arquivo.png)
            var request = _httpContextAccessor.HttpContext.Request;
            var urlBase = $"{request.Scheme}://{request.Host}";
            var urlPublica = $"{urlBase}/{diretorioDestino.Replace("\\", "/")}/{nomeArquivoUnico}";

            return urlPublica;
        }

        public void DeletarArquivo(string urlArquivo)
        {
            // Lógica para deletar o arquivo do disco (implementação futura)
        }
    }
}