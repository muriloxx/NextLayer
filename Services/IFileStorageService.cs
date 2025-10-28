using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    public interface IFileStorageService
    {
        // Salva o arquivo e retorna a URL de acesso a ele
        Task<string> SalvarArquivo(IFormFile arquivo, string diretorioDestino);

        void DeletarArquivo(string urlArquivo);
    }
}