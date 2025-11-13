using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers; // Para o cabeçalho de autenticação
using System.Net.Http.Json;    // Para PostAsJsonAsync e ReadFromJsonAsync
using System.Text.Json;
using System.Threading.Tasks;  // Necessário para métodos async
using System.Windows.Forms;
using System.Linq; // Necessário para .Any() na classe ApiErrorResponse

namespace NextLayer.AdminDesktop
{
    public partial class frmDashboard : Form
    {
        // --- Constantes e Variáveis Globais ---

        // URL base da sua API. Mude aqui se o endereço ou a porta mudarem.
        private const string ApiBaseUrl = "https://localhost:7121";

        // Variável de estado para o CRUD de funcionários.
        // Se for 'null', estamos criando um novo funcionário.
        // Se tiver um ID, estamos editando o funcionário com esse ID.
        private int? selectedEmployeeId = null;

        // --- Construtor e Configuração Inicial ---

        public frmDashboard()
        {
            InitializeComponent();

            // Métodos chamados na inicialização do formulário

            // 1. Configura a aparência da tabela de funcionários
            SetupDataGridView();

            // 2. Configura a aparência da tabela de chamados (COM A CORREÇÃO)
            SetupChamadosDataGridView();

            // 3. Preenche as caixas de filtro (ComboBox) da aba de chamados
            PreencherFiltrosChamados();
        }

        // Configura o DataGridView (tabela) dos funcionários
        private void SetupDataGridView()
        {
            dgvFuncionarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFuncionarios.ReadOnly = true;
            dgvFuncionarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFuncionarios.MultiSelect = false;
        }

        // Configura o DataGridView (tabela) dos chamados
        private void SetupChamadosDataGridView()
        {
            dgvChamados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvChamados.ReadOnly = true;
            dgvChamados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvChamados.MultiSelect = false;

            // --- Formatação da Coluna de Data (COM A CORREÇÃO) ---
            // Adicionamos um evento 'CellFormatting' para formatar a data/hora
            dgvChamados.CellFormatting += (sender, e) =>
            {
                // --- CORREÇÃO DE ERRO (NullReferenceException) ---
                // Verificamos se o valor ou a coluna são nulos ANTES de tentar ler.
                // Isso previne o "crash" que ocorria quando o form era carregado
                // (antes de a tabela ter dados ou nomes de colunas).
                if (e.Value == null || e.ColumnIndex < 0 || dgvChamados.Columns[e.ColumnIndex] == null)
                {
                    return; // Sai do evento sem fazer nada
                }

                // Se passou na verificação, podemos formatar a data com segurança
                var column = dgvChamados.Columns[e.ColumnIndex];
                if (column.Name == "DataAbertura" && e.Value is DateTime)
                {
                    e.Value = ((DateTime)e.Value).ToString("dd/MM/yyyy HH:mm");
                    e.FormattingApplied = true;
                }
            };
        }

        // Preenche as ComboBoxes (filtros) da aba de chamados
        private void PreencherFiltrosChamados()
        {
            // --- Filtro de Status ---
            // Usamos objetos anônimos (com Text e Value) para facilitar
            cmbFiltroStatus.Items.Add(new { Text = "-- Todos Status --", Value = "" });
            cmbFiltroStatus.Items.Add(new { Text = "Aberto (IA)", Value = "Aberto (IA)" });
            cmbFiltroStatus.Items.Add(new { Text = "Aguardando Analista", Value = "Aguardando Analista" });
            cmbFiltroStatus.Items.Add(new { Text = "Em Andamento (Analista)", Value = "Em Andamento (Analista)" });
            cmbFiltroStatus.Items.Add(new { Text = "Aguardando Cliente", Value = "Aguardando Cliente" });
            cmbFiltroStatus.Items.Add(new { Text = "Concluído", Value = "Concluído" });
            cmbFiltroStatus.Items.Add(new { Text = "Encerrado", Value = "Encerrado" });
            cmbFiltroStatus.Items.Add(new { Text = "Cancelado", Value = "Cancelado" });

            // Define qual propriedade do objeto anônimo será mostrada (Text)
            cmbFiltroStatus.DisplayMember = "Text";
            // Define qual propriedade será usada como valor (Value)
            cmbFiltroStatus.ValueMember = "Value";
            cmbFiltroStatus.SelectedIndex = 0; // Deixa "-- Todos Status --" selecionado

            // --- Filtro de Prioridade ---
            cmbFiltroPrioridade.Items.Add(new { Text = "-- Todas Prioridades --", Value = "" });
            cmbFiltroPrioridade.Items.Add(new { Text = "Baixa", Value = "Baixa" });
            cmbFiltroPrioridade.Items.Add(new { Text = "Média", Value = "Média" });
            cmbFiltroPrioridade.Items.Add(new { Text = "Alta", Value = "Alta" });

            cmbFiltroPrioridade.DisplayMember = "Text";
            cmbFiltroPrioridade.ValueMember = "Value";
            cmbFiltroPrioridade.SelectedIndex = 0;
        }

        // --- Evento de Fechamento do Formulário ---

        // Quando o usuário fecha o Dashboard (no 'X'), encerramos a aplicação inteira.
        private void frmDashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        // ====================================================================
        // --- ABA: GESTÃO DE FUNCIONÁRIOS
        // ====================================================================

        // --- Evento de clique do botão "Carregar/Atualizar Lista" (Funcionários) ---
        private async void btnCarregarFuncionarios_Click(object sender, EventArgs e)
        {
            btnCarregarFuncionarios.Enabled = false;
            btnCarregarFuncionarios.Text = "Carregando...";
            dgvFuncionarios.DataSource = null;

            try
            {
                // Pega o cliente HTTP já autenticado (com Token)
                var client = ApiClient.GetClient();

                // Chama o endpoint GET /api/Employee
                var response = await client.GetAsync($"{ApiBaseUrl}/api/Employee");

                if (response.IsSuccessStatusCode)
                {
                    // Converte a resposta JSON em uma lista de EmployeeViewModel
                    var funcionarios = await response.Content.ReadFromJsonAsync<List<EmployeeViewModel>>();
                    // Define a lista como a fonte de dados da tabela
                    dgvFuncionarios.DataSource = funcionarios;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erro ao carregar funcionários: {response.StatusCode}\n{errorContent}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha na aplicação: {ex.Message}", "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-abilita o botão, independente do resultado
                btnCarregarFuncionarios.Enabled = true;
                btnCarregarFuncionarios.Text = "Carregar/Atualizar Lista";
            }
        }

        // --- Evento de clique no botão "Salvar" (Funcionários) ---
        // Este método trata tanto a CRIAÇÃO (POST) quanto a EDIÇÃO (PUT)
        private async void btnSalvar_Click(object sender, EventArgs e)
        {
            btnSalvar.Enabled = false;
            btnLimpar.Enabled = false;

            try
            {
                var client = ApiClient.GetClient();
                HttpResponseMessage response;

                // ---------------------------------------------
                // --- MODO: CRIAÇÃO (selectedEmployeeId é null)
                // ---------------------------------------------
                if (selectedEmployeeId == null)
                {
                    // Validação simples (agora lendo do txtSenhaFunc)
                    if (string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtSenhaFunc.Text))
                    {
                        throw new Exception("E-mail e Senha são obrigatórios para criar um novo usuário.");
                    }

                    // Monta o objeto de criação
                    var createData = new CreateEmployeeRequest
                    {
                        Name = txtNome.Text,
                        Email = txtEmail.Text,
                        Role = txtCargo.Text,
                        Password = txtSenhaFunc.Text,        // CORRIGIDO (estava label4)
                        ConfirmPassword = txtSenhaFunc.Text, // CORRIGIDO (estava label4)
                        IsAdmin = chkIsAdmin.Checked
                    };

                    // Envia o POST para o endpoint de Registro
                    response = await client.PostAsJsonAsync($"{ApiBaseUrl}/api/Registration/employee", createData);
                }
                // ---------------------------------------------
                // --- MODO: EDIÇÃO (selectedEmployeeId tem um ID)
                // ---------------------------------------------
                else
                {
                    // Monta o objeto de edição
                    var updateData = new UpdateEmployeeRequest
                    {
                        Name = txtNome.Text,
                        Role = txtCargo.Text,
                        IsAdmin = chkIsAdmin.Checked
                    };

                    // Lógica da Senha: Só envia a nova senha se o usuário digitou algo
                    if (!string.IsNullOrWhiteSpace(txtSenhaFunc.Text)) // CORRIGIDO (estava label4)
                    {
                        if (txtSenhaFunc.Text.Length < 6) // CORRIGIDO (estava label4)
                        {
                            throw new Exception("A nova senha deve ter no mínimo 6 caracteres.");
                        }
                        updateData.NewPassword = txtSenhaFunc.Text; // CORRIGIDO (estava label4)
                    }

                    // Envia o PUT para o endpoint de Edição
                    string url = $"{ApiBaseUrl}/api/Employee/{selectedEmployeeId.Value}";
                    response = await client.PutAsJsonAsync(url, updateData);
                }

                // ---------------------------------------------
                // --- Tratamento da Resposta (Comum aos dois)
                // ---------------------------------------------
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Funcionário salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm(); // Limpa o formulário

                    // Dispara o evento de clique do botão "Carregar" para atualizar a tabela
                    btnCarregarFuncionarios_Click(null, null);
                }
                else
                {
                    // Tenta ler a mensagem de erro da API
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    string erroMsg = errorResponse?.Message ?? $"Erro desconhecido (Status: {response.StatusCode})";

                    // Se for um erro de validação (com múltiplos erros)
                    if (errorResponse?.Errors != null && errorResponse.Errors.Any())
                    {
                        erroMsg = string.Join("\n", errorResponse.Errors);
                    }
                    throw new Exception(erroMsg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha ao salvar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSalvar.Enabled = true;
                btnLimpar.Enabled = true;
            }
        }

        // --- Evento de clique no botão "Limpar" (Funcionários) ---
        private void btnLimpar_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        // Método auxiliar para limpar o formulário de funcionário
        private void ClearForm()
        {
            txtNome.Text = "";
            txtEmail.Text = "";
            txtCargo.Text = "";
            txtSenhaFunc.Text = ""; // CORRIGIDO (estava label4)
            chkIsAdmin.Checked = false;

            // Reseta o estado para "Criação"
            selectedEmployeeId = null;
            txtEmail.ReadOnly = false; // Permite editar o e-mail
            gbFormFuncionario.Text = "Cadastrar Novo Funcionário";
        }

        // --- Evento de clique na CÉLULA da tabela (Funcionários) ---
        private void dgvFuncionarios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verifica se o clique foi em uma linha válida (e.RowIndex >= 0)
            if (e.RowIndex >= 0)
            {
                var row = dgvFuncionarios.Rows[e.RowIndex];
                // Pega o objeto 'EmployeeViewModel' associado à linha
                var employee = row.DataBoundItem as EmployeeViewModel;

                if (employee != null)
                {
                    // Preenche o formulário com os dados
                    txtNome.Text = employee.Name;
                    txtEmail.Text = employee.Email;
                    txtCargo.Text = employee.Role;
                    chkIsAdmin.Checked = employee.IsAdmin;
                    txtSenhaFunc.Text = ""; // CORRIGIDO (estava label4)

                    // Entra no modo "Edição"
                    selectedEmployeeId = employee.Id;
                    txtEmail.ReadOnly = true; // Proíbe editar o e-mail (que é o login)
                    gbFormFuncionario.Text = $"Editando: {employee.Name}";
                }
            }
        }

        // ====================================================================
        // --- ABA: CHAMADOS
        // ====================================================================

        // --- Evento de clique do botão "Carregar Todos os Chamados" ---
        private async void btnCarregarChamados_Click(object sender, EventArgs e)
        {
            btnCarregarChamados.Enabled = false;
            btnCarregarChamados.Text = "Carregando...";
            dgvChamados.DataSource = null;

            try
            {
                var client = ApiClient.GetClient();

                // Pega os valores selecionados dos filtros (ComboBox)
                // Usamos 'dynamic' para acessar a propriedade 'Value' do objeto anônimo
                string status = (cmbFiltroStatus.SelectedItem as dynamic)?.Value ?? "";
                string prioridade = (cmbFiltroPrioridade.SelectedItem as dynamic)?.Value ?? "";

                // Monta a URL correta (api/Chamado/todos-admin) com os filtros
                string url = $"{ApiBaseUrl}/api/Chamado/todos-admin?status={Uri.EscapeDataString(status)}&prioridade={Uri.EscapeDataString(prioridade)}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // Converte o JSON em uma lista de ChamadoViewModel
                    var chamados = await response.Content.ReadFromJsonAsync<List<ChamadoViewModel>>();
                    dgvChamados.DataSource = chamados;

                    // Renomeia os cabeçalhos das colunas para nomes mais amigáveis
                    if (dgvChamados.Columns["NumeroChamado"] != null)
                        dgvChamados.Columns["NumeroChamado"].HeaderText = "Nº Chamado";
                    if (dgvChamados.Columns["DataAbertura"] != null)
                        dgvChamados.Columns["DataAbertura"].HeaderText = "Abertura";
                    if (dgvChamados.Columns["NomeCliente"] != null)
                        dgvChamados.Columns["NomeCliente"].HeaderText = "Cliente";
                    if (dgvChamados.Columns["NomeAnalista"] != null)
                        dgvChamados.Columns["NomeAnalista"].HeaderText = "Analista";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erro ao carregar chamados: {response.StatusCode}\n{errorContent}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha na aplicação: {ex.Message}", "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCarregarChamados.Enabled = true;
                btnCarregarChamados.Text = "Carregar Todos os Chamados";
            }
        }
    }

    // ====================================================================
    // --- CLASSES AUXILIARES (Helpers) ---
    // ====================================================================

    // --- Cliente de API ---
    // Classe estática para criar um HttpClient que já envia nosso Token JWT
    public static class ApiClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static HttpClient GetClient()
        {
            httpClient.DefaultRequestHeaders.Clear();

            // Pega o Token que foi salvo no AuthStorage (na tela de login)
            if (!string.IsNullOrEmpty(AuthStorage.Token))
            {
                // Adiciona o cabeçalho de autorização "Bearer [token]"
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", AuthStorage.Token);
            }

            return httpClient;
        }
    }

    // --- ViewModel para a Tabela de Funcionários ---
    public class EmployeeViewModel
    {
        // [JsonPropertyName] garante que o C# consiga ler o JSON (que usa camelCase)
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public int Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("email")]
        public string? Email { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("role")]
        public string? Role { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; }
    }

    // --- Classe para CRIAR um funcionário (Enviar para a API) ---
    public class CreateEmployeeRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("email")]
        public string? Email { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("role")]
        public string? Role { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("password")]
        public string? Password { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("confirmPassword")]
        public string? ConfirmPassword { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; }
    }

    // --- Classe para EDITAR um funcionário (Enviar para a API) ---
    public class UpdateEmployeeRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("role")]
        public string? Role { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; }

        // A API de edição aceita uma senha nova (opcional)
        [System.Text.Json.Serialization.JsonPropertyName("newPassword")]
        public string? NewPassword { get; set; }
    }

    // --- ViewModel para a Tabela de Chamados ---
    public class ChamadoViewModel
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public int Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("numeroChamado")]
        public string? NumeroChamado { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("titulo")]
        public string? Titulo { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("prioridade")]
        public string? Prioridade { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("dataAbertura")]
        public DateTime DataAbertura { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("nomeCliente")]
        public string? NomeCliente { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("nomeAnalista")]
        public string? NomeAnalista { get; set; }
    }

    // --- Classe genérica para ler ERROS da API ---
    public class ApiErrorResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string? Message { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }
    }
}