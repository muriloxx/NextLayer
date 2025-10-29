// --- ARQUIVO: Program.cs (COMPLETO E COMENTADO) ---

// Usings necessários para os serviços, Entity Framework, Configuração, etc.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders; // Para UseStaticFiles
using NextLayer.Data; // Onde está o AppDbContext
using NextLayer.Services; // Onde estão TODAS as interfaces e classes de serviço
using Microsoft.Extensions.Logging; // Para logs
using System.Net.Http; // Para AddHttpClient

// Usings para Autenticação JWT
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// --- Início das Instruções de Nível Superior ---
var builder = WebApplication.CreateBuilder(args);

// --- SEÇÃO 1: REGISTRO DE SERVIÇOS (Injeção de Dependência) ---

// 1. Configuração do CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// 2. Configuração do Banco de Dados (Entity Framework Core + PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// 3. Nossos Serviços de Negócios
builder.Services.AddScoped<IAuthService, AuthService>(); // Serviço de Login/Cadastro
builder.Services.AddScoped<IChamadoService, ChamadoService>(); // Serviço de Chamados
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>(); // Serviço de Upload
builder.Services.AddScoped<IFaqService, FaqService>(); // Serviço de FAQ
builder.Services.AddScoped<IDashboardService, DashboardService>(); // (NOVO) Serviço de Relatórios

// 4. Serviço de Inteligência Artificial (usando API REST da Groq)
builder.Services.AddHttpClient(); // Registra IHttpClientFactory
builder.Services.AddScoped<IIaService, GeminiIaService>(); // Mapeia Interface -> Implementação (com lógica Groq)

// 5. Serviço de Contexto HTTP
builder.Services.AddHttpContextAccessor();

// 6. Configuração de Autenticação JWT (JSON Web Token)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
            ?? throw new ArgumentNullException("Jwt:Key não encontrada no appsettings.json")))
    };
});

// 7. Serviço de Autorização
builder.Services.AddAuthorization();

// 8. Serviços Padrão do ASP.NET Core
builder.Services.AddControllers();
builder.Services.AddRazorPages(); // Para a tela de erro padrão
builder.Services.AddEndpointsApiExplorer(); // Necessário para o Swagger
builder.Services.AddSwaggerGen();


// --- FIM DO REGISTRO DE SERVIÇOS ---


var app = builder.Build(); // Constrói a aplicação

// --- SEED INICIAL DO FAQ ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var faqService = services.GetRequiredService<IFaqService>();
        Task.Run(async () => await faqService.SeedInitialFaqsAsync()).Wait();
        Console.WriteLine("Seed de FAQ verificado/executado.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro durante o seeding inicial do FAQ.");
    }
}
// --- FIM DO SEED ---


// --- SEÇÃO 2: CONFIGURAÇÃO DO PIPELINE HTTP (A ORDEM IMPORTA!) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // Mostra erros detalhados no navegador
}
else
{
    app.UseExceptionHandler("/Error"); // Página de erro genérica (Razor Page)
    app.UseHsts(); // Força o uso de HTTPS
}

app.UseHttpsRedirection();

// Habilita o serviço de arquivos estáticos (CSS, JS, Imagens na wwwroot, e nossos Uploads)
app.UseStaticFiles();

// Habilita o roteamento
app.UseRouting();

// Aplica a política de CORS
app.UseCors(MyAllowSpecificOrigins);

// Habilita Autenticação e Autorização (Ordem correta)
app.UseAuthentication(); // 1. Verifica QUEM é o usuário (lê o token)
app.UseAuthorization();  // 2. Verifica o que o usuário PODE FAZER (baseado na Role)

// Mapeia os endpoints
app.MapRazorPages(); // Para a página /Error
app.MapControllers(); // Para sua API

// Inicia a aplicação
app.Run();

// --- FIM DO ARQUIVO ---