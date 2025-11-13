# NextLayer - Painel do Administrador (Desktop)

Este projeto é o cliente desktop (Windows Forms) para administração do sistema de help desk `NextLayer`. Ele consome a `NextLayer API` para fornecer uma interface gráfica robusta para administradores gerirem usuários e chamados.

## 🚀 Origem do Projeto

Este projeto é uma das "frentes" (frontends) do **Projeto Integrado Multidisciplinar (PIM)**, desenvolvido para o curso de **Análise e Desenvolvimento de Sistemas** da **Universidade Paulista (UNIP)** (3º/4º Semestre - 2025/2).

Enquanto o `index.html` serve como o portal para Clientes e Analistas, este aplicativo desktop foi criado especificamente para o **Administrador do Sistema**, oferecendo funcionalidades de gestão que não estão disponíveis no portal web.

## ✨ Funcionalidades Principais

* **Login Seguro:** Autenticação de administrador via JWT, consumindo o endpoint `/api/Auth/login`.
* **Verificação de Permissão:** O aplicativo só permite o login de usuários que a API retorna com a flag `isAdmin = true`.
* **Aba de Gestão de Funcionários (CRUD):**
    * **Listar (Read):** Carrega todos os funcionários (`Employee`) da base de dados numa tabela.
    * **Criar (Create):** Formulário para registar novos funcionários (Analistas ou Admins), com validação de campos (incluindo senha e domínio de e-mail).
    * **Editar (Update):** Permite alterar o nome, cargo, status de admin e redefinir a senha de funcionários existentes.
    * *Nota: A exclusão (Delete) pode ser implementada futuramente.*
* **Aba de Gestão de Chamados:**
    * **Listar (Read):** Carrega *todos* os chamados do sistema numa tabela centralizada.
    * **Filtragem Avançada:** Permite filtrar a lista de chamados por Status e Prioridade.
    * **Ver Detalhes:** Permite abrir um novo formulário (ainda em desenvolvimento) para ver o histórico de chat e detalhes de um chamado específico com um duplo-clique.

## 💻 Tecnologias Utilizadas

* **.NET 8**
* **Windows Forms (WinForms)**
* `HttpClient` (para consumo da API REST)
* `System.Text.Json` (para serialização e deserialização de objetos C# <-> JSON)

## ⚙️ Como Executar

### Pré-requisitos
1.  **A API `NextLayer` DEVE estar em execução** (normalmente em `https://localhost:7121`), pois este aplicativo depende 100% dela.
2.  Um usuário com `IsAdmin = true` registado na base de dados para poder fazer o login.

### 1. Executar pelo Visual Studio
1.  Abra a solução completa `NextLayer.sln` no Visual Studio 2022.
2.  No "Gerenciador de Soluções", clique com o botão direito na Solução e selecione **"Configurar Projetos de Inicialização..."**.
3.  Escolha **"Vários projetos de inicialização"** e defina a "Ação" como **"Iniciar" (Start)** para ambos:
    * `NextLayer` (a API)
    * `NextLayer.AdminDesktop` (o WinForms)
4.  Prima **F5** ou clique em "Iniciar". A API irá arrancar (consola) e o formulário de login do Windows Forms aparecerá.
5.  Faça login com as suas credenciais de Administrador.
