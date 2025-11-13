# NextLayer - API de Help Desk com IA

NextLayer é uma API backend robusta para um sistema de help desk e gestão de chamados (ticketing), construída com as tecnologias mais recentes da plataforma .NET.

Este projeto serve como o "cérebro" central para múltiplas interfaces de cliente, incluindo um portal web (SPA) e um painel de administração desktop (Windows Forms).

## 🚀 Origem do Projeto

Este projeto foi desenvolvido como parte do **Projeto Integrado Multidisciplinar (PIM)** para o curso de **Análise e Desenvolvimento de Sistemas** da **Universidade Paulista (UNIP)** (3º/4º Semestre - 2025/2).

O objetivo principal foi aplicar os conceitos aprendidos em sala de aula para criar uma solução de software completa, focada em ferramentas de suporte técnico, segurança da informação e boas práticas de desenvolvimento (como a LGPD), simulando um ambiente de negócios real.

## ✨ Funcionalidades Principais

Esta API fornece endpoints para todas as operações do sistema, incluindo:

* **Autenticação Segura:** Sistema de login baseado em Token JWT com autorização por papéis (Roles) e "Policies" customizadas (`Client`, `Employee`, `Admin`).
* **Controle de Acesso:** Separação clara de permissões para cada tipo de usuário.
* **Gestão de Chamados (CRUD):**
    * Criação de novos chamados por clientes, com upload de múltiplos anexos.
    * Sistema de chat em tempo real (baseado em polling) para cada chamado.
    * Triagem de chamados (alteração de status, prioridade e analista responsável).
* **Gestão de Usuários (CRUD):**
    * Cadastro de Clientes (com validação de CPF).
    * Cadastro de Funcionários (com validação de e-mail institucional).
    * Edição, listagem e exclusão de funcionários por administradores.
* **Integração com IA (Grok):**
    * **Respostas Iniciais:** A IA fornece a primeira resposta automática ao cliente quando um chamado é criado.
    * **Sugestões de FAQ:** A IA sugere artigos de FAQ relevantes enquanto o cliente digita o seu problema.
* **Dashboard de Relatórios:** Endpoints que fornecem estatísticas para painéis de BI (ex: total de chamados abertos, chamados por status, chamados por prioridade).
* **Armazenamento de Ficheiros:** Serviço de armazenamento local para guardar anexos de chamados de forma segura.

## 💻 Tecnologias Utilizadas

* **.NET 8**
* **ASP.NET Core Web API**
* **Entity Framework Core 8** (para o ORM)
* **PostgreSQL** (Banco de Dados)
* **Autenticação JWT** (JSON Web Tokens)
* **BCrypt.Net-Next** (para Hashing de senhas)
* **API do Google Gemini** (para as funcionalidades de IA)
* **Arquitetura de Serviços** (Services) para desacoplar a lógica de negócio dos Controllers.

## ⚙️ Como Executar

### Pré-requisitos
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* Um servidor [PostgreSQL](https://www.postgresql.org/download/) em execução.
* (Opcional) Uma chave de API do Google Gemini (ou Groq, etc.) para a IA funcionar.

### 1. Configurar o `appsettings.Development.json`
Antes de executar, configure as suas "secrets". O ficheiro deve ter a seguinte estrutura:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=nextlayer_db;Username=postgres;Password=SEU_PASSWORD"
  },
  "Jwt": {
    "Key": "SUA_CHAVE_SECRETA_SUPER_LONGA_E_SEGURA_AQUI",
    "Issuer": "NextLayerAPI",
    "Audience": "NextLayerApp"
  },
  "AiService": {
    "ApiKey": "SUA_CHAVE_DE_API_DO_GEMINI_AQUI"
  }
}
