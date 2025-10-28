// --- ARQUIVO: Program.cs (COMPLETO E CORRIGIDO COM CORS) ---

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NextLayer.Data;
using NextLayer.Services; // NECESSÁRIO para as interfaces e classes de serviço
// using Mscc.GenerativeAI; // Não precisa mais aqui
using Microsoft.Extensions.Logging;
using System.Net.Http; // NECESSÁRIO para AddHttpClient
using System.Text; // Para a chave JWT
using Microsoft.AspNetCore.Authentication.JwtBearer; // Para JWT
using Microsoft.IdentityModel.Tokens; // Para JWT

var builder = WebApplication.CreateBuilder(args); // Definido UMA VEZ

// --- SEÇÃO 1: REGISTRO DE SERVIÇOS ---

// 1. --- CONFIGURAÇÃO DE CORS ---
// Define uma política de CORS chamada "_myAllowSpecificOrigins"
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_myAllowSpecificOrigins",
                      policy =>
                      {
                          // Permite requisições de QUALQUER origem (incluindo 'null')
                          // Para produção, você pode restringir: policy.WithOrigins("http://seu-site.com")
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});
// --- FIM DA CONFIGURAÇÃO DE CORS ---

// 2. Banco de Dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// 3. Nossos Serviços
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChamadoService, ChamadoService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFaqService, FaqService>(); // Serviço de FAQ

// 4. Serviço de IA (Groq)
builder.Services.AddHttpClient(); // NECESSÁRIO
builder.Services.AddScoped<IIaService, GeminiIaService>(); // Mapeia Interface -> Implementação

// 5. Autenticação JWT
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
            ?? throw new ArgumentNullException("Jwt:Key não encontrada")))
    };
});
builder.Services.AddAuthorization();

// 6. Serviços Padrão do ASP.NET
builder.Services.AddControllers();
builder.Services.AddRazorPages(); // Mantém as páginas padrão (Welcome, Error)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- FIM DO REGISTRO DE SERVIÇOS ---

var app = builder.Build(); // Definido UMA VEZ

// --- Seed do FAQ ---
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
    app.UseSwagger(); app.UseSwaggerUI(); app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error"); app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles(); // Para servir uploads da wwwroot

// O Roteamento deve vir antes do CORS e da Autenticação
app.UseRouting();

// --- APLICA A POLÍTICA DE CORS ---
// Esta linha PERMITE que o 'origin null' acesse sua API
app.UseCors("_myAllowSpecificOrigins");
// --- FIM DA APLICAÇÃO DO CORS ---

// A Autenticação deve vir ANTES da Autorização
app.UseAuthentication();
app.UseAuthorization();

// Mapeia os endpoints
app.MapRazorPages(); // Para a página /Error
app.MapControllers(); // Para sua API

app.Run();