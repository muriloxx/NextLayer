// --- ARQUIVO: Program.cs (COMPLETO E CORRIGIDO) ---

// Usings devem vir PRIMEIRO
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NextLayer.Data;
using NextLayer.Services; // NECESSÁRIO para as interfaces e classes de serviço
using Mscc.GenerativeAI; // NECESSÁRIO para Model (se usado no registro)
using Microsoft.Extensions.Logging;
using System.Net.Http; // NECESSÁRIO para AddHttpClient

// --- INÍCIO DAS INSTRUÇÕES DE NÍVEL SUPERIOR ---

var builder = WebApplication.CreateBuilder(args); // Definido UMA VEZ

// --- SEÇÃO 1: REGISTRO DE SERVIÇOS ---

builder.Services.AddCors(options => {
    options.AddPolicy(name: "_myAllowSpecificOrigins", policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChamadoService, ChamadoService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddHttpContextAccessor();

// REGISTRO DA IA (Usando Groq via GeminiIaService)
builder.Services.AddHttpClient(); // NECESSÁRIO
builder.Services.AddScoped<IIaService, GeminiIaService>(); // Mapeia Interface -> Implementação

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- FIM DO REGISTRO DE SERVIÇOS ---


var app = builder.Build(); // Definido UMA VEZ

// --- SEÇÃO 2: CONFIGURAÇÃO DO PIPELINE HTTP ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("_myAllowSpecificOrigins"); // Usa a política
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.Run();

// --- FIM DAS INSTRUÇÕES DE NÍVEL SUPERIOR ---
// (Não pode haver mais código C# solto aqui ou em outro arquivo .cs)