# NextLayer: Sistema de Service Desk com IA
## Projeto Integrado Multidisciplinar (PIM) - Análise e Desenvolvimento de Sistemas

Este repositório contém o código-fonte do **NextLayer**, um sistema de Service Desk (Help Desk) completo desenvolvido como o Projeto Integrado Multidisciplinar (PIM) para os 3º e 4º semestres do curso de Análise e Desenvolvimento de Sistemas.

O sistema é uma aplicação Web moderna composta por uma API RESTful em ASP.NET Core 8 e um front-end em JavaScript puro (Vanilla JS), com integração de Inteligência Artificial para triagem e suporte.

---

## 1. Objetivo Geral (Escopo do PIM)

Conforme os objetivos do PIM, o projeto visa desenvolver um sistema de Service Desk funcional para otimizar o atendimento ao cliente e centralizar solicitações de suporte. O NextLayer cumpre este objetivo implementando um portal onde clientes podem abrir chamados e um painel completo onde analistas e administradores podem gerenciar todo o ciclo de vida do atendimento.

## 2. Disciplinas Contempladas

O projeto aplica diretamente os conceitos das seguintes disciplinas do 3º e 4º semestres:

* **Engenharia de Software:** Adoção da arquitetura de serviços (Service Pattern), separação de responsabilidades (Controllers, Services, Models, ViewModels) e modelagem de requisitos em diagramas UML.
* **Programação Orientada a Objetos:** Utilização de classes, herança (ex: `Employee` e `Client` como usuários), interfaces (`IAuthService`, `IChamadoService`, etc.) e encapsulamento em toda a API C#.
* **Banco de Dados:** Modelagem e implementação de um banco de dados relacional (PostgreSQL) com chaves estrangeiras, índices e relacionamentos (1-N) usando Entity Framework Core.
* **Desenvolvimento de Sistemas Web:** Construção de uma API RESTful segura (`[Authorize]`) em ASP.NET Core 8 e um front-end reativo em Vanilla JS (`index.html`) que consome essa API de forma assíncrona (`fetch`).
* **Inteligência Artificial e Machine Learning:** Integração de uma IA (Groq/Llama3, via `GeminiIaService.cs`) para realizar a triagem inicial de chamados e sugerir artigos de FAQ proativamente.

## 3. Tecnologias Utilizadas

| Categoria | Tecnologia | Justificativa |
| :--- | :--- | :--- |
| **Back-end** | ASP.NET Core 8 (C#) | Plataforma moderna, robusta e de alta performance para criação de APIs RESTful. |
| **Front-end** | HTML5, CSS3, Vanilla JS | Para um front-end leve, sem frameworks, que se comunica com o back-end (SPA). |
| **Banco de Dados** | PostgreSQL | Banco de dados relacional SQL open-source, robusto e escalável (hospedado no Supabase). |
| **ORM** | Entity Framework Core | Abstração do banco de dados, facilitando a modelagem e migrações. |
| **Autenticação** | JWT (Tokens) | Padrão de mercado para autenticação stateless em APIs, com BClaims (`Role`, `isAdmin`). |
| **Segurança** | BCrypt.Net-Next | Biblioteca padrão para hashing de senhas, protegendo os dados dos usuários. |
| **Inteligência Artificial**| Groq (Llama3) | API de inferência de alta velocidade para respostas de IA e sugestões de FAQ. |

## 4. Funcionalidades Implementadas

O sistema `NextLayer` está funcionalmente completo:

### Módulo de Clientes
* **Autenticação:** Cadastro e Login de Clientes.
* **Gestão de Chamados:** Abertura de novos chamados com upload de anexos.
* **Chat:** Interação em tempo real com Analistas ou IA dentro de cada chamado.
* **FAQ Inteligente:** O sistema sugere artigos do FAQ antes mesmo do cliente abrir o chamado.
* **Gestão de Perfil:** O cliente pode alterar a própria senha (requer senha antiga).

### Módulo de Funcionários (Analistas e Admins)
* **Dashboard (BI):** Painel de relatórios em tempo real com 4 gráficos/tabelas (Total Abertos, Abertos por Prioridade, Recentes, Status).
* **Gestão de Chamados:** Visualização de filas de chamados, atribuição de analista, mudança de status e prioridade.
* **Chat:** Resposta direta aos clientes.
* **Gestão de Perfil:** O funcionário pode alterar a própria senha.

### Módulo de Administrador (Função `IsAdmin = true`)
* **Gestão de Funcionários (CRUD):** O Admin possui uma aba exclusiva para:
    * **Listar** todos os funcionários.
    * **Cadastrar** novos funcionários (definindo se são Admins ou não).
    * **Editar** funcionários existentes (Nome, Cargo, Nível de Admin).
    * **Excluir** funcionários.
* **Redefinição de Senha:** O Admin pode forçar uma nova senha para qualquer usuário (Cliente ou Funcionário) que a tenha esquecido.

## 5. Como Executar

1.  **Configurar o `appsettings.json`:**
    * Preencha a `ConnectionStrings:DefaultConnection` com sua string do PostgreSQL.
    * Preencha a `Groq:ApiKey` com sua chave de API do Groq.
    * Preencha a `Jwt` com suas chaves secretas (Issuer, Audience, Key).
2.  **Rodar a API:**
    * Certifique-se de que o Entity Framework Core CLI esteja instalado (`dotnet tool install --global dotnet-ef`).
    * Rode `Update-Database` no Console do Gerenciador de Pacotes para criar as tabelas no seu banco.
    * Inicie o projeto (F5 no Visual Studio ou `dotnet run`). A API estará rodando em `https://localhost:7121`.
3.  **Abrir o Front-end:**
    * Abra o arquivo `index.html` em qualquer navegador. O JavaScript no arquivo fará a conexão com a API.
