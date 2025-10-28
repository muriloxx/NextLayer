// --- ARQUIVO: Program.cs (COMPLETO E COMENTADO - COM FAQ) ---

// Usings necessários para os serviços, Entity Framework, Configuração, etc.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders; // Para UseStaticFiles
using NextLayer.Data; // Onde está o AppDbContext
using NextLayer.Services; // Onde estão TODAS as interfaces e classes de serviço
// using Mscc.GenerativeAI; // Não é mais necessário aqui, pois a IA é encapsulada
using Microsoft.Extensions.Logging; // Para logs
using System.Net.Http; // Para AddHttpClient

// --- INÍCIO DAS INSTRUÇÕES DE Nível Superior ---

var builder = WebApplication.CreateBuilder(args); // Cria o construtor da aplicação web

// --- SEÇÃO 1: REGISTRO DE SERVIÇOS (Injeção de Dependência) ---
// Configura os serviços que a aplicação usará.

// 1. Configuração do CORS (Cross-Origin Resource Sharing)
// Permite que o front-end (index.html) acesse a API
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; // Nome da política de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          // Em desenvolvimento, permite qualquer origem, cabeçalho e método.
                          // Em produção, restrinja a origem (ex: policy.WithOrigins("http://seu-dominio.com"))
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// 2. Configuração do Banco de Dados (Entity Framework Core + PostgreSQL)
// Lê a string de conexão do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Registra o AppDbContext para usar PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// 3. Serviços de Autenticação
// Mapeia a interface IAuthService para a implementação AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// 4. Serviços do Sistema de Chamados
// Mapeia a interface IChamadoService para a implementação ChamadoService
builder.Services.AddScoped<IChamadoService, ChamadoService>();

// 5. Serviços de Upload de Arquivos
// Mapeia a interface IFileStorageService para a implementação LocalFileStorageService
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
// Adiciona o HttpContextAccessor, necessário para o LocalFileStorageService obter a URL base
builder.Services.AddHttpContextAccessor();

// 6. Serviços de Inteligência Artificial (usando API REST da Groq)
// Registra o IHttpClientFactory, que gerencia instâncias de HttpClient
builder.Services.AddHttpClient();
// Mapeia a interface IIaService (ou IChatIaService) para a implementação GeminiIaService (ou GroqIaService)
// A implementação (GeminiIaService/GroqIaService) receberá IConfiguration, IHttpClientFactory, ILogger
builder.Services.AddScoped<IIaService, GeminiIaService>(); // <-- Use os nomes corretos aqui

// 7. (NOVO) Serviço de FAQ
// Mapeia a interface IFaqService para a implementação FaqService
builder.Services.AddScoped<IFaqService, FaqService>();

// 8. Serviços Padrão do ASP.NET Core e Swagger
// Adiciona suporte para Controllers de API
builder.Services.AddControllers();
// Adiciona suporte para Razor Pages (se usadas)
builder.Services.AddRazorPages();
// Adiciona serviços para a documentação da API via Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- FIM DO REGISTRO DE SERVIÇOS ---


var app = builder.Build(); // Constrói a aplicação

// --- (NOVO) SEED INICIAL DO FAQ ---
// Este bloco executa o método para popular as FAQs iniciais UMA VEZ na inicialização,
// somente se a tabela FaqItens estiver vazia.
using (var scope = app.Services.CreateScope()) // Cria um escopo de serviço temporário
{
    var services = scope.ServiceProvider;
    try
    {
        var faqService = services.GetRequiredService<IFaqService>(); // Pega o serviço de FAQ
        // Chama o método de seed de forma síncrona (adequado para inicialização)
        // Task.Run(...).Wait() é uma forma de fazer isso fora de um método async
        Task.Run(async () => await faqService.SeedInitialFaqsAsync()).Wait();
        Console.WriteLine("Seed de FAQ verificado/executado."); // Log simples
    }
    catch (Exception ex)
    {
        // Loga um erro se o seeding falhar
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro durante o seeding inicial do FAQ.");
        // Considerar se a aplicação deve parar aqui ou continuar sem os FAQs iniciais
    }
}
// --- FIM DO SEED ---


// --- SEÇÃO 2: CONFIGURAÇÃO DO PIPELINE HTTP ---
// Define como as requisições HTTP são processadas em sequência.

// Configurações específicas para o ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Habilita o endpoint do Swagger JSON
    app.UseSwaggerUI(); // Habilita a interface visual do Swagger UI
    app.UseDeveloperExceptionPage(); // Mostra erros detalhados no navegador (NÃO USE EM PRODUÇÃO)
}
else // Configurações para produção
{
    app.UseExceptionHandler("/Error"); // Usa uma página de erro genérica (Razor Page)
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(); // Adiciona header HSTS para segurança
}

// Redireciona requisições HTTP para HTTPS
app.UseHttpsRedirection();

// Habilita o serviço de arquivos estáticos (CSS, JS, Imagens na wwwroot, e nossos Uploads)
app.UseStaticFiles();

// Habilita o sistema de roteamento do ASP.NET Core
app.UseRouting();

// Aplica a política de CORS definida anteriormente
app.UseCors(MyAllowSpecificOrigins); // Garanta que o nome da política está correto

// Habilita os middlewares de Autenticação e Autorização
// (A ordem é importante: Autenticação antes de Autorização)
// app.UseAuthentication(); // Descomente quando implementar autenticação (ex: JWT)
app.UseAuthorization();

// Mapeia as rotas definidas nos Controllers para os endpoints da API
app.MapControllers();
// Mapeia as rotas para as Razor Pages (se houver)
app.MapRazorPages();

// Inicia a aplicação e começa a escutar por requisições HTTP
app.Run();

// --- FIM DO ARQUIVO ---