using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace NextLayer.AdminDesktop
{
    public partial class frmLogin : Form
    {
        private const string ApiBaseUrl = "https://localhost:7121";
        private static readonly HttpClient httpClient = new HttpClient();

        public frmLogin()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            btnLogin.Enabled = false;
            btnLogin.Text = "Entrando...";

            try
            {
                var loginData = new
                {
                    email = txtEmail.Text,
                    password = txtSenha.Text
                };

                var response = await httpClient.PostAsJsonAsync($"{ApiBaseUrl}/api/Auth/login", loginData);

                if (!response.IsSuccessStatusCode)
                {
                    string erroMsg = "E-mail ou senha inválidos.";
                    if (response.Content != null)
                    {
                        var errorResponse = await response.Content.ReadFromJsonAsync<LoginErrorResponse>();
                        if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                        {
                            erroMsg = errorResponse.Message;
                        }
                    }
                    throw new Exception(erroMsg);
                }

                var loginResult = await response.Content.ReadFromJsonAsync<LoginSuccessResponse>();

                if (loginResult == null || !loginResult.IsAdmin)
                {
                    throw new Exception("Acesso negado. Esta aplicação é restrita a administradores.");
                }

                AuthStorage.Token = loginResult.Token;
                AuthStorage.UserName = loginResult.UserName;

                // Linha temporária de sucesso
                MessageBox.Show("Login de Admin bem-sucedido!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                frmDashboard dashboard = new frmDashboard();
                dashboard.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Falha: {ex.Message}";
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Entrar";
            }
        }
    }

    // --- CLASSES HELPER ATUALIZADAS (com '?' para warnings) ---

    public class LoginSuccessResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("token")]
        public string? Token { get; set; } // Adicionado '?'

        [System.Text.Json.Serialization.JsonPropertyName("userName")]
        public string? UserName { get; set; } // Adicionado '?'

        [System.Text.Json.Serialization.JsonPropertyName("userType")]
        public string? UserType { get; set; } // Adicionado '?'

        [System.Text.Json.Serialization.JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; }
    }

    public class LoginErrorResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string? Message { get; set; } // Adicionado '?'
    }

    public static class AuthStorage
    {
        public static string? Token { get; set; } // Adicionado '?'
        public static string? UserName { get; set; } // Adicionado '?'
    }
}