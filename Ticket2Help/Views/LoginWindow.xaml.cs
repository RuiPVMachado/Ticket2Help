using System.Windows;
using System.Windows.Input;
using BLL.Interfaces;
using BLL.Services;
using DAL.Interfaces;
using DAL.Repositories;
using MODELS;

namespace Ticket2Help.Views
{
    /// <summary>
    /// Janela de login do sistema Ticket2Help
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly IUtilizadorService _utilizadorService;

        public LoginWindow()
        {
            InitializeComponent();

            // Injeção de dependências manual (pode ser melhorada com IoC container)
            IUtilizadorRepository utilizadorRepo = new UtilizadorRepository();
            _utilizadorService = new UtilizadorService(utilizadorRepo);

            // Permitir login com Enter
            txtPassword.KeyDown += (s, e) => {
                if (e.Key == Key.Enter) BtnLogin_Click(null, null);
            };
            txtUsername.KeyDown += (s, e) => {
                if (e.Key == Key.Enter) txtPassword.Focus();
            };
        }

        /// <summary>
        /// Manipula o evento de clique do botão de login
        /// </summary>
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Password;

                // Validações básicas
                if (string.IsNullOrEmpty(username))
                {
                    ShowError("Por favor, introduza o nome de utilizador.");
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    ShowError("Por favor, introduza a palavra-passe.");
                    txtPassword.Focus();
                    return;
                }

                // Desabilitar botão durante validação
                btnLogin.IsEnabled = false;
                btnLogin.Content = "A validar...";

                // Validar credenciais
                Utilizador utilizador = _utilizadorService.ValidarLogin(username, password);

                if (utilizador != null)
                {
                    // Login bem-sucedido
                    var mainWindow = new MainWindow(utilizador);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ShowError("Credenciais inválidas. Verifique o nome de utilizador e palavra-passe.");
                    txtPassword.Clear();
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erro ao realizar login: {ex.Message}");
            }
            finally
            {
                // Reabilitar botão
                btnLogin.IsEnabled = true;
                btnLogin.Content = "Entrar";
            }
        }

        /// <summary>
        /// Exibe uma mensagem de erro na interface
        /// </summary>
        /// <param name="message">Mensagem de erro a exibir</param>
        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Esconde a mensagem de erro
        /// </summary>
        private void HideError()
        {
            lblError.Visibility = Visibility.Collapsed;
        }

        // Esconder erro quando o utilizador começa a digitar
        private void txtUsername_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            HideError();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HideError();
        }
    }
}