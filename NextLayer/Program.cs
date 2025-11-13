// --- ARQUIVO: Program.cs (COMPLETO E CORRIGIDO) ---

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NextLayer.Data;
using NextLayer.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- SEÇÃO 1: REGISTRO DE SERVIÇOS ---

// 1. --- CONFIGURAÇÃO DE CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_myAllowSpecificOrigins",
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});
// --- FIM DA CONFIGURAÇÃO DE CORS ---

// 2. Banco de Dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// 3. Nossos Serviços de Negócios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChamadoService, ChamadoService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFaqService, FaqService>(); // Serviço de FAQ
builder.Services.AddScoped<IDashboardService, DashboardService>(); // Serviço de Relatórios

// 4. Serviço de Inteligência Artificial (Groq)
builder.Services.AddHttpClient();
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
builder.Services.AddAuthorization(options =>
{
    // Esta política verifica se o token tem a claim "isAdmin" com o valor "True"
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("isAdmin", "True"));
});
// 6. Serviços Padrão do ASP.NET
builder.Services.AddControllers();
builder.Services.AddRazorPages(); // Mantém as páginas padrão (Welcome, Error)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// (A linha duplicada 'AddScoped<IAuthService, AuthService>' foi removida daqui)

// --- FIM DO REGISTRO DE SERVIÇOS ---

var app = builder.Build();

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

// app.UseDefaultFiles() DEVE vir ANTES de app.UseStaticFiles().
// Isto diz ao servidor para procurar "index.html" quando a raiz (/) é pedida.
app.UseDefaultFiles();

// Isto serve os ficheiros que o UseDefaultFiles encontrou (ex: index.html),
// bem como outros (css, js, imagens) da wwwroot.
app.UseStaticFiles();

// O Roteamento deve vir antes do CORS e da Autenticação
app.UseRouting();

// --- APLICA A POLÍTICA DE CORS ---
app.UseCors("_myAllowSpecificOrigins");
// --- FIM DA APLICAÇÃO DO CORS ---

// A Autenticação deve vir ANTES da Autorização
app.UseAuthentication();
app.UseAuthorization();

// Mapeia os endpoints
app.MapRazorPages(); // Para a página /Error
app.MapControllers(); // Para sua API

app.Run();